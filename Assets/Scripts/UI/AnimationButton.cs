using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

public class AnimationButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [Header("Độ lún")]
    [SerializeField] private float pushScale = 0.9f;
    [Header("Thời gian lún và trở lại bình thường")]
    [SerializeField] private float pushDuration = 0.1f;
    [SerializeField] private float releaseDuration = 0.25f;
    private Vector3 originalScale;
    void Awake()
    {
        originalScale = transform.localScale;
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        
        transform.DOKill();
        transform.DOScale(transform.localScale * pushScale, pushDuration).SetEase(Ease.OutQuad).SetUpdate(true);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        AudioManager.Instance.PlayButtonAudio();
        HapticManager.Instance.PlayVibrateMedium();
        transform.DOKill();
        transform.DOScale(originalScale, releaseDuration).SetEase(Ease.OutQuad).SetUpdate(true);
    }
    void OnDisable()
    {
        transform.DOKill();
        transform.localScale = originalScale;
    }
}
