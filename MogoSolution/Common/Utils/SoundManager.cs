using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Mogo.Util;
using Mogo.GameData;

public class SoundManagerExEvent
{
    /// <summary>
    /// 点击按钮音效
    /// </summary>
    public readonly static string PlayButtonSound = "Click_Button";
}

public class SoundManager
{

    #region 变量

    private static float soundVolume = 0.5f;
    public static float SoundVolume
    {
        get { return soundVolume; }
        set
        {
            soundVolume = value > 0.95 ? 1 : (value < 0.05) ? 0 : value;
            //EventDispatcher.TriggerEvent<float>(SoundManagerExEvent.SoundVolumeChanged, value);
        }
    }

    public static Dictionary<string, AudioClip> audioClipBuffer = new Dictionary<string, AudioClip>();

    protected static AudioListener listener;
    protected static string curButtonSoundId = "";
    protected static AudioSource defaultSoundSource;
    protected static AudioSource defaultMusicSource;
    public static string defaultSoundSourceName = "SoundSource";
    public static string defaultMusicSourceName = "MusicSource";

    #endregion

    #region 初始化

    static SoundManager()
    {
    }

    public static void Init()
    {
        defaultSoundSource = GameObject.Find("Driver").transform.FindChild(defaultSoundSourceName).gameObject.GetComponent<AudioSource>();
        defaultMusicSource = GameObject.Find("Driver").transform.FindChild(defaultMusicSourceName).gameObject.GetComponent<AudioSource>();
        
        audioClipBuffer = new Dictionary<string, AudioClip>();

        AddListeners();
    }

    public static void AddListeners()
    {
        EventDispatcher.AddEventListener<ButtonClickSoundType>(SoundManagerExEvent.PlayButtonSound, PlayButtonSound);
    }

    public static void RemoveListeners()
    {
        EventDispatcher.RemoveEventListener<ButtonClickSoundType>(SoundManagerExEvent.PlayButtonSound, PlayButtonSound);
    }

    #endregion

    #region 加载和卸载声音

    public static void LoadAudioClip(string soundPath, AudioSource source, bool isLoop, Action<AudioSource, UnityEngine.Object, bool> action = null)
    {
        if (audioClipBuffer.ContainsKey(soundPath))
        {
            if (action != null)
                action(source, audioClipBuffer[soundPath], isLoop);
            return;
        }

        AssetCacheMgr.GetResourceAutoRelease(soundPath, (obj) =>
        {
            UnityEngine.Object.DontDestroyOnLoad(obj);
            if (action != null)
                action(source, obj, isLoop);

            if (obj is AudioClip && !audioClipBuffer.ContainsKey(soundPath))
                audioClipBuffer.Add(soundPath, obj as AudioClip);
        });
    }

    public static void LoadAudioClip(string soundPath, Action<UnityEngine.Object> action = null)
    {
        if (audioClipBuffer.ContainsKey(soundPath))
        {
            if (action != null)
                action(audioClipBuffer[soundPath]);
            return;
        }

        AssetCacheMgr.GetResourceAutoRelease(soundPath, (obj) =>
        {
            UnityEngine.Object.DontDestroyOnLoad(obj);
            if (action != null)
                action(obj);

            if (obj is AudioClip && !audioClipBuffer.ContainsKey(soundPath))
                audioClipBuffer.Add(soundPath, obj as AudioClip);
        });
    }

    #endregion

    #region 播放

    public static void PlaySound(string soundPath)
    {
        LoadAudioClip(soundPath, PlaySoundByObject);
    }

    public static void GameObjectPlaySound(GameObject go, string soundPath, bool isLoop = false)
    {
        if (!go)
            return;
        AudioSource gameObjectAudioSource = go.GetComponent<AudioSource>();
        if (gameObjectAudioSource == null)
            gameObjectAudioSource = go.AddComponent<AudioSource>();
        else if (gameObjectAudioSource.isPlaying)
            gameObjectAudioSource.Stop();

        PlaySoundOnSourceByID(soundPath, gameObjectAudioSource, isLoop);
    }

