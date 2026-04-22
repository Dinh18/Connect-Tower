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
