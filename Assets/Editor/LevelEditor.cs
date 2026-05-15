#if UNITY_EDITOR
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using System;
[CustomEditor(typeof(MakeLevel))]
public class LevelEditor : Editor
{
    // Setting block
    int typeBlock = 0; // 0: Normal, 1: Hide
    bool enableSceneEdit = false;
    LevelDataSO levelDataToLoad;

    public override void OnInspectorGUI()
    {
        MakeLevel makeLevel = (MakeLevel)target;
        serializedObject.Update();
        SerializedProperty prop = serializedObject.GetIterator();
        Undo.RecordObject(makeLevel, "Change Level Settings");

        EditorGUILayout.HelpBox("LEVEL SETTINGS", MessageType.Info);
        makeLevel.Level = EditorGUILayout.IntField("Level", makeLevel.Level);
        makeLevel.moves = EditorGUILayout.IntField("Moves", makeLevel.moves);
        
        GUILayout.Space(10);
        EditorGUILayout.HelpBox("LOAD LEVEL", MessageType.Info);
        levelDataToLoad = (LevelDataSO)EditorGUILayout.ObjectField("Level Data", levelDataToLoad, typeof(LevelDataSO), false);
        if (GUILayout.Button("Load Level Data", GUILayout.Height(30))) {
            if (levelDataToLoad != null) {
                Undo.RecordObject(makeLevel, "Load Level");
                // makeLevel.LoadLevel(levelDataToLoad);
            } else {
                Debug.LogWarning("Please assign a LevelDataSO to load.");
            }
        }

        GUILayout.Space(10);
        EditorGUILayout.HelpBox("GRID SETTINGS", MessageType.Info);
        makeLevel.row1 = EditorGUILayout.IntSlider("Row 1 Slots", makeLevel.row1, 0, 10);
        makeLevel.row2 = EditorGUILayout.IntSlider("Row 2 Slots", makeLevel.row2, 0, 10);
        
        if (GUILayout.Button("Update Grid Layout", GUILayout.Height(30))) {
            makeLevel.SettingSlots();
        }

        GUILayout.Space(10);
        EditorGUILayout.HelpBox("TOPICS SETTINGS", MessageType.Info);
        makeLevel.totalTopics = EditorGUILayout.IntSlider("Total Topics", makeLevel.totalTopics, 1, 20);
        DrawTopicListInSpector(makeLevel);

        GUILayout.Space(10);
        EditorGUILayout.HelpBox("BLOCK EDITING", MessageType.Info);
        enableSceneEdit = EditorGUILayout.Toggle("Enable Scene Editing", enableSceneEdit);
        if (enableSceneEdit) {
            EditorGUILayout.HelpBox("Scene Edit Active: Left click slot to Add block. Right click slot to Remove.", MessageType.Warning);
        }

        GUILayout.BeginHorizontal();
        typeBlock = GUILayout.Toggle(typeBlock == 0, "Normal Block", EditorStyles.radioButton) ? 0 : typeBlock;
        typeBlock = GUILayout.Toggle(typeBlock == 1, "Hide Block", EditorStyles.radioButton) ? 1 : typeBlock;
        GUILayout.EndHorizontal();

        DrawSlotsListInSpector(makeLevel);

        GUILayout.Space(10);
        EditorGUILayout.HelpBox("ACTIONS", MessageType.Info);
        
        if (GUILayout.Button("Auto-Fill Random Topics", GUILayout.Height(30))) {
            makeLevel.AutoFillTopics();
        }

        if(GUILayout.Button("Preview Level in Scene", GUILayout.Height(30))) {
            makeLevel.UpdateSlotsInEditor();
            makeLevel.GenerateBlocks();
        }

        if(GUILayout.Button("Reset Everything", GUILayout.Height(30))) {
            makeLevel.Reset();
        }

        GUI.backgroundColor = Color.green;
        if(GUILayout.Button("Save Level Data", GUILayout.Height(40))) {
            makeLevel.SaveLevelData();
        }
        GUI.backgroundColor = Color.white;

        serializedObject.ApplyModifiedProperties();
    }
    private void DrawTopicListInSpector(MakeLevel makeLevel)
    {
        GUILayout.Space(20);
        GUILayout.Label("Topics List", EditorStyles.boldLabel);
        GUILayout.BeginHorizontal();
        for(int i = 0; i < makeLevel.topics.Count; i++)
        {
            if(i == makeLevel.indexTopicSelected)
            {
                GUI.backgroundColor = Color.green;
            }
            else
            {
                GUI.backgroundColor = Color.white;
            }
            if(GUILayout.Button($"Topic {makeLevel.topics[i].topicID}\n{makeLevel.amountBlockOfTopic[i]}/4", GUILayout.Width(60), GUILayout.Height(60)))
            {
                makeLevel.indexTopicSelected = i;
            }
        }
        GUI.backgroundColor = Color.white;
        GUILayout.EndHorizontal();
    }
    private void DrawSlotsListInSpector(MakeLevel makeLevel)
    {
        int totalBlocks = 0;
        foreach (var slot in makeLevel.slots) totalBlocks += slot.blocks.Count;
        if (totalBlocks % 4 != 0) {
            EditorGUILayout.HelpBox($"Total Blocks: {totalBlocks} (NOT a multiple of 4!). Fix before saving.", MessageType.Error);
        } else {
            EditorGUILayout.HelpBox($"Total Blocks: {totalBlocks} (Valid).", MessageType.Info);
        }
        
        GUILayout.Space(10);
        GUILayout.Label("Slots Editor (Fallback if Scene Edit is off)", EditorStyles.boldLabel);

        int index = 0;     

        GUILayout.BeginHorizontal();
        for(int i = 0; i < makeLevel.row1; i++)
        {
            if (index >= makeLevel.slots.Count) break;
            DrawSingleSlot(makeLevel, index);
            index++;
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        for(int i = 0; i < makeLevel.row2; i++)
        {
            if (index >= makeLevel.slots.Count) break;
            DrawSingleSlot(makeLevel, index);
            index++;
        }
        GUILayout.EndHorizontal();
    }

    private void DrawSingleSlot(MakeLevel makeLevel, int index)
    {
        GUILayout.BeginVertical("box"); 
        makeLevel.slots[index].slotType = (SlotController.SlotType)EditorGUILayout.EnumPopup(makeLevel.slots[index].slotType, GUILayout.Width(65));
        
        string btnText = $"Slot {index}\n[{makeLevel.slots[index].blocks.Count}/4]";
        Rect rect = GUILayoutUtility.GetRect(new GUIContent(btnText), GUI.skin.button, GUILayout.Width(60), GUILayout.Height(60));

        if (Event.current.type == EventType.MouseDown && Event.current.button == 1 && rect.Contains(Event.current.mousePosition))
        {
            makeLevel.RemoveBlockFromSlot(index);
            Event.current.Use(); 
        }
        if(GUI.Button(rect, btnText))
        {
            if (Event.current.button == 0) 
            {
                if (makeLevel.topics.Count > 0)
                    makeLevel.AddBlockToSlot(index, makeLevel.topics[makeLevel.indexTopicSelected], typeBlock);
            }
        }
        
        if(makeLevel.slots[index].slotType == SlotController.SlotType.Hide)
        {
            string btnHideText = "Q Topic " + (makeLevel.slots[index].questionTopic != null ? makeLevel.slots[index].questionTopic.topicID.ToString() : "None");
            if(GUILayout.Button(btnHideText, GUILayout.Width(65), GUILayout.Height(30)))
            {
                makeLevel.slots[index].questionTopic = makeLevel.topics[makeLevel.indexTopicSelected];
            }
        }
        GUILayout.EndVertical();
    }

    private void OnSceneGUI()
    {
        if (!enableSceneEdit) return;

        MakeLevel makeLevel = (MakeLevel)target;
        if (makeLevel.slots == null || makeLevel.slots.Count == 0) return;

        Event e = Event.current;
        int controlID = GUIUtility.GetControlID(FocusType.Passive);
        
        int index = 0;
        for (int i = 0; i < makeLevel.row1; i++)
        {
            if (index >= makeLevel.slots.Count) break;
            Vector3 pos = makeLevel.transform.position + makeLevel.slots[index].position;
            DrawSceneSlot(makeLevel, index, pos, e);
            index++;
        }

        for (int i = 0; i < makeLevel.row2; i++)
        {
            if (index >= makeLevel.slots.Count) break;
            Vector3 pos = makeLevel.transform.position + makeLevel.slots[index].position;
            DrawSceneSlot(makeLevel, index, pos, e);
            index++;
        }

        if (e.type == EventType.MouseDown)
        {
            GUIUtility.hotControl = controlID;
        }
        else if (e.type == EventType.MouseUp)
        {
            GUIUtility.hotControl = 0;
        }
    }

    private void DrawSceneSlot(MakeLevel makeLevel, int index, Vector3 pos, Event e)
    {
        int blockCount = makeLevel.slots[index].blocks.Count;
        
        Handles.color = new Color(0.2f, 0.2f, 0.2f, 0.3f);
        Handles.DrawSolidDisc(pos, Vector3.forward, Constants.SLOT_WIDTH / 2f);

        for (int b = 0; b < blockCount; b++)
        {
            Vector3 blockPos = pos + new Vector3(0, Constants.BLOCK_HEIGHT * b, -b * 0.1f);
            Handles.color = makeLevel.slots[index].blocks[b].typeBlock == BlockController.BlockType.Hide ? Color.gray : Color.white;
            Handles.DrawWireCube(blockPos, new Vector3(Constants.SLOT_WIDTH * 0.8f, Constants.BLOCK_HEIGHT * 0.8f, 0));
            
            string topicStr = makeLevel.slots[index].blocks[b].blockTopic != null ? makeLevel.slots[index].blocks[b].blockTopic.topicID.ToString() : "?";
            GUIStyle style = new GUIStyle();
            style.normal.textColor = Color.black;
            style.alignment = TextAnchor.MiddleCenter;
            Handles.Label(blockPos, topicStr, style);
        }

        float interactSize = Constants.SLOT_WIDTH * 0.8f;
        Vector3 interactPos = pos + new Vector3(0, Constants.BLOCK_HEIGHT * Mathf.Max(0, blockCount - 0.5f), 0);
        
        float distance = HandleUtility.DistanceToRectangle(interactPos, Quaternion.identity, interactSize);
        if (distance < 20f)
        {
            Handles.color = Color.yellow;
            Handles.DrawWireCube(interactPos, new Vector3(interactSize, interactSize, 0));

            if (e.type == EventType.MouseDown && e.button == 0) // Left click
            {
                Undo.RecordObject(makeLevel, "Add Block");
                if (makeLevel.topics.Count > 0 && makeLevel.indexTopicSelected < makeLevel.topics.Count) {
                    makeLevel.AddBlockToSlot(index, makeLevel.topics[makeLevel.indexTopicSelected], typeBlock);
                    makeLevel.UpdateSlotsInEditor();
                    makeLevel.GenerateBlocks();
                }
                e.Use();
            }
            else if (e.type == EventType.MouseDown && e.button == 1) // Right click
            {
                Undo.RecordObject(makeLevel, "Remove Block");
                makeLevel.RemoveBlockFromSlot(index);
                makeLevel.UpdateSlotsInEditor();
                makeLevel.GenerateBlocks();
                e.Use();
            }
        }
    }
}
#endif
