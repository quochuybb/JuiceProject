using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Core.Gem;
using TMPro;
using UnityEngine.SceneManagement;
using Unity.Netcode;

public class PvPBoardManager : MonoBehaviour
{
    private const int COLUMNS = 9;
    private const int TOTAL_CELLS_TO_SPAWN = 90;

    [Header("UI References")]
    [SerializeField] private Slider myHPSlider;
    [SerializeField] private Slider opponentHPSlider;
    [SerializeField] private TextMeshProUGUI myHPText;
    [SerializeField] private TextMeshProUGUI opponentHPText;
    [SerializeField] private TextMeshProUGUI MMRTextWin;
    [SerializeField] private TextMeshProUGUI MMRTextLose;
    [SerializeField] private Button addNumberButton;
    [SerializeField] private GameObject winPanel;
    [SerializeField] private GameObject losePanel;
    [SerializeField] private int countAdd;


    [Header("Board Settings")]
    [SerializeField] private GameObject cellPrefab;
    [SerializeField] private Transform contentTransform;
    public Sprite[] numberSprites;

    private List<CellData> dataList = new List<CellData>();
    private CellUI selectedCellUI;

    private void OnEnable()
    {
        NetworkPlayer.OnClientGameStarted += HandleGameStarted;
        NetworkPlayer.OnClientHPUpdated += HandleHPUpdated;
        NetworkPlayer.OnClientMatchEnded += HandleMatchEnded;
    }

    private void OnDisable()
    {
        NetworkPlayer.OnClientGameStarted -= HandleGameStarted;
        NetworkPlayer.OnClientHPUpdated -= HandleHPUpdated;
        NetworkPlayer.OnClientMatchEnded -= HandleMatchEnded;
    }

    private void HandleGameStarted(int boardSeed)
    {
        countAdd = 5;
        Debug.Log($"[PvPBoardManager] Generating PVP board with seed: {boardSeed}");
        
        UpdateHPUI(1000, 1000);

        if (winPanel != null) winPanel.SetActive(false);
        if (losePanel != null) losePanel.SetActive(false);

        Random.InitState(boardSeed);
        
        dataList = BoardGenerator.GenerateInitialBoard(1, COLUMNS);
        
        GenerateBoardEmpty(TOTAL_CELLS_TO_SPAWN);
        ImplementNumberToCell(dataList, 0);
    }

    private void HandleHPUpdated(int myHP, int opponentHP)
    {
        UpdateHPUI(myHP, opponentHP);
    }

    private void UpdateHPUI(int myHP, int oppHP)
    {
        if (myHPSlider != null) myHPSlider.value = (float)myHP / 1000f;
        if (opponentHPSlider != null) opponentHPSlider.value = (float)oppHP / 1000f;

        if (myHPText != null) myHPText.text = $"{myHP}/1000";
        if (opponentHPText != null) opponentHPText.text = $"{oppHP}/1000";
    }

