﻿using System.Collections.Generic;
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
        int skipCount = 0;
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
                            //Debug.DrawRay(new Vector3(x, y, 0), Vector3.up * 0.5f, Color.green, 100);
                            //Debug.DrawRay(new Vector3(x, y, 0), Vector3.right * 0.5f, Color.green, 100);
                            skipCount++;
                            acceptPoint = false;
                            break;
                        }
                    }

                    if (acceptPoint)
                    {
                        //Debug.DrawRay(new Vector3(x, y, 0), Vector3.up * 0.5f, Color.red, 100);
                        //Debug.DrawRay(new Vector3(x, y, 0), Vector3.right * 0.5f, Color.red, 100);
                        result.Add((x, y));
                    }
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

    Vector3 GetRandomPositionAtBottomMidOf2x2(List<(int, int)> openPositions, (int x, int y) defaultPos)
    {
        if (openPositions.Count == 0)
            return new Vector3(defaultPos.x + 1.0f, defaultPos.y); // Mid-bottom of 2x2

        int posIdx = Random.Range(0, openPositions.Count);
        (int cellX, int cellY) = openPositions[posIdx];
            openPositions.RemoveAt(posIdx);

        return new Vector3(cellX + 1.0f, cellY); // Mid-bottom of 2x2
    }

    public void AddEnemiesOfType(Transform parent, EnemyId id, int count, List<(int, int)> openPositions, (int x, int y) defaultPos)
    {
        for (int i = 0; i < count; ++i)
        {
            var enemy = Enemies.Instance.CreateEnemy(id);
            enemy.transform.SetParent(parent);
            var randomPos = GetRandomPositionAtBottomMidOf2x2(openPositions, defaultPos);
            enemy.transform.position = randomPos;
        }
    }

    public void AddEnemiesForWorld(List<EnemySpawnDefinition> enemiesToSpawn, Transform parent, List<(Vector3, float)> forbiddenPositions)
    {
        var openPositions = GetOpen2x2Positions(forbiddenPositions);

        foreach (EnemySpawnDefinition item in enemiesToSpawn)
        {
            var defaultPos = openPositions[0];
            AddEnemiesOfType(parent, item.EnemyId, item.Count, openPositions, defaultPos);
        }
    }
}
