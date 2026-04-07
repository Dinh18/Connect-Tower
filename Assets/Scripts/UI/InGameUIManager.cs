using System.Collections;
using DG.Tweening;
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
    [SerializeField] private Text levelDifficultLevelText;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color warningColor = Color.red;
    [SerializeField] private float normalFontSize = 50;
    [SerializeField] private float warningFontSize = 60;
    [SerializeField] private float flashSpeed = 0.5f;
    private Coroutine flashCoroutine;
    [Header("Button References")]
    [SerializeField] private Button SettingButton;
    [SerializeField] private Button continueButton;
    [SerializeField] private Button backMainMenuButton;
    [SerializeField] private Button tryAgainButton;
    [Header("Booster UI References")]
    [SerializeField] private Text addMovesCountText;
    [SerializeField] private Text shuffleCountText;
    [SerializeField] private Text hintCountText;
    private UIManager uIManager;
    void Awake()
    {
        GameManager.OnChangeMoves+=UpdateMovesText;
        // DataManager.OnChangeCoins+=UpdateCoinsText;
        // DataManager.OnChangeLevel+=UpdateLevelText;

        SlotsManager.OnChangeFinishedSlots+=UpdateProgressText;
        SlotsManager.OnChangeFinishedSlots+=UpdateFinishedSlotsSlider;
        DataManager.OnChangeCountBooster+=UpdateBoosterCountText;
    }
    public void Hide()
    {
        this.gameObject.SetActive(false);
    }

    public void Setup(UIManager uIManager)
    {
        this.uIManager = uIManager;
        SettingButton.onClick.AddListener(uIManager.OpenSetting);

        continueButton.onClick.AddListener(uIManager.OnClickBackHome);
        backMainMenuButton.onClick.AddListener(uIManager.OnClickBackHome);
        tryAgainButton.onClick.AddListener(OnClickTryAgain);
        coinsButton.onClick.AddListener(uIManager.OpenShop);

        movesText.color = normalColor;
        addMovesCountText.text = DataManager.Instance.GetAmountOfBoosterByID((int) Constants.BoosterType.AddMove).ToString();
        shuffleCountText.text = DataManager.Instance.GetAmountOfBoosterByID((int) Constants.BoosterType.Shuffle).ToString();
        hintCountText.text = DataManager.Instance.GetAmountOfBoosterByID((int) Constants.BoosterType.Hint).ToString();
    }

    public void Show()
    {
        this.gameObject.SetActive(true);
        coinsText.text = DataManager.Instance.playerData.totalCoins.ToString();
        levelText.text = "Level " + (DataManager.Instance.playerData.currentLevel + 1).ToString();
        if(LevelLoader.Instance.gameDifficult == LevelLoader.GameDifficult.Hard)
        {
            levelDifficultImgae.sprite = Resources.Load<Sprite>(Constants.HARD_TEXT_UI);
            levelDifficultLevelText.text = "Hard";

            levelDifficultImgae.gameObject.SetActive(true);
            levelDifficultLevelText.gameObject.SetActive(true);
        }
        else if(LevelLoader.Instance.gameDifficult == LevelLoader.GameDifficult.VeryHard)
        {
            levelDifficultImgae.sprite = Resources.Load<Sprite>(Constants.SUPERHARD_TEXT_UI);
            levelDifficultLevelText.text = "Super Hard";

            levelDifficultImgae.gameObject.SetActive(true);
            levelDifficultLevelText.gameObject.SetActive(true);
        }
        else
        {
            levelDifficultImgae.gameObject.SetActive(false);
            levelDifficultLevelText.gameObject.SetActive(false);
        }

    }

    public void UpdateMovesText(int moves)
    {
        movesText.text = moves.ToString();
        if(moves > 0 && moves <= 5)
        {
            if(flashCoroutine == null)
            {
                flashCoroutine = StartCoroutine(FlashRoutine());
            }
        }
        else if(moves <= 0 || moves > 5)
        {
            StopFlashing();
        }   
    }

    private IEnumerator FlashRoutine()
    {
        while(true)
        {
            movesText.color = normalColor;
            yield return new WaitForSeconds(flashSpeed);

            movesText.color = warningColor;
            yield return new WaitForSeconds(flashSpeed);
        }
    }

    private void StopFlashing()
    {
        if(flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
            flashCoroutine = null;
        }
        movesText.color = normalColor;
    }


    private void OnClickTryAgain()
    {
        GameManager.Instance.ChangeState(GameManager.GameState.Playing);
    }

    // private void UpdateCoinsText(int totalCoins)
    // {
    //     coinsText.text = totalCoins.ToString();
    // }

    // private void UpdateLevelText(int level)
    // {
    //     levelText.text = "Level " + level.ToString();
    // }

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

    private void UpdateBoosterCountText(int id, int numsBooster)
    {
        switch (id)
        {
            case (int) Constants.BoosterType.AddMove:
                addMovesCountText.text = numsBooster.ToString();
                break;
            case (int) Constants.BoosterType.Shuffle:
                shuffleCountText.text = numsBooster.ToString();
                break;
            case (int) Constants.BoosterType.Hint:
                hintCountText.text = numsBooster.ToString();
                break;
        }
    }

}
