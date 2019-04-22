using GFun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    public AudioClips AudioClips;
    public int SfxSourceCount = 20;
    public bool ShowDebugOutput = false;
    public AudioMixerGroup MusicMixerGroup;
    public AudioMixerGroup SfxMixerGroup;
    public AudioMixerGroup AmbientMixerGroup;

    private AudioMixerSnapshot IntroScreenSnapshot;
    private AudioMixerSnapshot InGameSnapshot;

    private bool IsMusicPlaying;

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

        SetMusicVolume(PlayerPrefs.GetFloat(PlayerPrefsNames.MaxMusicVolume, 0));
        SetSfxVolume(PlayerPrefs.GetFloat(PlayerPrefsNames.SfxVolume, 0));
        SetAmbientVolume(PlayerPrefs.GetFloat(PlayerPrefsNames.AmbientVolume, 0));

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

    public void PlaySfxClip(AudioClip clip, int maxInstances, float pitchRandomVariation = 0.0f)
    {
        AudioSource selectedSource = null;
        int count = CountPlayingInstancesOfClip(clip, out AudioSource existingPlayingSource);
        selectedSource = count >= maxInstances ? existingPlayingSource : GetSourceForNewSound(replaceIfNoneAvailable: true);
        if (selectedSource != null)
        {
            // Replace an existing source
            selectedSource.Stop();
            selectedSource.clip = clip;
            selectedSource.pitch = 1.0f + (pitchRandomVariation * UnityEngine.Random.value - pitchRandomVariation * 0.5f);
            selectedSource.Play();
        }
    }

    public void StopMusic()
    {
        IsMusicPlaying = false;
        InGameSnapshot.TransitionTo(2.0f);
        StartCoroutine(StopMusicCo());
    }

    public void PlayMusic(AudioClip clip)
    {
        if (!IsMusicPlaying)
        {
            IsMusicPlaying = true;
            IntroScreenSnapshot.TransitionTo(0.5f);
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
    public float GetMusicVolume()
    {
        float volume;
        MusicMixerGroup.audioMixer.GetFloat("MusicVolume", out volume);
        return volume;
    }

    public void SetMusicVolume(float volume)
    {
        MusicMixerGroup.audioMixer.SetFloat("MusicVolume", volume);
    }

    public float GetSfxVolume()
    {
        float volume;
        SfxMixerGroup.audioMixer.GetFloat("SFXVolume", out volume);
        return volume;
    }

    public void SetSfxVolume(float volume)
    {
        SfxMixerGroup.audioMixer.SetFloat("SFXVolume", volume);
    }

    public float GetAmbientVolume()
    {
        float volume;
        AmbientMixerGroup.audioMixer.GetFloat("AmbientVolume", out volume);
        return volume;
    }

    public void SetAmbientVolume(float volume)
    {
        AmbientMixerGroup.audioMixer.SetFloat("AmbientVolume", volume);
    }
    #endregion
}
