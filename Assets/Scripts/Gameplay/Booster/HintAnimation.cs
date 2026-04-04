using DG.Tweening;
using UnityEngine;

public class HintAnimation : MonoBehaviour
{
    void OnEnable()
    {
        this.transform.DORotate(new Vector3(0, 0, 45), 1f)
                      
                      .SetLoops(-1, LoopType.Yoyo)
                      
                      .SetEase(Ease.InOutSine);
    }
}
