
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RankItem : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image bgImage;
    [SerializeField] private TextMeshProUGUI rankText;
    [SerializeField] private Image frameImage;
    [SerializeField] private Image avatarImage;
    [SerializeField] private TextMeshProUGUI playerNameText;
    [SerializeField] private TextMeshProUGUI currLevelText;
    [Header("Sprite References")]
    [SerializeField] private Sprite green_Bg;
    [SerializeField] private Sprite red_Bg;
    
    public void Setup(int rank, PlayerData playerData, bool isCurrPlayer = false)
    {

        bgImage.sprite = isCurrPlayer ? red_Bg : green_Bg;

        frameImage.sprite = CoreServices.Get<DataManager>().GetFrameByID(playerData.frameID).itemSprite;
        avatarImage.sprite = CoreServices.Get<DataManager>().GetAvatarByID(playerData.avatarID).itemSprite;

        rankText.text = rank.ToString();

        playerNameText.text = playerData.playerName;
        currLevelText.text = (playerData.currentLevel + 1).ToString();

    }
}
