using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Core.Gem;
using TMPro;
using UnityEngine.SceneManagement;
public enum GameModeType
{
    GemMission,    
    TargetScore,    
    RecipeCrafting,
}
public class BoardManager : MonoBehaviour
{
    private const int COLUMNS = 9;
    private const int TOTAL_CELLS_TO_SPAWN = 90;
    public GameModeType currentGameMode;
    [SerializeField] private GemManager gemManager;
    [SerializeField] private TextMeshProUGUI stageText;
    [SerializeField] private GameObject cellPrefab;
    [SerializeField] private Transform contentTransform;
    [SerializeField] private GameObject winPanel;
    [SerializeField] private GameObject losePanel;
    [SerializeField] private Button addNumberButton;
    [SerializeField] private int currentScore;
    [SerializeField] private int targetScore;
    [SerializeField] private TextMeshProUGUI scoreText;
    public Sprite[] numberSprites;

    [SerializeField] private int stage = 1;
    [SerializeField] private int level = 1;

    [SerializeField] private int countAdd;

    private CellUI selectedCellUI;

    private List<CellData> dataList;
    public static BoardManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    private void Start()
    {
        countAdd = 5;

        if (winPanel != null) winPanel.SetActive(false);
        if (losePanel != null) losePanel.SetActive(false);

        GenerateStage(stage, GameModeType.GemMission, 0, false);
        
    }
    public void GenerateStage(int currentStage, GameModeType mode, int targetScoreForMode = 0, bool keepMissions = false)
    {
        if (mode != GameModeType.TargetScore)
        {
            scoreText.gameObject.SetActive(false);
        }
        currentGameMode = mode;
        currentScore = 0;
        targetScore = targetScoreForMode;
        
        if (scoreText != null) scoreText.text = "0 / " + targetScore.ToString();

        dataList = BoardGenerator.GenerateInitialBoard(currentStage, COLUMNS);

        if (currentGameMode == GameModeType.GemMission)
        {
            if (!keepMissions) gemManager.GenerateMission(level);
            gemManager.AssignGemsToBoard(dataList, 0, dataList.Count);
        }

        GenerateBoardEmpty(TOTAL_CELLS_TO_SPAWN);
        ImplementNumberToCell(dataList, 0);
    }

    public void GenerateBoardEmpty(int totalCellToSpawn)
    {
        foreach (Transform child in contentTransform)
        {
            Destroy(child.gameObject);
        }
        contentTransform.DetachChildren();
        while (dataList.Count < totalCellToSpawn)
        {
            dataList.Add(new CellData { value = 0, isCleared = true });
        }

        for (int i = 0; i < totalCellToSpawn; i++)
        {
            GameObject newCell = Instantiate(cellPrefab, contentTransform);
            CellUI cellUIComponent = newCell.GetComponent<CellUI>();

            if (cellUIComponent != null)
            {
                cellUIComponent.cell = dataList[i];
                cellUIComponent.cell.indexBoard = i;
                cellUIComponent.OnCellClicked += ProcessInput;
            }
        }
    }

    public void ImplementNumberToCell(List<CellData> cellDataList, int startIndex)
    {
        int totalCells = contentTransform.childCount;

        for (int i = startIndex; i < cellDataList.Count; i++)
        {
            if (i >= totalCells) break;
            
            CellData currentData = cellDataList[i];
            Transform cellTransform = contentTransform.GetChild(i);
            CellUI cellUIComponent = cellTransform.GetComponent<CellUI>();

            cellUIComponent.cell = currentData;
            cellUIComponent.ResetVisualState();
            
            Transform numberObj = cellTransform.Find("Number");
            if (numberObj != null)
            {
                Image cellImage = numberObj.GetComponent<Image>();
                if (cellImage != null)
                {
                    if (currentData.value >= 1 && currentData.value <= 9)
                    {
                        cellImage.sprite = numberSprites[currentData.value - 1];
                        cellImage.enabled = true; 
                        Color tempColor = cellImage.color;
                        tempColor.a = currentData.isCleared ? 0.25f : 1f;
                        cellImage.color = tempColor;
                    }
                    else
                    {
                        cellImage.enabled = false; 
                    }
                }
            }

            Transform gemObj = cellTransform.Find("Gem"); 
            if (gemObj != null)
            {
                Image gemImage = gemObj.GetComponent<Image>();
                if (gemImage != null)
                {
                    if (currentData.hasGem && !currentData.isCleared)
                    {
                        gemImage.gameObject.SetActive(true);
                        Color gemColor = gemImage.color;
                        gemColor.a = 1f;
                        gemImage.color = gemColor;
                        
                        gemImage.sprite = gemManager.GetGemSprite(currentData.gemType);
                    }
                    else
                    {
                        gemImage.gameObject.SetActive(false); 
                    }
                }
            }
        }
    }
    
