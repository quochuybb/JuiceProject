using System;
using System.Collections.Generic;

[Serializable]
public class GameSessionData
{
    public int CurrentLayer;
    public GameModeType SelectedMode;
    public float TargetScore;
    public string CurrentNodeID;
    public int CurrentChapter;
    public string CurrentChapterID;
    public NodeType type;
    public int CurrentMapSeed;
    
    public List<int> recipeListIDs = new List<int>();
    
    public bool isFull;
    public float currentCoin;
    
    public List<int> inventoryListIDs = new List<int>();
    
    public List<string> CompletedNodes = new List<string>();

    public GameSessionData() { }

    public void PackFromGameSession()
    {
        CurrentLayer = GameSession.CurrentLayer;
        SelectedMode = GameSession.SelectedMode;
        TargetScore = GameSession.TargetScore;
        CurrentNodeID = GameSession.CurrentNodeID;
        CurrentChapter = GameSession.CurrentChapter;
        CurrentChapterID = GameSession.CurrentChapterID;
        type = GameSession.type;
        CurrentMapSeed = GameSession.CurrentMapSeed;
        isFull = GameSession.isFull;
        currentCoin = GameSession.currentCoin;
        
        CompletedNodes = new List<string>(GameSession.CompletedNodes);

        recipeListIDs.Clear();
        foreach (var r in GameSession.recipeList)
        {
            if (r != null) recipeListIDs.Add(r.recipeID);
        }

        inventoryListIDs.Clear();
        foreach (var r in GameSession.inventoryList)
        {
            if (r != null) inventoryListIDs.Add(r.recipeID);
        }
    }
    
    public void UnpackToGameSession(Dictionary<int, RecipeData> allRecipes)
    {
        GameSession.CurrentLayer = CurrentLayer;
        GameSession.SelectedMode = SelectedMode;
        GameSession.TargetScore = TargetScore;
        GameSession.CurrentNodeID = CurrentNodeID;
        GameSession.CurrentChapter = CurrentChapter;
        GameSession.CurrentChapterID = CurrentChapterID;
        GameSession.type = type;
        GameSession.CurrentMapSeed = CurrentMapSeed;
        GameSession.isFull = isFull;
        GameSession.currentCoin = currentCoin;

        GameSession.CompletedNodes = new List<string>(CompletedNodes);

        GameSession.recipeList.Clear();
        foreach (var id in recipeListIDs)
        {
            if (allRecipes.TryGetValue(id, out var recipe))
                GameSession.recipeList.Add(recipe);
        }

        GameSession.inventoryList.Clear();
        foreach (var id in inventoryListIDs)
        {
            if (allRecipes.TryGetValue(id, out var recipe))
                GameSession.inventoryList.Add(recipe);
        }
    }
}
