using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

public class LevelSolverEditor : EditorWindow
{
    LevelDataSO levelData;
    int maxSearchNodes = 100000;

    [MenuItem("Tools/Level Solver")]
    public static void ShowWindow()
    {
        GetWindow<LevelSolverEditor>("Level Solver AI");
    }

    void OnGUI()
    {
        GUILayout.Label("Connect Tower AI Solver", EditorStyles.boldLabel);
        levelData = (LevelDataSO)EditorGUILayout.ObjectField("Level Data", levelData, typeof(LevelDataSO), false);
        maxSearchNodes = EditorGUILayout.IntField("Max Nodes", maxSearchNodes);

        if (GUILayout.Button("Solve"))
        {
            if (levelData != null)
                Solve(levelData);
        }
    }

    struct SlotState : System.IEquatable<SlotState>
    {
        public byte type; // 0: Normal, 1: Hide, 2: Ice
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
        public string moveDesc;
        public int g;
        public int h;
        public int f => g + h;
    }

    private void Solve(LevelDataSO level)
    {
        GameState initial = CreateInitialState(level);
        if (initial.IsWin())
        {
            Debug.Log("Level is already won.");
            return;
        }

        var openList = new List<Node>();
        var closedSet = new HashSet<GameState>();

        var startNode = new Node { state = initial, parent = null, moveDesc = "Start", g = 0, h = Heuristic(initial) };
        openList.Add(startNode);
        closedSet.Add(initial);

        int expanded = 0;

        while (openList.Count > 0 && expanded < maxSearchNodes)
        {
            openList.Sort((a, b) => a.f.CompareTo(b.f));
            Node curr = openList[0];
            openList.RemoveAt(0);
            expanded++;

            if (curr.state.IsWin())
            {
                PrintSolution(curr, expanded);
                return;
            }

            foreach (var succ in GenerateSuccessors(curr))
            {
                if (!closedSet.Contains(succ.state))
                {
                    closedSet.Add(succ.state);
                    openList.Add(succ);
                }
            }
        }

        Debug.Log($"No solution found. Expanded {expanded} nodes.");
    }

    private int Heuristic(GameState state)
    {
        int h = 0;
        for (int i = 0; i < state.slots.Length; i++)
        {
            if (state.slots[i].count > 0 && !state.slots[i].IsCompleted())
            {
                h++;
                // Penalty for hidden blocks
                for(int j=0; j<state.slots[i].count; j++) {
                    if (state.slots[i].hiddens[j]) h++;
                }
            }
        }
        return h;
    }

    private List<Node> GenerateSuccessors(Node node)
    {
        var list = new List<Node>();
        GameState state = node.state;
        int numSlots = state.slots.Length;

        for (int i = 0; i < numSlots; i++)
        {
            if (!state.slots[i].isRevealed) continue;
            if (state.slots[i].type == 2) continue; // Ice slot cannot move out
            
            // Optimization: If slot is already completed, no need to move out, unless it's blocking something? 
            // Wait, completed slots can technically be moved out of in standard games, but it's never optimal.
            if (state.slots[i].IsCompleted()) continue;

            int moveCount = state.slots[i].GetMoveCount();
            if (moveCount == 0) continue;

            int moveTopic = state.slots[i].GetTopTopic();

            for (int j = 0; j < numSlots; j++)
            {
                if (i == j) continue;
                if (!state.slots[j].isRevealed) continue;
                if (state.slots[j].count == 4) continue; // Full

                // Must match topic or be empty
                if (state.slots[j].count > 0 && state.slots[j].GetTopTopic() != moveTopic) continue;

                // Optimization: Don't move a full identical chunk to an empty slot (useless move)
                if (state.slots[j].count == 0 && moveCount == state.slots[i].count && !state.slots[i].hiddens[0]) continue;

                int amountToMove = Mathf.Min(4 - state.slots[j].count, moveCount);
                if (amountToMove <= 0) continue;

                GameState nextState = CloneState(state);
                
                // Do move
                for (int m = 0; m < amountToMove; m++)
                {
                    nextState.slots[j].topics[nextState.slots[j].count] = nextState.slots[i].topics[nextState.slots[i].count - 1];
                    nextState.slots[j].hiddens[nextState.slots[j].count] = false; // becoming top reveals it
                    nextState.slots[j].count++;
                    nextState.slots[i].count--;
                }

                // Reveal new top of source slot if it was hidden
                if (nextState.slots[i].count > 0)
                {
                    nextState.slots[i].hiddens[nextState.slots[i].count - 1] = false;
                }

                // Check triggers for Hide slots
                CheckReveals(ref nextState);

                list.Add(new Node
                {
                    state = nextState,
                    parent = node,
                    g = node.g + 1,
                    h = Heuristic(nextState),
                    moveDesc = $"Move {amountToMove} blocks of topic {moveTopic} from Slot {i} to Slot {j}"
                });
            }
        }

        return list;
    }

    private void CheckReveals(ref GameState state)
    {
        for (int i = 0; i < state.slots.Length; i++)
        {
            if (state.slots[i].type == 1 && !state.slots[i].isRevealed) // Hide slot
            {
                int reqTopic = state.slots[i].questionTopicID;
                // Check if any slot is completed with this topic
                for (int j = 0; j < state.slots.Length; j++)
                {
                    if (state.slots[j].IsCompleted() && state.slots[j].topics[0] == reqTopic)
                    {
                        state.slots[i].isRevealed = true;
                        break;
                    }
                }
            }
        }
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

    private void PrintSolution(Node winNode, int expanded)
    {
        var path = new List<Node>();
        Node curr = winNode;
        while (curr != null)
        {
            path.Add(curr);
            curr = curr.parent;
        }
        path.Reverse();

        Debug.Log($"<color=green>Solution found in {path.Count - 1} steps! Nodes expanded: {expanded}</color>");
        for (int i = 1; i < path.Count; i++)
        {
            Debug.Log($"Step {i}: {path[i].moveDesc}");
        }
    }

    private GameState CreateInitialState(LevelDataSO data)
    {
        GameState state = new GameState { slots = new SlotState[data.slots.Count] };
        for (int i = 0; i < data.slots.Count; i++)
        {
            var sd = data.slots[i];
            state.slots[i].Init();
            state.slots[i].type = (byte)sd.slotType;
            state.slots[i].questionTopicID = sd.questionTopic != null ? sd.questionTopic.topicID : -1;
            state.slots[i].isRevealed = (sd.slotType != SlotController.SlotType.Hide);

            // In BlocksManager, j = Count - 1 is pushed FIRST (bottom)
            for (int j = sd.blocks.Count - 1; j >= 0; j--)
            {
                var bd = sd.blocks[j];
                state.slots[i].topics[state.slots[i].count] = bd.blockTopic.topicID;
                state.slots[i].hiddens[state.slots[i].count] = (bd.typeBlock == BlockController.BlockType.Hide);
                state.slots[i].count++;
            }
            
            // Ensure top block is revealed
            if (state.slots[i].count > 0)
            {
                state.slots[i].hiddens[state.slots[i].count - 1] = false;
            }
        }
        return state;
    }
}
