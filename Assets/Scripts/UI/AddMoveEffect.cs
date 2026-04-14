using System;
using DG.Tweening;
using UnityEngine;

public class AddMoveEffect : MonoBehaviour, IBoosterEffect
{
    [SerializeField] private RectTransform addMoveIcon;
    [SerializeField] private RectTransform moveCountText;
    private Vector3 originPos;
    private Vector3 originScale;
    void Awake()
    {
        originPos = addMoveIcon.anchoredPosition;
        originScale = addMoveIcon.localScale;
    }
    // private 
    public void PlayEffect(Action ExcuteBooster)
    {
        Vector3 screenCenter = new Vector3(Screen.width / 2f, Screen.height / 2f, 0);
        Sequence sequence = DOTween.Sequence();
        sequence.Append(addMoveIcon.DOMove(screenCenter, 0.5f));
        sequence.Join(addMoveIcon.DOScale(2f, 0.5f).SetEase(Ease.OutQuad));
        sequence.Append(addMoveIcon.DOMove(moveCountText.transform.position, 0.5f));
        sequence.Join(addMoveIcon.DOScale(1f, 0.5f).SetEase(Ease.OutQuad));
        // sequence.Append(moveCountText.DO)
        sequence.OnComplete(() =>
        {
            sequence.Append(addMoveIcon.DOAnchorPos(originPos, 0.5f));
            sequence.Join(addMoveIcon.DOScale(1f, 0.5f).SetEase(Ease.OutQuad));

            moveCountText.transform.DOKill();
            moveCountText.transform.DOPunchScale(new Vector3(0.5f,0.5f,0.5f), 0.3f, 5, 4);
            HapticManager.Instance.PlayVibrateMedium();
            ExcuteBooster();
            
        });
    }
}
