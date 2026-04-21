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
    public static event Action<int, int> OnChangeFinishedSlots;
    void Awake()
    {
        slotPrefab = Resources.Load<GameObject>(Constants.SLOT_PREFAB_PATH);
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
        OnChangeFinishedSlots?.Invoke(finishedTopic,numsTopic);
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
        OnChangeFinishedSlots?.Invoke(finishedTopic, numsTopic);
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
}