    public void ProcessInput(CellUI clickedCellUI)
    {
        if (selectedCellUI == null)
        {
            selectedCellUI = clickedCellUI;
            selectedCellUI.ToggleSelection(true); 
            return;
        }

        if (selectedCellUI == clickedCellUI)
        {
            selectedCellUI.ToggleSelection(false); 
            selectedCellUI = null;
            return;
        }

        if (IsMatch(selectedCellUI.cell, clickedCellUI.cell))
        {
            HandleMatchSuccess(selectedCellUI, clickedCellUI);
            selectedCellUI = null;
        }
        else
        {
            int val1 = selectedCellUI.cell.value;
            int val2 = clickedCellUI.cell.value;
            
            if (val1 == val2 || val1 + val2 == 10)
            {
                HandleBlockedMatch(selectedCellUI, clickedCellUI);
                selectedCellUI.ToggleSelection(false); 
                selectedCellUI = null;
            }
            else
            {
                HandleMatchFail(selectedCellUI, clickedCellUI);
            }        
        }
    }
    private void HandleMatchFail(CellUI cell1, CellUI cell2)
    {
        cell1.ToggleSelection(false);
        
        cell2.ToggleSelection(true);
        
        selectedCellUI = cell2;

        if (SoundManager.Instance != null)
            SoundManager.Instance.PlayChooseNumber();
    }
    private bool HasAnyValidMatch()
    {
        for (int i = 0; i < dataList.Count; i++)
        {
            CellData firstCell = dataList[i];
            if (firstCell.value == 0 || firstCell.isCleared) continue;

            for (int j = i + 1; j < dataList.Count; j++)
            {
                CellData secondCell = dataList[j];
                if (secondCell.value == 0 || secondCell.isCleared) continue;

                if (IsMatch(firstCell, secondCell))
                {
                    return true;
                }
            }
        }
        return false;
    }

    private void HandleMatchSuccess(CellUI cell1, CellUI cell2)
    {
        switch (currentGameMode)
        {
            case GameModeType.GemMission:
                if (cell1.cell.hasGem) gemManager.CollectGem(cell1.cell.gemType);
                if (cell2.cell.hasGem) gemManager.CollectGem(cell2.cell.gemType);
                break;

            case GameModeType.TargetScore:
                int matchScore = (cell1.cell.value + cell2.cell.value) * 10;
                AddScore(matchScore);
                break;

            case GameModeType.RecipeCrafting:
                break;
        }

        cell1.cell.isCleared = true;
        cell2.cell.isCleared = true;
        dataList[cell1.cell.indexBoard].isCleared = true;
        dataList[cell2.cell.indexBoard].isCleared = true;

        if (SoundManager.Instance != null)
            SoundManager.Instance.PlayPairClear();

        cell1.OnMatchSuccess();
        cell2.OnMatchSuccess();

        Invoke(nameof(CheckAndClearEmptyRows), 0.5f);
    }
    private void AddScore(int amount)
    {
        currentScore += amount;
        if (scoreText != null) scoreText.text = currentScore.ToString() + " / " + targetScore.ToString();
    }
    private void HandleBlockedMatch(CellUI cell1, CellUI cell2)
    {
        List<CellUI> blockingCells = new List<CellUI>();

        int minIndex = Mathf.Min(cell1.cell.indexBoard, cell2.cell.indexBoard);
        int maxIndex = Mathf.Max(cell1.cell.indexBoard, cell2.cell.indexBoard);

        int x1 = cell1.cell.indexBoard % COLUMNS;
        int y1 = cell1.cell.indexBoard / COLUMNS;
        int x2 = cell2.cell.indexBoard % COLUMNS;
        int y2 = cell2.cell.indexBoard / COLUMNS;

        int deltaX = Mathf.Abs(x1 - x2);
        int deltaY = Mathf.Abs(y1 - y2);

        if ((deltaX == 0 || deltaY == 0 || deltaX == deltaY) && (cell1.cell.indexBoard != cell2.cell.indexBoard))
        {
            int stepX = (x2 > x1) ? 1 : ((x2 < x1) ? -1 : 0);
            int stepY = (y2 > y1) ? 1 : ((y2 < y1) ? -1 : 0);

            int currentX = x1 + stepX;
            int currentY = y1 + stepY;

            while (currentX != x2 || currentY != y2)
            {
                int currentIndex = currentY * COLUMNS + currentX;
                if (currentIndex >= 0 && currentIndex < dataList.Count)
                {
                    CellData currentCell = dataList[currentIndex];
                    if (currentCell != null && currentCell.value != 0 && !currentCell.isCleared)
                    {
                        blockingCells.Add(contentTransform.GetChild(currentIndex).GetComponent<CellUI>());
                    }
                }
                currentX += stepX;
                currentY += stepY;
            }
        }
        else
        {
            for (int i = minIndex + 1; i < maxIndex; i++)
            {
                CellData currentCell = dataList[i];
                if (currentCell != null && currentCell.value != 0 && !currentCell.isCleared)
                {
                    blockingCells.Add(contentTransform.GetChild(i).GetComponent<CellUI>());
                }
            }
        }

        foreach (var ui in blockingCells)
        {
            if (ui != null) ui.Shake();
        }

    }
    
