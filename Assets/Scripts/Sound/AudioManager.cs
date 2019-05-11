using GFun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    public enum eScene
    {
        IntroScreen,
        InGame,
        InMenu,
        InMenuSetMusicVolume,
        InMenuSetSfxVolume,
        InMenuSetAmbientVolume,
        Max
    }
    public AudioClips AudioClips;
    public int SfxSourceCount = 20;
    public bool ShowDebugOutput = false;
    public AudioMixerGroup MusicMixerGroup;
    public AudioMixerGroup SfxMixerGroup;
    public AudioMixerGroup AmbientMixerGroup;

    private AudioMixerSnapshot IntroScreenSnapshot;
    private AudioMixerSnapshot InGameSnapshot;
    private AudioMixerSnapshot InMenuSnapshot;
    private AudioMixerSnapshot InMenuSetMusicVolumeSnapshot;
    private AudioMixerSnapshot InMenuSetSfxVolumeSnapshot;
    private AudioMixerSnapshot InMenuSetAmbientVolumeSnapshot;

    private bool IsMusicPlaying;

    private float MinVolume = 0.0001f;
    private float MinVolumenInDb = -80f;

    private eScene CurrentScene = eScene.Max;

    AudioSource musicSource_;
    AudioSource ambienceSource_;
    List<AudioSource> sfxSources_ = new List<AudioSource>();
    
    bool debugIsShown_;

    void Awake()
    {
        Instance = this;

        musicSource_ = gameObject.AddComponent<AudioSource>();
        musicSource_.loop = true;
        ambienceSource_ = gameObject.AddComponent<AudioSource>();
        musicSource_.outputAudioMixerGroup = MusicMixerGroup;
        ambienceSource_.outputAudioMixerGroup = AmbientMixerGroup;

        IntroScreenSnapshot = MusicMixerGroup.audioMixer.FindSnapshot("IntroScreen");
        InGameSnapshot = MusicMixerGroup.audioMixer.FindSnapshot("InGame");
        InMenuSnapshot = MusicMixerGroup.audioMixer.FindSnapshot("InMenu");

        InMenuSetMusicVolumeSnapshot = MusicMixerGroup.audioMixer.FindSnapshot("OptionsMusic");
        InMenuSetSfxVolumeSnapshot = MusicMixerGroup.audioMixer.FindSnapshot("OptionsSFX");
        InMenuSetAmbientVolumeSnapshot = MusicMixerGroup.audioMixer.FindSnapshot("OptionsAmbient");

        for (int i = 0; i < SfxSourceCount; ++i)
        {
            var sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.outputAudioMixerGroup = SfxMixerGroup;
            sfxSources_.Add(sfxSource);
        }
    }

    int CountPlayingInstancesOfClip(AudioClip clip, out AudioSource currentlyPlayingInstance)
    {
        currentlyPlayingInstance = null;
        int result = 0;
        for(int i = 0; i < sfxSources_.Count; ++i)
        {
            if (sfxSources_[i].isPlaying && sfxSources_[i].clip == clip)
            {
                result++;
                currentlyPlayingInstance = sfxSources_[i];
            }
        }
        return result;
    }

    AudioSource GetSourceForNewSound(bool replaceIfNoneAvailable)
    {
        for (int i = 0; i < sfxSources_.Count; ++i)
        {
            if (!sfxSources_[i].isPlaying)
                return sfxSources_[i];
        }

        // No sources were available, replace a random existing if requested, else return null
        return replaceIfNoneAvailable ? sfxSources_[UnityEngine.Random.Range(0, sfxSources_.Count)] : null;
    }

    #region Coroutines
    private IEnumerator PlayMusicCo(AudioClip clip)
    {
        musicSource_.clip = clip;
        musicSource_.loop = true;
        musicSource_.Play();

        yield return null;
    }

    private IEnumerator StopMusicCo()
    {
        musicSource_.Stop();
        yield return null;
    }

    #endregion  

    public void PlaySfxClip(AudioClip clip, int maxInstances, float pitchRandomVariation = 0.0f, float pitch = 1.0f)
    {
        AudioSource selectedSource = null;
        int count = CountPlayingInstancesOfClip(clip, out AudioSource existingPlayingSource);
        selectedSource = count >= maxInstances ? existingPlayingSource : GetSourceForNewSound(replaceIfNoneAvailable: true);
        if (selectedSource != null)
        {
            // Replace an existing source
            selectedSource.Stop();
            selectedSource.clip = clip;
            selectedSource.pitch = pitch + (pitchRandomVariation * UnityEngine.Random.value - pitchRandomVariation * 0.5f);
            selectedSource.Play();
        }
    }

    public void StopMusic()
    {
        IsMusicPlaying = false;
        StartCoroutine(StopMusicCo());
    }

    public void PlayMusic(AudioClip clip)
    {
        if (!IsMusicPlaying)
        {
            IsMusicPlaying = true;
            StartCoroutine(PlayMusicCo(clip));
        }
    }

    public void StopAmbience()
    {
        ambienceSource_.Stop();
    }

    public void PlayAmbience(AudioClip clip, bool loop)
    {
        ambienceSource_.Stop();
        ambienceSource_.clip = clip;
        ambienceSource_.loop = loop;
        ambienceSource_.Play();
    }

    private void Update()
    {
        if (ShowDebugOutput)
        {
            debugIsShown_ = true;
            for (int i = 0; i < sfxSources_.Count; ++i)
            {
                SceneGlobals.Instance.DebugLinesScript.SetLine("AudioSource " + i, sfxSources_[i].clip?.name);
            }
        }
        else
        {
            if (debugIsShown_)
            {
                debugIsShown_ = false;
                for (int i = 0; i < sfxSources_.Count; ++i)
                {
                    SceneGlobals.Instance.DebugLinesScript.RemoveLine("AudioSource " + i);
                }
            }
        }
    }

    #region Volume Getters and Setters
    private void ClearVolumeValues()
    {
        MusicMixerGroup.audioMixer.ClearFloat("MusicVolume");
        MusicMixerGroup.audioMixer.ClearFloat("SFXVolume");
        MusicMixerGroup.audioMixer.ClearFloat("AmbientVolume");
    }

    private float GetMusicVolume()
    {
        float volume;
        MusicMixerGroup.audioMixer.GetFloat("MusicVolume", out volume);
        return volume;
    }

    private void SetMusicVolume(float volume)
    {
        MusicMixerGroup.audioMixer.SetFloat("MusicVolume", Mathf.Log10(volume) * 20);
    }

    private float GetSfxVolume()
    {
        float volume;
        SfxMixerGroup.audioMixer.GetFloat("SFXVolume", out volume);
        return volume;
    }

    private void SetSfxVolume(float volume)
    {
        SfxMixerGroup.audioMixer.SetFloat("SFXVolume", Mathf.Log10(volume) * 20);
    }

    private float GetAmbientVolume()
    {
        float volume;
        AmbientMixerGroup.audioMixer.GetFloat("AmbientVolume", out volume);
        return volume;
    }

    private void SetAmbientVolume(float volume)
    {
        AmbientMixerGroup.audioMixer.SetFloat("AmbientVolume", Mathf.Log10(volume) * 20);
    }
    #endregion

    public IEnumerator SetAudioProfile(eScene scene)
    {
        yield return null;
        if (CurrentScene != scene)
        {
            CurrentScene = scene;
            ClearVolumeValues();
        }
        switch (scene)
        {
            case eScene.IntroScreen:
                IntroScreenSnapshot.TransitionTo(0f);
                break;
            case eScene.InGame:
                InGameSnapshot.TransitionTo(2.0f);
                break;
            case eScene.InMenu:
                InMenuSnapshot.TransitionTo(0);
                break;
            case eScene.InMenuSetMusicVolume:
                InMenuSetMusicVolumeSnapshot.TransitionTo(0);
                break;
            case eScene.InMenuSetSfxVolume:
                InMenuSetSfxVolumeSnapshot.TransitionTo(0);
                break;
            case eScene.InMenuSetAmbientVolume:
                InMenuSetAmbientVolumeSnapshot.TransitionTo(0);
                break;
            case eScene.Max:
            default:
                break;
        }

        StartCoroutine(SetPlayerVolume());
    }

    private IEnumerator SetPlayerVolume()
    {
        yield return null;

        float playerMusicVolumeAdjustment = PlayerPrefs.GetFloat(PlayerPrefsNames.MaxMusicVolume, 1f);
        float musicVolumeInDb = GetMusicVolume();
        SetMusicVolume(musicVolumeInDb > MinVolumenInDb ? playerMusicVolumeAdjustment : MinVolume);

        float playerSfxVolumeAdjustment = PlayerPrefs.GetFloat(PlayerPrefsNames.SfxVolume, 1f);
        float sfxVolumeInDb = GetSfxVolume();
        SetSfxVolume(sfxVolumeInDb > MinVolumenInDb ? playerSfxVolumeAdjustment : MinVolume);

        float playerAmbientVolumeAdjustment = PlayerPrefs.GetFloat(PlayerPrefsNames.AmbientVolume, 1f);
        float ambientVolumeInDb = GetAmbientVolume();
        SetAmbientVolume(ambientVolumeInDb > MinVolumenInDb ? playerAmbientVolumeAdjustment : MinVolume);
    }
}
