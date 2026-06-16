
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RefillHeartPopup : Popup
{
    [SerializeField] private Button closeButton;
    [SerializeField] private Button watchVideo;
    [SerializeField] private Button refillButton;
    [SerializeField] private TextMeshProUGUI heardCountText;

    private void ClosePopup()
    {
        CoreServices.Get<UIManager>().PopUI();
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
        
    }

    public override void Show()
    {
        // this.gameObject.SetActive(true);
        base.Show();
        UpdateHeardCountText(new HeartUpdatedEvent { heartCount = CoreServices.Get<DataManager>().GetHearts() });
    }
    private void OnclickWatchVideo()
    {
        CoreServices.Get<DataManager>().AddHeart(1,CoreServices.Get<DataManager>().GetNextHeartTime());
    }
    private void OnClickRefillHeart()
    {
        if(CoreServices.Get<DataManager>().GetHearts() >= 5)
        {
            CoreServices.Get<UIManager>().PopUI();
            return;
        }
        if(CoreServices.Get<DataManager>().GetTotalCoins() > 900)
        {
            CoreServices.Get<DataManager>().AddHeart(5 - CoreServices.Get<DataManager>().GetHearts(),"");
            CoreServices.Get<DataManager>().UseCoins(900);
            CoreServices.Get<UIManager>().PopUI();
        }
        else
        {
            // mainMenuUIManager.OnClickShop();
            CoreServices.Get<UIManager>().OpenShop();
        }
    }
    private void UpdateHeardCountText(HeartUpdatedEvent heartUpdated)
    {
        heardCountText.text = heartUpdated.heartCount.ToString();
    }

}
