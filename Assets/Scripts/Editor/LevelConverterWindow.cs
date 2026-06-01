using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;

public class LevelConverterWindow : EditorWindow
{
    private string sourceFolder = @"D:\Downloads\BallSort\ExportedProject\Assets\Resources\data";
    private string destFolder = "Assets/Resources/Data/Levels";
    private BlockTopic[] availableTopics;

    [MenuItem("Tools/Convert Levels")]
    public static void ShowWindow()
    {
        GetWindow<LevelConverterWindow>("Convert Levels");
    }

    private void OnGUI()
    {
        GUILayout.Label("Level Converter", EditorStyles.boldLabel);
        sourceFolder = EditorGUILayout.TextField("Source Folder", sourceFolder);
        destFolder = EditorGUILayout.TextField("Dest Folder", destFolder);

        if (GUILayout.Button("Convert All Levels"))
        {
            ConvertLevels();
        }
    }

    private void ConvertLevels()
    {
        if (!Directory.Exists(sourceFolder))
        {
            Debug.LogError("Source folder not found: " + sourceFolder);
            return;
        }

        if (!Directory.Exists(destFolder))
        {
            Directory.CreateDirectory(destFolder);
        }

        availableTopics = Resources.LoadAll<BlockTopic>("Data/topics2");
        if (availableTopics == null || availableTopics.Length == 0)
        {
            Debug.LogError("No topics found in Resources/Data/topics2!");
            return;
        }

        string[] jsonFiles = Directory.GetFiles(sourceFolder, "lv_*.json");
        List<string> fileList = new List<string>(jsonFiles);
        // Sort files to process them in order, though each has puzzle_id
        // It's better to extract level numbers to set the SO's level id correctly.
        // Or we can just use a global counter starting from 1.
        int globalLevelIndex = 1;

        // The files are lv_1_100, lv_101_600, etc. We should sort them by the first number.
        fileList.Sort((a, b) =>
        {
            int numA = ExtractFirstNumber(Path.GetFileName(a));
            int numB = ExtractFirstNumber(Path.GetFileName(b));
            return numA.CompareTo(numB);
        });

        AssetDatabase.StartAssetEditing();
        try
        {
            foreach (string file in fileList)
            {
                Debug.Log("Processing " + file);
                string json = File.ReadAllText(file);
                
                // Extract all puzzle blocks. Tubes array is like [[0,0],[1,1]], steps is like [[0,1]]
                MatchCollection levelMatches = Regex.Matches(json, @"{""puzzle_id"":""[^""]+"",""tubes"":(\[\[.*?\]\])(?:,""steps"":(\[\[.*?\]\]))?");
                foreach (Match levelMatch in levelMatches)
                {
                    string tubesStr = levelMatch.Groups[1].Value; // e.g. [[0,0,0,1],[0,1,1,1]]

                    LevelDataSO levelSO = ScriptableObject.CreateInstance<LevelDataSO>();
                    levelSO.level = globalLevelIndex;
                    levelSO.difficult = 0;

                    int calculatedMoves = 10;
                    if (levelMatch.Groups.Count > 2 && !string.IsNullOrEmpty(levelMatch.Groups[2].Value))
                    {
                        string stepsStr = levelMatch.Groups[2].Value;
                        int stepCount = Regex.Matches(stepsStr, @"\[([0-9,\s]+)\]").Count;
                        calculatedMoves = stepCount + 10;
                    }
                    levelSO.moves = calculatedMoves;

                    List<List<int>> tubes = ParseTubes(tubesStr);
                    
                    int totalTubes = tubes.Count;
                    levelSO.row1 = totalTubes / 2 + (totalTubes % 2);
                    levelSO.row2 = totalTubes / 2;
                    levelSO.slots = new List<SlotSetupData>();

                    HashSet<int> uniqueColors = new HashSet<int>();

                    foreach (var tube in tubes)
                    {
                        SlotSetupData slotData = new SlotSetupData();
                        slotData.slotType = SlotController.SlotType.Normal;
                        slotData.blocks = new List<BlockSetupData>();

                        foreach (int color in tube)
                        {
                            if (color > 0)
                            {
                                uniqueColors.Add(color);
                                BlockSetupData blockData = new BlockSetupData();
                                blockData.typeBlock = BlockController.BlockType.Normal;
                                // Map color to topic
                                int topicIndex = (color - 1) % availableTopics.Length;
                                blockData.blockTopic = availableTopics[topicIndex];
                                blockData.indexSprite = 0;
                                slotData.blocks.Add(blockData);
                            }
                        }
                        levelSO.slots.Add(slotData);
                    }
                    
                    levelSO.numsTopic = uniqueColors.Count;

                    string assetPath = $"{destFolder}/Level_{globalLevelIndex:D2}.asset";
                    if (File.Exists(assetPath))
                    {
                        AssetDatabase.DeleteAsset(assetPath);
                    }
                    AssetDatabase.CreateAsset(levelSO, assetPath);
                    globalLevelIndex++;
                }
            }
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        Debug.Log($"<color=green>Successfully converted {globalLevelIndex - 1} levels!</color>");
    }

    private int ExtractFirstNumber(string filename)
    {
        Match m = Regex.Match(filename, @"\d+");
        if (m.Success)
            return int.Parse(m.Value);
        return 0;
    }

    private List<List<int>> ParseTubes(string tubesStr)
    {
        List<List<int>> tubes = new List<List<int>>();
        // tubesStr is like [[0,0,0,1],[0,1,1,1]]
        MatchCollection matches = Regex.Matches(tubesStr, @"\[([0-9,\s]+)\]");
        foreach (Match m in matches)
        {
            string numsStr = m.Groups[1].Value;
            string[] parts = numsStr.Split(',');
            List<int> tube = new List<int>();
            foreach (string p in parts)
            {
                if (int.TryParse(p.Trim(), out int val))
                {
                    tube.Add(val);
                }
            }
            tubes.Add(tube);
        }
        return tubes;
    }
}
