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
    public enum EditActionMode { Block, Slot }
    public EditActionMode currentEditMode = EditActionMode.Block;
    public BlockTopic currentHiddenSlotTopic = null;

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
    private LevelDataSO[] availableLevels;
    private bool showLevelDropdown = false;
    private Vector2 levelScrollPos;

    private void RefreshAvailableLevels()
    {
        availableLevels = Resources.LoadAll<LevelDataSO>("Data/Levels");
        System.Array.Sort(availableLevels, (a, b) => a.level.CompareTo(b.level));
    }

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
            bool leftClick = Input.GetMouseButtonDown(0);
            bool rightClick = Input.GetMouseButtonDown(1);
            
            if (leftClick || rightClick)
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
                        int targetIndex = 0;
                        int slotIndex = levelLoader.slots.IndexOf(slot);
                        if (slotIndex != -1 && slots[slotIndex].blocks != null && slots[slotIndex].blocks.Count > 0)
                        {
                            int count = slots[slotIndex].blocks.Count;
                            float relativeY = hit.point.y - slot.stackAnchor.position.y;
                            float minDistance = float.MaxValue;
                            
                            for(int j = 0; j < count; j++)
                            {
                                int index_from_bottom = count - 1 - j;
                                // Constants.BLOCK_HEIGHT is used in BlocksManager to space them
                                float blockY = index_from_bottom * Constants.BLOCK_HEIGHT;
                                float dist = Mathf.Abs(relativeY - blockY);
                                if (dist < minDistance)
                                {
                                    minDistance = dist;
                                    targetIndex = j;
                                }
                            }
                        }
                        
                        ProcessSlotClick(slot, rightClick, targetIndex);
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
        currentHiddenSlotTopic = null;
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
        ProcessSlotClick(slotController, false, 0);
    }

    private void ProcessSlotClick(SlotController slotController, bool isRightClick, int targetBlockIndex)
    {
        if (!isEditMode) return;
        
        int slotIndex = levelLoader.slots.IndexOf(slotController);
        if (slotIndex == -1) return;

        SaveUndoState();

        if (currentEditMode == EditActionMode.Slot)
        {
            slots[slotIndex].slotType = currentSlotMechanic;
            if (currentSlotMechanic == SlotController.SlotType.Hide)
            {
                if (currentHiddenSlotTopic != null) slots[slotIndex].questionTopic = currentHiddenSlotTopic;
                else if (topics.Count > 0) slots[slotIndex].questionTopic = topics[0];
            }
            else 
            {
                slots[slotIndex].questionTopic = null;
            }
        }
        else if (currentEditMode == EditActionMode.Block)
        {
            if (isRightClick)
            {
                RemoveBlockFromSlot(slotIndex);
            }
            else
            {
                if (currentPaintbrushTopic != null)
                {
                    AddBlockToSlot(slotIndex, currentPaintbrushTopic, (int)currentBlockMechanic);
                }
                else
                {
                    if (targetBlockIndex < 0) targetBlockIndex = 0;
                    if (slots[slotIndex].blocks != null && targetBlockIndex < slots[slotIndex].blocks.Count)
                    {
                        slots[slotIndex].blocks[targetBlockIndex].typeBlock = currentBlockMechanic;
                    }
                }
            }
        }

        RenderGrid();
    }

    private Stack<string> undoStack = new Stack<string>();

    private void SaveUndoState()
    {
        string json = JsonUtility.ToJson(GenerateLevelDataSO());
        undoStack.Push(json);
    }

    private void LoadFromSO(LevelDataSO tempSO)
    {
        levelIndex = tempSO.level;
        moves = tempSO.moves;
        difficulty = tempSO.difficult;
        row1 = tempSO.row1;
        row2 = tempSO.row2;
        
        slots = new List<SlotSetupData>();
        foreach (var sData in tempSO.slots)
        {
            SlotSetupData newSlot = new SlotSetupData();
            newSlot.slotType = sData.slotType;
            newSlot.questionTopic = sData.questionTopic;
            newSlot.blocks = new List<BlockSetupData>();
            if (sData.blocks != null)
            {
                foreach (var bData in sData.blocks)
                {
                    BlockSetupData newBlock = new BlockSetupData();
                    newBlock.blockTopic = bData.blockTopic;
                    newBlock.typeBlock = bData.typeBlock;
                    newBlock.indexSprite = bData.indexSprite;
                    newSlot.blocks.Add(newBlock);
                }
            }
            slots.Add(newSlot);
        }

        topics.Clear();
        amountBlockOfTopic.Clear();
        foreach (var s in slots)
        {
            if (s.blocks != null)
            {
                foreach (var b in s.blocks)
                {
                    if (b.blockTopic != null && !topics.Exists(t => t.topicID == b.blockTopic.topicID))
                    {
                        topics.Add(b.blockTopic);
                        amountBlockOfTopic.Add(0);
                    }
                }
            }
            if (s.slotType == SlotController.SlotType.Hide && s.questionTopic != null)
            {
                if (!topics.Exists(t => t.topicID == s.questionTopic.topicID))
                {
                    topics.Add(s.questionTopic);
                    amountBlockOfTopic.Add(0);
                }
            }
        }
        
        for (int i = 0; i < topics.Count; i++) amountBlockOfTopic.Add(0);
        foreach (var s in slots)
        {
            if (s.blocks != null)
            {
                foreach (var b in s.blocks)
                {
                    int tIdx = topics.FindIndex(t => t.topicID == b.blockTopic.topicID);
                    if (tIdx >= 0) amountBlockOfTopic[tIdx]++;
                }
            }
        }

        RenderGrid();
    }

    public void Undo()
    {
        if (undoStack.Count > 0)
        {
            string json = undoStack.Pop();
            LevelDataSO tempSO = ScriptableObject.CreateInstance<LevelDataSO>();
            JsonUtility.FromJsonOverwrite(json, tempSO);
            LoadFromSO(tempSO);
        }
    }

    public void LoadLevelEditor()
    {
        InitManagers();
#if UNITY_EDITOR
        string assetPath = $"Assets/Resources/Data/Levels/Level_{levelIndex:D2}.asset";
        LevelDataSO so = UnityEditor.AssetDatabase.LoadAssetAtPath<LevelDataSO>(assetPath);
        if (so != null)
        {
            LoadFromSO(so);
            Debug.Log("Loaded level from " + assetPath);
        }
        else
        {
            Debug.LogError("Not found: " + assetPath);
        }
#else
        string path = Path.Combine(Application.persistentDataPath, $"Level_{levelIndex:D2}.json");
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            LevelDataSO so = ScriptableObject.CreateInstance<LevelDataSO>();
            JsonUtility.FromJsonOverwrite(json, so);
            LoadFromSO(so);
            Debug.Log("Loaded level from " + path);
        }
        else
        {
            Debug.LogError("Not found: " + path);
        }