    private void CheckAndClearEmptyRows()
    {
        bool hasRowCleared = false;
        int totalRows = dataList.Count / COLUMNS; 

        for (int y = totalRows - 1; y >= 0; y--)
        {
            int startIndex = y * COLUMNS;
            bool isRowFullyCleared = true;
            bool isFillerRow = true; 

            for (int x = 0; x < COLUMNS; x++)
            {
                CellData cell = dataList[startIndex + x];
                if (!cell.isCleared)
                {
                    isRowFullyCleared = false;
                    break;
                }
                if (cell.value != 0) isFillerRow = false; 
            }

            if (isRowFullyCleared && !isFillerRow)
            {
                dataList.RemoveRange(startIndex, COLUMNS);
                for (int i = 0; i < COLUMNS; i++)
                {
                    dataList.Add(new CellData { value = 0, isCleared = true });
                }
                hasRowCleared = true;
            }
        }

        if (hasRowCleared)
        {
            if (SoundManager.Instance != null)
                SoundManager.Instance.PlayRowClear();

            for (int i = 0; i < dataList.Count; i++)
            {
                dataList[i].indexBoard = i;
            }

            ImplementNumberToCell(dataList,0);
        }
        

        if (gemManager.AreAllMissionsCompleted())
        {
            HandleGameWin();
            return;
        }

        if (CleanBoard())
        {
            return;
        }
        if (countAdd == 0 && !HasAnyValidMatch())
        {
            HandleGameOver();
        }
        CheckWinCondition();
    }
    public void GoHome()
    {
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlayUIClick();

        SceneManager.LoadScene("MainMenu");
    }

    private bool IsMatch(CellData firstCell, CellData secondCell)
    {
        if (firstCell.value != secondCell.value && firstCell.value + secondCell.value != 10) return false; 
        
        int minIndex = Mathf.Min(firstCell.indexBoard, secondCell.indexBoard);
        int maxIndex = Mathf.Max(firstCell.indexBoard, secondCell.indexBoard);
        
        bool isConsecutive = true;
        for (int i = minIndex + 1; i < maxIndex; i++)
        {
            CellData middleCell = dataList[i];
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
            CellData currentCell = dataList[currentIndex]; 

            if (currentCell != null && currentCell.value != 0 && !currentCell.isCleared)
            {
                return false; 
            }
            currentX += stepX;
            currentY += stepY;
        }

        return true;
    }
    
    public void AddNumber()
    {
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlayUIClick();

        if (countAdd == 0)
        { 

            return;
        }

        List<CellData> listCopyNumber = new List<CellData>();
        foreach (CellData cell in dataList)
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
        for (int i = 0; i < dataList.Count; i++)
        {
            if (dataList[i].value == 0)
            {
                insertIndex = i;
                break;
            }
        }

        if (insertIndex == -1)
        {
            insertIndex = dataList.Count;
        }

        int neededCells = insertIndex + listCopyNumber.Count;
        if (neededCells > dataList.Count)
        {
            int cellsToAdd = neededCells - dataList.Count;
            int rowsToAdd = Mathf.CeilToInt((float)cellsToAdd / COLUMNS);
            AddMoreCell(rowsToAdd * COLUMNS + COLUMNS * 2);
        }

        int startIndexToUpdateUI = insertIndex;
        for (int i = 0; i < listCopyNumber.Count; i++)
        {
            dataList[insertIndex].value = listCopyNumber[i].value;
            dataList[insertIndex].isCleared = false;
            dataList[insertIndex].hasGem = false;
            dataList[insertIndex].gemType = GemType.None;

            insertIndex++;
        }

        gemManager.AssignGemsToBoard(dataList, startIndexToUpdateUI, listCopyNumber.Count);

        ImplementNumberToCell(dataList, startIndexToUpdateUI);

        countAdd--;
        Transform number = addNumberButton.transform.Find("IconNumber/Number");
        if (number != null)
        {
            TextMeshProUGUI numberText = number.GetComponentInChildren<TextMeshProUGUI>();
            if (numberText != null)
            {
                numberText.text = countAdd.ToString();
            }
        }
    }
    
