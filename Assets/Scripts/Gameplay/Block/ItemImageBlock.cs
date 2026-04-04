using UnityEngine;
using UnityEngine.UI;

public class ItemImageBlock : MonoBehaviour
{
    [SerializeField]private Image image;
    public void AddImage(Sprite sprite)
    {
        
        if (sprite == null)
        {
            Debug.LogError("LỖI: Hình ảnh truyền vào từ DataLevel đang bị NULL! Hãy kiểm tra lại file DataLevel.");
            return;
        }

        if (image != null)
        {
            image.sprite = sprite;
        }
        else
        {
            Debug.LogError("LỖI: Biến myImage chưa được gán! Hãy mở Prefab Block lên và kéo component Image vào ô này.");
        }
    }

    public void ShowImage()
    {
        this.gameObject.SetActive(true);
    }
    public void HideImage()
    {
        this.gameObject.SetActive(false);
    }
}
