using MEC;
using System.Collections.Generic;
using UnityEngine;

public class Dungeon1BossPluginScript : MapPluginScript
{
    public AudioClip Music;
    public AudioClip DistantThunder;
    public override string Name => "The Golem King";
    public Transform PlayerStartPosition;
    public Transform PortalPosition;
    public GameObject Chest;
    public LightingEffectSettings FlashLightingSettings;
    public AnimationCurve FlashCurve;
    public float FlashTime;

    LightingEffectSettings startLight_;
    Color baseAmbientColor_;
    MiniGolemController[] miniGolems_;
    GolemController[] golems_;
    GolemKingController golemKing_;
    PortalScript nextLevelPortal_;
    LightingImageEffect lightingImageEffect_;

    private void Start()
    {
        lightingImageEffect_ = SceneGlobals.Instance.LightingImageEffect;
        startLight_ = LightingImageEffect.Instance.CurrentValues;
        baseAmbientColor_ = LightingImageEffect.Instance.CurrentValues.AmbientLight;
        ScaleAmbientLight(0.6f);
        nextFlash = Time.time + 2 + Random.value;

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

    float nextFlash;
    float nextThunder = float.MaxValue;

    void UpdateThunder(float time)
    {
        if (time > nextFlash)
        {
            lightingImageEffect_.FlashColor(FlashLightingSettings, FlashCurve, FlashTime);
            nextFlash = time + 5.0f + Random.value * 3;
            nextThunder = time + Random.value * 0.3f + 0.3f;
        }

        if (time > nextThunder)
        {
            AudioManager.Instance.PlaySfxClip(DistantThunder, 2, 0.2f);
            nextThunder = float.MaxValue;
        }
    }

    private void Update()
    {
        UpdateThunder(Time.time);
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
    }

    public override void ApplyToMap(Vector3Int position)
    {
        ApplyTilemap(position);
    }

    void ScaleAmbientLight(float amount)
    {
        Color.RGBToHSV(baseAmbientColor_, out float H, out float S, out float V);
        V *= amount;
        startLight_.AmbientLight = Color.HSVToRGB(H, S, V);
        LightingImageEffect.Instance.SetBaseColor(startLight_);
    }

    public override Vector3 GetPlayerStartPosition()
        => PlayerStartPosition.position;
}
