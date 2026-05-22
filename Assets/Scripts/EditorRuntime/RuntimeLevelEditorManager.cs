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

                // Bỏ qua nếu click vào vùng của bảng OnGUI bên trái (Width = 350 * scale 1.5 = 525 pixel)
                if (showGUI && Input.mousePosition.x < 550f)
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

        if (currentPaintbrushTopic != null)
        {
            AddBlockToSlot(slotIndex, currentPaintbrushTopic, 0);
            RenderGrid();
        }
        else
        {
            RemoveBlockFromSlot(slotIndex);
            RenderGrid();
        }
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

    public void Playtest()
    {
        InitManagers();
        
        LevelDataSO playtestSO = ScriptableObject.CreateInstance<LevelDataSO>();
        playtestSO.level = levelIndex;
        playtestSO.moves = moves;
        playtestSO.difficult = difficulty;
        playtestSO.row1 = row1;
        playtestSO.row2 = row2;
        playtestSO.numsTopic = topics.Count;
        playtestSO.slots = new List<SlotSetupData>();

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
            playtestSO.slots.Add(sData);
        }

        LevelLoader.playtestLevelData = playtestSO;
        LevelLoader.isPlaytestingTempLevel = true;
        
        ExitEditMode();
        showGUI = false; // Hide UI to play
        
        levelLoader.LoadLevel(); // Reload level using temp data
        CoreServices.Get<GameManager>().ChangeState(GameManager.GameState.Playing);
    }

    public void SaveLevel()
    {
        LevelDataSO newLevelData = ScriptableObject.CreateInstance<LevelDataSO>();
        newLevelData.level = levelIndex;
        newLevelData.moves = moves;
        newLevelData.difficult = difficulty;
        newLevelData.row1 = row1;
        newLevelData.row2 = row2;
        newLevelData.numsTopic = topics.Count;
        newLevelData.slots = new List<SlotSetupData>();

        foreach (var slot in slots)
        {
            SlotSetupData sData = new SlotSetupData();
            sData.slotType = slot.slotType;
            sData.questionTopic = slot.questionTopic;
            sData.blocks = new List<BlockSetupData>();
            foreach (var block in slot.blocks)
            {
                BlockSetupData bData = new BlockSetupData();
                bData.blockTopic = block.blockTopic;
                bData.typeBlock = block.typeBlock;
                bData.indexSprite = block.indexSprite;
                sData.blocks.Add(bData);
            }
            newLevelData.slots.Add(sData);
        }

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
        // Giảm scale xuống 1.5 để phù hợp màn hình PC (Free Aspect)
        float scale = 1.5f;
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
        
        // Dock window sát lề trái, độ rộng 350 để chừa phần bên phải xếp block
        windowRect = new Rect(0, 0, 350, Screen.height / scale);

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
        GUILayout.Label("Row1:");
        string row1Str = GUILayout.TextField(row1.ToString());
        if (int.TryParse(row1Str, out int r1)) row1 = r1;
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Row2:");
        string row2Str = GUILayout.TextField(row2.ToString());
        if (int.TryParse(row2Str, out int r2)) row2 = r2;
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
        GUILayout.Label("--- Hành Động ---", GUI.skin.box);
        if (GUILayout.Button("2. Playtest (Chơi thử)", GUILayout.Height(40)))
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
