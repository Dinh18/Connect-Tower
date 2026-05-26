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

    private System.Collections.IEnumerator Start()
    {
        // 3. Khởi tạo một lưới trống (Blank Slate) thay vì load level hiện tại của người chơi
        LevelDataSO emptyLevel = ScriptableObject.CreateInstance<LevelDataSO>();
        emptyLevel.slots = new System.Collections.Generic.List<SlotSetupData>();
        
        int r1 = 3;
        int r2 = 3;
        for (int i = 0; i < r1 + r2; i++)
        {
            SlotSetupData sData = new SlotSetupData();
            sData.blocks = new System.Collections.Generic.List<BlockSetupData>();
            emptyLevel.slots.Add(sData);
        }

        emptyLevel.row1 = r1;
        emptyLevel.row2 = r2;
        emptyLevel.moves = 999;
        LevelLoader.playtestLevelData = emptyLevel;
        LevelLoader.isPlaytestingTempLevel = true;

        // Chờ 1 frame để đảm bảo tất cả các Manager và UI đã subscribe event
        yield return null;

        Debug.Log("<color=cyan>[LevelEditorSceneSetup]</color> Đã chờ 1 frame. Bắt đầu chuyển State sang Playing...");

        var gameManager = CoreServices.Get<GameManager>();
        if (gameManager != null)
        {
            gameManager.ChangeState(GameManager.GameState.Playing);
            Debug.Log("<color=cyan>[LevelEditorSceneSetup]</color> Đã gọi ChangeState(Playing) thành công!");
            
            // Tự động mở bảng Editor
            if (RuntimeLevelEditorManager.Instance != null)
            {
                RuntimeLevelEditorManager.Instance.ToggleEditor();
                Debug.Log("<color=cyan>[LevelEditorSceneSetup]</color> Đã mở bảng Editor.");
            }
        }
        else
        {
            Debug.LogError("<color=red>[LevelEditorSceneSetup]</color> LỖI: Không tìm thấy GameManager!");
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
        // Đã chuyển phần này xuống LateUpdate để liên tục ghi đè CameraController
    }

    private void LateUpdate()
    {
        // Liên tục ghi đè CameraController để phóng to Grid và đưa nó ra giữa phần trống bên phải
        if (Camera.main != null)
        {
            // Phóng to màn chơi (Giảm orthographicSize = Zoom In)
            // Kích thước chuẩn trên dọc thường là ~10-15. Đặt 7.5f sẽ giúp grid to và rõ ràng hơn.
            Camera.main.orthographicSize = 7.5f;

            // Căn chính giữa màn hình (0f) và hạ trọng tâm Camera xuống một chút (y = 2.5f)
            // Tăng y của Camera sẽ làm lưới thụt xuống dưới một chút, vừa vặn không bị đè vào UI
            Vector3 camPos = Camera.main.transform.position;
            camPos.x = 0f; 
            camPos.y = 2.5f;
            Camera.main.transform.position = camPos;
        }
    }
}
