using System;
using UnityEngine;

public static class Constants
{
    public const string BLOCK_TEST_1_PATH = "Prefabs/Block_Test1";
    public const string SLOT_PREFAB_PATH = "Prefabs/Slot";
    public const string MATERIAL_COLOR_1_PATH = "Material/Type_1";
    public const string MATERIAL_COLOR_2_PATH = "Material/Type_2";
    public const string MATERIAL_COLOR_3_PATH = "Material/Type_3";
    public const string MATERIAL_COLOR_4_PATH = "Material/Type_4";
    public const string MATERIAL_COLOR_5_PATH = "Material/Type_5";
    public const string MATERIAL_COLOR_6_PATH = "Material/Type_6";
    public const string MATERIAL_COLOR_7_PATH = "Material/Type_7";
    public const string MATERIAL_COLOR_8_PATH = "Material/Type_8";
    public const string MATERIAL_COLOR_9_PATH = "Material/Type_9";
    public const string MATERIAL_COLOR_W_PATH = "Material/Type_W";
    public const string MATERIAL_COLOR_HIDE_PATH = "Material/Ground";
    public const string MATERIAL_BASE_PATH = "Material/Pole_Texture";
    public const string MATERIAL_ICE_PATH = "Material/Ice_Obstacle_MAT";
    public const string MATERIAL_ICE_BLOCK = "Material/Ice_Block_Material";
    // Mesh
    public const string MESH_ICE_BASE_PATH = "Mesh/Ice_Base";
    public const string MESH_BASE_PATH = "Mesh/Pole_Base";
    // HÀM HỖ TRỢ: Chuyển đổi chuỗi Hex thành Color
    private static Color GetColorFromHex(string hexCode)
    {
        Color color;
        // ColorUtility sẽ dịch mã Hex. Nếu mã lỗi, nó trả về màu trong suốt, ngược lại trả về màu chuẩn.
        if (ColorUtility.TryParseHtmlString(hexCode, out color))
        {
            return color;
        }
        Debug.LogError("Mã Hex không hợp lệ: " + hexCode);
        return Color.white; // Màu mặc định nếu nhập sai mã
    }

    public static readonly Color COLOR_1 = GetColorFromHex("#31B8FF");
    public static readonly Color COLOR_2 = GetColorFromHex("#00FF5E");
    public static readonly Color COLOR_3 = GetColorFromHex("#FF2981");
    public static readonly Color COLOR_4 = GetColorFromHex("#FFFF00");
    public static readonly Color COLOR_5 = GetColorFromHex("#FF00DB");
    public static readonly Color COLOR_6 = GetColorFromHex("#00FFFF");
    public static readonly Color COLOR_7 = GetColorFromHex("#8F00FF");
    public static readonly Color COLOR_8 = GetColorFromHex("#FFA731");
    public static readonly Color COLOR_9 = GetColorFromHex("#05C9EE"); 
    public static readonly Color COLOR_W = Color.white;

    // Slot
    public static readonly Vector3 POSITION_FIRST_SLOT = new Vector3(0,0,0);
    public static float SLOT_WIDTH = 1.07f;
    public static float SLOT_HEIGHT = 4.052984f;
    // Block
    public static float BLOCK_HEIGHT = 0.65f;
    // Data Level
    public const string DATALEVEL_TEST_PATH = "DataLevel/Level_1";
    public const string LEVELS_PATH = "Levels";
    // Scene Name
    public const string INITIALIZER_SCENE_NAME = "InitializerScene";
    public const string MENU_SCENE_NAME = "MenuScene";
    public const string INGAME_SCENE_NAME = "GameScene";
    // Setting size
    public const float SETTING_HEIGHT_MAINMENU = 830f;
    public const float SETTING_HEIGHT_INGAME = 950f;
    //Total topics
    public const int TOTAL_TOPICS = 26;
    public enum BoosterType
    {
        AddMove = 0,
        Shuffle = 1,
        Hint = 2
    }
    // level images
    public const string NORMAL_LVL_NEXT_STAGE = "Sprite/NORMAL_LVL_NEXT_STAGE";
    public const string HARD_LVL_NEXT_STAGE = "Sprite/HARD_LVL_NEXT_STAGE";
    public const string SUPERHARD_LVL_NEXT_STAGE = "Sprite/SUPERHARD_LVL_NEXT_STAGE";
    public const string NORMAL_LVL_STAGE_CURRENT = "Sprite/NORMAL_LVL_STAGE_CURRENT";
    public const string HARD_LVL_STAGE_CURRENT = "Sprite/HARD_LVL_STAGE_CURRENT";
    public const string SUPERHARD_LVL_STAGE_CURRENT = "Sprite/SUPERHARD_LVL_STAGE_CURRENT";
    public const string HARD_LVL_SKULL = "Sprite/HARD_LVL_SKULL";
    public const string SUPERHARD_LVL_SKULL = "Sprite/SUPERHARD_LVL_SKULL";
    public const string HARD_TEXT_UI = "Sprite/HARD_TEXT_UI (1)";
    public const string SUPERHARD_TEXT_UI = "Sprite/SUPERHARD_TEXT_UI (1)";
    // Heart Icon
    public const string ADD_HEART_ICON = "Sprite/LIVES_ICON_0";
    public const string HEART_ICON = "Sprite/LIVE_ICON";
}
