using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

public class ShopTools : EditorWindow
{
    [MenuItem("Tools/Auto Setup Shop Buttons")]
    public static void SetupShopButtons()
    {
        // Tìm ShopPanel trong Scene (ngay cả khi nó đang bị ẩn)
        ShopPanel[] shopPanels = Resources.FindObjectsOfTypeAll<ShopPanel>();
        if (shopPanels.Length == 0)
        {
            Debug.LogError("Không tìm thấy ShopPanel trong Scene! Hãy mở Scene chứa Shop và thử lại.");
            return;
        }

        ShopPanel shop = shopPanels[0];
        int buttonCount = 0;

        // Dùng Undo để có thể Ctrl+Z nếu muốn
        Undo.RecordObject(shop.gameObject, "Setup Shop Buttons");

        // Quét tất cả các phần tử con của Shop (bạn có thể thay đổi cách quét tùy ý)
        // Dưới đây giả định bạn muốn gắn vào các GameObject có tên chứa chữ "Package" hoặc "Item"
        Transform[] allChildren = shop.GetComponentsInChildren<Transform>(true);
        foreach (Transform child in allChildren)
        {
            // Tùy chỉnh điều kiện ở đây. Giả sử các gói hàng của bạn tên là "Package1", "CoinPack", v.v.
            // Để an toàn, chúng ta sẽ quét các Transform có Image mà chưa có Button
            if (child.GetComponent<Image>() != null && (child.name.ToLower().Contains("pack") || child.name.ToLower().Contains("item") || child.name.ToLower().Contains("booster")))
            {
                Button btn = child.GetComponent<Button>();
                if (btn == null)
                {
                    btn = child.gameObject.AddComponent<Button>();
                    buttonCount++;
                }

                AnimationButton animBtn = child.GetComponent<AnimationButton>();
                if (animBtn == null)
                {
                    child.gameObject.AddComponent<AnimationButton>();
                    buttonCount++;
                }
                
                EditorUtility.SetDirty(child.gameObject);
            }
        }

        Debug.Log($"<color=green>Đã tự động thêm {buttonCount} components (Button & AnimationButton) vào các gói hàng trong Shop!</color>");
    }
}
