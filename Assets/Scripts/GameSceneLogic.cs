using UnityEngine;

public class GameSceneLogic : MonoBehaviour
{
    public Transform DynamicObjectRoot;

    void CreatePlayer()
    {
        string characterTag = CurrentRunData.Instance.StartingCharacterTag;
        var player = PlayableCharacters.Instance.InstantiateCharacter(characterTag, Vector3.zero);
        player.transform.SetParent(DynamicObjectRoot);
    }

    private void Awake()
    {
        CreatePlayer();
    }
}
