using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Screen = UnityEngine.Screen;

public class HomePanel : Panel
{
    [SerializeField] private Button playButton;
    [SerializeField] private Text playText;
    [SerializeField] private Transform playHolder;
    [SerializeField] private ParticleSystem finishParticle; 
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
    [Header("Button Play Setting")]
    [SerializeField] private Sprite greenPlayButton;
    [SerializeField] private Sprite redPlayButton;
    [SerializeField] private Sprite purplePlayButton;
    [SerializeField] private Sprite hardSkull;
    [SerializeField] private Sprite spHardSkull;
    [SerializeField] private Image skullLeftImage;
    [SerializeField] private Image skullRightImage;

    private MainMenu mainMenu;

    private bool enableAddHeartButton;

    private Quaternion skullLeftOriginalRot;
    private Quaternion skullRightOriginalRot;
    private bool isSkullsRotInitialized = false;

    void Awake()
    {
        var animBtn = playButton.GetComponent<AnimationButton>();
        if (animBtn != null && playHolder != null)
        {
            animBtn.targetTransform = playHolder;
        }
    }

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
        if(CoreServices.Get<GameManager>().GetPrevState() != GameManager.GameState.Win) StartCoroutine(DelayShowLevelButton());
        else
        {
            PlayAnimationPlayButton();
        }
        StartCoroutine(DelayPlaySadAnim());
    }

    private IEnumerator DelayShowLevelButton()
    {
        yield return new WaitUntil(() => CoreServices.Get<DataManager>().dataReady);
        SetupButton();
        
    }

    private void SetupButton()
    {
        LevelLoader.GameDifficult difficultLevel = (LevelLoader.GameDifficult)CoreServices.Get<LevelLoader>()
                                                .GetDifficultLevel(CoreServices.Get<DataManager>().GetCurrentLevel());
        
        bool showLeft = false;
        bool showRight = false;

        if(difficultLevel == LevelLoader.GameDifficult.Easy)
        {
            playButton.gameObject.GetComponent<Image>().sprite = greenPlayButton;
        }
        else if(difficultLevel == LevelLoader.GameDifficult.Hard)
        {
            playButton.gameObject.GetComponent<Image>().sprite = purplePlayButton;
            skullLeftImage.sprite = hardSkull;
            showLeft = true;
        }
        else
        {
            playButton.gameObject.GetComponent<Image>().sprite = redPlayButton;
            skullLeftImage.sprite = spHardSkull;
            showLeft = true;
            showRight = true;
        }
        playText.text = "Level "+(CoreServices.Get<DataManager>().GetCurrentLevel() + 1).ToString();
        AnimateSkulls(showLeft, showRight);
    }

    private void AnimateSkulls(bool showLeft, bool showRight)
    {
        if (!isSkullsRotInitialized)
        {
            skullLeftOriginalRot = skullLeftImage.transform.localRotation;
            skullRightOriginalRot = skullRightImage.transform.localRotation;
            isSkullsRotInitialized = true;
        }

        if (showLeft)
        {
            skullLeftImage.transform.DOKill();
            skullLeftImage.transform.localRotation = skullLeftOriginalRot;
            
            Sequence seqLeft = DOTween.Sequence();
            
            if (!skullLeftImage.gameObject.activeSelf)
            {
                skullLeftImage.gameObject.SetActive(true);
                skullLeftImage.transform.localScale = Vector3.zero;
                seqLeft.Append(skullLeftImage.transform.DOScale(1.1f, 0.4f).SetEase(Ease.OutBack));
            }
            else
            {
                skullLeftImage.transform.localScale = Vector3.one;
            }
            
            seqLeft.Append(skullLeftImage.transform.DOScale(0.95f, 0.8f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine));
            skullLeftImage.transform.DOLocalRotate(new Vector3(0, 0, 5f), 1f).SetRelative(true).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
        }
        else
        {
            if (skullLeftImage.gameObject.activeSelf)
            {
                skullLeftImage.transform.DOKill();
                skullLeftImage.transform.DOScale(0f, 0.3f).SetEase(Ease.InBack).OnComplete(() => {
                    skullLeftImage.gameObject.SetActive(false);
                });
            }
        }

        if (showRight)
        {
            skullRightImage.transform.DOKill();
            skullRightImage.transform.localRotation = skullRightOriginalRot;

            Sequence seqRight = DOTween.Sequence();
            
            if (!skullRightImage.gameObject.activeSelf)
            {
                skullRightImage.gameObject.SetActive(true);
                skullRightImage.transform.localScale = Vector3.zero;
                seqRight.AppendInterval(0.15f);
                seqRight.Append(skullRightImage.transform.DOScale(1.1f, 0.4f).SetEase(Ease.OutBack));
            }
            else
            {
                skullRightImage.transform.localScale = Vector3.one;
            }

            seqRight.Append(skullRightImage.transform.DOScale(0.95f, 0.8f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine));
            skullRightImage.transform.DOLocalRotate(new Vector3(0, 0, -5f), 1f).SetRelative(true).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine).SetDelay(0.2f);
        }
        else
        {
            if (skullRightImage.gameObject.activeSelf)
            {
                skullRightImage.transform.DOKill();
                skullRightImage.transform.DOScale(0f, 0.3f).SetEase(Ease.InBack).OnComplete(() => {
                    skullRightImage.gameObject.SetActive(false);
                });
            }
        }
    }

    private void PlayAnimationPlayButton()
    {
        Sequence sequence = DOTween.Sequence();

        // 1. NHẤC LÊN CAO (Lấy đà): Phóng to và di chuyển lên trên một chút (OutQuad tạo cảm giác mượt)
        float liftHeight = 50f; // Khoảng cách nhấc lên (bạn có thể tuỳ chỉnh theo UI của mình)
        sequence.Append(playHolder.DOScale(1.2f, 0.4f).SetEase(Ease.OutQuad));
        sequence.Join(playHolder.DOLocalMoveY(liftHeight, 0.4f).SetRelative(true).SetEase(Ease.OutQuad));

        // 1.5. LẮC LẮC: Lắc nhẹ hai bên khi đang ở trên cao
        sequence.Append(playHolder.DOShakeRotation(0.25f, new Vector3(0, 0, 20f), 10, 90f, false));

        // 2. ĐẬP MẠNH XUỐNG: Rơi xuống rất nhanh (InExpo) và thu về size gốc
        sequence.Append(playHolder.DOLocalMoveY(-liftHeight, 0.15f).SetRelative(true).SetEase(Ease.InExpo));
        sequence.Join(playHolder.DOScale(1f, 0.15f).SetEase(Ease.InExpo));

        // 3. HIỆU ỨNG VA CHẠM (Squash & Stretch): Bẹp theo trục Y và phình trục X khi chạm đất, sau đó nảy về lại
        sequence.Append(playHolder.DOPunchScale(new Vector3(0.2f, -0.2f, 0), 0.3f, 2, 0.5f));

        // 4. HOÀN THÀNH
        sequence.OnComplete(() =>
        {
                // finishParticle.Play();
                // playText.text = "Level " + (CoreServices.Get<DataManager>().GetCurrentLevel() + 1).ToString();
                ChangeLevelWithParticle();
        });
    }

    private void ChangeLevelWithParticle()
    {
        // 1. Bắn khói
        finishParticle.Play();

        // 2. Canh thời gian để đổi chữ. 
        // Giả sử Particle mất 0.15s để phình to che kín chữ. Ta dùng DOVirtual.DelayedCall để đợi.
        DOVirtual.DelayedCall(0.15f, () => 
        {
            SetupButton();
        });
    }

    private IEnumerator DelayPlaySadAnim()
    {
        yield return new WaitForSeconds(15);
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
