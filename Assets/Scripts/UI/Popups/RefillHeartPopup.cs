using UnityEngine;
using UnityEngine.UI;

public class RefillHeartPopup : MonoBehaviour, IMenu
{
    private UIManager uIManager;
    [SerializeField] private Button closeButton;
    [SerializeField] private Button watchVideo;
    [SerializeField] private Button refillButton;
    [SerializeField] private Text heardCountText;
    [SerializeField] private GameObject dimImage;
    // [SerializeField] private Text timeText;
    private MainMenuUIManager mainMenuUIManager;
    private DataManager dataManager;
    void OnEnable()
    {
        closeButton.onClick.AddListener(() => GameEventBus.Publish(new RequestCloseBoosterPopupEvent()));
        watchVideo.onClick.AddListener(OnclickWatchVideo);
        refillButton.onClick.AddListener(OnClickRefillHeart);
        GameEventBus.Subscribe<HeartUpdatedEvent>(UpdateHeardCountText);
    }
    void OnDisable()
    {
        closeButton.onClick.RemoveListener(() => GameEventBus.Publish(new RequestCloseBoosterPopupEvent()));
        watchVideo.onClick.RemoveListener(OnclickWatchVideo);
        refillButton.onClick.RemoveListener(OnClickRefillHeart);
        GameEventBus.UnSubscribe<HeartUpdatedEvent>(UpdateHeardCountText);
    }

    public void Hide()
    {
        this.gameObject.SetActive(false);
        dimImage.SetActive(false);
    }

    public void Setup(UIManager uIManager)
    {
        this.uIManager = uIManager;
        this.dataManager = CoreServices.Get<DataManager>();
    }

    public void Show()
    {
        this.gameObject.SetActive(true);
        dimImage.SetActive(true);
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
            GameEventBus.Publish(new RequestCloseBoosterPopupEvent());
            return;
        }
        if(dataManager.GetTotalCoins() > 900)
        {
            dataManager.AddHeart(5 - dataManager.GetHearts(),"");
            dataManager.UseCoins(900);
            mainMenuUIManager.UpdateCoinText();
            GameEventBus.Publish(new RequestCloseBoosterPopupEvent());
        }
        else
        {
            mainMenuUIManager.OnClickShop();
        }
    }
    public void ConfigMainMenu(MainMenuUIManager mainMenuUIManager)
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

    public GameObject GetGameObject()
    {
        return this.gameObject;
    }
}
