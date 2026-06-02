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

        int totalBlocks = allPoppedBlocks.Count;
        float shakeDuration = 1.0f;
        float pullDuration = 1.2f; // Time for each block to fall into the black hole
        Vector3 centerPos = centerPivot.position;

        for(int i = 0; i < totalBlocks; i++)
        {
            BlockController block = allPoppedBlocks[i];
            SlotController slot = originSlots[i];

            block.transform.SetParent(centerPivot);
            block.transform.DOKill();
            
            // 1. Shake effect
            sequence.Insert(0, block.transform.DOShakePosition(shakeDuration, new Vector3(0.15f, 0.15f, 0), 25, 90f, false, true));
            sequence.Insert(0, block.transform.DOShakeRotation(shakeDuration, new Vector3(0, 0, 20f), 25, 90f, true));

            // 2. Spiral Path Calculation
            int spiralPointsCount = 10;
            Vector3[] pathArr = new Vector3[spiralPointsCount + 1];
            
            // First point MUST be arcPeak so it flies up out of the slot first
            pathArr[0] = slot.arcPeak.transform.position; 
            
            Vector3 startSpiralPos = slot.arcPeak.transform.position;
            Vector3 toStart = startSpiralPos - centerPos;
            float initialRadius = toStart.magnitude;
            
            // Add some randomness so blocks don't follow perfectly uniform curves
            float initialAngle = Mathf.Atan2(toStart.y, toStart.x) + Random.Range(-0.5f, 0.5f);
            
            float totalRounds = 4.0f; // Orbits around the center
            float totalAngle = totalRounds * Mathf.PI * 2f;
            
            for(int j = 0; j < spiralPointsCount; j++)
            {
                float t = (j + 1) / (float)spiralPointsCount; 
                
                float tAngle = t * t; // Angle speeds up (EaseIn)
                float currentRadius = initialRadius * (1f - t); // Radius shrinks linearly
                if (j == spiralPointsCount - 1) currentRadius = 0;

                float currentAngle = initialAngle + totalAngle * tAngle;
                
                Vector3 point = centerPos + new Vector3(Mathf.Cos(currentAngle), Mathf.Sin(currentAngle), 0) * currentRadius;
                pathArr[j + 1] = point;
            }

            float absoluteStartTime = shakeDuration + moveIndex * 0.04f; // staggered start

            // 3. Move along spiral, scale down, and tumble
            sequence.Insert(absoluteStartTime, block.transform.DOPath(pathArr, pullDuration, PathType.CatmullRom).SetEase(Ease.InSine));
            sequence.Insert(absoluteStartTime, block.transform.DOScale(Vector3.zero, pullDuration).SetEase(Ease.InSine));
            sequence.Insert(absoluteStartTime, block.transform.DORotate(new Vector3(0, 0, 1080f), pullDuration, RotateMode.FastBeyond360).SetRelative(true).SetEase(Ease.InQuad));
            
            moveIndex++;
        }

        float endTime = shakeDuration + (moveIndex * 0.04f) + pullDuration;
        
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
        
        // Blackhole appears aggressively during the shake
        blackHole.transform.DOScale(Vector3.one, shakeDuration + 0.2f).SetEase(Ease.OutBack);
        
        // Blackhole contracts slightly when the last blocks are entering
        float accumulateTime = endTime - 0.2f;
        sequence.Insert(accumulateTime, blackHole.transform.DOScale(new Vector3(0.5f, 0.5f, 0.5f), 0.2f).SetEase(Ease.InQuad));

        List<SlotController> randomSlots = new List<SlotController>(slotsManager.GetAllSlots());
        ShuffleList(randomSlots);
        ShuffleList(diffcultBLocks);

        foreach(var kvp in sameBlocks) 
        {
            diffcultBLocks.AddRange(kvp.Value);
        }

        int index = 0;
        int safeCounter = 0; 

        float dropStartTime = endTime + 0.2f;
        float duration = 0.8f;

        while(index < diffcultBLocks.Count)
        {
            safeCounter++;
            if (safeCounter > 1000) break;

            for(int i = 0; i < randomSlots.Count; i++)
            {
                if(index >= diffcultBLocks.Count) break;

                if(randomSlots[i].blocks.Count >= 4 || !randomSlots[i].isRevealed || randomSlots[i].slotType == SlotController.SlotType.Ice) continue;

                BlockController block = diffcultBLocks[index];
                SlotController slot = randomSlots[i];
                
                Vector3 destination = new Vector3(slot.stackAnchor.position.x,
                                                slot.stackAnchor.position.y + Constants.BLOCK_HEIGHT * slot.blocks.Count,
                                                slot.stackAnchor.position.z);
                List<Vector3> path = new List<Vector3>{slot.arcPeak.position, destination};

                float absoluteDropTime = dropStartTime + (index * 0.04f); 

                sequence.InsertCallback(absoluteDropTime, () => {
                    block.transform.SetParent(CoreServices.Get<BlocksManager>().transform);
                    block.transform.DOKill(); 
                });
                
                sequence.Insert(absoluteDropTime, block.transform.DOPath(path.ToArray(), duration, PathType.CatmullRom).SetEase(Ease.OutQuad));
                sequence.Insert(absoluteDropTime, block.transform.DOScale(Vector3.one, duration));

                slot.blocks.Push(block);
                index++;
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
