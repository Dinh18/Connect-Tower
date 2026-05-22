
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

public class SettingPopup : Popup
{
    private UIManager uIManager;
    [SerializeField] private Button closeButton;
    // [SerializeField] private Button backHomeButton;
    [SerializeField] private GameObject soundInActive;
    [SerializeField] private GameObject hapticInActive;
    [SerializeField] private Button soundButton;
    [SerializeField] private Button hapticButton;
    private void ClosePopup()
    {
        CoreServices.Get<UIManager>().PopUI();
    }
    void OnEnable()
    {
        soundButton.onClick.AddListener(OnClickSoundButton);
        hapticButton.onClick.AddListener(OnClickHapticButton);
        closeButton.onClick.AddListener(ClosePopup);
    }
    void OnDisable()
    {
        soundButton.onClick.RemoveListener(OnClickSoundButton);
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
        if(HapticManager.Instance.IsHapticOn()) hapticInActive.SetActive(false);
        else hapticInActive.SetActive(true);
    }

    private void OnClickSoundButton()
    {
        if(CoreServices.Get<AudioManager>().ToggleSound()) soundInActive.SetActive(false);
        else soundInActive.SetActive(true);
    }

    private void OnClickHapticButton()
    {
        if(HapticManager.Instance.ToggleHaptic()) hapticInActive.gameObject.SetActive(false);
        else hapticInActive.gameObject.SetActive(true);
    }


}