#endif
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

    public void InsertLevel()
    {
#if UNITY_EDITOR
        if (!EditorUtility.DisplayDialog("Xác nhận Chèn Level", 
            $"Bạn sắp chèn một level mới vào ID {levelIndex}. Tất cả các level từ {levelIndex} trở đi sẽ bị đẩy lên 1 ID (ví dụ Level_{levelIndex:D2} thành Level_{levelIndex+1:D2}).\nBạn có chắc chắn muốn thực hiện không?", 
            "Có, Chèn", "Hủy"))
        {
            return;
        }
#endif

        LevelDataSO newLevelData = GenerateLevelDataSO();

#if UNITY_EDITOR
        string dir = "Assets/Resources/Data/Levels";
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

        // Tìm Max Level
        int maxLevel = -1;
        string[] guids = AssetDatabase.FindAssets("t:LevelDataSO", new[] { dir });
        foreach(string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            LevelDataSO so = AssetDatabase.LoadAssetAtPath<LevelDataSO>(assetPath);
            if (so != null && so.level > maxLevel) 
            {
                maxLevel = so.level;
            }
        }

        // Shift levels từ maxLevel lùi về levelIndex
        for (int i = maxLevel; i >= levelIndex; i--)
        {
            string oldPath = $"{dir}/Level_{i:D2}.asset";
            string newName = $"Level_{i + 1:D2}";
            
            LevelDataSO so = AssetDatabase.LoadAssetAtPath<LevelDataSO>(oldPath);
            if (so != null)
            {
                so.level = i + 1;
                EditorUtility.SetDirty(so);
                AssetDatabase.RenameAsset(oldPath, newName);
            }
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // Lưu level hiện tại vào chỗ vừa trống
        string newPath = $"{dir}/Level_{levelIndex:D2}.asset";
        AssetDatabase.CreateAsset(newLevelData, newPath);
        AssetDatabase.Refresh();
        Debug.Log($"<color=green>Đã chèn level mới thành công tại {newPath}. Các level cũ đã được dịch chuyển.</color>");
#else
        string dir = Application.persistentDataPath;
        int maxLevel = -1;
        string[] files = Directory.GetFiles(dir, "Level_*.json");
        foreach(string file in files)
        {
            string fileName = Path.GetFileNameWithoutExtension(file);
            if (fileName.StartsWith("Level_") && int.TryParse(fileName.Substring(6), out int l))
            {
                if (l > maxLevel) maxLevel = l;
            }
        }

        for (int i = maxLevel; i >= levelIndex; i--)
        {
            string oldPath = Path.Combine(dir, $"Level_{i:D2}.json");
            string newPath = Path.Combine(dir, $"Level_{i + 1:D2}.json");
            if (File.Exists(oldPath))
            {
                string oldJson = File.ReadAllText(oldPath);
                LevelDataSO tempSO = ScriptableObject.CreateInstance<LevelDataSO>();
                JsonUtility.FromJsonOverwrite(oldJson, tempSO);
                tempSO.level = i + 1;
                File.WriteAllText(newPath, JsonUtility.ToJson(tempSO, true));
                File.Delete(oldPath);
            }
        }

        string newJson = JsonUtility.ToJson(newLevelData, true);
        string newFilePath = Path.Combine(dir, $"Level_{levelIndex:D2}.json");
        File.WriteAllText(newFilePath, newJson);
        Debug.Log($"<color=green>Đã chèn level mới JSON thành công tại {newFilePath}.</color>");
#endif
    }

    private Texture2D solidBg;

    // --- AUTO PLAY SOLVER ---
    private Coroutine autoPlayCoroutine;

    struct SlotState : System.IEquatable<SlotState>
    {
        public byte type; 
        public int questionTopicID;
        public bool isRevealed;
        public int count;
        public int[] topics;
        public bool[] hiddens;

        public void Init()
        {
            topics = new int[4];
            hiddens = new bool[4];
            count = 0;
        }

        public SlotState Clone()
        {
            var s = new SlotState();
            s.type = this.type;
            s.questionTopicID = this.questionTopicID;
            s.isRevealed = this.isRevealed;
            s.count = this.count;
            s.topics = (int[])this.topics.Clone();
            s.hiddens = (bool[])this.hiddens.Clone();
            return s;
        }

        public bool Equals(SlotState other)
        {
            if (type != other.type || questionTopicID != other.questionTopicID || isRevealed != other.isRevealed || count != other.count) return false;
            for (int i = 0; i < count; i++)
            {
                if (topics[i] != other.topics[i] || hiddens[i] != other.hiddens[i]) return false;
            }
            return true;
        }

        public override int GetHashCode()
        {
            int hash = type + (isRevealed ? 100 : 0) + count * 1000 + questionTopicID * 10000;
            for (int i = 0; i < count; i++)
            {
                hash ^= (topics[i] + (hiddens[i] ? 73 : 0)) << (i * 4);
            }
            return hash;
        }

        public bool IsCompleted()
        {
            if (count != 4) return false;
            for (int i = 0; i < 4; i++)
            {
                if (hiddens[i] || topics[i] != topics[0]) return false;
            }
            return true;
        }

        public int GetTopTopic() => count > 0 ? topics[count - 1] : -1;
        public bool IsTopHidden() => count > 0 ? hiddens[count - 1] : false;

        public int GetMoveCount()
        {
            if (count == 0 || IsTopHidden()) return 0;
            int topTopic = GetTopTopic();
            int m = 1;
            for (int i = count - 2; i >= 0; i--)
            {
                if (!hiddens[i] && topics[i] == topTopic) m++;
                else break;
            }
            return m;
        }
    }

    struct GameState : System.IEquatable<GameState>
    {
        public SlotState[] slots;

        public bool Equals(GameState other)
        {
            for (int i = 0; i < slots.Length; i++)
            {
                if (!slots[i].Equals(other.slots[i])) return false;
            }
            return true;
        }

        public override int GetHashCode()
        {
            int hash = 17;
            for(int i = 0; i < slots.Length; i++)
                hash = hash * 31 + slots[i].GetHashCode();
            return hash;
        }

        public bool IsWin()
        {
            for (int i = 0; i < slots.Length; i++)
            {
                if (slots[i].count > 0 && !slots[i].IsCompleted()) return false;
            }
            return true;
        }
    }

    class Node
    {
        public GameState state;
        public Node parent;
        public int fromSlot;
        public int toSlot;
        public int g;
        public int h;
        public int f => g + h;
    }

    private GameState CreateInitialState()
    {
        LevelDataSO data = GenerateLevelDataSO();
        GameState state = new GameState { slots = new SlotState[data.slots.Count] };
        for (int i = 0; i < data.slots.Count; i++)
        {
            var sd = data.slots[i];
            state.slots[i].Init();
            state.slots[i].type = (byte)sd.slotType;
            state.slots[i].questionTopicID = sd.questionTopic != null ? sd.questionTopic.topicID : -1;
            state.slots[i].isRevealed = (sd.slotType != SlotController.SlotType.Hide);

            for (int j = sd.blocks.Count - 1; j >= 0; j--)
            {
                var bd = sd.blocks[j];
                state.slots[i].topics[state.slots[i].count] = bd.blockTopic.topicID;
                state.slots[i].hiddens[state.slots[i].count] = (bd.typeBlock == BlockController.BlockType.Hide);
                state.slots[i].count++;
            }
            
            if (state.slots[i].count > 0)
            {
                state.slots[i].hiddens[state.slots[i].count - 1] = false;
            }
        }
        return state;
    }

    private GameState CloneState(GameState s)
    {
        var next = new GameState { slots = new SlotState[s.slots.Length] };
        for (int i = 0; i < s.slots.Length; i++)
        {
            next.slots[i] = s.slots[i].Clone();
        }
        return next;
    }

    private List<Node> GenerateSuccessors(Node node)
    {
        var list = new List<Node>();
        GameState state = node.state;
        int numSlots = state.slots.Length;

        for (int i = 0; i < numSlots; i++)
        {
            if (!state.slots[i].isRevealed) continue;
            if (state.slots[i].type == 2) continue; 
            if (state.slots[i].IsCompleted()) continue;

            int moveCount = state.slots[i].GetMoveCount();
            if (moveCount == 0) continue;

            int moveTopic = state.slots[i].GetTopTopic();

            for (int j = 0; j < numSlots; j++)
            {
                if (i == j) continue;
                if (!state.slots[j].isRevealed) continue;
                if (state.slots[j].count == 4) continue; 

                if (state.slots[j].count > 0 && state.slots[j].GetTopTopic() != moveTopic) continue;
                if (state.slots[j].count == 0 && moveCount == state.slots[i].count && !state.slots[i].hiddens[0]) continue;

                int amountToMove = Mathf.Min(4 - state.slots[j].count, moveCount);
                if (amountToMove <= 0) continue;

                GameState nextState = CloneState(state);
                bool wasCompleted = state.slots[j].IsCompleted();

                for (int m = 0; m < amountToMove; m++)
                {
                    nextState.slots[j].topics[nextState.slots[j].count] = nextState.slots[i].topics[nextState.slots[i].count - 1];
                    nextState.slots[j].hiddens[nextState.slots[j].count] = false; 
                    nextState.slots[j].count++;
                    nextState.slots[i].count--;
                }

                if (nextState.slots[i].count > 0)
                {
                    nextState.slots[i].hiddens[nextState.slots[i].count - 1] = false;
                }

                if (!wasCompleted && nextState.slots[j].IsCompleted())
                {
                    int reqTopic = nextState.slots[j].topics[0];
                    for (int k = 0; k < nextState.slots.Length; k++)
                    {
                        if (nextState.slots[k].type == 1 && !nextState.slots[k].isRevealed && nextState.slots[k].questionTopicID == reqTopic)
                        {
                            nextState.slots[k].isRevealed = true;
                            break; 
                        }
                    }
                }

                list.Add(new Node
                {
                    state = nextState,
                    parent = node,
                    fromSlot = i,
                    toSlot = j,
                    g = node.g + 1,
                    h = 0
                });
            }
        }
        return list;
    }

    public void StartAutoPlay()
    {
        if (autoPlayCoroutine != null) StopCoroutine(autoPlayCoroutine);
        autoPlayCoroutine = StartCoroutine(AutoPlayRoutine());
    }

    private System.Collections.IEnumerator AutoPlayRoutine()
    {
        GameState initial = CreateInitialState();
        if (initial.IsWin())
        {
            Debug.Log("Level is already won.");
            yield break;
        }

        var openList = new Queue<Node>();
        var closedSet = new HashSet<GameState>();

        var startNode = new Node { state = initial, parent = null, fromSlot = -1, toSlot = -1, g = 0, h = 0 };
        openList.Enqueue(startNode);
        closedSet.Add(initial);

        int expanded = 0;
        int maxSearchNodes = 100000;
        Node winNode = null;

        Debug.Log("AutoPlay: Solving...");

        while (openList.Count > 0 && expanded < maxSearchNodes)
        {
            Node curr = openList.Dequeue();
            expanded++;

            if (curr.state.IsWin())
            {
                winNode = curr;
                break;
            }

            foreach (var succ in GenerateSuccessors(curr))
            {
                if (!closedSet.Contains(succ.state))
                {
                    closedSet.Add(succ.state);
                    openList.Enqueue(succ);
                }
            }

            // Yield per 5000 nodes to prevent freezing main thread
            if (expanded % 5000 == 0) yield return null;
        }

        if (winNode == null)
        {
            if (openList.Count == 0)
            {
                Debug.LogError($"<color=red>AutoPlay: LEVEL NÀY KHÔNG THỂ GIẢI ĐƯỢC (Unsolvable)! Đã thử toàn bộ {expanded} trường hợp.</color>");
            }
            else
            {
                Debug.LogError($"<color=orange>AutoPlay: Thuật toán dừng sau khi đạt giới hạn {expanded} nodes. Có thể level quá phức tạp hoặc cấu hình sai.</color>");
            }
            yield break;
        }

        var path = new List<Node>();
        Node n = winNode;
        while (n != null && n.parent != null)
        {
            path.Add(n);
            n = n.parent;
        }
        path.Reverse();

        int minSteps = path.Count; 
        Debug.Log($"<color=cyan><b>MỨC ĐỘ TỐI ƯU NHẤT: {minSteps} BƯỚC ĐI (MINIMUM STEPS)</b></color>");
        Debug.Log($"AutoPlay: Bắt đầu tự động chơi {minSteps} bước...");

        Playtest();
        yield return new WaitForSeconds(1.5f); // Wait for level load and animations

        CoreServices.Get<InputManager>().SetInputBlocked(true);

        foreach (Node step in path)
        {
            if (CoreServices.Get<GameManager>().GetCurrState() != GameManager.GameState.Playing) break;

            List<SlotController> runtimeSlots = CoreServices.Get<SlotsManager>().GetAllSlots();
            SlotController source = runtimeSlots[step.fromSlot];
            SlotController target = runtimeSlots[step.toSlot];

            if(source.SelectToMove())
            {
                yield return new WaitForSeconds(0.2f);
                if (target.SelectToRecive(source))
                {
                    yield return new WaitForSeconds(0.6f); // delay per move to observe
                }
                else 
                {
                    Debug.LogError("AutoPlay: Failed to move blocks!");
                    source.UnSelect();
                    break;
                }
            }
            else
            {
                Debug.LogError("AutoPlay: Failed to select source slot!");
                break;
            }
        }
        CoreServices.Get<InputManager>().SetInputBlocked(false);
    }

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

    public void RandomGenerateBlocks()
    {
        SaveUndoState();
        InitManagers();

        int numSlots = row1 + row2;
        int numTopics = numSlots - 2;
        if (numTopics <= 0) return;

        // Reset grid
        GenerateGrid();
        topics.Clear();
        amountBlockOfTopic.Clear();
        currentPaintbrushTopic = null;
        currentPaintbrushIndex = -1;

        if (availableTopics == null || availableTopics.Length == 0) return;

        List<BlockTopic> poolTopics = new List<BlockTopic>(availableTopics);
        for (int i = 0; i < poolTopics.Count; i++)
        {
            BlockTopic temp = poolTopics[i];
            int randomIndex = Random.Range(i, poolTopics.Count);
            poolTopics[i] = poolTopics[randomIndex];
            poolTopics[randomIndex] = temp;
        }

        for (int i = 0; i < Mathf.Min(numTopics, poolTopics.Count); i++)
        {
            topics.Add(poolTopics[i]);
            amountBlockOfTopic.Add(4);
        }

        int maxRetries = 50; // Tăng lên để có nhiều cơ hội tìm thấy level Foolproof hơn
        bool foundSolvable = false;

        for (int retry = 0; retry < maxRetries; retry++)
        {
            List<BlockSetupData> blockPool = new List<BlockSetupData>();
            for (int i = 0; i < topics.Count; i++)
            {
                for (int j = 0; j < 4; j++)
                {
                    BlockSetupData b = new BlockSetupData();
                    b.blockTopic = topics[i];
                    b.typeBlock = BlockController.BlockType.Normal;
                    b.indexSprite = j;
                    blockPool.Add(b);
                }
            }

            // Shuffle pool
            for (int i = 0; i < blockPool.Count; i++)
            {
                BlockSetupData temp = blockPool[i];
                int randomIndex = Random.Range(i, blockPool.Count);
                blockPool[i] = blockPool[randomIndex];
                blockPool[randomIndex] = temp;
            }

            if (difficulty == 0) // Dễ
            {
                blockPool.Sort((a, b) => 
                {
                    if (Random.value < 0.7f) return a.blockTopic.topicID.CompareTo(b.blockTopic.topicID);
                    return Random.Range(-1, 2);
                });
            }
            else if (difficulty == 2) // Siêu khó
            {
                for (int i = 0; i < blockPool.Count - 1; i++)
                {
                    if (blockPool[i].blockTopic.topicID == blockPool[i+1].blockTopic.topicID)
                    {
                        for (int j = i + 2; j < blockPool.Count; j++)
                        {
                            if (blockPool[j].blockTopic.topicID != blockPool[i].blockTopic.topicID)
                            {
                                BlockSetupData temp = blockPool[i+1];
                                blockPool[i+1] = blockPool[j];
                                blockPool[j] = temp;
                                break;
                            }
                        }
                    }
                }
            }

            // Phân bổ sao cho KHÔNG có slot rỗng (Mỗi slot nhận 1 block trước)
            for (int i = 0; i < numSlots; i++)
            {
                slots[i].blocks.Clear();
                if (blockPool.Count > 0)
                {
                    slots[i].blocks.Add(blockPool[0]);
                    blockPool.RemoveAt(0);
                }
            }

            // Phân bổ ngẫu nhiên phần còn lại (tối đa 4 block mỗi slot)
            while (blockPool.Count > 0)
            {
                int rSlot = Random.Range(0, numSlots);
                if (slots[rSlot].blocks.Count < 4)
                {
                    slots[rSlot].blocks.Insert(0, blockPool[0]);
                    blockPool.RemoveAt(0);
                }
            }

            // Kiểm tra khả năng giải và tính Foolproof
            if (IsLevelFoolproof(CreateInitialState()))
            {
                foundSolvable = true;
                Debug.Log($"<color=green>Đã tạo level HOÀN HẢO (Foolproof) thành công sau {retry + 1} lần thử nghiệm.</color>");
                break;
            }
        }

        if (!foundSolvable)
        {
            Debug.LogWarning("Không tìm được cấu hình HOÀN HẢO sau 50 lần thử! Có thể level quá khó hoặc ít cột rỗng. Sẽ giữ cấu hình ngẫu nhiên cuối cùng.");
        }

        RenderGrid();
    }

    private bool CheckSolvable(GameState initial)
    {
        if (initial.IsWin()) return true;

        var openList = new Stack<GameState>();
        var closedSet = new HashSet<GameState>();

        openList.Push(initial);
        closedSet.Add(initial);

        int expanded = 0;
        int maxSearchNodes = 10000;

        while (openList.Count > 0 && expanded < maxSearchNodes)
        {
            GameState curr = openList.Pop();
            expanded++;

            if (curr.IsWin()) return true;

            int numSlots = curr.slots.Length;
            for (int i = 0; i < numSlots; i++)
            {
                if (!curr.slots[i].isRevealed || curr.slots[i].type == 2 || curr.slots[i].IsCompleted()) continue;
                int moveCount = curr.slots[i].GetMoveCount();
                if (moveCount == 0) continue;
                int moveTopic = curr.slots[i].GetTopTopic();

                for (int j = 0; j < numSlots; j++)
                {
                    if (i == j || !curr.slots[j].isRevealed || curr.slots[j].count == 4) continue;
                    if (curr.slots[j].count > 0 && curr.slots[j].GetTopTopic() != moveTopic) continue;
                    if (curr.slots[j].count == 0 && moveCount == curr.slots[i].count && !curr.slots[i].hiddens[0]) continue;

                    int amountToMove = Mathf.Min(4 - curr.slots[j].count, moveCount);
                    if (amountToMove <= 0) continue;

                    GameState nextState = CloneState(curr);
                    for (int m = 0; m < amountToMove; m++)
                    {
                        nextState.slots[j].topics[nextState.slots[j].count] = nextState.slots[i].topics[nextState.slots[i].count - 1];
                        nextState.slots[j].hiddens[nextState.slots[j].count] = false;
                        nextState.slots[j].count++;
                        nextState.slots[i].count--;
                    }
                    if (nextState.slots[i].count > 0) nextState.slots[i].hiddens[nextState.slots[i].count - 1] = false;

                    if (!closedSet.Contains(nextState))
                    {
                        if (nextState.IsWin()) return true;
                        closedSet.Add(nextState);
                        openList.Push(nextState);
                    }
                }
            }
        }
        return false;
    }

    private bool IsLevelFoolproof(GameState initial)
    {
        var reachable = new HashSet<GameState>();
        var queue = new Queue<GameState>();
        var adj = new Dictionary<GameState, List<GameState>>();
        
        reachable.Add(initial);
        queue.Enqueue(initial);
        
        int expanded = 0;
        int maxSearchNodes = 10000; // Giới hạn 10000 để Random không bị đứng máy quá lâu
        
        while (queue.Count > 0 && expanded < maxSearchNodes)
        {
            GameState curr = queue.Dequeue();
            expanded++;
            
            var dummy = new Node { state = curr, g = 0, h = 0 };
            var succNodes = GenerateSuccessors(dummy);
            
            List<GameState> nextStates = new List<GameState>();
            foreach (var node in succNodes) nextStates.Add(node.state);

            adj[curr] = nextStates;
            
            foreach (var n in nextStates)
            {
                if (!reachable.Contains(n))
                {
                    reachable.Add(n);
                    queue.Enqueue(n);
                }
            }
        }

        // Nếu level quá phức tạp để check nhanh, ta có thể bỏ qua (hoặc bạn có thể cho return false để khắt khe hơn)
        if (expanded >= maxSearchNodes) return false; 

        var canReachWin = new HashSet<GameState>();
        var revQueue = new Queue<GameState>();
        var revAdj = new Dictionary<GameState, List<GameState>>();
        
        foreach (var s in reachable) revAdj[s] = new List<GameState>();
        foreach (var kvp in adj)
        {
            foreach (var n in kvp.Value) revAdj[n].Add(kvp.Key);
        }

        foreach (var s in reachable)
        {
            if (s.IsWin())
            {
                canReachWin.Add(s);
                revQueue.Enqueue(s);
            }
        }

        while (revQueue.Count > 0)
        {
            GameState curr = revQueue.Dequeue();
            foreach (var prev in revAdj[curr])
            {
                if (!canReachWin.Contains(prev))
                {
                    canReachWin.Add(prev);
                    revQueue.Enqueue(prev);
                }
            }
        }

        return reachable.Count == canReachWin.Count;
    }

    public void CheckFoolproof()
    {
        Debug.Log("Đang phân tích toàn bộ các nước đi có thể (Foolproof Check)...");
        GameState initial = CreateInitialState();
        
        var reachable = new HashSet<GameState>();
        var queue = new Queue<GameState>();
        var adj = new Dictionary<GameState, List<GameState>>();
        
        reachable.Add(initial);
        queue.Enqueue(initial);
        
        int expanded = 0;
        int maxSearchNodes = 50000;
        
        while (queue.Count > 0 && expanded < maxSearchNodes)
        {
            GameState curr = queue.Dequeue();
            expanded++;
            
            var dummy = new Node { state = curr, g = 0, h = 0 };
            var succNodes = GenerateSuccessors(dummy);
            
            List<GameState> nextStates = new List<GameState>();
            foreach (var node in succNodes)
            {
                nextStates.Add(node.state);
            }

            adj[curr] = nextStates;
            
            foreach (var n in nextStates)
            {
                if (!reachable.Contains(n))
                {
                    reachable.Add(n);
                    queue.Enqueue(n);
                }
            }
        }

        if (expanded >= maxSearchNodes)
        {
            Debug.LogWarning($"<color=orange>Cảnh báo: Không gian trạng thái quá lớn (>{maxSearchNodes}). Chỉ kiểm tra được 1 phần.</color>");
        }

        var canReachWin = new HashSet<GameState>();
        var revQueue = new Queue<GameState>();

        var revAdj = new Dictionary<GameState, List<GameState>>();
        foreach (var s in reachable) revAdj[s] = new List<GameState>();

        foreach (var kvp in adj)
        {
            foreach (var n in kvp.Value)
            {
                revAdj[n].Add(kvp.Key);
            }
        }

        foreach (var s in reachable)
        {
            if (s.IsWin())
            {
                canReachWin.Add(s);
                revQueue.Enqueue(s);
            }
        }

        while (revQueue.Count > 0)
        {
            GameState curr = revQueue.Dequeue();
            foreach (var prev in revAdj[curr])
            {
                if (!canReachWin.Contains(prev))
                {
                    canReachWin.Add(prev);
                    revQueue.Enqueue(prev);
                }
            }
        }

        int stuckCount = reachable.Count - canReachWin.Count;
        if (stuckCount == 0)
        {
            Debug.Log($"<color=cyan>HOÀN HẢO! Đã duyệt {reachable.Count} trạng thái. Dù đi như thế nào cũng CÓ THỂ giải được (Foolproof)!</color>");
        }
        else
        {
            Debug.LogError($"<color=red>CẢNH BÁO! Có {stuckCount} trạng thái mà người chơi sẽ BỊ KẸT (không thể thắng) nếu đi sai!</color>");
        }
    }

    private void DrawEditorWindow(int windowID)
    {
        scrollPos = GUILayout.BeginScrollView(scrollPos);

        GUILayout.Label("--- Cấu Hình ---", GUI.skin.box);
        GUILayout.BeginHorizontal();
        GUILayout.Label("Level ID:");
        string lvlStr = GUILayout.TextField(levelIndex.ToString());
        if (int.TryParse(lvlStr, out int l)) levelIndex = l;
        
        if (GUILayout.Button("▼", GUILayout.Width(30)))
        {
            showLevelDropdown = !showLevelDropdown;
            if (showLevelDropdown) RefreshAvailableLevels();
        }
        GUILayout.EndHorizontal();

        if (showLevelDropdown && availableLevels != null)
        {
            levelScrollPos = GUILayout.BeginScrollView(levelScrollPos, GUI.skin.box, GUILayout.Height(150));
            for (int i = 0; i < availableLevels.Length; i++)
            {
                if (GUILayout.Button($"Level {availableLevels[i].level}", GUILayout.Height(25)))
                {
                    levelIndex = availableLevels[i].level;
                    showLevelDropdown = false;
                    LoadLevelEditor();
                }
            }
            GUILayout.EndScrollView();
            GUILayout.Space(5);
        }

        GUILayout.BeginHorizontal();
        GUILayout.Label("Moves:");
        string movesStr = GUILayout.TextField(moves.ToString());
        if (int.TryParse(movesStr, out int m)) moves = m;
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Độ khó:", GUILayout.Width(60));
        if (GUILayout.Toggle(difficulty == 0, "Dễ", "Button")) difficulty = 0;
        if (GUILayout.Toggle(difficulty == 1, "Khó", "Button")) difficulty = 1;
        if (GUILayout.Toggle(difficulty == 2, "Siêu Khó", "Button")) difficulty = 2;
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
        GUILayout.Label("--- CHẾ ĐỘ CLICK ---", GUI.skin.box);
        GUILayout.BeginHorizontal();
        if (GUILayout.Toggle(currentEditMode == EditActionMode.Block, "Sửa Block", "Button")) currentEditMode = EditActionMode.Block;
        if (GUILayout.Toggle(currentEditMode == EditActionMode.Slot, "Sửa Cột (Slot)", "Button")) currentEditMode = EditActionMode.Slot;
        GUILayout.EndHorizontal();

        GUILayout.Space(10);
        GUILayout.Label("--- CƠ CHẾ (Mechanics) ---", GUI.skin.box);
        
        if (currentEditMode == EditActionMode.Slot)
        {
            GUILayout.Label("Loại Cột (Slot):");
            GUILayout.BeginHorizontal();
            if (GUILayout.Toggle(currentSlotMechanic == SlotController.SlotType.Normal, "Bình Thường (Hủy)", "Button")) currentSlotMechanic = SlotController.SlotType.Normal;
            if (GUILayout.Toggle(currentSlotMechanic == SlotController.SlotType.Hide, "Ẩn (Hide)", "Button")) currentSlotMechanic = SlotController.SlotType.Hide;
            if (GUILayout.Toggle(currentSlotMechanic == SlotController.SlotType.Ice, "Băng (Ice)", "Button")) currentSlotMechanic = SlotController.SlotType.Ice;
            GUILayout.EndHorizontal();

            if (currentSlotMechanic == SlotController.SlotType.Hide)
            {
                GUILayout.Space(5);
                GUILayout.Label("Topic Mở Khóa Cột Ẩn:");
                if (topics.Count == 0) GUILayout.Label("(Vui lòng chọn Topics trong level trước)");
                foreach (var t in topics)
                {
                    if (GUILayout.Toggle(currentHiddenSlotTopic == t, t.topicName, "Button"))
                    {
                        currentHiddenSlotTopic = t;
                    }
                }
            }
        }
        else 
        {
            GUILayout.Label("Loại Block:");
            GUILayout.BeginHorizontal();
            if (GUILayout.Toggle(currentBlockMechanic == BlockController.BlockType.Normal, "Bình Thường (Hủy)", "Button")) currentBlockMechanic = BlockController.BlockType.Normal;
            if (GUILayout.Toggle(currentBlockMechanic == BlockController.BlockType.Hide, "Ẩn (Hide)", "Button")) currentBlockMechanic = BlockController.BlockType.Hide;
            GUILayout.EndHorizontal();
        }

        GUILayout.Space(10);
        GUILayout.Label("--- Topics Level & Cọ Vẽ Block ---", GUI.skin.box);
        
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
        if (GUILayout.Button("Bỏ chọn Cọ (Chế độ Đổi Mechanic Block)"))
        {
            currentPaintbrushTopic = null;
            currentPaintbrushIndex = -1;
        }

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
        if (GUILayout.Button("Random Level (Ngẫu nhiên)", GUILayout.Height(40)))
        {
            RandomGenerateBlocks();
        }

        GUILayout.Space(10);
        if (GUILayout.Button("2. Playtest (Thử Nghiệm)", GUILayout.Height(40)))
        {
            Playtest();
        }

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("3. Lưu Đè (Save)", GUILayout.Height(40)))
        {
            SaveLevel();
        }
        if (GUILayout.Button("Chèn (Insert)", GUILayout.Height(40)))
        {
            InsertLevel();
        }
        GUILayout.EndHorizontal();
        
        if (GUILayout.Button("4. Load Level Data", GUILayout.Height(40)))
        {
            LoadLevelEditor();
        }

        if (GUILayout.Button("5. Auto Play (Tự giải)", GUILayout.Height(40)))
        {
            StartAutoPlay();
        }

        if (GUILayout.Button("6. Kiểm tra Bị Kẹt (Foolproof)", GUILayout.Height(40)))
        {
            CheckFoolproof();
        }

        GUILayout.EndScrollView();
        GUI.DragWindow();
    }
}
