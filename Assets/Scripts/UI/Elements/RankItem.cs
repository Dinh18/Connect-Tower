
using UnityEngine;
using UnityEngine.UI;

public class RankItem : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image bgImage;
    [SerializeField] private Image badgeImage;
    [SerializeField] private Text rankText;
    [SerializeField] private Image frameImage;
    [SerializeField] private Image avatarImage;
    [SerializeField] private Text playerNameText;
    [SerializeField] private Text coinText;
    [SerializeField] private Image coinImage;
    [SerializeField] private Text currLevelText;
    [Header("Sprite References")]
    [SerializeField] private Sprite gold_Badge;
    [SerializeField] private Sprite sliver_Badge;
    [SerializeField] private Sprite bronze_Badge;
    [SerializeField] private Sprite red_Bg;
    [SerializeField] private Sprite Orange_Bg;
    [SerializeField] private Sprite yellow_Bg;
    [SerializeField] private Sprite gray_Bg;
    [SerializeField] private Sprite green_Bg;
    
    public void Setup(int rank, PlayerData playerData, bool isCurrPlayer = false)
    {
        switch(rank)
        {
            case 1:
                rankText.gameObject.SetActive(false);
                bgImage.sprite = red_Bg;
                badgeImage.sprite = gold_Badge;
                coinText.text = "5000";
                break;
            case 2:
                bgImage.sprite = Orange_Bg;
                rankText.gameObject.SetActive(false);
                badgeImage.sprite = sliver_Badge;
                coinText.text = "3000";
                break;
            case 3:
                rankText.gameObject.SetActive(false);
                bgImage.sprite = yellow_Bg;
                badgeImage.sprite = bronze_Badge;
                coinText.text = "1000";
                break;
            default:
                bgImage.sprite = gray_Bg;
                rankText.gameObject.SetActive(true);
                rankText.text = rank.ToString();
                badgeImage.gameObject.SetActive(false);
                coinText.gameObject.SetActive(false);
                coinImage.gameObject.SetActive(false);
                break;
        }

        if(isCurrPlayer && rank > 3) bgImage.sprite = green_Bg;

        frameImage.sprite = CoreServices.Get<DataManager>().GetFrameByID(playerData.frameID).itemSprite;
        avatarImage.sprite = CoreServices.Get<DataManager>().GetAvatarByID(playerData.avatarID).itemSprite;

        playerNameText.text = playerData.playerName;
        currLevelText.text = (playerData.currentLevel + 1).ToString();

    }
}
