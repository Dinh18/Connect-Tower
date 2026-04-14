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
    private BoosterButton boosterButton;
    private UIManager uIManager;
    // void OnEnable()
    // {
    //     addButton.onClick.AddListener(boosterButton.OnClickAddBoosterButton);
    //     closeButton.onClick.AddListener(OnClickClose);
    // }

    // void OnDisable()
    // {
    //     addButton.onClick.RemoveListener(boosterButton.OnClickAddBoosterButton);  
    //     closeButton.onClick.RemoveListener(OnClickClose);   
    // }

    public void Setup(UIManager uIManager)
    {
        this.uIManager = uIManager;
        closeButton.onClick.RemoveAllListeners();
        closeButton.onClick.AddListener(uIManager.CloseAddBooster);
    }

    public void Show()
    {
        this.gameObject.SetActive(true);
        // GameManager.Instance.ChangeState(GameManager.GameState.Pause);
    }

    public void SetConfig(BoosterButton boosterButton ,string header, string coins, Constants.BoosterType boosterType, bool isFirstTime)
    {
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
        this.boosterButton = boosterButton;
        headerText.text = header;
        coinsText.text = coins;
        addButton.onClick.RemoveAllListeners();
        addButton.onClick.AddListener(boosterButton.OnClickAddBoosterButton);
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

    public void Hide()
    {
        // GameManager.Instance.ChangeState(GameManager.GameState.Playing);
        this.gameObject.SetActive(false);
    }

    public void OnClickClose()
    {
        uIManager.PopPopup();
    }

    public IEnumerator AddBoosterEffect(RectTransform rectTransform)
    {
        foreach(RectTransform icon in boosterIcon)
        {
            Vector3 originPos = icon.anchoredPosition;
            icon.DOKill();
            icon.gameObject.SetActive(true);
            if(boosterButton.GetBooster().GetBoosterType() == Constants.BoosterType.AddMove)
            {
                icon.GetComponent<Image>().sprite = addMoveIcon;
            }
            else if(boosterButton.GetBooster().GetBoosterType() == Constants.BoosterType.Shuffle)
            {
                icon.GetComponent<Image>().sprite = shuffleIcon;
            }
            else
            {
                icon.GetComponent<Image>().sprite = hintIcon;
            }
            icon.DOMove(rectTransform.position, 0.7f).SetEase(Ease.OutQuad).OnComplete(() =>
            {
                icon.gameObject.SetActive(false);
                icon.anchoredPosition = originPos;
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
        string instruction;
        if(boosterButton.GetBooster().GetBoosterType() == Constants.BoosterType.AddMove)
        {
            instruction = "Use the Extra Move Booster to get extra moves!";
        }
        else if(boosterButton.GetBooster().GetBoosterType() == Constants.BoosterType.Shuffle)
        {
            instruction = "Use it to shuffle the board!";
        }
        else
        {
            instruction = "Use it to reveal a correct placement";
        }
        // StartCoroutine(AddBoosterEffect(boosterButton.gameObject.GetComponent<RectTransform>()));
        TutorialManager.Instance.StartUseBoosterTutorial(boosterButton.gameObject, instruction);
        uIManager.CloseAddBooster();
    }
}
