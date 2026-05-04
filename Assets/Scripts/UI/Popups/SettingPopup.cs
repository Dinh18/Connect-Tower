
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

public class SettingPopup : MonoBehaviour, IMenu
{
    private UIManager uIManager;
    [SerializeField] private Button closeButton;
    [SerializeField] private Button backHomeButton;
    [SerializeField] private GameObject soundInActive;
    [SerializeField] private GameObject hapticInActive;
    [SerializeField] private Button soundButton;
    [SerializeField] private Button hapticButton;
    [SerializeField] private GameObject dimImage;
    private PopupAnimation popupAnimation;
    void OnEnable()
    {
        soundButton.onClick.AddListener(OnClickSoundButton);
        closeButton.onClick.AddListener(() => GameEventBus.Publish(new RequestCloseBoosterPopupEvent()));
    }
    void OnDisable()
    {
        soundButton.onClick.RemoveListener(OnClickSoundButton);
        closeButton.onClick.RemoveListener(() => GameEventBus.Publish(new RequestCloseBoosterPopupEvent()));
    }
    public void Hide()
    {
        // popupAnimation.CloseAnimation(0.3f);
        this.gameObject.SetActive(false);
        dimImage.SetActive(false);
    }


    public void Setup(UIManager uIManager)
    {
        this.uIManager = uIManager;

        soundButton.onClick.AddListener(OnClickSoundButton);
        hapticButton.onClick.AddListener(OnClickHapticButton);

        if (backHomeButton != null)
        {
            backHomeButton.onClick.AddListener(uIManager.OnClickBackHome);
        }
        
    }

    public void Show()
    {
        this.gameObject.SetActive(true);
        dimImage.SetActive(true);

        if(backHomeButton != null)
        {
            bool inGame = uIManager.isCurrentlyInGame();
            backHomeButton.gameObject.SetActive(inGame);
        }

        if(AudioManager.Instance.IsSoundOn()) soundInActive.SetActive(false);
        else soundInActive.SetActive(true);
        if(HapticManager.Instance.IsHapticOn()) hapticInActive.SetActive(false);
        else hapticInActive.SetActive(true);

    }

    private void OnClickSoundButton()
    {
        if(AudioManager.Instance.ToggleSound()) soundInActive.SetActive(false);
        else soundInActive.SetActive(true);
    }

    private void OnClickHapticButton()
    {
        if(HapticManager.Instance.ToggleHaptic()) hapticInActive.gameObject.SetActive(false);
        else hapticInActive.gameObject.SetActive(true);
    }

    public GameObject GetGameObject()
    {
        return this.gameObject;
    }
}
