using UnityEngine;

public class AppBootstrapper : MonoBehaviour
{
    [Header("Core Systems")]
    [SerializeField] private DataManager dataManager;
    [SerializeField] private GameManager gameManager;
    [SerializeField] private UIManager uiManager;
    [SerializeField] private TutorialUIController tutorialUIController;
    
    [Header("Gameplay Systems")]
    [SerializeField] private LevelLoader levelLoader;
    [SerializeField] private SlotsManager slotsManager;
    [SerializeField] private BlocksManager blocksManager;
    [SerializeField] private HeartManager heartManager;
    [SerializeField] private CameraController cameraController;
    [SerializeField] private BoosterManager boosterManager;

    void Awake()
    {
        // THỨ TỰ KHỞI TẠO CỰC KỲ QUAN TRỌNG

        // 1. Dữ liệu (Phải có đầu tiên)
        dataManager.Init();
        
        // 2. Các Manager Gameplay đơn lẻ
        CoreServices.Register(slotsManager);
        CoreServices.Register(blocksManager);
        CoreServices.Register(heartManager);
        CoreServices.Register(cameraController);
        CoreServices.Register(tutorialUIController);
        
        // 3. Level Loader (Cần data và các manager gameplay)
        levelLoader.Init(slotsManager, blocksManager, gameManager, dataManager);
        
        // 4. Game Manager (Đầu não điều phối)
        gameManager.Init(slotsManager, heartManager, cameraController, levelLoader);
        
        // 5. UI (Giao diện hiển thị)
        uiManager.Init(gameManager, dataManager);
        
        // 6. Booster Manager
        CoreServices.Register(boosterManager);

        Debug.Log("<color=green><b>[AppBootstrapper]</b></color> Toàn bộ hệ thống đã được liên kết thành công!");
    }

    void Start()
    {
        // Mọi thứ đã sẵn sàng, bắt đầu game
        gameManager.StartGame();
    }
}
