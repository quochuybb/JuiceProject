using System;
using UnityEngine;
using Unity.Netcode;

public partial class NetworkPlayer : NetworkBehaviour
{
    public static NetworkPlayer LocalInstance { get; private set; }

    public NetworkVariable<int> PlayerMMR = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    
    public NetworkVariable<Unity.Collections.FixedString32Bytes> PlayerUsername = new NetworkVariable<Unity.Collections.FixedString32Bytes>("", NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public static event Action<NetworkPlayer> OnServerPlayerSpawned;
    public static event Action<NetworkPlayer> OnServerPlayerDespawned;
    public static event Action<NetworkPlayer, string> OnServerSaveProgressRequested;

    public static event Action<int> OnClientGameStarted;
    public static event Action<int, int> OnClientHPUpdated;
    public static event Action<bool, int> OnClientMatchEnded;
    public static event Action<ulong, int> OnServerAttackReceived;

    public override void OnNetworkSpawn()
    {
        DontDestroyOnLoad(gameObject);

        if (IsServer)
        {
            OnServerPlayerSpawned?.Invoke(this);
        }
        else if (IsOwner)
        {
            LocalInstance = this;
            Debug.Log($"[Client] Connected with Username: {PlayerUsername.Value}");
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer)
        {
            OnServerPlayerDespawned?.Invoke(this);
        }
        else if (IsOwner)
        {
            LocalInstance = null;
        }
    }

    [ClientRpc]
    public void RpcStartGameClientRpc(int boardSeed, ClientRpcParams clientRpcParams = default)
    {
        if (IsOwner)
        {
            Debug.Log($"[Client] START GAME! Board Seed: {boardSeed}");
            OnClientGameStarted?.Invoke(boardSeed);
        }
    }

    [ServerRpc]
    public void CmdAttackServerRpc(int damageAmount)
    {
        OnServerAttackReceived?.Invoke(OwnerClientId, damageAmount);
    }

    [ClientRpc]
    public void RpcUpdateHPClientRpc(int myHP, int opponentHP, ClientRpcParams clientRpcParams = default)
    {
        if (IsOwner)
        {
            OnClientHPUpdated?.Invoke(myHP, opponentHP);
        }
    }

    [ClientRpc]
    public void RpcEndMatchClientRpc(bool isWinner, int newMmr, ClientRpcParams clientRpcParams = default)
    {
        if (IsOwner)
        {
            OnClientMatchEnded?.Invoke(isWinner, newMmr);
        }
    }
    
    [ServerRpc]
    public void CmdSaveProgressServerRpc(string sessionJson)
    {
        OnServerSaveProgressRequested?.Invoke(this, sessionJson);
    }
    
    [ClientRpc]
    public void RpcLoadSessionClientRpc(string sessionJson)
    {
        if (IsOwner)
        {
            Debug.Log($"[Client] Received Save Data from Server. Unpacking...");
            var data = Newtonsoft.Json.JsonConvert.DeserializeObject<GameSessionData>(sessionJson);
            
            var allRecipesList = Resources.LoadAll<RecipeData>("ScriptObjects/Recipes");
            var allRecipesDictionary = new System.Collections.Generic.Dictionary<int, RecipeData>();
            foreach (var r in allRecipesList)
            {
                if (r != null) allRecipesDictionary[r.recipeID] = r;
            }

            data.UnpackToGameSession(allRecipesDictionary);

            if (!string.IsNullOrEmpty(GameSession.CurrentChapterID))
            {
                var allChapters = Resources.LoadAll<ChapterData>("ScriptObjects");
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
    }


    
    public void SaveProgress()
    {
        if (!IsOwner) return;

        Debug.Log("[Client] Starting to pack GameSession Data to save to Server...");
        
        GameSessionData data = new GameSessionData();
        data.PackFromGameSession();
        
        string json = Newtonsoft.Json.JsonConvert.SerializeObject(data);
        
        CmdSaveProgressServerRpc(json);
    }

    public void SaveAndQuit()
    {
        if (IsOwner)
        {
            SaveProgress();
            Debug.Log("[Client] Saved Game. Exiting...");
        }
        
        Application.Quit();
        
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    private void OnApplicationQuit()
    {
        if (IsOwner)
        {
            SaveProgress();
        }
    }


}

