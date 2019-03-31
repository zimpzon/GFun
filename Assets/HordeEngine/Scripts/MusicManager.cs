//using System.Collections;
//using UnityEngine;

//public class MusicManagerScript : MonoBehaviour
//{
//    public static MusicManagerScript Instance;
//    public AudioClip GameMusic1Clip;

//    AudioSource audioSource_;
//    bool isPlaying_;
//    float volume_;

//    void Awake()
//    {
//        Instance = this;
//        audioSource_ = GetComponent<AudioSource>();
//    }

//    public void SetVolume(float volume)
//    {
//        volume_ = volume;
//        audioSource_.volume = volume;
//    }

//    IEnumerator Fade(float from, float to)
//    {
//        const float Speed = 1.0f;
//        float vol = from;
//        if (from > to)
//        {
//            while (vol > to)
//            {
//                vol -= Time.unscaledDeltaTime * Speed;
//                audioSource_.volume = vol;
//                yield return null;
//            }
//        }
//        else
//        {
//            while (vol < to)
//            {
//                vol += Time.unscaledDeltaTime * Speed;
//                audioSource_.volume = vol;
//                yield return null;
//            }
//        }

//        audioSource_.volume = to;
//    }

//    IEnumerator Play(AudioClip clip)
//    {
//        if (isPlaying_)
//            yield return Fade(audioSource_.volume, 0.0f);

//        audioSource_.clip = clip;
//        audioSource_.loop = true;
//        audioSource_.Play();
//        isPlaying_ = true;

//        audioSource_.volume = volume_;
//    }

//    public void StopMusic()
//    {
//        StartCoroutine(Fade(audioSource_.volume, 0.0f));
//    }

//    public void PlayIntroMusic()
//    {
//        StartCoroutine(Play(IntroMusicClip));
//    }

//    public void PlayGameMusic(AudioClip clip)
//    {
//        StartCoroutine(Play(clip));
//    }
//}
