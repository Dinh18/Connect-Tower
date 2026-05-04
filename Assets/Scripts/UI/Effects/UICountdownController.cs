using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class UICountdownController : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] float countdownTime = 10f; // Thời gian đếm ngược tổng cộng
    [SerializeField] bool startOnEnable = true; // Bắt đầu khi object được active

    private Image _image;// Tỷ lệ fill của Image (0 = đóng, 1 = mở hoàn toàn)
    private float _currentTime;
    private bool _isRunning = false;    
    void Awake()
    {
        // Lấy Image và Material để điều khiển
        _image = GetComponent<Image>();
    }

    void Update()
    {
        if (!_isRunning) return;

        _currentTime -= Time.deltaTime;

        // Tính toán tỷ lệ "đã mở"
        // Ta muốn bắt đầu từ đóng (Progress = 0) và mở dần (Progress = 1)
        // theo thời gian đếm ngược (ví dụ từ 10s về 0s).
        float progressAmount = (_currentTime / countdownTime);
        _image.fillAmount = progressAmount;

        // Khi đếm xong
        if (_currentTime <= 0f)
        {
            _isRunning = false;
            OnCountdownFinished();
        }
    }

    public void StartCountdown(float time)
    {
        _currentTime = time;
        countdownTime = time;
        _image.fillAmount = 1f; // Bắt đầu từ đầy (đóng)
        _isRunning = true;
    }

    public void ResetCountdown()
    {
        _isRunning = false;
        _image.fillAmount = 0f; // Reset về trạng thái đóng
    }

    private void OnCountdownFinished()
    {
        Debug.Log("Countdown Finished!");
        // Bạn có thể thêm code ở đây (ví dụ: phát âm thanh, kích hoạt skill, v.v.)
    }
}