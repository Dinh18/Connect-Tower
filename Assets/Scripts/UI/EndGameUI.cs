using System.Collections;
using DG.Tweening;
using UnityEngine;

public class EndGameUI : MonoBehaviour
{
    [Header("Panel Refrences")]
    private LevelCompletedUI levelCompleted;
    private LevelFailedUI levelFailed;

    void Awake()
    {
        InitPanels();
    }

    private void InitPanels()
    {
        if (levelCompleted == null) 
            levelCompleted = GetComponentInChildren<LevelCompletedUI>(true);
            
        if (levelFailed == null) 
            levelFailed = GetComponentInChildren<LevelFailedUI>(true);
    }

    public void Setup(UIManager uIManager)
    {
        InitPanels(); 

        if (levelCompleted == null || levelFailed == null)
        {
            Debug.LogError("LỖI: Không tìm thấy LevelCompletedUI hoặc LevelFailedUI. Hãy kiểm tra lại cấu trúc Hierarchy xem chúng có đang nằm làm CON của EndGameUI chưa!");
            return;
        }

        levelCompleted.Setup(uIManager);
        levelFailed.Setup(uIManager);
    }
    
    public void ShowLevelCompletedPanel()
    {

        levelFailed.Hide();

        ShowDimImage();

        levelCompleted.Show();
        
    }
    
    public void ShowLevelFailedPanel()
    {
        AudioManager.Instance.PlayLVLLoseAudio();
        ShowDimImage();
        levelCompleted.Hide();
        levelFailed.Show();
    }
    private void ShowDimImage()
    {
        this.gameObject.SetActive(true);
        this.transform.localScale = Vector3.zero;
        this.transform.DOKill();
        this.transform.DOScale(1, 0.5f).SetEase(Ease.OutBack);
    }
    public void Hide()
    {
        this.gameObject.SetActive(false);
    }
}
