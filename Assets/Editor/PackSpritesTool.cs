using UnityEngine;
using UnityEditor;
using UnityEditor.U2D;
using UnityEngine.U2D;
using System.Collections.Generic;

public class PackSpritesTool : EditorWindow
{
    [MenuItem("Tools/Pack All Sprites To Atlas")]
    public static void PackSprites()
    {
        // Tìm Sprite Atlas có tên "New Sprite Atlas"
        string[] atlasGuids = AssetDatabase.FindAssets("New Sprite Atlas t:SpriteAtlas");
        if (atlasGuids.Length == 0)
        {
            Debug.LogError("Không tìm thấy 'New Sprite Atlas'. Hãy đảm bảo bạn đã tạo nó.");
            return;
        }

        string atlasPath = AssetDatabase.GUIDToAssetPath(atlasGuids[0]);
        SpriteAtlas atlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(atlasPath);

        if (atlas == null)
        {
            Debug.LogError("Không thể load Sprite Atlas tại: " + atlasPath);
            return;
        }

        // Tìm tất cả các Texture2D trong project
        string[] texGuids = AssetDatabase.FindAssets("t:Texture2D");
        List<Object> packables = new List<Object>();

        foreach (string guid in texGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            
            // Bỏ qua các sprite trong thư mục Packages để tránh lỗi
            if (path.StartsWith("Packages/")) continue;

            // Kiểm tra xem texture có phải là kiểu Sprite không
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null && importer.textureType == TextureImporterType.Sprite)
            {
                Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                if (tex != null && !packables.Contains(tex))
                {
                    packables.Add(tex);
                }
            }
        }

        // Xoá các packable cũ (nếu có) để tránh trùng lặp
        Object[] existing = atlas.GetPackables();
        if (existing.Length > 0)
        {
            SpriteAtlasExtensions.Remove(atlas, existing);
        }

        // Thêm danh sách sprite mới vào atlas
        SpriteAtlasExtensions.Add(atlas, packables.ToArray());
        
        // Lưu thay đổi
        AssetDatabase.SaveAssets();
        
        Debug.Log($"Đã thêm thành công {packables.Count} sprites vào {atlas.name} tại đường dẫn: {atlasPath}");
    }
}
