using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BorderPanel : MonoBehaviour
{
    [SerializeField] private UICountdownController countdownImage;

    void OnEnable()
    {
        GameEventBus.Subscribe<StartBorderFlashEvent>(StartInfiniteMovesCountDown);
        GameEventBus.Subscribe<StopBorderFlashEvent>(StopInfiniteMovesCountDown);
    }

    void OnDisable()
    {
        GameEventBus.UnSubscribe<StartBorderFlashEvent>(StartInfiniteMovesCountDown);
        GameEventBus.UnSubscribe<StopBorderFlashEvent>(StopInfiniteMovesCountDown);
    }

    public void Show()
    {
        this.gameObject.SetActive(true);
        if (countdownImage != null)
        {
            countdownImage.gameObject.SetActive(false);
        }
    }

    public void Hide()
    {
        this.gameObject.SetActive(false);
    }

    public void StartInfiniteMovesCountDown(StartBorderFlashEvent startBorderFlash)
    {
        if(startBorderFlash.borderType == BorderType.Ice && countdownImage != null)
        {
            countdownImage.gameObject.SetActive(true);
            countdownImage.StartCountdown(startBorderFlash.flashTime);
        }
    }

    private void StopInfiniteMovesCountDown(StopBorderFlashEvent stopBorderFlash)
    {
        if (countdownImage != null)
        {
            countdownImage.ResetCountdown();
            countdownImage.gameObject.SetActive(false);
        }
    }
}
