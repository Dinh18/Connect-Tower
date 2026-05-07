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
        // Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);

        rectTransform.position = startPos;

        Vector3 firstPos = new Vector3(startPos.x - 50, startPos.y - 100, rectTransform.position.z); 

        Vector3[] path = new Vector3[] { 
            rectTransform.position, 
            firstPos, 
            targetUI.transform.position 
        };

        // DOPath sẽ tự nối các điểm thành một đường cong (CatmullRom)
        rectTransform.DOPath(path, 0.8f, PathType.CatmullRom)
                    .SetEase(Ease.InQuad)
                    .OnComplete(() => {
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