using UnityEngine;
using UnityEngine.UI;

public class RefillHeartPopup : Popup
{
    private UIManager uIManager;
    [SerializeField] private Button closeButton;
    [SerializeField] private Button watchVideo;
    [SerializeField] private Button refillButton;
    [SerializeField] private Text heardCountText;
    private MainMenuUIPanel mainMenuUIManager;
    private DataManager dataManager;

    private void ClosePopup()
    {
        GameEventBus.Publish(new RequestClosePopupEvent());
    }
    void OnEnable()
    {
        closeButton.onClick.AddListener(ClosePopup);
        watchVideo.onClick.AddListener(OnclickWatchVideo);
        refillButton.onClick.AddListener(OnClickRefillHeart);
        GameEventBus.Subscribe<HeartUpdatedEvent>(UpdateHeardCountText);
    }
    void OnDisable()
    {
        closeButton.onClick.RemoveListener(ClosePopup);
        watchVideo.onClick.RemoveListener(OnclickWatchVideo);
        refillButton.onClick.RemoveListener(OnClickRefillHeart);
        GameEventBus.UnSubscribe<HeartUpdatedEvent>(UpdateHeardCountText);
    }

    public override void Hide()
    {
        // this.gameObject.SetActive(false);
        base.Hide();
    }

    public void Setup(UIManager uIManager)
    {
        this.uIManager = uIManager;
        this.dataManager = CoreServices.Get<DataManager>();
    }

    public override void Show()
    {
        // this.gameObject.SetActive(true);
        base.Show();
        UpdateHeardCountText(new HeartUpdatedEvent { heartCount = dataManager.GetHearts() });
    }
    private void OnclickWatchVideo()
    {
        dataManager.AddHeart(1,dataManager.GetNextHeartTime());
    }
    private void OnClickRefillHeart()
    {
        if(dataManager.GetHearts() >= 5)
        {
            GameEventBus.Publish(new RequestClosePopupEvent());
            return;
        }
        if(dataManager.GetTotalCoins() > 900)
        {
            dataManager.AddHeart(5 - dataManager.GetHearts(),"");
            dataManager.UseCoins(900);
            mainMenuUIManager.UpdateCoinText();
            GameEventBus.Publish(new RequestClosePopupEvent());
        }
        else
        {
            mainMenuUIManager.OnClickShop();
        }
    }
    public void ConfigMainMenu(MainMenuUIPanel mainMenuUIManager)
    {
        if(this.mainMenuUIManager == null)
        {
            this.mainMenuUIManager = mainMenuUIManager; 
        }

        // refillButton.onClick.RemoveAllListeners();
        // refillButton.onClick.AddListener(OnClickRefillHeart);
    }
    private void UpdateHeardCountText(HeartUpdatedEvent heartUpdated)
    {
        heardCountText.text = heartUpdated.heartCount.ToString();
    }

}
