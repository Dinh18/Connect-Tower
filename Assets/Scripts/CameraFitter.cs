using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public static class CameraFitter
{
    private static readonly Vector3[] _cornersBuffer = new Vector3[4];

    /// <summary>
    /// Tính toán Size và Vị trí Camera để Map luôn nằm ngay trên Bottom UI dựa vào dữ liệu LevelDataSO.
    /// </summary>
    public static (float size, Vector3 targetPos) CalculateCameraMetrics(
        Camera camera,
        IEnumerable<SlotController> slots,
        RectTransform bottomUI,
        RectTransform topUI,
        float paddingTop = 0f,
        float paddingBottom = 0f,
        float paddingSides = 0f)
    {
        if (camera == null) return (5f, Vector3.zero);

        // 1. Tính toán Bounding Box của tất cả Renderer (Slots + Blocks) trong không gian Local của Camera
        Bounds camLocalBounds = new Bounds();
        bool hasValid = false;

        if (slots != null)
        {
            foreach (var slot in slots)
            {
                if (slot == null || slot.transform == null || !slot.gameObject.activeInHierarchy) continue;
                
                // Lấy renderers của slot
                List<Renderer> allRenderers = new List<Renderer>();
                allRenderers.AddRange(slot.GetComponentsInChildren<Renderer>(false));

                // Lấy thêm renderers của các blocks đang nằm trong slot (vì block có thể khác parent)
                if (slot.blocks != null)
                {
                    foreach (var block in slot.blocks)
                    {
                        if (block != null && block.gameObject.activeInHierarchy)
                        {
                            allRenderers.AddRange(block.GetComponentsInChildren<Renderer>(false));
                        }
                    }
                }

                foreach (var r in allRenderers)
                {
                    // Bỏ qua các vfx/particle để không làm sai lệch bounding box
                    if (r is ParticleSystemRenderer) continue;

                    // Lấy bounding box thế giới của renderer
                    Bounds bounds = r.bounds;
                    Vector3 extents = bounds.extents;
                    Vector3 center = bounds.center;
                    
                    // Chuyển 8 đỉnh của bounds vào không gian local của camera
                    for (int i = 0; i < 8; i++)
                    {
                        Vector3 corner = center + new Vector3(
                            (i & 1) == 0 ? extents.x : -extents.x,
                            (i & 2) == 0 ? extents.y : -extents.y,
                            (i & 4) == 0 ? extents.z : -extents.z
                        );
                        
                        Vector3 localCorner = camera.transform.InverseTransformPoint(corner);
                        if (!hasValid)
                        {
                            camLocalBounds = new Bounds(localCorner, Vector3.zero);
                            hasValid = true;
                        }
                        else
                        {
                            camLocalBounds.Encapsulate(localCorner);
                        }
                    }
                }
            }
        }

        if (!hasValid)
        {
            return (5f, camera.transform.position);
        }

        // Kích thước thật của VÙNG CHƠI (Local Camera Space) bao trọn toàn bộ vật thể 3D
        float playWidthLocal = camLocalBounds.size.x;
        float playHeightLocal = camLocalBounds.size.y;
        
        float targetLocalWidth = playWidthLocal + (paddingSides * 2f);
        float targetLocalHeight = playHeightLocal + paddingTop + paddingBottom;

        // 2. Lấy chiều cao UI theo Pixel
        float screenHeight = Screen.height;
        Canvas parentCanvas = bottomUI != null ? bottomUI.GetComponentInParent<Canvas>() : null;
        float bottomUIPixelHeight = GetTruePixelHeightFromBottom(bottomUI, parentCanvas, camera);
        float topUIPixelHeight = GetTruePixelHeightFromTop(topUI, parentCanvas, camera, screenHeight);

        float safePixelHeight = screenHeight - bottomUIPixelHeight - topUIPixelHeight;
        if (safePixelHeight < screenHeight * 0.2f) safePixelHeight = screenHeight * 0.2f;

        // 3. Tính OrthoSize (Zoom) để Vùng Chơi vừa khít màn hình
        float sizeForHeight = (targetLocalHeight * (screenHeight / safePixelHeight)) / 2f;
        float screenAspect = camera.aspect;
        if (screenAspect <= 0.001f) screenAspect = 1f;
        float sizeForWidth = targetLocalWidth / (2f * screenAspect);

        float finalSize = Mathf.Max(sizeForHeight, sizeForWidth);

        // 4. Tính toán Vị trí Camera mới
        float localUnitsPerPixel = (finalSize * 2f) / screenHeight;
        
        float safeAreaBottomLocalY = -finalSize + bottomUIPixelHeight * localUnitsPerPixel;
        float safeAreaTopLocalY = finalSize - topUIPixelHeight * localUnitsPerPixel;
        
        // Căn giữa Board vào vùng Safe Area (trừ UI trên và dưới)
        float safeAreaCenterLocalY = (safeAreaBottomLocalY + safeAreaTopLocalY) / 2f;
        float targetBoardCenterLocalY = safeAreaCenterLocalY + (paddingBottom - paddingTop) / 2f;
        
        // Vector vị trí trung tâm mong muốn của Board so với Camera mới
        Vector3 targetBoardLocalCenter = new Vector3(0f, targetBoardCenterLocalY, camLocalBounds.center.z);
        
        // Tọa độ World hiện tại của trung tâm Board
        Vector3 boardWorldCenter = camera.transform.TransformPoint(camLocalBounds.center);
        
        // Camera mới lùi lại dựa trên targetBoardLocalCenter
        Vector3 targetCameraPos = boardWorldCenter - camera.transform.TransformDirection(targetBoardLocalCenter);

        return (finalSize, targetCameraPos);
    }

    private static float GetTruePixelHeightFromBottom(RectTransform ui, Canvas canvas, Camera cam)
    {
        if (ui == null) return 0f;

        Vector3[] screenCorners = new Vector3[4];
        ui.GetWorldCorners(_cornersBuffer);
        for (int i = 0; i < 4; i++)
        {
            if (canvas != null && canvas.renderMode == RenderMode.ScreenSpaceOverlay) 
                screenCorners[i] = _cornersBuffer[i];
            else 
            {
                Camera projectionCam = (canvas != null && canvas.worldCamera != null) ? canvas.worldCamera : cam;
                screenCorners[i] = RectTransformUtility.WorldToScreenPoint(projectionCam, _cornersBuffer[i]);
            }
        }
        return Mathf.Max(0f, Mathf.Max(screenCorners[1].y, screenCorners[2].y));
    }

    private static float GetTruePixelHeightFromTop(RectTransform ui, Canvas canvas, Camera cam, float screenHeight)
    {
        if (ui == null) return 0f;

        Vector3[] screenCorners = new Vector3[4];
        ui.GetWorldCorners(_cornersBuffer);
        for (int i = 0; i < 4; i++)
        {
            if (canvas != null && canvas.renderMode == RenderMode.ScreenSpaceOverlay) 
                screenCorners[i] = _cornersBuffer[i];
            else 
            {
                Camera projectionCam = (canvas != null && canvas.worldCamera != null) ? canvas.worldCamera : cam;
                screenCorners[i] = RectTransformUtility.WorldToScreenPoint(projectionCam, _cornersBuffer[i]);
            }
        }
        return Mathf.Max(0f, screenHeight - Mathf.Min(screenCorners[0].y, screenCorners[3].y));
    }

    public static void FitBoardOrtho(
        Camera camera,
        IEnumerable<SlotController> slots,
        RectTransform bottomUI,
        RectTransform topUI = null,
        float paddingTop = 0f,
        float paddingBottom = 0f,
        float paddingSides = 0f,
        float tweenDurationSeconds = 0f)
    {
        if (camera == null) return;

        var metrics = CalculateCameraMetrics(camera, slots, bottomUI, topUI, paddingTop, paddingBottom, paddingSides);

        camera.DOKill();
        camera.transform.DOKill();

        if (tweenDurationSeconds > 0f)
        {
            camera.DOOrthoSize(metrics.size, tweenDurationSeconds).SetEase(Ease.OutCubic);
            camera.transform.DOMove(metrics.targetPos, tweenDurationSeconds).SetEase(Ease.OutCubic);
        }
        else
        {
            camera.orthographicSize = metrics.size;
            camera.transform.position = metrics.targetPos;
        }
    }
}