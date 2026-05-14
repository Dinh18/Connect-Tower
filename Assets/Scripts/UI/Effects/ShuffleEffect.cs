using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class ShuffleEffect : MonoBehaviour, IBoosterEffect
{
    [SerializeField] private SlotsManager slotsManager;
    [SerializeField] private Transform centerPivot;
    [SerializeField] private GameObject blackHole; 
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
