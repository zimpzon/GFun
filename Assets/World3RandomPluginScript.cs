using System.Collections.Generic;
using UnityEngine;

public class World3RandomPluginScript : MapPluginScript
{
    public override string Name => $"In the OutDoors - {CurrentRunData.Instance.FloorInWorld}";
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

        if (difficulty < 12)
            DebugLinesScript.Show("Unexpected difficulty for World3 (expected >= 12)", difficulty);

        difficulty -= 11;
        difficulty = 1;
        int minotaurCount = difficulty / 2;
        int elfCount = difficulty + 1;
        int ravenCount = Random.Range(0, (difficulty / 2) + 3) + 1;

        enemySpawnDefinitions.Add( new EnemySpawnDefinition() { EnemyId = EnemyId.Raven, Count = ravenCount });
        enemySpawnDefinitions.Add( new EnemySpawnDefinition() { EnemyId = EnemyId.Elf, Count = elfCount });
        enemySpawnDefinitions.Add( new EnemySpawnDefinition() { EnemyId = EnemyId.Minotaur, Count = minotaurCount });

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
