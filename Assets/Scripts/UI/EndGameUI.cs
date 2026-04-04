using UnityEngine;

public class EndGameUI : MonoBehaviour
{
    [Header("Panel Refrences")]
    [SerializeField] GameObject levelCompletedPanel;
    [SerializeField] GameObject levelFailedPanel;
    
    public void ShowLevelCompletedPanel()
    {
        this.gameObject.SetActive(true);
        levelCompletedPanel.SetActive(true);
        levelFailedPanel.SetActive(false);
    }
    public void ShowLevelFailedPanel()
    {
        this.gameObject.SetActive(true);
        levelCompletedPanel.SetActive(false);
        levelFailedPanel.SetActive(true);
    }
    public void Hide()
    {
        this.gameObject.SetActive(false);
    }
}
