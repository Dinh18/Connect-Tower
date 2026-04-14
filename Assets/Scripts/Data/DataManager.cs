using System.IO;
using UnityEngine;
using UnityEditor;
using System;

public class DataManager : MonoBehaviour
{
    public static DataManager Instance;
    public PlayerData playerData;
    private string saveFilePath;
    public static event Action<int> OnChangeCoins;
    public static event Action<int> OnChangeLevel;
    public static event Action<int,int> OnChangeCountBooster;
    public static event Action<int> OnChangeHeart;
    void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this.gameObject);
            return;
        }

        saveFilePath = Path.Combine(Application.persistentDataPath, "saveData.json");
        LoadGame();
    }

    void Start()
    {
        
    }

    public void SaveGame()
    {
        // chuyen palyer data thanh chuoi json
        // true de co dinh dang de doc hon
        string jsonString = JsonUtility.ToJson(playerData, true);
        // Ghi chuoi json vao file
        File.WriteAllText(saveFilePath, jsonString);
        Debug.Log("<color=green>Đã lưu game thành công tại: </color>" + saveFilePath);
    }

    public void LoadGame()
    {
        if(File.Exists(saveFilePath))
        {
            // Doc chuoi json tu file
            string jsonString = File.ReadAllText(saveFilePath);
            // Chuyen chuoi json thanh doi tuong player data
            playerData = JsonUtility.FromJson<PlayerData>(jsonString);

            Debug.Log("<color=green>Đã tải game thành công từ: </color>" + saveFilePath);
        }
        else
        {
            playerData = new PlayerData(); // Khoi tao du lieu mac dinh neu khong co file luu

            Debug.LogWarning("<color=red>Không tìm thấy file lưu game tại: </color>" + saveFilePath);

            SaveGame();
        }
    }

    [ContextMenu("Delete Save Data")]
    public void DeleteData()
    {
        if(File.Exists(saveFilePath))
        {
            File.Delete(saveFilePath);
            playerData = new PlayerData(); // Khoi tao du lieu mac dinh sau khi xoa file luu
            Debug.Log("<color=green>Đã xóa dữ liệu game tại: </color>" + saveFilePath);
        }
    }
    public int GetAmountOfBoosterByID(int id)
    {
        return playerData.boosters[id].count;
    }
    public void UseBooster(int id)
    {
        for(int i = 0; i < playerData.boosters.Count; i++)
        {
            if(playerData.boosters[i].id == id)
            {
                playerData.boosters[i].count--;
                SaveGame();
                OnChangeCountBooster?.Invoke(id, playerData.boosters[i].count);
                return;
            }
        }
    }

    public void AddBooster(int id, int amount, int price)
    {
        for(int i = 0; i < playerData.boosters.Count; i++)
        {
            if(playerData.boosters[i].id == id)
            {
                playerData.boosters[i].count += amount;
                UseCoins(price);
                SaveGame();
                OnChangeCountBooster?.Invoke(id, playerData.boosters[i].count);
                return;
            }
        }
        
    }
    public void AddCoins(int amount)
    {
        playerData.totalCoins += amount;
        SaveGame();
        OnChangeCoins?.Invoke(playerData.totalCoins);
    }
    public void UseCoins(int amount)
    {
        playerData.totalCoins -= amount;
        SaveGame();
        OnChangeCoins?.Invoke(playerData.totalCoins);
    }
    public void UseHeart(string nextHeartTime)
    {
        if(playerData.heart > 0)
        {
            playerData.heart--;
            playerData.nextHeartTime = nextHeartTime;
            SaveGame();
            OnChangeHeart?.Invoke(playerData.heart);
        }
    }

    public void AddHeart(int amount, string nextHeartTime)
    {
        playerData.heart = Math.Min(playerData.heart+=amount, 5);
        playerData.nextHeartTime = nextHeartTime;
        SaveGame();
        OnChangeHeart?.Invoke(playerData.heart);
    }


    public void LevelUp(LevelLoader.GameDifficult gameDifficult, int maxLevel)
    {
        if(playerData.currentLevel < maxLevel)
        {
            playerData.currentLevel++;
        }
        switch(gameDifficult)
        {
            case LevelLoader.GameDifficult.Easy:
                AddCoins(40);
                break;
            case LevelLoader.GameDifficult.Hard:
                AddCoins(80);
                break;
            case LevelLoader.GameDifficult.VeryHard:
                AddCoins(120);
                break;
        }
        if(playerData.currentLevel == playerData.boosters[(int) Constants.BoosterType.AddMove].unlockedLevel)
        {
            UnlockBooster((int) Constants.BoosterType.AddMove);
        }
        if(playerData.currentLevel == playerData.boosters[(int) Constants.BoosterType.Shuffle].unlockedLevel)
        {
            UnlockBooster((int) Constants.BoosterType.Shuffle);
        }
        if(playerData.currentLevel == playerData.boosters[(int) Constants.BoosterType.Hint].unlockedLevel)
        {
            UnlockBooster((int) Constants.BoosterType.Hint);
        }
        SaveGame();
        OnChangeLevel?.Invoke(playerData.currentLevel);
    }

    public void UnlockBooster(int id)
    {
        for(int i = 0; i < playerData.boosters.Count; i++)
        {
            if(playerData.boosters[i].id == id)
            {
                playerData.boosters[i].isUnlocked = true;
                return;
            }
        }
        Debug.Log("Khong tim thay booster");
    }

    public bool IsUnLockedBooster(int id)
    {
        for(int i = 0; i < playerData.boosters.Count; i++)
        {
            if(playerData.boosters[i].id == id)
            {
                // playerData.boosters[i].isUnlocked = true;
                return playerData.boosters[i].isUnlocked;
            }
        }
        Debug.Log("Khong tim thay booster");
        return false;
    }

    public bool IsFirstTimeUserBooster(int id)
    {
        for(int i = 0; i < playerData.boosters.Count; i++)
        {
            if(playerData.boosters[i].id == id)
            {
                // playerData.boosters[i].isUnlocked = true;
                return playerData.boosters[i].isFirstTime;
            }
        }
        Debug.Log("Khong tim thay booster");
        return false;
    }

    public void UsedBooster(int id)
    {
        for(int i = 0; i < playerData.boosters.Count; i++)
        {
            if(playerData.boosters[i].id == id)
            {
                playerData.boosters[i].isFirstTime = false;
                return;
            }
        }
        Debug.Log("Khong tim thay booster");
    }

    public int GetUnclockedLevel(int id)
    {
           for(int i = 0; i < playerData.boosters.Count; i++)
        {
            if(playerData.boosters[i].id == id)
            {
                return playerData.boosters[i].unlockedLevel;
            }
        }
        return 0;
    }
}
