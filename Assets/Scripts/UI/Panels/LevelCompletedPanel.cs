using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class LevelCompletedPanel : Panel
{
    // private UIManager uIManager; // Loại bỏ phụ thuộc
    [SerializeField] private Button continueButton;
    [SerializeField] private Transform header;
    [SerializeField] private Transform coinImage;
    [SerializeField] private Text coinText;
    private int oldCoins;

    void OnEnable()
    {
        continueButton.onClick.AddListener(OnContinueClicked);
    }

    void OnDisable()
    {
        continueButton.onClick.RemoveAllListeners();
    }

    public override void Show()
    {
        int winAmount = CoreServices.Get<LevelLoader>().GetCurrentLevelReward();
        oldCoins = CoreServices.Get<DataManager>().GetTotalCoins() - winAmount;
        if (coinText != null) coinText.text = oldCoins.ToString();

        this.gameObject.SetActive(true);
        StartCoroutine(ShowCoroutine());
    }

    private IEnumerator ShowCoroutine()
    {
        // AudioManager.Instance.PlayLVLWinAudio();
        GameEventBus.Publish(new RequestPlaySFX{soundID = SoundID.LevelWin});
        coinImage.DOKill();
        coinImage.localScale = Vector3.zero;
        coinImage.DOScale(1, 0.5f).SetEase(Ease.OutBack);
        foreach(Transform child in header)
        {
            child.DOKill();
            child.localScale = Vector3.zero;
        }

        foreach(Transform child in header)
        {
            child.DOScale(1, 0.5f).SetEase(Ease.OutBack);
            yield return new WaitForSeconds(0.1f);
        }  
    }

    private void OnContinueClicked()
    {
        continueButton.interactable = false;
        int winAmount = CoreServices.Get<LevelLoader>().GetCurrentLevelReward();
        StartCoroutine(SpawnWinCoinsRoutine(winAmount));
    }

    private IEnumerator SpawnWinCoinsRoutine(int winAmount)
    {
        if(coinText != null) DOTween.Kill(coinText.transform);

        Vector2 buttonPos = continueButton.transform.position;
        
        int visualCoins = 10;
        int coinValueBase = winAmount / visualCoins;
        int coinValueRemainder = winAmount % visualCoins;

        for (int i = 0; i < visualCoins; i++)
        {
            GameObject coinObj = CoinEffect.Instance.GetCoin();
            CoinFlyEffect coinEffect = coinObj.GetComponent<CoinFlyEffect>();
            
            int finalValueForThisCoin = coinValueBase + (i == visualCoins - 1 ? coinValueRemainder : 0);

            RectTransform targetRect = coinText != null ? coinText.GetComponent<RectTransform>() : coinImage.GetComponent<RectTransform>();

            Vector2 randomOffset = UnityEngine.Random.insideUnitCircle * 100f;
            Vector2 startPos = buttonPos + randomOffset;

            coinEffect.StartBurstAndFly(startPos, targetRect, () => 
            {
                OnCoinHitTarget(finalValueForThisCoin);
            });

            yield return new WaitForSeconds(0.1f); 
        }

        yield return new WaitForSeconds(0.8f);
        
        continueButton.interactable = true;
        CoreServices.Get<GameManager>().ChangeState(GameManager.GameState.MainMenu);
    }

    private void OnCoinHitTarget(int amountAdded)
    {
        oldCoins += amountAdded; 
        if(coinText != null)
        {
            coinText.text = oldCoins.ToString();
            coinText.transform.DOPunchScale(new Vector3(0.2f, 0.2f, 0), 0.1f).SetEase(Ease.InOutBounce);
        }
        else
        {
            coinImage.DOPunchScale(new Vector3(0.2f, 0.2f, 0), 0.1f).SetEase(Ease.InOutBounce);
        }

        GameEventBus.Publish(new RequestPlaySFX{soundID = SoundID.CoinCollect});
    }

    public override void Hide() => this.gameObject.SetActive(false);
    public GameObject GetGameObject() => this.gameObject;
}
