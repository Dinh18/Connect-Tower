using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Screen = UnityEngine.Screen;

public class HomePanel : Panel
{
    [SerializeField] private Button playButton;
    [SerializeField] private Text playText;
    [Header("Profile References")]
    [SerializeField] private Button generalStatsButton;
    [SerializeField] private Image avatarImage;
    [SerializeField] private Image frameImage;
    [Header("Coins References")]
    [SerializeField] private Button addCoins;
    [SerializeField] private Text coinText;
    [Header("Setting References")]
    [SerializeField] private Button setting;
    private int oldCoins;
    [Header("Heart References")]
    [SerializeField] private Text heartCountText;
    [SerializeField] private Image heartIcon;
    [SerializeField] private Button addHeartButton;
    [Header("LevelUI References")]
    // [SerializeField] private LevelUIManager levelUIManager;
    private MainMenu mainMenu;

    private bool enableAddHeartButton;

    void OnEnable()
    {
        playButton.onClick.AddListener(OnClickPlay);
        addCoins.onClick.AddListener(OnClicAddCoin);
        addHeartButton.onClick.AddListener(OpenRefillHeart);
        generalStatsButton.onClick.AddListener(() => CoreServices.Get<UIManager>().ShowUI<GeneralStatsPopup>());
        setting.onClick.AddListener(OnClickSetting);

        GameEventBus.Subscribe<HeartUpdatedEvent>(OnHeartUpdated);
        GameEventBus.Subscribe<RequestSaveProfile>(ChangeProfile);
        GameEventBus.Subscribe<CoinsUpdatedEvent>(UpdateCoinText);
    }

    void OnDisable()
    {
        playButton.onClick.RemoveListener(OnClickPlay);
        addCoins.onClick.RemoveListener(OnClicAddCoin);
        addHeartButton.onClick.AddListener(OpenRefillHeart);
        generalStatsButton.onClick.RemoveListener(() => CoreServices.Get<UIManager>().ShowUI<GeneralStatsPopup>());
        setting.onClick.RemoveListener(OnClickSetting);

        GameEventBus.UnSubscribe<HeartUpdatedEvent>(OnHeartUpdated);
        GameEventBus.UnSubscribe<RequestSaveProfile>(ChangeProfile);
        GameEventBus.UnSubscribe<CoinsUpdatedEvent>(UpdateCoinText);
    }

    

    public override void Setup(Menu menu)
    {
        Debug.Log("Current Level: " + CoreServices.Get<DataManager>().GetCurrentLevel());
        if(mainMenu == null) this.mainMenu = menu as MainMenu;
        avatarImage.sprite = CoreServices.Get<DataManager>().GetCurrAvatar().itemSprite;
        frameImage.sprite = CoreServices.Get<DataManager>().GetCurrFrame().itemSprite;
        oldCoins = CoreServices.Get<DataManager>().GetTotalCoins();
        coinText.text = CoreServices.Get<DataManager>().GetTotalCoins().ToString();
        if(CoreServices.Get<DataManager>().GetHearts() < 5) enableAddHeartButton = true;
        else enableAddHeartButton = false;
        StartCoroutine(DelayShowTextLevel());
        StartCoroutine(DelayPlaySadAnim());
    }

    private IEnumerator DelayShowTextLevel()
    {
        yield return new WaitUntil(() => CoreServices.Get<DataManager>().dataReady);
        playText.text = "Level "+(CoreServices.Get<DataManager>().GetCurrentLevel() + 1).ToString();
    }

    private IEnumerator DelayPlaySadAnim()
    {
        yield return new WaitForSeconds(30);
        GameEventBus.Publish(new RequestChangeAnimationNPC{newState = NPCState.Sad});
    }

    private void OnClickPlay()
    {
        Debug.Log("Current Level: " + CoreServices.Get<DataManager>().GetCurrentLevel());

        if(CoreServices.Get<DataManager>().GetHearts() > 0)
        {
            CoreServices.Get<GameManager>().ChangeState(GameManager.GameState.Playing);
        }
        else
        {
            CoreServices.Get<UIManager>().ShowUI<RefillHeartPopup>();
        }
    }

    private void OnClicAddCoin()
    {
        mainMenu.OnShopButtonClicked();
    }

    public void UpdateCoinText(CoinsUpdatedEvent evt)
    {
        coinText.text = evt.totalCoins.ToString();
    }

    private void UpdateHeartCountText(int heart)
    {
        heartCountText.text = heart.ToString();
        if(heart < 5)
        {
            heartIcon.sprite = Resources.Load<Sprite>(Constants.ADD_HEART_ICON);
            enableAddHeartButton = true;
        }
        else
        {
            heartIcon.sprite = Resources.Load<Sprite>(Constants.HEART_ICON);
            enableAddHeartButton = false;
        }
    }

    private void OnHeartUpdated(HeartUpdatedEvent ev)
    {
        UpdateHeartCountText(ev.heartCount);
    }

    public override void Show()
    {
        base.Show();
        oldCoins = CoreServices.Get<DataManager>().GetTotalCoins();
        coinText.text = oldCoins.ToString();
        // StartCoroutine(levelUIManager.Show());
        OnHeartUpdated(new HeartUpdatedEvent { heartCount = CoreServices.Get<DataManager>().GetHearts() });

    }

    private void ChangeProfile(RequestSaveProfile evt)
    {
        frameImage.sprite = CoreServices.Get<DataManager>().GetFrameByID(evt.frameID).itemSprite;
        avatarImage.sprite = CoreServices.Get<DataManager>().GetAvatarByID(evt.avatarID).itemSprite;
    }

    private void OnClickSetting()
    {
        Debug.Log("OnClick Setting");
        CoreServices.Get<UIManager>().ShowUI<SettingPopup>();
    }

    public void OpenRefillHeart()
    {
        if(enableAddHeartButton)
        {
            CoreServices.Get<UIManager>().ShowUI<RefillHeartPopup>();
        }
    }
}