    public static void PlaySoundOnSourceByID(string soundPath, AudioSource gameObjectAudioSource, bool isLoop = false)
    {
        LoadAudioClip(soundPath, gameObjectAudioSource, isLoop, PlaySoundOnSourceByObject);
    }

    public static void PlaySoundOnSourceByObject(AudioSource gameObjectAudioSource, UnityEngine.Object clipObject, bool isLoop = false)
    {
        if (gameObjectAudioSource == null)
        {
            return;
        }

        if (clipObject is AudioClip)
        {
            gameObjectAudioSource.clip = clipObject as AudioClip;
            gameObjectAudioSource.volume = SoundVolume;
            gameObjectAudioSource.loop = isLoop;
            gameObjectAudioSource.Play();
            return;
        }

        var clip = (clipObject as GameObject).GetComponent<AudioSource>().clip;
        if (clip != null)
        {
            gameObjectAudioSource.clip = clip;
            gameObjectAudioSource.volume = SoundVolume;
            gameObjectAudioSource.loop = isLoop;
            gameObjectAudioSource.Play();
        }
    }

    public static void StopPlaySound(GameObject go, string soundPath)
    {
        AudioSource gameObjectAudioSource = go.GetComponent<AudioSource>();
        if (gameObjectAudioSource == null)
            return;
        else if (gameObjectAudioSource.isPlaying)
            gameObjectAudioSource.Stop();
    }

    #endregion

    #region 控件音效

    public static void PlayButtonSound(ButtonClickSoundType buttonType)
    {
        string soundPath = "";
        switch (buttonType)
        {
            case ButtonClickSoundType.Weak://弱
                soundPath = "ui_select_weak.mp3";
                break;
            case ButtonClickSoundType.Middle://中
                soundPath = "ui_select_middle.mp3";
                break;
            case ButtonClickSoundType.Strong://强
                soundPath = "ui_select_strong.mp3";
                break;
            case ButtonClickSoundType.Open://打开
                soundPath = "ui_window_open.mp3";
                break;
            case ButtonClickSoundType.Close://关闭
                soundPath = "ui_window_close.mp3";
                break;
        }
        if (soundPath == "") return;
        if (curButtonSoundId == soundPath && defaultSoundSource.isPlaying) return;
        curButtonSoundId = soundPath;
        LoadAudioClip(soundPath, PlaySoundByObject);
    }

    public static void PlaySoundByObject(UnityEngine.Object clipObject)
    {
        if (clipObject == null)
        {
            LoggerHelper.Error("animation clip Object is null!");
            return;
        }
        if (clipObject is AudioClip)
        {
            defaultSoundSource = MyPlaySound(defaultSoundSource, defaultSoundSourceName, clipObject as AudioClip, soundVolume);
            return;
        }

        var clip = (clipObject as GameObject).GetComponent<AudioSource>().clip;
        if (clip != null)
            defaultSoundSource = MyPlaySound(defaultSoundSource, defaultSoundSourceName, clip, soundVolume);
    }

    #endregion

    #region 播放全局声音

    static public AudioSource MyPlaySound(AudioSource defaultSource, string sourceName, AudioClip clip) { return MyPlaySound(defaultSource, sourceName, clip, 1f, 1f); }

    public static AudioSource MyPlaySound(AudioSource defaultSource, string sourceName, AudioClip clip, float volume) { return MyPlaySound(defaultSource, sourceName, clip, volume, 1f); }

    public static AudioSource MyPlaySound(AudioSource defaultSource, string sourceName, AudioClip clip, float volume, float pitch)
    {
        if (clip != null)
        {
            if (listener == null)
            {
                listener = GameObject.Find("Driver").GetComponent<AudioListener>();

                if (listener == null)
                    listener = GameObject.Find("Driver").AddComponent<AudioListener>();
            }

            if (listener != null)
            {
                AudioSource source = defaultSource;
                if (source == null)
                {
                    defaultSource = GameObject.Find("Driver").transform.FindChild(sourceName).gameObject.AddComponent<AudioSource>();
                    source = defaultSource;
                }

                source.volume = volume;
                source.pitch = pitch;
                source.PlayOneShot(clip);
                return source;
            }
        }
        return null;
    }

    #endregion
}
