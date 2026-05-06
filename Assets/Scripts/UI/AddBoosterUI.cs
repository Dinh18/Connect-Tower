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
    [SerializeField] private List<RectTransform> boosterIcon;
    [SerializeField] private GameObject dimImage;
    private BoosterButton boosterButton;
    private Constants.BoosterType boosterType;

    public void Setup(UIManager uIManager)
    {
        closeButton.onClick.RemoveAllListeners();
        closeButton.onClick.AddListener(() => GameEventBus.Publish(new RequestCloseBoosterPopupEvent()));
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
        
        if(boosterType == Constants.BoosterType.AddMove)
        {
            boosterIconImage.sprite = addMoveIcon;
            tutorialText.text = "Use it to get extra moves";
        }
        else if (boosterType == Constants.BoosterType.Shuffle)
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
        GameEventBus.Publish(new RequestCloseBoosterPopupEvent());
    }

    public IEnumerator AddBoosterEffect(RectTransform rectTransform)
    {
        foreach(RectTransform icon in boosterIcon)
        {
            Vector3 originPos = icon.anchoredPosition;
            icon.DOKill();
            icon.gameObject.SetActive(true);
            if(boosterButton.GetBooster().GetBoosterType() == Constants.BoosterType.AddMove)
                icon.GetComponent<Image>().sprite = addMoveIcon;
            else if(boosterButton.GetBooster().GetBoosterType() == Constants.BoosterType.Shuffle)
                icon.GetComponent<Image>().sprite = shuffleIcon;
            else
                icon.GetComponent<Image>().sprite = hintIcon;

            icon.DOMove(rectTransform.position, 0.7f).SetEase(Ease.OutQuad).OnComplete(() =>
            {
                icon.gameObject.SetActive(false);
                icon.anchoredPosition = originPos;
                AudioManager.Instance.PlayAddBoosterAudio();
            });
            yield return new WaitForSeconds(0.1f);
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
