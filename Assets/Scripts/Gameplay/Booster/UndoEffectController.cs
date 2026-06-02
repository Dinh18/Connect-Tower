using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class UndoEffectController : MonoBehaviour
{
    private void OnEnable()
    {
        GameEventBus.Subscribe<RequestExecuteBoosterEvent>(OnRequestExecuteBooster);
    }

    private void OnDisable()
    {
        GameEventBus.UnSubscribe<RequestExecuteBoosterEvent>(OnRequestExecuteBooster);
    }

    private void OnRequestExecuteBooster(RequestExecuteBoosterEvent evt)
    {
        if (evt.boosterType != BoosterType.Undo) return;

        // 1. Lấy thông tin bước đi trước đó từ GameplayController
        MoveStep preMoveStep = CoreServices.Get<GamePlayController>().PreMoveStep();
        if (preMoveStep == null)
        {
            Debug.Log("No last move found");
            evt.onComplete?.Invoke(false); // Thất bại
            return;
        }

        // 2. Xử lý logic Undo
        SlotController sourceSlot = preMoveStep.sourceSlot;
        SlotController targetSlot = preMoveStep.targetSlot;
        int numsBlock = preMoveStep.numsBlock;
        float startY = (sourceSlot.blocks.Count == 0) ? sourceSlot.stackAnchor.position.y : sourceSlot.blocks.Peek().transform.position.y + sourceSlot.height;
        
        Sequence undoSeq = DOTween.Sequence();

        for (int i = 0; i < numsBlock; i++)
        {
            // Kỹ năng phòng thủ: Check xem targetSlot có còn block nào để rút ra không
            if (targetSlot.blocks.Count == 0) 
            {
                Debug.LogWarning("Target slot đã trống, không thể Undo thêm!");
                break; 
            }

            // Rút block trên cùng của cọc Đích ra và nhét về cọc Nguồn
            var block = targetSlot.blocks.Pop();
            sourceSlot.blocks.Push(block);

            // Xử lý Animation DOTween
            List<Vector3> pathArr = sourceSlot.PathToMoveBlock(targetSlot, i, startY);

            block.transform.DOKill(); // Ngắt mọi animation cũ để tránh xung đột
            
            float delay = i * 0.2f; // Increased delay for more deliberate sequencing
            float flightDuration = 0.8f; // Slower flight

            // Rung rinh và phình to lên một chút để chuẩn bị bay ngược
            undoSeq.Insert(delay, block.transform.DOShakeRotation(0.3f, new Vector3(0, 0, 25f), 20));
            undoSeq.Insert(delay, block.transform.DOScale(new Vector3(1.2f, 1.2f, 1.2f), 0.25f).SetLoops(2, LoopType.Yoyo));

            // Bay lùi theo đường cong (Dùng Ease.InOutBack để có cảm giác giật về)
            undoSeq.Insert(delay + 0.3f, block.transform.DOPath(pathArr.ToArray(), flightDuration, PathType.CatmullRom).SetEase(Ease.InOutBack));
            undoSeq.InsertCallback(delay + 0.3f, () => {
                GameEventBus.Publish(new RequestPlaySFX{soundID = SoundID.MoveWoosh});
            });
            
            // Xoay lộn vòng ngược chiều kim đồng hồ để tạo cảm giác "Rewind" (quay ngược thời gian)
            undoSeq.Insert(delay + 0.3f, block.transform.DORotate(new Vector3(0, 0, -360f), flightDuration, RotateMode.FastBeyond360).SetRelative(true).SetEase(Ease.InOutQuad));
            
            // Khi tiếp đất ở vị trí cũ thì nhún nảy (Squash & Stretch)
            undoSeq.Insert(delay + 0.3f + flightDuration, block.transform.DOScale(new Vector3(1.1f, 0.9f, 1.1f), 0.15f).SetLoops(2, LoopType.Yoyo));
        }
        
        Debug.Log($"Undo thành công: Trả {numsBlock} block từ {targetSlot.name} về {sourceSlot.name}");
        
        // 3. Trả về kết quả thành công cho Booster gọi tới sau khi TOÀN BỘ animation hoàn tất
        undoSeq.OnComplete(() => {
            evt.onComplete?.Invoke(true);
        });
    }
}
