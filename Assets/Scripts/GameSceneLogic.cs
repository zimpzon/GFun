using MEC;
using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
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
    public MapPlugins MapPlugins;
    public TextMeshProUGUI LoadingText;
    public TextMeshProUGUI KilledByText;
    public TextMeshProUGUI StatsText;
    public TextMeshProUGUI HistoryText;

    int enemyAliveCount_;
    int enemyDeadCount_;
    Vector3 latestEnemyDeathPosition_;

    PortalScript nextLevelPortal_;
    BlackHolePluginScript blackHolePlugin_;
    MapScript map_;
    PlayableCharacterScript playerScript_;

    void CreatePlayer(Vector3 position)
    {
        string characterTag = CurrentRunData.Instance.StartingCharacterTag;
        var player = PlayableCharacters.Instance.InstantiateCharacter(characterTag, position);
        player.transform.SetParent(DynamicObjectRoot);
        playerScript_ = player.GetComponent<PlayableCharacterScript>();

        if (!CurrentRunData.Instance.HasPlayerData)
        {
            UpdateCurrentRunWithPlayerData(CurrentRunData.Instance, playerScript_);
            CurrentRunData.Instance.HasPlayerData = true;
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

        EnemyDropScript.Instance.SpawnDrops(enemy, position);
    }

    void OnEnemySpawned(IEnemy enemy, Vector3 position)
    {
        enemyAliveCount_++;
    }

    void OnAutoPickUp(AutoPickUpType type, int value, Vector3 position)
    {
        if (type == AutoPickUpType.Coin)
        {
            CurrentRunData.Instance.AddCoins(value);
            UpdateCoinWidget();
        }
        else if (type == AutoPickUpType.Health)
        {
            playerScript_.AddHealth(value, "Health Pickup");
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

        blackHolePlugin_ = FindObjectOfType<BlackHolePluginScript>();
    }

    private void Start()
    {
        TerminalCommands.RegisterCommands();

        StartCoroutine(SceneGlobals.Instance.AudioManager.SetAudioProfile(AudioManager.eScene.InGame));
        Timing.RunCoroutine(EnterLevelLoop().CancelWith(this.gameObject));
    }

    void UpdateInitCanvas()
    {
        if (CurrentRunData.Instance.NextMapType == MapType.Floor)
        {
            LoadingText.text = $"Floor {CurrentRunData.Instance.CurrentFloor}";
        }
        else if (CurrentRunData.Instance.NextMapType == MapType.Shop)
        {
            LoadingText.text = $"The Shop";
        }
        else
        {
            LoadingText.text = $"Missing text: {CurrentRunData.Instance.NextMapType}";
        }
    }

    IEnumerator<float> EnterLevelLoop()
    {
        System.GC.Collect(0, System.GCCollectionMode.Forced, blocking: true, compacting: true);

        if (CurrentRunData.Instance.RunEnded)
            CurrentRunData.StartNewRun();

        if (CurrentRunData.Instance.NextMapType == MapType.Floor)
            CurrentRunData.Instance.CurrentFloor++;

        UpdateInitCanvas();
        InitCanvas.gameObject.SetActive(true);
        yield return 0;

        // Prepare map
        float startTime = Time.unscaledTime;
        if (CurrentRunData.Instance.NextMapType == MapType.Shop)
            GenerateShop();
        else
            GenerateMap();

        // Prepare player
        CreatePlayer(map_.GetPlayerStartPosition(CurrentRunData.Instance.NextMapType));
        PlayableCharacters.Instance.SetCharacterToHumanControlled(playerScript_.tag);
        Helpers.SetCameraPositionToActivePlayer();
        var playerInScene = PlayableCharacters.GetPlayerInScene();
        var playerPos = playerInScene.transform.position;

        // Prepare enemies
        Time.timeScale = 0.01f;
        if (CurrentRunData.Instance.NextMapType == MapType.Floor)
        {
            List<(Vector3, float)> forbiddenPositions = new List<(Vector3, float)>();
            forbiddenPositions.Add((playerPos, 10));
            if (blackHolePlugin_.isActiveAndEnabled)
                forbiddenPositions.Add((blackHolePlugin_.transform.position, 4));

            EnemySpawner.Instance.AddEnemiesForFloor(DynamicObjectRoot, CurrentRunData.Instance.CurrentFloor, forbiddenPositions);
        }

        var enemiesAtMapStart = FindObjectsOfType<EnemyScript>();
        foreach (var enemy in enemiesAtMapStart)
            GameEvents.RaiseEnemySpawned(enemy, enemy.transform.position);

        const float MinimumShowTime = 1.5f;
        float endTime = Time.unscaledTime + (startTime - Time.unscaledTime) + MinimumShowTime;
        while (Time.unscaledTime < endTime)
            yield return 0;

        // Show what we created
        UpdateCoinWidget();
        InitCanvas.gameObject.SetActive(false);

        var targetPos = playerPos;
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
        map_.TriggerExplosion(targetPos, 2);

        var playerCenter = targetPos + Vector3.up * 0.5f;
        ParticleScript.EmitAtPosition(ParticleScript.Instance.PlayerLandParticles, playerCenter, 15);
        ParticleScript.EmitAtPosition(ParticleScript.Instance.MuzzleFlashParticles, playerCenter, 1);
        ParticleScript.EmitAtPosition(ParticleScript.Instance.WallDestructionParticles, playerCenter, 5);

        float shakeEndTime = Time.unscaledTime + 0.3f;
        while (Time.unscaledTime < shakeEndTime)
        {
            CameraShake.Instance.SetMinimumShake(1.0f);
            yield return 0;
        }

        Time.timeScale = 1.0f;

        if (CurrentRunData.Instance.NextMapType == MapType.Shop)
            Timing.RunCoroutine(ShopLoop().CancelWith(gameObject));
        else
            Timing.RunCoroutine(GameLoop().CancelWith(gameObject));

        yield return 0;
    }

    IEnumerator<float> GameLoop()
    {
        CurrentRunData.Instance.NextMapType = MapType.Shop;

        while (true)
        {
            //if (Input.GetKeyDown(KeyCode.X))
            //    playerScript_.Die();

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

    IEnumerator<float> ShopLoop()
    {
        CurrentRunData.Instance.NextMapType = MapType.Floor;
        var shopScript = FindObjectOfType<ShopPluginScript>();
        nextLevelPortal_.gameObject.SetActive(true);
        nextLevelPortal_.transform.position = shopScript.PortalPosition.position;
        nextLevelPortal_.OnPlayerEnter.AddListener(OnPlayerEnterPortal);

        while (true)
        {
            if (playerScript_.IsDead)
            {
                StartCoroutine(DeadLoop());
                yield break;
            }

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
        map_.TriggerExplosion(pos, 3);
        var portalCenter = pos + Vector3.up * 1.5f;
        ParticleScript.EmitAtPosition(ParticleScript.Instance.PlayerLandParticles, portalCenter, 25);
        ParticleScript.EmitAtPosition(ParticleScript.Instance.MuzzleFlashParticles, portalCenter, 1);
        ParticleScript.EmitAtPosition(ParticleScript.Instance.WallDestructionParticles, portalCenter, 10);

        yield return 0;
    }

    void UpdateLocalStats()
    {
        GameProgressData.CurrentProgress.EnemiesKilled += CurrentRunData.Instance.EnemiesKilled;
        GameProgressData.CurrentProgress.NumberOfDeaths++;
        GameProgressData.SaveProgress();
    }

    bool playFabUpdateComplete;
    IEnumerator UpdatePlayFabStats()
    {
        playFabUpdateComplete = false;

        var statList = new List<(string name, int value)>
        {
            (PlayFabData.Stat_SecondsPlayed, (int)(CurrentRunData.Instance.RunEndTime - CurrentRunData.Instance.RunStartTime)),
            (PlayFabData.Stat_FloorReached, CurrentRunData.Instance.CurrentFloor),
            (PlayFabData.Stat_EnemiesKilled, CurrentRunData.Instance.EnemiesKilled),
            (PlayFabData.Stat_CoinsCollected, CurrentRunData.Instance.CoinsCollected),
            (PlayFabData.Stat_ItemsBought, CurrentRunData.Instance.ItemsBought),
            (PlayFabData.Stat_Deaths, 1),
        };

        yield return PlayFabFacade.Instance.UpdateStats(statList);
        playFabUpdateComplete = true;
    }

    IEnumerator<float> DeadLoop()
    {
        GameEvents.ClearListeners();
        SceneGlobals.Instance.AudioManager.PlaySfxClip(PlayerDeadSound, 1);
        DeadCanvas.gameObject.SetActive(true);
        CurrentRunData.EndRun();

        float time = CurrentRunData.Instance.RunEndTime - CurrentRunData.Instance.RunStartTime;
        var timeSpan = System.TimeSpan.FromSeconds(time);
        KilledByText.text = $"Killed By <color=#ff0000>{playerScript_.KilledBy}</color>";

        var sb = new StringBuilder(100);
        sb.AppendLine($"Floor Reached: <color=#00ff00>{CurrentRunData.Instance.CurrentFloor}</color>");
        sb.AppendLine($"Enemies Killed: <color=#00ff00>{CurrentRunData.Instance.EnemiesKilled}</color>");
        sb.AppendLine($"Coins Collected: <color=#00ff00>{CurrentRunData.Instance.CoinsCollected}</color>");
        sb.AppendLine($"Time: <color=#00ff00>{timeSpan.ToString("hh\\:mm\\:ss")}</color>");

        StatsText.text = sb.ToString();

        sb.Clear();
        for (int i = 0; i < CurrentRunData.Instance.HealthEvents.Count; ++i)
        {
            var evt = CurrentRunData.Instance.HealthEvents[i];
            var span = System.TimeSpan.FromSeconds(evt.Time - CurrentRunData.Instance.RunStartTime);
            if (evt.HealthChange < 0)
            {
                string append = "";
                int hpLeft = evt.HealthBefore + evt.HealthChange;
                if (hpLeft == 0)
                    append = "(Killed)";
                else if (hpLeft < 0)
                    append = $"(Killed, {-hpLeft} Overkill)";

                sb.AppendLine($"{span.ToString("hh\\:mm\\:ss")}: <color=#ff0000>{evt.HealthChange}</color> from {evt.ChangeSource} {append}");
            }
            else
            {
                string append = "";
                int overHeal = (evt.HealthBefore + evt.HealthChange) - evt.MaxHealth;
                if (overHeal > 0)
                    append = $"({overHeal} Overheal)";

                sb.AppendLine($"{span.ToString("hh\\:mm\\:ss")}: <color=#00ff00>+{evt.HealthChange}</color> from {evt.ChangeSource} {append}");
            }
        }
        HistoryText.text = sb.ToString();

        MiniMapCamera.Instance.Show(false);

        UpdateLocalStats();

        // Make external calls to PlayFab and block unless timed out
        playFabUpdateComplete = false;
        float updatePlayFabTimeout = Time.unscaledTime + 3.0f;
        StartCoroutine(UpdatePlayFabStats());
        while (!playFabUpdateComplete && Time.unscaledTime < updatePlayFabTimeout)
            yield return 0;

        if (Time.unscaledDeltaTime > updatePlayFabTimeout)
            DebugLinesScript.Show("Stats update timed out", Time.time);

        float timeScale = Time.timeScale;

        while (true)
        {
            timeScale = Mathf.Max(0.0001f, timeScale - Time.unscaledDeltaTime);
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

    public void GenerateShop()
    {
        MapBuilder.ZeroMap();
        var shop = Instantiate(MapPlugins.ShopPlugin);
        shop.GetComponent<MapPluginScript>().ApplyToMap(MapBuilder.Center);
        MapBuilder.BuildMapTiles(MapBuilder.MapSource, map_, MapStyle);
        BuildPathingGraph();
    }

    public void GenerateMap()
    {
        int w = 40;
        int h = 40;

        MapBuilder.GenerateMapFloor(w, h, MapFloorAlgorithm.RandomWalkers);

        var plugins = FindObjectsOfType<MapPluginScript>();
        foreach (var plugin in plugins)
        {
            MapBuilder.BuildCollisionMapFromMapSource();
            plugin.ApplyToMap(new Vector3Int((int)plugin.transform.position.x, (int)plugin.transform.position.y, 0));
        }

        MapBuilder.BuildMapTiles(MapBuilder.MapSource, map_, MapStyle);

        BuildPathingGraph();
    }

    void BuildPathingGraph()
    {
        var gridGraph = (GridGraph)AstarPath.active.data.AddGraph(typeof(GridGraph));
        gridGraph.SetDimensions(MapBuilder.MapMaxWidth, MapBuilder.MapMaxHeight, 1.0f);
        gridGraph.center = MapBuilder.WorldCenter;
        gridGraph.collision.use2D = true;
        gridGraph.collision.collisionCheck = false;
        gridGraph.neighbours = NumNeighbours.Four;
        gridGraph.cutCorners = false;
        gridGraph.rotation = new Vector3(-90, 270, 90);

        AstarPath.active.Scan();
        AstarPath.active.AddWorkItem(new AstarWorkItem(ctx =>
        {
            var graph = AstarPath.active.data.gridGraph;
            for (int y = 0; y < graph.depth; ++y)
            {
                for (int x = 0; x < graph.width; ++x)
                {
                    var node = graph.GetNode(x, y);
                    node.Walkable = MapBuilder.CollisionMap[x, y] == 0;
                }
            }
            graph.GetNodes(node => graph.CalculateConnections((GridNodeBase)node));
        }));
    }
}
