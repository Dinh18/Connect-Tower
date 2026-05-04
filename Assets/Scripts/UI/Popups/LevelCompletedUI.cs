using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class LevelCompletedUI : MonoBehaviour, IMenu
{
    // private UIManager uIManager; // Loại bỏ phụ thuộc
    [SerializeField] private Button continueButton;
    [SerializeField] private Transform header;
    [SerializeField] private Transform coinImage;

    void OnEnable()
    {
        continueButton.onClick.AddListener(() => CoreServices.Get<GameManager>().ChangeState(GameManager.GameState.MainMenu));
    }

    void OnDisable()
    {
        continueButton.onClick.RemoveAllListeners();
    }

    public void Setup(UIManager uIManager)
    {
        // Vẫn giữ để tương thích nhưng không dùng đến
    }

    public void Show()
    {
        this.gameObject.SetActive(true);
        StartCoroutine(ShowCoroutine());
    }

    private IEnumerator ShowCoroutine()
    {
        AudioManager.Instance.PlayLVLWinAudio();
        coinImage.DOKill();
        coinImage.localScale = Vector3.zero;
        coinImage.DOScale(1, 0.5f).SetEase(Ease.OutBack);
        foreach(Transform child in header)
        {
            child.DOKill();
            child.localScale = Vector3.zero;
        }

        foreach(Transform child in header)
        {
            child.DOScale(1, 0.5f).SetEase(Ease.OutBack);
            yield return new WaitForSeconds(0.1f);
        }  
    }

    public void Hide() => this.gameObject.SetActive(false);
    public GameObject GetGameObject() => this.gameObject;
}
