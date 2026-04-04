using DG.Tweening;
using UnityEngine;

public class HeaderSlot : MonoBehaviour
{
    private BorderHeaderSlot border;
    private SlotController slot;
    [SerializeField] private float duration = 0.5f;

    public void Setup(SlotController slot)
    {
        this.slot = slot;
        border = GetComponentInChildren<BorderHeaderSlot>();
        border.SetUp();
        this.Hide();
    }
    void OnEnable()
    {
        this.transform.localScale = Vector3.zero;

        // 2. Thực hiện tween scale lên 1
        this.transform.DOScale(1f, duration)
            .SetEase(Ease.OutBack)
            .SetUpdate(true);
    }
    public void Show()
    {
        border.SetColor(GetColor());
        this.gameObject.SetActive(true);
    }
    public void Hide()
    {
        this .gameObject.SetActive(false);
    }

    public Color GetColor()
    {
        switch(slot.blocks.Peek().GetColorBlock())
        {
            case BlockController.ColorBlock.Color_1:
                return Constants.COLOR_1;
            case BlockController.ColorBlock.Color_2:
                return Constants.COLOR_2;
            case BlockController.ColorBlock.Color_3:
                return Constants.COLOR_3;
            case BlockController.ColorBlock.Color_4:
                return Constants.COLOR_4;
            case BlockController.ColorBlock.Color_5:
                return Constants.COLOR_5;
            case BlockController.ColorBlock.Color_6:
                return Constants.COLOR_6;
            case BlockController.ColorBlock.Color_7:
                return Constants.COLOR_7;
            case BlockController.ColorBlock.Color_8:
                return Constants.COLOR_8;
            case BlockController.ColorBlock.Color_9:
                return Constants.COLOR_9;
            default:
                return Constants.COLOR_W;
        }
    }

}
