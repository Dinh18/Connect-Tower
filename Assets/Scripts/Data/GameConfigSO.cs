using UnityEngine;
[CreateAssetMenu(fileName = "GameConfigSO", menuName = "Scriptable Objects/GameConfig")]
public class GameConfigSO : ScriptableObject
{
    public int coinRewardEasy;
    public int coinRewardHard;
    public int coinRewardSuperHard;
    public int addMoveCost;
}