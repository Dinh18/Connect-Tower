using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuUIManager : MonoBehaviour, IMenu
{
    private UIManager uIManager;
    [Header("Button References")]
    [SerializeField] private Button addCoins;
    [SerializeField] private Button setting;
    [SerializeField] private Button playButton;
    [SerializeField] private Button homeButton;
    [SerializeField] private Button shopButton;
    [Header("Coins Text Setting")]
    [SerializeField] private Text coinText;
    // [SerializeField] private float countDuration = 1.5f;
    [Header("Header Setting")]
    [SerializeField] private Text heartCountText;
    [SerializeField] private Image heartIcon;
    [SerializeField] private Button addHeartButton;
    [SerializeField] private RefillHeartPopup refillHeartPopup;
    [Header("Level UI")]
    [SerializeField] private LevelUIManager levelUIManager;
    private bool enableAddHeartButton;
    private int oldCoins;
    // [SerializeField] private Button settingButton;
    [Header("Animation Setting")]
    private BackgroundButton shopBackGround;
    private BackgroundButton homeBackGround;
    [Header("UI References")]
    [SerializeField] private GameObject homeButtonBackground;
    [SerializeField] private GameObject shopButtonBackground;

    void OnEnable()
    {
        homeButton.onClick.AddListener(OnClickHome);
        shopButton.onClick.AddListener(OnClickShop);
        playButton.onClick.AddListener(OnClickPlay);
        addCoins.onClick.AddListener(OnClickShop);
        addHeartButton.onClick.AddListener(OnClickAddHeart);
    }

    void OnDisable()
    {
        homeButton.onClick.RemoveListener(OnClickHome);
        shopButton.onClick.RemoveListener(OnClickShop);
        playButton.onClick.RemoveListener(OnClickPlay);
        addCoins.onClick.RemoveListener(OnClickShop);
        addHeartButton.onClick.RemoveListener(OnClickAddHeart);
    }

    public void Setup(UIManager uIManager)
    {
        this.uIManager = uIManager;
        refillHeartPopup.Setup(uIManager);
        refillHeartPopup.ConfigMainMenu(this);

        oldCoins = DataManager.Instance.playerData.totalCoins;
        coinText.text = DataManager.Instance.playerData.totalCoins.ToString();

        setting.onClick.RemoveAllListeners();
        setting.onClick.AddListener(uIManager.OpenSetting);

        DataManager.OnChangeHeart-=UpdateHeartCountText;
        DataManager.OnChangeHeart+=UpdateHeartCountText;


        UpdateHeartCountText(DataManager.Instance.playerData.heart);

        if(DataManager.Instance.playerData.heart < 5) enableAddHeartButton = true;
        else enableAddHeartButton = false;

        shopBackGround = shopButton.GetComponent<BackgroundButton>();
        homeBackGround = homeButton.GetComponent<BackgroundButton>();

        levelUIManager.Show();

        shopBackGround.Init();
        homeBackGround.Init();

        // Debug.Log()

    }


    private void OnClickHome()
    {
        // yield return new WaitForEndOfFrame();
        homeBackGround.Select();
        uIManager.CloseShop();   
        shopBackGround.UnSelect();
        // Debug.Log("Ngu");
    }
    public void OnClickShop()
    {
        shopBackGround.Select();
        uIManager.OpenShop(true);
        homeBackGround.UnSelect();
    }
    private void OnClickPlay()
    {
        if(DataManager.Instance.playerData.heart > 0)
        {
            GameManager.Instance.ChangeState(GameManager.GameState.Playing);
        }
        else
        {
            OpenRefillHeart();
        }
    }

    public void OpenRefillHeart()
    {
        uIManager.PushPopupToFront(refillHeartPopup, refillHeartPopup.GetComponent<RectTransform>());
    }

    public void UpdateCoinText()
    {
        coinText.text = DataManager.Instance.playerData.totalCoins.ToString();
    }

    public void AddCoin(int winAmount)
    {
        // 1. Lấy tổng tiền cuối cùng (Đã trừ tiền mua Booster, đã cộng tiền Win)
        int finalCoins = DataManager.Instance.playerData.totalCoins;
        
        // 2. Tính ra số tiền ở "Trạng thái trung gian" (Chỉ trừ Booster, chưa cộng Win)
        int coinsBeforeWin = finalCoins - winAmount; 

        // 3. ÉP THẲNG SỐ TIỀN TRÊN UI VỀ MỨC ĐÃ GIẢM NGAY LẬP TỨC
        oldCoins = coinsBeforeWin;
        coinText.text = oldCoins.ToString();

        // 4. Bắt đầu bắn đồng xu bay lên để cộng dần winAmount vào
        StartCoroutine(SpawnWinCoinsRoutine(winAmount));
    }

    private IEnumerator SpawnWinCoinsRoutine(int winAmount)
    {
        DOTween.Kill(coinText.transform);
        // Chờ 0.5 giây để người chơi kịp nhìn thấy chữ "You Win!"
        // yield return new WaitForSeconds(0.5f);

        Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
        
        int visualCoins = 10;
        int coinValueBase = winAmount / visualCoins;
        int coinValueRemainder = winAmount % visualCoins;

        for (int i = 0; i < visualCoins; i++)
        {
            GameObject coinObj = CoinEffect.Instance.GetCoin();
            CoinFlyEffect coinEffect = coinObj.GetComponent<CoinFlyEffect>();

            // Tính toán xem ĐỒNG XU NÀY mang giá trị bao nhiêu tiền
            int finalValueForThisCoin = coinValueBase + (i == visualCoins - 1 ? coinValueRemainder : 0);

            coinEffect.StartBurstAndFly(screenCenter, coinText.GetComponent<RectTransform>(), () => 
            {
                OnCoinHitTarget(finalValueForThisCoin);
            });

            // CHÌA KHÓA NẰM Ở ĐÂY: Dừng 0.1 giây rồi mới vòng lại bắn đồng xu tiếp theo
            yield return new WaitForSeconds(0.1f); 
        }
    }
    private void OnCoinHitTarget(int amountAdded)
    {
        // Cộng dồn tiền thật
        oldCoins += amountAdded; 
        coinText.text = oldCoins.ToString();

        // Chơi âm thanh ăn tiền (sẽ kêu "Ting ting ting" theo từng đồng rất đã tai)
        AudioManager.Instance.PlayCoinCollectAudio();


        Debug.Log("oldCoins: " + oldCoins);
        // Làm icon text giật nảy lên một chút để có cảm giác va chạm
        coinText.transform.DOPunchScale(new Vector3(0.2f, 0.2f, 0), 0.1f).SetEase(Ease.InOutBounce);
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
            refillHeartPopup.ConfigMainMenu(this);
            OpenRefillHeart();
        }
    }


    public void Hide()
    {
        this.gameObject.SetActive(false);
    }

    public void Show()
    {
        this.gameObject.SetActive(true);
        oldCoins = DataManager.Instance.playerData.totalCoins;
        coinText.text = oldCoins.ToString();
        levelUIManager.Show();
        OnClickHome();
    }

    public GameObject GetGameObject()
    {
        return this.gameObject;
    }
}
