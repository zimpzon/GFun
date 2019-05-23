using MEC;
using System.Collections.Generic;
using UnityEngine;

public class Dungeon1BossPluginScript : MapPluginScript
{
    public AudioClip Music;
    public override string Name => "The Golem King";
    public Transform PlayerStartPosition;
    public Transform PortalPosition;
    public GameObject Chest;

    Color baseAmbientColor_;
    MiniGolemController[] miniGolems_;
    GolemController[] golems_;
    GolemKingController golemKing_;
    PortalScript nextLevelPortal_;

    private void Start()
    {
        CurrentRunData.Instance.Boss1Attempts++;

        golemKing_ = FindObjectOfType<GolemKingController>();

        miniGolems_ = FindObjectsOfType<MiniGolemController>();
        golems_ = FindObjectsOfType<GolemController>();
        for (int i = 0; i < golems_.Length; ++i)
            golems_[i].gameObject.SetActive(false);

        GameEvents.OnGolemKingCallingForHelp += GameEvents_OnGolemKingCallingForHelp;
        GameEvents.OnAllEnemiesKilled += GameEvents_OnAllEnemiesKilled;

        nextLevelPortal_ = GameSceneLogic.Instance.NextLevelPortal;
        nextLevelPortal_.gameObject.SetActive(true);
        nextLevelPortal_.transform.position = Vector3.right * 800;

        Chest.SetActive(false);
    }

    private void GameEvents_OnAllEnemiesKilled()
    {
        CurrentRunData.Instance.Boss1Kills++;
        Timing.RunCoroutine(VictoryCo().CancelWith(this.gameObject));
    }

    IEnumerator<float> VictoryCo()
    {
        nextLevelPortal_.transform.position = PortalPosition.position;
        Chest.SetActive(true);
        ScaleAmbientLight(1.0f);

        yield return Timing.WaitForSeconds(2.0f);
        PlayerInfoScript.Instance.ShowInfo("Victory!", Color.yellow);

        // Let the dust settle
        yield return Timing.WaitForSeconds(5.0f);

        AudioManager.Instance.PlayMusic(Music, 0.8f);
    }

    private void GameEvents_OnGolemKingCallingForHelp()
    {
        for (int i = 0; i < miniGolems_.Length; ++i)
            miniGolems_[i].Run(i * 2f + 2);

        for (int i = 0; i < golems_.Length; ++i)
        {
            golems_[i].SetAppearTimeLimits(i * 4, 0);
            golems_[i].gameObject.SetActive(true);
        }

        baseAmbientColor_ = LightingImageEffect.Instance.CurrentValues.AmbientLight;
        ScaleAmbientLight(0.01f);
    }

    public override void ApplyToMap(Vector3Int position)
    {
        ApplyTilemap(position);
    }

    void ScaleAmbientLight(float amount)
    {
        var currentLight = LightingImageEffect.Instance.CurrentValues;
        Color.RGBToHSV(baseAmbientColor_, out float H, out float S, out float V);
        V *= amount;
        currentLight.AmbientLight = Color.HSVToRGB(H, S, V);
        LightingImageEffect.Instance.SetBaseColorTarget(currentLight, 10);
    }

    public override Vector3 GetPlayerStartPosition()
        => PlayerStartPosition.position;
}
