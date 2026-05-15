using UnityEngine;
using System.Collections.Generic;
using Firebase.Firestore;

[FirestoreData]
[System.Serializable]
public class PlayerData
{
    [FirestoreProperty] public string playerName {get; set;} = "Player";
    [FirestoreProperty] public string frameID {get; set;} = "0";
    [FirestoreProperty] public string avatarID {get; set;} = "0";
    [FirestoreProperty] public int currStreak {get; set;} = 0;
    [FirestoreProperty] public int maxStreak {get; set;} = 0;
    [FirestoreProperty] public int currentLevel {get; set;} = 0;
    [FirestoreProperty] public bool isFirstTimePlay {get; set;} = true;

    // Nested data groups
    [FirestoreProperty] public WalletData wallet {get; set;} = new WalletData();
    [FirestoreProperty] public InventoryData inventory {get; set;} = new InventoryData();
    [FirestoreProperty] public ProgressData progress {get; set;} = new ProgressData();
}
[FirestoreData]
[System.Serializable]
public class WalletData
{
    [FirestoreProperty] public int totalCoins {get; set;} = 1000;
    [FirestoreProperty] public int heart {get; set;} = 5;
    [FirestoreProperty] public string nextHeartTime {get; set;}
}
[FirestoreData]
[System.Serializable]
public class InventoryData
{
    [FirestoreProperty] public List<BoosterData> boosters {get; set;} = new List<BoosterData>{
        new BoosterData(0, "Add Move", "Use the Extra Move to get extra moves!", 3, 900, 2, false, true),
        new BoosterData(1, "Shuffle", "Use it to shuffle the board!", 3, 800, 3, false, true),
        new BoosterData(2, "Hint", "Use it to reveal a correct placement", 3, 900,13, false, true),
        new BoosterData(3, "Undo", "Back previous step", 3, 200, -10,true, false)
    };
}
[FirestoreData]
[System.Serializable]
public class ProgressData
{
    [FirestoreProperty] public List<MechanicData> mechanics{get;set;} = new List<MechanicData>{
        new MechanicData(0, "Block Hide", 6, true),
        new MechanicData(1, "Slot Hide", 17, true),
        new MechanicData(2, "Ice Slot", 20, true)
    };
}
