using System.Collections;
using DG.Tweening;
using UnityEngine;

public class EndGameUI : MonoBehaviour
{
    [Header("Panel Refrences")]
    [SerializeField] GameObject levelCompletedPanel;
    [SerializeField] Transform headerLevelCompleted;
    [SerializeField] GameObject levelFailedPanel;
    
    public IEnumerator ShowLevelCompletedPanel()
    {
        AudioManager.Instance.PlayLVLWinAudio();
        this.gameObject.SetActive(true);
        levelCompletedPanel.SetActive(true);
        levelFailedPanel.SetActive(false);
        
        this.transform.localScale = Vector3.zero;
        this.transform.DOKill();
        this.transform.DOScale(1, 0.5f).SetEase(Ease.OutBack);
        foreach(Transform child in headerLevelCompleted)
        {
            child.DOKill();
            child.localScale = Vector3.zero;
            child.DOScale(1, 1f).SetEase(Ease.OutBack);
            yield return new WaitForSeconds(0.1f);
        }
        
        
    }
    public void ShowLevelFailedPanel()
    {
        AudioManager.Instance.PlayLVLLoseAudio();
        this.gameObject.SetActive(true);
        levelCompletedPanel.SetActive(false);
        levelFailedPanel.SetActive(true);
    }
    public void Hide()
    {
        this.gameObject.SetActive(false);
    }
}
