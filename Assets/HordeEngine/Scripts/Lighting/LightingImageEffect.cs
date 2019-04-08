using System;
using UnityEngine;

public struct LightingEffectSettings
{
    public void SetDefaults()
    {
        Brightness = 1.2f;
        MonochromeAmount = 0.0f;
        MonochromeFactorR = 0.22f;
        MonochromeFactorG = 0.67f;
        MonochromeFactorB = 0.11f;
        MonochromeDisplayR = 1.0f;
        MonochromeDisplayG = 1.0f;
        MonochromeDisplayB = 1.0f;
    }

    public float Brightness;
    public float MonochromeAmount;
    public float MonochromeFactorR;
    public float MonochromeFactorG;
    public float MonochromeFactorB;
    public float MonochromeDisplayR;
    public float MonochromeDisplayG;
    public float MonochromeDisplayB;
}

public class LightingImageEffect : MonoBehaviour
{
    // TODO: make private
    public Material EffectMaterial;
    public float Brightness = 1.0f;
    public float MonochromeAmount = 0.0f;
    public float MonochromeFactorR = 1.0f;
    public float MonochromeFactorG = -1.0f;
    public float MonochromeFactorB = 1.0f;
    public float MonochromeDisplayR = 0.1f;
    public float MonochromeDisplayG = 1.0f;
    public float MonochromeDisplayB = 0.0f;

    [NonSerialized] public RenderTexture LightingTexture;

    LightingEffectSettings flash_;
    LightingEffectSettings target_;
    LightingEffectSettings current_;

    float baseTransitionSpeed_;
    float flashTimeMs_;
    float flashStartTime_;
    AnimationCurve flashCurve_;

    public void SetBaseColorTarget(LightingEffectSettings target, float transitionSpeed = 30)
    {
        target_ = target;
        baseTransitionSpeed_ = transitionSpeed;
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

    void MoveToTarget(float target, float dt, ref float value)
    {
        float diff = target - value;
        value += diff * dt * baseTransitionSpeed_;
    }

    void MoveCurrentToTarget(float dt)
    {
        MoveToTarget(target_.Brightness, dt, ref current_.Brightness);
        MoveToTarget(target_.MonochromeAmount, dt, ref current_.MonochromeAmount);

        MoveToTarget(target_.MonochromeFactorR, dt, ref current_.MonochromeFactorR);
        MoveToTarget(target_.MonochromeFactorG, dt, ref current_.MonochromeFactorG);
        MoveToTarget(target_.MonochromeFactorB, dt, ref current_.MonochromeFactorB);

        MoveToTarget(target_.MonochromeDisplayR, dt, ref current_.MonochromeDisplayR);
        MoveToTarget(target_.MonochromeDisplayG, dt, ref current_.MonochromeDisplayG);
        MoveToTarget(target_.MonochromeDisplayB, dt, ref current_.MonochromeDisplayB);
    }

    private void Start()
    {
        current_.Brightness = EffectMaterial.GetFloat("_Brightness");
        current_.MonochromeAmount = EffectMaterial.GetFloat("_MonochromeAmount");

        current_.MonochromeFactorR = EffectMaterial.GetFloat("_MonochromeFactorR");
        current_.MonochromeFactorG = EffectMaterial.GetFloat("_MonochromeFactorG");
        current_.MonochromeFactorB = EffectMaterial.GetFloat("_MonochromeFactorB");

        current_.MonochromeFactorR = EffectMaterial.GetFloat("_MonochromeDisplayR");
        current_.MonochromeFactorG = EffectMaterial.GetFloat("_MonochromeDisplayG");
        current_.MonochromeFactorB = EffectMaterial.GetFloat("_MonochromeDisplayB");
    }

    private void Update()
    {
        float dt = Time.unscaledDeltaTime;
        MoveCurrentToTarget(dt);
        SceneGlobals.Instance.DebugLinesScript.SetLine("mono", current_.MonochromeAmount);
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        EffectMaterial.SetTexture("_LightingTex", LightingTexture);
        EffectMaterial.SetFloat("_Brightness", Brightness);
        EffectMaterial.SetFloat("_MonochromeAmount", current_.MonochromeAmount);
        EffectMaterial.SetFloat("_MonochromeFactorR", current_.MonochromeFactorR);
        EffectMaterial.SetFloat("_MonochromeFactorG", current_.MonochromeFactorG);
        EffectMaterial.SetFloat("_MonochromeFactorB", current_.MonochromeFactorB);
        EffectMaterial.SetFloat("_MonochromeDisplayR", current_.MonochromeDisplayR);
        EffectMaterial.SetFloat("_MonochromeDisplayG", current_.MonochromeDisplayG);
        EffectMaterial.SetFloat("_MonochromeDisplayB", current_.MonochromeDisplayB);

        Graphics.Blit(source, destination, EffectMaterial);
    }
}
