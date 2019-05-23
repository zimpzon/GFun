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

    public MapStyle MapStyleShop;
    public MapStyle MapStyleDungeon1;
    public MapStyle MapStyleIce1;
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
    public AudioClip GameMusic;
    public AudioClip ShopMusic;

    [System.NonSerialized] public PortalScript NextLevelPortal;
    [System.NonSerialized] public PortalScript BossPortal;

    int enemyAliveCount_;
    int enemyDeadCount_;
    Vector3 latestEnemyDeathPosition_;

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
            // This is a new game
            SavePlayerDataInCurrentRun(playerScript_, CurrentRunData.Instance);
            CurrentRunData.Instance.HasPlayerData = true;
        }
        else
        {
            InitializePlayerWithCurrentRunData(playerScript_, CurrentRunData.Instance);
        }
    }

    void AdvanceCurrentRun(CurrentRunData run, PlayableCharacterScript player)
    {
        // Advance progress.
        if (run.SpecialLocation == SpecialLocation.BossWorld1)
        {
            // World1 complete, move to world 2 (but first a visit to the shop)
            run.FloorInWorld = 1;
            run.World = World.World2;
            run.SpecialLocation = SpecialLocation.Shop;
            run.ShopKeeperPokeCount = 0; // Start over poke count when changing world
        }
        else if (run.SpecialLocation == SpecialLocation.Shop)
        {
            run.SpecialLocation = SpecialLocation.None;
        }
        else if (run.SpecialLocation == SpecialLocation.None)
        {
            // Just complated a standard floor.
            run.FloorInWorld++;
            run.TotalFloor++;
            run.SpecialLocation = SpecialLocation.Shop;
        }
        else
        {
            run.SpecialLocation = SpecialLocation.None;
        }
    }

    void SavePlayerDataInCurrentRun(PlayableCharacterScript player, CurrentRunData run)
    {
        // Remember player stats since player object is destroyed when loading next level
        run.MaxLife = player.MaxLife;
        run.Life = player.Life;
    }

    void InitializePlayerWithCurrentRunData(PlayableCharacterScript player, CurrentRunData run)
    {
        player.MaxLife = run.MaxLife;
        player.Life = run.Life;
    }

    void OnEnemyKilled(IEnemy enemy, Vector3 position)
    {
        latestEnemyDeathPosition_ = position;
        CurrentRunData.Instance.EnemiesKilled++;
        enemyDeadCount_++;
        enemyAliveCount_--;
        if (enemyAliveCount_ <= 0)
            OnAllEnemiesDead();

        LootDropScript.Instance.SpawnDrops(enemy, position);

        UpdateXp(position, enemy.XP);
    }

    void UpdateXp(Vector3 position, int xp)
    {
        int currentXp = GameProgressData.CurrentProgress.PlayerXp + CurrentRunData.Instance.XpEarned;
        int levelBefore = XpCalc.GetLevelAtXp(currentXp);
        if (xp != 0)
        {
            var color = Color.magenta;
            FloatingTextSpawner.Instance.Spawn(position + Vector3.up * 0.5f, $"XP: {xp}", color, 0.2f, 2.0f, TMPro.FontStyles.Normal);
        }

        CurrentRunData.Instance.XpEarned += xp;
        currentXp += xp;

        int levelAfter = XpCalc.GetLevelAtXp(currentXp);
        if (levelAfter > levelBefore)
            Timing.RunCoroutine(LevelUpCo(levelAfter).CancelWith(this.gameObject));

        int xpInThisLevel = currentXp - XpCalc.GetTotalXpRequired(levelAfter);
        int xpRequiredInThisLevel = XpCalc.GetXpRequired(levelAfter);
        XpWidget.Instance.ShowXp(levelAfter, xpInThisLevel, xpRequiredInThisLevel, currentXp);
    }

    IEnumerator<float> LevelUpCo(int newLevel)
    {
        FloatingTextSpawner.Instance.Spawn(AiBlackboard.Instance.PlayerPosition + Vector3.up * 0.5f, "Level Up", Color.yellow);
        yield return 0;
    }
    // handle mini in a loop?
    // WHY is event not received?
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
        System.GC.Collect(0, System.GCCollectionMode.Forced, blocking: true, compacting: true);

        Instance = this;

        GameEvents.ClearListeners();
        GameEvents.OnEnemyKilled += OnEnemyKilled;
        GameEvents.OnEnemySpawned += OnEnemySpawned;
        GameEvents.OnPlayerDamaged += OnPlayerDamaged;
        GameEvents.OnAutoPickUp += OnAutoPickUp;

        map_ = SceneGlobals.Instance.MapScript;

        NextLevelPortal = GameObject.FindWithTag("PurplePortal").GetComponent<PortalScript>();
        NextLevelPortal.gameObject.SetActive(false);
        NextLevelPortal.OnPlayerEnter.AddListener(OnPlayerEnterNextLevelPortal);

        BossPortal = GameObject.FindWithTag("GreenPortal").GetComponent<PortalScript>();
        BossPortal.gameObject.SetActive(false);
        BossPortal.OnPlayerEnter.AddListener(OnPlayerEnterBossPortal);
    }

    private void Start()
    {
        TerminalCommands.RegisterCommands();

        StartCoroutine(SceneGlobals.Instance.AudioManager.SetAudioProfile(AudioManager.eScene.InGame));
        StartCoroutine(EnterLevelLoopCo());
    }

    (GameObject plugin, MapStyle mapStyle) GetCurrentMapPluginPrefab(CurrentRunData run)
    {
        if (run.SpecialLocation == SpecialLocation.None)
        {
            if (run.World == World.World1)
                return (MapPlugins.World1RandomPlugin, MapStyleDungeon1);

            return (MapPlugins.World2RandomPlugin, MapStyleIce1);
        }
        else if (run.SpecialLocation == SpecialLocation.Shop)
        {
            return (MapPlugins.ShopPlugin, MapStyleShop);
        }
        else if (run.SpecialLocation == SpecialLocation.BossWorld1)
        {
            return (MapPlugins.Boss1Plugin, MapStyleDungeon1);
        }

        DebugLinesScript.Show("Location error, loading shop as default", "Location = " + run.SpecialLocation);
        return (MapPlugins.ShopPlugin, MapStyleShop);
    }

    IEnumerator EnterLevelLoopCo()
    {
        if (CurrentRunData.Instance.RunEnded)
            CurrentRunData.StartNewRun();

        (var mapPluginPrefab, var mapStyle) = GetCurrentMapPluginPrefab(CurrentRunData.Instance);
        var mapPlugin = Instantiate(mapPluginPrefab);
        mapPlugin.SetActive(false);
        var mapPluginScript = mapPlugin.GetComponent<MapPluginScript>();
        LoadingText.text = mapPluginScript.Name;

        InitCanvas.gameObject.SetActive(true);
        yield return null;

        // Prepare map
        float startTime = Time.unscaledTime;
        GenerateMapFromPlugin(mapPlugin, mapStyle);

        // Prepare player
        var playerStartPos = mapPluginScript.GetPlayerStartPosition();
        CreatePlayer(playerStartPos);
        PlayableCharacters.Instance.SetCharacterToHumanControlled(playerScript_.tag);
        Helpers.SetCameraPositionToActivePlayer();
        var playerInScene = PlayableCharacters.GetPlayerInScene();
        var playerPos = playerInScene.transform.position;
        UpdateXp(Vector3.zero, 0);

        Time.timeScale = 0.01f;
        const float MinimumShowTime = 1.5f;
        float endTime = Time.unscaledTime + (startTime - Time.unscaledTime) + MinimumShowTime;
        while (Time.unscaledTime < endTime)
            yield return null;

        // Show what we created
        mapPlugin.SetActive(true);

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
            yield return null;
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
            yield return null;
        }

        GameEvents.OnPlayerKilled += GameEvents_OnPlayerKilled;
        Time.timeScale = 1.0f;

        Timing.RunCoroutine(GameLoopCo().CancelWith(this.gameObject));
        Timing.RunCoroutine(mapPluginScript.GameLoopCo().CancelWith(mapPluginScript.gameObject));
    }

    void KillAllEnemies()
    {
        var enemies = FindObjectsOfType<EnemyScript>();
        int count = enemies.Length;
        foreach (var enemy in enemies)
            enemy.TakeDamage(10000, Vector3.up);
    }

    IEnumerator<float> GameLoopCo()
    {
        while (true)
        {
            if (Input.GetKeyDown(KeyCode.K))
                KillAllEnemies();

            yield return 0;
        }
    }

    private void GameEvents_OnPlayerKilled(IEnemy enemy)
    {
        Timing.RunCoroutine(DeadLoop().CancelWith(this.gameObject));
    }

    void OnAllEnemiesDead()
    {
        GameEvents.RaiseAllEnemieKiled();
        if (NextLevelPortal.gameObject.activeInHierarchy)
            return;

        Timing.RunCoroutine(ActivatePortalLoop(latestEnemyDeathPosition_).CancelWith(this.gameObject));
    }

    public void OnPlayerEnterNextLevelPortal()
    {
        SavePlayerDataInCurrentRun(playerScript_, CurrentRunData.Instance);
        AdvanceCurrentRun(CurrentRunData.Instance, playerScript_);
        LoadNextLevel();
    }

    public void OnPlayerEnterBossPortal()
    {
        SavePlayerDataInCurrentRun(playerScript_, CurrentRunData.Instance);
        if (CurrentRunData.Instance.World == World.World1)
            CurrentRunData.Instance.SpecialLocation = SpecialLocation.BossWorld1;
        else
            CurrentRunData.Instance.SpecialLocation = SpecialLocation.BossWorld1; // TODO: More bosses?

        LoadNextLevel();
    }

    public IEnumerator<float> ActivatePortalLoop(Vector3 position)
    {
        NextLevelPortal.gameObject.SetActive(true);

        float t = 0;
        while (t < 1)
        {
            CameraShake.Instance.SetMinimumShake(0.2f + t);
            t += Time.unscaledDeltaTime;
            yield return 0;
        }

        var pos = position;
        NextLevelPortal.transform.position = pos;
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
        GameProgressData.CurrentProgress.PlayerXp += CurrentRunData.Instance.XpEarned;
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
            (PlayFabData.Stat_FloorReached, CurrentRunData.Instance.TotalFloor),
            (PlayFabData.Stat_EnemiesKilled, CurrentRunData.Instance.EnemiesKilled),
            (PlayFabData.Stat_CoinsCollected, CurrentRunData.Instance.CoinsCollected),
            (PlayFabData.Stat_ItemsBought, CurrentRunData.Instance.ItemsBought),
            (PlayFabData.Stat_Boss1Attempts, CurrentRunData.Instance.Boss1Attempts),
            (PlayFabData.Stat_Boss1Kills, CurrentRunData.Instance.Boss1Kills),
            (PlayFabData.Stat_Deaths, 1),
        };

        yield return PlayFabFacade.Instance.UpdateStats(statList);
        playFabUpdateComplete = true;
    }

    IEnumerator<float> DeadLoop()
    {
        AudioManager.Instance.StopMusic();
        GameEvents.ClearListeners();
        SceneGlobals.Instance.AudioManager.PlaySfxClip(PlayerDeadSound, 1);
        DeadCanvas.gameObject.SetActive(true);
        CurrentRunData.EndRun();

        float time = CurrentRunData.Instance.RunEndTime - CurrentRunData.Instance.RunStartTime;
        var timeSpan = System.TimeSpan.FromSeconds(time);
        KilledByText.text = $"Killed By <color=#ff0000>{playerScript_.KilledBy}</color>";

        var sb = new StringBuilder(100);
        sb.AppendLine($"Floor Reached: <color=#00ff00>{CurrentRunData.Instance.TotalFloor}</color>");
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

    public void GenerateMapFromPlugin(GameObject plugin, MapStyle mapStyle)
    {
        MapBuilder.ZeroMap();
        plugin.GetComponent<MapPluginScript>().ApplyToMap(MapBuilder.Center);
        MapBuilder.BuildMap(MapBuilder.MapSource, map_, mapStyle);
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
