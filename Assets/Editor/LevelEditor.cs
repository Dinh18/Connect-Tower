#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

[CustomEditor(typeof(MakeLevel))]
public class LevelEditor : Editor
{
    private BlockTopic[] allAvailableTopics = null;

    private void OnEnable()
    {
        // Pre-load all available topics inside Assets/Resources/Data/topics2/
        allAvailableTopics = Resources.LoadAll<BlockTopic>("Data/topics2");
    }

    public override void OnInspectorGUI()
    {
        GUILayout.Space(10);
        EditorGUILayout.HelpBox("🏆 HỆ THỐNG THIẾT KẾ LEVEL (Connect Tower) 🏆\n\nGiao diện điều khiển Level Editor hiện nay được tích hợp hoàn toàn trong cửa sổ chuyên dụng của Unity, cho phép bạn co dãn tùy thích và neo (dock) linh hoạt.", MessageType.Info);
        GUILayout.Space(10);

        GUI.backgroundColor = new Color(0.2f, 0.62f, 0.95f);
        if (GUILayout.Button("🔧 MỞ CỬA SỔ LEVEL EDITOR WINDOW", GUILayout.Height(45)))
        {
            LevelEditorWindow.ShowWindow();
        }
        
        GUI.backgroundColor = new Color(0.2f, 0.8f, 0.45f);
        if (GUILayout.Button("🔍 DI CHUYỂN TỚI SCENE VIEW", GUILayout.Height(30)))
        {
            var sceneView = SceneView.lastActiveSceneView;
            if (sceneView != null) sceneView.Focus();
            else EditorWindow.FocusWindowIfItsOpen<SceneView>();
        }
        GUI.backgroundColor = Color.white;
        GUILayout.Space(10);
    }

    private void OnSceneGUI()
    {
        MakeLevel makeLevel = (MakeLevel)target;
        if (makeLevel == null) return;

        // Load topics fallback if needed
        if (allAvailableTopics == null || allAvailableTopics.Length == 0)
        {
            allAvailableTopics = Resources.LoadAll<BlockTopic>("Data/topics2");
        }

        // Prevent Unity's default mouse selections in the scene to keep focus on MakeLevel
        int controlID = GUIUtility.GetControlID(FocusType.Passive);
        HandleUtility.AddDefaultControl(controlID);

        // Draw Interactive Overlay on top of Scene view nodes
        Handles.BeginGUI();
        DrawInSceneNodeOverlays(makeLevel);
        Handles.EndGUI();

        // Continuously repaint the scene view to handle snappy visual highlights and paints
        if (GUI.changed)
        {
            EditorUtility.SetDirty(makeLevel);
        }
    }

    private List<SlotController> GetSlotControllersInScene(MakeLevel makeLevel)
    {
        List<SlotController> list = new List<SlotController>();
        for (int i = 0; i < makeLevel.transform.childCount; i++)
        {
            var controller = makeLevel.transform.GetChild(i).GetComponent<SlotController>();
            if (controller != null) list.Add(controller);
        }
        return list;
    }

