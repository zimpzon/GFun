using System.Collections.Generic;
using UnityEngine;

public class Dungeon1BossPluginScript : MapPluginScript
{
    public AudioClip Music;
    public override string Name => "The Golem King";
    public Transform PlayerStartPosition;
    public Transform PortalPosition;
    public Transform ChestPosition;

    PortalScript nextLevelPortal_;

    public override void ApplyToMap(Vector3Int position)
    {
        ApplyTilemap(position);
        nextLevelPortal_ = GameSceneLogic.Instance.NextLevelPortal;
        nextLevelPortal_.gameObject.SetActive(true);
        nextLevelPortal_.transform.position = PortalPosition.position;
    }

    public override Vector3 GetPlayerStartPosition()
        => PlayerStartPosition.position;

    public override IEnumerator<float> GameLoopCo()
    {
        AudioManager.Instance.PlayMusic(Music, 0.8f);

        while (true)
        {
            yield return 0;
        }
    }
}
