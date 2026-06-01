
using UnityEngine;
using UnityEngine.UI;

public class BoosterButton : MonoBehaviour
{
    private Button boosterButton;
    private Booster booster;
    private IBoosterEffect boosterEffect;
    // private UIManager uIManager; // Đã loại bỏ
    [SerializeField] Text countText;
    [SerializeField] GameObject addImage;
    // [SerializeField] AddBoosterUI addBoosterUI;
    [SerializeField] GameObject lockElements;
    [SerializeField] GameObject unlockElements;
    [SerializeField] FloatingNotifier floatingNotifier;

    void OnEnable()
    {
        boosterButton.onClick.AddListener(OnButtonClicked);
        GameEventBus.Subscribe<BoosterCountUpdatedEvent>(UpdateCountText);
        GameEventBus.Subscribe<RequestUnlockBoosterEvent>(UnClockBooster);
        GameEventBus.Subscribe<BoosterAnimationStateEvent>(OnBoosterAnimationStateChanged);
    }

    void OnDisable()
    {
        boosterButton.onClick.RemoveListener(OnButtonClicked);
        GameEventBus.UnSubscribe<BoosterCountUpdatedEvent>(UpdateCountText);
        GameEventBus.UnSubscribe<RequestUnlockBoosterEvent>(UnClockBooster);
        GameEventBus.UnSubscribe<BoosterAnimationStateEvent>(OnBoosterAnimationStateChanged);
    }

    private void OnBoosterAnimationStateChanged(BoosterAnimationStateEvent evt)
    {
        boosterButton.interactable = !evt.isAnimating;
    }

    void Awake()
    {
        boosterButton = GetComponent<Button>();
        booster = GetComponentInChildren<Booster>();
        boosterEffect = GetComponentInChildren<IBoosterEffect>();
    }
        

    public Booster GetBooster() => booster;


    public void Show()
    {
        int id = (int)booster.GetBoosterType();
        bool isUnlocked = CoreServices.Get<DataManager>().IsUnLockedBooster(id) || LevelLoader.isPlaytestingTempLevel;
        
        lockElements.SetActive(!isUnlocked);
        unlockElements.SetActive(isUnlocked);

        if(!LevelLoader.isPlaytestingTempLevel && CoreServices.Get<DataManager>().IsFirstTimeUserBooster(id))
        {
            // Không tự động trigger tutorial ở đây nữa, BottomPanel sẽ trigger theo ngữ cảnh
            // GameEventBus.Publish(new RequestOpenBoosterPopupEvent { type = booster.GetBoosterType(), boosterTransform = this.GetComponent<RectTransform>()});
        }

        int count = LevelLoader.isPlaytestingTempLevel ? 99 : CoreServices.Get<DataManager>().GetAmountOfBoosterByID(id);
        UpdateCountText(new BoosterCountUpdatedEvent { boosterId = id, count = count });
    }

    public void UpdateCountText(BoosterCountUpdatedEvent boosterCountUpdated)
    {
        int id = boosterCountUpdated.boosterId;
        int amount = boosterCountUpdated.count;
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
        if(!CoreServices.Get<DataManager>().IsUnLockedBooster((int)booster.GetBoosterType()))
        {
            floatingNotifier.ShowWarning("Unlock at level " + (CoreServices.Get<DataManager>().GetBooster((int)booster.GetBoosterType()).unlockedLevel + 1));
            return;
        }
        if (!LevelLoader.isPlaytestingTempLevel && booster.GetNumsBooster() <= 0)
        {
            GameEventBus.Publish(new RequestOpenBoosterPopupEvent { type = booster.GetBoosterType() , boosterTransform = this.GetComponent<RectTransform>()});
            return;
        }

        

        if(CoreServices.Get<GameManager>().GetCurrState() == GameManager.GameState.Pause) return;

        GameEventBus.Publish(new BoosterAnimationStateEvent { isAnimating = true });
        CoreServices.Get<InputManager>().SetInputBlocked(true);

        if(boosterEffect != null) 
        {
            boosterEffect.PlayEffect(() => 
            {
                booster.Excute();
                if(booster.GetBoosterType() == BoosterType.Shuffle || booster.GetBoosterType() == BoosterType.Undo)
                {
                    GameEventBus.Publish(new BoardStateChangedEvent());
                }
                CoreServices.Get<InputManager>().SetInputBlocked(false);
                GameEventBus.Publish(new BoosterAnimationStateEvent { isAnimating = false });
            });
        }
        else
        {
            booster.Excute();
            if(booster.GetBoosterType() == BoosterType.Shuffle || booster.GetBoosterType() == BoosterType.Undo)
            {
                GameEventBus.Publish(new BoardStateChangedEvent());
            }
            CoreServices.Get<InputManager>().SetInputBlocked(false);
            GameEventBus.Publish(new BoosterAnimationStateEvent { isAnimating = false });
        }
    }

    private void UnClockBooster(RequestUnlockBoosterEvent evt)
    {
        if(evt.boosterType == booster.GetBoosterType())
        {
            lockElements.SetActive(false);
            unlockElements.SetActive(true);
        }
    }


    public void OnClickAddBoosterButton()
    {
        // Bắn tín hiệu qua EventBus
        GameEventBus.Publish(new AddBoosterEvent{ boosterButton = this });
    }
}
