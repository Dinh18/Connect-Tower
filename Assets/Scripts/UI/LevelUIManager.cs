using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class LevelUIManager : MonoBehaviour
{
    [Header("Scroll View Settings")]
    public ScrollRect scrollRect;
    public RectTransform contentRect;
    public int totalLevels = 1000;      // Tổng số Level của game
    public float itemHeight = 200f;     // Chiều cao 1 Level + Khoảng cách

    [Header("Prefab")]
    private GameObject levelPrefab;

    // --- CÁC BIẾN LƯU TRỮ HÌNH ẢNH ---
    private Sprite normalSprite;
    private Sprite hardSprite;
    private Sprite superSprite;
    private Sprite hardSkullSprite;
    private Sprite superSkullSprite;

    // --- CÁC BIẾN QUẢN LÝ CUỘN ---
    private List<RectTransform> spawnedItems = new List<RectTransform>();
    private int visibleItemsCount;
    private int lastFirstVisibleIndex = -1;

    void Awake()
    {
        levelPrefab = Resources.Load<GameObject>(Constants.LEVEL_PREFAB);
    }

    void Start()
    {
        InitRecycledScrollView();
    }

    void OnEnable()
    {
        Show();
    }

    public void FocusLevel(int level, bool instant = false)
    {
        // 1. Tính toán vị trí Y mục tiêu
        // Vì Level 1 ở Y=0, Level 2 ở Y=itemHeight... 
        // Để Level N nằm ở đáy Viewport, Content phải hạ xuống một khoảng (level-1)*itemHeight
        float targetY = -(level - 1) * itemHeight;

        // 2. Giới hạn (Clamp) để Content không bị trôi quá đà gây hở đáy hoặc hở đỉnh
        float minHeight = 0;
        float maxHeight = contentRect.rect.height - scrollRect.viewport.rect.height;
        
        // Lưu ý: Với hệ tọa độ ngược, ta clamp trong khoảng từ -maxHeight đến 0
        targetY = Mathf.Clamp(targetY, -maxHeight, minHeight);

        // 3. Thực hiện di chuyển
        if (instant)
        {
            contentRect.anchoredPosition = new Vector2(contentRect.anchoredPosition.x, targetY);
            UpdateVisibleItems(); // Cập nhật ngay lập tức các item hiển thị
        }
        else
        {
            // Dùng DOTween để cuộn mượt mà trong 0.5s
            contentRect.DOAnchorPosY(targetY, 0.5f)
                .SetEase(Ease.OutCubic)
                .OnUpdate(UpdateVisibleItems); // Cập nhật item liên tục khi đang cuộn
        }
    }

    // Gọi hàm này mỗi khi Menu được bật lên
    public void Show()
    {
        this.gameObject.SetActive(true);
        
        // Đợi 1 frame để Unity tính toán lại Layout trước khi Focus
        Canvas.ForceUpdateCanvases();
        
        int currentLvl = DataManager.Instance.playerData.currentLevel;
        FocusLevel(currentLvl, true); // Mặc định mở ra là thấy ngay (instant)
    }

    private void InitRecycledScrollView()
    {
        // 1. Kéo dài khung Content để chứa đủ 1000 level
        contentRect.sizeDelta = new Vector2(contentRect.sizeDelta.x, totalLevels * itemHeight);

        // 2. Tính số lượng nút cần đẻ ra (Vừa đủ che kín màn hình + 2 nút dự phòng)
        float viewportHeight = scrollRect.viewport.rect.height;
        visibleItemsCount = Mathf.CeilToInt(viewportHeight / itemHeight) + 2;

        // 3. Đẻ ra đúng số lượng nút đó (Ví dụ: 8-10 nút)
        for (int i = 0; i < visibleItemsCount; i++)
        {
            GameObject levelObj = Instantiate(levelPrefab, contentRect);
            RectTransform rect = levelObj.GetComponent<RectTransform>();

            rect.anchorMin = new Vector2(0.5f, 0f);
            rect.anchorMax = new Vector2(0.5f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            // Gắn reference cho Controller
            levelObj.GetComponent<LevelUIController>().SetUp(this);

            spawnedItems.Add(rect);
        }

        // 4. Lắng nghe sự kiện vuốt màn hình
        scrollRect.onValueChanged.AddListener(OnScroll);

        // Khởi động lần đầu
        UpdateVisibleItems();

        Canvas.ForceUpdateCanvases(); // Bắt Unity tính toán UI ngay lập tức
        scrollRect.verticalNormalizedPosition = 0f;
    }

    private void OnScroll(Vector2 scrollPos)
    {
        UpdateVisibleItems();
    }

    private void UpdateVisibleItems()
    {
        // Vị trí y hiện tại của Content
        float contentY = contentRect.anchoredPosition.y;

        float scrolledDistance = -contentY;

        // Tính xem Level nào đang nằm trên cùng màn hình
        int firstVisibleIndex = Mathf.FloorToInt(scrolledDistance / itemHeight);
        firstVisibleIndex = Mathf.Clamp(firstVisibleIndex, 0, totalLevels - visibleItemsCount);

        // Chống gọi hàm liên tục nếu chưa trượt qua 1 level mới
        if (firstVisibleIndex == lastFirstVisibleIndex) return;
        lastFirstVisibleIndex = firstVisibleIndex;

        // Lặp qua 10 nút đang có để cập nhật vị trí và Data
        for (int i = 0; i < visibleItemsCount; i++)
        {
            int dataIndex = firstVisibleIndex + i; 
            RectTransform itemRect = spawnedItems[i];

            if (dataIndex >= totalLevels)
            {
                itemRect.gameObject.SetActive(false);
                continue;
            }

            itemRect.gameObject.SetActive(true);

            // Xếp nút trượt dần xuống dưới (Y âm)
            itemRect.anchoredPosition = new Vector2(0, dataIndex * itemHeight);

            // 💡 GIẢ LẬP ĐỘ KHÓ (Bạn có thể thay bằng Data thực tế từ GameLoader)
            LevelLoader.GameDifficult difficult = GetMockDifficulty(dataIndex + 1);

            // Truyền Data (Level thực tế = dataIndex + 1)
            itemRect.GetComponent<LevelUIController>().ShowLevel(dataIndex + 1, difficult);
        }
    }

    // Hàm tạm để test độ khó: Cứ mỗi 5 level thì Hard, 10 level thì VeryHard
    private LevelLoader.GameDifficult GetMockDifficulty(int level)
    {
        if (level % 10 == 0) return LevelLoader.GameDifficult.VeryHard;
        if (level % 5 == 0) return LevelLoader.GameDifficult.Hard;
        return LevelLoader.GameDifficult.Easy;
    }


    public Sprite GetLevelSprite(LevelLoader.GameDifficult gameDifficult)
    {
        if (gameDifficult == LevelLoader.GameDifficult.Easy)
        {
            if (normalSprite == null) normalSprite = Resources.Load<Sprite>(Constants.NORMAL_LVL);
            return normalSprite;
        }
        else if (gameDifficult == LevelLoader.GameDifficult.Hard)
        {
            if (hardSprite == null) hardSprite = Resources.Load<Sprite>(Constants.HARD_LVL);
            return hardSprite;
        }
        else
        {
            if (superSprite == null) superSprite = Resources.Load<Sprite>(Constants.SUPERHARD_LVL);
            return superSprite;
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