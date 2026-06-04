using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class ShuffleEffectController : MonoBehaviour
{
    [Header("Shuffle References")]
    [SerializeField] private SlotsManager slotsManager;
    [SerializeField] private Transform centerPivot;
    [SerializeField] private GameObject blackHole;

    private void OnEnable()
    {
        GameEventBus.Subscribe<RequestExecuteBoosterEvent>(OnRequestBooster);
    }

    private void OnDisable()
    {
        GameEventBus.UnSubscribe<RequestExecuteBoosterEvent>(OnRequestBooster);
    }

    private void OnRequestBooster(RequestExecuteBoosterEvent evt)
    {
        if (evt.boosterType == BoosterType.Shuffle)
        {
            ShuffleBlock(evt.onComplete);
        }
    }

    private void ShuffleBlock(System.Action<bool> onComplete)
    {
        List<BlockController> diffcultBLocks = new List<BlockController>();
        Dictionary<int, List<BlockController>> sameBlocks = new Dictionary<int, List<BlockController>>();
        Sequence sequence = DOTween.Sequence();
        
        int moveIndex = 0;
        float moveDuration = 0.4f;
        
        List<BlockController> allPoppedBlocks = new List<BlockController>();
        List<SlotController> originSlots = new List<SlotController>();

        foreach(SlotController slot in slotsManager.GetAllSlots())
        {
            if(slot.isFinished || !slot.isRevealed || slot.slotType == SlotController.SlotType.Ice) continue;
            
            List<BlockController> poppedBlocks = slot.MoveToShuffle(diffcultBLocks, sameBlocks);
            foreach(BlockController block in poppedBlocks)
            {
                allPoppedBlocks.Add(block);
                originSlots.Add(slot);
            }
        }

        float radius = 2f; 
        int totalBlocks = allPoppedBlocks.Count;

        for(int i = 0; i < totalBlocks; i++)
        {
            BlockController block = allPoppedBlocks[i];
            SlotController slot = originSlots[i];

            float angle = i * Mathf.PI * 2f / totalBlocks;
            Vector3 offset = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * radius;
            Vector3 gatherPos = centerPivot.position + offset;
            Vector3[] pathArr = new Vector3[] {slot.arcPeak.transform.position, gatherPos};
            
            block.transform.SetParent(centerPivot);
            block.transform.DOKill();
            sequence.Insert(moveIndex * 0.02f, block.transform.DOPath(pathArr, moveDuration, PathType.CatmullRom).SetEase(Ease.InBack));
            moveIndex++;
        }

        float spinDuration = 1f;
        float spinDelay = moveIndex * 0.02f + moveDuration + 0.1f;
        
        sequence.InsertCallback(spinDelay, () => {
            GameEventBus.Publish(new RequestPlaySFX{soundID = SoundID.Shuffle});
        });
        
        blackHole.transform.localScale = Vector3.zero;
        
        ParticleSystem[] pSystems = blackHole.GetComponentsInChildren<ParticleSystem>(true);
        foreach (var ps in pSystems)
        {
            var main = ps.main;
            main.scalingMode = ParticleSystemScalingMode.Hierarchy;
            ps.Clear();
            ps.Play();
        }

        blackHole.SetActive(true);
        blackHole.transform.DOKill();
        
        blackHole.transform.DOScale(Vector3.one, spinDelay).SetEase(Ease.OutQuad);
        
        float accumulateTime = spinDelay + spinDuration - 0.3f;
        sequence.Insert(accumulateTime, blackHole.transform.DOScale(new Vector3(0.5f, 0.5f, 0.5f), 0.3f).SetEase(Ease.InQuad));

        int rounds = 3;
        sequence.Insert(spinDelay, centerPivot.DORotate(new Vector3(0,0,-360 * rounds), spinDuration, RotateMode.FastBeyond360).SetEase(Ease.InCubic));
        for(int i = 0; i < totalBlocks; i++)
        {
            Transform blockTransform = allPoppedBlocks[i].gameObject.transform;

            sequence.Insert(spinDelay, blockTransform.DOLocalMove(Vector3.zero, spinDuration).SetEase(Ease.InCubic));
            sequence.Insert(spinDelay, blockTransform.DOScale(Vector3.zero, spinDuration).SetEase(Ease.InCubic));
        }

        float endTime = spinDelay + spinDuration;

        List<SlotController> randomSlots = new List<SlotController>(slotsManager.GetAllSlots());
        List<BlockController> initialDifficultBlocks = new List<BlockController>(diffcultBLocks);

        bool foundSolvable = false;
        int attempt = 0;
        
        SlotController[] bestTargetSlots = null;
        List<BlockController> bestDifficultBlocks = null;
        
        while (!foundSolvable && attempt < 100)
        {
            attempt++;
            ShuffleList(randomSlots);
            
            diffcultBLocks = new List<BlockController>(initialDifficultBlocks);
            ShuffleList(diffcultBLocks);

            foreach(var kvp in sameBlocks) 
            {
                diffcultBLocks.AddRange(kvp.Value);
            }

            int index = 0;
            bool assignmentFailed = false;

            Dictionary<SlotController, int> logicalBlocks = new Dictionary<SlotController, int>();
            foreach(var slot in randomSlots) {
                logicalBlocks[slot] = slot.blocks.Count;
            }

            SlotController[] tempTargetSlots = new SlotController[diffcultBLocks.Count];

            // 1. Ensure slots with hidden blocks get at least 1 block
            foreach(var slot in randomSlots) {
                if (slot.blocks.Count > 0 && !slot.blocks.Peek().isRevealed && slot.slotType != SlotController.SlotType.Ice) {
                    if (index < diffcultBLocks.Count) {
                        tempTargetSlots[index] = slot;
                        logicalBlocks[slot]++;
                        index++;
                    }
                }
            }

            // 2. Distribute the rest
            int safeCounter = 0;
            while(index < diffcultBLocks.Count)
            {
                safeCounter++;
                if (safeCounter > 1000) { assignmentFailed = true; break; }

                for(int i = 0; i < randomSlots.Count; i++)
                {
                    if(index >= diffcultBLocks.Count) break;

                    SlotController slot = randomSlots[i];
                    if(logicalBlocks[slot] >= 4 || !slot.isRevealed || slot.slotType == SlotController.SlotType.Ice) continue;

                    tempTargetSlots[index] = slot;
                    logicalBlocks[slot]++;
                    index++;
                }
            }

            if (assignmentFailed) continue;

            // Apply logically
            for(int i = 0; i < diffcultBLocks.Count; i++) {
                tempTargetSlots[i].blocks.Push(diffcultBLocks[i]);
            }

            // Check if solvable
            if (slotsManager.HasAvailableMoves()) {
                foundSolvable = true;
            }
            
            // Revert logical state
            for(int i = diffcultBLocks.Count - 1; i >= 0; i--) {
                tempTargetSlots[i].blocks.Pop();
            }

            // Always save the latest valid assignment in case we don't find a solvable one
            bestTargetSlots = tempTargetSlots;
            bestDifficultBlocks = diffcultBLocks;
        }

        float dropStartTime = endTime + 0.2f;
        float duration = 0.8f;

        if (bestTargetSlots != null && bestDifficultBlocks != null)
        {
            for(int i = 0; i < bestDifficultBlocks.Count; i++)
            {
                BlockController block = bestDifficultBlocks[i];
                SlotController slot = bestTargetSlots[i];
                
                Vector3 destination = new Vector3(slot.stackAnchor.position.x,
                                                slot.stackAnchor.position.y + Constants.BLOCK_HEIGHT * slot.blocks.Count,
                                                slot.stackAnchor.position.z);
                List<Vector3> path = new List<Vector3>{slot.arcPeak.position, destination};

                float absoluteDropTime = dropStartTime + (i * 0.04f); 

                sequence.InsertCallback(absoluteDropTime, () => {
                    block.transform.SetParent(CoreServices.Get<BlocksManager>().transform);
                    CoreServices.Get<HapticManager>().PlayHaptic();
                    block.transform.DOKill(); 
                });
                
                sequence.Insert(absoluteDropTime, block.transform.DOPath(path.ToArray(), duration, PathType.CatmullRom).SetEase(Ease.OutQuad));
                sequence.Insert(absoluteDropTime, block.transform.DOScale(Vector3.one, duration));

                slot.blocks.Push(block);
            }
        }
        
        sequence.InsertCallback(dropStartTime, () => // Blackhole explosion
        {
            blackHole.transform.DOScale(new Vector3(3.0f, 3.0f, 3.0f), 0.2f).SetEase(Ease.OutFlash).OnComplete(() =>
            {
                blackHole.transform.DOScale(Vector3.zero, 0.5f).SetEase(Ease.InBack).OnComplete(() =>
                {
                    blackHole.SetActive(false);
                    blackHole.transform.localScale = Vector3.one;
                });
            });
        });

        sequence.OnComplete(() => {
            centerPivot.rotation = Quaternion.identity;
            onComplete?.Invoke(true);
        });
    }   

    private void ShuffleList<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            T temp = list[i];
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }
}
