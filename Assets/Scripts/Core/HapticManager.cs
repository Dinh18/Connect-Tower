using System.Collections;
using CandyCoded.HapticFeedback;
using UnityEngine;

public class HapticManager : MonoBehaviour
{
    public static HapticManager Instance;
    private bool isHapticOn;
    void Awake()
    {
        Instance = this;
        LoadHapticSetting();
    }

    public void PlayHaptic()
    {
        if(!isHapticOn) return;
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
        return isHapticOn;
    }


    public bool IsHapticOn()
    {
        return isHapticOn;
    }



   

    
    
    
}
