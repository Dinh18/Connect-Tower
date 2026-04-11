using UnityEngine;
using UnityEngine.UI;

public class LevelUIController : MonoBehaviour
{
    [SerializeField] private Text levelText;
    [SerializeField] private Image levelImage;
    [SerializeField] private Image badge_1;
    [SerializeField] private Image badge_2;
    private LevelUIManager levelUIManager;
    public void SetUp(LevelUIManager levelUIManager)
    {
        this.levelUIManager = levelUIManager;
    }
    public void ShowLevel(int level, LevelLoader.GameDifficult gameDifficult)
    {
        levelText.text = level.ToString();
        levelImage.sprite = levelUIManager.GetLevelSprite(gameDifficult);
        Sprite skullSprite = levelUIManager.GetSkullSprite(gameDifficult);
        if(skullSprite == null)
        {
            badge_1.gameObject.SetActive(false);
            badge_2.gameObject.SetActive(false);
        }
        else
        {
            if(gameDifficult == LevelLoader.GameDifficult.Hard)
            {
                badge_1.gameObject.SetActive(true);
                badge_2.gameObject.SetActive(false);
            }
            else if(gameDifficult == LevelLoader.GameDifficult.VeryHard)
            {
                badge_1.gameObject.SetActive(true);
                badge_2.gameObject.SetActive(true);
            }
            badge_1.sprite = skullSprite;
            badge_2.sprite = skullSprite;
        }
    }
}
