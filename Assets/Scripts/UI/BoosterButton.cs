using UnityEngine;
using UnityEngine.UI;

public class BoosterButton : MonoBehaviour
{
    private Button boosterButton;
    private IBooster booster;
    private IBoosterEffect boosterEffect;
    private UIManager uIManager;
    [SerializeField] Text countText;
    [SerializeField] GameObject addImage;
    [SerializeField] AddBoosterUI addBoosterUI;
    void OnEnable()
    {
        boosterButton.onClick.AddListener(OnButtonClicked);
        DataManager.OnChangeCountBooster+=UpdateCountText;
    }
    void OnDisable()
    {
        boosterButton.onClick.RemoveListener(OnButtonClicked);
        DataManager.OnChangeCountBooster-=UpdateCountText;

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
        this.uIManager = uIManager;
        addBoosterUI.Setup(uIManager);
    }

    public void Show()
    {
        int id = (int)booster.GetBoosterType();
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
        if(booster.GetNumsBooster() <= 0)
        {
            string header = booster.GetName();
            string coin = booster.GetPrice().ToString();
            // addBoosterUI.SetConfig(this,header, coin, booster.GetBoosterType());
            // uIManager.PushPopupToFront(addBoosterUI);
            uIManager.OpenAddBooster(addBoosterUI, this, header, coin, booster.GetBoosterType());
            return;
        }

        if(GameManager.Instance.GetCurrState() == GameManager.GameState.Pause) return;

        boosterButton.interactable = false;

        if(boosterEffect != null) boosterEffect.PlayEffect(booster.Excute);

        boosterButton.interactable = true;
    }

    public void OnClickAddBoosterButton()
    {
        uIManager.OnClickAddBooster(this);
    }

    public void PlayAddEffect()
    {
        StartCoroutine(addBoosterUI.AddBoosterEffect(this.gameObject.GetComponent<RectTransform>()));
    }
}
