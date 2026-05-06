using UnityEngine;
using DG.Tweening;

public class MagnifyingGlassEffect : MonoBehaviour
{
    private Camera mainCamera;
    private bool isActive = false;
    private float timer = 0f;
    private System.Action onComplete;

    public void Activate(Camera cam, float activeDuration, System.Action onFinish)
    {
        mainCamera = cam;
        timer = activeDuration;
        onComplete = onFinish;
        isActive = true;

        UpdatePosition(true);
        
        transform.localScale = Vector3.zero;
        gameObject.SetActive(true);
        transform.DOScale(new Vector3(2,2,2), 0.4f).SetEase(Ease.OutBack);
    }

    void Update()
    {
        if (!isActive) return;

        UpdatePosition(false);

        timer -= Time.deltaTime;
        if (timer <= 0)
        {
            Deactivate();
        }
    }

    private void UpdatePosition(bool instant)
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = 10f; 
        Vector3 worldPos = mainCamera.ScreenToWorldPoint(mousePos);
        worldPos.z = transform.position.z; 

        if (instant)
        {
            transform.position = worldPos;
        }
        else
        {
            transform.position = Vector3.Lerp(transform.position, worldPos, Time.deltaTime * 20f);
        }
    }

    public void Deactivate()
    {
        if (!isActive) return;
        isActive = false;

        transform.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack).OnComplete(() =>
        {
            gameObject.SetActive(false);
            onComplete?.Invoke();
        });
    }
}
