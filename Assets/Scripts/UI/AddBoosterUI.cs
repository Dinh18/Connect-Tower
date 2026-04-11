using UnityEngine;
using UnityEngine.UI;

public class AddBoosterUI : MonoBehaviour
{
    [SerializeField] private Text headerText;
    [SerializeField] private Text tutorialText;
    [SerializeField] private Text coinsText;
    [SerializeField] private Image boosterIconImage;
    [SerializeField] private Button addButton;
    [SerializeField] private Sprite addMoveIcon;
    [SerializeField] private Sprite shuffleIcon;
    [SerializeField] private Sprite hintIcon;
    private BoosterButton boosterButton;
    void OnEnable()
    {
        addButton.onClick.AddListener(boosterButton.OnClickAddBoosterButton);
    }

    void OnDisable()
    {
        addButton.onClick.RemoveListener(boosterButton.OnClickAddBoosterButton);     
    }

    public void Setup(BoosterButton boosterButton)
    {
        this.boosterButton = boosterButton;
    }

    public void Show(BoosterButton boosterButton ,string header, string coins, Constants.BoosterType boosterType)
    {
        this.boosterButton = boosterButton;
        headerText.text = header;
        coinsText.text = coins;
        this.gameObject.SetActive(true);
        if(boosterType == Constants.BoosterType.AddMove)
        {
            boosterIconImage.sprite = addMoveIcon;
            tutorialText.text = "Use it to get extra moves";
        }
        else if (boosterType == Constants.BoosterType.Shuffle)
        {
            boosterIconImage.sprite = shuffleIcon;
            tutorialText.text = "Use it to shuffle the blocks";;
        }
        else
        {
            tutorialText.text = "Use it to see matching blocks";
            boosterIconImage.sprite = hintIcon;
        }
    }

    public void Hide()
    {
        this.gameObject.SetActive(false);
    }
}
