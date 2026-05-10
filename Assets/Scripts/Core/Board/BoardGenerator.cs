using System.Collections.Generic;
using UnityEngine;

public static class BoardGenerator
{
    public static List<CellData> GenerateInitialBoard(int stage, int columns)
    {
        int targetPairs;
        if (stage == 1)
            targetPairs = 3;
        else if (stage == 2)
            targetPairs = 2;
        else
            targetPairs = 1;

        for (int attempt = 0; attempt < 100; attempt++)
        {
            List<CellData> dataList = GenerateDynamicPerfectArray(columns);

            int currentPairs = CountMatchablePairs(dataList, columns);

            for (int step = 0; step < 3000; step++)
            {
                if (currentPairs == targetPairs) break;

                int i = Random.Range(0, dataList.Count);
                int j = Random.Range(0, dataList.Count);
                if (i == j) continue;

                CellData temp = dataList[i];
                dataList[i] = dataList[j];
                dataList[j] = temp;

                int newPairs = CountMatchablePairs(dataList, columns);
                int oldDist = Mathf.Abs(currentPairs - targetPairs);
                int newDist = Mathf.Abs(newPairs - targetPairs);

                if (newDist <= oldDist)
                {
                    currentPairs = newPairs;
                }
                else
                {
                    dataList[j] = dataList[i];
                    dataList[i] = temp;
                }
            }

            if (currentPairs == targetPairs)
            {
                return dataList;
            }
        }

        List<CellData> fallback = GenerateDynamicPerfectArray(columns);
        return fallback;
    }

    private static int CountMatchablePairs(List<CellData> dataList, int columns)
    {
        int count = 0;
        int totalCount = dataList.Count;
        int totalRows = Mathf.CeilToInt((float)totalCount / columns);

        bool[] used = new bool[totalCount]; 

        for (int idx = 0; idx < totalCount; idx++)
        {
            if (used[idx]) continue; 

            CellData cell = dataList[idx];
            if (cell.value == 0 || cell.isCleared) continue;

            int cx = idx % columns;
            int cy = idx / columns;
            if (cx + 1 < columns)
            {
                int ni = cy * columns + (cx + 1);
                if (ni < totalCount && !used[ni] && IsMatchable(cell, dataList[ni]))
                {
                    count++;
                    used[idx] = true; 
                    used[ni] = true;  
                    continue;       
                }
            }
            if (cy + 1 < totalRows)
            {
                int ni = (cy + 1) * columns + cx;
                if (ni < totalCount && !used[ni] && IsMatchable(cell, dataList[ni]))
                {
                    count++;
                    used[idx] = true;
                    used[ni] = true;
                    continue;
                }
            }
            if (cx + 1 < columns && cy + 1 < totalRows)
            {
                int ni = (cy + 1) * columns + (cx + 1);
                if (ni < totalCount && !used[ni] && IsMatchable(cell, dataList[ni]))
                {
                    count++;
                    used[idx] = true;
                    used[ni] = true;
                    continue;
                }
            }
            if (cx - 1 >= 0 && cy + 1 < totalRows)
            {
                int ni = (cy + 1) * columns + (cx - 1);
                if (ni < totalCount && !used[ni] && IsMatchable(cell, dataList[ni]))
                {
                    count++;
                    used[idx] = true;
                    used[ni] = true;
                    continue;
                }
            }
            if (cx == columns - 1 && idx + 1 < totalCount)
            {
                int ni = idx + 1;
                CellData nextCell = dataList[ni];
                if (nextCell.value != 0 && !nextCell.isCleared && !used[ni] && IsMatchable(cell, nextCell))
                {
                    count++;
                    used[idx] = true;
                    used[ni] = true;
                    continue;
                }
            }
        }

        return count;
    }

    private static bool IsMatchable(CellData a, CellData b)
    {
        if (b.value == 0 || b.isCleared) return false;
        return a.value == b.value || a.value + b.value == 10;
    }

    private static List<CellData> GenerateDynamicPerfectArray(int columns)
    {
        int[] countNumber1To9 = { 1, 1, 1, 1, 1, 1, 1, 1, 1 };

        for (int i = 0; i < columns; i++)
        {
            int randomGroup = Random.Range(1, 6);
            if (randomGroup == 5) countNumber1To9[4] += 2;
            else
            {
                countNumber1To9[randomGroup - 1] += 1;
                countNumber1To9[9 - randomGroup] += 1;
            }
        }

        List<CellData> result = new List<CellData>();
        for (int i = 0; i < countNumber1To9.Length; i++)
        {
            int value = i + 1;
            for (int j = 0; j < countNumber1To9[i]; j++)
            {
                result.Add(new CellData { value = value });
            }
        }
        ShuffleList(result);
        return result;
    }

    private static void ShuffleList(List<CellData> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            CellData temp = list[i];
            int randomIndex = Random.Range(i, list.Count);
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }
}