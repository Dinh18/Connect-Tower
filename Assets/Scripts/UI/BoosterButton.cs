using UnityEngine;
using UnityEngine.UI;

public class BoosterButton : MonoBehaviour
{
    private Button boosterButton;
    private IBooster booster;
    private IBoosterEffect boosterEffect;
    // private UIManager uIManager; // Đã loại bỏ
    [SerializeField] Text countText;
    [SerializeField] GameObject addImage;
    [SerializeField] AddBoosterUI addBoosterUI;
    [SerializeField] GameObject lockElements;
    [SerializeField] GameObject unlockElements;

    void OnEnable()
    {
        boosterButton.onClick.AddListener(OnButtonClicked);
        DataManager.OnChangeCountBooster += UpdateCountText;
    }

    void OnDisable()
    {
        boosterButton.onClick.RemoveListener(OnButtonClicked);
        DataManager.OnChangeCountBooster -= UpdateCountText;
    }

    void Awake()
    {
        boosterButton = GetComponent<Button>();
        booster = GetComponentInChildren<IBooster>();
        boosterEffect = GetComponentInChildren<IBoosterEffect>();
    }

    public IBooster GetBooster() => booster;

    public void Setup(UIManager uIManager)
    {
        addBoosterUI.Setup(uIManager);
    }

    public void Show()
    {
        int id = (int)booster.GetBoosterType();
        bool isUnlocked = DataManager.Instance.IsUnLockedBooster(id);
        
        lockElements.SetActive(!isUnlocked);
        unlockElements.SetActive(isUnlocked);

        if(DataManager.Instance.playerData.currentLevel == DataManager.Instance.GetUnclockedLevel(id) && DataManager.Instance.IsFirstTimeUserBooster(id))
        {
            addBoosterUI.SetupButton(this);
            OpenAddBoosterPopup(true);
        }

        UpdateCountText(id, DataManager.Instance.GetAmountOfBoosterByID(id));
    }

    public void UpdateCountText(int id, int amount)
    {
        if(id != (int) booster.GetBoosterType()) return;
        if(amount <= 0)
        {
            addImage.SetActive(true);
        }
        else
        {
            addImage.SetActive(false);
            countText.text = amount.ToString();
        }
    }

    public void OnButtonClicked()
    {
        if (!DataManager.Instance.IsUnLockedBooster((int)booster.GetBoosterType())) return;

        int id = (int)booster.GetBoosterType();
        if(DataManager.Instance.IsFirstTimeUserBooster(id) && TutorialManager.Instance.currentTutorial == TutorialManager.TutorialType.BoosterUI)
        {
            TutorialManager.Instance.EndBoosterTutorial(id);
        }

        if(booster.GetNumsBooster() <= 0)
        {
            OpenAddBoosterPopup(false);
            return;
        }

        if(CoreServices.Get<GameManager>().GetCurrState() == GameManager.GameState.Pause) return;

        boosterButton.interactable = false;
        if(boosterEffect != null) boosterEffect.PlayEffect(booster.Excute);
        boosterButton.interactable = true;
    }

    private void OpenAddBoosterPopup(bool isFirstTime)
    {
        string header = booster.GetName();
        string coin = booster.GetPrice().ToString();
        // Bắn tín hiệu qua EventBus
        GameEventBus.OnRequestAddBooster?.Invoke(addBoosterUI, this, header, coin, booster.GetBoosterType(), isFirstTime);
    }

    public void OnClickAddBoosterButton()
    {
        // Bắn tín hiệu qua EventBus
        GameEventBus.OnClickAddBooster?.Invoke(this);
    }

    public void PlayAddEffect()
    {
        StartCoroutine(addBoosterUI.AddBoosterEffect(this.gameObject.GetComponent<RectTransform>()));
    }
}
