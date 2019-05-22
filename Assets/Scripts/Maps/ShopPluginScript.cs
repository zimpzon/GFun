﻿using System.Collections.Generic;
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

        bossPortal_ = GameSceneLogic.Instance.BossPortal;
        bossPortal_.gameObject.SetActive(true);
        bossPortal_.transform.position = BossPortalPosition.position;
    }

    public override IEnumerator<float> GameLoopCo()
    {
        AudioManager.Instance.PlayMusic(Music, 0.8f);

        while (true)
        {
            yield return 0;
        }
    }
}
