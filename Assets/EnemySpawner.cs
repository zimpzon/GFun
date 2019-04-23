using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public static EnemySpawner Instance;

    private void Awake()
    {
        Instance = this;
    }

    List<(int, int)> GetOpenPositions(Vector3 playerPos, float minDistance)
    {
        var result = new List<(int, int)>();
        for (int y = 0; y < MapBuilder.MapMaxHeight; ++y)
        {
            for (int x = 0; x < MapBuilder.MapMaxWidth; ++x)
            {
                if (MapBuilder.CollisionMap[x, y] == MapBuilder.TileWalkable)
                {
                    float distanceToplayer = (playerPos - new Vector3(x, y, 0)).magnitude;
                    if (distanceToplayer > minDistance)
                        result.Add((x, y));
                }
            }
        }
        return result;
    }

    Vector3 GetRandomPosition(List<(int, int)> openPositions)
    {
        int posIdx = Random.Range(0, openPositions.Count);
        (int cellX, int cellY) = openPositions[posIdx];
        openPositions.RemoveAt(posIdx);
        return new Vector3(cellX + 0.5f, cellY + 0.5f);
    }

    public void AddEnemiesOfType(Transform parent, EnemyId id, int count, List<(int, int)> openPositions)
    {
        for (int i = 0; i < count; ++i)
        {
            var enemy = Enemies.Instance.CreateEnemy(id);
            enemy.transform.SetParent(parent);
            var randomPos = GetRandomPosition(openPositions);
            enemy.transform.position = randomPos;
        }
    }

    public void AddEnemiesForFloor(Transform parent, int floor, Vector3 playerPos, float playerMinDistance)
    {
        var openPositions = GetOpenPositions(playerPos, playerMinDistance);

        // if enemy is large just clear some space around it

        int batCount = 3 + floor * 2;
        int fireBatCount = 2 + floor * 2;
        int scytheCount = 1 + floor / 4;

        AddEnemiesOfType(parent, EnemyId.Bat, batCount, openPositions);
        AddEnemiesOfType(parent, EnemyId.FireBat, fireBatCount, openPositions);
        AddEnemiesOfType(parent, EnemyId.SeekerScythe, scytheCount, openPositions);
    }
}
