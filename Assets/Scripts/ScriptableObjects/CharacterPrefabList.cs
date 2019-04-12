using UnityEngine;

[CreateAssetMenu(fileName = "new CharacterPrefabList.asset", menuName = "GFun/CharacterPrefabList", order = 10)]
public class CharacterPrefabList : ScriptableObject
{
    public GameObject[] CharacterPrefabs;
}
