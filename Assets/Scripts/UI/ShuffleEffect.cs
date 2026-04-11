using System;
using DG.Tweening;
using UnityEngine;

public class ShuffleEffect : MonoBehaviour, IBoosterEffect
{
    [SerializeField] private RectTransform shuffleIcon;
    private Vector3 originPos;
    private Quaternion originRot;
    void Awake()
    {
        originPos = shuffleIcon.anchoredPosition;
        originRot = shuffleIcon.rotation;
    }

    public void PlayEffect(Action ExcuteBooster)
    {
        Sequence sequence = DOTween.Sequence();

        Vector3 screenCenter = new Vector3(Screen.width / 2f, Screen.height / 2f, 0);

        sequence.Append(shuffleIcon.DOMove(screenCenter, 0.5f));
        sequence.Join(shuffleIcon.DOScale(3, 0.5f).SetEase(Ease.OutQuad));
        // sequence.AppendInterval(0.1f);
        sequence.JoinCallback(() =>
        {
            ExcuteBooster(); 
        });
        sequence.Append(shuffleIcon.DORotate(new Vector3(0,0,360),0.5f, RotateMode.FastBeyond360).SetEase(Ease.OutBack)); 
        sequence.OnComplete(() =>
        {
            sequence.Append(shuffleIcon.DOAnchorPos(originPos, 0.5f));
            sequence.Join(shuffleIcon.DOScale(1, 0.5f).SetEase(Ease.OutQuad));
        }); 
    }
}
