using Firebase;
using Firebase.Extensions;
using UnityEngine;

public class FirebaseManager : MonoBehaviour
{
    public static bool IsFirebaseReady {get; private set;}  = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        InitializeFirebase();
    }

 
    private void InitializeFirebase()
    {
        Debug.Log("Đang kiểm tra cấu hình Firebase...");

        // Hàm này sẽ check xem điện thoại của user có đủ file thư viện Google Play Services chưa
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => 
        {
            DependencyStatus dependencyStatus = task.Result;
            
            if (dependencyStatus == DependencyStatus.Available) 
            {
                // Mọi thứ hoàn hảo! Firebase đã sẵn sàng để gọi
                IsFirebaseReady = true;
                Debug.Log("Khởi tạo Firebase thành công!");

                // Tới đây bạn có thể gọi hàm Login, hoặc Load Leaderboard
                // ví dụ: LoadTop10Leaderboard();
            } 
            else 
            {
                // Báo lỗi nếu thiếu thư viện cốt lõi trên máy người chơi
                Debug.LogError($"Không thể khởi tạo Firebase: {dependencyStatus}");
            }
        });
    }
}
