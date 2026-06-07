using UnityEngine;
using SQLite;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;

public class DatabaseManager : MonoBehaviour
{
    public static DatabaseManager Instance { get; private set; }

    private SQLiteConnection _db;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        InitDatabase();
    }

    private void InitDatabase()
    {
        // Lưu file database ở thư mục an toàn của server
        string dbPath = Path.Combine(Application.persistentDataPath, "ServerData.db");
        _db = new SQLiteConnection(dbPath);
        
        // Tạo bảng nếu chưa có
        _db.CreateTable<AccountUser>();
        
        Debug.Log($"[Database] SQLite initialized at: {dbPath}");
    }

    public bool CreateAccount(string username, string password)
    {
        var existingUser = _db.Find<AccountUser>(username);
        if (existingUser != null)
        {
            Debug.LogWarning($"[Database] Tên đăng nhập '{username}' đã tồn tại!");
            return false;
        }

        string hash = HashPassword(password);
        var newUser = new AccountUser(username, hash);
        
        _db.Insert(newUser);
        Debug.Log($"[Database] Đã tạo tài khoản thành công: {username}");
        return true;
    }

    public AccountUser GetAccount(string username)
    {
        return _db.Find<AccountUser>(username);
    }

    public bool VerifyAccount(string username, string password, out AccountUser user)
    {
        user = _db.Find<AccountUser>(username);
        if (user == null)
        {
            Debug.LogWarning($"[Database] Không tìm thấy tài khoản: {username}");
            return false;
        }

        string hash = HashPassword(password);
        if (user.PasswordHash == hash)
        {
            Debug.Log($"[Database] Đăng nhập thành công: {username}");
            return true;
        }
        
        Debug.LogWarning($"[Database] Sai mật khẩu cho: {username}");
        return false;
    }
    
    public void UpdateMMR(string username, int newMMR)
    {
        var user = _db.Find<AccountUser>(username);
        if (user != null)
        {
            user.MMR = newMMR;
            _db.Update(user);
        }
    }

    public void SaveProgress(string username, GameSessionData sessionData)
    {
        var user = _db.Find<AccountUser>(username);
        if (user != null)
        {
            user.SessionDataJSON = JsonConvert.SerializeObject(sessionData);
            _db.Update(user);
            Debug.Log($"[Database] Đã lưu tiến trình cho {username}");
        }
    }

    public GameSessionData LoadProgress(string username)
    {
        var user = _db.Find<AccountUser>(username);
        if (user != null && !string.IsNullOrEmpty(user.SessionDataJSON))
        {
            try
            {
                return JsonConvert.DeserializeObject<GameSessionData>(user.SessionDataJSON);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[Database] Lỗi khi đọc JSON tiến trình: {e.Message}");
            }
        }
        return null;
    }

    // Hàm băm mật khẩu cơ bản (Nên đổi sang PBKDF2/BCrypt nếu dùng thực tế)
    private string HashPassword(string password)
    {
        using (SHA256 sha256Hash = SHA256.Create())
        {
            byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(password));
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < bytes.Length; i++)
            {
                builder.Append(bytes[i].ToString("x2"));
            }
            return builder.ToString();
        }
    }

    private void OnDestroy()
    {
        if (_db != null)
        {
            _db.Close();
        }
    }
}
