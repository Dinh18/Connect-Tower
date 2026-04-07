using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuUIManager : MonoBehaviour, IMenu
{
    private UIManager uIManager;
    [Header("Button References")]
    [SerializeField] private Button AddCoins;
    [SerializeField] private Button Setting;
    [SerializeField] private Button playButton;
    [SerializeField] private Button homeButton;
    [SerializeField] private Button shopButton;
    [Header("Coins Text Setting")]
    [SerializeField] private Text coinText;
    [SerializeField] private float countDuration = 1.5f;
    [Header("Header Setting")]
    [SerializeField] private Text heartCountText;
    [SerializeField] private Image heartIcon;
    [SerializeField] private Button addHeartButton;
    [SerializeField] private RefillHeartPopup refillHeartPopup;
    private bool enableAddHeartButton;
    private int oldCoins;
    // [SerializeField] private Button settingButton;
    [Header("Animation Setting")]
    private Vector3 originPosHomeButton;
    private Vector3 originPosShopButton;
    [SerializeField] private float startY = -1000f;
    [SerializeField] private float duration = 0.5f;
    [SerializeField] private float newPos;
    [Header("UI References")]
    [SerializeField] private GameObject homeButtonBackground;
    [SerializeField] private GameObject shopButtonBackground;
    [Header("Level References")]
    [SerializeField] private LevelUI currLevel;
    [SerializeField] private LevelUI nextLevel1;
    [SerializeField] private LevelUI nextLevel2;

    void OnEnable()
    {
        DataManager.OnChangeLevel+=ShowLevels;
        DataManager.OnChangeHeart+=UpdateHeartCountText;
        DataManager.OnChangeCoins+=UpdateCoinsText;

        homeButton.onClick.AddListener(OnClickHome);
        shopButton.onClick.AddListener(OnClickShop);
        playButton.onClick.AddListener(OnClickPlay);
        AddCoins.onClick.AddListener(OnClickShop);
        Setting.onClick.AddListener(uIManager.OpenSetting);
        addHeartButton.onClick.AddListener(OnClickAddHeart);
    }

    void OnDisable()
    {
        DataManager.OnChangeLevel-=ShowLevels;
        DataManager.OnChangeHeart-=UpdateHeartCountText;
        DataManager.OnChangeCoins-=UpdateCoinsText;

        homeButton.onClick.RemoveListener(OnClickHome);
        shopButton.onClick.RemoveListener(OnClickShop);
        playButton.onClick.RemoveListener(OnClickPlay);
        AddCoins.onClick.RemoveListener(OnClickShop);
        Setting.onClick.RemoveListener(uIManager.OpenSetting);
        addHeartButton.onClick.RemoveListener(OnClickAddHeart);
    }

    public void Setup(UIManager uIManager)
    {
        originPosShopButton = shopButton.transform.position;
        originPosHomeButton = homeButton.transform.position;

        this.uIManager = uIManager;
        refillHeartPopup.Setup(uIManager);

        oldCoins = DataManager.Instance.playerData.totalCoins;
        coinText.text = DataManager.Instance.playerData.totalCoins.ToString();
        // heartCountText.text = DataManager.Instance.playerData.heart.ToString();
        UpdateHeartCountText(DataManager.Instance.playerData.heart);

        ShowLevels(DataManager.Instance.playerData.currentLevel);

        if(DataManager.Instance.playerData.heart < 5) enableAddHeartButton = true;
        else enableAddHeartButton = false;

        OnClickHome();
    }

    public void ShowLevels(int level)
    {
        currLevel.ShowLevel(level);
        nextLevel1.ShowLevel(level);
        nextLevel2.ShowLevel(level);
    }

    private void OnClickHome()
    {
        shopButtonBackground.SetActive(false);
        shopButton.transform.position = originPosShopButton;
        
        uIManager.CloseShop();

        homeButtonBackground.SetActive(true);
        homeButtonBackground.GetComponent<RectTransform>().DOAnchorPosY(startY,duration).From();
        homeButton.gameObject.transform.position = originPosHomeButton + new Vector3(0, newPos, 0);
    }
    private void OnClickShop()
    {
        shopButtonBackground.SetActive(true);
        shopButton.gameObject.transform.position = originPosShopButton + new Vector3(0, newPos, 0);
        shopButtonBackground.GetComponent<RectTransform>().DOAnchorPosY(startY,duration).From();
        
        uIManager.OpenShop();

        homeButtonBackground.SetActive(false);
        homeButton.gameObject.transform.position = originPosHomeButton;
    }
    private void OnClickPlay()
    {
        GameManager.Instance.ChangeState(GameManager.GameState.Playing);
    }

    public void AddCoin()
    {
        int currCoins = DataManager.Instance.playerData.totalCoins;

        // Dừng các hiệu ứng đang chạy (nếu người chơi bấm nhận thưởng liên tục)
        DOTween.Kill("GoldCounter"); 
        AudioManager.Instance.PlayCoinCollectAudio();
        // DOTween sẽ chạy một biến ảo (Virtual) từ số cũ lên số mới
        DOVirtual.Int(oldCoins, currCoins, countDuration, (value) => 
        {
            coinText.text = value.ToString();
        })
        .SetId("GoldCounter")
        .SetEase(Ease.OutQuad); 

        oldCoins = currCoins;
    }

    public void UpdateCoinsText(int coins)
    {
        coinText.text = coins.ToString();
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

    private void OnClickAddHeart()
    {
        if(enableAddHeartButton)
        {
            refillHeartPopup.Show();
        }
    }


    public void Hide()
    {
        this.gameObject.SetActive(false);
    }

    public void Show()
    {
        this.gameObject.SetActive(true);
    }
}
