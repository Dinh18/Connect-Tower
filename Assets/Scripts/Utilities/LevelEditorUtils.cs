using UnityEngine;
using UnityEditor;
using System.Linq;
using System.IO;

public static class LevelEditorUtils
{
    private const string FolderPath = "Assets/Resources/Levels"; // Đường dẫn chứa level

    public static int GetNextLevelID()
    {
        // 1. Đảm bảo thư mục tồn tại
        if (!Directory.Exists(FolderPath))
        {
            Directory.CreateDirectory(FolderPath);
            return 1; // Nếu chưa có gì thì bắt đầu từ ID 1
        }

        // 2. Tìm tất cả các file .asset trong thư mục đó
        string[] guids = AssetDatabase.FindAssets("t:LevelData", new[] { FolderPath });

        if (guids.Length == 0) return 1;

        // 3. Lấy ra ID lớn nhất (giả sử tên file là "Level_1", "Level_2"...)
        int maxID = guids.Select(guid => 
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            string fileName = Path.GetFileNameWithoutExtension(path);
            
            // Tách số từ tên file (Ví dụ: "Level_10" -> 10)
            int id;
            if (int.TryParse(System.Text.RegularExpressions.Regex.Match(fileName, @"\d+").Value, out id))
                return id;
            return 0;
        }).Max();

        return maxID + 1;
    }
}