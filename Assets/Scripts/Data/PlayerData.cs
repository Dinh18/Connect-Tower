using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class PlayerData
{
    public string playerName = "Player";
    public int currentLevel = 0;
    public int totalCoins = 10000;
    public int heart = 5;
    public string nextHeartTime;
    public List<BoosterData> boosters = new List<BoosterData>{new BoosterData(0, "Add Move", 3, 1, false, true),
                                                              new BoosterData(1, "Shuffle", 3, 2, false, true),
                                                              new BoosterData(2, "Hint", 3, 3, false, true)};
    public bool isFirstTimePlay = true;

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
