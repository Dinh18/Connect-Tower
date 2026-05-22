#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public class LevelEditorWindow : EditorWindow
{
    private MakeLevel makeLevel;
    
    // Window navigation tabs
    private int activeTab = 0; // 0: Config, 1: Blocks, 2: Mechanics, 3: Playtest, 4: Save
    public static int ActiveTabStatic = 0;

    private void SetActiveTab(int tabIndex)
    {
        activeTab = tabIndex;
        ActiveTabStatic = tabIndex;
    }
    
    // Topic paintbrush settings
    private BlockTopic paintbrushTopic = null;
    public static BlockTopic PaintbrushTopicStatic = null;
    private BlockTopic[] allAvailableTopics = null;
    private Vector2 topicScrollPos;
    private bool showAvailableTopicsList = true;

    private void SetPaintbrush(BlockTopic topic)
    {
        paintbrushTopic = topic;
        PaintbrushTopicStatic = topic;
    }
    
    // Scroll views
    private Vector2 mainScrollPos;
    
    // Custom styles
    private GUIStyle headerStyle;
    private GUIStyle activeTabStyle;
    private GUIStyle inactiveTabStyle;
    private GUIStyle titleLabelStyle;
    private Texture2D activeTabBgTex;
    private Texture2D inactiveTabBgTex;

    [MenuItem("Tools/Connect Tower/Level Editor Window")]
    public static void ShowWindow()
    {
        LevelEditorWindow window = GetWindow<LevelEditorWindow>("Connect Tower Level Editor");
        window.minSize = new Vector2(300, 450);
        window.Show();
    }

    private void OnEnable()
    {
        // Pre-load all available topics
        allAvailableTopics = Resources.LoadAll<BlockTopic>("Data/topics2");
        
        // Listen to play mode changes to clear playtest configurations
        EditorApplication.playModeStateChanged += OnPlayModeChanged;
        
        // Auto-link with active MakeLevel in scene
        UpdateMakeLevelReference();
    }

    private void OnDisable()
    {
        EditorApplication.playModeStateChanged -= OnPlayModeChanged;
        CleanupTextures();
    }

    private void OnPlayModeChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredEditMode)
        {
            LevelLoader.isPlaytestingTempLevel = false;
            LevelLoader.playtestLevelData = null;
        }
    }

    private void UpdateMakeLevelReference()
    {
        if (makeLevel == null)
        {
            makeLevel = FindFirstObjectByType<MakeLevel>();
        }
    }

    private void CleanupTextures()
    {
        if (activeTabBgTex != null) DestroyImmediate(activeTabBgTex);
        if (inactiveTabBgTex != null) DestroyImmediate(inactiveTabBgTex);
    }

    private void InitStyles()
    {
        if (headerStyle != null) return;

        headerStyle = new GUIStyle(EditorStyles.boldLabel);
        headerStyle.fontSize = 12;
        headerStyle.normal.textColor = new Color(0.2f, 0.75f, 1f);

        titleLabelStyle = new GUIStyle(EditorStyles.boldLabel);
        titleLabelStyle.fontSize = 13;
        titleLabelStyle.alignment = TextAnchor.MiddleCenter;
        titleLabelStyle.normal.textColor = Color.white;

        activeTabBgTex = MakeTex(2, 2, new Color(0.1f, 0.6f, 0.95f, 1f));
        inactiveTabBgTex = MakeTex(2, 2, new Color(0.22f, 0.22f, 0.25f, 1f));

        activeTabStyle = new GUIStyle(GUI.skin.button);
        activeTabStyle.normal.background = activeTabBgTex;
        activeTabStyle.normal.textColor = Color.white;
        activeTabStyle.fontStyle = FontStyle.Bold;

        inactiveTabStyle = new GUIStyle(GUI.skin.button);
        inactiveTabStyle.normal.background = inactiveTabBgTex;
        inactiveTabStyle.normal.textColor = new Color(0.7f, 0.7f, 0.7f);
        inactiveTabStyle.fontStyle = FontStyle.Normal;
    }

    private Texture2D MakeTex(int width, int height, Color col)
    {
        Color[] pix = new Color[width * height];
        for (int i = 0; i < pix.Length; ++i) pix[i] = col;
        Texture2D result = new Texture2D(width, height);
        result.SetPixels(pix);
        result.Apply();
        return result;
    }

    private void OnGUI()
    {
        InitStyles();
        UpdateMakeLevelReference();

        // Title and Main Info
        GUILayout.Space(10);
        GUILayout.Label("🏰 CONNECT TOWER LEVEL EDITOR 🏰", titleLabelStyle);
        GUILayout.Space(10);

        if (makeLevel == null)
        {
            EditorGUILayout.HelpBox("⚠️ Không tìm thấy đối tượng 'MakeLevel' trong Scene hiện tại!\nHãy mở Scene thiết kế level hoặc tạo mới một đối tượng MakeLevel.", MessageType.Warning);
            GUI.backgroundColor = new Color(0.2f, 0.7f, 1f);
            if (GUILayout.Button("➕ Tạo MakeLevel Game Object Mới trong Scene", GUILayout.Height(38)))
            {
                GameObject newMakeLevelObj = new GameObject("MakeLevel_Container");
                makeLevel = newMakeLevelObj.AddComponent<MakeLevel>();
                Undo.RegisterCreatedObjectUndo(newMakeLevelObj, "Create MakeLevel");
                Selection.activeGameObject = newMakeLevelObj;
                
                // Try auto loading slot & block prefabs from Resources if possible
                makeLevel.slotPrefab = Resources.Load<GameObject>("Prefabs/Slot");
                makeLevel.blockPrefab = Resources.Load<GameObject>("Prefabs/Block");
                
                // Find or create block holder
                var bh = GameObject.Find("BlocksManager") ?? GameObject.Find("BlockHolder");
                if (bh != null) makeLevel.blockHolder = bh.transform;
            }
            GUI.backgroundColor = Color.white;
            return;
        }

        // Active Tabs Selection
        GUILayout.BeginHorizontal();
        string[] tabs = { "Cấu Hình", "Xếp Block", "Mechanic", "Chơi Thử", "Lưu" };
        for (int i = 0; i < tabs.Length; i++)
        {
            bool isCurrent = (activeTab == i);
            if (GUILayout.Button(tabs[i], isCurrent ? activeTabStyle : inactiveTabStyle, GUILayout.Height(30)))
            {
                SetActiveTab(i);
                SetPaintbrush(null); // Clear brush when switching tabs
                GUIUtility.keyboardControl = 0;
            }
        }
        GUILayout.EndHorizontal();
        GUILayout.Space(15);

        // Scrollview wrapper for the main content to make the window fully scalable and resizable!
        mainScrollPos = EditorGUILayout.BeginScrollView(mainScrollPos, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

        switch (activeTab)
        {
            case 0:
                DrawConfigTabContent();
                break;
            case 1:
                DrawBlocksTabContent();
                break;
            case 2:
                DrawMechanicsTabContent();
                break;
            case 3:
                DrawPlaytestTabContent();
                break;
            case 4:
                DrawSaveTabContent();
                break;
        }

        GUILayout.Space(20);
        EditorGUILayout.EndScrollView();

        // Footer Operations (Sticky at the bottom)
        GUILayout.Box("", GUILayout.Height(1), GUILayout.ExpandWidth(true));
        GUILayout.Space(5);
        GUILayout.BeginHorizontal();
        
        GUI.backgroundColor = new Color(0.2f, 0.6f, 1f);
        if (GUILayout.Button("🔄 Cập nhật / Vẽ lại Scene Grid", GUILayout.Height(28)))
        {
            Undo.RecordObject(makeLevel, "Sync and Rebuild Slots");
            makeLevel.SettingSlots();
            makeLevel.UpdateSlotsInEditor();
            makeLevel.GenerateBlocks();
        }
        
        GUI.backgroundColor = new Color(0.9f, 0.25f, 0.25f);
        if (GUILayout.Button("💥 Reset Lưới", GUILayout.Height(28)))
        {
            if (EditorUtility.DisplayDialog("Xóa Toàn Bộ Màn Chơi", "Bạn có chắc chắn muốn xóa sạch slots, blocks và các topics hiện tại?", "Đồng Ý", "Hủy"))
            {
                Undo.RecordObject(makeLevel, "Reset Level Editor");
                makeLevel.Reset();
                SetPaintbrush(null);
            }
        }
        GUILayout.EndHorizontal();
        GUI.backgroundColor = Color.white;
        GUILayout.Space(8);
    }

    private void DrawConfigTabContent()
    {
        GUILayout.Label("BƯỚC 1: CẤU HÌNH GRID & CHỌN TOPICS", headerStyle);
        GUILayout.Space(8);

        // Row Sliders
        EditorGUI.BeginChangeCheck();
        int newRow1 = EditorGUILayout.IntSlider("Số Slot Hàng 1 (Dưới):", makeLevel.row1, 0, 10);
        int newRow2 = EditorGUILayout.IntSlider("Số Slot Hàng 2 (Trên):", makeLevel.row2, 0, 10);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(makeLevel, "Change Grid Rows");
            makeLevel.row1 = newRow1;
            makeLevel.row2 = newRow2;
        }

        GUILayout.Space(12);

        // Chosen Topics Info
        GUILayout.Label($"Topics đã được chọn: {makeLevel.topics.Count}", EditorStyles.boldLabel);
        GUILayout.Space(5);

        // Scrollview list of all available topics from resources
        if (allAvailableTopics != null && allAvailableTopics.Length > 0)
        {
            showAvailableTopicsList = EditorGUILayout.Foldout(showAvailableTopicsList, "Danh sách các topic có sẵn:");
            if (showAvailableTopicsList)
            {
                topicScrollPos = GUILayout.BeginScrollView(topicScrollPos, GUILayout.Height(150), GUILayout.ExpandWidth(true));
                
                foreach (BlockTopic topic in allAvailableTopics)
                {
                    if (topic == null) continue;
                    bool isSelected = makeLevel.topics.Exists(t => t.topicID == topic.topicID);

                    GUILayout.BeginHorizontal();
                    bool newSelected = GUILayout.Toggle(isSelected, $" ID {topic.topicID:D2}: {topic.topicName}");
                    GUILayout.EndHorizontal();

                    if (newSelected != isSelected)
                    {
                        Undo.RecordObject(makeLevel, "Modify Level Topics");
                        if (newSelected)
                        {
                            makeLevel.topics.Add(topic);
                            makeLevel.amountBlockOfTopic.Add(0);
                        }
                        else
                        {
                            int idx = makeLevel.topics.FindIndex(t => t.topicID == topic.topicID);
                            if (idx >= 0)
                            {
                                makeLevel.topics.RemoveAt(idx);
                                if (idx < makeLevel.amountBlockOfTopic.Count)
                                    makeLevel.amountBlockOfTopic.RemoveAt(idx);
                            }
                        }
                        makeLevel.totalTopics = makeLevel.topics.Count;
                        makeLevel.SettingSlots();
                    }
                }
                GUILayout.EndScrollView();
            }
        }
        else
        {
            GUILayout.Label("⚠️ Không tìm thấy file BlockTopic nào trong Resources/Data/topics2/");
        }

        GUILayout.Space(15);

        // Generator Action Button - GENERATES EMPTY GRID ONLY!
        GUI.backgroundColor = new Color(0.2f, 0.8f, 0.4f);
        if (GUILayout.Button("🛠️ DỰNG LƯỚI SLOTS RỖNG TRÊN SCENE", GUILayout.Height(38)))
        {
            Undo.RecordObject(makeLevel, "Generate Slots Grid");
            
            // Xóa sạch các block cũ khỏi slots để bắt đầu một bảng rỗng thủ công hoàn hảo
            foreach (var slot in makeLevel.slots)
            {
                slot.blocks.Clear();
            }
            for (int i = 0; i < makeLevel.amountBlockOfTopic.Count; i++)
            {
                makeLevel.amountBlockOfTopic[i] = 0;
            }
            
            makeLevel.SettingSlots();
            makeLevel.UpdateSlotsInEditor();
            makeLevel.GenerateBlocks();
            Debug.Log("Empty connected slots grid generated inside scene view.");
        }
        GUI.backgroundColor = Color.white;
    }

    private void DrawBlocksTabContent()
    {
        GUILayout.Label("BƯỚC 2: XẾP BLOCKS & SƠN CHỦ ĐỀ TOPIC", headerStyle);
        GUILayout.Space(8);

        EditorGUILayout.HelpBox("💡 Hãy click trực tiếp nút [+] hoặc [-] xuất hiện trên đỉnh mỗi Slot trong Scene View để thêm hoặc bớt Blocks theo cách thủ công.", MessageType.Info);
        GUILayout.Space(8);

        GUILayout.Label("🎨 Cọ Vẽ Topic Paintbrush:", EditorStyles.boldLabel);
        GUILayout.Label("Chọn 1 topic bên dưới làm cọ vẽ, sau đó click chuột trực tiếp vào Block trên Scene để tô gán topic đó cực nhanh.", EditorStyles.wordWrappedMiniLabel);
        GUILayout.Space(8);

        // Brush Active State Box
        if (paintbrushTopic != null)
        {
            GUI.backgroundColor = new Color(0.1f, 0.85f, 0.4f, 1f);
            GUILayout.BeginVertical("box");
            GUILayout.Label($"🖌️ ĐANG BẬT CỌ VẼ: ID {paintbrushTopic.topicID} - {paintbrushTopic.topicName}", EditorStyles.boldLabel);
            if (GUILayout.Button("Tắt Cọ Vẽ", GUILayout.Width(100), GUILayout.Height(20)))
            {
                SetPaintbrush(null);
            }
            GUILayout.EndVertical();
            GUI.backgroundColor = Color.white;
        }
        else
        {
            GUILayout.Label("❌ Paintbrush đang Tắt (Click Block trên Scene để xoay vòng topics).", EditorStyles.miniLabel);
        }

        GUILayout.Space(8);

        // Grid selection of topics to activate brush
        GUILayout.Label("Chọn topic làm cọ vẽ:", EditorStyles.boldLabel);
        if (makeLevel.topics.Count == 0)
        {
            GUILayout.Label("⚠️ Chưa chọn topics nào! Vui lòng chọn topics ở Tab 'Cấu Hình' trước.");
        }
        else
        {
            for (int i = 0; i < makeLevel.topics.Count; i++)
            {
                BlockTopic t = makeLevel.topics[i];
                if (t == null) continue;

                // Sync and count real blocks in the scene
                int blockCount = 0;
                foreach (var s in makeLevel.slots)
                {
                    foreach (var b in s.blocks)
                    {
                        if (b.blockTopic != null && b.blockTopic.topicID == t.topicID) blockCount++;
                    }
                }

                if (i < makeLevel.amountBlockOfTopic.Count)
                {
                    makeLevel.amountBlockOfTopic[i] = blockCount;
                }

                bool isSelected = (paintbrushTopic != null && paintbrushTopic.topicID == t.topicID);
                GUI.backgroundColor = isSelected ? new Color(0.2f, 0.85f, 0.4f) : new Color(0.2f, 0.2f, 0.22f);

                GUILayout.BeginHorizontal("box");
                if (GUILayout.Button($"ID {t.topicID:D2}: {t.topicName} ({blockCount}/4 blocks gán)", GUILayout.ExpandWidth(true), GUILayout.Height(26)))
                {
                    SetPaintbrush(t);
                    makeLevel.indexTopicSelected = i;
                }
                GUILayout.EndHorizontal();
            }
            GUI.backgroundColor = Color.white;
        }

        GUILayout.Space(15);

        // Autofill Option
        GUI.backgroundColor = new Color(0.2f, 0.65f, 1f);
        if (GUILayout.Button("🎲 TỰ ĐỘNG ĐIỀN ĐỀU TOPICS (AUTO FILL)", GUILayout.Height(35)))
        {
            Undo.RecordObject(makeLevel, "Auto Fill Level Topics");
            makeLevel.AutoFillTopics();
            makeLevel.UpdateSlotsInEditor();
            makeLevel.GenerateBlocks();
            SetPaintbrush(null);
            Debug.Log("Auto-filled topics uniformly across all slots.");
        }
        GUI.backgroundColor = Color.white;
    }

    private void DrawMechanicsTabContent()
    {
        GUILayout.Label("BƯỚC 3: THIẾT LẬP MECHANICS SLOT & BLOCK", headerStyle);
        GUILayout.Space(8);

        GUILayout.Label("🎮 Hướng dẫn gán Mechanics trên Scene View:", EditorStyles.boldLabel);
        GUILayout.Label("1. Đổi Loại Slot:\n   Click trực tiếp vào Nhãn Loại Slot (ví dụ: 'NORMAL', 'ICE') dưới chân mỗi cột slot để đổi vòng tròn qua: Normal -> Hide -> Ice.", EditorStyles.wordWrappedLabel);
        GUILayout.Space(4);
        GUILayout.Label("2. Chọn Câu Hỏi Slot Ẩn (Hide Unlock Topic):\n   Nếu Slot ở chế độ 'Hide', click vào nút '🔓 Q:...' dưới chân slot để chọn topic yêu cầu mở khóa bằng menu thả xuống.", EditorStyles.wordWrappedLabel);
        GUILayout.Space(4);
        GUILayout.Label("3. Đổi Loại Block:\n   Click trực tiếp vào từng Block trên Scene View để đổi chế độ: Normal Block -> Hide Block (Block bị giấu/ẩn).", EditorStyles.wordWrappedLabel);

        GUILayout.Space(15);
        GUILayout.Box("", GUILayout.Height(1), GUILayout.ExpandWidth(true));
        GUILayout.Space(10);

        // Stats summary
        int normalSlots = 0, hideSlots = 0, iceSlots = 0;
        foreach (var s in makeLevel.slots)
        {
            if (s.slotType == SlotController.SlotType.Normal) normalSlots++;
            else if (s.slotType == SlotController.SlotType.Hide) hideSlots++;
            else if (s.slotType == SlotController.SlotType.Ice) iceSlots++;
        }

        GUILayout.Label("Thống Kê Cơ Bản:", EditorStyles.boldLabel);
        GUILayout.Label($"• Tổng số Slots: {makeLevel.slots.Count}");
        GUILayout.Label($"  - Slot Bình Thường (Normal): {normalSlots}");
        GUILayout.Label($"  - Slot Ẩn (Hide): {hideSlots}");
        GUILayout.Label($"  - Slot Băng (Ice): {iceSlots}");
    }

    private void DrawPlaytestTabContent()
    {
        GUILayout.Label("BƯỚC 4: CHƠI THỬ TỨC THÌ (INSTANT PLAYTEST)", headerStyle);
        GUILayout.Space(8);

        GUILayout.Label("Tự động biên dịch màn chơi đang dựng thành file LevelDataSO tạm thời, đánh dấu cờ kiểm nghiệm và kích hoạt Play Mode để bạn thử nghiệm di chuyển block ngay tại chỗ.", EditorStyles.wordWrappedLabel);
        GUILayout.Space(15);

        if (EditorApplication.isPlaying)
        {
            GUILayout.BeginVertical("box");
            GUI.color = Color.red;
            GUILayout.Label("🔴 GAME ĐANG CHẠY THỬ (PLAYTEST ACTIVE)", EditorStyles.boldLabel);
            GUI.color = Color.white;
            GUILayout.Label("Các tiến trình nâng level và lưu data Firebase đã được tự động khóa để bảo mật dữ liệu lưu trữ thật.", EditorStyles.wordWrappedMiniLabel);
            GUILayout.Space(15);

            GUI.backgroundColor = new Color(0.9f, 0.3f, 0.25f);
            if (GUILayout.Button("🛑 DỪNG CHƠI THỬ (STOP PLAYTEST)", GUILayout.Height(45)))
            {
                EditorApplication.isPlaying = false;
            }
            GUI.backgroundColor = Color.white;
            GUILayout.EndVertical();
        }
        else
        {
            int blockCount = 0;
            foreach (var slot in makeLevel.slots) blockCount += slot.blocks.Count;

            if (blockCount == 0)
            {
                GUILayout.Label("⚠️ Không có block nào để chơi thử! Vui lòng thêm blocks trước.");
            }
            else if (blockCount % 4 != 0)
            {
                GUILayout.Label($"⚠️ Tổng số block ({blockCount}) chưa chia hết cho 4 (bắt buộc bộ 4 để giải đố!).", EditorStyles.boldLabel);
            }
            else
            {
                GUI.backgroundColor = new Color(0.2f, 0.8f, 0.45f);
                if (GUILayout.Button("▶ BẮT ĐẦU CHƠI THỬ NGAY", GUILayout.Height(50)))
                {
                    // Update and save structures
                    makeLevel.SettingSlots();

                    // Instantiating temporary ScriptableObject
                    LevelDataSO playtestSO = ScriptableObject.CreateInstance<LevelDataSO>();
                    playtestSO.level = makeLevel.Level;
                    playtestSO.moves = makeLevel.moves;
                    playtestSO.difficult = makeLevel.difficulty;
                    playtestSO.row1 = makeLevel.row1;
                    playtestSO.row2 = makeLevel.row2;
                    playtestSO.numsTopic = makeLevel.topics.Count;
                    playtestSO.slots = new List<SlotSetupData>();

                    foreach (var s in makeLevel.slots)
                    {
                        SlotSetupData sData = new SlotSetupData {
                            slotType = s.slotType,
                            questionTopic = s.questionTopic,
                            position = s.position,
                            blocks = new List<BlockSetupData>()
                        };
                        foreach (var b in s.blocks)
                        {
                            sData.blocks.Add(new BlockSetupData {
                                typeBlock = b.typeBlock,
                                blockTopic = b.blockTopic,
                                indexSprite = b.indexSprite
                            });
                        }
                        playtestSO.slots.Add(sData);
                    }

                    // Assign to LevelLoader
                    LevelLoader.playtestLevelData = playtestSO;
                    LevelLoader.isPlaytestingTempLevel = true;

                    // Execute play mode
                    EditorApplication.isPlaying = true;
                    Debug.Log("<color=cyan>[Level Editor]</color> Bắt đầu Instant Playtest!");
                }
                GUI.backgroundColor = Color.white;
            }
        }
    }

    private void DrawSaveTabContent()
    {
        GUILayout.Label("BƯỚC 5: THIẾT LẬP LEVEL & LƯU FILE SO", headerStyle);
        GUILayout.Space(8);

        // Fields
        EditorGUI.BeginChangeCheck();
        int newLevelIndex = EditorGUILayout.IntField("Số Thứ Tự Level (Index):", makeLevel.Level);
        int newMoves = EditorGUILayout.IntField("Số Lượt Di Chuyển (Moves):", makeLevel.moves);
        
        string[] diffs = { "Easy (Dễ)", "Hard (Khó)", "Very Hard (Siêu Khó)" };
        int newDifficulty = EditorGUILayout.Popup("Độ Khó Level (Difficulty):", makeLevel.difficulty, diffs);
        
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(makeLevel, "Change Level Settings");
            makeLevel.Level = newLevelIndex;
            makeLevel.moves = newMoves;
            makeLevel.difficulty = newDifficulty;
        }

        GUILayout.Space(15);
        GUILayout.Label("🔍 KIỂM TRA ĐIỀU KIỆN LƯU MÀN (VALIDATION):", EditorStyles.boldLabel);

        // Verification validation parameters
        int totalBlocks = 0;
        foreach (var s in makeLevel.slots) totalBlocks += s.blocks.Count;

        bool hasSlots = makeLevel.slots.Count > 0;
        bool multOfFour = (totalBlocks > 0 && totalBlocks % 4 == 0);
        bool topicsPerfect = true;
        List<string> errorLog = new List<string>();

        for (int i = 0; i < makeLevel.topics.Count; i++)
        {
            BlockTopic t = makeLevel.topics[i];
            if (t == null) continue;

            int cnt = 0;
            foreach (var s in makeLevel.slots)
            {
                foreach (var b in s.blocks)
                {
                    if (b.blockTopic != null && b.blockTopic.topicID == t.topicID) cnt++;
                }
            }
            if (cnt != 4)
            {
                topicsPerfect = false;
                errorLog.Add($"ID {t.topicID} ({t.topicName}): có {cnt}/4 blocks");
            }
        }

        DrawValidationItem("Đã dựng lưới Slot trong Scene", hasSlots);
        DrawValidationItem($"Tổng số block chia hết cho 4 (Hiện có: {totalBlocks})", multOfFour);
        DrawValidationItem("Mỗi topic gán chính xác cho 4 blocks", topicsPerfect && makeLevel.topics.Count > 0);

        if (!topicsPerfect && errorLog.Count > 0)
        {
            GUILayout.Space(5);
            GUI.color = new Color(1f, 0.6f, 0.2f);
            GUILayout.Label("Lỗi phân bổ topics:", EditorStyles.boldLabel);
            foreach (var err in errorLog)
            {
                GUILayout.Label($" ⚠️ {err}", EditorStyles.miniLabel);
            }
            GUI.color = Color.white;
        }

        GUILayout.Space(20);

        bool readyToSave = hasSlots && multOfFour && topicsPerfect && makeLevel.topics.Count > 0;

        if (readyToSave)
        {
            GUI.backgroundColor = new Color(0.15f, 0.8f, 0.35f);
            if (GUILayout.Button("💾 LƯU MÀN CHƠI (SAVE LEVEL DATA)", GUILayout.Height(50)))
            {
                Undo.RecordObject(makeLevel, "Pre-Save Validation");
                makeLevel.SettingSlots();
                makeLevel.SaveLevelData();
                EditorUtility.DisplayDialog("Lưu Màn Chơi", $"Màn chơi Level {makeLevel.Level} đã được lưu thành công vào Assets/Resources/Data/Levels/Level_{makeLevel.Level:D2}.asset", "Tuyệt Vời");
            }
        }
        else
        {
            GUI.backgroundColor = new Color(0.35f, 0.35f, 0.35f);
            GUILayout.Box("⚠️ HÃY HOÀN THÀNH TOÀN BỘ ĐIỀU KIỆN PHÂN BỔ TRƯỚC KHI LƯU", GUILayout.Height(50), GUILayout.ExpandWidth(true));
        }
        GUI.backgroundColor = Color.white;
    }

    private void DrawValidationItem(string text, bool passed)
    {
        GUILayout.BeginHorizontal();
        GUI.color = passed ? Color.green : Color.red;
        GUILayout.Label(passed ? " ✔ " : " ✘ ", GUILayout.Width(22));
        GUI.color = Color.white;
        GUILayout.Label(text);
        GUILayout.EndHorizontal();
    }
}
#endif