    private void HandleMatchEnded(bool isWinner, int newMmr)
    {
        if (isWinner)
        {
            if (winPanel != null) winPanel.SetActive(true);
            Debug.Log($"[PvPBoardManager] You Win! New MMR: {newMmr}");
            if (MMRTextWin != null) MMRTextWin.text = $"MMR: {newMmr}";
        }
        else
        {
            if (losePanel != null) losePanel.SetActive(true);
            Debug.Log($"[PvPBoardManager] You Lose! New MMR: {newMmr}");
            if (MMRTextLose != null) MMRTextLose.text = $"MMR: {newMmr}";
        }
        
        if (WebClientManager.Instance != null && WebClientManager.Instance.CurrentUser != null)
        {
            WebClientManager.Instance.CurrentUser.mmr = newMmr;
        }
        
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.Shutdown();
        }
    }    
    private void GenerateBoardEmpty(int totalCellToSpawn)
    {
        foreach (Transform child in contentTransform) Destroy(child.gameObject);
        contentTransform.DetachChildren();

        while (dataList.Count < totalCellToSpawn)
            dataList.Add(new CellData { value = 0, isCleared = true });

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
        }
    }

    public void ProcessInput(CellUI clickedCellUI)
    {
        if (selectedCellUI == null)
        {
            if (SoundManager.Instance != null) SoundManager.Instance.PlayChooseNumber();
            selectedCellUI = clickedCellUI;
            selectedCellUI.ToggleSelection(true); 
            return;
        }

        if (selectedCellUI == clickedCellUI)
        {
            if (SoundManager.Instance != null) SoundManager.Instance.PlayChooseNumber();
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
            
            bool isValidPair = (val1 == val2) || (val1 + val2 == 10);
            if (RecipeManager.instance != null && RecipeManager.instance.GetMatchingRecipe(val1, val2) != null)
                isValidPair = true;

            if (isValidPair)
            {
                // Hợp lệ nhưng bị cản đường -> Rung lắc các ô cản
                HandleBlockedMatch(selectedCellUI, clickedCellUI);
                selectedCellUI.ToggleSelection(false); 
                selectedCellUI = null;
            }
            else
            {
                // Ghép sai luật -> Đổi lựa chọn sang ô mới
                HandleMatchFail(selectedCellUI, clickedCellUI);
            }
        }
    }

    private bool IsMatch(CellData firstCell, CellData secondCell)
    {
        bool isValidPair = (firstCell.value == secondCell.value);
        
        // KIỂM TRA THÊM CÔNG THỨC (RECIPE)
        if (RecipeManager.instance != null)
        {
            RecipeData matchedRecipe = RecipeManager.instance.GetMatchingRecipe(firstCell.value, secondCell.value);
            if (matchedRecipe != null)
            {
                isValidPair = true;
                Debug.Log(matchedRecipe.foodFirst + ":" + matchedRecipe.foodSecond + ":" + matchedRecipe.recipeName);

            }
        }

        if (!isValidPair) return false;
        
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

    private void HandleMatchSuccess(CellUI cell1, CellUI cell2)
    {
        cell1.cell.isCleared = true;
        cell2.cell.isCleared = true;
        dataList[cell1.cell.indexBoard].isCleared = true;
        dataList[cell2.cell.indexBoard].isCleared = true;

        if (SoundManager.Instance != null) SoundManager.Instance.PlayPairClear();
        cell1.OnMatchSuccess();
        cell2.OnMatchSuccess();
        
        int baseDamage = (cell1.cell.value + cell2.cell.value) * 2; 
        int finalDamage = baseDamage;

        if (RecipeManager.instance != null)
        {
            RecipeData matchedRecipe = RecipeManager.instance.GetMatchingRecipe(cell1.cell.value, cell2.cell.value);
            if (matchedRecipe != null)
            {
                finalDamage = (baseDamage * 3) + (int)matchedRecipe.recipeCost;
                Debug.Log($"[PvPBoardManager] Ghép thành công {matchedRecipe.recipeName}! Sát thương bạo kích: {finalDamage}");
            }
        }
        
        if (NetworkPlayer.LocalInstance != null)
        {
            Debug.Log($"[PvPBoardManager] Gửi lệnh tấn công lên Server! Sát thương: {finalDamage}");
            NetworkPlayer.LocalInstance.CmdAttackServerRpc(finalDamage);
        }

        Invoke(nameof(CheckAndClearEmptyRows), 0.5f);
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
                if (!cell.isCleared) { isRowFullyCleared = false; break; }
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
            if (SoundManager.Instance != null) SoundManager.Instance.PlayRowClear();
            for (int i = 0; i < dataList.Count; i++) dataList[i].indexBoard = i;
            ImplementNumberToCell(dataList,0);
        }

        CleanBoard();
    }

    private bool CleanBoard()
    {
        foreach (CellData cell in dataList)
        {
            if (cell.value != 0) return false; 
        }

        dataList = BoardGenerator.GenerateInitialBoard(1, COLUMNS);
        GenerateBoardEmpty(TOTAL_CELLS_TO_SPAWN);
        ImplementNumberToCell(dataList, 0);
        countAdd += 3;
        Debug.Log("[PvPBoardManager] Clean Board");
        return true; 
    }

    private void HandleMatchFail(CellUI cell1, CellUI cell2)
    {
        cell1.ToggleSelection(false);
        cell2.ToggleSelection(true);
        selectedCellUI = cell2;

        if (SoundManager.Instance != null)
            SoundManager.Instance.PlayChooseNumber();
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
}
