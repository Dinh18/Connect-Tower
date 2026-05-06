using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

public class AutoSliceSpriteTool : Editor
{
    // Đường dẫn cố định mà tool sẽ quét (Lưu ý: phải có chữ Assets ở đầu)
    private const string TARGET_FOLDER = "Assets/Resources/Texture2D";

    // Regex quy định định dạng tên file (A_x hoặc A_x_y)
    private const string REGEX_PATTERN = @"^[A-Z]_[1-9](_\d{1,3})?$";

    [MenuItem("Tools/🦸‍♂️ 1-Click Process All Block Sprites")]
    public static void ProcessAllSprites()
    {
        // 1. Kiểm tra xem thư mục có tồn tại không
        if (!AssetDatabase.IsValidFolder(TARGET_FOLDER))
        {
            Debug.LogError($"[Sprite Tool] Không tìm thấy thư mục: {TARGET_FOLDER}. Vui lòng kiểm tra lại đường dẫn!");
            return;
        }

        // 2. Tìm tất cả file Texture2D nằm trong thư mục đích
        string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { TARGET_FOLDER });
        Regex regex = new Regex(REGEX_PATTERN);
        int successCount = 0;

        EditorUtility.DisplayProgressBar("Đang xử lý ảnh", "Đang quét các file hợp lệ...", 0f);

        for (int i = 0; i < guids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            string fileName = Path.GetFileNameWithoutExtension(path);

            EditorUtility.DisplayProgressBar("Đang xử lý ảnh", $"Đang cắt: {fileName}", (float)i / guids.Length);

            // 3. Nếu tên file KHÔNG khớp với Regex -> Bỏ qua
            if (!regex.IsMatch(fileName)) continue;

            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                // 4. Chỉ thiết lập các thông số bắt buộc để Tool có thể cắt được ảnh
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Multiple;
                importer.isReadable = true; // Bắt buộc bật để Unity đọc được pixel vùng trong suốt

                // Cập nhật để lấy quyền Read/Write
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

                // 5. Tải tấm ảnh lên RAM để tiến hành cắt
                Texture2D readableTex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);

                // Cắt theo thuật toán Automatic
                Rect[] slicedRects = InternalSpriteUtility.GenerateAutomaticSpriteRectangles(readableTex, 4, 0);
                List<SpriteMetaData> metaDataList = new List<SpriteMetaData>();

                // 6. Áp dụng Pivot = Center cho tất cả các mảnh vừa cắt
                for (int j = 0; j < slicedRects.Length; j++)
                {
                    SpriteMetaData smd = new SpriteMetaData();
                    smd.rect = slicedRects[j];
                    smd.alignment = (int)SpriteAlignment.Center;
                    smd.name = $"{fileName}_{j}";
                    smd.pivot = new Vector2(0.5f, 0.5f);
                    metaDataList.Add(smd);
                }

                // Ghi đè dữ liệu cắt vào file ảnh
                importer.spritesheet = metaDataList.ToArray();
                
                // Tắt quyền Readable đi để tối ưu RAM khi chạy Game
                importer.isReadable = false; 

                // Lưu lại file lần cuối
                EditorUtility.SetDirty(importer);
                importer.SaveAndReimport();
                
                successCount++;
            }
        }

        EditorUtility.ClearProgressBar();

        Debug.Log($"<color=green><b>[Thành Công]</b></color> Đã tự động cắt chuẩn và căn tâm Center cho {successCount} tấm ảnh!");
    }
}