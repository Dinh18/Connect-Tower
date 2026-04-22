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
