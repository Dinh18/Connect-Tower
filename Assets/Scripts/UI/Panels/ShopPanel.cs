using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopPanel : Panel
{
    [SerializeField] private Button closeButton;
    [SerializeField] private TextMeshProUGUI coinCountText;
    private MainMenu mainMenu;
    void OnEnable()
    {
        closeButton.onClick.AddListener(OnClickClose);
        GameEventBus.Subscribe<CoinsUpdatedEvent>(UpdateCoinText);
    }
    void OnDisable()
    {
        closeButton.onClick.RemoveListener(OnClickClose);
        GameEventBus.UnSubscribe<CoinsUpdatedEvent>(UpdateCoinText);
    }
    public override void Setup(Menu menu)
    {
        this.mainMenu = menu as MainMenu;
        ShowCloseButton(mainMenu == null);

        // Tự động tắt LayoutElement nếu là popup standalone để tránh ảnh hưởng kích thước
        var layoutElement = GetComponent<LayoutElement>();
        if (layoutElement != null)
        {
            layoutElement.enabled = (mainMenu != null);
        }
    }
    public override void Hide()
    {
        base.Hide();
        this.gameObject.SetActive(false);
    }

    public override void Show()
    {
        base.Show();
        this.gameObject.SetActive(true);    
        coinCountText.text = CoreServices.Get<DataManager>().GetTotalCoins().ToString();
    }

    public void ShowCloseButton(bool show)
    {
        closeButton.gameObject.SetActive(show);
    }
    public void UpdateCoinText(CoinsUpdatedEvent evt)
    {
        coinCountText.text = evt.totalCoins.ToString();
    }
    public void OnClickClose()
    {
        if (mainMenu != null)
        {
            mainMenu.OnHomeButtonClicked();
        }
        else
        {
            CoreServices.Get<UIManager>().CloseShop();
        }
    }
}
