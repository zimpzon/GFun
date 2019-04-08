using UnityEngine;

public class TriggerLightingColorChange : MonoBehaviour
{
    public LightingEffectSettings LightingEffectSettings;
    public int Speed = 2;

    LightingImageEffect lightingImageEffect_;

    private void Start()
    {
        lightingImageEffect_ = SceneGlobals.Instance.LightingImageEffect;    
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        SceneGlobals.Instance.DebugLinesScript.SetLine("trigger", Time.time);
        lightingImageEffect_.SetBaseColorTarget(LightingEffectSettings, Speed);
    }
}
