using UnityEngine;

public class CameraController : MonoBehaviour
{
    Camera mainCamera;
    [SerializeField] private float paddingWidth = 2f;
    [SerializeField] private float paddingHeight = 1f;
    private Transform gridRoot;
    public void Setup()
    {
        mainCamera = GetComponent<Camera>();
    }
    public void FitCamera(int row1, int row2)
    {
        if(mainCamera.aspect < 0.6)
        {
            paddingWidth = 1f;
            paddingHeight = 0.5f;
        }
        else
        {
            paddingWidth = 4;
            paddingHeight = 3;
        }
        // Chốt chặn an toàn: Nhỡ chưa gọi Setup mà đã gọi FitCamera thì tự động lấy Camera
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


}
