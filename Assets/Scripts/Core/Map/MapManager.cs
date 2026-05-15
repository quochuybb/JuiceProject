using System;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;


public class MapManager : MonoBehaviour
{
    public static MapManager Instance { get; private set; }
    [SerializeField] private GameObject nodePrefab;
    [SerializeField] private ChapterData chapterData;
    [SerializeField] private Transform mapContainerTransform;
    private int seed;
    private Random mapRNG;
    private void Awake()
    {
        Instance = this;
    }

    public void GenerateSeed(ChapterData chapter)
    {
        chapterData = chapter;
        seed = chapter.chapterID.GetHashCode() + UnityEngine.Random.Range(0, 1000);
        GenerateMapGraph();
    }

    void GenerateMapGraph()
    {
        mapRNG = new Random(seed);
        List<List<MapNodeData>> logicMap = CreateLogicMapData(); 

        for (int i = 0; i < logicMap.Count - 1; i++)
        {
            ConnectLayers(logicMap[i], logicMap[i+1], mapRNG);
        }

        DrawMapToScreen(logicMap);
    }

    void DrawMapToScreen(List<List<MapNodeData>> logicMap)
    {
        foreach (var layer in logicMap)
        {
            foreach (var nodeData in layer)
            {
                GameObject newButtonGO = Instantiate(nodePrefab, mapContainerTransform);
            
                NodeUIButton uiScript = newButtonGO.GetComponent<NodeUIButton>();
                uiScript.Setup(nodeData);

            }
        }
    }

    private List<List<MapNodeData>> CreateLogicMapData()
    {
        List<List<MapNodeData>> logicMap = new List<List<MapNodeData>>();
        
        int globalNodeCounter = 0; 
        for (int colIndex = 0; colIndex < chapterData.layers.Count; colIndex++)
        {
            LayerConfig layerConfig = chapterData.layers[colIndex];
            
            List<MapNodeData> currentColumnNodes = new List<MapNodeData>();

            int nodeCountInThisColumn = mapRNG.Next(layerConfig.minNodes, layerConfig.maxNodes + 1);

            List<NodeType> availableTypes = new List<NodeType>();
            List<float> weights = new List<float>();
            foreach (var weightConfig in layerConfig.nodeWeights)
            {
                availableTypes.Add(weightConfig.nodeType);
                weights.Add(weightConfig.weightRatio);
            }

            for (int rowIndex = 0; rowIndex < nodeCountInThisColumn; rowIndex++)
            {
                NodeType selectedType = GetRandomWeighted(availableTypes, weights, mapRNG);

                MapNodeData newNode = new MapNodeData
                {
                    nodeID = $"Node_{colIndex}_{globalNodeCounter}", 
                    type = selectedType,
                    layerIndex = colIndex,  
                    rowIndex = rowIndex     
                };

                currentColumnNodes.Add(newNode);
                globalNodeCounter++;
            }

            logicMap.Add(currentColumnNodes);
        }

        return logicMap;
    }
    public T GetRandomWeighted<T>(List<T> items, List<float> weights, System.Random rng)
    {
        float totalWeight = 0f;
        foreach (float weight in weights)
        {
            totalWeight += weight;
        }

        float randomValue = (float)rng.NextDouble() * totalWeight;

        for (int i = 0; i < items.Count; i++)
        {
            randomValue -= weights[i];

            if (randomValue <= 0f)
            {
                return items[i];
            }
        }
        return items[items.Count - 1]; 
    }
    void ConnectLayers(List<MapNodeData> layerA, List<MapNodeData> layerB, System.Random rng)
    {
        foreach (MapNodeData nodeA in layerA)
        {
            MapNodeData randomNodeB = layerB[rng.Next(layerB.Count)];
            nodeA.AddOutgoingEdge(randomNodeB);
            randomNodeB.AddIncomingEdge(nodeA);
        }

        foreach (MapNodeData nodeB in layerB)
        {
            if (nodeB.incomingEdges.Count == 0) 
            {
                MapNodeData randomNodeA = layerA[rng.Next(layerA.Count)];
                randomNodeA.AddOutgoingEdge(nodeB);
                nodeB.AddIncomingEdge(randomNodeA);
            }
        }
    
    }
    
}
