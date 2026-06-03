using System;
using System.Collections;
using CandyCoded.HapticFeedback;
using UnityEngine;

public class HapticManager : MonoBehaviour
{
    private bool isHapticOn;
    public static Action<bool> OnToggle;
    void Awake()
    {
        LoadHapticSetting();
        CoreServices.Register<HapticManager>(this);
    }

    public void PlayHaptic()
    {
        if(!isHapticOn) return;
        HapticFeedback.LightFeedback();
    }

    public void DelayPlayHaptic(float delay)
    {
        if(!isHapticOn) return;
        StartCoroutine(DelayHapticCoroutine(delay));
    }

    private IEnumerator DelayHapticCoroutine(float delay)
    {
        yield return new WaitForSeconds(delay);
        HapticFeedback.LightFeedback();
    }

    private void LoadHapticSetting()
    {
        isHapticOn = PlayerPrefs.GetInt("HapticState", 1) == 1;
    }

    public bool ToggleHaptic()
    {
        isHapticOn = !isHapticOn;
        PlayerPrefs.SetInt("HapticState", isHapticOn ? 1 : 0);
        PlayerPrefs.Save();
        OnToggle?.Invoke(isHapticOn);
        return isHapticOn;
    }


    public bool IsHapticOn()
    {
        return isHapticOn;
    }



   

    
    
    
}
