using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.UI;

public class QuitLevelPopup : MonoBehaviour, IMenu
{
    [SerializeField] private GameObject dimImage;
    [SerializeField] private Text titleText;
    [SerializeField] private Button closeButton;
    [SerializeField] private Button continueButton;
    [SerializeField] private Button homeButton;
    [SerializeField] private Button replayButton;
    void OnEnable()
    {
        closeButton.onClick.AddListener(() => GameEventBus.Publish(new RequestClosePopupEvent()));
        continueButton.onClick.AddListener(() => GameEventBus.Publish(new RequestClosePopupEvent()));
        homeButton.onClick.AddListener(OnClickHome);
        replayButton.onClick.AddListener(OnClickReplay);
    }
    void OnDisable()
    {
        closeButton.onClick.RemoveListener(() => GameEventBus.Publish(new RequestClosePopupEvent()));
        continueButton.onClick.RemoveListener(() => GameEventBus.Publish(new RequestClosePopupEvent()));
        homeButton.onClick.RemoveListener(OnClickHome);
        replayButton.onClick.RemoveListener(OnClickReplay);
    }

    
    public GameObject GetGameObject()
    {
        return this.gameObject;
    }

    public void Hide()
    {
        this.gameObject.SetActive(false);
        dimImage.SetActive(false);
    }

    

    public void Setup(UIManager uIManager)
    {
        
    }

    public void Show()
    {
        this.gameObject.SetActive(true);
        dimImage.SetActive(true);
    }

    public void SetConfig(bool isBackHome)
    {
        if(isBackHome)
        {
            titleText.text = "Quit Level";
            homeButton.gameObject.SetActive(true);
            replayButton.gameObject.SetActive(false);
        }
        else
        {
            titleText.text = "Replay";
            homeButton.gameObject.SetActive(false);
            replayButton.gameObject.SetActive(true);
        }
    }

    private void OnClickReplay()
    {
        CoreServices.Get<UIManager>().OnClickTryAgain();
    }

    public void OnClickHome()
    {
        CoreServices.Get<GameManager>().ChangeState(GameManager.GameState.MainMenu);
        CoreServices.Get<HeartManager>().UseHeart();
    }
}
