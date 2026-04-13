
using UnityEngine;
using UnityEngine.UI;

public class SettingPopup : MonoBehaviour, IMenu
{
    private UIManager uIManager;
    [SerializeField] private Button closeButton;
    [SerializeField] private Button backHomeButton;
    // void OnEnable()
    // {
    //     backHomeButton.onClick.AddListener(uIManager.OnClickBackHome);
    //     closeButton.onClick.AddListener(uIManager.CloseSetting);
    // }
    // void OnDisable()
    // {
    //     backHomeButton.onClick.RemoveListener(uIManager.OnClickBackHome);
    //     closeButton.onClick.RemoveListener(uIManager.CloseSetting);
    // }
    public void Hide()
    {
        this.gameObject.SetActive(false);
    }


    public void Setup(UIManager uIManager)
    {
        this.uIManager = uIManager;

        if (backHomeButton != null)
        {
            backHomeButton.onClick.AddListener(uIManager.OnClickBackHome);
        }
        
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(uIManager.CloseSetting);
        }
    }

    public void Show()
    {
        this.gameObject.SetActive(true);

        if(backHomeButton != null)
        {
            bool inGame = uIManager.isCurrentlyInGame();
            backHomeButton.gameObject.SetActive(inGame);
        }
        
    }
}
