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

public abstract class Popup : UIView
{
    public PopupType popupType;
    public GameObject dimImage;
    public override void Hide()
    {
        if(dimImage != null) dimImage.SetActive(false);
        gameObject.SetActive(false);
    }
    public override void Show()
    {
        if(dimImage != null) dimImage.SetActive(true);
        gameObject.SetActive(true);
    }
}
