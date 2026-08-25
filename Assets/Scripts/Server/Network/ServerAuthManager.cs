using System.Text;
using UnityEngine;
using Unity.Netcode;

[RequireComponent(typeof(NetworkManager))]
public class ServerAuthManager : MonoBehaviour
{
    public static System.Collections.Generic.Dictionary<ulong, string> ClientUsernames = new System.Collections.Generic.Dictionary<ulong, string>();
    public static System.Collections.Generic.Dictionary<ulong, string> ClientRoomIds = new System.Collections.Generic.Dictionary<ulong, string>();

    public static string GetUsernameForClient(ulong clientId)
    {
        if (ClientUsernames.TryGetValue(clientId, out string username))
        {
            Debug.Log("GetUsernameForClient: " + clientId + " - " + username);
            return username;
        }
        return "";
    }

    private void Start()
    {
        NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnect;
    }

    private void OnClientDisconnect(ulong clientId)
    {
        if (ClientUsernames.ContainsKey(clientId))
            ClientUsernames.Remove(clientId);
        if (ClientRoomIds.ContainsKey(clientId))
            ClientRoomIds.Remove(clientId);
    }

    private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        response.Approved = false;
        response.CreatePlayerObject = false;

        byte[] payload = request.Payload;
        if (payload == null || payload.Length == 0)
        {
            Debug.LogWarning("[ServerAuth] Client connect without info.");
            response.Reason = "Missing credentials.";
            return;
        }

        string rawPayload = Encoding.UTF8.GetString(payload);
        string[] parts = rawPayload.Split('|');
        
        if (parts.Length < 2)
        {
            Debug.LogWarning("[ServerAuth] Payload format is invalid (Missing RoomId or Token).");
            response.Reason = "Invalid Auth Payload Format.";
            return;
        }

        string jwtToken = parts[0];
        string roomId = parts[1];

        if (JwtUtility.VerifyToken(jwtToken, out JwtPayload decodedPayload))
        {
            string username = decodedPayload.username;

            ClientUsernames[request.ClientNetworkId] = username;
            ClientRoomIds[request.ClientNetworkId] = roomId; 
            
            Debug.Log($"[ServerAuth] JWT Auth successful! Client {request.ClientNetworkId} ({username}) joined room: {roomId}");
            response.Approved = true;
            response.CreatePlayerObject = true; 
        }
        else
        {
            Debug.LogWarning($"[ServerAuth] Reject connection from {request.ClientNetworkId} - Invalid JWT Token.");
            response.Reason = "Invalid JWT Token.";
        }
    }
}
