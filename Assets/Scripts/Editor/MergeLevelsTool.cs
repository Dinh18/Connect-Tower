using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System.IO;

public class MergeLevelsTool : EditorWindow
{
    [MenuItem("Tools/Merge Levels_2 into Levels")]
    public static void MergeLevels()
    {
        string sourceDir = "Assets/Resources/Data/Levels_2";
        string targetDir = "Assets/Resources/Data/Levels";
        
        if (!AssetDatabase.IsValidFolder(sourceDir))
        {
            Debug.LogError("Source directory does not exist: " + sourceDir);
            return;
        }

        if (!AssetDatabase.IsValidFolder(targetDir))
        {
            Debug.LogError("Target directory does not exist: " + targetDir);
            return;
        }

        string[] guids = AssetDatabase.FindAssets("t:LevelDataSO", new[] { sourceDir });
        List<LevelDataSO> levels = new List<LevelDataSO>();
        
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            LevelDataSO so = AssetDatabase.LoadAssetAtPath<LevelDataSO>(path);
            if (so != null)
            {
                levels.Add(so);
            }
        }

        levels.Sort((a, b) => a.level.CompareTo(b.level));

        int newLevelId = 31;
        
        foreach (LevelDataSO so in levels)
        {
            string oldPath = AssetDatabase.GetAssetPath(so);
            
            so.level = newLevelId;
            EditorUtility.SetDirty(so);
            
            string newName = $"Level_{newLevelId:D2}";
            string newPath = $"{targetDir}/{newName}.asset";

            string error = AssetDatabase.MoveAsset(oldPath, newPath);
            if (!string.IsNullOrEmpty(error))
            {
                Debug.LogError($"Failed to move {oldPath} to {newPath}: {error}");
            }
            else
            {
                Debug.Log($"Moved {oldPath} -> {newPath} (ID: {newLevelId})");
            }
            
            newLevelId++;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        Debug.Log("Merge complete! Total moved: " + levels.Count);
    }
}
