using MEC;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSceneLogic : MonoBehaviour
{
    public static GameSceneLogic Instance;

    public MapStyle MapStyle;
    public Canvas DeadCanvas;
    public Canvas InitCanvas;
    public string CampSceneName = "TheCampScene";
    public string GameSceneName = "GameScene";
    public Transform DynamicObjectRoot;
    public AudioClip PlayerDeadSound;
    public GameObjectPool CoinPool;

    int enemyAliveCount_;
    int enemyDeadCount_;
    Vector3 latestEnemyDeathPosition_;

    PortalScript nextLevelPortal_;
    MapScript map_;
    PlayableCharacterScript playerScript_;

    void CreatePlayer(Vector3 position)
    {
        string characterTag = CurrentRunData.Instance.StartingCharacterTag;
        var player = PlayableCharacters.Instance.InstantiateCharacter(characterTag, position);
        player.transform.SetParent(DynamicObjectRoot);
        playerScript_ = player.GetComponent<PlayableCharacterScript>();

        if (!CurrentRunData.Instance.IsInitialized)
        {
            CurrentRunData.Instance.IsInitialized = true;
            UpdateCurrentRunWithPlayerData(CurrentRunData.Instance, playerScript_);
        }
        else
        {
            InitializePlayerWithCurrentRunData(playerScript_, CurrentRunData.Instance);
        }
    }

    void UpdateCurrentRunWithPlayerData(CurrentRunData run, PlayableCharacterScript player)
    {
        print("Updating current run with player data");
        run.MaxLife = player.MaxLife;
        run.Life = player.Life;
    }

    void InitializePlayerWithCurrentRunData(PlayableCharacterScript player, CurrentRunData run)
    {
        print("Initializing player from current run data");
        player.MaxLife = run.MaxLife;
        player.Life = run.Life;
    }

    void OnEnemyKilled(IEnemy enemy, Vector3 position)
    {
        latestEnemyDeathPosition_ = position;
        CurrentRunData.Instance.EnemiesKilled++;
        enemyAliveCount_--;
        enemyDeadCount_++;

        int count = Random.Range(0, 5 + enemy.Level);
        for (int i = 0; i < count; ++i)
        {
            var coin = CoinPool.GetFromPool();
            coin.transform.position = position;
            var coinScript = coin.GetComponent<AutoPickUpActorScript>();
            var randomDirection = Random.insideUnitCircle.normalized;
            float randomForce = (Random.value * 0.5f + 0.5f) * 3;
            coinScript.ObjectPool = CoinPool;
            coin.gameObject.SetActive(true);
            coinScript.Throw(randomDirection * randomForce);
        }
    }

    void OnEnemySpawned(IEnemy enemy, Vector3 position)
    {
        enemyAliveCount_++;
    }

    void OnAutoPickUp(AutoPickUpType type, int value, Vector3 position)
    {
        if (type == AutoPickUpType.Coin)
        {
            CurrentRunData.Instance.Coins += value;
            UpdateCoinWidget();
        }
        else if (type == AutoPickUpType.Health)
        {
            playerScript_.AddHealth(value);
        }
    }

    void UpdateHealthWidget() => HealthWidget.Instance.ShowLife(playerScript_.Life, playerScript_.MaxLife);
    void OnPlayerDamaged(IEnemy enemy) => UpdateHealthWidget();

    public void UpdateCoinWidget()
    {
        if (CoinWidgetScript.Instance != null)
            CoinWidgetScript.Instance.SetAmount(CurrentRunData.Instance.Coins);
    }

    private void Awake()
    {
        Instance = this;

        GameEvents.ClearListeners();
        GameEvents.OnEnemyKilled += OnEnemyKilled;
        GameEvents.OnEnemySpawned += OnEnemySpawned;
        GameEvents.OnPlayerDamaged += OnPlayerDamaged;
        GameEvents.OnAutoPickUp += OnAutoPickUp;

        map_ = SceneGlobals.Instance.MapScript;
        nextLevelPortal_ = FindObjectOfType<PortalScript>();
        nextLevelPortal_.gameObject.SetActive(false);
    }

    private void Start()
    {
        TerminalCommands.RegisterCommands();

        Timing.RunCoroutine(EnterLevelLoop());
    }

    IEnumerator<float> EnterLevelLoop()
    {
        InitCanvas.gameObject.SetActive(true);
        yield return 0;

        float startTime = Time.time;
        GenerateMap();

        var enemiesAtMapStart = FindObjectsOfType<EnemyScript>();
        foreach (var enemy in enemiesAtMapStart)
            GameEvents.RaiseEnemySpawned(enemy, enemy.transform.position);

        CreatePlayer(map_.GetPlayerStartPosition());
        UpdateCoinWidget();

        PlayableCharacters.Instance.SetCharacterToHumanControlled(playerScript_.tag);
        Helpers.SetCameraPositionToActivePlayer();

        const float MinimumShowTime = 1.5f;
        float timeLeft = (startTime - Time.time) + MinimumShowTime;
        yield return Timing.WaitForSeconds(timeLeft);

        InitCanvas.gameObject.SetActive(false);

        var playerInScene = PlayableCharacters.GetPlayerInScene();
        var targetPos = playerInScene.transform.position;
        var startOffset = new Vector3(0, 1, -12);
        float t = 1;
        SceneGlobals.Instance.AudioManager.PlaySfxClip(SceneGlobals.Instance.AudioManager.AudioClips.PlayerLand, 1, 0.1f);
        while (t >= 0)
        {
            CameraShake.Instance.SetMinimumShake(0.5f);

            var pos = targetPos + startOffset * t;
            playerInScene.transform.SetPositionAndRotation(pos, Quaternion.Euler(0, 0, t * 500));

            t -= Time.unscaledDeltaTime * 2;
            yield return 0;
        }

        playerInScene.transform.SetPositionAndRotation(targetPos, Quaternion.identity);
        map_.ExplodeWalls(targetPos, 2);

        var playerCenter = targetPos + Vector3.up * 0.5f;
        ParticleScript.EmitAtPosition(ParticleScript.Instance.PlayerLandParticles, playerCenter, 15);
        ParticleScript.EmitAtPosition(ParticleScript.Instance.MuzzleFlashParticles, playerCenter, 1);
        ParticleScript.EmitAtPosition(ParticleScript.Instance.WallDestructionParticles, playerCenter, 5);

        float shakeEndTime = Time.time + 0.3f;
        while (Time.time < shakeEndTime)
        {
            CameraShake.Instance.SetMinimumShake(1.0f);
            yield return 0;
        }

        Timing.RunCoroutine(GameLoop().CancelWith(gameObject));
        yield return 0;
    }

    IEnumerator<float> GameLoop()
    {
        while (true)
        {
            if (Input.GetKeyDown(KeyCode.X))
            {
                playerScript_.Die();
            }

            if (playerScript_.IsDead)
            {
                StartCoroutine(DeadLoop());
                yield break;
            }

            if (enemyAliveCount_ <= 0 && !nextLevelPortal_.gameObject.activeInHierarchy)
                OnAllEnemiesDead();

            yield return 0;
        }
    }

    void OnAllEnemiesDead()
    {
        Timing.RunCoroutine(ActivatePortalLoop().CancelWith(gameObject));
    }

    public void OnPlayerEnterPortal()
    {
        UpdateCurrentRunWithPlayerData(CurrentRunData.Instance, playerScript_);

        LoadNextLevel();
    }

    IEnumerator<float> ActivatePortalLoop()
    {
        nextLevelPortal_.gameObject.SetActive(true);

        float t = 0;
        while (t < 1)
        {
            CameraShake.Instance.SetMinimumShake(0.2f + t);
            t += Time.unscaledDeltaTime;
            yield return 0;
        }

        var pos = latestEnemyDeathPosition_;
        nextLevelPortal_.transform.position = pos;
        nextLevelPortal_.OnPlayerEnter.AddListener(OnPlayerEnterPortal);
        map_.ExplodeWalls(pos, 3);
        var portalCenter = pos + Vector3.up * 1.5f;
        ParticleScript.EmitAtPosition(ParticleScript.Instance.PlayerLandParticles, portalCenter, 25);
        ParticleScript.EmitAtPosition(ParticleScript.Instance.MuzzleFlashParticles, portalCenter, 1);
        ParticleScript.EmitAtPosition(ParticleScript.Instance.WallDestructionParticles, portalCenter, 10);

        yield return 0;
    }

    void OnRunEnded()
    {
        GameProgressData.CurrentProgress.EnemiesKilled += CurrentRunData.Instance.EnemiesKilled;
        GameProgressData.CurrentProgress.NumberOfDeaths++;
        GameProgressData.SaveProgress();

        CurrentRunData.Reset();
    }

    IEnumerator<float> DeadLoop()
    {
        GameEvents.ClearListeners();
        SceneGlobals.Instance.AudioManager.PlaySfxClip(PlayerDeadSound, 1);
        DeadCanvas.gameObject.SetActive(true);
        MiniMapCamera.Instance.Show(false);

        OnRunEnded();

        float timeScale = Time.timeScale;

        while (true)
        {
            timeScale = Mathf.Max(0.001f, timeScale - Time.unscaledDeltaTime);
            Time.timeScale = timeScale;

            if (Input.GetKeyDown(KeyCode.R))
                LoadNextLevel();

            if (Input.GetKeyDown(KeyCode.Space))
                LoadTheCamp();

            yield return 0;
        }
    }

    void LoadTheCamp()
    {
        Time.timeScale = 1.0f;
        SceneManager.LoadScene(CampSceneName, LoadSceneMode.Single);
    }

    void LoadNextLevel()
    {
        Time.timeScale = 1.0f;
        SceneManager.LoadScene(GameSceneName, LoadSceneMode.Single);
    }

    public void GenerateMap()
    {
        int w = 100;
        int h = 50;

        MapBuilder.GenerateMapFloor(w, h, MapFloorAlgorithm.RandomWalkers);
        MapBuilder.Fillrect(new Vector2Int(90, 55), 20, 4, 1);

        var plugins = FindObjectsOfType<MapPluginScript>();
        foreach (var plugin in plugins)
            plugin.ApplyToMap();

        MapBuilder.BuildMapTiles(MapBuilder.MapSource, map_, MapStyle);
    }
}
