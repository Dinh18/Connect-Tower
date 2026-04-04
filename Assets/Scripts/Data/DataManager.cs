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
    void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);

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
                break;
            }
        }
        SaveGame();
    }

    public void AddBooster(int id, int amount)
    {
        for(int i = 0; i < playerData.boosters.Count; i++)
        {
            if(playerData.boosters[i].id == id)
            {
                playerData.boosters[i].count += amount;
            }
        }
        SaveGame();
    }

    public void AddCoins(int amount)
    {
        playerData.totalCoins += amount;
        SaveGame();
        OnChangeCoins?.Invoke(playerData.totalCoins);
    }
    public void useCoins(int amount)
    {
        playerData.totalCoins -= amount;
        SaveGame();
        OnChangeCoins?.Invoke(playerData.totalCoins);
    }

    public void LevelUp(LevelLoader.GameDifficult gameDifficult)
    {
        playerData.currentLevel++;
        switch(gameDifficult)
        {
            case LevelLoader.GameDifficult.Easy:
                playerData.totalCoins += 40;
                break;
            case LevelLoader.GameDifficult.Hard:
                playerData.totalCoins += 80;
                break;
            case LevelLoader.GameDifficult.VeryHard:
                playerData.totalCoins += 120;
                break;
        }
        SaveGame();
        OnChangeLevel?.Invoke(playerData.currentLevel + 1);
    }
}
