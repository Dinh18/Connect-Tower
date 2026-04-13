using UnityEngine;
using UnityEngine.UI;

public class RefillHeartPopup : MonoBehaviour, IMenu
{
    private UIManager uIManager;
    [SerializeField] private Button closeButton;
    [SerializeField] private Button watchVideo;
    [SerializeField] private Button refillButton;
    [SerializeField] private Text heardCountText;
    // [SerializeField] private Text timeText;
    private MainMenuUIManager mainMenuUIManager;
    void OnEnable()
    {
        closeButton.onClick.AddListener(OnclickClose);
        watchVideo.onClick.AddListener(OnclickWatchVideo);
        refillButton.onClick.AddListener(OnClickRefillHeart);
        DataManager.OnChangeHeart+=UpdateHeardCountText;
    }
    void OnDisable()
    {
        closeButton.onClick.RemoveListener(OnclickClose);
        watchVideo.onClick.RemoveListener(OnclickWatchVideo);
        refillButton.onClick.RemoveListener(OnClickRefillHeart);
        DataManager.OnChangeHeart-=UpdateHeardCountText;
    }
    public void Hide()
    {
        this.gameObject.SetActive(false);
    }

    public void Setup(UIManager uIManager)
    {
        this.uIManager = uIManager;
    }

    public void Show()
    {
        this.gameObject.SetActive(true);
        UpdateHeardCountText(DataManager.Instance.playerData.heart);
    }
    private void OnclickWatchVideo()
    {
        DataManager.Instance.AddHeart(1,DataManager.Instance.playerData.nextHeartTime);
    }
    private void OnClickRefillHeart()
    {
        if(DataManager.Instance.playerData.totalCoins > 900)
        {
            DataManager.Instance.AddHeart(5 - DataManager.Instance.playerData.heart,"");
            DataManager.Instance.UseCoins(900);
            OnclickClose();
        }
        else
        {
            mainMenuUIManager.OnClickShop();
        }
    }
    private void OnclickClose()
    {
        uIManager.PopPopup();
    }
    public void ConfigMainMenu(MainMenuUIManager mainMenuUIManager)
    {
        if(this.mainMenuUIManager == null)
        {
            this.mainMenuUIManager = mainMenuUIManager; 
        }
    }
    private void UpdateHeardCountText(int amount)
    {
        heardCountText.text = amount.ToString();
    }
}
