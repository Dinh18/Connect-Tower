using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum ProfileType
{
    Frame,
    Avatar,
}

public class EditProfilePopup : Popup
{
    [Header("UI References")]
    [SerializeField] private Button closeButton;
    [SerializeField] private Button saveButton;
    [SerializeField] private InputField editNameText;
    [SerializeField] private Image avatarImage;
    [SerializeField] private Image frameImage;
    [Header("Item holder")]
    [SerializeField] private ToggleGroup frameList;
    [SerializeField] private ToggleGroup avatarList;
    [Header("Toggles group")]
    [SerializeField] private Toggle frameTab;
    [SerializeField] private Toggle avatarTab;
    [SerializeField] private ProfileItemUI frameUIPrefabs;
    [SerializeField] private ProfileItemUI avatarUIPrefabs;
    private List<ProfileItemUI> allFramesUI;
    private List<ProfileItemUI> allAvatarsUI;
    private FrameDataSO selectedFrame;
    private AvatarDataSO selectedAvatar;
    private FrameDataSO originalFrame;
    private AvatarDataSO originalAvatar;
    private string originalName;
    void Start()
    {
        Setup();
    }
    private void ClosePopup()
    {
        CoreServices.Get<UIManager>().PopUI();
    }
    void OnEnable()
    {
        closeButton.onClick.AddListener(ClosePopup);
        saveButton.onClick.AddListener(OnClickSave);
        editNameText.onValueChanged.AddListener(OnNameChanged);
        
        avatarTab.onValueChanged.RemoveAllListeners();
        frameTab.onValueChanged.RemoveAllListeners();

        ConfigContent();

        OnTabChanged(ProfileType.Frame,true);

        frameTab.onValueChanged.AddListener((isOn) => OnTabChanged(ProfileType.Frame, isOn));
        avatarTab.onValueChanged.AddListener((isOn) => OnTabChanged(ProfileType.Avatar, isOn));
    }
    void OnDisable()
    {
        closeButton.onClick.RemoveListener(ClosePopup);
        saveButton.onClick.RemoveListener(OnClickSave);
        editNameText.onValueChanged.RemoveListener(OnNameChanged);
    }

    private void Setup()
    {
        DataManager dataManager = CoreServices.Get<DataManager>();
        allFramesUI = new List<ProfileItemUI>();
        allAvatarsUI = new List<ProfileItemUI>();
        // setup Frame
        foreach(var avatar in dataManager.GetAllAvatarData())
        {
            ProfileItemUI profileItem = Instantiate(avatarUIPrefabs, avatarList.transform);
            profileItem.Setup(avatar, avatarList);
            profileItem.GetComponent<Toggle>().onValueChanged.RemoveAllListeners();
            profileItem.GetComponent<Toggle>().onValueChanged.AddListener((isOn) => OnChangeProfile(ProfileType.Avatar, profileItem.itemData, isOn));
            allAvatarsUI.Add(profileItem);
        }
        // setup avatar
        foreach(var frame in dataManager.GetAllFrameData())
        {
            ProfileItemUI profileItem = Instantiate(frameUIPrefabs, frameList.transform);
            profileItem.Setup(frame, frameList);
            profileItem.GetComponent<Toggle>().onValueChanged.RemoveAllListeners();
            profileItem.GetComponent<Toggle>().onValueChanged.AddListener((isOn) => OnChangeProfile(ProfileType.Frame, profileItem.itemData, isOn));
            allFramesUI.Add(profileItem);
        }
        OnTabChanged(ProfileType.Frame,true);
    }

    private void ConfigContent()
    {
        originalFrame = CoreServices.Get<DataManager>().GetCurrFrame();
        originalAvatar = CoreServices.Get<DataManager>().GetCurrAvatar();
        originalName = CoreServices.Get<DataManager>().GetPlayerName();
        
        editNameText.text = originalName;
        selectedFrame = originalFrame;
        selectedAvatar = originalAvatar;
        
        avatarImage.sprite = originalAvatar.itemSprite;
        frameImage.sprite = originalFrame.itemSprite;

        if(allFramesUI != null) SelectItem<FrameDataSO>(allFramesUI, originalFrame);
        if(allAvatarsUI != null) SelectItem<AvatarDataSO>(allAvatarsUI, originalAvatar);

        OnTabChanged(ProfileType.Frame,true);
        CheckSaveButtonInteractable();
    }

    public void SelectItem<T>(List<ProfileItemUI> allItemsUI, T currItem) where T : ProfileItemData
    {
        foreach(ProfileItemUI profileItem in allItemsUI)
        {
            if(currItem.id == profileItem.itemData.id)
            {
                profileItem.selectedToggle.isOn = true;
            }
        }
    }

    private void OnTabChanged(ProfileType profileType, bool isOn)
    {
        if(isOn)
        {
            avatarList.gameObject.SetActive(false);
            frameList.gameObject.SetActive(false);
            switch (profileType)
            {
                case ProfileType.Frame:
                    frameList.gameObject.SetActive(true);
                    break;
                case ProfileType.Avatar:
                    avatarList.gameObject.SetActive(true);
                    break;
                default:
                    frameList.gameObject.SetActive(true);
                    break;
            }
        }
    }

    private void OnChangeProfile(ProfileType profileType, ProfileItemData itemData, bool isOn)
    {
        if(isOn)
        {
            if(profileType == ProfileType.Frame)
            {
                frameImage.sprite = itemData.itemSprite;
                selectedFrame = itemData as FrameDataSO;
            }
            else if(profileType == ProfileType.Avatar)
            {
                avatarImage.sprite = itemData.itemSprite;
                selectedAvatar = itemData as AvatarDataSO;
            }
            CheckSaveButtonInteractable();
        }
    }

    private void OnNameChanged(string newName)
    {
        CheckSaveButtonInteractable();
    }

    private void CheckSaveButtonInteractable()
    {
        bool hasChanged = false;
        if (editNameText.text != originalName) hasChanged = true;
        if (selectedFrame != null && originalFrame != null && selectedFrame.id != originalFrame.id) hasChanged = true;
        if (selectedAvatar != null && originalAvatar != null && selectedAvatar.id != originalAvatar.id) hasChanged = true;
        saveButton.interactable = hasChanged;
    }

    private void OnClickSave()
    {
        if(selectedFrame == null) selectedFrame = CoreServices.Get<DataManager>().GetCurrFrame();
        if(selectedAvatar == null) selectedAvatar = CoreServices.Get<DataManager>().GetCurrAvatar();
        GameEventBus.Publish(new RequestSaveProfile{playerName = editNameText.text, frameID = selectedFrame.id, avatarID = selectedAvatar.id});
        CoreServices.Get<UIManager>().PopUI();
    }
}
