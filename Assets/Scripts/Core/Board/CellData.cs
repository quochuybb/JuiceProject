using System.Collections;
using System.Collections.Generic;
using Core.Gem;
using UnityEngine;

public class CellData
{
    public int value;
    public bool isCleared;
    public GemType gemType = GemType.None;
    public int indexBoard;
    public bool hasGem = false;
}