using UnityEngine;
using UnityEngine.UI;

public class GeneralStatsPopup : Popup
{
    [SerializeField] private Button closeButton;
    [SerializeField] private Button editProfile;
    [SerializeField] private Image frameImage;
    [SerializeField] private Image avatarImage;
    [SerializeField] private Text nameText;
    [SerializeField] private Text currStreakText;
    [SerializeField] private Text maxStreakText;
    
    void OnEnable()
    {
        currStreakText.text = CoreServices.Get<DataManager>().GetCurrStreak().ToString();
        maxStreakText.text = CoreServices.Get<DataManager>().GetMaxStreak().ToString();
        closeButton.onClick.AddListener(ClosePopup);
        editProfile.onClick.AddListener(OpenEditProfile);
        GameEventBus.UnSubscribe<RequestSaveProfile>(ChangeProfile);
        GameEventBus.Subscribe<RequestSaveProfile>(ChangeProfile);
    }
    void OnDisable()
    {
        closeButton.onClick.RemoveListener(ClosePopup);
        editProfile.onClick.RemoveListener(OpenEditProfile);
    }
    void Start()
    {
        avatarImage.sprite = CoreServices.Get<DataManager>().GetCurrAvatar().itemSprite;
        frameImage.sprite = CoreServices.Get<DataManager>().GetCurrFrame().itemSprite;
        nameText.text = CoreServices.Get<DataManager>().GetPlayerName();
    }
    private void ClosePopup()
    {
        GameEventBus.Publish(new RequestClosePopupEvent{});
    }
    private void OpenEditProfile()
    {
        GameEventBus.Publish(new RequestOpenPopupEvent{targetPopup = PopupType.EditProfile});
    }

    private void ChangeProfile(RequestSaveProfile evt)
    {
        nameText.text = evt.playerName;
        frameImage.sprite = CoreServices.Get<DataManager>().GetFrameByID(evt.frameID).itemSprite;
        avatarImage.sprite = CoreServices.Get<DataManager>().GetAvatarByID(evt.avatarID).itemSprite;
    }
    
}
