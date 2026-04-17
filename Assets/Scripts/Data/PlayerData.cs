using UnityEngine;
using System.Collections.Generic;
using JetBrains.Annotations;

[System.Serializable]
public class PlayerData
{
    public string playerName = "Player";
    public int currentLevel = 0;
    public int totalCoins = 1000;
    public int heart = 5;
    public string nextHeartTime;
    public List<BoosterData> boosters = new List<BoosterData>{new BoosterData(0, "Add Move", 3, 3, false, true),
                                                              new BoosterData(1, "Shuffle", 3, 9, false, true),
                                                              new BoosterData(2, "Hint", 3, 13, false, true)};
    public List<MechanicData> mechanics = new List<MechanicData>{new MechanicData(0, "Block Hide", 6, true),
                                                                 new MechanicData(1, "Slot Hide", 17, true),
                                                                 new MechanicData(2, "Ice Slot", 20, true)};                                                   
    public bool isFirstTimePlay = true;

}
[System.Serializable]
public class MechanicData
{
    public int id;
    public string name;
    public int levelUnclock;
    public bool isFirstTimePlay;
    public MechanicData(int id, string name, int levelUnclock, bool firstTimePlay)
    {
        this.id = id;
        this.name = name;
        this.levelUnclock = levelUnclock;
        this.isFirstTimePlay = firstTimePlay;
    }
}
[System.Serializable]
public class BoosterData
{
    public int id;
    public string name;
    public int count;
    public int unlockedLevel;
    public bool isUnlocked;
    public bool isFirstTime;
    public BoosterData(int id, string name, int count, int unlockedLevel, bool isUnlocked, bool isFirstTime)
    {
        this.id = id;
        this.name = name;
        this.count = count;
        this.unlockedLevel = unlockedLevel;
        this.isUnlocked = isUnlocked;
        this.isFirstTime = isFirstTime;
    }
}   
