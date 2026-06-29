using System;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;

public class WebClientManager : MonoBehaviour
{
    public static WebClientManager Instance { get; private set; }

    [Header("API Config")]
    public string BaseUrl = "http://localhost:3000/api";
    
    [Header("Tài khoản đang đăng nhập")]
    public string CurrentToken;
    public AccountUser CurrentUser;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Đăng ký tài khoản
    /// </summary>
    public async Task<bool> RegisterAsync(string username, string password)
    {
        string url = $"{BaseUrl}/auth/register";
        string jsonPayload = JsonConvert.SerializeObject(new { username, password });

        using (UnityWebRequest request = CreatePostRequest(url, jsonPayload))
        {
            var operation = request.SendWebRequest();
            while (!operation.isDone) await Task.Yield();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("[WebClient] Đăng ký thành công!");
                return true;
            }
            else
            {
                Debug.LogError($"[WebClient] Đăng ký lỗi: {request.error} - {request.downloadHandler.text}");
                return false;
            }
        }
    }

    /// <summary>
    /// Đăng nhập và nhận JWT Token
    /// </summary>
    public async Task<bool> LoginAsync(string username, string password)
    {
        string url = $"{BaseUrl}/auth/login";
        string jsonPayload = JsonConvert.SerializeObject(new { username, password });

        using (UnityWebRequest request = CreatePostRequest(url, jsonPayload))
        {
            var operation = request.SendWebRequest();
            while (!operation.isDone) await Task.Yield();

            if (request.result == UnityWebRequest.Result.Success)
            {
                // Phân tích kết quả JSON trả về
                var responseJson = request.downloadHandler.text;
                var responseData = JsonConvert.DeserializeObject<LoginResponse>(responseJson);

                // Lưu Token và Thông tin User
                CurrentToken = responseData.token;
                CurrentUser = responseData.user;
                
                Debug.Log($"[WebClient] Đăng nhập thành công! Lấy được Token. Vàng: {CurrentUser.gold}");
                return true;
            }
            else
            {
                Debug.LogError($"[WebClient] Đăng nhập lỗi: {request.error} - {request.downloadHandler.text}");
                return false;
            }
        }
    }

    /// <summary>
    /// Lưu tiến trình lên Server
    /// </summary>
    public async Task<bool> SaveProgressAsync(string sessionDataJSON)
    {
        if (string.IsNullOrEmpty(CurrentToken))
        {
            Debug.LogError("[WebClient] Không có Token, không thể Save!");
            return false;
        }

        string url = $"{BaseUrl}/player/save";
        string jsonPayload = JsonConvert.SerializeObject(new { session_data = sessionDataJSON });

        using (UnityWebRequest request = CreatePostRequest(url, jsonPayload))
        {
            request.SetRequestHeader("Authorization", $"Bearer {CurrentToken}");

            var operation = request.SendWebRequest();
            while (!operation.isDone) await Task.Yield();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("[WebClient] Đã lưu tiến trình thành công lên Web API!");
                return true;
            }
            else
            {
                Debug.LogError($"[WebClient] Lỗi lưu game: {request.error} - {request.downloadHandler.text}");
                return false;
            }
        }
    }

    /// <summary>
    /// Tải lại tiến trình từ Server (Dùng khi đã có Token)
    /// </summary>
    public async Task<bool> LoadProgressAsync()
    {
        if (string.IsNullOrEmpty(CurrentToken))
        {
            Debug.LogError("[WebClient] Không có Token, yêu cầu Đăng nhập lại!");
            return false;
        }

        string url = $"{BaseUrl}/player/me";

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            request.SetRequestHeader("Authorization", $"Bearer {CurrentToken}");

            var operation = request.SendWebRequest();
            while (!operation.isDone) await Task.Yield();

            if (request.result == UnityWebRequest.Result.Success)
            {
                var responseJson = request.downloadHandler.text;
                CurrentUser = JsonConvert.DeserializeObject<AccountUser>(responseJson);
                
                // Giải nén JSON vào GameSession
                if (!string.IsNullOrEmpty(CurrentUser.session_data))
                {
                    try
                    {
                        RecipeData[] allRecipeDatas = UnityEngine.Resources.LoadAll<RecipeData>("ScriptObjects/Recipes");
                        System.Collections.Generic.Dictionary<int, RecipeData> recipeDict = new System.Collections.Generic.Dictionary<int, RecipeData>();
                        foreach (var r in allRecipeDatas) recipeDict[r.recipeID] = r;

                        GameSessionData data = JsonConvert.DeserializeObject<GameSessionData>(CurrentUser.session_data);
                        data.UnpackToGameSession(recipeDict);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError("[WebClient] Lỗi giải mã Session Data lúc LoadProgress: " + ex.Message);
                    }
                }

                Debug.Log($"[WebClient] Tải tiến trình thành công! Vàng hiện tại: {CurrentUser.gold}");
                return true;
            }
            else
            {
                Debug.LogError($"[WebClient] Lỗi tải game: {request.error} - {request.downloadHandler.text}");
                return false;
            }
        }
    }

    // --- Hàm tiện ích tạo UnityWebRequest POST ---
    private UnityWebRequest CreatePostRequest(string url, string jsonPayload)
    {
        UnityWebRequest request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonPayload);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        return request;
    }
}

// Cấu trúc Data để hứng cục JSON từ API Login trả về
[Serializable]
public class LoginResponse
{
    public string token;
    public AccountUser user;
}
