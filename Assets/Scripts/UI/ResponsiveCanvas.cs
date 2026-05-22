using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasScaler))]
public class ResponsiveCanvas : MonoBehaviour
{
    void Awake()
    {
        CanvasScaler scaler = GetComponent<CanvasScaler>();
        float sizeScreen = (float)Screen.width/ (float)Screen.height;

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
        
    }
}