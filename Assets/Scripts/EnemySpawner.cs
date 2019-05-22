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

    public void AddEnemiesForWorld1(Transform parent, int difficulty, List<(Vector3, float)> forbiddenPositions)
    {
        var openPositions = GetOpen2x2Positions(forbiddenPositions);

        bool isFirstLevel = difficulty == 1;

        int batCount = 2 + difficulty / 2;
        int fireBatCount = difficulty - 1;
        int fleeingBatCount = 2;
        int scytheCount = isFirstLevel ? 0 : Random.Range(0, (difficulty / 2) + 2);
        int golemCount = (difficulty & 1) == 1 ? 0 : 1 + difficulty / 6;

        var defaultPos = openPositions[0];
        AddEnemiesOfType(parent, EnemyId.SeekerScythe, scytheCount, openPositions, defaultPos);
        AddEnemiesOfType(parent, EnemyId.Bat, batCount, openPositions, defaultPos);
        AddEnemiesOfType(parent, EnemyId.FireBat, fireBatCount, openPositions, defaultPos);
        AddEnemiesOfType(parent, EnemyId.FleeingBat, fleeingBatCount, openPositions, defaultPos);
        AddEnemiesOfType(parent, EnemyId.Golem, golemCount, openPositions, defaultPos);
    }

    public void AddEnemiesForWorld2(Transform parent, int difficulty, List<(Vector3, float)> forbiddenPositions)
    {
        var openPositions = GetOpen2x2Positions(forbiddenPositions);

        if (difficulty < 7)
            DebugLinesScript.Show("Unexpected difficulty for World2 (expected >= 7)", difficulty);

        // Minimum difficulty here is 7 (5 World1 + boss + 1)
        difficulty -= 6; // = 1 and up

        int dragonHatchlingCount = 2 + difficulty / 2;
        int fireBatCount = difficulty + 1;
        int scytheCount = Random.Range(0, (difficulty / 2) + 2);

        var defaultPos = openPositions[0];
        AddEnemiesOfType(parent, EnemyId.SeekerScythe, scytheCount, openPositions, defaultPos);
        AddEnemiesOfType(parent, EnemyId.FireBat, fireBatCount, openPositions, defaultPos);
        AddEnemiesOfType(parent, EnemyId.DragonHatchling, dragonHatchlingCount, openPositions, defaultPos);
    }
}
