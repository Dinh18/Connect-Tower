using System.Drawing;
using System.Net.Mail;
using JetBrains.Annotations;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    Camera mainCamera;

    void Awake()
    {
        CoreServices.Register<CameraController>(this);  
    }

    public void Setup()
    {
        mainCamera = GetComponent<Camera>();
    }

    public void FitCamera(int row1, int row2)
    {
        if (mainCamera == null) Setup();

        int maxColumns = Mathf.Max(row1, row2);
        int numRows = (row1 == 0 || row2 == 0) ? 1 : 2;

        // 1. Kích thước thực tế của lưới (Grid)
        // Hàng dưới cùng ở tọa độ y = 0
        // Nếu có 2 hàng, hàng trên ở y = SLOT_HEIGHT (khoảng 4.05)
        // Cọc gỗ cao tương đương 4 block (~2.6) + đỉnh cọc (~0.5) = 3.1
        float maxPoleHeight = 3.1f;
        float gridHeight = (numRows - 1) * Constants.SLOT_HEIGHT + maxPoleHeight;
        float gridWidth = maxColumns * Constants.SLOT_WIDTH;

        // 2. Tính toán Orthographic Size bao trọn Grid
        // Màn hình điện thoại thường có tỷ lệ dài. Ta thêm Padding để không bị sát viền.
        float paddingWidth = 2.0f; // Đã tăng thêm một chút để lấy không gian hai bên
        float paddingHeight = (mainCamera.aspect < 0.6f) ? 4.5f : 3f; // Điện thoại dài cần nhiều padding trên dưới hơn

        float orthoToFitHeight = (gridHeight + paddingHeight) / 2f;
        float orthoToFitWidth = (gridWidth + paddingWidth) / (2f * mainCamera.aspect);
        
        float finalOrthoSize = Mathf.Max(orthoToFitHeight, orthoToFitWidth);
        mainCamera.orthographicSize = finalOrthoSize;

        // 3. Tính toán trọng tâm Y thực tế của Grid
        float gridCenterY = gridHeight / 2f;

        // 4. Bù trừ UI (UI Offset) cho Game 2.5D
        // Vì hệ thống UI phía trên (Top UI: Move, Coin) tốn nhiều không gian hơn,
        // kèm theo hiệu ứng phối cảnh 2.5D khiến các cọc vươn cao lên trên.
        // Giải pháp: Đưa Camera LÊN TRÊN (cộng thêm Y) để toàn bộ lưới dịch XUỐNG DƯỚI.
        float uiOffset = finalOrthoSize * 0.35f; // Tăng từ 25% lên 35% để hạ lưới xuống thêm 1 xíu nữa theo ý bạn

        Vector3 cameraPos = transform.position;
        cameraPos.x = 0;
        cameraPos.y = gridCenterY + uiOffset; // CỘNG uiOffset để camera cao lên -> Lưới tụt xuống
        cameraPos.z = -10f;

        transform.position = cameraPos;
    }

    // Đã xóa bỏ hàm SetupCamera cũ vì dư thừa và dễ gây nhầm lẫn
}
