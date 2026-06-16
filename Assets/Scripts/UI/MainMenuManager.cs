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

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        coin.text = GameSession.currentCoin.ToString();

        // Nếu người chơi đã đăng nhập rồi (quay lại từ scene Game),
        // bỏ qua màn hình SignIn và hiển thị thẳng Map Panel
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsClient)
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

    public void OnStartServer()
    {
        ConnectionManager.Instance.StartDedicatedServer();
    }
    public void OnSignIn()
    {
        if (username.text == "")
        {
            username.text = "admin";
        }

        if (password.text == "")
        {
            password.text = "admin123";
        }
        ConnectionManager.Instance.StartClient(username.text, password.text);
        ;
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
    public void OnBackCampaignFromChapterButton()
    {
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlayUIClick();
        Debug.Log("Back to Main Menu");
        SlidePanel(mapPanel,campaignPanel);
    }
    public void OnPlayMultiplayerButton()
    {
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlayUIClick();

        Debug.Log("Open Lobby");
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
}