using UnityEngine;
using UnityEngine.UI;

public class RefillHeartPopup : MonoBehaviour, IMenu
{
    private UIManager uIManager;
    [SerializeField] private Button closeButton;
    [SerializeField] private Button watchVideo;
    [SerializeField] private Button refillButton;
    void OnEnable()
    {
        closeButton.onClick.AddListener(Hide);
        watchVideo.onClick.AddListener(OnclickWatchVideo);
        refillButton.onClick.AddListener(OnClickRefillHeart);
    }
    void OnDisable()
    {
        closeButton.onClick.RemoveListener(Hide);
        watchVideo.onClick.RemoveListener(OnclickWatchVideo);
        refillButton.onClick.RemoveListener(OnClickRefillHeart);
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
        }
    }
}
