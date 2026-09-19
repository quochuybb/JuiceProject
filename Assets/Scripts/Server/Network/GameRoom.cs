using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Core.Gem;

public class GameRoom
{
    private const int COLUMNS = 9;

    public string RoomId { get; private set; }
    public ulong Player1Id { get; private set; }
    public ulong Player2Id { get; private set; }
    
    public int Player1HP { get; set; } = 1000;
    public int Player2HP { get; set; } = 1000;
    
    public int BoardSeed { get; private set; }
    public List<CellData> Player1Board { get; private set; }
    public List<CellData> Player2Board { get; private set; }
    public List<RecipeData> Player1Recipe { get; private set; } = new List<RecipeData>();
    public List<RecipeData> Player2Recipe { get; private set; } = new List<RecipeData>();
    public int Player1AddCount { get; set; } = 5;
    public int Player2AddCount { get; set; } = 5;

    public GameRoom(string roomId, ulong player1Id, ulong player2Id)
    {
        RoomId = roomId;
        Player1Id = player1Id;
        Player2Id = player2Id;

        BoardSeed = Random.Range(1000, 999999);
        Random.InitState(BoardSeed);
        Player1Board = BoardGenerator.GenerateInitialBoard(1, COLUMNS);
        Random.InitState(BoardSeed);
        Player2Board = BoardGenerator.GenerateInitialBoard(1, COLUMNS);
        Debug.Log($"[GameRoom] Created Room {RoomId}. Seed: {BoardSeed}");
    }

