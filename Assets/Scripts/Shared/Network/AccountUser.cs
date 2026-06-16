using UnityEngine;

public class AccountUser
{
    public string Username { get; set; }
    
    public string PasswordHash { get; set; }
    
    public int MMR { get; set; }
    
    public string SessionDataJSON { get; set; }

    public AccountUser() { }
    
    public AccountUser(string username, string passwordHash, string displayName = "", string sessionDataJSON = "") 
    { 
        Username = username; 
        PasswordHash = passwordHash;
        MMR = 1000;
        SessionDataJSON = sessionDataJSON;

    }
}