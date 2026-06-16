using System;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource bgSource;
    [SerializeField] private AudioSource mixSource;
    [SerializeField]private AudioSO[] audios;
    [SerializeField] private AudioClip tingClip;
    [SerializeField] private AudioClip ingameBGClip;
    [SerializeField] private AudioClip mainMenuBGClip;

    private bool isSoundOn;
    private bool isMusicOn;
    public static event Action<bool> OnToggle;
    public static event Action<bool> OnMusicToggle;

    public void Init()
    {
        LoadAudioSetting();
        audios = Resources.LoadAll<AudioSO>(Constants.AUDIOS_PATH);
    }
    void OnEnable()
    {
        GameEventBus.Subscribe<RequestPlaySFX>(PlaySFX);
    }
    void OnDisable()
    {
        GameEventBus.UnSubscribe<RequestPlaySFX>(PlaySFX);
    }
    public AudioSO GetAudioData(SoundID soundID)
    {
        foreach(var audio in audios)
        {
            if(audio.soundID == soundID) return audio;
        }
        return null;
    }
    public void PlaySFX(RequestPlaySFX evt)
    {
        if (!isSoundOn || evt.soundID == SoundID.None) return;

        AudioSO data = GetAudioData(evt.soundID);
        if (data != null && data.audioClip != null)
        {
            // Tạm thời dùng PlayOneShot, nhưng nên thay bằng việc lấy AudioSource từ Pool
            sfxSource.PlayOneShot(data.audioClip);
        }
        else
        {
            Debug.LogWarning($"[AudioManager] Không tìm thấy audio clip cho: {evt.soundID}");
        }
    }
    public void PlayTingSFX(int index)
    {
        if(!isSoundOn) return;
        mixSource.clip = tingClip;
        mixSource.pitch = 1 + index*0.05f; // Tăng pitch nhẹ cho mỗi lần gọi để tạo cảm giác "ting" khác nhau
        mixSource.Play();
    }
    private void LoadAudioSetting()
    {
        isSoundOn = PlayerPrefs.GetInt("SoundState", 1) == 1;
        isMusicOn = PlayerPrefs.GetInt("MusicState", 1) == 1;
        if (bgSource != null)
        {
            bgSource.mute = !isMusicOn;
        }
    }

    public bool ToggleSound()
    {
        isSoundOn = !isSoundOn;
        PlayerPrefs.SetInt("SoundState", isSoundOn ? 1 : 0);
        PlayerPrefs.Save();
        OnToggle?.Invoke(isSoundOn);
        return isSoundOn;
    }

    public void PlayInGameBG()
    {
        if (bgSource != null)
        {
            bgSource.clip = ingameBGClip;
            if (isMusicOn)
            {
                bgSource.Play();
                bgSource.loop = true;
            }
        }
    }

    public void PlayMainMenuBG()
    {
        if (bgSource != null)
        {
            bgSource.clip = mainMenuBGClip;
            if (isMusicOn)
            {
                bgSource.Play();
                bgSource.loop = true;
            }
        }
    }

    public void StopBG()
    {
        if (bgSource != null)
        {
            bgSource.Stop();
        }
    }

    public bool IsSoundOn()
    {
        return isSoundOn;
    }

    public bool ToggleMusic()
    {
        isMusicOn = !isMusicOn;
        PlayerPrefs.SetInt("MusicState", isMusicOn ? 1 : 0);
        PlayerPrefs.Save();
        if (bgSource != null)
        {
            bgSource.mute = !isMusicOn;
        }
        OnMusicToggle?.Invoke(isMusicOn);
        return isMusicOn;
    }

    public bool IsMusicOn()
    {
        return isMusicOn;
    }
}
