using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class FloatingNotifier : MonoBehaviour
{
    [SerializeField] private CanvasGroup warningCanvasGroup;
    [SerializeField] private RectTransform warningRect;
    [SerializeField] private Text warningText;
    private float floatDistance = 150f; // Khoảng cách bay lên
    private float animationTime = 1.5f; // Thời gian bay và mờ đi (1.5 giây)
    private Vector2 originalPos;

    void Awake()
    {
        originalPos = warningRect.anchoredPosition;
        Debug.Log(originalPos);
    }

    public void ShowWarning(string warningMesh)
    {
        warningText.text = warningMesh;
        warningCanvasGroup.DOKill();
        warningRect.DOKill();

        warningCanvasGroup.gameObject.SetActive(true);

        warningCanvasGroup.alpha = 1f; 
        warningRect.anchoredPosition = originalPos;

        warningRect.DOAnchorPosY(floatDistance, animationTime).SetRelative(true).SetEase(Ease.OutQuad);


        warningCanvasGroup.DOFade(0f, animationTime).SetEase(Ease.InCubic).OnComplete(() => 
        {
            warningCanvasGroup.gameObject.SetActive(false);
        });

    }
}
