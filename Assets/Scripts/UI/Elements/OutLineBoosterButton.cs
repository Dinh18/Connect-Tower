using DG.Tweening;
using UnityEngine;

public class OutLineBoosterButton : MonoBehaviour
{
    [SerializeField] private GameObject outLine;
    [SerializeField] private BoosterButton boosterButton;
    void OnEnable()
    {
        GameEventBus.Subscribe<NoMovesAvailableEvent>(ShowOutline);
        GameEventBus.Subscribe<BoardStateChangedEvent>(HideOutline);
    }
    void OnDisable()
    {
        this.transform.DOKill();    
        outLine.SetActive(false);
        GameEventBus.UnSubscribe<NoMovesAvailableEvent>(ShowOutline);
        GameEventBus.UnSubscribe<BoardStateChangedEvent>(HideOutline);
    }
    
    public void ShowOutline(NoMovesAvailableEvent evt)
    {
        if(!CoreServices.Get<DataManager>().IsUnLockedBooster((int)boosterButton.GetBooster().GetBoosterType())) return;
        if (outLine.activeSelf) return;

        outLine.SetActive(true);

        this.transform.DOKill();
        this.transform.DOScale(1.2f, 0.5f)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
    }

    public void HideOutline(BoardStateChangedEvent evt)
    {
        this.transform.DOKill();
        this.transform.localScale = Vector3.one;
        outLine.SetActive(false);
    }
}
