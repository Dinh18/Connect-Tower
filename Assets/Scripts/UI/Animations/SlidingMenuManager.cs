using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class SlidingMenuManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private RectTransform contentRect;       // Kéo object 'Content' vào đây
    [SerializeField] private RectTransform safeAreaRect;      // Kéo 'SafeAreaPanel' (hoặc Viewport) vào đây
    [SerializeField] private LayoutElement[] panelLayouts;    // Kéo 3 panel (Shop, Home, Leaderboard) vào mảng này

    [Header("Animation Settings")]
    [SerializeField] private float slideDuration = 0.4f;
    [SerializeField] private Ease slideEase = Ease.OutQuint;
    private float currentPanelWidth;

    IEnumerator Start()
    {
        yield return new WaitForEndOfFrame();
        SetupResponsivePanels();
        float targetX = -(1 * currentPanelWidth);
        Canvas.ForceUpdateCanvases();

        Debug.Log("Khóa vị trí Home tại: " + targetX);
        
        contentRect.DOKill();
        contentRect.DOAnchorPosX(targetX, 0f);
    }

    // Hàm đo đạc và ép kích thước Panel để phủ kín toàn màn hình
    private void SetupResponsivePanels()
    {
        // 1. Lấy Root Canvas để đảm bảo kích thước chính xác 100% toàn màn hình
        Canvas canvas = GetComponentInParent<Canvas>().rootCanvas;
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();

        // Sử dụng kích thước toàn màn hình
        currentPanelWidth = canvasRect.rect.width;
        float currentPanelHeight = canvasRect.rect.height;

        // 2. Ép tất cả các Panel con bằng đúng kích thước màn hình
        foreach (var layout in panelLayouts)
        {
            layout.gameObject.SetActive(true);
            layout.minWidth = currentPanelWidth;
            layout.minHeight = currentPanelHeight;
        }

        // 3. TUYỆT CHIÊU: Ép Unity tính toán lại UI ngay lập tức
        LayoutRebuilder.ForceRebuildLayoutImmediate(contentRect);
    }

    public void GoToTab(int tabIndex)
    {
        // Tính toán khoảng cách trượt
        float targetX = -(tabIndex * currentPanelWidth);

        contentRect.DOKill();
        contentRect.DOAnchorPosX(targetX, slideDuration).SetEase(slideEase);
    }

#if UNITY_EDITOR
    // [Chỉ chạy trên Editor] Nếu bạn đổi Simulator sang máy khác, UI tự động nắn lại không cần Play lại
    private void Update()
    {
        if (!Application.isPlaying && safeAreaRect != null)
        {
            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas != null)
            {
                RectTransform canvasRect = canvas.GetComponent<RectTransform>();
                if (currentPanelWidth != canvasRect.rect.width)
                {
                    SetupResponsivePanels();
                }
            }
        }
    }
#endif
}
