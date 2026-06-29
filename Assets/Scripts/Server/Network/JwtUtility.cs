using System;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;

public static class JwtUtility
{
    // Bắt buộc phải giống hệt JWT_SECRET trong file .env của NodeJS
    private const string SECRET_KEY = "super_secret_juice_key_2026"; 

    /// <summary>
    /// Xác thực JWT Token và trả về thông tin Payload nếu hợp lệ.
    /// </summary>
    public static bool VerifyToken(string token, out JwtPayload decodedPayload)
    {
        decodedPayload = null;

        if (string.IsNullOrEmpty(token)) return false;

        string[] parts = token.Split('.');
        if (parts.Length != 3) return false;

        string header = parts[0];
        string payload = parts[1];
        string signature = parts[2];

        // 1. Kiểm tra chữ ký (Signature)
        string dataToSign = $"{header}.{payload}";
        string expectedSignature = ComputeHmacSha256(dataToSign, SECRET_KEY);

        if (signature != expectedSignature)
        {
            Debug.LogError("[JwtUtility] Chữ ký JWT không hợp lệ (Bị hack hoặc sai Secret Key)!");
            return false;
        }

        // 2. Giải mã Payload để lấy userId và username
        string decodedPayloadJson = DecodeBase64Url(payload);
        decodedPayload = JsonConvert.DeserializeObject<JwtPayload>(decodedPayloadJson);

        // 3. Kiểm tra Hạn sử dụng (Expiration)
        long currentTimeStamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        if (decodedPayload.exp < currentTimeStamp)
        {
            Debug.LogError("[JwtUtility] Token đã hết hạn!");
            return false;
        }

        return true;
    }

    private static string ComputeHmacSha256(string data, string secret)
    {
        byte[] keyBytes = Encoding.UTF8.GetBytes(secret);
        byte[] dataBytes = Encoding.UTF8.GetBytes(data);

        using (var hmac = new HMACSHA256(keyBytes))
        {
            byte[] hash = hmac.ComputeHash(dataBytes);
            return Base64UrlEncode(hash);
        }
    }

    private static string Base64UrlEncode(byte[] input)
    {
        return Convert.ToBase64String(input)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    private static string DecodeBase64Url(string input)
    {
        string base64 = input.Replace('-', '+').Replace('_', '/');
        switch (base64.Length % 4)
        {
            case 2: base64 += "=="; break;
            case 3: base64 += "="; break;
        }
        byte[] bytes = Convert.FromBase64String(base64);
        return Encoding.UTF8.GetString(bytes);
    }
}

[Serializable]
public class JwtPayload
{
    public int userId;
    public string username;
    public long iat; // Ngày tạo
    public long exp; // Ngày hết hạn
}
