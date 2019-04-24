using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public static EnemySpawner Instance;

    private void Awake()
    {
        Instance = this;
    }

    List<(int, int)> GetOpen2x2Positions(Vector3 playerPos, float minDistance)
    {
        var result = new List<(int, int)>();
        for (int y = 0; y < MapBuilder.MapMaxHeight - 1; ++y)
        {
            for (int x = 0; x < MapBuilder.MapMaxWidth - 1; ++x)
            {
                if (MapBuilder.CollisionMap[x + 0, y + 0] == MapBuilder.TileWalkable &&
                    MapBuilder.CollisionMap[x + 1, y + 0] == MapBuilder.TileWalkable &&
                    MapBuilder.CollisionMap[x + 0, y + 1] == MapBuilder.TileWalkable &&
                    MapBuilder.CollisionMap[x + 1, y + 1] == MapBuilder.TileWalkable)
                {
                    float distanceToplayer = (playerPos - new Vector3(x, y, 0)).magnitude;
                    if (distanceToplayer > minDistance)
                    {
                        result.Add((x, y));
                    }
                }
            }
        }

        if (result.Count == 0)
            throw new System.InvalidOperationException($"No open positions for enemies with a min distance of {minDistance} from player");

        return result;
    }

    Vector3 GetRandomPositionAtBottomMidOf2x2(List<(int, int)> openPositions)
    {
        int posIdx = Random.Range(0, openPositions.Count);
        (int cellX, int cellY) = openPositions[posIdx];
        openPositions.RemoveAt(posIdx);
        return new Vector3(cellX + 1.0f, cellY);
    }

    public void AddEnemiesOfType(Transform parent, EnemyId id, int count, List<(int, int)> openPositions)
    {
        for (int i = 0; i < count; ++i)
        {
            var enemy = Enemies.Instance.CreateEnemy(id);
            enemy.transform.SetParent(parent);
            var randomPos = GetRandomPositionAtBottomMidOf2x2(openPositions);
            enemy.transform.position = randomPos;

            var vec = new Vector3(randomPos.x, randomPos.y, 0);
            Debug.DrawRay(vec, Vector3.up * 0.5f, Color.green, 10);
            Debug.DrawRay(vec, Vector3.right * 0.5f, Color.green, 10);
        }
    }

    public void AddEnemiesForFloor(Transform parent, int floor, Vector3 playerPos, float playerMinDistance)
    {
        var openPositions = GetOpen2x2Positions(playerPos, playerMinDistance);

        // if enemy is large just clear some space around it

        int batCount = 3 + floor * 2;
        int fireBatCount = 2 + floor * 2;
        int scytheCount = 1 + floor / 4;

        AddEnemiesOfType(parent, EnemyId.Bat, batCount, openPositions);
        AddEnemiesOfType(parent, EnemyId.FireBat, fireBatCount, openPositions);
        AddEnemiesOfType(parent, EnemyId.SeekerScythe, scytheCount, openPositions);
    }
}