    private void DrawInSceneNodeOverlays(MakeLevel makeLevel)
    {
        var slotControllers = GetSlotControllersInScene(makeLevel);
        if (slotControllers.Count == 0) return;

        Event e = Event.current;

        for (int i = 0; i < slotControllers.Count; i++)
        {
            var controller = slotControllers[i];
            if (controller == null || i >= makeLevel.slots.Count) continue;

            SlotSetupData slotData = makeLevel.slots[i];
            Vector3 slotWorldPos = controller.transform.position;
            Vector2 slotGUIPos = HandleUtility.WorldToGUIPoint(slotWorldPos);

            // 1. Draw Slot Header (Type modifier button) below the slot base
            Rect headerRect = new Rect(slotGUIPos.x - 52, slotGUIPos.y + 40, 104, 22);

            string typeName = slotData.slotType.ToString().ToUpper();
            Color typeColor = slotData.slotType == SlotController.SlotType.Ice ? new Color(0.3f, 0.7f, 1f) :
                              slotData.slotType == SlotController.SlotType.Hide ? new Color(1f, 0.6f, 0.2f) :
                              new Color(0.85f, 0.85f, 0.85f);

            GUI.backgroundColor = typeColor;
            if (GUI.Button(headerRect, $"S{i:D2}: {typeName}", EditorStyles.miniButton))
            {
                Undo.RecordObject(makeLevel, "Cycle Slot Type");
                int currType = (int)slotData.slotType;
                int nextType = (currType + 1) % 3; // Normal = 0, Hide = 1, Ice = 2
                slotData.slotType = (SlotController.SlotType)nextType;

                makeLevel.UpdateSlotsInEditor();
                makeLevel.GenerateBlocks();
                e.Use();
            }
            GUI.backgroundColor = Color.white;

            // 2. Hide Slot Question Selector Dropdown
            if (slotData.slotType == SlotController.SlotType.Hide)
            {
                Rect questionRect = new Rect(slotGUIPos.x - 52, slotGUIPos.y + 65, 104, 20);
                string questionText = slotData.questionTopic != null ? $"🔓 Q:{slotData.questionTopic.topicID}" : "🔓 Chọn Q";

                if (GUI.Button(questionRect, questionText, EditorStyles.miniButton))
                {
                    GenericMenu menu = new GenericMenu();
                    menu.AddItem(new GUIContent("Không chọn"), slotData.questionTopic == null, () => {
                        Undo.RecordObject(makeLevel, "Clear Question Topic");
                        slotData.questionTopic = null;
                        makeLevel.UpdateSlotsInEditor();
                        makeLevel.GenerateBlocks();
                    });

                    foreach (var topic in makeLevel.topics)
                    {
                        if (topic == null) continue;
                        bool isCurrent = (slotData.questionTopic != null && slotData.questionTopic.topicID == topic.topicID);
                        menu.AddItem(new GUIContent($"Topic {topic.topicID} - {topic.topicName}"), isCurrent, () => {
                            Undo.RecordObject(makeLevel, "Assign Question Topic");
                            slotData.questionTopic = topic;
                            makeLevel.UpdateSlotsInEditor();
                            makeLevel.GenerateBlocks();
                        });
                    }
                    menu.ShowAsContext();
                    e.Use();
                }
            }

            // 3. Add (+) / Remove (-) Block overlay buttons positioned right above the stack height
            int blockCount = slotData.blocks.Count;
            Vector3 blockAddWorldPos = slotWorldPos + Vector3.up * (Constants.BLOCK_HEIGHT * (blockCount + 0.5f) + 0.25f);
            Vector2 blockAddGUIPos = HandleUtility.WorldToGUIPoint(blockAddWorldPos);

            Rect plusRect = new Rect(blockAddGUIPos.x - 28, blockAddGUIPos.y - 11, 24, 22);
            Rect minusRect = new Rect(blockAddGUIPos.x + 4, blockAddGUIPos.y - 11, 24, 22);

            GUI.backgroundColor = new Color(0.2f, 0.8f, 0.35f);
            if (GUI.Button(plusRect, "+", GUI.skin.button))
            {
                Undo.RecordObject(makeLevel, "Add Block to Slot Stack");
                BlockTopic paintTopic = (LevelEditorWindow.PaintbrushTopicStatic != null) ? LevelEditorWindow.PaintbrushTopicStatic :
                                       (makeLevel.topics.Count > 0) ? makeLevel.topics[0] : null;

                if (paintTopic != null)
                {
                    int topicIndex = makeLevel.topics.FindIndex(t => t.topicID == paintTopic.topicID);
                    if (topicIndex >= 0) makeLevel.indexTopicSelected = topicIndex;

                    makeLevel.AddBlockToSlot(i, paintTopic, 0); // Default Normal Block type
                    makeLevel.UpdateSlotsInEditor();
                    makeLevel.GenerateBlocks();
                }
                else
                {
                    EditorUtility.DisplayDialog("Cảnh Báo", "Vui lòng chọn hoặc cấu hình ít nhất 1 Topic khả dụng ở bước Cấu Hình trước!", "OK");
                }
                e.Use();
            }

            GUI.backgroundColor = new Color(0.9f, 0.3f, 0.25f);
            if (GUI.Button(minusRect, "-", GUI.skin.button))
            {
                Undo.RecordObject(makeLevel, "Remove Block from Slot Stack");
                makeLevel.RemoveBlockFromSlot(i);
                makeLevel.UpdateSlotsInEditor();
                makeLevel.GenerateBlocks();
                e.Use();
            }
            GUI.backgroundColor = Color.white;

            // 4. Stack of interactive clickable blocks
            for (int k = 0; k < blockCount; k++)
            {
                int stackMultiplier = blockCount - 1 - k;
                Vector3 blockWorldPos = controller.stackAnchor.position + Vector3.up * (Constants.BLOCK_HEIGHT * stackMultiplier);
                Vector2 blockGUIPos = HandleUtility.WorldToGUIPoint(blockWorldPos);

                Rect blockRect = new Rect(blockGUIPos.x - 38, blockGUIPos.y - 11, 76, 22);
                BlockSetupData blockSetup = slotData.blocks[k];
                string blockName = blockSetup.blockTopic != null ? $"T:{blockSetup.blockTopic.topicID}" : "T:?";

                if (blockSetup.typeBlock == BlockController.BlockType.Hide)
                {
                    blockName += " [Ẩn]";
                }

                // Choose a distinct color based on topic id
                Color blockCol = Color.white;
                if (blockSetup.blockTopic != null)
                {
                    int colIndex = blockSetup.blockTopic.topicID % 5;
                    blockCol = colIndex == 0 ? new Color(0.9f, 0.45f, 0.45f) :
                               colIndex == 1 ? new Color(0.45f, 0.75f, 0.95f) :
                               colIndex == 2 ? new Color(0.45f, 0.9f, 0.55f) :
                               colIndex == 3 ? new Color(0.95f, 0.85f, 0.45f) :
                               new Color(0.85f, 0.5f, 0.95f);
                }

                if (blockSetup.typeBlock == BlockController.BlockType.Hide)
                {
                    blockCol = Color.Lerp(blockCol, Color.black, 0.38f);
                }

                GUI.backgroundColor = blockCol;

                string blockTooltip = blockSetup.blockTopic != null ? blockSetup.blockTopic.topicName : "Trống";

                if (GUI.Button(blockRect, new GUIContent(blockName, blockTooltip), GUI.skin.button))
                {
                    Undo.RecordObject(makeLevel, "Interact Scene Block");

                    if (LevelEditorWindow.ActiveTabStatic == 2)
                    {
                        // Mechanics mode active: Click to toggle block mechanic type
                        blockSetup.typeBlock = blockSetup.typeBlock == BlockController.BlockType.Normal ?
                                               BlockController.BlockType.Hide : BlockController.BlockType.Normal;
                    }
                    else
                    {
                        // Topic Paintbrush mode: paint topic
                        if (LevelEditorWindow.PaintbrushTopicStatic != null)
                        {
                            blockSetup.blockTopic = LevelEditorWindow.PaintbrushTopicStatic;
                        }
                        else if (makeLevel.topics.Count > 0)
                        {
                            // Cycle topic on click if no paintbrush selected
                            int currentIdx = makeLevel.topics.FindIndex(t => t.topicID == blockSetup.blockTopic?.topicID);
                            int nextIdx = (currentIdx + 1) % makeLevel.topics.Count;
                            blockSetup.blockTopic = makeLevel.topics[nextIdx];
                        }
                    }

                    makeLevel.UpdateSlotsInEditor();
                    makeLevel.GenerateBlocks();
                    e.Use();
                }
                GUI.backgroundColor = Color.white;
            }
        }
    }
}
#endif
