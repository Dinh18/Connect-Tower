using System;
using UnityEngine;
using UnityEngine.UI;

public class HeartManager : MonoBehaviour
{
    [SerializeField] int restoreDuration = 120;
    [SerializeField] private Text timerText;
    private DateTime nextHeartTime;
    private DateTime lastHeartTime;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        LoadHeart();
    }

    // Update is called once per frame
    void Update()
    {
        if(DataManager.Instance.playerData.heart < 5)
        {
            UpdateTimer();
        }
        else
        {
            timerText.text = "Max";
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
            DataManager.Instance.AddHeart(1, nextHeartTime.ToString());
        }
        else
        {
            // Hiển thị định dạng mm:ss
            TimeSpan time = TimeSpan.FromSeconds(secondsLeft);
            timerText.text = string.Format("{0:D2}:{1:D2}", time.Minutes, time.Seconds);
        }
    }

    public void UseHeart()
    {
        if (DataManager.Instance.playerData.heart > 0)
        {
            if (DataManager.Instance.playerData.heart == 5)
            {
                nextHeartTime = DateTime.Now.AddSeconds(restoreDuration);
            }
            
            DataManager.Instance.UseHeart(nextHeartTime.ToString());
        }
    }

    private void LoadHeart()
    {
        string nextTimeStr = DataManager.Instance.playerData.nextHeartTime;
        if (!string.IsNullOrEmpty(nextTimeStr))
        {
            nextHeartTime = DateTime.Parse(nextTimeStr);
            
            // Tính toán bù năng lượng khi offline
            double secondsOffline = (DateTime.Now - nextHeartTime).TotalSeconds;
            if (secondsOffline > 0)
            {
                int energyToAdd = 1 + (int)(secondsOffline / restoreDuration);
                DataManager.Instance.AddHeart(energyToAdd, nextHeartTime.ToString());
                
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
