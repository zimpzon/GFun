using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public static EnemySpawner Instance;

    private void Awake()
    {
        Instance = this;
    }

    List<(int, int)> GetOpen2x2Positions(List<(Vector3, float)> forbiddenPositions)
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
                    bool acceptPoint = true;
                    for (int i = 0; i < forbiddenPositions.Count; ++i)
                    {
                        var point = forbiddenPositions[i].Item1;
                        var minDist = forbiddenPositions[i].Item2;
                        float distance = (point - new Vector3(x, y, 0)).magnitude;
                        if (distance < minDist)
                        {
                            acceptPoint = false;
                            break;
                        }
                    }

                    if (acceptPoint)
                        result.Add((x, y));
                }
            }
        }

        if (result.Count == 0)
        {
            result.Add((0, 0));
            Debug.LogError("No open positions for enemies, defaulting to 0, 0");
        }

        return result;
    }

    Vector3 GetRandomPositionAtBottomMidOf2x2(List<(int, int)> openPositions)
    {
        int posIdx = Random.Range(0, openPositions.Count);
        (int cellX, int cellY) = openPositions[posIdx];
        if (openPositions.Count > 1)
            openPositions.RemoveAt(posIdx);
        else
            DebugLinesScript.Show("No more open positions for enemies", Time.time);

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
        }
    }

    public void AddEnemiesForFloor(Transform parent, int floor, List<(Vector3, float)> forbiddenPositions)
    {
        var openPositions = GetOpen2x2Positions(forbiddenPositions);

        int batCount = 1 + floor / 2;
        int fireBatCount = floor;
        int fleeingBatCount = 2;
        int scytheCount = Random.value < 0.75f ? 0 : 1 + floor / 3;
        int dragonCount = 1 + floor / 3;
        int golemCount = (floor & 1) == 1 ? 0 : 1 + floor / 10;

        AddEnemiesOfType(parent, EnemyId.SeekerScythe, scytheCount, openPositions);
        AddEnemiesOfType(parent, EnemyId.Bat, batCount, openPositions);
        AddEnemiesOfType(parent, EnemyId.FireBat, fireBatCount, openPositions);
        AddEnemiesOfType(parent, EnemyId.FleeingBat, fleeingBatCount, openPositions);
        AddEnemiesOfType(parent, EnemyId.DragonHatchling, dragonCount, openPositions);
        AddEnemiesOfType(parent, EnemyId.Golem, golemCount, openPositions);
    }
}
