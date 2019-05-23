using MEC;
using System.Collections.Generic;
using UnityEngine;

public class Dungeon1BossPluginScript : MapPluginScript
{
    public AudioClip Music;
    public override string Name => "The Golem King";
    public Transform PlayerStartPosition;
    public Transform PortalPosition;
    public Transform ChestPosition;

    MiniGolemController[] miniGolems_;
    GolemController[] golems_;
    GolemKingController golemKing_;

    PortalScript nextLevelPortal_;

    private void Start()
    {
        var dynamicObjects = GameObject.FindWithTag("DynamicObjects");

        golemKing_ = FindObjectOfType<GolemKingController>();
        GameEvents.RaiseEnemySpawned((IEnemy)golemKing_, Vector3.zero);

        miniGolems_ = FindObjectsOfType<MiniGolemController>();
        for (int i = 0; i < miniGolems_.Length; ++i)
            GameEvents.RaiseEnemySpawned((IEnemy)miniGolems_[i], Vector3.zero);

        golems_ = FindObjectsOfType<GolemController>();
        for (int i = 0; i < golems_.Length; ++i)
        {
            GameEvents.RaiseEnemySpawned((IEnemy)golems_[i], Vector3.zero);
            golems_[i].enabled = false;
        }

        GameEvents.OnGolemKingCallingForHelp += GameEvents_OnGolemKingCallingForHelp;
    }

    private void GameEvents_OnGolemKingCallingForHelp()
    {
        for (int i = 0; i < miniGolems_.Length; ++i)
            miniGolems_[i].Run();

        for (int i = 0; i < golems_.Length; ++i)
        {
            golems_[i].SetAppearTimeLimits(i * 5, 1);
            golems_[i].enabled = true;
        }

        DarkenAmbientLight();
    }

    public override void ApplyToMap(Vector3Int position)
    {
        ApplyTilemap(position);
        nextLevelPortal_ = GameSceneLogic.Instance.NextLevelPortal;
        nextLevelPortal_.gameObject.SetActive(false);
        nextLevelPortal_.transform.position = PortalPosition.position;
    }

    void DarkenAmbientLight()
    {
        var currentLight = LightingImageEffect.Instance.CurrentValues;
        Color.RGBToHSV(currentLight.AmbientLight, out float H, out float S, out float V);
        V *= 0.25f;
        currentLight.AmbientLight = Color.HSVToRGB(H, S, V);
        LightingImageEffect.Instance.SetBaseColorTarget(currentLight, 5);
    }

    public override Vector3 GetPlayerStartPosition()
        => PlayerStartPosition.position;

    bool bossDead_;

    public override IEnumerator<float> GameLoopCo()
    {
        yield return Timing.WaitForSeconds(0.5f);

        while (!bossDead_)
        {
            yield return 0;
        }

        AudioManager.Instance.PlayMusic(Music, 0.8f);
    }
}
