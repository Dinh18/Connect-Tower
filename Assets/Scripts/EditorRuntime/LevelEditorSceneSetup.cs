using UnityEngine;

public class LevelEditorSceneSetup : MonoBehaviour
{
    private void Awake()
    {
        Debug.Log("<color=cyan>[LevelEditorSceneSetup]</color> Đang khởi tạo môi trường Level Editor độc lập...");
        
        // 1. Chặn vĩnh viễn việc lưu Firebase và gọi API Game chính
        LevelLoader.isPlaytestingTempLevel = true;

        // 2. Tự động sinh ra Tool Editor
        if (RuntimeLevelEditorManager.Instance == null)
        {
            GameObject editorObj = new GameObject("RuntimeLevelEditor");
            editorObj.AddComponent<RuntimeLevelEditorManager>();
        }

        // Tự động chuyển sang trạng thái Playing để UIManager tự ẩn HomePanel đúng cách
        // Không disable MainCanvas trực tiếp để tránh lỗi Coroutine/NullReference
    }

    private void Start()
    {
        var gameManager = CoreServices.Get<GameManager>();
        if (gameManager != null)
        {
            gameManager.ChangeState(GameManager.GameState.Playing);
        }

        // --- CĂN CHỈNH CHO MÀN HÌNH NGANG (FREE ASPECT) ---
        // 1. Sửa lỗi bóp méo UI bằng cách ép CanvasScaler match theo Chiều Cao (Height)
        GameObject mainCanvas = GameObject.Find("MainCanvas");
        if (mainCanvas != null)
        {
            var scaler = mainCanvas.GetComponent<UnityEngine.UI.CanvasScaler>();
            if (scaler != null)
            {
                scaler.matchWidthOrHeight = 1f; // 1 = Match Height
            }
        }

        // 2. Sửa lỗi Camera bị lệch (Dịch Camera sang trái để Grid hiển thị lệch sang phải, tránh bị bảng công cụ đè lên)
        if (Camera.main != null)
        {
            Vector3 camPos = Camera.main.transform.position;
            camPos.x = -2.5f; // Dịch sang trái 2.5 đơn vị
            Camera.main.transform.position = camPos;
        }
    }
}
