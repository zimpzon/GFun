using System.Linq;
using UnityEngine;

public class PlayableCharacterData
{
    public string Tag;
    public string DisplayName;
}

public class PlayableCharacters : MonoBehaviour
{
    public static PlayableCharacters Instance;
    public CharacterPrefabList CharacterPrefabList;

    PlayableCharacterScript playerInScene_;
    public static PlayableCharacterScript GetPlayerInScene() => Instance.playerInScene_;

    private void Awake()
    {
        Instance = this;
    }

    public GameObject InstantiateCharacter(string characterTag, Vector3 position)
    {
        var prefab = GetCharacterPrefab(characterTag);
        var instance = Instantiate(prefab, position, Quaternion.identity);
        return instance;
    }

    GameObject GetCharacterPrefab(string characterTag)
        => CharacterPrefabList.CharacterPrefabs.Where(p => p.tag == characterTag).Single();

    public bool SetCharacterToHumanControlled(string characterTag, bool showChangeEffect = false)
    {
        var toBeControlled = GameObject.FindWithTag(characterTag)?.GetComponent<PlayableCharacterScript>();
        SceneGlobals.NullCheck(toBeControlled, "toBeControlled");

        var alreadyControlled = playerInScene_;

        bool noChange = alreadyControlled?.tag == toBeControlled.tag;
        if (noChange)
            return false;

        alreadyControlled?.SetIsHumanControlled(false);
        toBeControlled.SetIsHumanControlled(true, showChangeEffect);
        playerInScene_ = toBeControlled;

        return true;
    }
}
