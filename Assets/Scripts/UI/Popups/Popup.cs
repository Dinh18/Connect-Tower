using UnityEngine;

public enum PopupType
{
    RefillHeart,
    Booster,
    Setting,
    QuitLevel,
    GeneralStats,
    EditProfile,
}

public abstract class Popup : MonoBehaviour
{
    public PopupType popupType;
    public GameObject dimImage;
    public virtual void Hide()
    {
        if(dimImage != null) dimImage.SetActive(false);
        gameObject.SetActive(false);
    }
    public virtual void Show()
    {
        if(dimImage != null) dimImage.SetActive(true);
        gameObject.SetActive(true);
    }
}
