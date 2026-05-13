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
        // Hàng dưới cùng ở tọa độ y = 0. Cọc gỗ cao max ~3.1
        float maxPoleHeight = 3.1f;
        float gridHeight = (numRows - 1) * Constants.SLOT_HEIGHT + maxPoleHeight;
        float gridWidth = maxColumns * Constants.SLOT_WIDTH;

        // 2. Tính toán tỷ lệ phần trăm màn hình cho UI (Dựa trên thiết kế Canvas Scaler Match = Width)
        // Tỷ lệ màn hình chuẩn là 16:9
        float referenceAspect = 9f / 16f; // 0.5625
        
        // Ở 16:9, UI phía trên (Header, Moves, Target) chiếm khoảng 28% và UI dưới (Booster, Hand) chiếm 18%
        float baseTopUIMargin = 0.28f;
        float baseBottomUIMargin = 0.18f;

        // Với màn hình iPad (rộng hơn) hoặc đt dài (hẹp hơn), chiều cao phần trăm UI sẽ tự scale theo
        float aspectMultiplier = mainCamera.aspect / referenceAspect;
        float topUIMargin = baseTopUIMargin * aspectMultiplier;
        float bottomUIMargin = baseBottomUIMargin * aspectMultiplier;
        
        // Vùng không gian có thể hiển thị Grid (Playable Area)
        float playableHeightRatio = Mathf.Clamp(1f - topUIMargin - bottomUIMargin, 0.35f, 0.8f);

        // 3. Tính toán Orthographic Size để bao trọn Grid
        // Chiều cao: Phải nằm trọn trong vùng Playable
        float orthoToFitHeight = gridHeight / (2f * playableHeightRatio);

        // Chiều rộng: Bao trọn chiều rộng với một chút padding
        float paddingWidth = 1.8f; 
        float orthoToFitWidth = (gridWidth + paddingWidth) / (2f * mainCamera.aspect);
        
        // Chọn size lớn nhất để đảm bảo không bị cắt viền
        float finalOrthoSize = Mathf.Max(orthoToFitHeight, orthoToFitWidth);
        mainCamera.orthographicSize = finalOrthoSize;

        // 4. Tính toán Offset để canh giữa Grid vào Playable Area thay vì Screen Center
        float gridCenterY = gridHeight / 2f;

        // Tỷ lệ tâm của vùng Playable tính từ dưới đáy màn hình lên
        float centerRatio = bottomUIMargin + (playableHeightRatio / 2f);
        
        // Độ chênh lệch giữa tâm Playable và tâm màn hình (tính bằng world units)
        // Tâm màn hình là 0.5. Nếu centerRatio < 0.5 (tức vùng playable thấp hơn tâm màn hình), offset sẽ âm
        float playableOffsetFromScreenCenter = finalOrthoSize * (2f * centerRatio - 1f);

        // 5. Bù trừ hiệu ứng phối cảnh 2.5D và Ưu tiên nửa dưới màn hình
        // Các cọc gỗ vươn cao lên trên tạo cảm giác hình ảnh bị lệch lên đỉnh màn hình.
        // Ngoài ra, để ưu tiên board nằm ở nửa dưới của màn hình (gần với các nút bấm/booster hơn),
        // ta sẽ tăng offset lên ~40% (0.40f) của ortho size để đẩy camera LÊN nhiều hơn, 
        // qua đó kéo toàn bộ lưới XUỐNG thấp hơn.
        float perspectiveOffset = finalOrthoSize * 0.40f;

        Vector3 cameraPos = transform.position;
        cameraPos.x = 0;
        // Căn tâm playable area, và cộng thêm perspectiveOffset để dời Board xuống nửa dưới màn hình
        cameraPos.y = gridCenterY - playableOffsetFromScreenCenter + perspectiveOffset;
        cameraPos.z = -10f;

        transform.position = cameraPos;
    }
}
