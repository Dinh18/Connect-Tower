using System;
using UnityEngine;
using UnityEngine.UI;

public class HeartManager : MonoBehaviour
{
    [SerializeField] int restoreDuration = 120;
    [SerializeField] private Text timerTextMainMenu;
    [SerializeField] private Text timerTextAddHeardPopup;
    private DateTime nextHeartTime;
    private DateTime lastHeartTime;
    private DataManager dataManager;
    void Awake()
    {
        CoreServices.Register<HeartManager>(this);
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        dataManager = CoreServices.Get<DataManager>();
        LoadHeart();
    }

    // Update is called once per frame
    void Update()
    {
        if(dataManager.GetHearts() < 5)
        {
            UpdateTimer();
        }
        else
        {
            timerTextMainMenu.text = "Max";
            timerTextAddHeardPopup.text = "Max";
        }
    }

    public void UpdateTimer()
    {
        DateTime currentTime = DateTime.Now;
        DateTime targetTime = nextHeartTime;

        double secondsLeft = (targetTime - currentTime).TotalSeconds;

        if(secondsLeft <= 0)
        {
            nextHeartTime = nextHeartTime.AddSeconds(restoreDuration);
            dataManager.AddHeart(1, nextHeartTime.ToString());
        }
        else
        {
            // Hiển thị định dạng mm:ss
            TimeSpan time = TimeSpan.FromSeconds(secondsLeft);
            timerTextMainMenu.text = string.Format("{0:D2}:{1:D2}", time.Minutes, time.Seconds);
            timerTextAddHeardPopup.text = string.Format("{0:D2}:{1:D2}", time.Minutes, time.Seconds);
        }
    }

    public void UseHeart()
    {
        if (dataManager.GetHearts() > 0)
        {
            if (dataManager.GetHearts() == 5)
            {
                nextHeartTime = DateTime.Now.AddSeconds(restoreDuration);
            }
            
            dataManager.UseHeart(nextHeartTime.ToString());
        }
    }

    private void LoadHeart()
    {
        string nextTimeStr = dataManager.GetNextHeartTime();
        if (!string.IsNullOrEmpty(nextTimeStr))
        {
            nextHeartTime = DateTime.Parse(nextTimeStr);
            
            // Tính toán bù năng lượng khi offline
            double secondsOffline = (DateTime.Now - nextHeartTime).TotalSeconds;
            if (secondsOffline > 0)
            {
                int energyToAdd = 1 + (int)(secondsOffline / restoreDuration);
                dataManager.AddHeart(energyToAdd, nextHeartTime.ToString());
                
                // Tính lại mốc thời gian lẻ còn dư
                double remainderSeconds = secondsOffline % restoreDuration;
                nextHeartTime = DateTime.Now.AddSeconds(restoreDuration - remainderSeconds);
            }
        }
        else
        {
            nextHeartTime = DateTime.Now;
        }
    }
}
