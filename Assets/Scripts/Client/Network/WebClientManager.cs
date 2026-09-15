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
    
    [Header("Account Login and Token Login")]
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
    
    public async Task<bool> RegisterAsync(string username, string password)
    {
        string url = $"{BaseUrl}/auth/register";
        string jsonPayload = JsonConvert.SerializeObject(new { username, password });

        using (UnityWebRequest request = UnityWebRequest.Post(url,jsonPayload,"application/json"))
        {
            request.timeout = 15;
            var operation = request.SendWebRequest();
            while (!operation.isDone) await Task.Yield();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("[WebClient] Register success!");
                return true;
            }
            else
            {
                Debug.LogError($"[WebClient] Register fail: {request.error} - {request.downloadHandler.text}");
                return false;
            }
        }
    }

    public async Task<bool> LoginAsync(string username, string password)
    {
        string url = $"{BaseUrl}/auth/login";
        string jsonPayload = JsonConvert.SerializeObject(new { username, password });

        using (UnityWebRequest request = UnityWebRequest.Post(url,jsonPayload,"application/json"))
        {
            request.timeout = 15;
            var operation = request.SendWebRequest();
            while (!operation.isDone) await Task.Yield();

            if (request.result == UnityWebRequest.Result.Success)
            {
                var responseJson = request.downloadHandler.text;
                var responseData = JsonConvert.DeserializeObject<LoginResponse>(responseJson);

                CurrentToken = responseData.token;
                CurrentUser = responseData.user;
                
                Debug.Log($"[WebClient] Login success!");
                return true;
            }
            else
            {
                Debug.LogError($"[WebClient] Login fail: {request.error} - {request.downloadHandler.text}");
                return false;
            }
        }
    }

    public async Task<bool> SaveProgressAsync(string sessionDataJSON)
    {
        if (string.IsNullOrEmpty(CurrentToken))
        {
            Debug.LogError("[WebClient] No Token, cannot save progress!");
            return false;
        }

        string url = $"{BaseUrl}/player/save";
        string jsonPayload = JsonConvert.SerializeObject(new { session_data = sessionDataJSON });

        using (UnityWebRequest request = UnityWebRequest.Post(url,jsonPayload,"application/json"))
        {
            request.timeout = 15;
            request.SetRequestHeader("Authorization", $"Bearer {CurrentToken}");

            var operation = request.SendWebRequest();
            while (!operation.isDone) await Task.Yield();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("[WebClient] Save progress success!");
                return true;
            }
            else
            {
                Debug.LogError($"[WebClient] Save progress fail: {request.error} - {request.downloadHandler.text}");
                return false;
            }
        }
    }

    public async Task<bool> LoadProgressAsync()
    {
        if (string.IsNullOrEmpty(CurrentToken))
        {
            Debug.LogError("[WebClient] No Token, cannot load progress!");
            return false;
        }

        string url = $"{BaseUrl}/player/me";

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            request.timeout = 15;
            request.SetRequestHeader("Authorization", $"Bearer {CurrentToken}");

            var operation = request.SendWebRequest();
            while (!operation.isDone) await Task.Yield();

            if (request.result == UnityWebRequest.Result.Success)
            {
                var responseJson = request.downloadHandler.text;
                CurrentUser = JsonConvert.DeserializeObject<AccountUser>(responseJson);
                
                if (!string.IsNullOrEmpty(CurrentUser.session_data))
                {
                    try
                    {
                        RecipeData[] allRecipeDatas = UnityEngine.Resources.LoadAll<RecipeData>("ScriptObjects/Recipes");
                        System.Collections.Generic.Dictionary<int, RecipeData> recipeDict = new System.Collections.Generic.Dictionary<int, RecipeData>();
                        foreach (var r in allRecipeDatas) recipeDict[r.recipeID] = r;

                        GameSessionData data = JsonConvert.DeserializeObject<GameSessionData>(CurrentUser.session_data);
                        data.UnpackToGameSession(recipeDict);

                        if (!string.IsNullOrEmpty(GameSession.CurrentChapterID))
                        {
                            var allChapters = UnityEngine.Resources.LoadAll<ChapterData>("ScriptObjects");
                            foreach (var c in allChapters)
                            {
                                if (c != null && c.chapterID == GameSession.CurrentChapterID)
                                {
                                    GameSession.CurrentChapterData = c;
                                    break;
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError("[WebClient] Error when unpacking session data: " + ex.Message);
                    }
                }

                Debug.Log($"[WebClient] Load progress successfully! Current Gold: {CurrentUser.gold}");
                return true;
            }
            else
            {
                Debug.LogError($"[WebClient] Error when loading game: {request.error} - {request.downloadHandler.text}");
                return false;
            }
        }
    }

    private async Task<bool> SendMatchCommandAsync(string endpoint)
    {
        if (string.IsNullOrEmpty(CurrentToken)) return false;
    
        string url = $"{BaseUrl}/match/{endpoint}";
        using (UnityWebRequest request = CreatePostRequest(url, "{}"))
        {
            request.timeout = 30;
            request.SetRequestHeader("Authorization", $"Bearer {CurrentToken}");
            var op = request.SendWebRequest();
            while (!op.isDone) await Task.Yield();
            return request.result == UnityWebRequest.Result.Success;
        }
    }

    public async Task<MatchStatusResponse> CheckStatusAsync()
    {
        if(string.IsNullOrEmpty(CurrentToken)) return null;

        string url = $"{BaseUrl}/match/status";
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            request.timeout = 30;
            request.SetRequestHeader("Authorization", $"Bearer {CurrentToken}");
            var op = request.SendWebRequest();
            while (!op.isDone) await Task.Yield();
            if (request.result == UnityWebRequest.Result.Success)
            {
                string json =  request.downloadHandler.text;
                return  JsonConvert.DeserializeObject<MatchStatusResponse>(json);
            }

            return null;
        }
    }

    public async Task<bool> FindMatchAsync()
    {
        return await SendMatchCommandAsync("find");
    }

    public async Task<bool> CancelMatchAsync()
    {
        return await SendMatchCommandAsync("cancel");
    }

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

[Serializable]
public class LoginResponse
{
    public string token;
    public AccountUser user;
}

[Serializable]
public class MatchStatusResponse
{
    public string status; 
    public string roomId;
    public string serverIp;
    public string serverPort;
    public string opponentName;
    public int opponentMMR;
}
