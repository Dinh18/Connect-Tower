using System;
using System.Collections.Generic;
using UnityEngine;


public class SlotsManager : MonoBehaviour
{
    [SerializeField] private Transform gridRoot;
    private LevelLoader levelLoader;
    private int finishedTopic;
    private int numsTopic;
    public int row1;
    public int row2;
    private Stack<GameObject> slotPool = new Stack<GameObject>();
    private GameObject slotPrefab;
    private bool levelCompleted;
    // Event handled by GameEventBus
    void Awake()
    {
        CoreServices.Register<SlotsManager>(this);
    }
    void Start()
    {
        this.levelLoader = CoreServices.Get<LevelLoader>(); 
    }
    void OnEnable()
    {
        SlotController.OnSlotCompleted += CheckLevelComplete;
    }
    void OnDisable()
    {
        SlotController.OnSlotCompleted -= CheckLevelComplete;
    }
    public void PoolSlot(int numsSlot)
    {
        if(slotPrefab == null) slotPrefab = Resources.Load<GameObject>(Constants.SLOT_PREFAB_PATH);
        for(int i = 0; i < numsSlot; i++)
        {
            GameObject slot = Instantiate(slotPrefab, gridRoot);
            slot.SetActive(false);
            slotPool.Push(slot);
        }
    }

    public List<SlotController> GetAllSlots() => levelLoader.slots; 

    public void SlotsGenerate(int row1, int row2, List<SlotController> slots, List<SlotSetupData> slotSetup, int numsTopic)
    {
        finishedTopic = 0;
        this.numsTopic = numsTopic;
        GameEventBus.Publish(new FinishedSlotsUpdatedEvent { finishedSlots = finishedTopic, totalSlots = numsTopic });
        foreach(Transform child in gridRoot.transform)
        {
            if(child.gameObject.activeSelf)
            {
                child.gameObject.SetActive(false);
                slotPool.Push(child.gameObject);
            }
        }
        this.row1 = row1;
        this.row2 = row2;
        int j = 0;
        float startX_Row1 = -(row1 - 1) * Constants.SLOT_WIDTH / 2f; 
        for(int i = 0; i < row1; i++)
        {
            GameObject slot;
            if(slotPool.Count <= 0)
            {
                slot = Instantiate(slotPrefab, gridRoot);
                slot.SetActive(false);
            }
            else
            {
                slot = slotPool.Pop();
                slot.SetActive(true);
            }
            slot.name = "Slot_0_" + i;
            slot.transform.localPosition = new Vector3(startX_Row1 + (i * Constants.SLOT_WIDTH), 0, 0);
            SlotController s = slot.GetComponent<SlotController>();
            if(slotSetup[j] == null) return;
            s.Setup(slotSetup[j].slotType, 0,slotSetup[j].questionTopic ? slotSetup[j].questionTopic : null);
            slot.SetActive(true);
            slots.Add(s);
            j++;
        }
        float startX_Row2 = -(row2 - 1) * Constants.SLOT_WIDTH / 2f;

        for(int i = 0; i < row2; i++)
        {
            GameObject slot;
            if(slotPool.Count <= 0)
            {
                slot = Instantiate(slotPrefab, gridRoot);
                slot.SetActive(false);
            }
            else
            {
                slot = slotPool.Pop();
                slot.SetActive(true);
            }
            slot.name = "Slot_1_" + i;

            slot.transform.localPosition = new Vector3(startX_Row2 + (i * Constants.SLOT_WIDTH), Constants.SLOT_HEIGHT, 0);

            SlotController s = slot.GetComponent<SlotController>();
            s.Setup(slotSetup[j].slotType, 1,slotSetup[j].questionTopic ? slotSetup[j].questionTopic : null);
            slots.Add(s);
            slot.SetActive(true);
            j++;
        }
        levelCompleted = false;
    }

    private void CheckLevelComplete(int topicID)
    {
        finishedTopic++;
        GameEventBus.Publish(new FinishedSlotsUpdatedEvent { finishedSlots = finishedTopic, totalSlots = numsTopic });
        foreach(SlotController slot in levelLoader.slots)
        {
            if(!slot.isRevealed && slot.blockTopic.topicID == topicID){
                slot.Reveal();
                return;
            }
        }
        foreach(SlotController slot in levelLoader.slots)
        {
            if(!slot.isFinished && slot.blocks.Count > 0){
                Debug.Log("Haven't Completed");
                return;
            }
        }
        levelCompleted = true;
        CoreServices.Get<GameManager>().ChangeState(GameManager.GameState.Win);
        levelLoader.LevelUp();
    }

    public bool GetLevelComleted() => levelCompleted;

    private struct SimState : IEquatable<SimState>
    {
        public uint[] slots;

        public bool Equals(SimState other)
        {
            if (slots == null || other.slots == null || slots.Length != other.slots.Length) return false;
            for (int i = 0; i < slots.Length; i++)
            {
                if (slots[i] != other.slots[i]) return false;
            }
            return true;
        }

        public override int GetHashCode()
        {
            if (slots == null) return 0;
            unchecked
            {
                int hash = 17;
                for (int i = 0; i < slots.Length; i++)
                {
                    hash = hash * 31 + (int)slots[i];
                }
                return hash;
            }
        }
    }

    private uint EncodeSlot(SlotController slot)
    {
        uint encoded = 0;
        BlockController[] arr = slot.blocks.ToArray();
        Array.Reverse(arr); // Stack enum order is top to bottom, we want bottom to top (index 0 is bottom)

        for (int j = 0; j < arr.Length; j++)
        {
            byte b;
            if (!arr[j].isRevealed) b = 255;
            else b = (byte)(arr[j].GetTopicID() + 1);
            encoded |= ((uint)b << (j * 8));
        }
        return encoded;
    }

