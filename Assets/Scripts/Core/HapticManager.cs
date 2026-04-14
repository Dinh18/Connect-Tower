using System.Collections;
using Solo.MOST_IN_ONE;
using UnityEngine;

public class HapticManager : MonoBehaviour
{
    public static HapticManager Instance;
    // private bool isHapticOn;
    void Awake()
    {
        Instance = this;
        LoadHapticSetting();
    }

    private void LoadHapticSetting()
    {
        MOST_HapticFeedback.HapticsEnabled = PlayerPrefs.GetInt("HapticState", 1) == 1;
    }

    private void PlayVibrate(MOST_HapticFeedback.HapticTypes hapticTypes)
    {
        if(!MOST_HapticFeedback.HapticsEnabled) return;
        MOST_HapticFeedback.Generate(hapticTypes);
        // Debug.Log("Rung");
    }

    public void PlayVibrateLight() =>PlayVibrate(MOST_HapticFeedback.HapticTypes.LightImpact);
    public void PlayVibrateMedium() =>PlayVibrate(MOST_HapticFeedback.HapticTypes.MediumImpact);
    public void PlayVibrateHeavy() =>PlayVibrate(MOST_HapticFeedback.HapticTypes.HeavyImpact);

    public IEnumerator PlayVibrateBlockFall(int num, float delay)
    {
        for(int i = 0; i < num; i++)
        {
            PlayVibrateHeavy();
            Debug.Log("Run "+i);
            yield return new WaitForSeconds(delay * i);
        }
    }
    

    public bool ToggleHaptic()
    {
        MOST_HapticFeedback.HapticsEnabled = !MOST_HapticFeedback.HapticsEnabled;
        PlayerPrefs.SetInt("HapticState", MOST_HapticFeedback.HapticsEnabled ? 1 : 0);
        PlayerPrefs.Save();
        return MOST_HapticFeedback.HapticsEnabled;
    }

    public bool IsHapticOn()
    {
        return MOST_HapticFeedback.HapticsEnabled;
    }
    
}
