using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.U2D;

public class BlockSpriteAtlasPacker : EditorWindow
{
    [MenuItem("Tools/Pack Block Sprites To Atlas")]
    public static void PackSprites()
    {
        string folderPath = "Assets/Resources/Texture2D";
        
        // Lấy tất cả các GUID của Texture2D trong folder
        string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { folderPath });
        
        List<Object> validSprites = new List<Object>();
        
        // Regex pattern:
        // ^[A-Z]_ : bắt đầu bằng chữ cái in hoa bất kỳ
        // \d : theo sau là 1 chữ số (x)
        // (?:_\d{1,3})? : (tuỳ chọn) theo sau là _ và 1 đến 3 chữ số (y)
        // $ : kết thúc chuỗi
        Regex regex = new Regex(@"^[A-Z]_\d(?:_\d{1,3})?$");

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            string fileName = Path.GetFileNameWithoutExtension(path);
            
            if (regex.IsMatch(fileName))
            {
                // Load object Texture2D (hoặc Sprite)
                Object obj = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                if (obj != null)
                {
                    validSprites.Add(obj);
                }
            }
        }

        if (validSprites.Count == 0)
        {
            Debug.LogWarning($"[SpriteAtlasPacker] Không tìm thấy ảnh nào thoả mãn định dạng [A-Z]_x hoặc [A-Z]_x_y trong thư mục {folderPath}");
            return;
        }

        string atlasPath = "Assets/Resources/BlockAtlas.spriteatlas";
        
        // Nếu đã có atlas cũ, xoá đi để tạo mới nhằm tránh bị duplicate data
        SpriteAtlas atlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(atlasPath);
        if (atlas != null)
        {
            AssetDatabase.DeleteAsset(atlasPath);
        }

        atlas = new SpriteAtlas();

        // Thiết lập Packing Settings
        SpriteAtlasPackingSettings packingSettings = new SpriteAtlasPackingSettings()
        {
            blockOffset = 1,
            enableRotation = false, // Không xoay để tránh lỗi UI/Sprite render
            enableTightPacking = false, // Dùng false cho lưới vuông vắn, an toàn hơn với UI
            padding = 2, // Đệm 2 pixel để tránh lem viền (bleeding)
        };
        atlas.SetPackingSettings(packingSettings);

        // Thiết lập Texture Settings
        SpriteAtlasTextureSettings textureSettings = new SpriteAtlasTextureSettings()
        {
            readable = false,
            generateMipMaps = false,
            sRGB = true,
            filterMode = FilterMode.Bilinear,
        };
        atlas.SetTextureSettings(textureSettings);

        // Nạp danh sách hình ảnh vào Atlas
        SpriteAtlasExtensions.Add(atlas, validSprites.ToArray());
        
        // Lưu thành file asset
        AssetDatabase.CreateAsset(atlas, atlasPath);
        AssetDatabase.SaveAssets();

        // Gọi lệnh pack (chỉ dùng cho Editor)
        SpriteAtlasUtility.PackAtlases(new SpriteAtlas[] { atlas }, EditorUserBuildSettings.activeBuildTarget);
        
        Debug.Log($"[SpriteAtlasPacker] Đã đóng gói thành công {validSprites.Count} ảnh vào {atlasPath}");
        
        // Highlight file atlas trong project window
        EditorGUIUtility.PingObject(atlas);
    }
}
