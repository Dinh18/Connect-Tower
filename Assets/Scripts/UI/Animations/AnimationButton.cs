using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

public class AnimationButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [Header("Độ lún")]
    private float pushScale = 0.9f;
    
    [Header("Thời gian lún và trở lại bình thường")]
    // LƯU Ý: pushDuration nên để rất nhỏ (vd: 0.05f) để nút lún xuống ngay lập tức, 
    // nếu để 0.5f thì nửa giây sau nó mới bắt đầu nảy (punch), cảm giác sẽ bị delay (lag).
    private float pushDuration = 0.05f; 
    private float punchDuration = 0.5f;
    private float releaseDuration = 0.05f;
    
    private int vibrato = 8;
    private float elasticity = 0.5f;
    private Vector3 originalScale;
    
    // Lực ép xuống (Giá trị âm để nút lõm vào)
    public Vector3 punchForceDown = new Vector3(-0.15f, -0.15f, 0f);
    
    // Lực nảy lên (Giá trị dương để nút phồng ra)
    public Vector3 punchForceUp = new Vector3(0.15f, 0.15f, 0f);

    void Awake()
    {
        originalScale = transform.localScale;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        AudioManager.Instance.PlayButtonDownAudio();
        transform.DOKill();
        // HapticManager.Instance.PlayHaptic();
        
        // SỬA LỖI TEOTÓP: Luôn lấy originalScale làm gốc
        Vector3 targetPushScale = originalScale * pushScale;

        // Đặt SetUpdate(true) ở Sequence để toàn bộ chuỗi không bị ảnh hưởng nếu game pause
        Sequence sequence = DOTween.Sequence().SetUpdate(true);

        // 1. Ép nhanh về size nhỏ
        sequence.Append(transform.DOScale(targetPushScale, pushDuration));
        // 2. Punch tại vị trí size nhỏ đó
        sequence.Append(transform.DOPunchScale(punchForceDown, punchDuration, vibrato, elasticity));
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // AudioManager.Instance.PlayButtonAudio();
        AudioManager.Instance.PlayButtonUpAudio();
        transform.DOKill();
        
        Sequence sequence = DOTween.Sequence().SetUpdate(true);

        // 1. Nở nhanh về size gốc
        sequence.Append(transform.DOScale(originalScale, releaseDuration).SetEase(Ease.OutQuad));
        // 2. Punch phồng ra tại size gốc (Dùng biến punchForceUp bạn đã khai báo)
        sequence.Append(transform.DOPunchScale(punchForceUp, punchDuration, vibrato, elasticity));
    }

    void OnDisable()
    {
        transform.DOKill();
        transform.localScale = originalScale;
    }
}