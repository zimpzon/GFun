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
    public static PlayableCharacters Instance;

    List<PlayableCharacterData> All = new List<PlayableCharacterData>();

    public static readonly PlayableCharacterData Character1 = new PlayableCharacterData { Tag = "Character1", DisplayName = "Phil Beans" };
    public static readonly PlayableCharacterData Character2 = new PlayableCharacterData { Tag = "Character2", DisplayName = "Character Two" };
    public static readonly PlayableCharacterData Character3 = new PlayableCharacterData { Tag = "Character3", DisplayName = "Character Three" };

    public static readonly PlayableCharacterData DefaultCharacter = Character1;

    PlayableCharacterData defaultCharacter_;
    PlayableCharacterData currentCharacter_;

    PlayableCharacterScript playerInScene_;

    public static PlayableCharacterData GetCurrentCharacter() => Instance.currentCharacter_;
    public static PlayableCharacterScript GetPlayerInScene() => Instance.playerInScene_;

    private void Awake()
    {
        Instance = this;

        All.Clear();
        All.Add(Character1);
        All.Add(Character2);
        All.Add(Character3);

        defaultCharacter_ = Character1;
    }

    public void SetNoActiveCharacter()
    {
        if (currentCharacter_ != null)
        {
            GameObject.FindWithTag(currentCharacter_.Tag).GetComponent<PlayableCharacterScript>().SetIsHumanControlled(false, showChangeEffect: false);
            currentCharacter_ = null;
        }
    }

    public PlayableCharacterData GetFromTagOrDefault(string tag)
    {
        var data = All.Where(d => d.Tag == tag).FirstOrDefault();
        if (data == null)
            data = defaultCharacter_;
        return data;
    }

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

            SetNoActiveCharacter();
        }

        characterScript.SetIsHumanControlled(true, showChangeEffect);
        playerInScene_ = characterScript;
        currentCharacter_ = newCharacter;
        return true;
    }
}
