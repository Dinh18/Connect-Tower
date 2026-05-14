using System;
using UnityEngine;

public class UndoEffect : MonoBehaviour, IBoosterEffect
{
    public void PlayEffect(Action ExcuteBooster)
    {
        ExcuteBooster?.Invoke();
    }
}
