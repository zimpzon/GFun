using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public AudioClips AudioClips;
    public int SfxSourceCount = 20;
    public bool ShowDebugOutput = false;

    AudioSource musicSource_;
    AudioSource ambienceSource_;
    List<AudioSource> sfxSources_ = new List<AudioSource>();

    bool debugIsShown_;
    bool musicIsPlaying_;
    float musicVolume_ = 1;
    float sfxVolume_ = 1;

    void Awake()
    {
        musicSource_ = gameObject.AddComponent<AudioSource>();
        ambienceSource_ = gameObject.AddComponent<AudioSource>();

        for (int i = 0; i < SfxSourceCount; ++i)
        {
            var sfxSource = gameObject.AddComponent<AudioSource>();
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
        return replaceIfNoneAvailable ? sfxSources_[Random.Range(0, sfxSources_.Count)] : null;
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
            selectedSource.pitch = 1.0f + (pitchRandomVariation * Random.value - pitchRandomVariation * 0.5f);
            selectedSource.Play();
        }
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume_ = volume;
        musicSource_.volume = volume;
    }

    public void SetSfxVolume(float volume)
    {
        sfxVolume_ = volume;
    }

    IEnumerator Fade(float from, float to)
    {
        const float Speed = 1.0f;
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
            yield return Fade(musicSource_.volume, 0.0f);

        musicSource_.clip = clip;
        musicSource_.loop = true;
        musicSource_.Play();
        musicIsPlaying_ = true;

        musicSource_.volume = musicVolume_;
    }

    public void StopMusic()
    {
        StartCoroutine(Fade(musicSource_.volume, 0.0f));
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
