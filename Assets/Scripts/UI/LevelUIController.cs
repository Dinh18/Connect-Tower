using UnityEngine;
using UnityEngine.UI;

public class LevelUIController : MonoBehaviour
{
    [SerializeField] private Text levelText;
    [SerializeField] private Image levelImage;
    [SerializeField] private Image badge_1;
    [SerializeField] private Image badge_2;
    private Sprite nextNormalSprite;
    private Sprite nextHardSprite;
    private Sprite nextSuperSprite;
    private Sprite currNormalSprite;
    private Sprite currHardSprite;
    private Sprite currSuperSprite;
    private Sprite hardSkullSprite;
    private Sprite superSkullSprite;
    private LevelUIManager levelUIManager;
    public void SetUp(LevelUIManager levelUIManager)
    {
        this.levelUIManager = levelUIManager;
    }
    public void ShowLevel(int levelOffset)
    {
        int level = DataManager.Instance.playerData.currentLevel + levelOffset;
        LevelLoader.GameDifficult gameDifficult;
        levelText.text = (level + 1).ToString();

        if(level >= LevelLoader.Instance.GetNumsLevel())
        {
            this.gameObject.SetActive(false);
            return;
        }

        this.gameObject.SetActive(true);
        gameDifficult = (LevelLoader.GameDifficult)LevelLoader.Instance.GetDifficultLevel(level);
        

        if(levelOffset == 0)
        {
            SetLevelImage(gameDifficult,true);
            // GetSkullSprite(gameDifficult);
        }
        else
        {
            SetLevelImage(gameDifficult,false);
            // GetSkullSprite(gameDifficult);
        }
        
    }

    private void SetLevelImage(LevelLoader.GameDifficult gameDifficult, bool isCurrentLevel)
    {
        if(isCurrentLevel)
        {
            levelImage.sprite = GetCurrentLevelSprite(gameDifficult);
        }
        else
        {
            levelImage.sprite = GetNextLevelSprite(gameDifficult);
        }
        if(gameDifficult == LevelLoader.GameDifficult.Easy)
        {
            badge_1.gameObject.SetActive(false);
            badge_2.gameObject.SetActive(false);
        }
        else if(gameDifficult == LevelLoader.GameDifficult.Hard)
        {
            badge_1.gameObject.SetActive(true);
            badge_1.sprite = GetSkullSprite(gameDifficult);

            badge_2.gameObject.SetActive(false);

        }
        else
        {
            badge_1.gameObject.SetActive(true);
            badge_2.gameObject.SetActive(true);
            badge_1.sprite = GetSkullSprite(gameDifficult);
            badge_2.sprite = GetSkullSprite(gameDifficult);
        }
        
    }

    public Sprite GetNextLevelSprite(LevelLoader.GameDifficult gameDifficult)
    {
        if (gameDifficult == LevelLoader.GameDifficult.Easy)
        {
            if (nextNormalSprite == null) nextNormalSprite = Resources.Load<Sprite>(Constants.NORMAL_NEXT_LVL);
            return nextNormalSprite;
        }
        else if (gameDifficult == LevelLoader.GameDifficult.Hard)
        {
            if (nextHardSprite == null) nextHardSprite = Resources.Load<Sprite>(Constants.HARD_NEXT_LVL);
            return nextHardSprite;
        }
        else
        {
            if (nextSuperSprite == null) nextSuperSprite = Resources.Load<Sprite>(Constants.SUPERHARD_NEXT_LVL);
            return nextSuperSprite;
        }
    }

    public Sprite GetCurrentLevelSprite(LevelLoader.GameDifficult gameDifficult)
    {
        if (gameDifficult == LevelLoader.GameDifficult.Easy)
        {
            if (currNormalSprite == null) currNormalSprite = Resources.Load<Sprite>(Constants.NORMAL_CURRENT_LVL);
            return currNormalSprite;
        }
        else if (gameDifficult == LevelLoader.GameDifficult.Hard)
        {
            if (currHardSprite == null) currHardSprite = Resources.Load<Sprite>(Constants.HARD_CURRENT_LVL);
            return currHardSprite;
        }
        else
        {
            if (currSuperSprite == null) currSuperSprite = Resources.Load<Sprite>(Constants.SUPERHARD_CURRENT_LVL);
            return currSuperSprite;
        }
    }

    public Sprite GetSkullSprite(LevelLoader.GameDifficult gameDifficult)
    {
        if (gameDifficult == LevelLoader.GameDifficult.Hard)
        {
            if (hardSkullSprite == null) hardSkullSprite = Resources.Load<Sprite>(Constants.HARD_LVL_SKULL);
            return hardSkullSprite;
        }
        else if (gameDifficult == LevelLoader.GameDifficult.VeryHard)
        {
            if (superSkullSprite == null) superSkullSprite = Resources.Load<Sprite>(Constants.SUPERHARD_LVL_SKULL);
            return superSkullSprite;
        }
        else return null;
    }
}
