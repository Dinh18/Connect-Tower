using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class AssetReorganizer : EditorWindow
{
    [MenuItem("Tools/Project/Reorganize & Cleanup")]
    public static void Reorganize()
    {
        bool confirm = EditorUtility.DisplayDialog("Reorganize Project", 
            "This will move files and delete unused scripts (Empty.cs). Are you sure?", "Yes", "No");
        
        if (!confirm) return;

        // 1. Rename SLot to Slot
        RenameFolder("Assets/Scripts/Gameplay/SLot", "Slot");

        // 2. Create subfolders in UI
        CreateFolder("Assets/Scripts/UI", "Animations");
        CreateFolder("Assets/Scripts/UI", "Panels");
        CreateFolder("Assets/Scripts/UI", "Popups");
        CreateFolder("Assets/Scripts/UI", "Effects");
        CreateFolder("Assets/Scripts/UI", "Elements");

        // 3. Move UI Files
        MoveFile("Assets/Scripts/UI/PopupAnimation.cs", "Assets/Scripts/UI/Animations/PopupAnimation.cs");
        MoveFile("Assets/Scripts/UI/AnimationButton.cs", "Assets/Scripts/UI/Animations/AnimationButton.cs");
        
        MoveFile("Assets/Scripts/UI/ShopPanel.cs", "Assets/Scripts/UI/Panels/ShopPanel.cs");
        MoveFile("Assets/Scripts/UI/MainMenuUIManager.cs", "Assets/Scripts/UI/Panels/MainMenuUIManager.cs");
        MoveFile("Assets/Scripts/UI/InGameUIManager.cs", "Assets/Scripts/UI/Panels/InGameUIManager.cs");
        MoveFile("Assets/Scripts/UI/LevelUIManager.cs", "Assets/Scripts/UI/Panels/LevelUIManager.cs");

        MoveFile("Assets/Scripts/UI/SettingPopup.cs", "Assets/Scripts/UI/Popups/SettingPopup.cs");
        MoveFile("Assets/Scripts/UI/RefillHeartPopup.cs", "Assets/Scripts/UI/Popups/RefillHeartPopup.cs");
        MoveFile("Assets/Scripts/UI/EndGameUI.cs", "Assets/Scripts/UI/Popups/EndGameUI.cs");
        MoveFile("Assets/Scripts/UI/LevelCompletedUI.cs", "Assets/Scripts/UI/Popups/LevelCompletedUI.cs");
        MoveFile("Assets/Scripts/UI/LevelFailedUI.cs", "Assets/Scripts/UI/Popups/LevelFailedUI.cs");

        MoveFile("Assets/Scripts/UI/CoinEffect.cs", "Assets/Scripts/UI/Effects/CoinEffect.cs");
        MoveFile("Assets/Scripts/UI/CoinFlyEffect.cs", "Assets/Scripts/UI/Effects/CoinFlyEffect.cs");
        MoveFile("Assets/Scripts/UI/HintEffect.cs", "Assets/Scripts/UI/Effects/HintEffect.cs");
        MoveFile("Assets/Scripts/UI/ShuffleEffect.cs", "Assets/Scripts/UI/Effects/ShuffleEffect.cs");
        MoveFile("Assets/Scripts/UI/AddMoveEffect.cs", "Assets/Scripts/UI/Effects/AddMoveEffect.cs");

        MoveFile("Assets/Scripts/UI/ShopButton.cs", "Assets/Scripts/UI/Elements/ShopButton.cs");
        MoveFile("Assets/Scripts/UI/BoosterButton.cs", "Assets/Scripts/UI/Elements/BoosterButton.cs");
        MoveFile("Assets/Scripts/UI/PlayButton.cs", "Assets/Scripts/UI/Elements/PlayButton.cs");
        MoveFile("Assets/Scripts/UI/HomeButton.cs", "Assets/Scripts/UI/Elements/HomeButton.cs");

        // 4. Cleanup unused files
        DeleteFile("Assets/Scripts/Empty.cs");

        AssetDatabase.Refresh();
        Debug.Log("<color=green><b>[Project Reorganizer]</b></color> Reorganization complete!");
    }

    private static void CreateFolder(string parent, string name)
    {
        if (!AssetDatabase.IsValidFolder(parent + "/" + name))
        {
            AssetDatabase.CreateFolder(parent, name);
        }
    }

    private static void MoveFile(string from, string to)
    {
        if (File.Exists(from) && !File.Exists(to))
        {
            string error = AssetDatabase.MoveAsset(from, to);
            if (!string.IsNullOrEmpty(error))
            {
                Debug.LogError($"Error moving {from} to {to}: {error}");
            }
        }
    }

    private static void RenameFolder(string path, string newName)
    {
        if (AssetDatabase.IsValidFolder(path))
        {
            AssetDatabase.RenameAsset(path, newName);
        }
    }

    private static void DeleteFile(string path)
    {
        if (File.Exists(path))
        {
            AssetDatabase.DeleteAsset(path);
            Debug.Log($"Deleted unused script: {path}");
        }
    }
}
