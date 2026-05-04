using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;

public class EndGameUI : MonoBehaviour
{
    [Header("Panel Refrences")]
    private LevelCompletedUI levelCompleted;
    private LevelFailedUI levelFailed;
    [SerializeField] private GameObject dimImage;
    [SerializeField] private GameObject levelCompletedVFX;
    [SerializeField] private GameObject hardLevelFrame; // Khung viền đỏ cho level khó
    private UIManager uIManager;

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
        this.uIManager = uIManager;
    }
    
    public void ShowLevelCompletedPanel()
    {
        StartCoroutine(LevelCompletedCoroutine());
    }

    private IEnumerator LevelCompletedCoroutine()
    {
        AudioManager.Instance.PlayFireWorkAudio();
        levelCompletedVFX.transform.localScale = Vector3.zero;
        levelCompletedVFX.SetActive(true);

        // Tạm dừng Particle System để đợi hiệu ứng xuất hiện xong
        ParticleSystem[] particles = levelCompletedVFX.GetComponentsInChildren<ParticleSystem>();
        foreach (var ps in particles)
        {
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        levelCompletedVFX.transform.DOScale(1, 0.5f).SetEase(Ease.OutBack);

        // Chờ animation scale hoàn thành (0.5s)
        yield return new WaitForSeconds(0.5f);

        // Bắt đầu chạy pháo giấy confetti
        foreach (var ps in particles)
        {
            ps.Play();
        }

        // Đợi thêm 1.5s để xem pháo giấy (tổng thời gian delay vẫn là 2s)
        yield return new WaitForSeconds(2f);

        levelCompletedVFX.SetActive(false);
        dimImage.SetActive(true);
        levelFailed.Hide();
        levelCompleted.Show();
    }

    
    public void ShowLevelFailedPanel()
    {
        AudioManager.Instance.PlayLVLLoseAudio();
        dimImage.SetActive(true);
        levelCompleted.Hide();
        levelFailed.Show();
        
    }
    public void Hide()
    {
        dimImage.SetActive(false);
        if (hardLevelFrame != null) hardLevelFrame.SetActive(false);
        levelCompleted.Hide();
        levelFailed.Hide();
    }
}