    public void HandleAttack(ulong attackerId, int damage)
    {
        if (attackerId == Player1Id)
        {
            Player2HP -= damage;
            if (Player2HP < 0) Player2HP = 0;
        }
        else if (attackerId == Player2Id)
        {
            Player1HP -= damage;
            if (Player1HP < 0) Player1HP = 0;
        }
        
        Debug.Log($"[GameRoom] {RoomId} - HP: P1({Player1HP}) vs P2({Player2HP})");

        var player1Obj = NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(Player1Id);
        if (player1Obj != null)
        {
            var p1 = player1Obj.GetComponent<NetworkPlayer>();
            ClientRpcParams p1Params = new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = new[] { Player1Id } } };
            p1.RpcUpdateHPClientRpc(Player1HP, Player2HP, p1Params);
        }

        var player2Obj = NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(Player2Id);
        if (player2Obj != null)
        {
            var p2 = player2Obj.GetComponent<NetworkPlayer>();
            ClientRpcParams p2Params = new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = new[] { Player2Id } } };
            p2.RpcUpdateHPClientRpc(Player2HP, Player1HP, p2Params); 
        }

        CheckWinCondition();
    }

    public bool HandleMatching(ulong clientId, int index1, int index2)
    {
        List<CellData> playerBoard;
        if (Player1Id == clientId)
        {
            playerBoard = Player1Board;
        }
        else
        {
            playerBoard = Player2Board;
        }
        CellData firstCell = playerBoard[index1];
        CellData secondCell = playerBoard[index2];
        bool isValidPair = (firstCell.value == secondCell.value) || (firstCell.value + secondCell.value == 10);
        
        List<RecipeData> playerRecipes = (clientId == Player1Id) ? Player1Recipe : Player2Recipe;
        if (!isValidPair && IsRecipeMatch(playerRecipes, firstCell.value, secondCell.value))
        {
            isValidPair = true;
        }

        if (!isValidPair) return false;
        
        int minIndex = Mathf.Min(firstCell.indexBoard, secondCell.indexBoard);
        int maxIndex = Mathf.Max(firstCell.indexBoard, secondCell.indexBoard);
        
        bool isConsecutive = true;
        for (int i = minIndex + 1; i < maxIndex; i++)
        {
            CellData middleCell = playerBoard[i];
            if (middleCell != null && middleCell.value != 0 && !middleCell.isCleared)
            {
                isConsecutive = false;
                break;
            }
        }
        if (isConsecutive) return true;
        
        int x1 = firstCell.indexBoard % COLUMNS;
        int y1 = firstCell.indexBoard / COLUMNS;
        int x2 = secondCell.indexBoard % COLUMNS;
        int y2 = secondCell.indexBoard / COLUMNS;

        int deltaX = Mathf.Abs(x1 - x2);
        int deltaY = Mathf.Abs(y1 - y2);

        if (deltaX != 0 && deltaY != 0 && deltaX != deltaY) return false; 

        int stepX = (x2 > x1) ? 1 : ((x2 < x1) ? -1 : 0);
        int stepY = (y2 > y1) ? 1 : ((y2 < y1) ? -1 : 0);

        int currentX = x1 + stepX;
        int currentY = y1 + stepY;

        while (currentX != x2 || currentY != y2)
        {
            int currentIndex = currentY * COLUMNS + currentX;
            CellData currentCell = playerBoard[currentIndex]; 
            if (currentCell != null && currentCell.value != 0 && !currentCell.isCleared)
            {
                return false; 
            }
            currentX += stepX;
            currentY += stepY;
        }

        return true;
    }

    public void SetPlayerRecipes(ulong clientId, List<int> recipeIDs)
    {
        RecipeData[] allRecipes = Resources.LoadAll<RecipeData>("ScriptObjects/Recipes");
        List<RecipeData> equippedRecipes = new List<RecipeData>();

        foreach (int id in recipeIDs)
        {
            foreach (RecipeData recipe in allRecipes)
            {
                if (recipe.recipeID == id)
                {
                    equippedRecipes.Add(recipe);
                    break;
                }
            }
        }

        if (clientId == Player1Id)
        {
            Player1Recipe = equippedRecipes;
            Debug.Log($"[GameRoom] Player1 equipped {equippedRecipes.Count} recipes");
        }
        else if (clientId == Player2Id)
        {
            Player2Recipe = equippedRecipes;
            Debug.Log($"[GameRoom] Player2 equipped {equippedRecipes.Count} recipes");
        }
    }

    private bool IsRecipeMatch(List<RecipeData> recipes, int val1, int val2)
    {
        return GetMatchingRecipeData(recipes, val1, val2) != null;
    }

    private RecipeData GetMatchingRecipeData(List<RecipeData> recipes, int val1, int val2)
    {
        foreach (RecipeData recipe in recipes)
        {
            if (recipe == null) continue;
            if ((recipe.foodFirst == val1 && recipe.foodSecond == val2) ||
                (recipe.foodFirst == val2 && recipe.foodSecond == val1))
            {
                return recipe;
            }
        }
        return null;
    }

    public void ProcessMatch(ulong clientId, int index1, int index2)
    {
        List<CellData> playerBoard = (clientId == Player1Id) ? Player1Board : Player2Board;

        if (index1 < 0 || index1 >= playerBoard.Count || index2 < 0 || index2 >= playerBoard.Count)
        {
            Debug.LogWarning($"[GameRoom] Invalid indices: {index1}, {index2}. Possible cheat!");
            return;
        }

        if (!HandleMatching(clientId, index1, index2))
        {
            Debug.LogWarning($"[GameRoom] Match REJECTED for client {clientId}: ({index1}, {index2})");
            return;
        }

        CellData firstCell = playerBoard[index1];
        CellData secondCell = playerBoard[index2];
        firstCell.isCleared = true;
        secondCell.isCleared = true;

        int baseDamage = (firstCell.value + secondCell.value) * 2;
        int finalDamage = baseDamage;

        List<RecipeData> playerRecipes = (clientId == Player1Id) ? Player1Recipe : Player2Recipe;
        RecipeData matchedRecipe = GetMatchingRecipeData(playerRecipes, firstCell.value, secondCell.value);
        if (matchedRecipe != null)
        {
            finalDamage = (baseDamage * 3) + (int)matchedRecipe.recipeCost;
            Debug.Log($"[GameRoom] Recipe bonus! {matchedRecipe.recipeName} → Damage: {finalDamage}");
        }

        Debug.Log($"[GameRoom] Match confirmed for {clientId}: ({index1}, {index2}) → Damage: {finalDamage}");

        HandleAttack(clientId, finalDamage);
    }

    public void ServerAddNumber(ulong clientId)
    {
        List<CellData> playerBoard = (clientId == Player1Id) ? Player1Board : Player2Board;
        int addCount = (clientId == Player1Id) ? Player1AddCount : Player2AddCount;

        if (addCount <= 0)
        {
            Debug.LogWarning($"[GameRoom] AddNumber REJECTED for {clientId}: no adds remaining");
            return;
        }

        List<CellData> listCopyNumber = new List<CellData>();
        foreach (CellData cell in playerBoard)
        {
            if (!cell.isCleared && cell.value != 0)
            {
                listCopyNumber.Add(new CellData
                {
                    value = cell.value,
                    isCleared = false,
                    hasGem = false,
                    gemType = GemType.None
                });
            }
        }
        if (listCopyNumber.Count == 0) return;

        int insertIndex = -1;
        for (int i = 0; i < playerBoard.Count; i++)
        {
            if (playerBoard[i].value == 0)
            {
                insertIndex = i;
                break;
            }
        }

        if (insertIndex == -1)
        {
            insertIndex = playerBoard.Count;
        }

        int neededCells = insertIndex + listCopyNumber.Count;
        if (neededCells > playerBoard.Count)
        {
            int cellsToAdd = neededCells - playerBoard.Count;
            int rowsToAdd = Mathf.CeilToInt((float)cellsToAdd / COLUMNS);
            ServerAddMoreCell(playerBoard, rowsToAdd * COLUMNS + COLUMNS * 2);
        }

        for (int i = 0; i < listCopyNumber.Count; i++)
        {
            playerBoard[insertIndex].value = listCopyNumber[i].value;
            playerBoard[insertIndex].isCleared = false;
            playerBoard[insertIndex].hasGem = false;
            playerBoard[insertIndex].gemType = GemType.None;
            insertIndex++;
        }

        if (clientId == Player1Id) Player1AddCount--;
        else Player2AddCount--;

        Debug.Log($"[GameRoom] ServerAddNumber for {clientId}. Board size: {playerBoard.Count}");
    }

    private void ServerAddMoreCell(List<CellData> board, int amountAddCell)
    {
        int startIndex = board.Count;
        int newTotalCell = board.Count + amountAddCell;
        for (int i = startIndex; i < newTotalCell; i++)
        {
            board.Add(new CellData { value = 0, isCleared = true, indexBoard = i });
        }
    }

    private void CheckWinCondition()
    {
        if (Player1HP <= 0 || Player2HP <= 0)
        {
            Debug.Log($"[GameRoom] Trận đấu kết thúc ở phòng {RoomId}! Đang gửi kết quả lên Node.js...");

            ulong winnerClientId = Player1HP > 0 ? Player1Id : Player2Id;
            ulong loserClientId = Player1HP <= 0 ? Player1Id : Player2Id;

            string winnerName = ServerAuthManager.GetUsernameForClient(winnerClientId);
            string loserName = ServerAuthManager.GetUsernameForClient(loserClientId);

            ServerMatchManager.Instance.SubmitMatchResult(winnerName, loserName, (winnerMmr, loserMmr) =>
            {
                var player1Obj = NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(Player1Id);
                if (player1Obj != null)
                {
                    var p1 = player1Obj.GetComponent<NetworkPlayer>();
                    ClientRpcParams p1Params = new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = new[] { Player1Id } } };
                    int myMmr = (Player1Id == winnerClientId) ? winnerMmr : loserMmr;
                    p1.RpcEndMatchClientRpc(Player1HP > 0, myMmr, p1Params);
                }

                var player2Obj = NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(Player2Id);
                if (player2Obj != null)
                {
                    var p2 = player2Obj.GetComponent<NetworkPlayer>();
                    ClientRpcParams p2Params = new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = new[] { Player2Id } } };
                    int myMmr = (Player2Id == winnerClientId) ? winnerMmr : loserMmr;
                    p2.RpcEndMatchClientRpc(Player2HP > 0, myMmr, p2Params);
                }
            });
        }
    }
}
