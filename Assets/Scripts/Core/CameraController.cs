using UnityEngine;

public class CameraController : MonoBehaviour
{
    Camera mainCamera;
    [SerializeField] private float paddingWidth = 0.5f;
    [SerializeField] private float paddingHeight = 2;
    [SerializeField] private Transform gridRoot;
    public void Setup()
    {
        mainCamera = GetComponent<Camera>();
    }
    public void FitCamera(int row1, int row2)
    {
        int maxColumn = Mathf.Max(row1, row2);

        float gridWidth = maxColumn * Constants.SLOT_WIDTH;
        float gridHeight = 2 * Constants.SLOT_HEIGHT;

        float sizeToFitHeight =(gridHeight + paddingWidth) / 2f;
        float sizeToFitWidth = ((gridWidth + paddingHeight) / 2f) / mainCamera.aspect;

        mainCamera.orthographicSize = Mathf.Max(sizeToFitHeight, sizeToFitWidth);
    }


}
