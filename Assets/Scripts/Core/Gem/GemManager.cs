using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Core.Gem
{
    public enum GemType
    {
        None,
        Orange,
        Purple,
        Red
    }
    
    public class GemManager : MonoBehaviour
    {
        [SerializeField] private Sprite[] spritesGem;
        [SerializeField] private GameObject gemUIPrefab;
        [SerializeField] private Transform gemUIContainer;

        private List<MissionCollectGem> missionCollectGems = new List<MissionCollectGem>();

        public Sprite GetGemSprite(GemType gemType)
        {
            int index = (int)gemType - 1;
            if (index >= 0 && index < spritesGem.Length)
            {
                return spritesGem[index];
            }
            return null;
        }

        public void GenerateMission(int currentLevel)
        {
            missionCollectGems.Clear();

            foreach (Transform child in gemUIContainer)
            {
                Destroy(child.gameObject);
            }

            List<GemType> gemTypes = new List<GemType> { GemType.Orange, GemType.Purple, GemType.Red };
            int countMission = Random.Range(2, 4); 

            for (int i = 0; i < countMission; i++)
            {
                int randomGemType = Random.Range(0, gemTypes.Count);
                GemType gemType = gemTypes[randomGemType];
                int baseAmount = 3 + currentLevel;
                int maxAmount = 4 + currentLevel;

                MissionCollectGem mission = new MissionCollectGem
                {
                    gemType = gemType,
                    requiredAmount = Random.Range(baseAmount, maxAmount + 1),
                    collectedAmount = 0,
                };
                missionCollectGems.Add(mission);
                SpawnGemUI(mission);

                gemTypes.Remove(gemType);
            }
        }

        public void SpawnGemUI(MissionCollectGem missionCollectGem)
        {
            if (gemUIPrefab == null || gemUIContainer == null)
            {
                Debug.LogWarning("gemUIPrefab hoặc gemUIContainer chưa được gán!");
                return;
            }

            GameObject gemUI = Instantiate(gemUIPrefab, gemUIContainer);

            Transform gemImage = gemUI.transform.Find("GemImage");
            if (gemImage != null)
            {
                Image image = gemImage.GetComponent<Image>();
                if (image != null)
                {
                    image.sprite = GetGemSprite(missionCollectGem.gemType);
                }
            }

            Transform count = gemUI.transform.Find("Count");
            if (count != null)
            {
                TextMeshProUGUI text = count.GetComponent<TextMeshProUGUI>();
                if (text != null)
                {
                    text.text = missionCollectGem.requiredAmount.ToString();
                }
            }
        }

        public void AssignGemsToBoard(List<CellData> boardData, int startIndex, int cellCount)
        {
            Dictionary<GemType, int> gemsOnBoard = new Dictionary<GemType, int>();
            foreach (var cell in boardData)
            {
                if (cell != null && !cell.isCleared && cell.value != 0 && cell.hasGem)
                {
                    if (!gemsOnBoard.ContainsKey(cell.gemType))
                        gemsOnBoard[cell.gemType] = 0;
                    gemsOnBoard[cell.gemType]++;
                }
            }

            List<GemType> gemsToSpawn = new List<GemType>();
            foreach (var mission in missionCollectGems)
            {
                if (mission.IsCompleted) continue;

                int totalNeeded = mission.requiredAmount - mission.collectedAmount;
                int currentlyOnBoard = gemsOnBoard.ContainsKey(mission.gemType) ? gemsOnBoard[mission.gemType] : 0;
                
                int amountToSpawn = totalNeeded - currentlyOnBoard;

                for (int i = 0; i < amountToSpawn; i++)
                {
                    gemsToSpawn.Add(mission.gemType);
                }
            }

            if (gemsToSpawn.Count == 0) return;

            for (int i = 0; i < gemsToSpawn.Count; i++)
            {
                GemType temp = gemsToSpawn[i];
                int randomIndex = Random.Range(i, gemsToSpawn.Count);
                gemsToSpawn[i] = gemsToSpawn[randomIndex];
                gemsToSpawn[randomIndex] = temp;
            }

            int Y = Mathf.CeilToInt((cellCount + 1) / 2f);
            int maxGemsThisTurn = missionCollectGems.FindAll(m => !m.IsCompleted).Count; // Z = số loại gem active
            int gemsSpawnedThisTurn = 0;
            int cellsSinceLastGem = 0;
            List<int> gemValuesThisTurn = new List<int>(); 

            int endIndex = Mathf.Min(startIndex + cellCount, boardData.Count);

            for (int i = startIndex; i < endIndex; i++)
            {
                if (gemsToSpawn.Count == 0) break; 

                CellData currentCell = boardData[i];

                if (currentCell.value == 0 || currentCell.isCleared || currentCell.hasGem) continue;

                cellsSinceLastGem++;
                bool wantToSpawnGem = false;

                float X = Random.Range(5f, 7f);
                if (Random.Range(0f, 100f) <= X)
                    wantToSpawnGem = true;

                if (cellsSinceLastGem >= Y)
                    wantToSpawnGem = true;

                if (wantToSpawnGem && gemsSpawnedThisTurn < maxGemsThisTurn)
                {
                    bool canSpawnAntiMatch = true;
                    foreach (int prevGemValue in gemValuesThisTurn)
                    {
                        if (currentCell.value == prevGemValue || currentCell.value + prevGemValue == 10)
                        {
                            canSpawnAntiMatch = false;
                            break;
                        }
                    }

                    if (canSpawnAntiMatch)
                    {
                        currentCell.hasGem = true;
                        
                        currentCell.gemType = gemsToSpawn[0];
                        gemsToSpawn.RemoveAt(0);

                        gemsSpawnedThisTurn++;
                        cellsSinceLastGem = 0; 
                        gemValuesThisTurn.Add(currentCell.value);
                    }
                }
            }
        }
        
        public bool AreAllMissionsCompleted()
        {
            if (missionCollectGems.Count == 0) return false;

            foreach (var mission in missionCollectGems)
            {
                if (!mission.IsCompleted)
                {
                    return false;
                }
            }
            return true;
        }
        
        public void CollectGem(GemType gemType)
        {
            if (gemType == GemType.None) return;

            foreach (var mission in missionCollectGems)
            {
                if (mission.gemType == gemType && !mission.IsCompleted)
                {
                    mission.collectedAmount++;
                    UpdateGemUI(mission);
                    break;
                }
            }
        }

  
        private void UpdateGemUI(MissionCollectGem mission)
        {
            foreach (Transform gemUI in gemUIContainer)
            {
                Transform gemImageTransform = gemUI.Find("GemImage");
                if (gemImageTransform == null) continue;

                Image image = gemImageTransform.GetComponent<Image>();
                if (image != null && image.sprite == GetGemSprite(mission.gemType))
                {
                    Transform countTransform = gemUI.Find("Count");
                    if (countTransform == null) continue;

                    TextMeshProUGUI text = countTransform.GetComponent<TextMeshProUGUI>();
                    if (text != null)
                    {
                        text.text = $"{mission.requiredAmount - mission.collectedAmount}";
                    }
                    break;
                }
            }
        }
    }
}