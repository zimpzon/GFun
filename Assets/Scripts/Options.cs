using GFun;
using System;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class Options : MonoBehaviour
{
    public Slider MusicVolumeControl;
    public Slider SFxVolumeControl;
    public Slider AmbientVolumeControl;

    public AudioMixerGroup MusicGroup;
    public AudioMixerGroup SFXGroup;
    public AudioMixerGroup AmbientGroup;

    private float MusicVolume;
    private float SFxVolume;
    private float AmbientVolume;

    // Start is called before the first frame update
    void Awake()
    {
        MusicVolume = PlayerPrefs.GetFloat(PlayerPrefsNames.MaxMusicVolume, 0.8f);
        SFxVolume = PlayerPrefs.GetFloat(PlayerPrefsNames.SfxVolume, 0.8f);
        AmbientVolume = PlayerPrefs.GetFloat(PlayerPrefsNames.AmbientVolume, 0.8f);

        MusicVolumeControl.value = MusicVolume;
        SFxVolumeControl.value = SFxVolume;
        AmbientVolumeControl.value = AmbientVolume;

        SceneGlobals globals;
        globals = SceneGlobals.Instance;
        globals.AudioManager.SetMusicVolume(MusicVolume);
        globals.AudioManager.SetSfxVolume(SFxVolume);
        globals.AudioManager.SetAmbientVolume(AmbientVolume);
    }

    public void AdjustMusicVolume(Single volume)
    {
        SceneGlobals globals = SceneGlobals.Instance;
        globals.AudioManager.PlayMusic(globals.AudioManager.AudioClips.IntroMusic);
        globals.AudioManager.StopAmbience();
        MusicVolume = volume;
        globals.AudioManager.SetMusicVolume(MusicVolume);
        PlayerPrefs.SetFloat(PlayerPrefsNames.MaxMusicVolume, MusicVolume);
    }

    public void AdjustSFxVolume(Single volume)
    {
        SceneGlobals globals = SceneGlobals.Instance;
        globals.AudioManager.StopMusic();
        globals.AudioManager.StopAmbience();
        globals.AudioManager.PlaySfxClip(globals.AudioManager.AudioClips.ShotgunShot, 1);
        SFxVolume = volume;
        globals.AudioManager.SetSfxVolume(SFxVolume);
        PlayerPrefs.SetFloat(PlayerPrefsNames.SfxVolume, SFxVolume);
    }

    public void AdjustAmbientVolume(Single volume)
    {
        SceneGlobals globals = SceneGlobals.Instance;
        globals.AudioManager.StopMusic();
        globals.AudioManager.PlayAmbience(globals.AudioManager.AudioClips.Campfire, true);
        AmbientVolume = volume;
        globals.AudioManager.SetAmbientVolume(AmbientVolume);
        PlayerPrefs.SetFloat(PlayerPrefsNames.AmbientVolume, AmbientVolume);
    }
}
