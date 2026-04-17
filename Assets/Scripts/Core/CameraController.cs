using System.Drawing;
using System.Net.Mail;
using JetBrains.Annotations;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    Camera mainCamera;
    [SerializeField] private float paddingWidth = 2f;
    [SerializeField] private float paddingHeight = 1f;
    // [SerializeField] private float padding = 2f;
    public void Setup()
    {
        mainCamera = GetComponent<Camera>();
    }
    public void FitCamera(int row1, int row2)
    {
        if(mainCamera.aspect < 0.6)
        {
            paddingWidth = 2f;
            paddingHeight = 1f;
        }
        else
        {
            paddingWidth = 1;
            paddingHeight = 5f;
        }
        if (mainCamera == null) Setup();

        int maxColumn = Mathf.Max(row1, row2);
        float gridWidth = maxColumn * Constants.SLOT_WIDTH;
        float gridHeight = Constants.SLOT_HEIGHT + 2f;


        float sizeToFitHeight = (gridHeight + paddingHeight) / 2f;
        
        float sizeToFitWidth = (gridWidth + paddingWidth) / (2f * mainCamera.aspect);

        mainCamera.orthographicSize = Mathf.Max(sizeToFitHeight, sizeToFitWidth);

        float centerY = Constants.SLOT_HEIGHT;
        if(row2 != 0) centerY = 6;
        transform.position = new Vector3(0, centerY, -10f);
    }
    public void SetupCamera(int topColumns, int bottomColumns)
    {
        int maxColumns = Mathf.Max(topColumns, bottomColumns);
        int numRows = (topColumns == 0 || bottomColumns == 0) ? 1 : 2;

        // 1. Kích thước thật của lưới (cộng thêm khoảng cách padding giữa các cọc)
        float gridWidth = maxColumns * Constants.SLOT_WIDTH;
        float gridHeight = numRows * Constants.SLOT_HEIGHT;

        // 2. Tính Orthographic Size bao trọn màn hình (Fix lỗi Aspect Ratio)
        float orthoToFitHeight = (gridHeight + 2) / 2f;
        float orthoToFitWidth = (gridWidth) / (2f * mainCamera.aspect);
        
        float finalOrthoSize = Mathf.Max(orthoToFitHeight, orthoToFitWidth);

        // Bù thêm một chút size để lấy không gian thở (Breathing room) cho UI trên/dưới
        mainCamera.orthographicSize = finalOrthoSize * 1.15f; 

        // 3. Tính toán Center Y (Tâm thật của lưới)
        // Giả sử (0,0) là vị trí hàng dưới cùng
        float gridTrueCenterY = gridHeight / 2f;

        // 4. UI Offset (Bí thuật cho Mobile)
        // Màn hình mobile dài, UI Top chiếm nhiều diện tích. Ta cần hạ trọng tâm lưới xuống một chút.
        // Khoảng cách này nên tính bằng tỷ lệ % của màn hình thay vì số cứng.
        float uiOffset = finalOrthoSize * 0.2f; // Dời tâm xuống khoảng 20% màn hình

        Vector3 cameraPos = transform.position;
        cameraPos.x = 0;
        // Tọa độ Y của Camera = Tâm thật của lưới + Bù trừ UI
        cameraPos.y = gridTrueCenterY + uiOffset; 
        cameraPos.z = -6.97f;

        transform.position = cameraPos;
    }
}
