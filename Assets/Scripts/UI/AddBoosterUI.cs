using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;


public class AddBoosterUI : MonoBehaviour, IMenu
{
    [SerializeField] private Text headerText;
    [SerializeField] private Text tutorialText;
    [SerializeField] private Text coinsText;
    [SerializeField] private Image boosterIconImage;
    [SerializeField] private Button addButton;
    [SerializeField] private Button claimButton;
    [SerializeField] private Button closeButton;
    [SerializeField] private Sprite addMoveIcon;
    [SerializeField] private Sprite shuffleIcon;
    [SerializeField] private Sprite hintIcon;
    [SerializeField] private GameObject dimImage;
    
    private RectTransform boosterTransform;
    private BoosterButton boosterButton;
    private BoosterType boosterType;

    public void Setup(UIManager uIManager)
    {
        // closeButton.onClick.RemoveAllListeners();
        // closeButton.onClick.AddListener(OnClickClose);
    }

    void OnEnable()
    {
        closeButton.onClick.AddListener(OnClickClose); 
        addButton.onClick.AddListener(OnClickBuyBooster);      
    }

    void OnDisable()
    {
        closeButton.onClick.RemoveAllListeners();
        addButton.onClick.RemoveAllListeners();
    }

    public void Show()
    {
        this.gameObject.SetActive(true);
        dimImage.SetActive(true);
    }

    public void SetConfig(RequestOpenBoosterPopupEvent requestOpenBoosterPopup)
    {
        int boosterID = (int)requestOpenBoosterPopup.type;
        DataManager dataManager = CoreServices.Get<DataManager>();
        bool isFirstTime = dataManager.IsFirstTimeUserBooster(boosterID);
        string header = dataManager.GetBooster(boosterID).name;
        int coins = dataManager.GetBooster(boosterID).price;
        boosterType = requestOpenBoosterPopup.type;
        boosterTransform = requestOpenBoosterPopup.boosterTransform;

        if(isFirstTime)
        {
            claimButton.gameObject.SetActive(true);
            addButton.gameObject.SetActive(false);
            closeButton.gameObject.SetActive(false);
        }
        else
        {
            claimButton.gameObject.SetActive(false);
            addButton.gameObject.SetActive(true);
            closeButton.gameObject.SetActive(true);
        }

        headerText.text = header;
        coinsText.text = coins.ToString();
        
        // addButton.onClick.RemoveAllListeners();
        // addButton.onClick.AddListener(boosterButton.OnClickAddBoosterButton);
        
        claimButton.onClick.RemoveAllListeners();
        claimButton.onClick.AddListener(OnClickClaim);
        
        if(boosterType == BoosterType.AddMove)
        {
            boosterIconImage.sprite = addMoveIcon;
            tutorialText.text = "Use it to get extra moves";
        }
        else if (boosterType == BoosterType.Shuffle)
        {
            boosterIconImage.sprite = shuffleIcon;
            tutorialText.text = "Use it to shuffle the blocks";
        }
        else
        {
            tutorialText.text = "Use it to see matching blocks";
            boosterIconImage.sprite = hintIcon;
        }
    }

    public void SetupBoosterButton(AddBoosterEvent addBoosterEvent)
    {
        this.boosterButton = addBoosterEvent.boosterButton;
    }

    public void Hide()
    {
        this.gameObject.SetActive(false);
        dimImage.SetActive(false);
    }

    public void OnClickClose()
    {
        GameEventBus.Publish(new RequestClosePopupEvent());
        Debug.Log("Gửi event đóng add booster popup");
    }

    

    public void OnClickBuyBooster()
    {
        if(boosterButton.GetBooster().GetPrice() > CoreServices.Get<DataManager>().GetTotalCoins())
        {
            GameEventBus.Publish(new RequestOpenPanelEvent{targetPanel = PanelType.Shop});
        }
        else
        {
            boosterButton.GetBooster().AddBooster(3);
            GameEventBus.Publish(new RequestAddBoosterEffectEvent{boosterTransfrom = boosterButton.GetComponent<RectTransform>(), spriteIcon = boosterIconImage.sprite});
            OnClickClose();
        }
    }

    public void SetupButton(BoosterButton boosterButton)
    {
        this.boosterButton = boosterButton;
    }

    public void OnClickClaim()
    {
        int id = (int)boosterType;
        DataManager dataManager = CoreServices.Get<DataManager>();
        
        dataManager.AddFreeBooster(id, 1);
        dataManager.UsedBooster(id);

        OnClickClose();
    }

    public GameObject GetGameObject() => this.gameObject;
    public Button GetClaimButton() => claimButton;
}
