using UnityEngine;

public enum TypeToggle
{
    None,
    Haptic,
    Sound
}

public class SettingToggleButton : MonoBehaviour
{
    [SerializeField] private GameObject inactiveIcon;
    [SerializeField] private TypeToggle typeToggle;

    void OnEnable()
    {
        if(typeToggle == TypeToggle.Sound) AudioManager.OnToggle += UpdateUI;
        else if(typeToggle == TypeToggle.Haptic) HapticManager.OnToggle += UpdateUI;
        if(typeToggle == TypeToggle.Sound)
        {
            inactiveIcon.SetActive(!CoreServices.Get<AudioManager>().IsSoundOn());
        }
        else if(typeToggle == TypeToggle.Haptic)
        {
            inactiveIcon.SetActive(!HapticManager.Instance.IsHapticOn());
        }
    }

    void OnDisable()
    {
        if(typeToggle == TypeToggle.Sound) AudioManager.OnToggle -= UpdateUI;
        else if(typeToggle == TypeToggle.Haptic) HapticManager.OnToggle -= UpdateUI;
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
    }

}