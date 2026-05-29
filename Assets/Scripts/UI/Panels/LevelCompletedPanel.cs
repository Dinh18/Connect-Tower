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

        // Hiệu ứng "Trải ra" cho Banner (làm chậm lại thành 1.5 giây)
        var bannerEffect = GetComponentInChildren<UIBannerWaveEffect>();
        if (bannerEffect != null)
        {
            bannerEffect.unfoldProgress = 0f;
            DOTween.To(() => bannerEffect.unfoldProgress, x => bannerEffect.unfoldProgress = x, 1f, 1.5f).SetEase(Ease.OutBack);
        }

        // Kích hoạt hiệu ứng gợn sóng từng chữ (làm chậm lại thành 2 giây)
        foreach(Transform child in header)
        {
            child.DOKill();
            
            var wavyText = child.GetComponent<TMPWavyText>();
            if (wavyText != null)
            {
                child.localScale = Vector3.one; // Khôi phục scale tổng vì ta scale từng chữ
                wavyText.showProgress = 0f;
                // Animate showProgress từ 0 lên 1 trong 2 giây
                DOTween.To(() => wavyText.showProgress, x => wavyText.showProgress = x, 1f, 2f).SetEase(Ease.Linear);
            }
            else
            {
                // Fallback nếu không dùng Wavy Text
                child.localScale = Vector3.zero;
                child.DOScale(1, 0.5f).SetEase(Ease.OutBack);
            }
        }

        yield return new WaitForSeconds(2f); // Đợi animation hoàn thành
    }

    private void OnContinueClicked()
    {
        continueButton.image.raycastTarget = false;
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
        
        continueButton.image.raycastTarget = true;
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
