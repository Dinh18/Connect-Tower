using DG.Tweening;
using TMPro;
using UnityEngine; // Tech Stack TMP

public class JuicyLoadingText : MonoBehaviour
{
    public TMP_Text loadingText;
    public float interval = 0.5f; // Thời gian xuất hiện mỗi dấu chấm
    
    private string baseText = "Loading";
    private Tween _dotsTween;

    void OnEnable()
    {
        // Đảm bảo text gốc chính xác
        loadingText.text = baseText;

        // Tween một số nguyên từ 0 đến 3
        _dotsTween = DOTween.To(() => 0, x => UpdateText(x), 3, interval * 3)
            .SetEase(Ease.Linear)
            .SetLoops(-1, LoopType.Restart); // Chạy vô hạn, reset về 0
    }

    void UpdateText(int dotsCount)
    {
        switch (dotsCount)
        {
            case 1: loadingText.text = "Loading."; break;
            case 2: loadingText.text = "Loading.."; break;
            case 3: loadingText.text = "Loading..."; break;
            default: loadingText.text = "Loading"; break; // case 0
        }
    }

    void OnDisable()
    {
        // Trụ cột CPU & Memory: Luôn kill tween để tránh leak
        _dotsTween?.Kill();
    }
}