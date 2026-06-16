using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HeatManager : MonoBehaviour
{
    [SerializeField] private float heatIncreaseRate = 10f; // Tốc độ tăng nhiệt
    [SerializeField] private float heatDecreaseRate = 20f; // Tốc độ giảm nhiệt
    private float currentHeat = 0f; // Nhiệt độ hiện tại
    private float maxHeat = 100f; // Nhiệt độ tối đa
    private List<SlotController> fireSlots;
    private float time = 0f;
    [SerializeField] private Slider heatSlider; // Tham chiếu đến UI Slider để hiển thị nhiệt độ
    public void Setup(List<SlotController> slots)
    {
        fireSlots = new List<SlotController>();
        foreach(var slot in slots)
        {
            if(slot.slotType == SlotController.SlotType.Fire)
            {
                fireSlots.Add(slot);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(fireSlots == null || fireSlots.Count <= 0) return;

        if(GetTotalBlocksOnFireSlots() > 0)
        {
            time += Time.deltaTime;
            if(time > 1f)
            {
                currentHeat = Mathf.Clamp(currentHeat + heatIncreaseRate * Time.deltaTime * GetTotalBlocksOnFireSlots(), 0, maxHeat);
                Debug.Log("Increase Heat: " + currentHeat);
            }
        }
        else
        {
            time = 0;
            currentHeat = Mathf.Clamp(currentHeat - heatDecreaseRate * Time.deltaTime, 0, maxHeat);
            Debug.Log("Decrease Heat: " + currentHeat);
        }
        UpdateHeatUI();
    }

    private int GetTotalBlocksOnFireSlots()
    {
        int totalBlocks = 0;
        foreach(var slot in fireSlots)
        {
            totalBlocks += slot.blocks.Count;
        }
        return totalBlocks;
    }

    private void UpdateHeatUI()
    {
        if(heatSlider != null)
        {
            heatSlider.value = currentHeat / maxHeat;
        }
    }
}
