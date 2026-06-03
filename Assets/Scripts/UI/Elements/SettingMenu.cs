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
    [SerializeField] private Button musicButton;
    [SerializeField] private Button homeButton;
    [SerializeField] private Button replayButton;
    private bool isExpanded = false;
    private bool isAnimating = false;

    void OnEnable()
    {
        settingButton.onClick.AddListener(OnClickSetting);
        soundButton.onClick.AddListener(OnClickSoundButton);
        hapticButton.onClick.AddListener(OnClickHapticButton);
        musicButton.onClick.AddListener(OnClickMusicButton);
        homeButton.onClick.AddListener(OnClickBackHomeButton);
        replayButton.onClick.AddListener(OnClickTryAgain);
    }
    void OnDisable()
    {
        settingButton.onClick.RemoveListener(OnClickSetting);
        soundButton.onClick.RemoveListener(OnClickSoundButton);
        musicButton.onClick.RemoveListener(OnClickMusicButton);
        hapticButton.onClick.RemoveListener(OnClickHapticButton);
        homeButton.onClick.RemoveListener(OnClickBackHomeButton);
        replayButton.onClick.RemoveListener(OnClickTryAgain);

    }

    public void OnClickSetting()
    {
        if (isAnimating) return;
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
        isAnimating = true;
        menuBlocker.SetActive(true);
        int i = 1;
        int childCount = settingMenu.childCount;
        foreach(RectTransform child in settingMenu)
        {
            
            Sequence seq = DOTween.Sequence();
            child.DOKill();

            float posY = spacing * i;
            child.localScale = Vector3.zero;
            child.gameObject.SetActive(true);
            seq.Append(child.DOAnchorPosY(posY,0.1f * i).SetEase(Ease.OutBack));
            seq.Join(child.DOScale(Vector3.one, 0.1f * i).SetEase(Ease.OutBack));
            
            if (i == childCount)
            {
                seq.OnComplete(() => isAnimating = false);
            }
            
            i++;
        }
        if (childCount == 0) isAnimating = false;
    }

    public void CloseSettingMenu()
    {
        isAnimating = true;
        menuBlocker.SetActive(false);
        int i = 1;
        int childCount = settingMenu.childCount;
        foreach(RectTransform child in settingMenu)
        {
            Sequence seq = DOTween.Sequence();
            child.DOKill();
            seq.Append(child.DOAnchorPosY(0,0.1f * i).SetEase(Ease.InBack));
            seq.Join(child.DOScale(Vector3.zero, 0.1f * i).SetEase(Ease.InBack));
            bool isLast = (i == childCount);
            seq.OnComplete(() =>
            {
                child.gameObject.SetActive(false);
                if (isLast) isAnimating = false;
            });
            i++;
        }
        if (childCount == 0) isAnimating = false;
    }

    private void OnClickSoundButton()
    {
        if (isAnimating) return;
        CoreServices.Get<AudioManager>().ToggleSound();
    }

    private void OnClickHapticButton()
    {
        if (isAnimating) return;
        CoreServices.Get<HapticManager>().ToggleHaptic();
    }

    private void OnClickMusicButton()
    {
        if (isAnimating) return;
        CoreServices.Get<AudioManager>().ToggleMusic();
    }

    private void OnClickBackHomeButton()
    {
        if (isAnimating) return;
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
        if (isAnimating) return;
        OnClickSetting();
        if(CoreServices.Get<GameManager>().Moved())
        {
            quitLevelPopup.SetConfig(false);
            CoreServices.Get<UIManager>().ShowUI<QuitLevelPopup>();
        } 
        else CoreServices.Get<GameManager>().RestartLevel();
    }
}
