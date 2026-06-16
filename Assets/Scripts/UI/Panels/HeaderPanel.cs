using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HeaderPanel : MonoBehaviour
{
    [Header("Moves text Setting")]
    [SerializeField] private TextMeshProUGUI movesText;
    [SerializeField] private TextMeshProUGUI coinsText;
    [SerializeField] private Button coinsButton;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI specialBlocksText;
    
    [Header("Progress Bar")]
    [SerializeField] private Slider finishedSlotsSlider;
    [SerializeField] private TextMeshProUGUI progressText;
    [SerializeField] private GameObject progressSpecialMode;
    [SerializeField] private Image levelDifficultImgae;
    [SerializeField] private Image levelDifficultProgressImage;
    [SerializeField] private TextMeshProUGUI levelDifficultLevelText;
    
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
    private bool isTextFlashing = false;
    private bool isBorderFlashing = false;

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
        GameEventBus.Subscribe<SpecialBlocksUpdatedEvent>(OnSpecialBlocksUpdated);
        

        if(coinsButton != null)
        {
            coinsButton.onClick.RemoveAllListeners();
            coinsButton.onClick.AddListener(() => CoreServices.Get<UIManager>().OpenShop());
        }
    }

    void OnDisable()
    {
        GameEventBus.UnSubscribe<MovesUpdatedEvent>(UpdateMovesText);
        GameEventBus.UnSubscribe<FinishedSlotsUpdatedEvent>(OnUpdateProgress);
        GameEventBus.UnSubscribe<CoinsUpdatedEvent>(OnCoinsUpdated);
        GameEventBus.UnSubscribe<SpecialBlocksUpdatedEvent>(OnSpecialBlocksUpdated);
    }

    private Vector2 originPos;

    void Awake()
    {
        originPos = GetComponent<RectTransform>().anchoredPosition;
        if (finishedSlotsSlider != null)
        {
            finishedSlotsSlider.minValue = 0f;
            finishedSlotsSlider.maxValue = 1f;
        }
    }

    public void Show()
    {
        this.gameObject.SetActive(true);
        
        // Ensure slider is always active regardless of mode
        if (finishedSlotsSlider != null) finishedSlotsSlider.gameObject.SetActive(true);

        if(CoreServices.Get<LevelLoader>().gameMode == GameMode.Normal)
        {
            if (progressText != null) progressText.gameObject.SetActive(true);
            if (progressSpecialMode != null) progressSpecialMode.SetActive(false);
        }
        else if(CoreServices.Get<LevelLoader>().gameMode == GameMode.SpecialTop)
        {
            if (progressText != null) progressText.gameObject.SetActive(false);
            if (progressSpecialMode != null) progressSpecialMode.SetActive(true);
        }

        
        // RectTransform rect = GetComponent<RectTransform>();
        // rect.DOKill();
        // rect.anchoredPosition = new Vector2(originPos.x, originPos.y + 500f);
        // rect.DOAnchorPosY(originPos.y, 0.5f).SetEase(Ease.OutBack);
        // // if(gameManager == null) Debug.LogError("GameManager is null in HeaderPanel");
        // movesText.text = CoreServices.Get<GameManager>().GetCurrentMoves().ToString();
    }

    public void Hide()
    {
        RectTransform rect = GetComponent<RectTransform>();
        rect.DOKill();
        rect.DOAnchorPosY(originPos.y + 500f, 0.3f).SetEase(Ease.InBack).OnComplete(() =>
        {
            this.gameObject.SetActive(false);
            rect.anchoredPosition = originPos;
        });
    }

    public void Setup()
    {

        if (coinsText != null) coinsText.text = CoreServices.Get<DataManager>().GetTotalCoins().ToString();
        if (levelText != null) levelText.text = "Level " + (CoreServices.Get<DataManager>().GetCurrentLevel() + 1).ToString();
        
        if (movesText != null)
        {
            movesText.color = normalColor;
            if (CoreServices.Get<GameManager>() != null) movesText.text = CoreServices.Get<GameManager>().GetCurrentMoves().ToString();
        }

        if (CoreServices.Get<LevelLoader>() != null)
        {
            if (CoreServices.Get<LevelLoader>().gameMode == GameMode.Normal)
            {
                OnUpdateProgress(new FinishedSlotsUpdatedEvent { finishedSlots = 0, totalSlots = CoreServices.Get<LevelLoader>().GetNumsTopic() });
            }
            SetupProgressBar(CoreServices.Get<LevelLoader>().gameDifficult);
            
            if (specialBlocksText != null)
            {
                bool isSpecialTop = CoreServices.Get<LevelLoader>().gameMode == GameMode.SpecialTop;
                specialBlocksText.gameObject.SetActive(isSpecialTop);
                // We do NOT toggle the parent here anymore because it might contain the slider.
                // If progressSpecialMode is exactly the parent, it is already handled in Show().
                
                if (isSpecialTop && CoreServices.Get<SlotsManager>() != null)
                {
                    specialBlocksText.text = CoreServices.Get<SlotsManager>().unsolvedSpecialBlocks.ToString();
                    if (finishedSlotsSlider != null)
                    {
                        finishedSlotsSlider.value = 0f;
                    }
                }
            }
        }
        StopWarningFlash();
    }

   

    private void SetupProgressBar(LevelLoader.GameDifficult gameDifficult)
    {
        if(gameDifficult == LevelLoader.GameDifficult.Hard)
        {
            if (levelDifficultImgae != null)
            {
                levelDifficultImgae.sprite = GetLevelSprite(gameDifficult);
                levelDifficultImgae.gameObject.SetActive(true);
            }
            if (levelDifficultProgressImage != null) levelDifficultProgressImage.sprite = GetLevelProgressSprite(gameDifficult);
            if (levelDifficultLevelText != null) 
            {
                levelDifficultLevelText.text = "Hard";
                levelDifficultLevelText.gameObject.SetActive(true);
            }
        }
        else if(gameDifficult == LevelLoader.GameDifficult.VeryHard)
        {
            if (levelDifficultImgae != null)
            {
                levelDifficultImgae.sprite = GetLevelSprite(gameDifficult);
                levelDifficultImgae.gameObject.SetActive(true);
            }
            if (levelDifficultProgressImage != null) levelDifficultProgressImage.sprite = GetLevelProgressSprite(gameDifficult);
            if (levelDifficultLevelText != null)
            {
                levelDifficultLevelText.text = "Super Hard";
                levelDifficultLevelText.gameObject.SetActive(true);
            }
        }
        else
        {
            if (levelDifficultProgressImage != null) levelDifficultProgressImage.sprite = GetLevelProgressSprite(gameDifficult);
            if (levelDifficultImgae != null) levelDifficultImgae.gameObject.SetActive(false);
            if (levelDifficultLevelText != null) levelDifficultLevelText.gameObject.SetActive(false);
        }
    }

    public void UpdateMovesText(MovesUpdatedEvent movesUpdatedEvent)
    { 
        if (movesText == null) return;
        int moves = movesUpdatedEvent.currentMoves;
        movesText.text = moves.ToString();

        bool hasLowBomb = false;
        var slotsManager = CoreServices.Get<SlotsManager>();
        if (slotsManager != null && slotsManager.GetAllSlots() != null)
        {
            foreach (var slot in slotsManager.GetAllSlots())
            {
                if (slot.slotType == SlotController.SlotType.Bomb && !slot.isDisposal)
                {
                    if (slot.GetCurrentBombMove() < 5)
                    {
                        hasLowBomb = true;
                        break;
                    }
                }
            }
        }

        bool shouldFlashBorder = (moves > 0 && moves <= 5) || hasLowBomb;
        bool shouldFlashText = (moves > 0 && moves <= 5);

        if (shouldFlashBorder)
        {
            if (!isBorderFlashing) StartBorderFlashOnly();
        }
        else
        {
            if (isBorderFlashing) StopBorderFlashOnly();
        }

        if (shouldFlashText)
        {
            if (!isTextFlashing) StartTextFlashOnly();
        }
        else
        {
            if (isTextFlashing) StopTextFlashOnly();
        }

        movesText.transform.localScale = Vector3.one;
        movesText.transform.DOPunchScale(Vector3.one * 0.2f, flashSpeed).SetEase(Ease.InOutSine);   
    }

    public void StartWarningFlash()
    {
        StartBorderFlashOnly();
        StartTextFlashOnly();
    }

    public void StopWarningFlash()
    {
        StopBorderFlashOnly();
        StopTextFlashOnly();
    }

    private void StartBorderFlashOnly()
    {
        GameEventBus.Publish(new StartBorderFlashEvent { borderType = BorderType.Warning, flashSpeed = flashSpeed, flashTime = 1000f });
        isBorderFlashing = true;
    }

    private void StopBorderFlashOnly()
    {
        GameEventBus.Publish(new StopBorderFlashEvent());
        isBorderFlashing = false;
    }

    private void StartTextFlashOnly()
    {
        if (movesText == null) return;
        movesText.DOKill();
        movesText.transform.DOKill();
        movesText.DOColor(warningColor, flashSpeed).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);    
        movesText.transform.DOScale(Vector3.one * scaleMultiplier, flashSpeed).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
        isTextFlashing = true;
    }

    private void StopTextFlashOnly()
    {
        if (movesText == null) return;
        movesText.DOKill();
        movesText.transform.DOKill();
        movesText.color = normalColor;
        movesText.transform.localScale = Vector3.one; 
        isTextFlashing = false;
    }

    private void OnUpdateProgress(FinishedSlotsUpdatedEvent finishedSlotsUpdated)
    {
        int finishedSlots = finishedSlotsUpdated.finishedSlots;
        int numSlots = finishedSlotsUpdated.totalSlots;
        
        if (progressText != null) progressText.text = finishedSlots.ToString() + "/" + numSlots.ToString();
        
        if (CoreServices.Get<LevelLoader>() != null && CoreServices.Get<LevelLoader>().gameMode == GameMode.Normal)
        {
            if (finishedSlotsSlider != null && numSlots > 0)
            {
                float value = (float) finishedSlots / numSlots;
                finishedSlotsSlider.DOValue(value, 0.5f).SetEase(Ease.OutCubic);
            }
        }
    }

    private void OnCoinsUpdated(CoinsUpdatedEvent coinsUpdatedEvent)
    {
        if (coinsText != null) coinsText.text = coinsUpdatedEvent.totalCoins.ToString();
    }

    private void OnSpecialBlocksUpdated(SpecialBlocksUpdatedEvent evt)
    {
        if (specialBlocksText != null)
        {
            specialBlocksText.text = evt.specialBlocksRemaining.ToString();
        }

        if (CoreServices.Get<LevelLoader>() != null && CoreServices.Get<LevelLoader>().gameMode == GameMode.SpecialTop)
        {
            if (finishedSlotsSlider != null && evt.totalSpecialBlocks > 0)
            {
                int solved = evt.totalSpecialBlocks - evt.specialBlocksRemaining;
                float value = (float) solved / evt.totalSpecialBlocks;
                finishedSlotsSlider.DOValue(value, 0.5f).SetEase(Ease.OutCubic);
            }
        }
    }

    public Transform GetProgressBarTransform()
    {
        return finishedSlotsSlider != null ? finishedSlotsSlider.transform : null;
    }

    public Transform GetSpecialBlockIconTransform()
    {
        return progressSpecialMode != null ? progressSpecialMode.transform : null;
    }
}
