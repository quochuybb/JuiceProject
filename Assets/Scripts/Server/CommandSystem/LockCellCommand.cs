using System.Collections.Generic;
using Core.BuffSystem;
using UnityEngine;

public class LockCellCommand : ICommand
{
    private int cellCount;

    public LockCellCommand(int count)
    {
        cellCount = count;
    }

    public void Execute(GameRoomContext context)
    {
        List<int> availableCells = new List<int>();
        for (int i = 0; i < context.EnemyBoard.Count; i++)
        {
            CellData cell = context.EnemyBoard[i];
            if (!cell.isCleared && cell.value != 0 && cell.cellState == CellState.Normal)
            {
                availableCells.Add(i);
            }
        }

        int lockCount = Mathf.Min(cellCount, availableCells.Count);
        for (int i = 0; i < lockCount; i++)
        {
            int randomIndex = Random.Range(0, availableCells.Count);
            int cellIndex = availableCells[randomIndex];
            context.EnemyBoard[cellIndex].cellState = CellState.Locked;
            availableCells.RemoveAt(randomIndex);
        }
    }
}
