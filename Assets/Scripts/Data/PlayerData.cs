using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class PlayerData
{
    public string playerName = "Player";
    public int currentLevel = 0;
    public bool isFirstTimePlay = true;

    // Nested data groups
    public WalletData wallet = new WalletData();
    public InventoryData inventory = new InventoryData();
    public ProgressData progress = new ProgressData();
}

[System.Serializable]
public class WalletData
{
    public int totalCoins = 1000;
    public int heart = 5;
    public string nextHeartTime;
}

[System.Serializable]
public class InventoryData
{
    public List<BoosterData> boosters = new List<BoosterData>{
        new BoosterData(0, "Add Move", 3, 3, false, true),
        new BoosterData(1, "Shuffle", 3, 9, false, true),
        new BoosterData(2, "Hint", 3, 13, false, true)
    };
}

[System.Serializable]
public class ProgressData
{
    public List<MechanicData> mechanics = new List<MechanicData>{
        new MechanicData(0, "Block Hide", 6, true),
        new MechanicData(1, "Slot Hide", 17, true),
        new MechanicData(2, "Ice Slot", 20, true)
    };
}
