using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using KToolkit;


public class AudioManager : KSingletonNoMono<AudioManager>
{
    private GameObject audioManager;
    private const int effectSourcePoolSize = 8;
    //AudioSource[] effectSourcePool = new AudioSource[effectSourcePoolSize];
    private List<AudioSource> effectSourcePool = new List<AudioSource>();
    private AudioSource musicSource;

    public AudioManager()
    {
        audioManager = GameObject.Find("AudioManager");
        if (audioManager == null)
        {
            audioManager = new GameObject("AudioManager");
        }
        Object.DontDestroyOnLoad(audioManager);
        musicSource = audioManager.AddComponent<AudioSource>();
        musicSource.loop = true;
        // Debug.Log(gameObject);
        for (int i = 0; i < effectSourcePoolSize; ++i)
        {
            effectSourcePool.Add(audioManager.AddComponent<AudioSource>());
            // effectSourcePool[i] = gameObject.AddComponent<AudioSource>();
        }
    }

    public void PlayBGM(string path = "", bool isLoop = true)
    {
        if(path == "")
        {
            musicSource.Play();
            return;
        }
        SetAudioSource(musicSource, path, isLoop);
        musicSource.Play();
    }

    public void PlayEffectAudio(string path)
    {
        foreach(var item in effectSourcePool)
        {
            if(item.isPlaying == false)
            {
                SetAudioSource(item, path);
                item.Play();
                break;
            }
        }
    }

    public void StopBGM()
    {
        musicSource.Stop();
    }

    public void StopBGMFadeOut(float fadeTime=2f)
    {
        if(musicSource.isPlaying == false)
        {
            return;
        }
        AudioFadeCoroutine(musicSource, 0f, fadeTime);
    }
    
    private IEnumerator AudioFadeCoroutine(AudioSource source, float targetVolume, float fadeOutTime)
    {
        float fadeSpeed = (source.volume - targetVolume) / fadeOutTime;
        while (source.volume > targetVolume)
        {
            source.volume -= fadeSpeed * Time.deltaTime;
            yield return null;
        }
    }

    public void PauseBGM()
    {
        musicSource.Pause();
    }

    public void ResumeBGM()
    {
        musicSource.UnPause();
    }

    public void SetAudioSource(AudioSource source, string path = "", bool isLoop = false, float volume = 0.25f)
    {
        var audio = Resources.Load<AudioClip>(path);
        if(audio == null)
        {
            Debug.LogError("Resources目录的对应路径下没有音效可以载入，请检查路径！");
            return;
        }
        source.clip = Resources.Load<AudioClip>(path);
        source.loop = isLoop;
        source.volume = volume;
    }
}
