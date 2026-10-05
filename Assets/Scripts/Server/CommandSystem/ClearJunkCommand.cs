using System.Collections.Generic;
using Core.BuffSystem;
using UnityEngine;

public class ClearJunkCommand : ICommand
{
    private int cellCount;

    public ClearJunkCommand(int count)
    {
        cellCount = count;
    }

    public void Execute(GameRoomContext context)
    {
        List<int> junkCells = new List<int>();
        for (int i = 0; i < context.PlayerBoard.Count; i++)
        {
            CellData cell = context.PlayerBoard[i];
            if (cell.cellState == CellState.Locked || cell.cellState == CellState.Frozen)
            {
                junkCells.Add(i);
            }
        }

        int clearCount = Mathf.Min(cellCount, junkCells.Count);
        for (int i = 0; i < clearCount; i++)
        {
            int randomIndex = Random.Range(0, junkCells.Count);
            int cellIndex = junkCells[randomIndex];
            context.PlayerBoard[cellIndex].cellState = CellState.Normal;
            junkCells.RemoveAt(randomIndex);
        }
    }
}
