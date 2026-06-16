using System;
using System.Collections.Generic;
using Firebase.Extensions;
using Firebase.Firestore;
using UnityEngine;

public class LeaderBoardPanel : Panel
{
    [Header("UI References")]
    [SerializeField] private RankItem rankItemPrefab;
    [SerializeField] private Transform top100Container;
    [SerializeField] private TopPlayerItem[] top3Items;
    // [SerializeField] private Transform curr_Player_Rank;
    private FirebaseFirestore db;
    void Start()
    {
        
    }

    public override void Setup(Menu menu)
    {
        if(db==null) db = FirebaseFirestore.DefaultInstance;
        foreach(Transform child in top100Container)
        {
            Destroy(child.gameObject);
        }
        FetchTop100();
        FetchTop3Players((top3List) => 
        {
            if (top3List != null && top3Items != null)
            {
                for (int i = 0; i < top3Items.Length; i++)
                {
                    if (top3Items[i] != null)
                    {
                        if (i < top3List.Count)
                        {
                            top3Items[i].Setup(top3List[i]);
                        }
                        else
                        {
                            top3Items[i].Setup(null);
                        }
                    }
                }
            }
        });
        // FetchExactMyRank();
    }

    public override void Show()
    {
        base.Show();
        this.gameObject.SetActive(true);
    }
    public override void Hide()
    {
        base.Hide();
        gameObject.SetActive(false);
    }
    public void FetchTop100()
    {
        // 1. Tạo câu truy vấn: Vào bảng Users -> Sắp xếp Level giảm dần -> Lấy 10 người
        Query top100Query = db.Collection("Users")
                              .OrderByDescending("currentLevel") 
                              .Limit(10);

        // 2. Thực thi tải dữ liệu
        top100Query.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("Lỗi tải Leaderboard: " + task.Exception);
                return;
            }

            QuerySnapshot snapshot = task.Result;
            int rank = 1;

            // 3. Duyệt qua từng người chơi lấy được
            foreach (DocumentSnapshot doc in snapshot.Documents)
            {
                if (doc.Exists)
                {
                    if (rank >= 4 && rank <= 10)
                    {
                        // Tự động ép kiểu JSON trên mạng về class PlayerData của bạn
                        PlayerData player = doc.ConvertTo<PlayerData>();
                        
                        RankItem rankPlayer = Instantiate(rankItemPrefab, top100Container);

                        bool isCurrPlayer = doc.Id == CoreServices.Get<DataManager>().UserId;
                        rankPlayer.Setup(rank, player, isCurrPlayer);
                    }

                    rank++;
                }
            }
        });
    }

    public void FetchTop3Players(Action<List<PlayerData>> onCallback)
    {
        Query top3Query = db.Collection("Users")
                            .OrderByDescending("currentLevel") 
                            .Limit(3);

        top3Query.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("Lỗi tải Top 3: " + task.Exception);
                onCallback?.Invoke(null);
                return;
            }

            QuerySnapshot snapshot = task.Result;
            List<PlayerData> top3List = new List<PlayerData>();

            foreach (DocumentSnapshot doc in snapshot.Documents)
            {
                if (doc.Exists)
                {
                    PlayerData player = doc.ConvertTo<PlayerData>();
                    top3List.Add(player);
                }
            }

            onCallback?.Invoke(top3List);
        });
    }

    // public void FetchExactMyRank()
    // {
    //     int myLevel = CoreServices.Get<DataManager>().GetCurrentLevel();

    //     // Đếm những người có level cao hơn mình
    //     Query countQuery = db.Collection("Users").WhereGreaterThan("currentLevel", myLevel);

    //     // AggregateQuery.Count() rất rẻ, đếm 1000 người cũng chỉ tính bằng 1 lượt Read
    //     countQuery.Count.GetSnapshotAsync(AggregateSource.Server).ContinueWithOnMainThread(task =>
    //     {
    //         if (task.IsFaulted) return;

    //         // Số người giỏi hơn mình
    //         long peopleBetterThanMe = task.Result.Count; 

    //         // Hạng của mình = Số người giỏi hơn + 1
    //         long myExactRank = peopleBetterThanMe + 1;

    //         PlayerData myLocalData = CoreServices.Get<DataManager>().playerData; 

    //         RankItem currPlayer =  Instantiate(rankItemPrefab, curr_Player_Rank);

    //         currPlayer.Setup((int)myExactRank, myLocalData,true);
    //     });
    // }
}   
