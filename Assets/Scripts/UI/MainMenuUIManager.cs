using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuUIManager : MonoBehaviour, IMenu
{
    private UIManager uIManager;
    [Header("Button References")]
    [SerializeField] private Button AddCoins;
    [SerializeField] private Button Setting;
    [SerializeField] private Button playButton;
    [SerializeField] private Button homeButton;
    [SerializeField] private Button shopButton;
    // [SerializeField] private Button settingButton;
    [Header("Animation Setting")]
    private Vector3 originPosHomeButton;
    private Vector3 originPosShopButton;
    [SerializeField] private float startY = -1000f;
    [SerializeField] private float duration = 0.5f;
    [SerializeField] private float newPos;
    [Header("UI References")]
    [SerializeField] private GameObject homeButtonBackground;
    [SerializeField] private GameObject shopButtonBackground;
   

    public void Setup(UIManager uIManager)
    {
        originPosShopButton = shopButton.transform.position;
        originPosHomeButton = homeButton.transform.position;

        this.uIManager = uIManager;

        homeButton.onClick.AddListener(OnClickHome);
        shopButton.onClick.AddListener(OnClickShop);
        playButton.onClick.AddListener(OnClickPlay);
        AddCoins.onClick.AddListener(OnClickShop);
        Setting.onClick.AddListener(uIManager.OpenSetting);

        OnClickHome();
    }

    private void OnClickHome()
    {
        shopButtonBackground.SetActive(false);
        shopButton.transform.position = originPosShopButton;
        
        uIManager.CloseShop();

        homeButtonBackground.SetActive(true);
        homeButtonBackground.GetComponent<RectTransform>().DOAnchorPosY(startY,duration).From();
        homeButton.gameObject.transform.position = originPosHomeButton + new Vector3(0, newPos, 0);
    }
    private void OnClickShop()
    {
        shopButtonBackground.SetActive(true);
        shopButton.gameObject.transform.position = originPosShopButton + new Vector3(0, newPos, 0);
        shopButtonBackground.GetComponent<RectTransform>().DOAnchorPosY(startY,duration).From();
        
        uIManager.OpenShop();

        homeButtonBackground.SetActive(false);
        homeButton.gameObject.transform.position = originPosHomeButton;
    }
    private void OnClickPlay()
    {
        GameManager.Instance.ChangeState(GameManager.GameState.Playing);
    }


    public void Hide()
    {
        this.gameObject.SetActive(false);
    }

    public void Show()
    {
        this.gameObject.SetActive(true);
    }
}
