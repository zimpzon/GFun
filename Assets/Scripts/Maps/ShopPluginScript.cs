using System.Collections.Generic;
using UnityEngine;

public class ShopPluginScript : MapPluginScript
{
    public AudioClip Music;
    public override string Name => "Reapers Hideout";
    public Transform PortalPosition;
    public Transform BossPortalPosition;

    PortalScript nextLevelPortal_;
    PortalScript bossPortal_;

    public override Vector3 GetPlayerStartPosition()
    {
        return MapUtil.GetLeftmostFreeCell();
    }

    public override void ApplyToMap(Vector3Int position)
    {
        ApplyTilemap(position);
        nextLevelPortal_ = GameSceneLogic.Instance.NextLevelPortal;
        nextLevelPortal_.gameObject.SetActive(true);
        nextLevelPortal_.transform.position = PortalPosition.position;

        bool showBossPortal = CurrentRunData.Instance.FloorInWorld > CurrentRunData.BossActivationFloor;
        if (CurrentRunData.Instance.World != World.World1)
            showBossPortal = false; // TODO: Remove when there is a boss for world two

        bossPortal_ = GameSceneLogic.Instance.BossPortal;
        bossPortal_.gameObject.SetActive(showBossPortal);
        bossPortal_.transform.position = BossPortalPosition.position;
    }

    public override IEnumerator<float> GameLoopCo()
    {
        AudioManager.Instance.PlayMusic(Music, 0.4f);

        while (true)
        {
            yield return 0;
        }
    }
}
