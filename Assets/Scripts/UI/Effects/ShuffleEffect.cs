using System;
using DG.Tweening;
using UnityEngine;

public class ShuffleEffect : MonoBehaviour, IBoosterEffect
{
    [SerializeField] private Transform centerPivot;
    [SerializeField] private Transform blockRoot;
    void Awake()
    {
        
    }

    public void PlayEffect(Action ExcuteBooster)
    {
        Sequence sequence = DOTween.Sequence();

        sequence.OnComplete(() =>
        {
            Debug.Log("Shuffle Effect Completed");
            ExcuteBooster?.Invoke();
        });
    }
}
