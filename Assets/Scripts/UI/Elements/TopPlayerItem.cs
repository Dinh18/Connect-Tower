using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TopPlayerItem : MonoBehaviour
{
    [SerializeField] private Image frameImage;
    [SerializeField] private Image avatarImage;
    [SerializeField] private TextMeshProUGUI playerNameText;
    [SerializeField] private TextMeshProUGUI currLevelText;

    public void Setup(PlayerData playerData)
    {
        if (playerData == null)
        {
            gameObject.SetActive(false);
            return;
        }

        gameObject.SetActive(true);
        frameImage.sprite = CoreServices.Get<DataManager>().GetFrameByID(playerData.frameID).itemSprite;
        avatarImage.sprite = CoreServices.Get<DataManager>().GetAvatarByID(playerData.avatarID).itemSprite;

        playerNameText.text = playerData.playerName;
        currLevelText.text = "Level: " + (playerData.currentLevel + 1).ToString();
    }
}
