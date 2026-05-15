using UnityEngine;
using UnityEngine.UI;

public class ProfileItemUI : MonoBehaviour
{
    public Toggle selectedToggle;
    public Image itemImage;
    public ProfileItemData itemData;
    public void Setup(ProfileItemData itemData, ToggleGroup toggleGroup)
    {
        this.itemData = itemData;
        selectedToggle.group = toggleGroup;
        itemImage.sprite = itemData.itemSprite;
    }
}
