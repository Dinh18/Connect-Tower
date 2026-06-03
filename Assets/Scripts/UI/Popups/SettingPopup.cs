
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

public class SettingPopup : Popup
{
    private UIManager uIManager;
    [SerializeField] private Button closeButton;
    // [SerializeField] private Button backHomeButton;
    [SerializeField] private GameObject soundInActive;
    [SerializeField] private GameObject musicInActive;
    [SerializeField] private GameObject hapticInActive;
    [SerializeField] private Button soundButton;
    [SerializeField] private Button musicButton;
    [SerializeField] private Button hapticButton;
    private void ClosePopup()
    {
        CoreServices.Get<UIManager>().PopUI();
    }
    void OnEnable()
    {
        soundButton.onClick.AddListener(OnClickSoundButton);
        musicButton.onClick.AddListener(OnClickMusicButton);
        hapticButton.onClick.AddListener(OnClickHapticButton);
        closeButton.onClick.AddListener(ClosePopup);
    }
    void OnDisable()
    {
        soundButton.onClick.RemoveListener(OnClickSoundButton);
        musicButton.onClick.RemoveListener(OnClickMusicButton);
        hapticButton.onClick.RemoveListener(OnClickHapticButton);
        closeButton.onClick.RemoveListener(ClosePopup);
    }
    public override void Hide()
    {
        base.Hide();
    }


    public void Setup(UIManager uIManager)
    {
        this.uIManager = uIManager;
    }

    public override void Show()
    {
        base.Show();

        if(CoreServices.Get<AudioManager>().IsSoundOn()) soundInActive.SetActive(false);
        else soundInActive.SetActive(true);
        if(CoreServices.Get<AudioManager>().IsMusicOn()) musicInActive.SetActive(false);
        else musicInActive.SetActive(true);
        if(CoreServices.Get<HapticManager>().IsHapticOn()) hapticInActive.SetActive(false);
        else hapticInActive.SetActive(true);
    }

    private void OnClickSoundButton()
    {
        if(CoreServices.Get<AudioManager>().ToggleSound()) soundInActive.SetActive(false);
        else soundInActive.SetActive(true);
    }

    private void OnClickMusicButton()
    {
        if(CoreServices.Get<AudioManager>().ToggleMusic()) musicInActive.SetActive(false);
        else musicInActive.SetActive(true);
    }
    private void OnClickHapticButton()
    {
        if(CoreServices.Get<HapticManager>().ToggleHaptic()) hapticInActive.gameObject.SetActive(false);
        else hapticInActive.gameObject.SetActive(true);
    }



}
