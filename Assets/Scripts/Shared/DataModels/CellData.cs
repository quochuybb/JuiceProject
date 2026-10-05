using System.Collections;
using System.Collections.Generic;
using Core.Gem;
using Core.BuffSystem;
using UnityEngine;

public class CellData
{
    public int value;
    public bool isCleared;
    public GemType gemType = GemType.None;
    public int indexBoard;
    public bool hasGem = false;
    public CellState cellState = CellState.Normal;
}