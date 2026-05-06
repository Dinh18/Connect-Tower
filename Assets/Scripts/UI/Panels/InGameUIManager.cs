using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class InGameUIManager : MonoBehaviour, IMenu
{
    [Header("Moves text Setting")]
    [SerializeField] private Text movesText;
    // [SerializeField] private Image warningBorderImage;
    [SerializeField] private Text coinsText;
    [SerializeField] private Button coinsButton;
    [SerializeField] private Text levelText;
    [Header("Progress Bar")]
    [SerializeField] private Slider finishedSlotsSlider;
    [SerializeField] private Text progressText;
    [SerializeField] private Image levelDifficultImgae;
    [SerializeField] private Image levelDifficultProgressImage;
    [SerializeField] private Text levelDifficultLevelText;
    
    private Sprite hardLevelSprite;
    private Sprite superLevelSprite;
    private Sprite normalLevelProgressSprite;
    private Sprite hardLevelProgressSprite;
    private Sprite superLevelProgressSprite;

    [Header("Move Count Text Setting")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color warningColor = Color.red;
    [SerializeField] private float flashSpeed = 0.5f; 
    [SerializeField] private float scaleMultiplier = 1.2f;
    [SerializeField] private UICountdownController countdownImage;
    private bool isFlashing = false;

    [Header("Button References")]
    [SerializeField] private Button SettingButton;

    private BoosterButton[] boosterButtons;
    private GameManager gameManager;
    private LevelLoader levelLoader;
    [SerializeField]private DifficultLevel difficultLevel;
    [SerializeField] private AddBoosterUI addBoosterUI;

    private Sprite GetLevelSprite(LevelLoader.GameDifficult gameDifficult)
    {
        if(gameDifficult == LevelLoader.GameDifficult.Hard)
        {
            if(hardLevelSprite == null) hardLevelSprite = Resources.Load<Sprite>(Constants.HARD_TEXT_UI);
            return hardLevelSprite;
        }
        else if(gameDifficult == LevelLoader.GameDifficult.VeryHard)
        {
            if(superLevelSprite == null) superLevelSprite = Resources.Load<Sprite>(Constants.SUPERHARD_TEXT_UI);
            return superLevelSprite;
        }
        return null;
    } 

    private Sprite GetLevelProgressSprite(LevelLoader.GameDifficult gameDifficult)
    {
        if(gameDifficult == LevelLoader.GameDifficult.Easy)
        {
            if(normalLevelProgressSprite == null) normalLevelProgressSprite = Resources.Load<Sprite>(Constants.NORMAL_PROGRESS);
            return normalLevelProgressSprite;
        }
        else if(gameDifficult == LevelLoader.GameDifficult.Hard)
        {
            if(hardLevelProgressSprite == null) hardLevelProgressSprite = Resources.Load<Sprite>(Constants.HARD_PROGRESS);
            return hardLevelProgressSprite;
        }
        else
        {
            if(superLevelProgressSprite == null) superLevelProgressSprite = Resources.Load<Sprite>(Constants.SUPER_HARD_PROGRESS);
            return superLevelProgressSprite;
        }
    }

    void OnEnable()
    {
        GameEventBus.Subscribe<MovesUpdatedEvent>(UpdateMovesText);
        GameEventBus.Subscribe<FinishedSlotsUpdatedEvent>(OnUpdateProgress);
        GameEventBus.Subscribe<CoinsUpdatedEvent>(OnCoinsUpdated);
        GameEventBus.Subscribe<RequestOpenBoosterPopupEvent>(OnOpenAddMovePopup);
        GameEventBus.Subscribe<StartBorderFlashEvent>(StartInfiniteMovesCountDown);
        GameEventBus.Subscribe<StopBorderFlashEvent>(StopInfiniteMovesCountDown);

        SettingButton.onClick.RemoveAllListeners();
        SettingButton.onClick.AddListener(() => GameEventBus.Publish(new RequestOpenPopupEvent { targetPopup = PopupType.Setting }));

        coinsButton.onClick.RemoveAllListeners();
        coinsButton.onClick.AddListener(() => GameEventBus.Publish(new RequestOpenPanelEvent { targetPanel = PanelType.Shop }));
    }

    void OnDisable()
    {
        GameEventBus.UnSubscribe<MovesUpdatedEvent>(UpdateMovesText);
        GameEventBus.UnSubscribe<FinishedSlotsUpdatedEvent>(OnUpdateProgress);
        GameEventBus.UnSubscribe<CoinsUpdatedEvent>(OnCoinsUpdated);
        GameEventBus.UnSubscribe<RequestOpenBoosterPopupEvent>(OnOpenAddMovePopup);
        GameEventBus.UnSubscribe<StartBorderFlashEvent>(StartInfiniteMovesCountDown);
        GameEventBus.UnSubscribe<StopBorderFlashEvent>(StopInfiniteMovesCountDown);

    }
    
    public void Setup(UIManager uIManager)
    {
        levelLoader = CoreServices.Get<LevelLoader>();
        gameManager = CoreServices.Get<GameManager>();

        boosterButtons = GetComponentsInChildren<BoosterButton>(true);
        foreach(var booster in boosterButtons)
        {
            booster.Setup(uIManager);
        }
    }

    public void Show()
    {
        this.gameObject.SetActive(true);
        if (gameManager == null) gameManager = CoreServices.Get<GameManager>();
        if (levelLoader == null) levelLoader = CoreServices.Get<LevelLoader>();

        coinsText.text = CoreServices.Get<DataManager>().GetTotalCoins().ToString();
        levelText.text = "Level " + (CoreServices.Get<DataManager>().GetCurrentLevel() + 1).ToString();
        movesText.color = normalColor;
        movesText.text = gameManager.GetMaxMoves().ToString();
        
        foreach(var booster in boosterButtons)
        {
            booster.Show();
        }
        OnUpdateProgress(new FinishedSlotsUpdatedEvent { finishedSlots = 0, totalSlots = levelLoader.GetNumsTopic() });
        SetupProgressBar(levelLoader.gameDifficult);
        StopWarningFlash();
        countdownImage.gameObject.SetActive(false);
    }

    public void ShowDifficultLevel()
    {
        if (difficultLevel != null) difficultLevel.ShowDifficultLevel();
    }

    private void SetupProgressBar(LevelLoader.GameDifficult gameDifficult)
    {
        if(gameDifficult == LevelLoader.GameDifficult.Hard)
        {
            levelDifficultImgae.sprite = GetLevelSprite(gameDifficult);
            levelDifficultProgressImage.sprite = GetLevelProgressSprite(gameDifficult);
            levelDifficultLevelText.text = "Hard";
            levelDifficultImgae.gameObject.SetActive(true);
            levelDifficultLevelText.gameObject.SetActive(true);
        }
        else if(gameDifficult == LevelLoader.GameDifficult.VeryHard)
        {
            levelDifficultImgae.sprite = GetLevelSprite(gameDifficult);
            levelDifficultProgressImage.sprite = GetLevelProgressSprite(gameDifficult);
            levelDifficultLevelText.text = "Super Hard";
            levelDifficultImgae.gameObject.SetActive(true);
            levelDifficultLevelText.gameObject.SetActive(true);
        }
        else
        {
            levelDifficultProgressImage.sprite = GetLevelProgressSprite(gameDifficult);
            levelDifficultImgae.gameObject.SetActive(false);
            levelDifficultLevelText.gameObject.SetActive(false);
        }
    }

    public void UpdateMovesText(MovesUpdatedEvent movesUpdatedEvent)
    { 
        int moves = movesUpdatedEvent.currentMoves;
        movesText.text = moves.ToString();
        if(moves > 0 && moves <= 5)
        {
            if(!isFlashing) StartWarningFlash();
        }
        else
        {
            if(isFlashing) StopWarningFlash();
        }   
        movesText.transform.localScale = Vector3.one;
        movesText.transform.DOPunchScale(Vector3.one * 0.2f, flashSpeed).SetEase(Ease.InOutSine);   
    }

    public void StartWarningFlash()
    {
        movesText.DOKill();
        movesText.transform.DOKill();
        movesText.DOColor(warningColor, flashSpeed).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);    
        movesText.transform.DOScale(Vector3.one * scaleMultiplier, flashSpeed).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
        // warningBorderImage.DOKill();
        // warningBorderImage.DOFade(borderMaxAlpha, flashSpeed).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
        GameEventBus.Publish(new StartBorderFlashEvent { borderType = BorderType.Warning, flashSpeed = flashSpeed, flashTime = 1000f });
        isFlashing = true;
    }

    public void StopWarningFlash()
    {
        movesText.DOKill();
        movesText.transform.DOKill();
        movesText.color = normalColor;
        movesText.transform.localScale = Vector3.one; 
        // warningBorderImage.DOKill();
        // warningBorderImage.DOFade(0f, 0.2f);
        GameEventBus.Publish(new StopBorderFlashEvent());
        isFlashing = false;
    }

    public void StartInfiniteMovesCountDown(StartBorderFlashEvent startBorderFlash)
    {
        if(startBorderFlash.borderType == BorderType.Ice)
        {
            countdownImage.gameObject.SetActive(true);
            countdownImage.StartCountdown(startBorderFlash.flashTime);
        }
    }

    private void StopInfiniteMovesCountDown(StopBorderFlashEvent stopBorderFlash)
    {
        countdownImage.ResetCountdown();
        countdownImage.gameObject.SetActive(false);
    }
    


    private void OnUpdateProgress(FinishedSlotsUpdatedEvent finishedSlotsUpdated)
    {
        // Update progress text
        int finishedSlots = finishedSlotsUpdated.finishedSlots;
        int numSlots = finishedSlotsUpdated.totalSlots;
        progressText.text = finishedSlots.ToString() + "/" + numSlots.ToString();
        // Update slider
        float value = (float) finishedSlots / numSlots;
        finishedSlotsSlider.DOValue(value, 0.5f).SetEase(Ease.OutCubic);
    }

    private void OnCoinsUpdated(CoinsUpdatedEvent coinsUpdatedEvent)
    {
        coinsText.text = coinsUpdatedEvent.totalCoins.ToString();
    }

    private void OnOpenAddMovePopup(RequestOpenBoosterPopupEvent requestOpenBoosterPopup)
    {
        addBoosterUI.SetConfig(requestOpenBoosterPopup);
        CoreServices.Get<UIManager>().PushPopupToFront(addBoosterUI, addBoosterUI.transform);
        
        BoosterButton matchingBoosterButton = null;
        if (boosterButtons != null)
        {
            foreach (var bb in boosterButtons)
            {
                if (bb.GetBooster().GetBoosterType() == requestOpenBoosterPopup.type)
                {
                    matchingBoosterButton = bb;
                    break;
                }
            }
        }

        if (matchingBoosterButton != null)
        {
            addBoosterUI.SetupButton(matchingBoosterButton);

            DataManager dataManager = CoreServices.Get<DataManager>();
            int boosterID = (int)requestOpenBoosterPopup.type;
            if (dataManager.IsFirstTimeUserBooster(boosterID))
            {
                var tutorialService = CoreServices.Get<TutorialService>();
                if (tutorialService != null)
                {
                    string useInstruction = "";
                    if (requestOpenBoosterPopup.type == Constants.BoosterType.AddMove)
                        useInstruction = "Use the Extra Move to get extra moves!";
                    else if (requestOpenBoosterPopup.type == Constants.BoosterType.Shuffle)
                        useInstruction = "Use it to shuffle the board!";
                    else
                        useInstruction = "Use it to reveal a correct placement";

                    tutorialService.StartBoosterTutorial(addBoosterUI.GetClaimButton(), matchingBoosterButton.GetComponent<Button>(), "Claim your free booster!", useInstruction);
                }
            }
        }
    }

    public void Hide() => this.gameObject.SetActive(false);
    public GameObject GetGameObject() => this.gameObject;
}
