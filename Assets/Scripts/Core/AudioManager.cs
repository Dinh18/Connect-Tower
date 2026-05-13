using System;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [SerializeField] private AudioSource sfxSource;
    private AudioSO[] audios;
    private bool isSoundOn;
    public static event Action<bool> OnToggle;
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
    private void LoadAudioSetting()
    {
        isSoundOn = PlayerPrefs.GetInt("SoundState", 1) == 1;
    }

    public bool ToggleSound()
    {
        isSoundOn = !isSoundOn;
        PlayerPrefs.SetInt("SoundState", isSoundOn ? 1 : 0);
        PlayerPrefs.Save();
        OnToggle?.Invoke(isSoundOn);
        return isSoundOn;
    }

    public bool IsSoundOn()
    {
        return isSoundOn;
    }

}
