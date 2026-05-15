using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.UI;

public class QuitLevelPopup : Popup
{
    [SerializeField] private Text titleText;
    [SerializeField] private Button closeButton;
    [SerializeField] private Button continueButton;
    [SerializeField] private Button homeButton;
    [SerializeField] private Button replayButton;
    private void ClosePopup()
    {
        GameEventBus.Publish(new RequestClosePopupEvent());
    }
    void OnEnable()
    {
        closeButton.onClick.AddListener(ClosePopup);
        continueButton.onClick.AddListener(ClosePopup);
        homeButton.onClick.AddListener(OnClickHome);
        replayButton.onClick.AddListener(OnClickReplay);
    }
    void OnDisable()
    {
        closeButton.onClick.RemoveListener(ClosePopup);
        continueButton.onClick.RemoveListener(ClosePopup);
        homeButton.onClick.RemoveListener(OnClickHome);
        replayButton.onClick.RemoveListener(OnClickReplay);
    }


    public override void Hide()
    {
        base.Hide();
    }

    public override void Show()
    {
        base.Show();
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
        CoreServices.Get<UIManager>().OnClickBackHome();
        CoreServices.Get<HeartManager>().UseHeart();
    }


}
