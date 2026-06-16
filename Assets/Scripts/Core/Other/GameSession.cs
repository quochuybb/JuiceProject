using System.Collections.Generic;

public static class GameSession
{
    public static int CurrentLayer = 1;
    public static GameModeType SelectedMode = GameModeType.GemMission;
    public static float TargetScore = 0;
    public static string CurrentNodeID = "";
    public static int CurrentChapter = 1;
    public static string CurrentChapterID = "";
    public static NodeType type;
    public static List<RecipeData> recipeList = new List<RecipeData>(5);
    public static bool isFull = false;
    public static event System.Action OnCoinChanged;

    private static float _currentCoin = 0;
    public static float currentCoin
    {
        get => _currentCoin;
        set
        {
            if (_currentCoin != value)
            {
                _currentCoin = value;
                OnCoinChanged?.Invoke();
            }
        }
    }
    public static List<RecipeData> inventoryList = new List<RecipeData>();
    public static List<string> CompletedNodes = new List<string>();

    public static ChapterData CurrentChapterData;
    public static int CurrentMapSeed = 0;
}