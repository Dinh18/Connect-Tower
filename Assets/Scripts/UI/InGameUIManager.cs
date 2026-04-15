using System.Collections;
using System.Reflection;
using DG.Tweening;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.PlayerLoop;
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
    [SerializeField] private float flashSpeed = 0.5f; // Thời gian cho 1 nhịp thở
    [SerializeField] private float scaleMultiplier = 1.2f;
    private bool isFlashing = false;
    [Header("Button References")]
    [SerializeField] private Button SettingButton;
    [Header("Booster UI References")]
    private BoosterButton[] boosterButtons;
    private UIManager uIManager;
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
        else return null;
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
        GameManager.OnChangeMoves+=UpdateMovesText;
        SlotsManager.OnChangeFinishedSlots+=UpdateProgressText;
        SlotsManager.OnChangeFinishedSlots+=UpdateFinishedSlotsSlider;
        DataManager.OnChangeCoins+=UpDateCoinText;
    }
    void OnDisable()
    {
        GameManager.OnChangeMoves-=UpdateMovesText;
        SlotsManager.OnChangeFinishedSlots-=UpdateProgressText;
        SlotsManager.OnChangeFinishedSlots-=UpdateFinishedSlotsSlider;
        DataManager.OnChangeCoins-=UpDateCoinText;
        // DataManager.OnChangeCountBooster-=UpdateBoosterCountText;

    }
    public void Hide()
    {
        this.gameObject.SetActive(false);
    }

    public void Setup(UIManager uIManager)
    {
        this.uIManager = uIManager;

        SettingButton.onClick.RemoveAllListeners();
        SettingButton.onClick.AddListener(uIManager.OpenSetting);

        coinsButton.onClick.RemoveAllListeners();
        coinsButton.onClick.AddListener(() => uIManager.OpenShop(false));

        boosterButtons = FindObjectsByType<BoosterButton>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach(var booster in boosterButtons)
        {
            booster.Setup(uIManager);
        }

    }
    public void Show()
    {
        this.gameObject.SetActive(true);
        coinsText.text = DataManager.Instance.playerData.totalCoins.ToString();
        levelText.text = "Level " + (DataManager.Instance.playerData.currentLevel + 1).ToString();
        movesText.color = normalColor;
        movesText.text = GameManager.Instance.GetMaxMoves().ToString();
        foreach(var booster in boosterButtons)
        {
            booster.Show();
        }

        UpdateFinishedSlotsSlider(0, LevelLoader.Instance.GetNumsTopic());
        UpdateProgressText(0, LevelLoader.Instance.GetNumsTopic());
        
        SetupProgressBar(LevelLoader.Instance.gameDifficult);
        
        StopWarningFlash();
        Debug.Log("InGame");

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
        else if(moves <= 0 || moves > 5)
        {
            StopWarningFlash();
        }   
    }

    public void StartWarningFlash()
    {
        movesText.DOKill();
        movesText.transform.DOKill();

        movesText.DOColor(warningColor, flashSpeed)
                 .SetLoops(-1, LoopType.Yoyo) 
                 .SetEase(Ease.InOutSine);    

        movesText.transform.DOScale(Vector3.one * scaleMultiplier, flashSpeed)
                           .SetLoops(-1, LoopType.Yoyo)
                           .SetEase(Ease.InOutSine);
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
        finishedSlotsSlider.DOValue(value, 0.5f)
                .SetEase(Ease.OutCubic);
    }
    private void UpdateProgressText(int finishedSlots, int numSlots)
    {
        progressText.text = finishedSlots.ToString() + "/" + numSlots.ToString();
    }

    private void UpDateCoinText(int coins)
    {
        coinsText.text = coins.ToString();
    }

    public GameObject GetGameObject()
    {
        return this.gameObject;
    }
}
