using GFun;
using System;
using UnityEngine;
using UnityEngine.UI;

public class Options : MonoBehaviour
{
    public Slider MusicVolumeControl;
    public Slider SFxVolumeControl;
    public float MusicVolume;
    public float SFxVolume;

    private SceneGlobals globals;

    // Start is called before the first frame update
    void Start()
    {
        MusicVolume = PlayerPrefs.GetFloat(PlayerPrefsNames.MusicVolume);
        SFxVolume = PlayerPrefs.GetFloat(PlayerPrefsNames.SfxVolume);
        MusicVolumeControl.value = MusicVolume;
        SFxVolumeControl.value = SFxVolume;
        globals = SceneGlobals.Instance;
    }

    public void AdjustMusicVolume(Single volume)
    {
        MusicVolume = volume;
        globals.AudioManager.SetMusicVolume(MusicVolume);
        PlayerPrefs.SetFloat(PlayerPrefsNames.MusicVolume, MusicVolume);
    }

    public void AdjustSFxVolume(Single volume)
    { 
        SFxVolume = volume;
        globals.AudioManager.SetSfxVolume(SFxVolume);
        PlayerPrefs.SetFloat(PlayerPrefsNames.SfxVolume, SFxVolume);
    }
}
