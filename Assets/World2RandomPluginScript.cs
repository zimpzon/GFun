using System.Collections.Generic;
using UnityEngine;

public class World2RandomPluginScript : MapPluginScript
{
    public override string Name => $"Frozen Maze - {CurrentRunData.Instance.FloorInWorld}";
    public AudioClip Music;

    Vector3 playerStartPos_;

    public override void ApplyToMap(Vector3Int position)
    {
        int w = 30;
        int h = 30;

        MapBuilder.GenerateRandomWalkersMapFloor(w, h, pathSize: 3, walkerCount: 8, steps: 40, minBeforeTurn: 5, maxBeforeTurn: 8);

        // We need collision map to select a player position so build it now
        MapBuilder.BuildCollisionMapFromMapSource();
        playerStartPos_ = MapUtil.GetRandomEdgePosition(out Vector3 unused);

        List<(Vector3, float)> forbiddenPositions = new List<(Vector3, float)>();
        forbiddenPositions.Add((playerStartPos_, 10));

        var dynamicObjects = GameObject.FindWithTag("DynamicObjects");
        int difficulty = CurrentRunData.Instance.StartingDifficulty + CurrentRunData.Instance.TotalFloor;
        List<EnemySpawnDefinition> enemySpawnDefinitions = new List<EnemySpawnDefinition>();

        if (difficulty < 6)
            DebugLinesScript.Show("Unexpected difficulty for World2 (expected >= 6)", difficulty);

        difficulty -= 5;

        int dragonHatchlingCount = 2 + difficulty / 2;
        int iceBatCount = difficulty + 1;
        int scytheCount = Random.Range(0, (difficulty / 2) + 3) + 1;

        enemySpawnDefinitions.Add( new EnemySpawnDefinition() { EnemyId = EnemyId.SeekerScythe, Count = scytheCount });
        enemySpawnDefinitions.Add( new EnemySpawnDefinition() { EnemyId = EnemyId.IceBat, Count = iceBatCount });
        enemySpawnDefinitions.Add( new EnemySpawnDefinition() { EnemyId = EnemyId.DragonHatchling, Count = dragonHatchlingCount });

        EnemySpawner.Instance.AddEnemiesForWorld(enemySpawnDefinitions, dynamicObjects.transform, forbiddenPositions);
    }

    public override Vector3 GetPlayerStartPosition() => playerStartPos_;

    public override IEnumerator<float> GameLoopCo()
    {
        AudioManager.Instance.PlayMusic(Music, 0.4f);

        while (true)
        {
            yield return 0;
        }
    }
}
