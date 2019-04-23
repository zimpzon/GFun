using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyLookup
{
    public IEnemy Enemy;
    public GameObject Prefab;
}

public class Enemies : MonoBehaviour
{
    public static Enemies Instance;

    public EnemyPrefabList EnemyPrefabList;
    public List<EnemyLookup> EnemyLookup;

    public List<EnemyLookup> FindEnemies(int minLevel, int maxLevel)
    {
        return EnemyLookup.Where(l => l.Enemy.Level >= minLevel && l.Enemy.Level <= maxLevel).ToList();
    }

    public GameObject CreateEnemy(EnemyId EnemyId)
    {
        var lookup = EnemyLookup.Where(l => l.Enemy.Id == EnemyId).Single();
        return CreateEnemy(lookup.Prefab);
    }

    public GameObject CreateEnemy(GameObject prefab)
    {
        var instance = Instantiate(prefab);
        return instance;
    }

    private void Awake()
    {
        Instance = this;
        EnemyLookup = EnemyPrefabList.EnemyPrefabs.Select(prefab =>
            new EnemyLookup { Enemy = prefab.GetComponent<IEnemy>(), Prefab = prefab }).ToList();
    }
}
