/*
using UnityEngine;
using Npgsql;
using Dapper;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using System.Data;

public class DatabaseManager : MonoBehaviour
{
    public static DatabaseManager Instance { get; private set; }

    [Header("PostgreSQL Settings")]
    public string Host = "localhost";
    public int Port = 5051;
    public string Username = "admin";
    public string Password = "adminpassword123";
    public string Database = "juicematch_db";

    private string _connectionString;

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

    private IDbConnection GetConnection()
    {
        return new NpgsqlConnection(_connectionString);
    }

    private void InitDatabase()
    {
        _connectionString = $"Host={Host};Port={Port};Username={Username};Password={Password};Database={Database};";

        using (var db = GetConnection())
        {
            db.Open();
            
            // Tạo bảng nếu chưa có (PostgreSQL syntax)
            string createTableQuery = @"
                CREATE TABLE IF NOT EXISTS users (
                    id SERIAL PRIMARY KEY,
                    username VARCHAR(50) UNIQUE NOT NULL,
                    password_hash VARCHAR(255) NOT NULL,
                    gold INT DEFAULT 0,
                    gem INT DEFAULT 0,
                    mmr INT DEFAULT 1000,
                    session_data TEXT,
                    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
                );";
            
            db.Execute(createTableQuery);
        }
        
        Debug.Log($"[Database] PostgreSQL initialized at {Host}:{Port}");
    }

    public bool CreateAccount(string username, string password)
    {
        using (var db = GetConnection())
        {
            var existingUser = db.QueryFirstOrDefault<AccountUser>("SELECT * FROM users WHERE username = @Username", new { Username = username });
            
            if (existingUser != null)
            {
                Debug.LogWarning($"[Database] Tên đăng nhập '{username}' đã tồn tại!");
                return false;
            }

            string hash = HashPassword(password);
            
            string insertQuery = @"
                INSERT INTO users (username, password_hash, gold, gem, mmr, session_data) 
                VALUES (@Username, @PasswordHash, 0, 0, 1000, @SessionData)";
                
            db.Execute(insertQuery, new { 
                Username = username, 
                PasswordHash = hash, 
                SessionData = "" 
            });

            Debug.Log($"[Database] Đã tạo tài khoản thành công: {username}");
            return true;
        }
    }

    public AccountUser GetAccount(string username)
    {
        using (var db = GetConnection())
        {
            return db.QueryFirstOrDefault<AccountUser>("SELECT * FROM users WHERE username = @Username", new { Username = username });
        }
    }

    public bool VerifyAccount(string username, string password, out AccountUser user)
    {
        user = GetAccount(username);
        
        if (user == null)
        {
            Debug.LogWarning($"[Database] Không tìm thấy tài khoản: {username}");
            return false;
        }

        string hash = HashPassword(password);
        if (user.password_hash == hash)
        {
            Debug.Log($"[Database] Đăng nhập thành công: {username}");
            return true;
        }
        
        Debug.LogWarning($"[Database] Sai mật khẩu cho: {username}");
        return false;
    }
    
    public void UpdateMMR(string username, int newMMR)
    {
        using (var db = GetConnection())
        {
            db.Execute("UPDATE users SET mmr = @MMR WHERE username = @Username", new { MMR = newMMR, Username = username });
        }
    }

    public void SaveProgress(string username, GameSessionData sessionData)
    {
        Debug.Log("Save Process");
        Debug.Log(username);
        Debug.Log(sessionData.ToString());
        string json = JsonConvert.SerializeObject(sessionData);
        using (var db = GetConnection())
        {
            db.Execute("UPDATE users SET session_data = @JSON WHERE username = @Username", new { JSON = json, Username = username });
            Debug.Log($"[Database] Đã lưu tiến trình cho {username}");
        }
    }

    public GameSessionData LoadProgress(string username)
    {
        var user = GetAccount(username);
        if (user != null && !string.IsNullOrEmpty(user.session_data))
        {
            try
            {
                return JsonConvert.DeserializeObject<GameSessionData>(user.session_data);
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
}*/
