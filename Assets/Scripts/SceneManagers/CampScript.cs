using GFun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CampScript : MonoBehaviour
{
    public static CampScript Instance;
    static readonly TrackedPath GhostPath = new TrackedPath();

    public string EnterPortalSceneName;
    public Canvas IntroCanvas;
    public Canvas LoadingCanvas;
    public Canvas OptionsCanvas;
    public Canvas EnterNameCanvas;
    public Canvas LeaderboardCanvas;
    public Canvas CampCanvas;
    public TMP_InputField EnterNameInput;
    public TextMeshProUGUI PlayerNameText;
    public TextMeshProUGUI PlayerNameErrorText;
    public Button ButtonPlayerNameOk;
    public TextMeshPro ControlsText;
    public GameObject PlayerNameRoot;
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
    public GameObject QuestPortal;

    LightingEffectSettings activeLightingSettings_;
    bool isInGraveyard_;
    CameraPositioner camPos_;
    CameraShake camShake_;
    LightingImageEffect lightingImageEffect_;
    IMapAccess mapAccess_;
    MapScript mapScript_;
    GhostPlayerScript ghostScript_;
    QuestId activatedQuest_;

    private void Awake()
    {
        Instance = this;
        GameProgressData.LoadProgress();
        GameProgressData.EnableSave = true;
    }

    IEnumerator Start()
    {
        LoadingCanvas.gameObject.SetActive(true);
        ShowPlayerName(false);
        HumanPlayerController.CanShoot = false;
        ControlsText.gameObject.SetActive(GameProgressData.CurrentProgress.NumberOfDeaths == 0);

        while (!PlayFabFacade.Instance.LoginProcessComplete)
            yield return null;

        Weapons.LoadWeaponsFromResources();
        Enemies.LoadEnemiesFromResources();

        QuestPortal.SetActive(false);

        LoadingCanvas.gameObject.SetActive(false);
        IntroCanvas.gameObject.SetActive(true);

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

        lightingImageEffect_.Darkness = 0.0f;
        SceneGlobals.Instance.GraveStoneManager.CreateGravestones();

        mapAccess_.BuildCollisionMapFromWallTilemap(mapScript_.FloorTileMap);
        SetLighting(MenuLightingSettings);

        CreateCharacters();
        ActivateLatestSelectedCharacter();

        StartCoroutine(InMenu());
    }

    public void UpdatePlayFabStatsAsync()
    {
        StartCoroutine(UpdatePlayFabStats());
    }

    IEnumerator UpdatePlayFabStats()
    {
        var statList = new List<(string name, int value)> {
            (PlayFabData.Stat_QuestsCompleted, GameProgressData.CurrentProgress.QuestProgress.CollectedQuests.Count)
        };
        yield return PlayFabFacade.Instance.UpdateStats(statList);
    }

    public void Debug_QuestPortalNotImplemented()
    {
        FloatingTextSpawner.Instance.Spawn(QuestPortal.transform.position, "Not Implemented", Color.yellow);
    }

    public void ActivateQuest(QuestId id)
    {
        QuestGiverScript.Instance.Close();
        activatedQuest_ = id;

        var pos = QuestPortal.transform.position;
        mapAccess_.TriggerExplosion(QuestPortal.transform.position, 2.9f);
        var portalCenter = pos + Vector3.up * 1.5f;
        ParticleScript.EmitAtPosition(ParticleScript.Instance.PlayerLandParticles, portalCenter, 25);
        ParticleScript.EmitAtPosition(ParticleScript.Instance.MuzzleFlashParticles, portalCenter, 1);
        ParticleScript.EmitAtPosition(ParticleScript.Instance.WallDestructionParticles, portalCenter, 10);
        QuestPortal.SetActive(true);
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
        if (string.IsNullOrEmpty(startCharacterTag))
            startCharacterTag = "Character1";

        // When developing we might experience the selected character is no longer unlocked. Revert to default.
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

    int resetProgressCount;

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
            if (resetProgressCount++ > 5)
            {
                GameProgressData.RestartProgress();
                PlayerInfoScript.Instance.ShowInfo("Progress Reset", Color.red);
            }
        }
    }

    public void OnPlayerEnterStartPortal()
    {
        StopAllCoroutines();
        StartCoroutine(PlayerEnteredPortal());
    }

    public void OnPlayerEnterQuestPortal()
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
            nextFlash = time + 8.0f + Random.value * 4;
            nextThunder = time + Random.value + 0.5f;
        }

        if (time > nextThunder)
        {
            AudioManager.Instance.PlaySfxClip(DistantThunder, 2, 0.2f);
            nextThunder = float.MaxValue;
        }
    }

    public void ShowPlayerName(bool show)
        => PlayerNameRoot.SetActive(show);

    string playerNameBeforeEdit;

    public void ShowEnterName()
    {
        playerNameBeforeEdit = GameProgressData.CurrentProgress.PlayerName;
        EnterNameInput.text = GameProgressData.CurrentProgress.PlayerName;
        PlayerNameErrorText.gameObject.SetActive(false);
        ButtonPlayerNameOk.enabled = true;
        EnterNameCanvas.gameObject.SetActive(true);
        EnterNameInput.ActivateInputField();
    }

    void OnPlayerNameUpdated()
    {
        ButtonPlayerNameOk.enabled = true;

        if (!string.IsNullOrEmpty(PlayFabFacade.Instance.LastError))
        {
            PlayerNameErrorText.text = $"Error: {PlayFabFacade.Instance.LastError}";
            PlayerNameErrorText.gameObject.SetActive(true);
        }
        else
        {
            GameProgressData.CurrentProgress.PlayerName = EnterNameInput.text;
            GameProgressData.SaveProgress();
            PlayerNameText.text = GameProgressData.CurrentProgress.PlayerName;

            PlayerInfoScript.Instance.ShowInfo($"Welcome, {GameProgressData.CurrentProgress.PlayerName}", Color.green);
            EnterNameCanvas.gameObject.SetActive(false);
        }
    }

    public void CloseEnterName(bool saveName)
    {
        if (!saveName || EnterNameInput.text == playerNameBeforeEdit)
        {
            EnterNameCanvas.gameObject.SetActive(false);
            return;
        }

        if (string.IsNullOrWhiteSpace(EnterNameInput.text) || EnterNameInput.text.Length < 4)
        {
            PlayerNameErrorText.text = "Name Is Too Short (Minimum 4)";
            PlayerNameErrorText.gameObject.SetActive(true);
            return;
        }

        if (!string.IsNullOrWhiteSpace(EnterNameInput.text))
        {
            ButtonPlayerNameOk.enabled = false;
            PlayFabFacade.Instance.UpdatePlayerNameAsync(EnterNameInput.text, OnPlayerNameUpdated);
        }
    }

    IEnumerator InCamp()
    {
        Time.timeScale = 1.0f;

        CampCanvas.gameObject.SetActive(true);

        CampfireSoundSource.enabled = true;
        LightingFadeTo(isInGraveyard_ ? GraveyardLightingSettings : CampLightingSettings, transitionSpeed: 20);
        IntroCanvas.enabled = false;
        StartCoroutine(SceneGlobals.Instance.AudioManager.SetAudioProfile(AudioManager.eScene.InGame));

        ShowPlayerName(true);
        if (string.IsNullOrWhiteSpace(GameProgressData.CurrentProgress.PlayerName))
        {
            GameProgressData.CurrentProgress.PlayerName = $"Anonymous{Random.Range(100000, 999999)}";
            GameProgressData.SaveProgress();
            ShowEnterName();
        }
        else
        {
            PlayerNameText.text = GameProgressData.CurrentProgress.PlayerName;
            PlayerInfoScript.Instance.ShowInfo($"Welcome back, {GameProgressData.CurrentProgress.PlayerName}", Color.green);
        }

        if (!ghostScript_.IsStarted)
            ghostScript_.Wander(GhostPath, 2.0f + Random.value * 3);

        SceneGlobals.Instance.AudioManager.StopMusic();

        nextFlash = Time.time + 3 + Random.value * 3;
        while (true)
        {
            UpdateThunder(Time.time);

            // Quick hacky solution to prevent entering name from triggering other stuff. Skip the checks.
            if (EnterNameCanvas.gameObject.activeInHierarchy)
            {
                if (Input.GetKeyDown(KeyCode.Escape))
                    CloseEnterName(saveName: false);

                if (Input.GetKey(KeyCode.Return))
                    CloseEnterName(saveName: true);

                yield return null;
            }
            else
            {
                if (Input.GetKeyDown(KeyCode.N))
                {
                    ShowEnterName();
                }
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    StartCoroutine(InMenu());
                    yield break;
                }
                else if (Input.GetKeyDown(KeyCode.M))
                {
                    StartCoroutine(ShowOptions());
                    yield break;
                }
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

        // Init the run data
        var player = PlayableCharacters.GetPlayerInScene();
        CurrentRunData.StartNewRun();
        CurrentRunData.Instance.MaxLife = player.MaxLife;
        CurrentRunData.Instance.Life = player.Life;
        CurrentRunData.Instance.CurrentWeapon = player.CurrentWeapon.Id;
        CurrentRunData.Instance.BulletAmmo = 999;
        CurrentRunData.Instance.ShellAmmo = 999;
        CurrentRunData.Instance.StartingCharacterTag = player.tag;
        CurrentRunData.Instance.HasPlayerData = true;

        GameProgressData.CurrentProgress.QuestProgress.ApplyAllRewards();
        CurrentRunData.StoreState();

        HumanPlayerController.CanShoot = true;
        SceneManager.LoadScene(EnterPortalSceneName, LoadSceneMode.Single);
    }

    IEnumerator PlayerEnteredPortal()
    {
        LoadingCanvas.gameObject.SetActive(true);
        yield return new WaitForSeconds(0.1f);

        StartGame();
    }
}
