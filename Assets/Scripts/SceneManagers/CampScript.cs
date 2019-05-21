using GFun;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CampScript : MonoBehaviour
{
    static readonly TrackedPath GhostPath = new TrackedPath();

    public string EnterPortalSceneName;
    public Canvas IntroCanvas;
    public Canvas LoadingCanvas;
    public Canvas OptionsCanvas;
    public AudioClip IntroMusicClip;
    public AudioClip DistantThunder;
    public LightingEffectSettings CampLightingSettings;
    public LightingEffectSettings GraveyardLightingSettings;
    public LightingEffectSettings MenuLightingSettings;
    public LightingEffectSettings FlashLightingSettings;
    public AnimationCurve FlashCurve;
    public float FlashTime;
    public AudioSource CampfireSoundSource;
    public Transform[] CharacterDefaultPositions;
    public GameObject GhostPlayerPrefab;
    public GameObject LockedCharacterPrefab;

    LightingEffectSettings activeLightingSettings_;
    bool isInGraveyard_;
    CameraPositioner camPos_;
    CameraShake camShake_;
    LightingImageEffect lightingImageEffect_;
    IMapAccess mapAccess_;
    MapScript mapScript_;
    GhostPlayerScript ghostScript_;

    private void Awake()
    {
        GameProgressData.LoadProgress();
        GameProgressData.EnableSave = true;
    }

    IEnumerator Start()
    {
        LoadingCanvas.gameObject.SetActive(true);

        while (!PlayFabFacade.Instance.LoginProcessComplete)
            yield return null;

        LoadingCanvas.gameObject.SetActive(false);

        string ghostPath = PlayerPrefs.GetString("LatestCampPath");
        try
        {
            if (string.IsNullOrWhiteSpace(ghostPath))
                PlayFabFacade.AllData.InfoResultPayload.TitleData.TryGetValue("DefaultCampGhost", out ghostPath);

            GhostPath.FromString(ghostPath);
            if (GhostPath.HasPath)
            {
                var ghost = Instantiate(GhostPlayerPrefab, Vector3.left * 10000, Quaternion.identity);
                ghostScript_ = ghost.GetComponent<GhostPlayerScript>();
            }
        }
        catch (System.Exception) { }

        camPos_ = SceneGlobals.Instance.CameraPositioner;
        camShake_ = SceneGlobals.Instance.CameraShake;
        lightingImageEffect_ = SceneGlobals.Instance.LightingImageEffect;
        mapAccess_ = SceneGlobals.Instance.MapAccess;
        mapScript_ = SceneGlobals.Instance.MapScript;

        SceneGlobals.Instance.GraveStoneManager.CreateGravestones();

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
        var lockedCharacter = Instantiate(LockedCharacterPrefab, position, Quaternion.identity);
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
        {
            GameProgressData.RestartProgress();
            PlayerInfoScript.Instance.ShowInfo("Progress Reset", Color.red);
        }
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

    float nextFlash;
    float nextThunder = float.MaxValue;

    void UpdateThunder(float time)
    {
        if (time > nextFlash)
        {
            lightingImageEffect_.FlashColor(FlashLightingSettings, FlashCurve, FlashTime);
            nextFlash = time + 10.0f + Random.value * 10;
            nextThunder = time + Random.value * 1.0f + 1.0f;
        }

        if (time > nextThunder)
        {
            AudioManager.Instance.PlaySfxClip(DistantThunder, 2, 0.2f);
            nextThunder = float.MaxValue;
        }
    }

    IEnumerator InCamp()
    {
        Time.timeScale = 1.0f;

        CampfireSoundSource.enabled = true;
        LightingFadeTo(isInGraveyard_ ? GraveyardLightingSettings : CampLightingSettings, transitionSpeed: 20);
        IntroCanvas.enabled = false;
        StartCoroutine(SceneGlobals.Instance.AudioManager.SetAudioProfile(AudioManager.eScene.InGame));

        if (!ghostScript_.IsStarted)
            ghostScript_.Wander(GhostPath, 2.0f + Random.value * 3);

        SceneGlobals.Instance.AudioManager.StopMusic();

        nextFlash = Time.time + 3 + Random.value * 3;
        while (true)
        {
            UpdateThunder(Time.time);

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
        Time.timeScale = 0.75f;

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

        CampfireSoundSource.enabled = true;
        Time.timeScale = 1.0f;
        IntroCanvas.enabled = true;

        LightingFadeTo(MenuLightingSettings, transitionSpeed: 20);
        StartCoroutine(SceneGlobals.Instance.AudioManager.SetAudioProfile(AudioManager.eScene.InGame));
        SceneGlobals.Instance.AudioManager.PlayMusic(IntroMusicClip);

        nextFlash = Time.time + 3 + Random.value * 3;
        while (true)
        {
            UpdateThunder(Time.time);

            if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonUp(0))
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
        var stringPath = HumanPlayerController.TrackedPath.AsString();
        PlayerPrefs.SetString("LatestCampPath", stringPath);
        PlayerPrefs.Save();

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
