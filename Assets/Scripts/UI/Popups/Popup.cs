using DG.Tweening;
using UnityEngine;

public abstract class Popup : UIView
{
    // public GameObject dimImage;
    public override void Hide()
    {
        gameObject.transform.DOKill();
        gameObject.transform.DOScale(Vector3.zero,0.3f).SetEase(Ease.InBack).OnComplete(() =>
        {
            // if(dimImage != null) dimImage.SetActive(false);
            gameObject.SetActive(false);
            gameObject.transform.localScale = Vector3.one;
        });
    }
    public override void Show()
    {
        // if(dimImage != null) dimImage.SetActive(true);
        gameObject.transform.localScale = Vector3.zero;
        gameObject.SetActive(true);
        gameObject.transform.DOKill();
        gameObject.transform.DOScale(Vector3.one,0.3f).SetEase(Ease.OutBack);
    }
}
