using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class BottomPanel : MonoBehaviour
{
    private BoosterButton[] boosterButtons;
    [SerializeField] private AddBoosterUI addBoosterUI;
    [SerializeField] private List<RectTransform> boosterIcon;
    private Vector2 originPos;
    void Awake()
    {
        boosterButtons = GetComponentsInChildren<BoosterButton>(true);
        originPos = GetComponent<RectTransform>().anchoredPosition;
    }

    void OnEnable()
    {
        GameEventBus.Subscribe<RequestOpenBoosterPopupEvent>(OnOpenAddBoosterPopup);
        GameEventBus.Subscribe<RequestAddBoosterEffectEvent>(OnPlayAddBoosterEffect);
    }

    void OnDisable()
    {
        GameEventBus.UnSubscribe<RequestOpenBoosterPopupEvent>(OnOpenAddBoosterPopup);
        GameEventBus.UnSubscribe<RequestAddBoosterEffectEvent>(OnPlayAddBoosterEffect);
    }

    public void Show()
    {
        this.gameObject.SetActive(true);
        
        RectTransform rect = GetComponent<RectTransform>();
        rect.DOKill();
        rect.anchoredPosition = new Vector2(originPos.x, originPos.y - 500f);
        rect.DOAnchorPosY(originPos.y, 0.5f).SetEase(Ease.OutBack);

        if(boosterButtons == null) Debug.Log("Chua co booster button nao");
        if (boosterButtons != null)
        {
            foreach(var booster in boosterButtons)
            {
                booster.Show();
            }
        }
    }

    public void Hide()
    {
        RectTransform rect = GetComponent<RectTransform>();
        rect.DOKill();
        rect.DOAnchorPosY(originPos.y - 500f, 0.3f).SetEase(Ease.InBack).OnComplete(() =>
        {
            this.gameObject.SetActive(false);
            rect.anchoredPosition = originPos;
        });
    }

    private void OnOpenAddBoosterPopup(RequestOpenBoosterPopupEvent requestOpenBoosterPopup)
    {
        if (addBoosterUI == null) return;
        
        addBoosterUI.SetConfig(requestOpenBoosterPopup);
        CoreServices.Get<UIManager>().ShowUI<AddBoosterUI>();
        
        BoosterButton matchingBoosterButton = null;
        if (boosterButtons != null)
        {
            foreach (var bb in boosterButtons)
            {
                if (bb.GetBooster().GetBoosterType() == requestOpenBoosterPopup.type)
                {
                    matchingBoosterButton = bb;
                    break;
                }
            }
        }

        if (matchingBoosterButton != null)
        {
            addBoosterUI.SetupButton(matchingBoosterButton);

            DataManager dataManager = CoreServices.Get<DataManager>();
            int boosterID = (int)requestOpenBoosterPopup.type;
            if (dataManager.IsFirstTimeUserBooster(boosterID))
            {
                var tutorialService = CoreServices.Get<TutorialService>();
                if (tutorialService != null)
                {
                    string useInstruction = "";
                    if (requestOpenBoosterPopup.type == BoosterType.AddMove)
                        useInstruction = "Use the Extra Move to get extra moves!";
                    else if (requestOpenBoosterPopup.type == BoosterType.Shuffle)
                        useInstruction = "Use it to shuffle the board!";
                    else
                        useInstruction = "Use it to reveal a correct placement";

                    tutorialService.StartBoosterTutorial(addBoosterUI.GetClaimButton(), matchingBoosterButton.GetComponent<Button>(), "Claim your free booster!", useInstruction);
                }
            }
        }
    }

    private IEnumerator AddBoosterEffect(RectTransform boosterTransform, Sprite sprite)
    {
        if (boosterIcon == null) yield break;
        
        foreach(RectTransform icon in boosterIcon)
        {
            Vector3 originPos = icon.anchoredPosition;
            icon.DOKill();
            icon.gameObject.SetActive(true);
            
            icon.gameObject.GetComponent<Image>().sprite = sprite;

            icon.DOMove(boosterTransform.position, 0.7f).SetEase(Ease.OutQuad).OnComplete(() =>
            {
                icon.gameObject.SetActive(false);
                icon.anchoredPosition = originPos;
                GameEventBus.Publish(new RequestPlaySFX{soundID = SoundID.AddBooster});
            });
            yield return new WaitForSeconds(0.1f);
        }
    }

    public void OnPlayAddBoosterEffect(RequestAddBoosterEffectEvent evt)
    {
        StartCoroutine(AddBoosterEffect(evt.boosterTransfrom, evt.spriteIcon));
    }
}
