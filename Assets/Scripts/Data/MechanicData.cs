

// [FirestoreData]
// [System.Serializable]
using UnityEngine;
[CreateAssetMenu(fileName = "NewMechanic", menuName = "Scriptable Objects/Mechanic Data")]
public class MechanicData : ScriptableObject
{
    public string id;
    public string nameMechanic;
    public string instruction;
    public int levelUnclock;
    // public bool isFirstTimePlay;
    // public MechanicData(){}

    // public MechanicData(int id, string name, int levelUnclock, bool firstTimePlay)
    // {
    //     this.id = id;
    //     this.nameMechanic = nameMechanic;
    //     this.levelUnclock = levelUnclock;
    //     this.isFirstTimePlay = firstTimePlay;
    // }
}
