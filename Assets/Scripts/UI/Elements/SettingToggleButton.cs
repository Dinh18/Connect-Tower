using UnityEngine;

public enum TypeToggle
{
    None,
    Haptic,
    Sound,
    Music
}

public class SettingToggleButton : MonoBehaviour
{
    [SerializeField] private GameObject inactiveIcon;
    [SerializeField] private TypeToggle typeToggle;

    void OnEnable()
    {
        if(typeToggle == TypeToggle.Sound) AudioManager.OnToggle += UpdateUI;
        else if(typeToggle == TypeToggle.Haptic) HapticManager.OnToggle += UpdateUI;
        else if(typeToggle == TypeToggle.Music) AudioManager.OnMusicToggle += UpdateUI;
        if(typeToggle == TypeToggle.Sound)
        {
            inactiveIcon.SetActive(!CoreServices.Get<AudioManager>().IsSoundOn());
        }
        else if(typeToggle == TypeToggle.Haptic)
        {
            inactiveIcon.SetActive(!CoreServices.Get<HapticManager>().IsHapticOn());
        }
        else if(typeToggle == TypeToggle.Music)
        {
            inactiveIcon.SetActive(!CoreServices.Get<AudioManager>().IsMusicOn());
        }
    }

    void OnDisable()
    {
        if(typeToggle == TypeToggle.Sound) AudioManager.OnToggle -= UpdateUI;
        else if(typeToggle == TypeToggle.Haptic) HapticManager.OnToggle -= UpdateUI;
        else if(typeToggle == TypeToggle.Music) AudioManager.OnToggle -= UpdateUI;
    }

    private void UpdateUI(bool isOn)
    {
        if(typeToggle == TypeToggle.Sound)
        {
            inactiveIcon.SetActive(!isOn);
        }
        else if(typeToggle == TypeToggle.Haptic)
        {
            inactiveIcon.SetActive(!isOn);
        }
        else if(typeToggle == TypeToggle.Music)
        {
            inactiveIcon.SetActive(!isOn);
        }
    }

}