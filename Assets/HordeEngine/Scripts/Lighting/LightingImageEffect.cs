using System;
using UnityEngine;

[Serializable]
public class LightingEffectSettings
{
    public void CopyTo(LightingEffectSettings other)
    {
        other.AmbientLight = AmbientLight;
        other.Brightness = Brightness;
        other.MonochromeAmount = MonochromeAmount;
        other.MonochromeFactorR = MonochromeFactorR;
        other.MonochromeFactorG = MonochromeFactorG;
        other.MonochromeFactorB = MonochromeFactorB;
        other.MonochromeDisplayR = MonochromeDisplayR;
        other.MonochromeDisplayG = MonochromeDisplayG;
        other.MonochromeDisplayB = MonochromeDisplayB;
    }

    public Color AmbientLight = new Color(0.9f, 0.9f, 0.9f);
    public float Brightness = 1.2f;
    public float MonochromeAmount = 0.0f;
    public float MonochromeFactorR = 0.23f;
    public float MonochromeFactorG = 0.66f;
    public float MonochromeFactorB = 0.11f;
    public float MonochromeDisplayR = 1.0f;
    public float MonochromeDisplayG = 1.0f;
    public float MonochromeDisplayB = 1.0f;
}

public class LightingImageEffect : MonoBehaviour
{
    public static LightingImageEffect Instance;

    public Material EffectMaterial;

    [NonSerialized] public RenderTexture LightingTexture;
    public LightingEffectSettings StartValues = new LightingEffectSettings();
    public LightingEffectSettings CurrentValues = new LightingEffectSettings();

    LightingEffectSettings flash_;
    LightingEffectSettings target_;

    float baseTransitionSpeed_;
    float flashTimeMs_;
    float flashStartTime_;
    AnimationCurve flashCurve_;

    int id_LightingTex;
    int id_Brightness;
    int id_MonochromeAmount;
    int id_MonochromeFactorR;
    int id_MonochromeFactorG;
    int id_MonochromeFactorB;
    int id_MonochromeDisplayR;
    int id_MonochromeDisplayG;
    int id_MonochromeDisplayB;

    public void SetBaseColorTarget(LightingEffectSettings target, float transitionSpeed = 30)
    {
        target_ = target ?? throw new NullReferenceException();
        baseTransitionSpeed_ = transitionSpeed;
    }

    public void SetBaseColor(LightingEffectSettings baseColor)
    {
        target_ = baseColor;
        target_.CopyTo(CurrentValues);
    }

    public void FlashColor(LightingEffectSettings flashTarget, AnimationCurve flashCurve, float flashTimeMs)
    {
        flashTimeMs_ = flashTimeMs;
        flashCurve_ = flashCurve;
        flashStartTime_ = Time.unscaledTime;
    }

    float GetFlashAmount()
    {
        float duration = Time.unscaledTime - flashStartTime_;
        if (duration > flashTimeMs_)
            return 0.0f;

        float t = duration / flashTimeMs_;
        float value = flashCurve_.Evaluate(t);
        return value;
    }

    private void Awake()
    {
        Instance = this;

        CurrentValues = new LightingEffectSettings();
        target_ = new LightingEffectSettings();

        id_LightingTex = Shader.PropertyToID("_LightingTex");
        id_Brightness = Shader.PropertyToID("_Brightness");
        id_MonochromeAmount = Shader.PropertyToID("_MonochromeAmount");
        id_MonochromeFactorR = Shader.PropertyToID("_MonochromeFactorR");
        id_MonochromeFactorG = Shader.PropertyToID("_MonochromeFactorG");
        id_MonochromeFactorB = Shader.PropertyToID("_MonochromeFactorB");
        id_MonochromeDisplayR = Shader.PropertyToID("_MonochromeDisplayR");
        id_MonochromeDisplayG = Shader.PropertyToID("_MonochromeDisplayG");
        id_MonochromeDisplayB = Shader.PropertyToID("_MonochromeDisplayB");

        SetBaseColor(StartValues);
    }

    void MoveToTarget(float target, float dt, ref float value)
    {
        float diff = target - value;
        value += diff * dt * baseTransitionSpeed_;
    }

    void MoveCurrentToTarget(float dt)
    {
        if (target_ == null || CurrentValues == null)
            return;

        MoveToTarget(target_.AmbientLight.r, dt, ref CurrentValues.AmbientLight.r);
        MoveToTarget(target_.AmbientLight.g, dt, ref CurrentValues.AmbientLight.g);
        MoveToTarget(target_.AmbientLight.b, dt, ref CurrentValues.AmbientLight.b);

        MoveToTarget(target_.Brightness, dt, ref CurrentValues.Brightness);
        MoveToTarget(target_.MonochromeAmount, dt, ref CurrentValues.MonochromeAmount);

        MoveToTarget(target_.MonochromeFactorR, dt, ref CurrentValues.MonochromeFactorR);
        MoveToTarget(target_.MonochromeFactorG, dt, ref CurrentValues.MonochromeFactorG);
        MoveToTarget(target_.MonochromeFactorB, dt, ref CurrentValues.MonochromeFactorB);

        MoveToTarget(target_.MonochromeDisplayR, dt, ref CurrentValues.MonochromeDisplayR);
        MoveToTarget(target_.MonochromeDisplayG, dt, ref CurrentValues.MonochromeDisplayG);
        MoveToTarget(target_.MonochromeDisplayB, dt, ref CurrentValues.MonochromeDisplayB);
    }

    private void Update()
    {
        float dt = Time.unscaledDeltaTime;
        MoveCurrentToTarget(dt);
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        EffectMaterial.SetTexture(id_LightingTex, LightingTexture);
        EffectMaterial.SetFloat(id_Brightness, CurrentValues.Brightness);
        EffectMaterial.SetFloat(id_MonochromeAmount, CurrentValues.MonochromeAmount);
        EffectMaterial.SetFloat(id_MonochromeDisplayR, CurrentValues.MonochromeDisplayR);
        EffectMaterial.SetFloat(id_MonochromeDisplayG, CurrentValues.MonochromeDisplayG);
        EffectMaterial.SetFloat(id_MonochromeDisplayB, CurrentValues.MonochromeDisplayB);
        EffectMaterial.SetFloat(id_MonochromeFactorR, CurrentValues.MonochromeFactorR);
        EffectMaterial.SetFloat(id_MonochromeFactorG, CurrentValues.MonochromeFactorG);
        EffectMaterial.SetFloat(id_MonochromeFactorB, CurrentValues.MonochromeFactorB);

        //EffectMaterial.SetTexture("_LightingTex", LightingTexture);
        //EffectMaterial.SetFloat("_Brightness", CurrentValues.Brightness);
        //EffectMaterial.SetFloat("_MonochromeAmount", CurrentValues.MonochromeAmount);
        //EffectMaterial.SetFloat("_MonochromeFactorR", CurrentValues.MonochromeFactorR);
        //EffectMaterial.SetFloat("_MonochromeFactorG", CurrentValues.MonochromeFactorG);
        //EffectMaterial.SetFloat("_MonochromeFactorB", CurrentValues.MonochromeFactorB);
        //EffectMaterial.SetFloat("_MonochromeDisplayR", CurrentValues.MonochromeDisplayR);
        //EffectMaterial.SetFloat("_MonochromeDisplayG", CurrentValues.MonochromeDisplayG);
        //EffectMaterial.SetFloat("_MonochromeDisplayB", CurrentValues.MonochromeDisplayB);

        Graphics.Blit(source, destination, EffectMaterial);
    }
}
