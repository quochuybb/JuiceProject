using System;

public class AccountUser
{
    public int id { get; set; }
    public string username { get; set; }
    public string password_hash { get; set; }
    public int gold { get; set; }
    public int gem { get; set; }
    public int mmr { get; set; }
    public string session_data { get; set; }
    public DateTime created_at { get; set; }

    public AccountUser() { }
    
    public AccountUser(string _username, string _passwordHash, string _sessionData = "") 
    { 
        username = _username; 
        password_hash = _passwordHash;
        gold = 0;
        gem = 0;
        mmr = 1000;
        session_data = _sessionData;
    }
}