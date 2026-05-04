using System.IO;
using UnityEngine;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Collections.Generic;

public class DataManager : MonoBehaviour
{
    public PlayerData playerData;
    private string saveFilePath;

    // Events are now handled by GameEventBus

    public void Init()
    {
        saveFilePath = Path.Combine(Application.persistentDataPath, "saveData.json");
        LoadGame();
        CoreServices.Register<DataManager>(this);
    }

    // --- ENCRYPTION ---
    private readonly byte[] encryptionKey = Encoding.UTF8.GetBytes("ThaleGame1234567");
    private readonly byte[] encryptionIV = Encoding.UTF8.GetBytes("ThaleTower765432");

    public void SaveGame()
    {
        string jsonString = JsonUtility.ToJson(playerData, true);
        try
        {
            string encryptedData = EncryptString(jsonString);
            File.WriteAllText(saveFilePath, encryptedData);
        }
        catch (Exception e)
        {
            Debug.LogError("Lỗi khi lưu game: " + e.Message);
            File.WriteAllText(saveFilePath, jsonString);
        }
    }

    public void LoadGame()
    {
        if(File.Exists(saveFilePath))
        {
            string fileContent = File.ReadAllText(saveFilePath);
            try
            {
                string decryptedJson = DecryptString(fileContent);
                playerData = JsonUtility.FromJson<PlayerData>(decryptedJson);
            }
            catch
            {
                // Fallback for old unencrypted files or corrupted files
                try { playerData = JsonUtility.FromJson<PlayerData>(fileContent); }
                catch { playerData = new PlayerData(); SaveGame(); }
            }
        }
        else
        {
            playerData = new PlayerData();
            SaveGame();
            Debug.Log("<color=yellow>[DataManager]</color> Không tìm thấy file save cũ. Đã tạo file save MỚI tại: " + saveFilePath);
        }
    }

    [ContextMenu("Delete Save Data")]
    public void DeleteData()
    {
        // Phải đảm bảo saveFilePath không bị null nếu bấm nút trước khi Play
        string path = string.IsNullOrEmpty(saveFilePath) ? Path.Combine(Application.persistentDataPath, "saveData.json") : saveFilePath;

        if (File.Exists(path))
        {
            File.Delete(path);
            playerData = new PlayerData(); // Khởi tạo lại dữ liệu trắng
            Debug.Log("<color=red>[DataManager]</color> Đã xóa thành công file save tại: " + path);
        }
        else
        {
            Debug.LogWarning("<color=yellow>[DataManager]</color> Không có file save nào để xóa tại: " + path);
        }
    }

