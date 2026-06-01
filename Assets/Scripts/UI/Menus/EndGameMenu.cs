using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;

public class EndGameMenu : Menu
{
    [Header("Panel Refrences")]
    [SerializeField] private LevelCompletedPanel levelCompleted;
    [SerializeField] private LevelFailedPanel levelFailed;
    [SerializeField] private GameObject dimImage;
    [SerializeField] private GameObject levelCompletedVFX;
    public override void Show()
    {
        base.Show();
        if(CoreServices.Get<GameManager>().GetCurrState() == GameManager.GameState.Win) ShowLevelCompletedPanel();
        else if(CoreServices.Get<GameManager>().GetCurrState() == GameManager.GameState.Lose) ShowLevelFailedPanel();
    }

    public override void Hide()
    {
        base.Hide();
        dimImage.SetActive(false);
        levelCompleted.Hide();
        levelFailed.Hide();
    }
    
    public void ShowLevelCompletedPanel()
    {
        StartCoroutine(LevelCompletedCoroutine());
    }

    private IEnumerator LevelCompletedCoroutine()
    {
        // AudioManager.Instance.PlayFireWorkAudio();
        GameEventBus.Publish(new RequestPlaySFX{soundID = SoundID.FireWork});
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
        
        // levelCompleted.Show();
        dimImage.SetActive(true);
        levelFailed.Hide();
        levelCompleted.Show();
    }

    
    public void ShowLevelFailedPanel()
    {
        GameEventBus.Publish(new RequestPlaySFX{soundID = SoundID.LevelLose});
        dimImage.SetActive(true);
        levelCompleted.Hide();
        levelFailed.Show();
    }
    
}
