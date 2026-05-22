using UnityEngine;

[CreateAssetMenu(fileName = "NewBooster", menuName = "Scriptable Objects/Booster Data")]
public class BoosterDataSO : ScriptableObject
{
    public string id;
    public string nameBooster;
    public string description;
    public int price;
    public int unlockedLevel;
}
