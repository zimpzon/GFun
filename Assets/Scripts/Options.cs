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
    private bool isMusicPlaying;
    private bool isAmbientSoundPlaying;

    // Start is called before the first frame update
    void Awake()
    {
        MusicVolume = PlayerPrefs.GetFloat(PlayerPrefsNames.MaxMusicVolume, 1f);
        SFxVolume = PlayerPrefs.GetFloat(PlayerPrefsNames.SfxVolume, 1f);
        AmbientVolume = PlayerPrefs.GetFloat(PlayerPrefsNames.AmbientVolume, 1f);

        MusicVolumeControl.value = MusicVolume;
        SFxVolumeControl.value = SFxVolume;
        AmbientVolumeControl.value = AmbientVolume;
    }

    public void AdjustMusicVolume(Single volume)
    {
        MusicVolume = volume;
        PlayerPrefs.SetFloat(PlayerPrefsNames.MaxMusicVolume, MusicVolume);
        SceneGlobals globals = SceneGlobals.Instance;
        StartCoroutine(globals.AudioManager.SetAudioProfile(AudioManager.eScene.InMenuSetMusicVolume));
        if (!isMusicPlaying)
        {
            isMusicPlaying = true;
            globals.AudioManager.PlayMusic(globals.AudioManager.AudioClips.IntroMusic);
        }
        globals.AudioManager.StopAmbience();
        isAmbientSoundPlaying = false;
    }


    public void AdjustSFxVolume(Single volume)
    {
        SFxVolume = volume;
        PlayerPrefs.SetFloat(PlayerPrefsNames.SfxVolume, SFxVolume);
        SceneGlobals globals = SceneGlobals.Instance;
        globals.AudioManager.StopMusic();
        globals.AudioManager.StopAmbience();
        StartCoroutine(globals.AudioManager.SetAudioProfile(AudioManager.eScene.InMenuSetSfxVolume));
        isMusicPlaying = false;
        isAmbientSoundPlaying = false;
        globals.AudioManager.PlaySfxClip(globals.AudioManager.AudioClips.ShotgunShot, 1);
    }

    public void AdjustAmbientVolume(Single volume)
    {
        AmbientVolume = volume;
        PlayerPrefs.SetFloat(PlayerPrefsNames.AmbientVolume, AmbientVolume);
        SceneGlobals globals = SceneGlobals.Instance;
        globals.AudioManager.StopMusic();
        StartCoroutine(globals.AudioManager.SetAudioProfile(AudioManager.eScene.InMenuSetAmbientVolume));
        isMusicPlaying = false;
        if (!isAmbientSoundPlaying)
        {
            isAmbientSoundPlaying = true;
            globals.AudioManager.PlayAmbience(globals.AudioManager.AudioClips.Campfire, true);
        }
    }
}
