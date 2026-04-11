using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class SafeAreaFitter : MonoBehaviour
{
    private RectTransform rectTransform;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        // Canvas[] canvas = GameObject.FindObjectsByType<Canvas>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        CanvasScaler scaler = GetComponentInParent<CanvasScaler>();
        float sizeScreen = (float)Screen.width/ (float)Screen.height;
        Debug.Log(sizeScreen);
        // foreach(Canvas cv in canvas)
        // {
        //     CanvasScaler scaler = cv.GetComponent<CanvasScaler>();
        //     scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        //     if(scaler == null) continue;
        //     if(sizeScreen < 0.6)
        //     {
        //         scaler.matchWidthOrHeight = 0;
        //     }
        //     else if(sizeScreen >= 0.6)
        //     {
        //         scaler.matchWidthOrHeight = 1;
        //     }
        //     else
        //     {
        //         scaler.matchWidthOrHeight = 0.5f;
        //     }
        // }
        if(sizeScreen < 0.6)
        {
            scaler.matchWidthOrHeight = 0;
        }
        else if(sizeScreen >= 0.6)
        {
            scaler.matchWidthOrHeight = 1;
        }
        else
        {
            scaler.matchWidthOrHeight = 0.5f;
        }
        ApplySafeArea();
    }

    void ApplySafeArea()
    {
        Rect safeArea = Screen.safeArea;

        // Chuyển đổi tọa độ Pixel của Safe Area thành tọa độ Anchor tỉ lệ (0 đến 1)
        Vector2 anchorMin = safeArea.position;
        Vector2 anchorMax = safeArea.position + safeArea.size;

        anchorMin.x /= Screen.width;
        anchorMin.y /= Screen.height;
        anchorMax.x /= Screen.width;
        anchorMax.y /= Screen.height;

        // Áp dụng viền an toàn cho Panel
        rectTransform.anchorMin = anchorMin;
        rectTransform.anchorMax = anchorMax;
    }
}