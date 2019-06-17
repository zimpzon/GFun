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
    public float Darkness = 0;
    public LightingEffectSettings StartValues = new LightingEffectSettings();
    public LightingEffectSettings CurrentValues = new LightingEffectSettings();
    LightingEffectSettings transitionValues_ = new LightingEffectSettings();

    LightingEffectSettings flash_ = new LightingEffectSettings();
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
    int id_Darkness;
    int id_AspectRatio;

    public void SetBaseColorTarget(LightingEffectSettings target, float transitionSpeed = 30)
    {
        target_ = target ?? throw new NullReferenceException();
        baseTransitionSpeed_ = transitionSpeed;
    }

    public void SetBaseColor(LightingEffectSettings baseColor)
    {
        target_ = baseColor;
        target_.CopyTo(CurrentValues);
        target_.CopyTo(transitionValues_);
    }

    public void FlashColor(LightingEffectSettings flashTarget, AnimationCurve flashCurve, float flashTimeMs)
    {
        flash_ = flashTarget;
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
        id_Darkness = Shader.PropertyToID("_Darkness");
        id_AspectRatio = Shader.PropertyToID("_AspectRatio");

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

        MoveToTarget(target_.AmbientLight.r, dt, ref transitionValues_.AmbientLight.r);
        MoveToTarget(target_.AmbientLight.g, dt, ref transitionValues_.AmbientLight.g);
        MoveToTarget(target_.AmbientLight.b, dt, ref transitionValues_.AmbientLight.b);

        MoveToTarget(target_.Brightness, dt, ref transitionValues_.Brightness);
        MoveToTarget(target_.MonochromeAmount, dt, ref transitionValues_.MonochromeAmount);

        MoveToTarget(target_.MonochromeFactorR, dt, ref transitionValues_.MonochromeFactorR);
        MoveToTarget(target_.MonochromeFactorG, dt, ref transitionValues_.MonochromeFactorG);
        MoveToTarget(target_.MonochromeFactorB, dt, ref transitionValues_.MonochromeFactorB);

        MoveToTarget(target_.MonochromeDisplayR, dt, ref transitionValues_.MonochromeDisplayR);
        MoveToTarget(target_.MonochromeDisplayG, dt, ref transitionValues_.MonochromeDisplayG);
        MoveToTarget(target_.MonochromeDisplayB, dt, ref transitionValues_.MonochromeDisplayB);

        float flashAmount = GetFlashAmount();
        CurrentValues.AmbientLight.r = transitionValues_.AmbientLight.r + flash_.AmbientLight.r * flashAmount;
        CurrentValues.AmbientLight.g = transitionValues_.AmbientLight.g + flash_.AmbientLight.g * flashAmount;
        CurrentValues.AmbientLight.b = transitionValues_.AmbientLight.b + flash_.AmbientLight.b * flashAmount;

        CurrentValues.Brightness = transitionValues_.Brightness + flash_.Brightness * flashAmount;
        CurrentValues.MonochromeAmount = transitionValues_.MonochromeAmount + flash_.MonochromeAmount * flashAmount;

        CurrentValues.MonochromeFactorR = transitionValues_.MonochromeFactorR + flash_.MonochromeFactorR * flashAmount;
        CurrentValues.MonochromeFactorG = transitionValues_.MonochromeFactorG + flash_.MonochromeFactorG * flashAmount;
        CurrentValues.MonochromeFactorB  = transitionValues_.MonochromeFactorB + flash_.MonochromeFactorB * flashAmount;

        CurrentValues.MonochromeDisplayR = transitionValues_.MonochromeDisplayR + flash_.MonochromeDisplayR * flashAmount;
        CurrentValues.MonochromeDisplayG = transitionValues_.MonochromeDisplayG + flash_.MonochromeDisplayG * flashAmount;
        CurrentValues.MonochromeDisplayB = transitionValues_.MonochromeDisplayB + flash_.MonochromeDisplayB * flashAmount;
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

        float aspectRatio = Screen.width / (float)Screen.height;
        EffectMaterial.SetFloat(id_AspectRatio, aspectRatio);
        EffectMaterial.SetFloat(id_Darkness, Darkness);

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
