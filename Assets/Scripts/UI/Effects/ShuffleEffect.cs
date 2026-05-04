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

        // CoreServices.Get<SlotsManager>().GetAllSlots().ForEach(slot =>
        // {
        //     if(slot.isFinished || !slot.isRevealed || slot.slotType == SlotController.SlotType.Ice) return;
        //     {
        //         while(slot.blocks.Count > 0)
        //         {
        //             BlockController block = slot.blocks.Pop();

        //             if(!block.isRevealed)
        //             {
        //                 slot.blocks.Push(block);
        //                 return;
        //             }
        //             Vector3 randomOffset = UnityEngine.Random.insideUnitSphere * 1.5f;
        //             Vector3 gatherPos = centerPivot.position + randomOffset;
        //             Vector3[] pathArr = new Vector3[] {slot.arcPeak.transform.position, gatherPos};
                    
        //             block.transform.SetParent(centerPivot);
        //             block.transform.DOKill();
        //             sequence.Insert(0, block.transform.DOPath(pathArr, 0.5f, PathType.CatmullRom).SetEase(Ease.InBack));
        //         }
        //     }
        // });

        sequence.OnComplete(() =>
        {
            Debug.Log("Shuffle Effect Completed");
            ExcuteBooster?.Invoke();
        });
    }
}
