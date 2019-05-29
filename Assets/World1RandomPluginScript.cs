using System.Collections.Generic;
using UnityEngine;

public class World1RandomPluginScript : MapPluginScript
{
    public override string Name => $"Kingdom Of Earth - {CurrentRunData.Instance.FloorInWorld}";
    public AudioClip Music;

    Vector3 playerStartPos_;

    public override void ApplyToMap(Vector3Int position)
    {
        int w = 40;
        int h = 40;

        MapBuilder.GenerateMapFloor(w, h, MapFloorAlgorithm.RandomWalkers);

        // We need collision map to select a player position so build it now
        MapBuilder.BuildCollisionMapFromMapSource();
        playerStartPos_ = MapUtil.GetRandomEdgePosition(out Vector3 unused);

        List<(Vector3, float)> forbiddenPositions = new List<(Vector3, float)>();
        forbiddenPositions.Add((playerStartPos_, 10));

        var dynamicObjects = GameObject.FindWithTag("DynamicObjects");
        int difficulty = CurrentRunData.Instance.StartingDifficulty + CurrentRunData.Instance.TotalFloor;
        List<EnemySpawnDefinition> enemySpawnDefinitions = new List<EnemySpawnDefinition>();

        bool isFirstLevel = difficulty == 1;

        int batCount = 2 + difficulty / 2;
        int fireBatCount = difficulty - 1;
        int fleeingBatCount = 2;
        int scytheCount = isFirstLevel ? 0 : Random.Range(0, (difficulty / 2) + 2);
        int golemCount = (difficulty & 1) == 1 ? 0 : 1 + difficulty / 6;

        enemySpawnDefinitions.Add( new EnemySpawnDefinition() { EnemyId = EnemyId.SeekerScythe, Count = scytheCount });
        enemySpawnDefinitions.Add( new EnemySpawnDefinition() { EnemyId = EnemyId.Bat, Count = batCount });
        enemySpawnDefinitions.Add( new EnemySpawnDefinition() { EnemyId = EnemyId.FireBat, Count = fireBatCount });
        enemySpawnDefinitions.Add( new EnemySpawnDefinition() { EnemyId = EnemyId.FleeingBat, Count = fleeingBatCount });
        enemySpawnDefinitions.Add( new EnemySpawnDefinition() { EnemyId = EnemyId.Golem, Count = golemCount });

        EnemySpawner.Instance.AddEnemiesForWorld(enemySpawnDefinitions, dynamicObjects.transform, forbiddenPositions);
    }

    public override Vector3 GetPlayerStartPosition() => playerStartPos_;

    public override IEnumerator<float> GameLoopCo()
    {
        AudioManager.Instance.PlayMusic(Music, 0.2f);

        while (true)
        {
            yield return 0;
        }
    }
}
