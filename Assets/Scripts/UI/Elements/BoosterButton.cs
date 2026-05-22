
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

    void OnEnable()
    {
        boosterButton.onClick.AddListener(OnButtonClicked);
        GameEventBus.Subscribe<BoosterCountUpdatedEvent>(UpdateCountText);
    }

    void OnDisable()
    {
        boosterButton.onClick.RemoveListener(OnButtonClicked);
        GameEventBus.UnSubscribe<BoosterCountUpdatedEvent>(UpdateCountText);
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
        bool isUnlocked = CoreServices.Get<DataManager>().IsUnLockedBooster(id);
        
        lockElements.SetActive(!isUnlocked);
        unlockElements.SetActive(isUnlocked);

        if(CoreServices.Get<DataManager>().IsFirstTimeUserBooster(id))
        {
            // addBoosterUI.SetupButton(this);
            GameEventBus.Publish(new RequestOpenBoosterPopupEvent { type = booster.GetBoosterType(), boosterTransform = this.GetComponent<RectTransform>()});
        }

        UpdateCountText(new BoosterCountUpdatedEvent { boosterId = id, count = CoreServices.Get<DataManager>().GetAmountOfBoosterByID(id) });
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
        if (booster.GetNumsBooster() <= 0)
        {
            GameEventBus.Publish(new RequestOpenBoosterPopupEvent { type = booster.GetBoosterType() , boosterTransform = this.GetComponent<RectTransform>()});
            return;
        }

        if(CoreServices.Get<GameManager>().GetCurrState() == GameManager.GameState.Pause) return;

        boosterButton.interactable = false;
        if(boosterEffect != null) boosterEffect.PlayEffect(booster.Excute);
        boosterButton.interactable = true;
    }


    public void OnClickAddBoosterButton()
    {
        // Bắn tín hiệu qua EventBus
        GameEventBus.Publish(new AddBoosterEvent{ boosterButton = this });
    }

    // public void PlayAddEffect()
    // {
    //     StartCoroutine(addBoosterUI.AddBoosterEffect(this.gameObject.GetComponent<RectTransform>()));
    // }
}
