using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class PlayerData
{
    public string playerName = "Player";
    public int currentLevel = 0;
    public int totalCoins = 10000;
    public int heart = 5;
    public List<BoosterData> boosters = new List<BoosterData>{new BoosterData(0, "Add Move", 2),
                                                              new BoosterData(1, "Shuffle", 2),
                                                              new BoosterData(2, "Hint", 2)};

}
[System.Serializable]
public class BoosterData
{
    public int id;
    public string name;
    public int count;
    public BoosterData(int id, string name, int count)
    {
        this.id = id;
        this.name = name;
        this.count = count;
    }
}   
