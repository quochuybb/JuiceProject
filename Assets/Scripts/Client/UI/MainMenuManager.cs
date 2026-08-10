using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    public static MainMenuManager Instance { get; private set; }
    [SerializeField] private float transitionDuration = 0.5f;
    [SerializeField] private Ease easeType = Ease.OutExpo;
    private Vector2 centerPosition = Vector2.zero;
    private Vector2 leftOffScreen = new Vector2(-1920f, 0f); 
    private Vector2 rightOffScreen = new Vector2(1920f, 0f);
    [SerializeField] private RectTransform mainMenuPanel;
    [SerializeField] private RectTransform campaignPanel;
    [SerializeField] private RectTransform mapPanel;
    [SerializeField] private RectTransform shopPanel;
    [SerializeField] private RectTransform signInPanel;
    [SerializeField] private TMP_InputField username;
    [SerializeField] private TMP_InputField password;
    [SerializeField] private TextMeshProUGUI coin;
    [SerializeField] private RectTransform gameMatchPanel;
    [SerializeField] private RectTransform rankingPanel;

    [Header("Matchmaking UI")]
    [SerializeField] private TextMeshProUGUI eloText;
    [SerializeField] private TextMeshProUGUI versusText;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        coin.text = GameSession.currentCoin.ToString();
        if (versusText != null) versusText.text = ""; // Trạng thái mặc định là rỗng

        // Nếu người chơi đã có Token rồi (quay lại từ scene Game),
        // bỏ qua màn hình SignIn và hiển thị thẳng Map Panel
        if (WebClientManager.Instance != null && !string.IsNullOrEmpty(WebClientManager.Instance.CurrentToken))
        {
            signInPanel.anchoredPosition = rightOffScreen;
            mainMenuPanel.anchoredPosition = rightOffScreen;
            campaignPanel.anchoredPosition = rightOffScreen;
            shopPanel.anchoredPosition = rightOffScreen;
            mapPanel.anchoredPosition = centerPosition;

            // Vẽ lại bản đồ Chapter đang chơi
            if (MapManager.Instance != null)
            {
                MapManager.Instance.ReloadCurrentMap();
            }
        }
    }

    private void OnEnable()
    {
        // Lót dép ngồi nghe: Nếu ConnectionManager hét lên "Thành công", lập tức chạy hàm OnSignInSuccess
        ConnectionManager.OnLoginSuccess += OnSignInSuccess;
        GameSession.OnCoinChanged += UpdateCoinDisplay;
    }

    private void OnDisable()
    {
        // Khi UI này bị tắt hoặc bị xóa, phải hủy đăng ký để tránh lỗi tràn RAM
        ConnectionManager.OnLoginSuccess -= OnSignInSuccess;
        GameSession.OnCoinChanged -= UpdateCoinDisplay;
    }

    public void UpdateCoinDisplay()
    {
        if (coin != null)
        {
            coin.text = GameSession.currentCoin.ToString();
        }
    }

    public void UpdateEloDisplay()
    {
        if (eloText != null && WebClientManager.Instance != null && WebClientManager.Instance.CurrentUser != null)
        {
            eloText.text = $"Elo Point: {WebClientManager.Instance.CurrentUser.mmr}";
        }
    }

    public void OnStartServer()
    {
        ConnectionManager.Instance.StartDedicatedServer();
    }
    public async void OnSignIn()
    {
        if (username.text == "")
        {
            username.text = "admin";
        }

        if (password.text == "")
        {
            password.text = "admin123";
        }

        // Đợi Web API xử lý Đăng nhập
        bool isSuccess = await WebClientManager.Instance.LoginAsync(username.text, password.text);
        
        // Nếu Đăng nhập thất bại (Có thể do chưa có tài khoản)
        if (!isSuccess)
        {
            Debug.Log("[MainMenu] Đăng nhập thất bại, đang thử Tự động Đăng ký tài khoản mới...");
            bool isRegisterSuccess = await WebClientManager.Instance.RegisterAsync(username.text, password.text);
            
            if (isRegisterSuccess)
            {
                Debug.Log("[MainMenu] Đăng ký thành công! Tiến hành đăng nhập lại...");
                // Đăng nhập lại lần nữa
                isSuccess = await WebClientManager.Instance.LoginAsync(username.text, password.text);
            }
            else
            {
                Debug.LogError("[MainMenu] Tự động đăng ký thất bại. Xin vui lòng kiểm tra lại mạng hoặc tài khoản.");
                return;
            }
        }

        // Nếu API trả về true (Đăng nhập đúng)
        if (isSuccess) 
        {
            // Cập nhật Vàng/Ngọc hiển thị trên UI từ dữ liệu mới lấy về
            GameSession.currentCoin = WebClientManager.Instance.CurrentUser.gold;
            UpdateCoinDisplay();
            UpdateEloDisplay();

            // Phục hồi dữ liệu túi đồ (Inventory, Recipe, Map) từ JSON
            string sessionDataJson = WebClientManager.Instance.CurrentUser.session_data;
            if (!string.IsNullOrEmpty(sessionDataJson))
            {
                try
                {
                    // Tải toàn bộ danh sách đồ trong game để tra cứu ID
                    RecipeData[] allRecipeDatas = UnityEngine.Resources.LoadAll<RecipeData>("ScriptObjects/Recipes");
                    System.Collections.Generic.Dictionary<int, RecipeData> recipeDict = new System.Collections.Generic.Dictionary<int, RecipeData>();
                    foreach (var r in allRecipeDatas)
                    {
                        recipeDict[r.recipeID] = r;
                    }

                    // Giải nén JSON vào GameSession
                    GameSessionData data = Newtonsoft.Json.JsonConvert.DeserializeObject<GameSessionData>(sessionDataJson);
                    data.UnpackToGameSession(recipeDict);
                    
                    Debug.Log($"[MainMenu] Phục hồi túi đồ thành công! Túi đồ: {GameSession.inventoryList.Count} món, Đang mặc: {GameSession.recipeList.Count} món.");
                }
                catch (System.Exception ex)
                {
                    Debug.LogError("[MainMenu] Lỗi giải mã Session Data: " + ex.Message);
                }
            }

            // Trượt màn hình sang Main Menu
            OnSignInSuccess();
        }
    }
    public void OnSignInSuccess()
    {
        Debug.Log("sign in success");
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlayUIClick();
            
        // Trượt Panel Đăng Nhập ra ngoài, Trượt Panel Sảnh (Main Menu) vào giữa
        SlidePanel(signInPanel, mainMenuPanel);
    }
    public void OnPlayChapterButton()
    {
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlayUIClick();
        Debug.Log("Open Chapter");
        SlidePanel(campaignPanel, mapPanel);
    }
    public void OnShopButton()
    {
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlayUIClick();
        Debug.Log("Open Shop");
        
        // Cập nhật lại giao diện của các nút mua đồ dựa trên Data mới nhất tải từ Server
        if (RecipeManager.instance != null)
        {
            RecipeManager.instance.RefreshAllRecipes();
        }

        SlidePanel(mainMenuPanel, shopPanel);
    }

    public void OnPlayCampaignButton()
    {
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlayUIClick();
        Debug.Log("Open Campaign");
        SlidePanel(mainMenuPanel, campaignPanel);
    }

    public void OnBackMainMenuFromCampaignButton()
    {
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlayUIClick();
        Debug.Log("Back to Main Menu");
        SlidePanel(campaignPanel,mainMenuPanel);
    }
    public void OnBackMainMenuFromShopButton()
    {
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlayUIClick();
        Debug.Log("Back to Main Menu");
        SlidePanel(shopPanel,mainMenuPanel);
    }
    
    public void OnRankingButton()
    {
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlayUIClick();
        Debug.Log("Open Ranking");
        SlidePanel(mainMenuPanel, rankingPanel);
    }

    public void OnBackMainMenuFromRankingButton()
    {
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlayUIClick();
        Debug.Log("Back to Main Menu from Ranking");
        SlidePanel(rankingPanel, mainMenuPanel);
    }
    public void OnBackCampaignFromChapterButton()
    {
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlayUIClick();
        Debug.Log("Back to Main Menu");
        SlidePanel(mapPanel,campaignPanel);
    }
    public async void OnPlayMultiplayerButton()
    {
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlayUIClick();

        Debug.Log("[Matchmaking] Đang gửi yêu cầu tìm trận...");
        if (versusText != null)
            versusText.text = "Matching...";
            
        bool success = await WebClientManager.Instance.FindMatchAsync();
        
        if (success)
        {
            Debug.Log("[Matchmaking] Đã vào hàng chờ thành công! Bắt đầu quét trạng thái...");
            StartCoroutine(CheckMatchStatusRoutine());
        }
        else
        {
            if (versusText != null)
                versusText.text = "Error connecting to server";
                
            // Chờ 2 giây rồi xóa chữ
            await System.Threading.Tasks.Task.Delay(2000);
            if (versusText != null)
                versusText.text = "";
        }
    }

    private System.Collections.IEnumerator CheckMatchStatusRoutine()
    {
        bool isMatched = false;
        
        while (!isMatched)
        {
            yield return new WaitForSeconds(2.0f); 
            
            var task = WebClientManager.Instance.CheckStatusAsync();
            yield return new WaitUntil(() => task.IsCompleted);

            var response = task.Result;
            
            if (response != null && response.status == "match_found")
            {
                isMatched = true; 
                Debug.Log($"[Matchmaking] YAY! Đã tìm thấy trận! Room ID: {response.roomId}");
                
                // Đổi chữ "Matching..." thành tên đối thủ
                if (versusText != null)
                    versusText.text = $"{response.opponentName}";

                // TRƯỢT SANG MÀN HÌNH ĐẤU (gameMatchPanel) TỪ MÀN HÌNH RANKING
                SlidePanel(rankingPanel, gameMatchPanel);

                // Chờ 2 giây để người chơi xem hoạt ảnh trượt và nhìn thấy tên đối thủ rồi mới kết nối
                yield return new WaitForSeconds(2.0f);

                // Kết nối vào Server Game bằng chuỗi Token + "|" + roomId
                string authPayload = $"{WebClientManager.Instance.CurrentToken}|{response.roomId}";
                ConnectionManager.Instance.StartClient(authPayload);
            }
            else
            {
                Debug.Log("[Matchmaking] Vẫn đang tìm kiếm...");
            }
        }
    }
    public void OnSettingButton()
    {
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlayUIClick();

        Debug.Log("Open Settings");
    }
    public void SlidePanel(RectTransform panelOld, RectTransform panelNew)
    {
        panelOld.DOAnchorPos(rightOffScreen, transitionDuration)
            .SetEase(easeType);

        panelNew.DOAnchorPos(centerPosition, transitionDuration)
            .SetEase(easeType);
    } 
    public void OnQuitButton()
    {
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlayUIClick();

        if (Unity.Netcode.NetworkManager.Singleton != null && Unity.Netcode.NetworkManager.Singleton.IsClient)
        {
            if (Unity.Netcode.NetworkManager.Singleton.LocalClient != null && Unity.Netcode.NetworkManager.Singleton.LocalClient.PlayerObject != null)
            {
                var localPlayer = Unity.Netcode.NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<NetworkPlayer>();
                if (localPlayer != null)
                {
                    localPlayer.SaveProgress();
                    StartCoroutine(QuitAfterDelay(0.5f));
                    return;
                }
            }
        }
        
        DoQuit();
    }

    private System.Collections.IEnumerator QuitAfterDelay(float delay)
    {
        // Chờ một chút để Server RPC kịp gửi đi
        yield return new WaitForSeconds(delay);
        DoQuit();
    }

    private void DoQuit()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    public void GoToMapFromLoad()
    {
        SlidePanel(mainMenuPanel, mapPanel);
        if (MapManager.Instance != null)
        {
            MapManager.Instance.ReloadCurrentMap();
        }
    }
}