    private void AddMoreCell(int amountAddCell)
    {
        int newTotalCell = dataList.Count + amountAddCell;
        for (int i = dataList.Count; i < newTotalCell; i++)
        {
            dataList.Add(new CellData { value = 0, isCleared = true });

            GameObject newCell = Instantiate(cellPrefab, contentTransform);
            CellUI cellUIComponent = newCell.GetComponent<CellUI>();

            if (cellUIComponent != null)
            {
                cellUIComponent.cell = dataList[i];
                cellUIComponent.cell.indexBoard = i;
                cellUIComponent.OnCellClicked += ProcessInput;
            }
        }
    }

    private bool CleanBoard()
    {
        foreach (CellData cell in dataList)
        {
            if (cell.value != 0) return false; 
        }

        stage++;
        if (stageText != null) stageText.text = "Stage: " + stage.ToString();
    
        countAdd = 5;
        Transform number = addNumberButton.transform.Find("IconNumber/Number");
        if (number != null)
        {
            TextMeshProUGUI numberText = number.GetComponentInChildren<TextMeshProUGUI>();
            if (numberText != null) numberText.text = countAdd.ToString();
        }
    
        GenerateStage(stage, currentGameMode, targetScore, true);    
        return true; 
    }

    private void HandleGameOver()
    {
        if (losePanel != null)
        {
            losePanel.SetActive(true);
        }
        
    }

    private void HandleGameWin()
    {
        if (winPanel != null)
        {
            winPanel.SetActive(true);
        }
    }
    
    public void RetryCurrentLevel()
    {
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlayUIClick();

        stage = 1;
        if (stageText != null) stageText.text = "Stage: " + stage.ToString();

        countAdd = 5;
        
        Transform number = addNumberButton.transform.Find("IconNumber/Number");
        if (number != null)
        {
            TextMeshProUGUI numberText = number.GetComponentInChildren<TextMeshProUGUI>();
            if (numberText != null)
            {
                numberText.text = countAdd.ToString();
            }
        }
        
        if (selectedCellUI != null)
        {
            selectedCellUI = null; 
        }

        if (winPanel != null) winPanel.SetActive(false);
        if (losePanel != null) losePanel.SetActive(false);

        GenerateStage(stage, GameModeType.GemMission, 0, false);
    }

    public void NextLevel()
    {
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlayUIClick();
        Debug.Log("Win back to map chapter");
        // level++;
        //
        // stage = 1;
        // if (stageText != null) stageText.text = "Stage: " + stage.ToString();
        //
        // countAdd = 5;
        //
        // Transform number = addNumberButton.transform.Find("IconNumber/Number");
        // if (number != null)
        // {
        //     TextMeshProUGUI numberText = number.GetComponentInChildren<TextMeshProUGUI>();
        //     if (numberText != null)
        //     {
        //         numberText.text = countAdd.ToString();
        //     }
        // }
        //
        // if (selectedCellUI != null)
        // {
        //     selectedCellUI = null;
        // }
        //
        // if (winPanel != null) winPanel.SetActive(false);
        // if (losePanel != null) losePanel.SetActive(false);
        //
        // GenerateStage(stage, false);
    }
    private void CheckWinCondition()
    {
        bool isWin = false;

        switch (currentGameMode)
        {
            case GameModeType.GemMission:
                isWin = gemManager.AreAllMissionsCompleted();
                break;

            case GameModeType.TargetScore:
                isWin = (currentScore >= targetScore);
                break;

            case GameModeType.RecipeCrafting:
                break;
        }

        if (isWin)
        {
            HandleGameWin();
        }
        else if (countAdd == 0 && !HasAnyValidMatch())
        {
            HandleGameOver();
        }
    }
}