using UnityEngine;

[CreateAssetMenu(fileName = "new WeaponPrefabList.asset", menuName = "GFun/WeaponPrefabList", order = 10)]
public class WeaponPrefabList : ScriptableObject
{
    public GameObject[] WeaponPrefabs;
}
