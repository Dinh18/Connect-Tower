using System.IO;
using UnityEngine;
using UnityEditor;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Collections.Generic;

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
        CoreServices.Register<DataManager>(this);
    }

    void Start()
    {
        
    }

    // 16-byte key and IV cho AES. Bạn có thể thay đổi chuỗi này để tăng tính bảo mật.
    private readonly byte[] encryptionKey = Encoding.UTF8.GetBytes("ThaleGame1234567");
    private readonly byte[] encryptionIV = Encoding.UTF8.GetBytes("ThaleTower765432");

    public void SaveGame()
    {
        string jsonString = JsonUtility.ToJson(playerData, true);
        
        try
        {
            string encryptedData = EncryptString(jsonString);
            File.WriteAllText(saveFilePath, encryptedData);
            Debug.Log("<color=green>Đã mã hóa và lưu game tại: </color>" + saveFilePath);
        }
        catch (Exception e)
        {
            Debug.LogError("Lỗi khi lưu game: " + e.Message);
            // Fallback lưu không mã hóa nếu có lỗi phát sinh
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
                Debug.Log("<color=green>Đã giải mã và tải game từ: </color>" + saveFilePath);
            }
            catch (Exception e)
            {
                Debug.LogWarning("Không thể giải mã dữ liệu (Có thể là file cũ chưa mã hóa). Thử đọc json thuần túy... Lỗi: " + e.Message);
                try 
                {
                    // Fallback cho file cũ chưa mã hóa
                    playerData = JsonUtility.FromJson<PlayerData>(fileContent);
                    Debug.Log("<color=green>Đã tải game (không mã hóa) từ: </color>" + saveFilePath);
                }
                catch
                {
                    Debug.LogError("File save bị hỏng hoàn toàn. Reset data.");
                    playerData = new PlayerData();
                    SaveGame();
                }
            }
        }
        else
        {
            playerData = new PlayerData();
            Debug.LogWarning("<color=red>Không tìm thấy file lưu game tại: </color>" + saveFilePath);
            SaveGame();
        }
    }

    private string EncryptString(string plainText)
    {
        using (Aes aesAlg = Aes.Create())
        {
            aesAlg.Key = encryptionKey;
            aesAlg.IV = encryptionIV;
            ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);
            using (MemoryStream msEncrypt = new MemoryStream())
            {
                using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                {
                    swEncrypt.Write(plainText);
                }
                return Convert.ToBase64String(msEncrypt.ToArray());
            }
        }
    }

    private string DecryptString(string cipherText)
    {
        byte[] cipherBytes = Convert.FromBase64String(cipherText);
        using (Aes aesAlg = Aes.Create())
        {
            aesAlg.Key = encryptionKey;
            aesAlg.IV = encryptionIV;
            ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
            using (MemoryStream msDecrypt = new MemoryStream(cipherBytes))
            using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
            using (StreamReader srDecrypt = new StreamReader(csDecrypt))
            {
                return srDecrypt.ReadToEnd();
            }
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

    // Tự động lưu game khi người chơi tắt App hoặc thu nhỏ màn hình (Rất quan trọng để tối ưu I/O)
    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus) SaveGame();
    }

    private void OnApplicationQuit()
    {
        SaveGame();
    }

    // --- HELPER METHODS (DRY Principle) ---
    public BoosterData GetBooster(int id)
    {
        return playerData.boosters.Find(b => b.id == id);
    }

    public MechanicData GetMechanic(int id)
    {
        return playerData.mechanics.Find(m => m.id == id);
    }
    // -------------------------------------

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
            // Đã xóa SaveGame() ở đây để tối ưu I/O. File sẽ tự save khi OnApplicationPause.
            Debug.Log("Use Booster");
            OnChangeCountBooster?.Invoke(id, b.count);
        }
    }

    public void AddBooster(int id, int amount, int price)
    {
        var b = GetBooster(id);
        if (b != null)
        {
            b.count += amount;
            UseCoins(price);
            Debug.Log("Add Booster");
            OnChangeCountBooster?.Invoke(id, b.count);
        }
    }

    public void AddCoins(int amount)
    {
        playerData.totalCoins += amount;
        Debug.Log("Add Coins");
        OnChangeCoins?.Invoke(playerData.totalCoins);
    }

    public void UseCoins(int amount)
    {
        playerData.totalCoins -= amount;
        Debug.Log("USe Coins");
        OnChangeCoins?.Invoke(playerData.totalCoins);
    }

    public void UseHeart(string nextHeartTime)
    {
        if(playerData.heart > 0)
        {
            playerData.heart--;
            playerData.nextHeartTime = nextHeartTime;
            OnChangeHeart?.Invoke(playerData.heart);
        }
    }

    public void AddHeart(int amount, string nextHeartTime)
    {
        playerData.heart = Math.Min(playerData.heart+=amount, 5);
        playerData.nextHeartTime = nextHeartTime;
        OnChangeHeart?.Invoke(playerData.heart);
    }

    public void LevelUp(LevelLoader.GameDifficult gameDifficult, int maxLevel)
    {
        if(playerData.currentLevel < maxLevel)
        {
            playerData.currentLevel++;
        }
        
        GameConfigSO config = Resources.Load<GameConfigSO>("GameConfig");
        int rewardE = config != null ? config.coinRewardEasy : 40;
        int rewardH = config != null ? config.coinRewardHard : 80;
        int rewardVH = config != null ? config.coinRewardSuperHard : 120;

        switch(gameDifficult)
        {
            case LevelLoader.GameDifficult.Easy:
                AddCoins(rewardE);
                break;
            case LevelLoader.GameDifficult.Hard:
                AddCoins(rewardH);
                break;
            case LevelLoader.GameDifficult.VeryHard:
                AddCoins(rewardVH);
                break;
        }

        // Fix lỗi Index bằng cách gọi GetBooster(ID)
        var addMove = GetBooster((int)Constants.BoosterType.AddMove);
        if(addMove != null && playerData.currentLevel == addMove.unlockedLevel)
            UnlockBooster(addMove.id);

        var shuffle = GetBooster((int)Constants.BoosterType.Shuffle);
        if(shuffle != null && playerData.currentLevel == shuffle.unlockedLevel)
            UnlockBooster(shuffle.id);

        var hint = GetBooster((int)Constants.BoosterType.Hint);
        if(hint != null && playerData.currentLevel == hint.unlockedLevel)
            UnlockBooster(hint.id);

        SaveGame(); // Vẫn giữ SaveGame ở đây vì qua Level là một cột mốc quan trọng (Checkpoint)
        OnChangeLevel?.Invoke(playerData.currentLevel);
    }

    public void UnlockBooster(int id)
    {
        var b = GetBooster(id);
        if (b != null) b.isUnlocked = true;
        else Debug.Log("Khong tim thay booster");
    }

    public bool IsUnLockedBooster(int id)
    {
        var b = GetBooster(id);
        if (b != null) return b.isUnlocked;
        
        Debug.Log("Khong tim thay booster");
        return false;
    }

    public bool IsFirstTimeUserBooster(int id)
    {
        var b = GetBooster(id);
        if (b != null) return b.isFirstTime;

        Debug.Log("Khong tim thay booster");
        return false;
    }

    public void UsedBooster(int id)
    {
        var b = GetBooster(id);
        if (b != null) b.isFirstTime = false;
        else Debug.Log("Khong tim thay booster");
    }

    public int GetUnclockedLevel(int id)
    {
        var b = GetBooster(id);
        return b != null ? b.unlockedLevel : 0;
    }

    public void PlayedMechanic(int id)
    {
        var m = GetMechanic(id);
        if (m != null) m.isFirstTimePlay = false;
    }

    public bool IsFirstTimePlayMechanic(int id)
    {
        var m = GetMechanic(id);
        return m != null && playerData.currentLevel == m.levelUnclock && m.isFirstTimePlay;
    }

    public int GetLevelUnlockMechanic(int id)
    {
        var m = GetMechanic(id);
        return m != null ? m.levelUnclock : 0;
    }
}
