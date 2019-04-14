using GFun;
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
    public AudioMixerGroup AmbianceMixerGroup;

    AudioSource musicSource_;
    AudioSource ambienceSource_;
    List<AudioSource> sfxSources_ = new List<AudioSource>();

    bool debugIsShown_;
    bool musicIsPlaying_;
    float musicVolume_ = 1;
    float sfxVolume_ = 1;

    void Awake()
    {
        Instance = this;

        musicSource_ = gameObject.AddComponent<AudioSource>();
        ambienceSource_ = gameObject.AddComponent<AudioSource>();
        musicSource_.outputAudioMixerGroup = MusicMixerGroup;
        ambienceSource_.outputAudioMixerGroup = AmbianceMixerGroup;
        SetMusicVolume(PlayerPrefs.GetFloat(PlayerPrefsNames.MusicVolume, 1));
        SetSfxVolume(PlayerPrefs.GetFloat(PlayerPrefsNames.SfxVolume, 1));

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

    public float GetMusicVolume()
    {
        return musicVolume_;
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume_ = volume;
        musicSource_.volume = volume;
    }

    public float GetSfxVolume()
    {
        return sfxVolume_;
    }

    public void SetSfxVolume(float volume)
    {
        sfxVolume_ = volume;
    }

    IEnumerator Fade(float from, float to)
    {
        const float Speed = 2.0f;
        float vol = from;
        if (from > to)
        {
            while (vol > to)
            {
                vol -= Time.unscaledDeltaTime * Speed;
                musicSource_.volume = vol;
                yield return null;
            }
        }
        else
        {
            while (vol < to)
            {
                vol += Time.unscaledDeltaTime * Speed;
                musicSource_.volume = vol;
                yield return null;
            }
        }

        musicSource_.volume = to;
    }

    IEnumerator PlayMusicCo(AudioClip clip)
    {
        if (musicIsPlaying_)
        {
            StopFadeCo();
            musicFadeCo_ = Fade(musicSource_.volume, 0.0f);
            yield return musicFadeCo_;
        }

        musicSource_.clip = clip;
        musicSource_.loop = true;
        musicSource_.Play();
        musicIsPlaying_ = true;

        musicSource_.volume = musicVolume_;
    }

    void StopFadeCo()
    {
        if (musicFadeCo_ != null)
        {
            StopCoroutine(musicFadeCo_);
            musicFadeCo_ = null;
        }
    }

    IEnumerator musicFadeCo_;
    public void StopMusic()
    {
        StopFadeCo();
        musicFadeCo_ = Fade(musicSource_.volume, 0.0f);
        StartCoroutine(musicFadeCo_);
    }

    public void PlayMusic(AudioClip clip)
    {
        StartCoroutine(PlayMusicCo(clip));
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
}
