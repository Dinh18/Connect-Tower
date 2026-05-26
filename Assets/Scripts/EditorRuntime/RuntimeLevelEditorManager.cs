using UnityEngine;
using System.Collections.Generic;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class RuntimeLevelEditorManager : MonoBehaviour
{
    public static RuntimeLevelEditorManager Instance { get; private set; }

    [Header("Level Data Config")]
    public int levelIndex = 0;
    public int moves = 10;
    public int difficulty = 0; // 0: Easy, 1: Hard, 2: VeryHard
    public int row1 = 3;
    public int row2 = 3;

    public SlotController.SlotType currentSlotMechanic = SlotController.SlotType.Normal;
    public BlockController.BlockType currentBlockMechanic = BlockController.BlockType.Normal;

    public List<BlockTopic> topics = new List<BlockTopic>();
    public List<int> amountBlockOfTopic = new List<int>();
    public List<SlotSetupData> slots = new List<SlotSetupData>();

    public BlockTopic currentPaintbrushTopic;
    public int currentPaintbrushIndex = -1;

    private SlotsManager slotsManager;
    private BlocksManager blocksManager;
    private LevelLoader levelLoader;

    public bool isEditMode = false;
    private bool showGUI = false;
    private Rect windowRect = new Rect(20, 20, 400, 700);
    private Vector2 scrollPos;

    private BlockTopic[] availableTopics;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        
        DontDestroyOnLoad(gameObject);
        availableTopics = Resources.LoadAll<BlockTopic>("Data/topics2");
    }

    private void Start()
    {
        InputManager.OnSlotClicked += HandleSlotClicked;
    }

    private void OnDestroy()
    {
        InputManager.OnSlotClicked -= HandleSlotClicked;
    }

    private void Update()
    {
        // Bí mật mở Editor bằng phím F12 trên PC hoặc có thể được gọi từ UI (HomePanel)
        if (Input.GetKeyDown(KeyCode.F12))
        {
            ToggleEditor();
        }

        if (isEditMode)
        {
            if (Input.GetMouseButtonDown(0))
            {
                // Bỏ qua nếu click vào UI Canvas thông thường
                if (UnityEngine.EventSystems.EventSystem.current != null && 
                    UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
                {
                    return; 
                }

                // Bỏ qua nếu click vào vùng của bảng OnGUI bên trái (Width = 300 * scale 1.15)
                if (showGUI && Input.mousePosition.x < 360f)
                {
                    return;
                }

                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    if (hit.collider.TryGetComponent(out SlotController slot))
                    {
                        HandleSlotClicked(slot);
                    }
                }
            }
        }
    }

    public void ToggleEditor()
    {
        showGUI = !showGUI;
        if (showGUI) 
        {
            EnterEditMode();
        }
    }

    public void OpenEditor()
    {
        showGUI = true;
        EnterEditMode();
    }

    public void InitManagers()
    {
        if(slotsManager == null) slotsManager = CoreServices.Get<SlotsManager>();
        if(blocksManager == null) blocksManager = CoreServices.Get<BlocksManager>();
        if(levelLoader == null) levelLoader = CoreServices.Get<LevelLoader>();
    }

    public void EnterEditMode()
    {
        isEditMode = true;
        var input = CoreServices.Get<InputManager>();
        if (input != null) input.SetInputBlocked(true);
    }

    public void ExitEditMode()
    {
        isEditMode = false;
        var input = CoreServices.Get<InputManager>();
        if (input != null) input.SetInputBlocked(false);
    }

    public void GenerateGrid()
    {
        InitManagers();

        int totalSlots = row1 + row2;
        while (slots.Count > totalSlots) slots.RemoveAt(slots.Count - 1);
        while (slots.Count < totalSlots) slots.Add(new SlotSetupData());

        foreach (var slot in slots)
        {
            if (slot.blocks == null) slot.blocks = new List<BlockSetupData>();
            slot.blocks.Clear();
        }

        for (int i = 0; i < amountBlockOfTopic.Count; i++) amountBlockOfTopic[i] = 0;

        RenderGrid();
    }

    public void ResetAll()
    {
        SaveUndoState();
        topics.Clear();
        amountBlockOfTopic.Clear();
        currentPaintbrushTopic = null;
        currentPaintbrushIndex = -1;
        currentSlotMechanic = SlotController.SlotType.Normal;
        currentBlockMechanic = BlockController.BlockType.Normal;
        row1 = 3;
        row2 = 3;
        moves = 0;
        GenerateGrid();
    }

    public void RenderGrid()
    {
        InitManagers();
        
        levelLoader.slots = new List<SlotController>();
        slotsManager.SlotsGenerate(row1, row2, levelLoader.slots, slots, topics.Count);
        blocksManager.BlocksGenerate(slots, levelLoader.slots);
    }

    private void HandleSlotClicked(SlotController slotController)
    {
        if (!isEditMode) return;
        
        int slotIndex = levelLoader.slots.IndexOf(slotController);
        if (slotIndex == -1) return;

        SaveUndoState();

        slots[slotIndex].slotType = currentSlotMechanic;
        if (currentSlotMechanic == SlotController.SlotType.Hide)
        {
            if (currentPaintbrushTopic != null) slots[slotIndex].questionTopic = currentPaintbrushTopic;
            else if (slots[slotIndex].questionTopic == null && topics.Count > 0) slots[slotIndex].questionTopic = topics[0];
        }

        if (currentPaintbrushTopic != null)
        {
            AddBlockToSlot(slotIndex, currentPaintbrushTopic, (int)currentBlockMechanic);
            RenderGrid();
        }
        else
        {
            RemoveBlockFromSlot(slotIndex);
            RenderGrid();
        }
    }

    private Stack<string> undoStack = new Stack<string>();

    private void SaveUndoState()
    {
        string json = JsonUtility.ToJson(GenerateLevelDataSO());
        undoStack.Push(json);
    }

    public void Undo()
    {
        if (undoStack.Count > 0)
        {
            string json = undoStack.Pop();
            LevelDataSO tempSO = ScriptableObject.CreateInstance<LevelDataSO>();
            JsonUtility.FromJsonOverwrite(json, tempSO);
            
            levelIndex = tempSO.level;
            moves = tempSO.moves;
            difficulty = tempSO.difficult;
            row1 = tempSO.row1;
            row2 = tempSO.row2;
            slots = tempSO.slots;

            amountBlockOfTopic.Clear();
            for (int i=0; i<topics.Count; i++) amountBlockOfTopic.Add(0);
            foreach(var s in slots)
            {
                foreach(var b in s.blocks)
                {
                    int tIdx = topics.FindIndex(t => t.topicID == b.blockTopic.topicID);
                    if (tIdx >= 0) amountBlockOfTopic[tIdx]++;
                }
            }

            RenderGrid();
        }
    }

    public void RestoreFromPlaytest()
    {
        levelLoader.LoadLevel(); 
        showGUI = true;
        EnterEditMode();
    }

    public void AddBlockToSlot(int slotIndex, BlockTopic blockTopic, int typeBlock)
    {
        if (slots[slotIndex].blocks.Count >= 4) return;
        if (currentPaintbrushIndex >= 0 && currentPaintbrushIndex < amountBlockOfTopic.Count)
        {
            if (amountBlockOfTopic[currentPaintbrushIndex] >= 4) return;
        }

        BlockSetupData newBlockSetup = new BlockSetupData();
        newBlockSetup.blockTopic = blockTopic;
        newBlockSetup.typeBlock = (BlockController.BlockType)typeBlock;
        
        if (currentPaintbrushIndex >= 0 && currentPaintbrushIndex < amountBlockOfTopic.Count)
        {
            newBlockSetup.indexSprite = amountBlockOfTopic[currentPaintbrushIndex];
            amountBlockOfTopic[currentPaintbrushIndex]++;
        }
        else
        {
            newBlockSetup.indexSprite = 0;
        }
        
        // Insert vào đầu danh sách (index 0) để block mới nằm trên cùng (TOP)
        slots[slotIndex].blocks.Insert(0, newBlockSetup);
    }

    public void RemoveBlockFromSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= slots.Count) return;

        var slot = slots[slotIndex];
        if (slot.blocks.Count == 0) return;

        // Xóa block trên cùng (ở index 0 do đã Insert(0))
        var block = slot.blocks[0];
        
        int tIdx = topics.FindIndex(t => t.topicID == block.blockTopic.topicID);
        if (tIdx >= 0 && tIdx < amountBlockOfTopic.Count)
        {
            amountBlockOfTopic[tIdx]--;
        }

        slot.blocks.RemoveAt(0);
    }

    private LevelDataSO GenerateLevelDataSO()
    {
        LevelDataSO so = ScriptableObject.CreateInstance<LevelDataSO>();
        so.level = levelIndex;
        so.moves = moves;
        so.difficult = difficulty;
        so.row1 = row1;
        so.row2 = row2;
        so.numsTopic = topics.Count;
        so.slots = new List<SlotSetupData>();

        foreach (var s in slots)
        {
            SlotSetupData sData = new SlotSetupData();
            sData.slotType = s.slotType;
            sData.questionTopic = s.questionTopic;
            sData.blocks = new List<BlockSetupData>();
            foreach (var b in s.blocks)
            {
                BlockSetupData bData = new BlockSetupData();
                bData.blockTopic = b.blockTopic;
                bData.typeBlock = b.typeBlock;
                bData.indexSprite = b.indexSprite;
                sData.blocks.Add(bData);
            }
            so.slots.Add(sData);
        }
        return so;
    }

    public void Playtest()
    {
        InitManagers();
        
        LevelLoader.playtestLevelData = GenerateLevelDataSO();
        LevelLoader.isPlaytestingTempLevel = true;
        
        ExitEditMode();
        showGUI = false; // Hide UI to play
        
        levelLoader.LoadLevel(); // Reload level using temp data
        CoreServices.Get<GameManager>().ChangeState(GameManager.GameState.Playing);
    }

    public void SaveLevel()
    {
        LevelDataSO newLevelData = GenerateLevelDataSO();

#if UNITY_EDITOR
        string dir = "Assets/Resources/Data/Levels";
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
        
        string assetPath = $"{dir}/Level_{levelIndex:D2}.asset";
        AssetDatabase.CreateAsset(newLevelData, assetPath);
        AssetDatabase.Refresh();
        Debug.Log("Level saved to " + assetPath);
#else
        string json = JsonUtility.ToJson(newLevelData, true);
        string path = Path.Combine(Application.persistentDataPath, $"Level_{levelIndex:D2}.json");
        File.WriteAllText(path, json);
        Debug.Log("Level saved to JSON at " + path);
#endif
    }

    private Texture2D solidBg;

    private void OnGUI()
    {
        // Thu nhỏ UI một chút theo yêu cầu
        float scale = 1.15f;
        GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(scale, scale, 1.0f));

        if (!showGUI)
        {
            if (isEditMode)
            {
                GUI.backgroundColor = new Color(0.2f, 0.8f, 0.4f);
                if (GUI.Button(new Rect(20, 20, 150, 45), "MỞ BẢNG EDITOR"))
                {
                    showGUI = true;
                }
                GUI.backgroundColor = Color.white;
                
                string brushName = currentPaintbrushTopic != null ? currentPaintbrushTopic.topicName : "XÓA BLOCK";
                GUI.Label(new Rect(180, 30, 300, 40), $"CỌ ĐANG CHỌN: {brushName}");
            }
            else
            {
                GUI.backgroundColor = new Color(0.2f, 0.6f, 1f);
                if (GUI.Button(new Rect(20, 20, 150, 45), "MỞ LEVEL EDITOR"))
                {
                    OpenEditor();
                }
                GUI.backgroundColor = Color.white;
            }
            return;
        }
        
        // Dock window sát lề trái, thu hẹp lại 300
        windowRect = new Rect(0, 0, 300, Screen.height / scale);

        if (solidBg == null) 
        {
            solidBg = new Texture2D(1, 1);
            solidBg.SetPixel(0, 0, new Color(0.15f, 0.15f, 0.15f, 0.95f));
            solidBg.Apply();
        }
        GUI.DrawTexture(windowRect, solidBg);

        windowRect = GUILayout.Window(999, windowRect, DrawEditorWindow, "Runtime Level Editor");
    }

    private void DrawEditorWindow(int windowID)
    {
        scrollPos = GUILayout.BeginScrollView(scrollPos);

        GUILayout.Label("--- Cấu Hình ---", GUI.skin.box);
        GUILayout.BeginHorizontal();
        GUILayout.Label("Level ID:");
        string lvlStr = GUILayout.TextField(levelIndex.ToString());
        if (int.TryParse(lvlStr, out int l)) levelIndex = l;
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Moves:");
        string movesStr = GUILayout.TextField(moves.ToString());
        if (int.TryParse(movesStr, out int m)) moves = m;
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label($"Row1: {row1}", GUILayout.Width(80));
        row1 = (int)GUILayout.HorizontalSlider(row1, 0, 5);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label($"Row2: {row2}", GUILayout.Width(80));
        row2 = (int)GUILayout.HorizontalSlider(row2, 0, 5);
        GUILayout.EndHorizontal();

        if (GUILayout.Button("1. Dựng Lưới (Generate Grid)", GUILayout.Height(40)))
        {
            GenerateGrid();
        }

        GUILayout.Space(10);
        GUILayout.Label("--- Topics (Chọn làm Cọ Vẽ) ---", GUI.skin.box);
        
        if (availableTopics != null)
        {
            for (int i = 0; i < availableTopics.Length; i++)
            {
                BlockTopic t = availableTopics[i];
                bool isSelected = topics.Contains(t);
                
                GUILayout.BeginHorizontal();
                bool toggle = GUILayout.Toggle(isSelected, $"T:{t.topicID} {t.topicName}");
                if (toggle && !isSelected)
                {
                    topics.Add(t);
                    amountBlockOfTopic.Add(0);
                }
                else if (!toggle && isSelected)
                {
                    int idx = topics.IndexOf(t);
                    topics.RemoveAt(idx);
                    amountBlockOfTopic.RemoveAt(idx);
                }

                if (toggle)
                {
                    int listIdx = topics.IndexOf(t);
                    int count = listIdx >= 0 && listIdx < amountBlockOfTopic.Count ? amountBlockOfTopic[listIdx] : 0;
                    GUI.color = (currentPaintbrushTopic == t) ? Color.green : Color.white;
                    if (GUILayout.Button($"[Chọn Cọ] ({count}/4)"))
                    {
                        currentPaintbrushTopic = t;
                        currentPaintbrushIndex = listIdx;
                    }
                    GUI.color = Color.white;
                }
                GUILayout.EndHorizontal();
            }
        }

        if (GUILayout.Button("Xóa Cọ Vẽ (Chế độ xóa Block)"))
        {
            currentPaintbrushTopic = null;
            currentPaintbrushIndex = -1;
        }

        GUILayout.Space(10);
        GUILayout.Label("--- CƠ CHẾ (Mechanics) ---", GUI.skin.box);
        
        GUILayout.Label("Loại Cột (Slot):");
        GUILayout.BeginHorizontal();
        if (GUILayout.Toggle(currentSlotMechanic == SlotController.SlotType.Normal, "Bình Thường (Hủy)", "Button")) currentSlotMechanic = SlotController.SlotType.Normal;
        if (GUILayout.Toggle(currentSlotMechanic == SlotController.SlotType.Hide, "Ẩn (Hide)", "Button")) currentSlotMechanic = SlotController.SlotType.Hide;
        if (GUILayout.Toggle(currentSlotMechanic == SlotController.SlotType.Ice, "Băng (Ice)", "Button")) currentSlotMechanic = SlotController.SlotType.Ice;
        GUILayout.EndHorizontal();

        GUILayout.Label("Loại Block:");
        GUILayout.BeginHorizontal();
        if (GUILayout.Toggle(currentBlockMechanic == BlockController.BlockType.Normal, "Bình Thường (Hủy)", "Button")) currentBlockMechanic = BlockController.BlockType.Normal;
        if (GUILayout.Toggle(currentBlockMechanic == BlockController.BlockType.Hide, "Ẩn (Hide)", "Button")) currentBlockMechanic = BlockController.BlockType.Hide;
        GUILayout.EndHorizontal();

        GUILayout.Space(10);
        GUILayout.Label("--- Hành Động ---");
        GUILayout.Space(10);
        
        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Hoàn Tác (Undo)", GUILayout.Height(30)))
        {
            Undo();
        }
        if (GUILayout.Button("Reset Toàn Bộ", GUILayout.Height(30)))
        {
            ResetAll();
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(10);
        if (GUILayout.Button("2. Playtest (Thử Nghiệm)", GUILayout.Height(40)))
        {
            Playtest();
        }

        if (GUILayout.Button("3. Save Level Data", GUILayout.Height(40)))
        {
            SaveLevel();
        }

        GUILayout.EndScrollView();
        GUI.DragWindow();
    }
}
