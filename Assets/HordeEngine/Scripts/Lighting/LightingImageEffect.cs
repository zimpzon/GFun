using System;
using UnityEngine;

public class LightingImageEffect : MonoBehaviour
{
    public Material EffectMaterial;
    public float Brightness = 1.0f;
    public float MonochromeAmount = 0.0f;
    public float MonochromeFactorR = 1.0f;
    public float MonochromeFactorG = -1.0f;
    public float MonochromeFactorB = 1.0f;
    public float MonochromeDisplayR = 0.1f;
    public float MonochromeDisplayG = 1.0f;
    public float MonochromeDisplayB = 0.0f;

    [NonSerialized]public RenderTexture LightingTexture;

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        EffectMaterial.SetTexture("_LightingTex", LightingTexture);
        EffectMaterial.SetFloat("_Brightness", Brightness);
        EffectMaterial.SetFloat("_MonochromeAmount", MonochromeAmount);
        EffectMaterial.SetFloat("_MonochromeFactorR", MonochromeFactorR);
        EffectMaterial.SetFloat("_MonochromeFactorG", MonochromeFactorG);
        EffectMaterial.SetFloat("_MonochromeFactorB", MonochromeFactorB);
        EffectMaterial.SetFloat("_MonochromeDisplayR", MonochromeDisplayR);
        EffectMaterial.SetFloat("_MonochromeDisplayG", MonochromeDisplayG);
        EffectMaterial.SetFloat("_MonochromeDisplayB", MonochromeDisplayB);

        Graphics.Blit(source, destination, EffectMaterial);
    }
}
