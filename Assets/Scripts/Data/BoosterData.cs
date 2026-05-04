[System.Serializable]
public class BoosterData
{
    public int id;
    public string name;
    public string description;
    public int count;
    public int price;
    public int unlockedLevel;
    public bool isUnlocked;
    public bool isFirstTime;

    public BoosterData(int id, string name, string description, int count, int price, int unlockedLevel, bool isUnlocked, bool isFirstTime)
    {
        this.id = id;
        this.name = name;
        this.description = description;
        this.count = count;
        this.price = price;

        this.unlockedLevel = unlockedLevel;
        this.isUnlocked = isUnlocked;
        this.isFirstTime = isFirstTime;
    }
}
