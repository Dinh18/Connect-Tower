using DG.Tweening;

using UnityEngine;
using UnityEngine.UI;

public class SettingMenu : MonoBehaviour
{
    [Header("Animation setting")]
    [SerializeField] private RectTransform settingMenu;
    [SerializeField] private float spacing = 170;
    [Header("Button references")]
    [SerializeField] private GameObject menuBlocker;
    [SerializeField] private QuitLevelPopup quitLevelPopup;
    [SerializeField] private Button settingButton;
    [SerializeField] private Button soundButton;
    [SerializeField] private Button hapticButton;
    [SerializeField] private Button homeButton;
    [SerializeField] private Button replayButton;
    private bool isExpanded = false;

    void OnEnable()
    {
        settingButton.onClick.AddListener(OnClickSetting);
        soundButton.onClick.AddListener(OnClickSoundButton);
        hapticButton.onClick.AddListener(OnClickHapticButton);
        homeButton.onClick.AddListener(OnClickBackHomeButton);
        replayButton.onClick.AddListener(OnClickTryAgain);
    }
    void OnDisable()
    {
        settingButton.onClick.RemoveListener(OnClickSetting);
        soundButton.onClick.RemoveListener(OnClickSoundButton);
        hapticButton.onClick.RemoveListener(OnClickHapticButton);
        homeButton.onClick.RemoveListener(OnClickBackHomeButton);
        replayButton.onClick.RemoveListener(OnClickTryAgain);

    }

    public void OnClickSetting()
    {
        isExpanded = !isExpanded;

            
        if(isExpanded)
        {
            OpenSettingMenu();
        }
        else
        {
            CloseSettingMenu();
        }   
    }

    public void OpenSettingMenu()
    {
        menuBlocker.SetActive(true);
        int i = 1;
        foreach(RectTransform child in settingMenu)
        {
            
            Sequence seq = DOTween.Sequence();
            child.DOKill();

            float posY = spacing * i;
            child.localScale = Vector3.zero;
            child.gameObject.SetActive(true);
            seq.Append(child.DOAnchorPosY(posY,0.1f * i).SetEase(Ease.OutBack));
            seq.Join(child.DOScale(Vector3.one, 0.1f * i).SetEase(Ease.OutBack));
            
            i++;
        }
    }

    public void CloseSettingMenu()
    {
        menuBlocker.SetActive(false);
        int i = 1;
        foreach(RectTransform child in settingMenu)
        {
            Sequence seq = DOTween.Sequence();
            child.DOKill();
            seq.Append(child.DOAnchorPosY(0,0.1f * i).SetEase(Ease.InBack));
            seq.Join(child.DOScale(Vector3.zero, 0.1f * i).SetEase(Ease.InBack));
            seq.OnComplete(() =>
            {
                child.gameObject.SetActive(false);
            });
            i++;
        }
    }

    private void OnClickSoundButton()
    {
        CoreServices.Get<AudioManager>().ToggleSound();
    }

    private void OnClickHapticButton()
    {
        HapticManager.Instance.ToggleHaptic();
    }

    private void OnClickBackHomeButton()
    {
        OnClickSetting();
        // CoreServices.Get<GameManager>().ChangeState(GameManager.GameState.MainMenu);
        if(CoreServices.Get<GameManager>().Moved())
        {
            quitLevelPopup.SetConfig(true);
            CoreServices.Get<UIManager>().ShowUI<QuitLevelPopup>();
        } 
        else CoreServices.Get<GameManager>().ChangeState(GameManager.GameState.MainMenu);
    }

    private void OnClickTryAgain()
    {
        OnClickSetting();
        if(CoreServices.Get<GameManager>().Moved())
        {
            quitLevelPopup.SetConfig(false);
            CoreServices.Get<UIManager>().ShowUI<QuitLevelPopup>();
        } 
        else CoreServices.Get<GameManager>().RestartLevel();
    }
}
