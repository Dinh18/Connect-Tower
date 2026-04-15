using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip addMoveAudio;
    [SerializeField] private AudioClip buttonAudio;
    [SerializeField] private AudioClip blockFailAudio;
    [SerializeField] private AudioClip clothAudio;
    [SerializeField] private AudioClip freezeUpAudio;
    [SerializeField] private AudioClip hintBoosterAudio;
    [SerializeField] private AudioClip blockIceFinishedAudio;
    [SerializeField] private AudioClip lvlWinAudio;
    [SerializeField] private AudioClip lvlLoseAudio;
    [SerializeField] private AudioClip coinCollectAudio;
    [SerializeField] private AudioClip moveWooshAudio;
    [SerializeField] private AudioClip PopMoved_Audio_1;
    [SerializeField] private AudioClip PopMoved_Audio_2;
    [SerializeField] private AudioClip PopMoved_Audio_3;
    [SerializeField] private AudioClip PopMoved_Audio_4;
    [SerializeField] private AudioClip slotFinishedAudio;
    [SerializeField] private AudioClip shuffleAudio;
    [SerializeField] private AudioClip hideBlockAudio;
    [SerializeField] private AudioClip addBoosterAudio;
    private bool isSoundOn;
    void Awake()
    {
        Instance = this;
        LoadAudioSetting();
    }
    public void PlaySFX(AudioClip audioClip)
    {
        if(audioClip == null) return;
        audioSource.volume = isSoundOn ? 1 : 0;
        audioSource.PlayOneShot(audioClip);
    }

    public void PlayAddMoveAudio() => PlaySFX(addMoveAudio);
    public void PlayButtonAudio() => PlaySFX(buttonAudio);
    public void PlayBlockFailAudio() => PlaySFX(blockFailAudio);
    public void PlayClothAudio() => PlaySFX(clothAudio);
    public void PlayFreezeUpAudio() => PlaySFX(freezeUpAudio);
    public void PlayHintBoosterAudio() => PlaySFX(hintBoosterAudio);
    public void PlayBlockIceFinishedAudio() => PlaySFX(blockIceFinishedAudio);
    public void PlayLVLWinAudio() => PlaySFX(lvlWinAudio);
    public void PlayLVLLoseAudio() => PlaySFX(lvlLoseAudio);
    public void PlayCoinCollectAudio() => PlaySFX(coinCollectAudio);
    public void PlayMoveWooshAudio() => PlaySFX(moveWooshAudio);
    public void PlaySlotFinishedAudio() => PlaySFX(slotFinishedAudio);
    public void PlayShuffleAudio() => PlaySFX(shuffleAudio);
    public void PlayHideBlockAudio() => PlaySFX(hideBlockAudio);
    public void PlayAddBoosterAudio() => PlaySFX(addBoosterAudio);
    public void PlayPopMovedAudio(int count)
    {
        switch(count)
        {
            case 1:
                PlaySFX(PopMoved_Audio_1);
                break;
            case 2:
                PlaySFX(PopMoved_Audio_2);
                break;
            case 3:
                PlaySFX(PopMoved_Audio_3);
                break;
            case 4:
                PlaySFX(PopMoved_Audio_4);
                break;
            
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
        return isSoundOn;
    }

    public bool IsSoundOn()
    {
        return isSoundOn;
    }

}
