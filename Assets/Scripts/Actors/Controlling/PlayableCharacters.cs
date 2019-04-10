using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayableCharacterData
{
    public string Tag;
    public string DisplayName;
    public bool IsUnlocked;
}

public class PlayableCharacters : MonoBehaviour
{
    List<PlayableCharacterData> All = new List<PlayableCharacterData>();
    public static readonly PlayableCharacterData PhilBeans = new PlayableCharacterData { Tag = "PhilBeans", DisplayName = "Phil Beans" };
    public static readonly PlayableCharacterData PhilBeans2 = new PlayableCharacterData { Tag = "PhilBeans2", DisplayName = "Phil Beans2" };
    public static readonly PlayableCharacterData DefaultCharacter = PhilBeans;

    PlayableCharacterData defaultCharacter_;
    PlayableCharacterData currentCharacter_;

    public PlayableCharacterData GetCurrentCharacter() => currentCharacter_;

    public bool SwitchToCharacter(PlayableCharacterData newCharacter, bool showChangeEffect)
    {
        var characterScript = GameObject.FindWithTag(newCharacter.Tag)?.GetComponent<PlayableCharacterScript>();
        SceneGlobals.NullCheck(characterScript);
        if (characterScript == null)
            return false;

        if (currentCharacter_ != null)
        {
            bool noChange = currentCharacter_.Tag == newCharacter.Tag;
            if (noChange)
                return false;

            GameObject.FindWithTag(currentCharacter_.Tag).GetComponent<PlayableCharacterScript>().SetIsHumanControlled(false, showChangeEffect);
            currentCharacter_ = null;
        }

        characterScript.SetIsHumanControlled(true, showChangeEffect);
        currentCharacter_ = newCharacter;
        return true;
    }

    private void Awake()
    {
        All.Clear();
        All.Add(PhilBeans);
        All.Add(PhilBeans2);

        defaultCharacter_ = PhilBeans;
    }
}
