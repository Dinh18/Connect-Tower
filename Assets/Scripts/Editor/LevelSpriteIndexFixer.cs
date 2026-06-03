using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class LevelSpriteIndexFixer
{
    [MenuItem("Tools/Fix Level Sprite Indexes")]
    public static void FixSpriteIndexes()
    {
        // Quét tìm tất cả LevelDataSO trong thư mục Levels
        string[] guids = AssetDatabase.FindAssets("t:LevelDataSO", new[] { "Assets/Resources/Data/Levels" });
        if (guids.Length == 0)
        {
            Debug.LogWarning("Không tìm thấy LevelDataSO nào trong thư mục Assets/Resources/Data/Levels");
            return;
        }

        int updatedCount = 0;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            LevelDataSO levelData = AssetDatabase.LoadAssetAtPath<LevelDataSO>(path);

            if (levelData != null)
            {
                bool modified = false;
                
                // Dictionary để nhóm các block theo cùng một BlockTopic
                Dictionary<BlockTopic, List<BlockSetupData>> topicGroups = new Dictionary<BlockTopic, List<BlockSetupData>>();

                // Thu thập tất cả các block trong level này
                foreach (SlotSetupData slot in levelData.slots)
                {
                    if (slot == null || slot.blocks == null) continue;
                    
                    foreach (BlockSetupData block in slot.blocks)
                    {
                        if (block == null || block.blockTopic == null) continue;

                        if (!topicGroups.ContainsKey(block.blockTopic))
                        {
                            topicGroups[block.blockTopic] = new List<BlockSetupData>();
                        }
                        topicGroups[block.blockTopic].Add(block);
                    }
                }

                // Gán lại indexSprite cho từng nhóm topic
                foreach (var kvp in topicGroups)
                {
                    List<BlockSetupData> blocks = kvp.Value;
                    
                    // Tạo một danh sách các index 0, 1, 2, 3 và xáo trộn để gán ngẫu nhiên
                    List<int> indices = new List<int>();
                    for (int i = 0; i < blocks.Count; i++)
                    {
                        indices.Add(i % 4);
                    }
                    
                    // Thuật toán xáo trộn Fisher-Yates
                    System.Random rng = new System.Random();
                    int n = indices.Count;
                    while (n > 1)
                    {
                        n--;
                        int k = rng.Next(n + 1);
                        int value = indices[k];
                        indices[k] = indices[n];
                        indices[n] = value;
                    }

                    // Tiến hành gán indexSprite đã được xáo trộn
                    for (int i = 0; i < blocks.Count; i++)
                    {
                        if (blocks[i].indexSprite != indices[i])
                        {
                            blocks[i].indexSprite = indices[i];
                            modified = true;
                        }
                    }
                }

                if (modified)
                {
                    EditorUtility.SetDirty(levelData);
                    updatedCount++;
                }
            }
        }

        // Lưu lại toàn bộ các file Asset đã bị thay đổi
        AssetDatabase.SaveAssets();
        Debug.Log($"Hoàn tất quét và sửa lỗi! Đã cập nhật tổng cộng {updatedCount} file LevelDataSO.");
    }
}
