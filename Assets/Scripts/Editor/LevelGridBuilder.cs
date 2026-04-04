#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System;

public class LevelGridBuilder : EditorWindow
{
    // --- BIẾN CẤU HÌNH ---
    private int slotsRow1 = 4;
    private int slotsRow2 = 4;
    private int totalTopics = 6; 
    // --- CẤU TRÚC DỮ LIỆU ---
    private class Slot
    {
        public List<int> blocks = new List<int>(); 
    }

    private List<Slot> row1 = new List<Slot>();
    private List<Slot> row2 = new List<Slot>();
    private string[] allTopicOptions;
    private int[] selectedTopicIDs; 
    private int currentPaletteIndex = 0; 
    private Dictionary<int, int> blocksPlacedPerTopicID = new Dictionary<int, int>();
    private bool showBoard = false;
    private Color colorDefault = GUI.backgroundColor;

    [MenuItem("Tools/Công Cụ Xếp Màn Chơi (Grid)")]
    public static void ShowWindow()
    {
        GetWindow<LevelGridBuilder>("Level Builder");
    }

    public void OnGUI()
    {
        slotsRow1 = EditorGUILayout.IntSlider("Số slot hàng 1", slotsRow1, 1, 5);
        slotsRow2 = EditorGUILayout.IntSlider("Số slot hàng 2", slotsRow2, 1, 5);
        totalTopics = EditorGUILayout.IntSlider("Số Topics", totalTopics, 1, slotsRow1 + slotsRow2 - 1);


        if(allTopicOptions == null || allTopicOptions.Length != Constants.TOTAL_TOPICS)
        {
            allTopicOptions = new string[Constants.TOTAL_TOPICS]; 
            for(int i = 0; i < Constants.TOTAL_TOPICS; i++)
            {
                allTopicOptions[i] = "Topic " + (i + 1);
            }
        }

        if(selectedTopicIDs == null || selectedTopicIDs.Length != totalTopics)
        {
            selectedTopicIDs = new int[totalTopics];
        }

        for(int i = 0; i < totalTopics; i++)
        {
            selectedTopicIDs[i] = EditorGUILayout.Popup("Topic " + (i + 1), selectedTopicIDs[i], allTopicOptions);
        }


        GUILayout.BeginHorizontal();
        for(int i = 0; i < selectedTopicIDs.Length; i++)
        {
            if(currentPaletteIndex == i)
            {
                GUI.backgroundColor = Color.green;
            }
            else
            {
                GUI.backgroundColor = colorDefault;
            }
            if(GUILayout.Button("Topic " + (selectedTopicIDs[i] + 1), GUILayout.Width(100), GUILayout.Height(50)))
            {
                currentPaletteIndex = i;
            }
            
        }
        GUI.backgroundColor = Color.white;
        GUILayout.EndHorizontal();

        if(GUILayout.Button("Tạo bảng"))
        {
            row1.Clear();
            row2.Clear();
            blocksPlacedPerTopicID.Clear();

            showBoard = true;

        }
        if(showBoard)
        {
            GUILayout.Label("Bảng đã tạo:");
            GenerateBoard();
        }
        if (row1.Count == 0 && row2.Count == 0) return;
    }

    private void GenerateBoard()
    {
        GUILayout.BeginHorizontal();
        for(int i = 0; i < slotsRow1; i++)
        {
            GUILayout.BeginVertical();
            for(int j = 0; j < 4; j++)
            {
                if(GUILayout.Button("Slot " + (i + 1) + " Block " + (j + 1), GUILayout.Width(100), GUILayout.Height(30)))
                {
                    
                }
            }
            GUILayout.EndVertical();
        }
        GUILayout.EndHorizontal();
        // row 2
        GUILayout.Space(20);
        GUILayout.BeginHorizontal();
        for(int i = 0; i < slotsRow2; i++)
        {
            GUILayout.BeginVertical();
            for(int j = 0; j < 4; j++)
            {
                if(GUILayout.Button("Slot " + (i + 1) + " Block " + (j + 1), GUILayout.Width(100), GUILayout.Height(30)))
                {
                    
                }
            }
            GUILayout.EndVertical();
        }
        GUILayout.EndHorizontal();
    }


}
#endif