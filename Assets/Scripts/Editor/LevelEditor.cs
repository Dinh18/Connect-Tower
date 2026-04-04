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
    // Setting slot
    int typeSlot = 0; // 0: Normal, 1: Hide, 2: Ice
    public override void OnInspectorGUI()
    {

        MakeLevel makeLevel = (MakeLevel)target;

        serializedObject.Update();

        SerializedProperty prop = serializedObject.GetIterator();

        Undo.RecordObject(makeLevel, "Change Level Settings");

        bool enterChildren = true;

        while(prop.NextVisible(enterChildren))
        {
            enterChildren = false;

            if(prop.name == "blockHolder")
            {
                makeLevel.row1 = EditorGUILayout.IntSlider("Số slot hàng 1", makeLevel.row1, 0, 5);
                makeLevel.row2 = EditorGUILayout.IntSlider("Số slot hàng 2", makeLevel.row2, 0, 5);

                makeLevel.totalTopics = EditorGUILayout.IntSlider("Số Topics", makeLevel.totalTopics, 1, makeLevel.row1 + makeLevel.row2 - 1);

                if (GUILayout.Button("Setting Slots and Topics", GUILayout.Width(150), GUILayout.Height(30)))
                {
                    makeLevel.SettingSlots();
                }
                
                DrawTopicListInSpector(makeLevel);

                GUILayout.Space(10);
                GUILayout.Label("Block Setup", EditorStyles.boldLabel);
                GUILayout.BeginHorizontal();
                if(GUILayout.Toggle(typeBlock == 0, "Normal", EditorStyles.radioButton))
                {
                    typeBlock = 0;
                }
                if(GUILayout.Toggle(typeBlock == 1, "Hide", EditorStyles.radioButton))
                    typeBlock = 1;
                GUILayout.EndHorizontal();

                DrawSlotsListInSpector(makeLevel);

                // if(GUILayout.Button("Generate Slots", GUILayout.Width(150), GUILayout.Height(30)))
                // {
                //     makeLevel.UpdateSlotsInEditor();
                // }
            }

            if (prop.name != "m_Script")
            {
                EditorGUILayout.PropertyField(prop, true);
            }
        }

        // if(GUILayout.Button("Generate Blocks", GUILayout.Width(150), GUILayout.Height(30)))
        // {
        //     makeLevel.GenerateBlocks();
        // }
        if(GUILayout.Button("Preview Level", GUILayout.Width(150), GUILayout.Height(30)))
        {
            makeLevel.UpdateSlotsInEditor();
            makeLevel.GenerateBlocks();
        }

        if(GUILayout.Button("Reset", GUILayout.Width(150), GUILayout.Height(30)))
        {
            makeLevel.Reset();
        }

        if(GUILayout.Button("Save Level Data", GUILayout.Width(150), GUILayout.Height(30)))
        {
            makeLevel.SaveLevelData();
        }
        

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
        GUILayout.Space(20);
        GUILayout.Label("Slots List", EditorStyles.boldLabel);

        int index = 0;;     

        GUILayout.BeginHorizontal();

        for(int i = 0; i < makeLevel.row1; i++)
        {
            if (index >= makeLevel.slots.Count) break;
            GUILayout.BeginVertical(); 
            makeLevel.slots[index].slotType = (SlotController.SlotType)EditorGUILayout.EnumPopup(makeLevel.slots[index].slotType, GUILayout.Width(65));
            // Tạo nút thêm block hoặc xóa block
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
                    makeLevel.AddBlockToSlot(index, makeLevel.topics[makeLevel.indexTopicSelected], typeBlock);
                }
            }
            // Tạo nút chọn topic khi là hide
            if(makeLevel.slots[index].slotType == SlotController.SlotType.Hide)
            {
                string btnHideText = "Question Topic " + (makeLevel.slots[index].questionTopic != null ? makeLevel.slots[index].questionTopic.topicID.ToString() : "None");
                if(GUILayout.Button(btnHideText, GUILayout.Width(100), GUILayout.Height(30)))
                {
                    makeLevel.slots[index].questionTopic = makeLevel.topics[makeLevel.indexTopicSelected];
                }
            }
            GUILayout.EndVertical();
            index++;
        }
        GUILayout.EndHorizontal();


        GUILayout.BeginHorizontal();
        
        for(int i = 0; i < makeLevel.row2; i++)
        {
            if (index >= makeLevel.slots.Count) break;
            GUILayout.BeginVertical(); 

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
                    makeLevel.AddBlockToSlot(index, makeLevel.topics[makeLevel.indexTopicSelected], typeBlock);
                }
            }
            // Tạo nút chọn topic khi là hide
            if(makeLevel.slots[index].slotType == SlotController.SlotType.Hide)
            {
                string btnHideText = "Question Topic " + (makeLevel.slots[index].questionTopic != null ? makeLevel.slots[index].questionTopic.topicID.ToString() : "None");
                if(GUILayout.Button(btnHideText, GUILayout.Width(100), GUILayout.Height(30)))
                {
                    makeLevel.slots[index].questionTopic = makeLevel.topics[makeLevel.indexTopicSelected];
                }
            }
            GUILayout.EndVertical();
            index++;
        }
        GUILayout.EndHorizontal();
        GUILayout.Space(20);
    }
}
#endif
