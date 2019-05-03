using GFun;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CampScript : MonoBehaviour
{
    public string EnterPortalSceneName;
    public Canvas IntroCanvas;
    public Canvas LoadingCanvas;
    public Canvas OptionsCanvas;
    public AudioClip IntroMusicClip;
    public LightingEffectSettings CampLightingSettings;
    public LightingEffectSettings GraveyardLightingSettings;
    public LightingEffectSettings MenuLightingSettings;
    public AudioSource CampfireSoundSource;
    public Transform[] CharacterDefaultPositions;

    LightingEffectSettings activeLightingSettings_;
    bool isInGraveyard_;
    CameraPositioner camPos_;
    CameraShake camShake_;
    LightingImageEffect lightingImageEffect_;
    IMapAccess mapAccess_;
    MapScript mapScript_;

    private void Awake()
    {
        GameProgressData.LoadProgress();
        GameProgressData.EnableSave = true;
    }

    void Start()
    {
        camPos_ = SceneGlobals.Instance.CameraPositioner;
        camShake_ = SceneGlobals.Instance.CameraShake;
        lightingImageEffect_ = SceneGlobals.Instance.LightingImageEffect;
        mapAccess_ = SceneGlobals.Instance.MapAccess;
        mapScript_ = SceneGlobals.Instance.MapScript;

        mapAccess_.BuildCollisionMapFromWallTilemap(mapScript_.FloorTileMap);
        SetLighting(MenuLightingSettings);

        CreateCharacters();
        ActivateLatestSelectedCharacter();

        StartCoroutine(InMenu());
    }

    void CreateCharacters()
    {
        for (int i = 0; i < PlayableCharacters.Instance.CharacterPrefabList.CharacterPrefabs.Length; ++i)
        {
            var characterPrefab = PlayableCharacters.Instance.CharacterPrefabList.CharacterPrefabs[i];
            var position = CharacterDefaultPositions[i].position;

            bool isUnlocked = GameProgressData.CharacterIsUnlocked(characterPrefab.tag);
            if (isUnlocked)
                CreateCharacter(characterPrefab.tag, position);
            else
                CreateGhost(characterPrefab.tag, position);
        }
    }

    void CreateCharacter(string characterTag, Vector3 position)
    {
        var character = PlayableCharacters.Instance.InstantiateCharacter(characterTag, position);
    }

    void CreateGhost(string characterTag, Vector3 position)
    {
        // TODO
    }

    void ActivateLatestSelectedCharacter()
    {
        string startCharacterTag = PlayerPrefs.GetString(PlayerPrefsNames.SelectedCharacterTag);

        // When developing we might experience the selected characer is no longer unlocked. Revert to default.
        bool isUnlocked = GameProgressData.CharacterIsUnlocked(startCharacterTag);
        if (!isUnlocked)
            startCharacterTag = PlayableCharacters.Instance.CharacterPrefabList.CharacterPrefabs[0].tag;

        PlayableCharacters.Instance.SetCharacterToHumanControlled(startCharacterTag);
        Helpers.SetCameraPositionToActivePlayer();
    }

    void SelectCharacter(string characterTag)
    {
        bool isUnLocked = GameProgressData.CharacterIsUnlocked(characterTag);
        if (!isUnLocked)
            return;

        PlayerPrefs.SetString(PlayerPrefsNames.SelectedCharacterTag, characterTag);
        PlayerPrefs.Save();

        PlayableCharacters.Instance.SetCharacterToHumanControlled(characterTag, showChangeEffect: true);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
            SelectCharacter(PlayableCharacters.Instance.CharacterPrefabList.CharacterPrefabs[0].tag);
        if (Input.GetKeyDown(KeyCode.Alpha2))
            SelectCharacter(PlayableCharacters.Instance.CharacterPrefabList.CharacterPrefabs[1].tag);
        if (Input.GetKeyDown(KeyCode.Alpha3))
            SelectCharacter(PlayableCharacters.Instance.CharacterPrefabList.CharacterPrefabs[2].tag);

        if (Input.GetKeyDown(KeyCode.F12))
            GameProgressData.RestartProgress();
    }

    public void OnPlayerEnterStartPortal()
    {
        StopAllCoroutines();
        StartCoroutine(PlayerEnteredPortal());
    }

    public void OnGraveyardEnter()
    {
        LightingFadeTo(GraveyardLightingSettings, transitionSpeed: 5);
        isInGraveyard_ = true;
    }

    public void OnGraveyardLeave()
    {
        LightingFadeTo(CampLightingSettings, transitionSpeed: 5);
        isInGraveyard_ = false;
    }

    void LightingFadeTo(LightingEffectSettings settings, float transitionSpeed = 30)
    {
        activeLightingSettings_ = settings;
        lightingImageEffect_.SetBaseColorTarget(activeLightingSettings_, transitionSpeed: transitionSpeed);
    }

    void SetLighting(LightingEffectSettings settings)
    {
        activeLightingSettings_ = settings;
        lightingImageEffect_.SetBaseColor(activeLightingSettings_);
    }

    IEnumerator InCamp()
    {
        Time.timeScale = 1.0f;

        CampfireSoundSource.enabled = true;
        LightingFadeTo(isInGraveyard_ ? GraveyardLightingSettings : CampLightingSettings, transitionSpeed: 20);
        IntroCanvas.enabled = false;
        SceneGlobals.Instance.AudioManager.StopMusic();

        while (true)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                StartCoroutine(InMenu());
                yield break;
            }
            else if(Input.GetKeyDown(KeyCode.M))
            {
                StartCoroutine(ShowOptions());
                yield break;
            }
            yield return null;
        }
    }

    IEnumerator ShowOptions()
    {
        HumanPlayerController.Disabled = true;

        CampfireSoundSource.enabled = false;
        Time.timeScale = 0.25f;

        OptionsCanvas.gameObject.SetActive(true);
        while (true)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                HumanPlayerController.Disabled = false;
                OptionsCanvas.gameObject.SetActive(false);
                StartCoroutine(InCamp());
                yield break;
            }
            yield return null;
        }
    }

    IEnumerator InMenu()
    {
        HumanPlayerController.Disabled = true;

        CampfireSoundSource.enabled = false;
        Time.timeScale = 0.25f;

        IntroCanvas.enabled = true;

        LightingFadeTo(MenuLightingSettings, transitionSpeed: 20);
        SceneGlobals.Instance.AudioManager.PlayMusic(IntroMusicClip);

        while (true)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                HumanPlayerController.Disabled = false;

                StartCoroutine(InCamp());
                yield break;
            }
            yield return null;
        }
    }

    void StartGame()
    {
        CurrentRunData.Clear();
        CurrentRunData.Instance.StartingCharacterTag = PlayableCharacters.GetPlayerInScene().tag;

        SceneManager.LoadScene(EnterPortalSceneName, LoadSceneMode.Single);
    }

    IEnumerator PlayerEnteredPortal()
    {
        LoadingCanvas.gameObject.SetActive(true);
        yield return new WaitForSeconds(0.1f);

        StartGame();
    }
}
