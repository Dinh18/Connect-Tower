using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UIElements;

public class HintEffext : MonoBehaviour, IBoosterEffect
{
    [SerializeField] private RectTransform hintIcon;
    private Vector3 originPos;
    void Awake()
    {
        originPos = hintIcon.anchoredPosition;
    }
    public void PlayEffect(Action ExcuteBooster)
    {
        Sequence sequence = DOTween.Sequence();

        Vector3 screenCenter = new Vector3(Screen.width/2, Screen.height/2, 0);

        float orbitRadius = 100f;

        Vector3 startOrbitPos = new Vector3(screenCenter.x + orbitRadius, screenCenter.y, 0);

        sequence.Append(hintIcon.DOMove(startOrbitPos, 0.5f));
        sequence.Join(hintIcon.DOScale(2,0.5f).SetEase(Ease.OutQuad));
        sequence.Append(DOVirtual.Float(0f, 720f, 1f, (angle) =>
        {
            float radian = angle * Mathf.Deg2Rad;
            
            float x = screenCenter.x + Mathf.Cos(radian) * 100;
            float y = screenCenter.y + Mathf.Sin(radian) * 100;
            
            hintIcon.position = new Vector2(x, y);
            
        })
        .SetEase(Ease.Linear));

        sequence.OnComplete(() =>
        {
            ExcuteBooster(); 
            sequence.AppendInterval(0.2f);
            sequence.Append(hintIcon.DOAnchorPos(originPos, 0.5f));
            sequence.Join(hintIcon.DOScale(1,0.5f).SetEase(Ease.OutQuad));
        });


    }
}
