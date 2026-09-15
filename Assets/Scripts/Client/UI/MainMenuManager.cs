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
        if (versusText != null) versusText.text = "";
        
        if (WebClientManager.Instance != null && !string.IsNullOrEmpty(WebClientManager.Instance.CurrentToken))
        {
            signInPanel.anchoredPosition = rightOffScreen;
            mainMenuPanel.anchoredPosition = rightOffScreen;
            campaignPanel.anchoredPosition = rightOffScreen;
            shopPanel.anchoredPosition = rightOffScreen;
            mapPanel.anchoredPosition = centerPosition;

            if (MapManager.Instance != null)
            {
                MapManager.Instance.ReloadCurrentMap();
            }
        }
    }

    private void OnEnable()
    {
        GameSession.OnCoinChanged += UpdateCoinDisplay;
    }

    private void OnDisable()
    {
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

        bool isSuccess = await WebClientManager.Instance.LoginAsync(username.text, password.text);
        
        if (!isSuccess)
        {
            Debug.Log("[MainMenu] Login fail, Trying to register new account...");
            bool isRegisterSuccess = await WebClientManager.Instance.RegisterAsync(username.text, password.text);
            
            if (isRegisterSuccess)
            {
                Debug.Log("[MainMenu] Register succeess, Login again...");
                isSuccess = await WebClientManager.Instance.LoginAsync(username.text, password.text);
            }
            else
            {
                Debug.LogError("[MainMenu] Register fail,Please try again.");
                return;
            }
        }

        if (isSuccess) 
        {
            GameSession.currentCoin = WebClientManager.Instance.CurrentUser.gold;
            UpdateCoinDisplay();
            UpdateEloDisplay();

            bool isLoadSuccess = await WebClientManager.Instance.LoadProgressAsync();
            if (isLoadSuccess)
            {
                Debug.Log($"[MainMenu] Restore Inventory Success! Inventory: {GameSession.inventoryList.Count} items, Equipped: {GameSession.recipeList.Count} items.");
            }
            else
            {
                Debug.LogWarning("[MainMenu] Load progress failed, but proceeding anyway.");
            }

            OnSignInSuccess();
        }
    }
    public void OnSignInSuccess()
    {
        Debug.Log("Sign in success");
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlayUIClick();
            
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
            Debug.Log(response.status);
            if (response != null && response.status == "match_found")
            {
                isMatched = true; 
                Debug.Log($"[Matchmaking] Match found! Room ID: {response.roomId}");
                
                if (versusText != null)
                    versusText.text = $"{response.opponentName}";

                SlidePanel(rankingPanel, gameMatchPanel);

                yield return new WaitForSeconds(2.0f);

                string authPayload = $"{WebClientManager.Instance.CurrentToken}|{response.roomId}";
                ConnectionManager.Instance.StartClient(authPayload);
            }
            else
            {
                Debug.Log("[Matchmaking] Finding Match...");
            }
        }
    }
    public void OnSettingButton()
    {
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlayUIClick();

        Debug.Log("Open Settings");
    }
    public void OnReturnToRankingFromMatch()
    {
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlayUIClick();

        SlidePanel(gameMatchPanel, rankingPanel);

        if (eloText != null && WebClientManager.Instance != null && WebClientManager.Instance.CurrentUser != null)
        {
            eloText.text = $"Elo Point: {WebClientManager.Instance.CurrentUser.mmr}";
        }
    }

    public void SlidePanel(RectTransform panelOld, RectTransform panelNew)
    {
        panelOld.DOAnchorPos(rightOffScreen, transitionDuration)
            .SetEase(easeType);

        panelNew.DOAnchorPos(centerPosition, transitionDuration)
            .SetEase(easeType);
    } 
    public async void OnQuitButton()
    {
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlayUIClick();

        Debug.Log("[Client] Saving Progress before quitting...");
        GameSessionData data = new GameSessionData();
        data.PackFromGameSession();
        string jsonPayload = Newtonsoft.Json.JsonConvert.SerializeObject(data);
        
        if (WebClientManager.Instance != null)
        {
            await WebClientManager.Instance.SaveProgressAsync(jsonPayload);
        }
        
        DoQuit();
    }

    private void OnApplicationQuit()
    {
        GameSessionData data = new GameSessionData();
        data.PackFromGameSession();
        string jsonPayload = Newtonsoft.Json.JsonConvert.SerializeObject(data);
        
        if (WebClientManager.Instance != null)
        {
            _ = WebClientManager.Instance.SaveProgressAsync(jsonPayload);
        }
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