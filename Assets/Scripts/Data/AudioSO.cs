using UnityEngine;

public enum SoundID
{
    None = 0,
    Button,
    ButtonDown,
    ButtonUp,
    AddMove,
    BlockFail,
    Cloth,
    FreezeUp,
    HintBooster,
    BlockIceFinished,
    LevelWin,
    LevelLose,
    CoinCollect,
    MoveWoosh,
    PopMoved1,
    PopMoved2,
    PopMoved3,
    PopMoved4,
    SlotFinished,
    Shuffle,
    HideBlock,
    AddBooster,
    SelectSlot,
    MoveFail,
    FireWork
}

[CreateAssetMenu(fileName = "AudioDatabase", menuName = "Scriptable Objects/AudioDatabase")]
public class AudioSO : ScriptableObject
{
    public SoundID soundID;
    public AudioClip audioClip;
}