    private int GetBlockCount(uint slot)
    {
        if ((slot & 0xFF000000) != 0) return 4;
        if ((slot & 0x00FF0000) != 0) return 3;
        if ((slot & 0x0000FF00) != 0) return 2;
        if ((slot & 0x000000FF) != 0) return 1;
        return 0;
    }

    private byte GetTopBlock(uint slot, int count)
    {
        if (count == 0) return 0;
        return (byte)((slot >> ((count - 1) * 8)) & 0xFF);
    }

    private byte GetBlockAt(uint slot, int index)
    {
        return (byte)((slot >> (index * 8)) & 0xFF);
    }

    private uint RemoveTopBlock(uint slot, int count)
    {
        uint mask = ~(255u << ((count - 1) * 8));
        return slot & mask;
    }

    private uint AddTopBlock(uint slot, int currentCount, byte block)
    {
        return slot | ((uint)block << (currentCount * 8));
    }

    public bool HasAvailableMoves()
    {
        List<SlotController> activeSlots = new List<SlotController>();
        foreach (var slot in levelLoader.slots)
        {
            if (!slot.isFinished && slot.isRevealed)
            {
                activeSlots.Add(slot);
            }
        }

        int numSlots = activeSlots.Count;
        if (numSlots == 0) return false;

        uint[] initialState = new uint[numSlots];
        bool[] isIceSlot = new bool[numSlots];
        for (int i = 0; i < numSlots; i++)
        {
            initialState[i] = EncodeSlot(activeSlots[i]);
            isIceSlot[i] = (activeSlots[i].slotType == SlotController.SlotType.Ice);
        }

        HashSet<SimState> visited = new HashSet<SimState>();
        Queue<SimState> queue = new Queue<SimState>();

        SimState startState = new SimState { slots = initialState };
        visited.Add(startState);
        queue.Enqueue(startState);

        int maxSearch = 2000;
        int searchCount = 0;

        while (queue.Count > 0)
        {
            SimState curr = queue.Dequeue();
            searchCount++;

            if (searchCount > maxSearch)
            {
                // Quá phức tạp, tạm coi là còn chơi được để tránh treo máy
                return true;
            }

            for (int src = 0; src < numSlots; src++)
            {
                if (isIceSlot[src]) continue; // Không thể lấy block từ Ice slot

                uint srcSlot = curr.slots[src];
                if (srcSlot == 0) continue; // slot rỗng

                int srcCount = GetBlockCount(srcSlot);
                byte srcTop = GetTopBlock(srcSlot, srcCount);

                if (srcTop == 255) continue; // block ẩn không thể bị di chuyển

                // Đếm số lượng block giống nhau liên tiếp ở trên cùng
                int sameColorCount = 1;
                for (int i = srcCount - 2; i >= 0; i--)
                {
                    if (GetBlockAt(srcSlot, i) == srcTop) sameColorCount++;
                    else break;
                }

                for (int dst = 0; dst < numSlots; dst++)
                {
                    if (src == dst) continue;

                    uint dstSlot = curr.slots[dst];
                    int dstCount = GetBlockCount(dstSlot);

                    if (dstCount == 4) continue; // slot đích đã đầy

                    byte dstTop = dstCount > 0 ? GetTopBlock(dstSlot, dstCount) : (byte)0;

                    // Có thể chuyển nếu slot đích rỗng hoặc block trên cùng giống nhau
                    if (dstCount == 0 || dstTop == srcTop)
                    {
                        int moveCount = Math.Min(4 - dstCount, sameColorCount);
                        
                        // Kiểm tra xem nước đi này có tạo ra "Bước tiến" (Progress) không?
                        // 1. Slot đích có được lấp đầy (4 block cùng loại) không?
                        bool isAllSame = true;
                        for (int i = 0; i < dstCount; i++)
                        {
                            if (GetBlockAt(dstSlot, i) != srcTop)
                            {
                                isAllSame = false;
                                break;
                            }
                        }

                        if (isAllSame && (dstCount + moveCount == 4))
                        {
                            return true; // Progress: Hoàn thành 1 slot!
                        }

                        // 2. Slot nguồn có để lộ ra một block đang bị ẩn không?
                        int newSrcCount = srcCount - moveCount;
                        if (newSrcCount > 0)
                        {
                            byte newSrcTop = GetTopBlock(srcSlot, newSrcCount);
                            if (newSrcTop == 255)
                            {
                                return true; // Progress: Mở khóa block ẩn!
                            }
                        }

                        // Nếu không tạo ra progress, thực hiện nước đi và thêm vào hàng đợi
                        uint newSrc = srcSlot;
                        uint newDst = dstSlot;

                        for (int m = 0; m < moveCount; m++)
                        {
                            newSrc = RemoveTopBlock(newSrc, srcCount - m);
                            newDst = AddTopBlock(newDst, dstCount + m, srcTop);
                        }

                        uint[] nextSlots = new uint[numSlots];
                        Array.Copy(curr.slots, nextSlots, numSlots);
                        nextSlots[src] = newSrc;
                        nextSlots[dst] = newDst;

                        SimState nextState = new SimState { slots = nextSlots };
                        if (visited.Add(nextState))
                        {
                            queue.Enqueue(nextState);
                        }
                    }
                }
            }
        }

        // Đã thử hết mọi nước đi khả thi nhưng không tạo ra được progress nào => Deadlock
        return false;
    }
}

