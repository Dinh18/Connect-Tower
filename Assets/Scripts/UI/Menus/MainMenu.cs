using System.Collections;
using DG.Tweening;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class MainMenu : Menu
{
    enum MainMenuPanel
    {
        Shop = 0,
        Home = 1,
        LeaderBoard = 2,
    }
    [Header("Button References")]
    [SerializeField] private Button homeButton;
    [SerializeField] private Button shopButton;
    [SerializeField] private Button cupButton;
    [Header("Animation Setting")]
    private BackgroundButton shopBackGround;
    private BackgroundButton homeBackGround;
    private BackgroundButton cupBackGround;
    [SerializeField] private SlidingMenuManager slidingMenu;
    [SerializeField] private Panel[] panels;
    [SerializeField] private GameObject bottomHolder;

    void Awake()
    {
        shopBackGround = shopButton.GetComponent<BackgroundButton>();
        homeBackGround = homeButton.GetComponent<BackgroundButton>();
        cupBackGround = cupButton.GetComponent<BackgroundButton>();
    }

    void OnEnable()
    {
        homeButton.onClick.AddListener(OnHomeButtonClicked);
        shopButton.onClick.AddListener(OnShopButtonClicked);
        cupButton.onClick.AddListener(OnCupButtonClicked);
    }

    void OnDisable()
    {
        homeButton.onClick.RemoveListener(OnHomeButtonClicked);
        shopButton.onClick.RemoveListener(OnShopButtonClicked);
        cupButton.onClick.RemoveListener(OnCupButtonClicked);
    }

    private void ShowPanel(MainMenuPanel panel)
    {
        shopBackGround.UnSelect();
        homeBackGround.UnSelect();
        cupBackGround.UnSelect();
        switch (panel)
        {
            case MainMenuPanel.Home:
                homeBackGround.Select();
                break;
            case MainMenuPanel.Shop:
                shopBackGround.Select();
                break;
            case MainMenuPanel.LeaderBoard:
                cupBackGround.Select();
                break;
        }

        slidingMenu.GoToTab((int) panel);
    }

    private Panel GetPanel<T>() where T : Panel
    {
        foreach(var panel in panels)
        {
            if(panel.GetType() == typeof(T)) return panel as T;
        }
        return null;
    }

    public void OnHomeButtonClicked() => ShowPanel(MainMenuPanel.Home);
    public void OnShopButtonClicked() => ShowPanel(MainMenuPanel.Shop);
    public void OnCupButtonClicked() => ShowPanel(MainMenuPanel.LeaderBoard);

    public override void Hide()
    {
        this.gameObject.SetActive(false);
    }

    public override void Show()
    {
        this.gameObject.SetActive(true);
        bottomHolder.SetActive(true);

        foreach (var panel in panels)
        {
            panel.Show();
            panel.Setup(this);
        }   

        ShowPanel(MainMenuPanel.Home);
    }

    public void GoToShop()
    {
        bottomHolder.SetActive(true);
        ShowPanel(MainMenuPanel.Shop);
        ShopPanel shopPanel = GetPanel<ShopPanel>() as ShopPanel;
        if (shopPanel != null)
        {
            shopPanel.ShowCloseButton(false);
        }
        this.gameObject.SetActive(true);
    }
}
