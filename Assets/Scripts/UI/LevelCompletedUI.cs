using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class LevelCompletedUI : MonoBehaviour, IMenu
{
    private UIManager uIManager;
    [SerializeField] private Button continueButton;
    [SerializeField] private Transform header;
    void OnEnable()
    {
        continueButton.onClick.AddListener(uIManager.OnClickBackHome);
    }
    void OnDisable()
    {
        continueButton.onClick.RemoveListener(uIManager.OnClickBackHome);
    }

    public void Hide()
    {
        this.gameObject.SetActive(false);
    }

    public void Setup(UIManager uIManager)
    {
        this.uIManager = uIManager;
    }

    public void Show()
    {
        this.gameObject.SetActive(true);
        StartCoroutine(ShowCoroutine());
    }

    private IEnumerator ShowCoroutine()
    {
        AudioManager.Instance.PlayLVLWinAudio();

        foreach(Transform child in header)
        {
            child.DOKill();
            child.localScale = Vector3.zero;
            child.DOScale(1, 1f).SetEase(Ease.OutBack);
            yield return new WaitForSeconds(0.1f);
        } 
        
    }

    public GameObject GetGameObject()
    {
        return this.gameObject;
    }
}
