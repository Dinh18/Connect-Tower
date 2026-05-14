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
            block.transform.DOPath(pathArr.ToArray(), 0.5f, PathType.CatmullRom)
                .SetDelay(i * 0.1f) // Tạo độ trễ nối tiếp nhau rất hay
                .SetEase(Ease.InOutQuad);
        }
        
        Debug.Log($"Undo thành công: Trả {numsBlock} block từ {targetSlot.name} về {sourceSlot.name}");
        
        // 3. Trả về kết quả thành công cho Booster gọi tới (để trừ tiền, báo SFX)
        evt.onComplete?.Invoke(true);
    }
}
