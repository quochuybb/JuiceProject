using UnityEngine;
using System;
using System.Collections.Generic;

public enum NodeType
{
    Easy,       
    Hard,      
    Shop,     
    Boss       
}

[Serializable]
public struct NodeWeight
{
    public NodeType nodeType;
    [Range(0f, 1f)]
    public float weightRatio; 
}

[Serializable]
public class LayerConfig
{
    [Min(1)]
    public int minNodes = 2;
    [Min(1)]
    public int maxNodes = 4;
    
    public List<NodeWeight> nodeWeights;
}