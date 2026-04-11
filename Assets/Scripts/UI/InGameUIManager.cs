using System.Collections;
using System.Reflection;
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
    private BoosterButton[] boosterButtons;
    // [SerializeField] private Text addMovesCountText;
    // [SerializeField] private Text shuffleCountText;
    // [SerializeField] private Text hintCountText;
    // [SerializeField] private GameObject addAddmoveImage;
    // [SerializeField] private GameObject addShuffleImage;
    // [SerializeField] private GameObject addHintImage;
    private UIManager uIManager;
    void OnEnable()
    {
        GameManager.OnChangeMoves+=UpdateMovesText;
        SlotsManager.OnChangeFinishedSlots+=UpdateProgressText;
        SlotsManager.OnChangeFinishedSlots+=UpdateFinishedSlotsSlider;
        // DataManager.OnChangeCountBooster+=UpdateBoosterCountText;

        SettingButton.onClick.AddListener(uIManager.OpenSetting);
        continueButton.onClick.AddListener(uIManager.OnClickBackHome);
        backMainMenuButton.onClick.AddListener(uIManager.OnClickBackHome);
        tryAgainButton.onClick.AddListener(OnClickTryAgain);
        coinsButton.onClick.AddListener(() => uIManager.OpenShop(false));
    }
    void OnDisable()
    {
        GameManager.OnChangeMoves-=UpdateMovesText;
        SlotsManager.OnChangeFinishedSlots-=UpdateProgressText;
        SlotsManager.OnChangeFinishedSlots-=UpdateFinishedSlotsSlider;
        // DataManager.OnChangeCountBooster-=UpdateBoosterCountText;

        SettingButton.onClick.RemoveListener(uIManager.OpenSetting);
        continueButton.onClick.RemoveListener(uIManager.OnClickBackHome);
        backMainMenuButton.onClick.RemoveListener(uIManager.OnClickBackHome);
        tryAgainButton.onClick.RemoveListener(OnClickTryAgain);
        coinsButton.onClick.RemoveListener(() => uIManager.OpenShop(false));
    }
    public void Hide()
    {
        this.gameObject.SetActive(false);
    }

    public void Setup(UIManager uIManager)
    {
        this.uIManager = uIManager;
        boosterButtons = FindObjectsOfType<BoosterButton>(true);
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

}
