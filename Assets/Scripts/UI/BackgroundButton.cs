using DG.Tweening;
using NUnit.Framework;
using UnityEngine;

public class BackgroundButton : MonoBehaviour
{
    [SerializeField] private RectTransform background;
    [SerializeField] private RectTransform icon;
    [SerializeField] private GameObject text;
    
    private Vector3 originPosBackground;
    private Vector3 originPosIcon;
    private bool isInit = false;

 

    public void Init()
    {
        originPosBackground = background.anchoredPosition;
        originPosIcon = icon.anchoredPosition;
        // Debug.Log("gv gvh ");
    }

    public void Select()
    {
        if(!isInit)
        {
            Init();
            isInit = true;
        } 
        background.gameObject.SetActive(true);
        text.SetActive(true);

        // background.DOKill();
        // icon.DOKill();

        background.anchoredPosition = originPosBackground;
        icon.anchoredPosition = originPosIcon;


        background.anchoredPosition = new Vector2(originPosBackground.x, originPosBackground.y - 1000f);
        
        background.DOAnchorPosY(originPosBackground.y, 0.3f);
        
        icon.anchoredPosition = originPosIcon;
        icon.DOAnchorPosY(originPosIcon.y + 40f, 0.5f).SetEase(Ease.OutBack);
    }

    public void UnSelect()
    {
        if (!isInit)
        {
            
            Init();
            isInit = true;
        } 
        // 💡 BÍ QUYẾT AN TOÀN: Luôn Kill trước khi giấu đi
        background.DOKill();
        icon.DOKill();

        // Khôi phục lại TOÀN BỘ vị trí gốc (Bao gồm cả background)
        background.anchoredPosition = originPosBackground;
        icon.anchoredPosition = originPosIcon;

        background.gameObject.SetActive(false);
        text.SetActive(false);
    }
}