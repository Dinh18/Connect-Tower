using Unity.Android.Gradle.Manifest;
using UnityEngine;
using UnityEngine.UI;

public class LevelUI : MonoBehaviour
{
    [Range(0,2)]
    [Header("Level Setting (0: current, 1: next1, 2: next2)")]
    [SerializeField] private int levelStage;
    [SerializeField] private Text levelText;
    [SerializeField] private Image levelImage;
    [SerializeField] private Image badgeImage1;
    [SerializeField] private Image badgeImage2;
    public void ShowLevel(int currLevel)
    {
        int level = levelStage + currLevel;
        levelText.text = (level + 1).ToString();
        if(level >= LevelLoader.Instance.GetNumsLevel())
        {
            levelImage.sprite = Resources.Load<Sprite>(Constants.NORMAL_LVL_NEXT_STAGE);
            badgeImage1.gameObject.SetActive(false);
            badgeImage2.gameObject.SetActive(false);
            return;
        }
        switch(LevelLoader.Instance.GetDifficultLevel(level))
        {
            case 0:
                if(levelStage == 0)
                {
                    levelImage.sprite = Resources.Load<Sprite>(Constants.NORMAL_LVL_STAGE_CURRENT);
                }
                else
                {
                    levelImage.sprite = Resources.Load<Sprite>(Constants.NORMAL_LVL_NEXT_STAGE);
                }
                badgeImage1.gameObject.SetActive(false);
                badgeImage2.gameObject.SetActive(false);
                break;
            case 1:
                if(levelStage == 0)
                {
                    levelImage.sprite = Resources.Load<Sprite>(Constants.HARD_LVL_STAGE_CURRENT);
                }
                else
                {
                    levelImage.sprite= Resources.Load<Sprite>(Constants.HARD_LVL_NEXT_STAGE);
                }
                badgeImage1.sprite = Resources.Load<Sprite>(Constants.HARD_LVL_SKULL);
                // badgeImage2.sprite = Resources.Load<Sprite>(Constants.HARD_LVL_SKULL);
                badgeImage1.gameObject.SetActive(true);
                badgeImage2.gameObject.SetActive(false);
                break;
            case 2:
                if(levelStage == 0)
                {
                    levelImage.sprite = Resources.Load<Sprite>(Constants.SUPERHARD_LVL_STAGE_CURRENT);
                }
                else
                {
                    levelImage.sprite = Resources.Load<Sprite>(Constants.SUPERHARD_LVL_NEXT_STAGE);
                }
                badgeImage1.sprite = Resources.Load<Sprite>(Constants.SUPERHARD_LVL_SKULL);
                badgeImage2.sprite = Resources.Load<Sprite>(Constants.SUPERHARD_LVL_SKULL);
                badgeImage1.gameObject.SetActive(true);
                badgeImage2.gameObject.SetActive(true);
                break;
        }
    }

    // private void SetupLevel(int levelStage, int currLevel)
    // {
    //     int level
    // }

}
