using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class InGameUIManager : MonoBehaviour, IMenu
{
    [Header("Moves text Setting")]
    [SerializeField] private Text movesText;
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
    private bool isFlashing = false;

    [Header("Button References")]
    [SerializeField] private Button SettingButton;

    private BoosterButton[] boosterButtons;
    private GameManager gameManager;
    private LevelLoader levelLoader;

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
        GameManager.OnChangeMoves += UpdateMovesText;
        SlotsManager.OnChangeFinishedSlots += UpdateProgressText;
        SlotsManager.OnChangeFinishedSlots += UpdateFinishedSlotsSlider;
        DataManager.OnChangeCoins += UpDateCoinText;
        
        SettingButton.onClick.RemoveAllListeners();
        SettingButton.onClick.AddListener(() => GameEventBus.OnRequestUI?.Invoke(GameEventBus.UIType.Settings));

        coinsButton.onClick.RemoveAllListeners();
        coinsButton.onClick.AddListener(() => GameEventBus.OnRequestUI?.Invoke(GameEventBus.UIType.Shop));
    }

    void OnDisable()
    {
        GameManager.OnChangeMoves -= UpdateMovesText;
        SlotsManager.OnChangeFinishedSlots -= UpdateProgressText;
        SlotsManager.OnChangeFinishedSlots -= UpdateFinishedSlotsSlider;
        DataManager.OnChangeCoins -= UpDateCoinText;
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

        coinsText.text = DataManager.Instance.playerData.totalCoins.ToString();
        levelText.text = "Level " + (DataManager.Instance.playerData.currentLevel + 1).ToString();
        movesText.color = normalColor;
        movesText.text = gameManager.GetMaxMoves().ToString();
        
        foreach(var booster in boosterButtons)
        {
            booster.Show();
        }

        UpdateFinishedSlotsSlider(0, levelLoader.GetNumsTopic());
        UpdateProgressText(0, levelLoader.GetNumsTopic());
        SetupProgressBar(levelLoader.gameDifficult);
        StopWarningFlash();
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

    public void UpdateMovesText(int moves)
    {
        movesText.text = moves.ToString();
        if(moves > 0 && moves <= 5)
        {
            if(!isFlashing) StartWarningFlash();
        }
        else
        {
            StopWarningFlash();
        }   
    }

    public void StartWarningFlash()
    {
        movesText.DOKill();
        movesText.transform.DOKill();
        movesText.DOColor(warningColor, flashSpeed).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);    
        movesText.transform.DOScale(Vector3.one * scaleMultiplier, flashSpeed).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
        isFlashing = true;
    }

    public void StopWarningFlash()
    {
        movesText.DOKill();
        movesText.transform.DOKill();
        movesText.color = normalColor;
        movesText.transform.localScale = Vector3.one; 
        isFlashing = false;
    }

    private void UpdateFinishedSlotsSlider(int finishedSlots, int numSlots)
    {
        float value = (float) finishedSlots / numSlots;
        finishedSlotsSlider.DOValue(value, 0.5f).SetEase(Ease.OutCubic);
    }

    private void UpdateProgressText(int finishedSlots, int numSlots)
    {
        progressText.text = finishedSlots.ToString() + "/" + numSlots.ToString();
    }

    private void UpDateCoinText(int coins) => coinsText.text = coins.ToString();

    public void Hide() => this.gameObject.SetActive(false);
    public GameObject GetGameObject() => this.gameObject;
}
