using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class UndoBooster : Booster
{
    private UnityEngine.UI.Button uiButton;
    [SerializeField] private FloatingNotifier floatingNotifier; // Vẫn giữ phòng hờ

    public override BoosterType GetBoosterType() => BoosterType.Undo;

    private void Awake()
    {
        uiButton = GetComponent<UnityEngine.UI.Button>();
        // Mặc định lúc mới vào game chưa có bước nào, nút Undo bị vô hiệu hóa
        if (uiButton != null) uiButton.interactable = false;
    }

    private void OnEnable()
    {
        GameEventBus.Subscribe<UndoAvailabilityChangedEvent>(OnUndoAvailabilityChanged);
    }

    private void OnDisable()
    {
        GameEventBus.UnSubscribe<UndoAvailabilityChangedEvent>(OnUndoAvailabilityChanged);
    }

    private void OnUndoAvailabilityChanged(UndoAvailabilityChangedEvent evt)
    {
        if (uiButton != null)
        {
            uiButton.interactable = evt.canUndo;
        }
    }

    public override void Excute()
    {
        // 1. Chỉ làm nhiệm vụ BÁO CÁO lên EventBus: "Tôi cần thực thi Undo"
        GameEventBus.Publish(new RequestExecuteBoosterEvent
        {
            boosterType = BoosterType.Undo,
            onComplete = (success) =>
            {
                if (success)
                {
                    // Trừ số lượng Booster và phát âm thanh
                    CoreServices.Get<DataManager>().UseBooster((int)BoosterType.Undo);
                    GameEventBus.Publish(new RequestPlaySFX { soundID = SoundID.HintBooster });
                }
                else
                {
                    // Nếu thất bại (hết bước), hiện thông báo
                    if (floatingNotifier != null) floatingNotifier.ShowWarning("No last move found!");
                }
            }
        });
    }
}
