
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
    // private PopupAnimation popupAnimation;
    // void OnEnable()
    // {
    //     soundButton.onClick.AddListener(OnClickSoundButton);
    // }
    // void OnDisable()
    // {
    //     soundButton.onClick.RemoveListener(OnClickSoundButton);
        
    // }
    public void Hide()
    {
        // popupAnimation.CloseAnimation(0.3f);
        this.gameObject.SetActive(false);
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
        
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(uIManager.CloseSetting);
        }
        // popupAnimation = this.gameObject.AddComponent<PopupAnimation>();
    }

    public void Show()
    {
        this.gameObject.SetActive(true);

        if(backHomeButton != null)
        {
            bool inGame = uIManager.isCurrentlyInGame();
            backHomeButton.gameObject.SetActive(inGame);
        }

        if(AudioManager.Instance.IsSoundOn()) soundInActive.SetActive(false); 
        if(HapticManager.Instance.IsHapticOn()) hapticInActive.gameObject.SetActive(false);
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
