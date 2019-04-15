public static class Helpers
{
    public static void SetCameraPositionToActivePlayer()
    {
        var playerInScene = PlayableCharacters.GetPlayerInScene();
        if (playerInScene == null)
            return;

        SceneGlobals.Instance.CameraPositioner.SetPosition(playerInScene.transform.position);
    }
}
