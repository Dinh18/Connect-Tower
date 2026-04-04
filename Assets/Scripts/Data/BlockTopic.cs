using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BlockTopic", menuName = "Scriptable Objects/BlockTopic")]
public class BlockTopic : ScriptableObject
{
    [Header("Topic Infor")]
    public string topicName;
    [Range(1,10)]
    public int blockColor;
    public int topicID;
    [Header("Visual")]
    public List<Sprite> blocksSprite;
}
