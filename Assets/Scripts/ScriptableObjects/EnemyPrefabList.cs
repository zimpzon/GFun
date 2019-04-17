using UnityEngine;

[CreateAssetMenu(fileName = "new EnemyPrefabList.asset", menuName = "GFun/EnemyPrefabList", order = 10)]
public class EnemyPrefabList : ScriptableObject
{
    public GameObject[] EnemyPrefabs;
}
