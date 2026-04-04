using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Android.Gradle;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class SlotsManager : MonoBehaviour
{
    [SerializeField] private Transform gridRoot;
    private LevelLoader levelLoader;
    private int finishedTopic;
    private int numsTopic;
    public int row1;
    public int row2;
    public static event Action<int, int> OnChangeFinishedSlots;
    void OnEnable()
    {
        SlotController.OnSlotCompleted += CheckLevelComplete;
    }
    void OnDisable()
    {
        SlotController.OnSlotCompleted -= CheckLevelComplete;
    }

    public void Setup(LevelLoader levelLoader)
    {
        this.levelLoader = levelLoader;   
    }

    public List<SlotController> GetAllSlots() => levelLoader.slots; 

    public void SlotsGenerate(int row1, int row2, List<SlotController> slots, List<SlotSetupData> slotSetup, int numsTopic)
    {
        finishedTopic = 0;
        this.numsTopic = numsTopic;
        OnChangeFinishedSlots?.Invoke(finishedTopic,numsTopic);
        foreach(Transform child in gridRoot.transform)
        {
            Destroy(child.gameObject);
        }
        this.row1 = row1;
        this.row2 = row2;
        int j = 0;
        // slots = new List<Slot>();
        GameObject slotPrefab = Resources.Load<GameObject>(Constants.SLOT_PREFAB_PATH);
        float startX_Row1 = -(row1 - 1) * Constants.SLOT_WIDTH / 2f; 

        for(int i = 0; i < row1; i++)
        {
            GameObject slot = GameObject.Instantiate(slotPrefab, gridRoot);
            slot.name = "Slot_0_" + i;
            
            slot.transform.localPosition = new Vector3(startX_Row1 + (i * Constants.SLOT_WIDTH), 0, 0);

            SlotController s = slot.GetComponent<SlotController>();
            s.Setup(slotSetup[j].slotType, slotSetup[j].questionTopic ? slotSetup[j].questionTopic : null);
            slots.Add(s);
            j++;
        }
        float startX_Row2 = -(row2 - 1) * Constants.SLOT_WIDTH / 2f;

        for(int i = 0; i < row2; i++)
        {
            GameObject slot = GameObject.Instantiate(slotPrefab, gridRoot);
            slot.name = "Slot_1_" + i;

            slot.transform.localPosition = new Vector3(startX_Row2 + (i * Constants.SLOT_WIDTH), Constants.SLOT_HEIGHT, 0);

            SlotController s = slot.GetComponent<SlotController>();
            s.Setup(slotSetup[j].slotType, slotSetup[j].questionTopic ? slotSetup[j].questionTopic : null);
            slots.Add(s);
            j++;
        }
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
        GameManager.Instance.ChangeState(GameManager.GameState.Win);
        levelLoader.LevelUp();
    }
}