    private string EncryptString(string plainText)
    {
        using (Aes aesAlg = Aes.Create())
        {
            aesAlg.Key = encryptionKey; aesAlg.IV = encryptionIV;
            ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);
            using (MemoryStream msEncrypt = new MemoryStream())
            {
                using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                using (StreamWriter swEncrypt = new StreamWriter(csEncrypt)) { swEncrypt.Write(plainText); }
                return Convert.ToBase64String(msEncrypt.ToArray());
            }
        }
    }

    private string DecryptString(string cipherText)
    {
        byte[] cipherBytes = Convert.FromBase64String(cipherText);
        using (Aes aesAlg = Aes.Create())
        {
            aesAlg.Key = encryptionKey; aesAlg.IV = encryptionIV;
            ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
            using (MemoryStream msDecrypt = new MemoryStream(cipherBytes))
            using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
            using (StreamReader srDecrypt = new StreamReader(csDecrypt)) { return srDecrypt.ReadToEnd(); }
        }
    }

    // --- DATA ACCESSORS ---
    public int GetCurrentLevel() => playerData.currentLevel;
    public int GetTotalCoins() => playerData.wallet.totalCoins;
    public int GetHearts() => playerData.wallet.heart;
    public string GetNextHeartTime() => playerData.wallet.nextHeartTime;
    public List<MechanicData> GetMechanics() => playerData.progress.mechanics;
    public BoosterData GetBooster(int id) => playerData.inventory.boosters.Find(b => b.id == id);
    public MechanicData GetMechanic(int id) => playerData.progress.mechanics.Find(m => m.id == id);

    public void AddCoins(int amount)
    {
        playerData.wallet.totalCoins += amount;
        GameEventBus.Publish(new CoinsUpdatedEvent { totalCoins = playerData.wallet.totalCoins });
    }

    public void UseCoins(int amount)
    {
        playerData.wallet.totalCoins -= amount;
        GameEventBus.Publish(new CoinsUpdatedEvent { totalCoins = playerData.wallet.totalCoins });
    }

    public void UseHeart(string nextHeartTime)
    {
        if(playerData.wallet.heart > 0)
        {
            playerData.wallet.heart--;
            playerData.wallet.nextHeartTime = nextHeartTime;
            GameEventBus.Publish(new HeartUpdatedEvent { heartCount = playerData.wallet.heart });
        }
    }

    public void AddHeart(int amount, string nextHeartTime)
    {
        playerData.wallet.heart = Math.Min(playerData.wallet.heart + amount, 5);
        playerData.wallet.nextHeartTime = nextHeartTime;
        GameEventBus.Publish(new HeartUpdatedEvent { heartCount = playerData.wallet.heart });
    }

    public void LevelUp(LevelLoader.GameDifficult gameDifficult, int maxLevel)
    {
        if(playerData.currentLevel < maxLevel) playerData.currentLevel++;
        
        GameConfigSO config = Resources.Load<GameConfigSO>("GameConfig");
        int reward = (gameDifficult == LevelLoader.GameDifficult.Easy) ? (config?.coinRewardEasy ?? 40) : 
                     (gameDifficult == LevelLoader.GameDifficult.Hard) ? (config?.coinRewardHard ?? 80) : (config?.coinRewardSuperHard ?? 120);

        AddCoins(reward);

        // Auto unlock boosters based on level
        foreach(var b in playerData.inventory.boosters)
        {
            if(playerData.currentLevel == b.unlockedLevel) b.isUnlocked = true;
        }

        SaveGame();
        GameEventBus.Publish(new LevelUpdatedEvent { newLevel = playerData.currentLevel });
    }

    public int GetAmountOfBoosterByID(int id)
    {
        var b = GetBooster(id);
        return b != null ? b.count : 0;
    }

    public void UseBooster(int id)
    {
        var b = GetBooster(id);
        if (b != null)
        {
            b.count--;
            GameEventBus.Publish(new BoosterCountUpdatedEvent { boosterId = id, count = b.count });
        }
    }

    public void AddBooster(int id, int amount, int price)
    {
        var b = GetBooster(id);
        if (b != null)
        {
            b.count += amount;
            UseCoins(price);
            GameEventBus.Publish(new BoosterCountUpdatedEvent { boosterId = id, count = b.count });
        }
    }

    public void UnlockBooster(int id)
    {
        var b = GetBooster(id);
        if (b != null) b.isUnlocked = true;
    }

    public bool IsUnLockedBooster(int id)
    {
        var b = GetBooster(id);
        return b != null && b.isUnlocked;
    }

    public bool IsFirstTimeUserBooster(int id)
    {
        var b = GetBooster(id);
        return b != null && b.isFirstTime;
    }

    public void UsedBooster(int id)
    {
        var b = GetBooster(id);
        if (b != null) b.isFirstTime = false;
    }

    public int GetUnclockedLevel(int id)
    {
        var b = GetBooster(id);
        return b != null ? b.unlockedLevel : 0;
    }

    public int GetLevelUnlockMechanic(int id)
    {
        var m = GetMechanic(id);
        return m != null ? m.levelUnclock : 0;
    }

    public bool IsFirstTimePlayMechanic(int id)
    {
        var m = GetMechanic(id);
        return m != null && playerData.currentLevel == m.levelUnclock && m.isFirstTimePlay;
    }

    public void PlayedMechanic(int id)
    {
        var m = GetMechanic(id);
        if (m != null) m.isFirstTimePlay = false;
    }

    private void OnApplicationPause(bool pauseStatus) { if (pauseStatus) SaveGame(); }
    private void OnApplicationQuit() { SaveGame(); }
}
