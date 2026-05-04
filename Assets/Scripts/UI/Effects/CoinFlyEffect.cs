using UnityEngine;
using DG.Tweening;
using System;

public class CoinFlyEffect : MonoBehaviour
{
    private RectTransform rectTransform;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    // Hàm này được gọi từ Manager khi muốn thả 1 đồng xu
    public void StartBurstAndFly(Vector2 startPos, RectTransform targetUI, Action onHitTarget)
    {
        

        rectTransform.position = startPos;

        Vector3 firstPos = new Vector3(startPos.x, startPos.y - 100, rectTransform.position.z); 


        Sequence coinSequence = DOTween.Sequence();

        coinSequence.Append(rectTransform.DOMove(firstPos,0.5f).SetEase(Ease.InQuad));
        coinSequence.Append(rectTransform.DOMove(targetUI.transform.position,0.5f).SetEase(Ease.InQuad));
        coinSequence.Join(rectTransform.DOScale(0.5f, 0.5f));

        coinSequence.OnComplete(() =>
        {
            onHitTarget?.Invoke();
            if (CoinEffect.Instance != null)
            {

                rectTransform.localScale = Vector3.one; // Reset scale trước khi trả về pool
                CoinEffect.Instance.ReturnCoin(gameObject);
            }
            else
            {
                gameObject.SetActive(false); // Sơ cua nếu chưa làm Pool
            }
        });
    }
}