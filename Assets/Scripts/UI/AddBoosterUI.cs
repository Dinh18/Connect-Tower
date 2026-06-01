using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;


public class AddBoosterUI : Popup
{
    [SerializeField] private Text headerText;
    [SerializeField] private Text tutorialText;
    [SerializeField] private Text coinsText;
    [SerializeField] private Image boosterIconImage;
    [SerializeField] private Button addButton;
    // [SerializeField] private Button claimButton;
    [SerializeField] private Button closeButton;
    [SerializeField] private Sprite addMoveIcon;
    [SerializeField] private Sprite shuffleIcon;
    [SerializeField] private Sprite hintIcon;
    [SerializeField] private Sprite undoIcon;
    
    private RectTransform boosterTransform;
    private BoosterButton boosterButton;
    private BoosterType boosterType;

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

    public override void Show()
    {
        base.Show();
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

        // if(isFirstTime)
        // {
        //     claimButton.gameObject.SetActive(true);
        //     addButton.gameObject.SetActive(false);
        //     closeButton.gameObject.SetActive(false);
        // }
        // else
        // {
            // claimButton.gameObject.SetActive(false);
            addButton.gameObject.SetActive(true);
            closeButton.gameObject.SetActive(true);
        // }

        headerText.text = header;
        coinsText.text = coins.ToString();
        
        // addButton.onClick.RemoveAllListeners();
        // addButton.onClick.AddListener(boosterButton.OnClickAddBoosterButton);
        
        // claimButton.onClick.RemoveAllListeners();
        // claimButton.onClick.AddListener(OnClickClaim);
        
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
        else if(boosterType == BoosterType.Hint)
        {
            tutorialText.text = "Use it to see matching blocks";
            boosterIconImage.sprite = hintIcon;
        }
        else if(boosterType == BoosterType.Undo)
        {
            tutorialText.text = "Use it to back previous step";
            boosterIconImage.sprite = undoIcon;
        }
    }

    public void SetupBoosterButton(AddBoosterEvent addBoosterEvent)
    {
        this.boosterButton = addBoosterEvent.boosterButton;
    }

    public override void Hide()
    {
        base.Hide();
    }

    public void OnClickClose()
    {
        CoreServices.Get<UIManager>().PopUI();
        Debug.Log("Gửi event đóng add booster popup");
    }

    

    public void OnClickBuyBooster()
    {
        if(boosterButton.GetBooster().GetPrice() > CoreServices.Get<DataManager>().GetTotalCoins())
        {
            CoreServices.Get<UIManager>().OpenShop();
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
        
        dataManager.AddBooster(id, 1, true);
        // dataManager.UsedBooster(id);
        OnClickClose();
    }

    // public Button GetClaimButton() => claimButton;


}
