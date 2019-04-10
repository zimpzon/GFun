using GFun;
using UnityEngine;

public static class Helpers
{
    public static void SetCameraPositionToActivePlayer()
    {
        var playerInScene = PlayableCharacters.GetPlayerInScene();
        if (playerInScene == null)
            return;

        SceneGlobals.Instance.CameraPositioner.SetPosition(playerInScene.transform.position);
    }

    public static void ActivateSelectedCharacter()
    {
        string startCharacterTag = PlayerPrefs.GetString(PlayerPrefsNames.SelectedCharacterTag);
        var characterData = PlayableCharacters.Instance.GetFromTagOrDefault(startCharacterTag);
        SceneGlobals.Instance.PlayableCharacters.SwitchToCharacter(characterData, showChangeEffect: false);
        SetCameraPositionToActivePlayer();
    }
}
