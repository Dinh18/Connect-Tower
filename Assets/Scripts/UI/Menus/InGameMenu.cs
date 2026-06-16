using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InGameMenu : Menu
{
    [Header("Panels")]
    [SerializeField] private HeaderPanel headerPanel;
    [SerializeField] private BottomPanel bottomPanel;
    [SerializeField] private BorderPanel borderPanel;
    [SerializeField] private DifficultLevel difficultLevelUI;
    
    private GameManager gameManager;
    private LevelLoader levelLoader;

    void OnEnable()
    {
        GameEventBus.Subscribe<LoadingFinished>(ShowIngameMenu);
    }

    void OnDisable()
    {
        GameEventBus.UnSubscribe<LoadingFinished>(ShowIngameMenu);
    }

    void Start()
    {
        levelLoader = CoreServices.Get<LevelLoader>();
        gameManager = CoreServices.Get<GameManager>();        
    }   

    public override void Show()
    {
        this.gameObject.SetActive(true);
        if (gameManager == null) gameManager = CoreServices.Get<GameManager>();
        if (levelLoader == null) levelLoader = CoreServices.Get<LevelLoader>();


    }

    public override void Hide() 
    {
        if (headerPanel != null) headerPanel.Hide();
        if (bottomPanel != null) bottomPanel.Hide();
        // if (borderPanel != null) borderPanel.Hide();
        if(this.gameObject.activeSelf) StartCoroutine(DelayHideIngameMenu(0.3f));
    }

    public IEnumerator DelayHideIngameMenu(float timre)
    {
        yield return new WaitForSeconds(timre);
        this.gameObject.SetActive(false);
    }

    public void ShowIngameMenu(LoadingFinished evt)
    {
        LevelLoader.GameDifficult difficultLevel = (LevelLoader.GameDifficult)levelLoader.GetDifficultLevel(CoreServices.Get<DataManager>().GetCurrentLevel());
        if (difficultLevelUI != null && difficultLevel != LevelLoader.GameDifficult.Easy) 
        {
            difficultLevelUI.ShowDifficultLevel(difficultLevel);
        }
        if (headerPanel != null) headerPanel.Show();
        if (bottomPanel != null) bottomPanel.Show();
        CameraFitter.FitBoardOrtho(Camera.main, levelLoader.slots, bottomPanel.GetComponent<RectTransform>(), headerPanel.GetComponent<RectTransform>());
    }
    public void Setup()
    {
        if (headerPanel != null) headerPanel.Setup();
        if (bottomPanel != null) bottomPanel.Setup();
    }

    public HeaderPanel GetHeaderPanel()
    {
        return headerPanel;
    }
}
