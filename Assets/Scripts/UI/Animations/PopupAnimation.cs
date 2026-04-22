using DG.Tweening;
using UnityEngine;

public class PopupAnimation : MonoBehaviour
{
    public void CloseAnimation(float duration)
    {
        transform.DOScale(Vector3.one * 1.1f, duration * 0.3f) // Nở nhẹ ra
                .SetEase(Ease.OutQuad)
                .OnComplete(() => {
                    transform.DOScale(Vector3.zero, duration * 0.7f) // Co lại cực nhanh
                        .SetEase(Ease.InBack)
                        .OnComplete(() => {
                            this.gameObject.SetActive(false);
                        });
                });
    }
    public void OpenAnimation(float duration)
    {
        
    }

}
