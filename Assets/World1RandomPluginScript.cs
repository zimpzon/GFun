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
        EnemySpawner.Instance.AddEnemiesForWorld1(dynamicObjects.transform, difficulty, forbiddenPositions);
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
