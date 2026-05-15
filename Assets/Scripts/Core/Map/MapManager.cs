using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = System.Random;

public class MapManager : MonoBehaviour
{
    public static MapManager Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private GameObject nodePrefab;
    [SerializeField] private Transform mapContainerTransform;

    [Header("Runtime Data")]
    [SerializeField] private ChapterData currentChapterData;
    private int currentSeed;
    private Random mapRNG;
    [SerializeField] private RectTransform contentRectTransform;
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void StartChapterMap(ChapterData chapter)
    {
        MainMenuManager.Instance.OnPlayChapterButton();
        currentChapterData = chapter;
        currentSeed = chapter.chapterID.GetHashCode() + UnityEngine.Random.Range(0, 1000);
        
        GenerateMapGraph();
    }

    private void GenerateMapGraph()
    {
        mapRNG = new Random(currentSeed);
        
        List<List<MapNodeData>> logicMap = CreateLogicMapData(); 

        for (int i = 0; i < logicMap.Count - 1; i++)
        {
            ConnectLayers(logicMap[i], logicMap[i + 1], mapRNG);
        }

        DrawMapToScreen(logicMap);
    }

    private void DrawMapToScreen(List<List<MapNodeData>> logicMap)
    {
        float layerSpacing = 250f; 
        float nodeSpacing = 200f;  
        float topPadding = 150f;   
        float bottomPadding = 300f;

        float totalHeight = ((logicMap.Count - 1) * layerSpacing) + topPadding + bottomPadding;
    
        contentRectTransform.sizeDelta = new Vector2(contentRectTransform.sizeDelta.x, totalHeight);

        for (int currentLayerIndex = 0; currentLayerIndex < logicMap.Count; currentLayerIndex++)
        {
            List<MapNodeData> currentLayerNodes = logicMap[currentLayerIndex];
            int totalNodesInLayer = currentLayerNodes.Count;

            foreach (var nodeData in currentLayerNodes)
            {
                GameObject newButtonGO = Instantiate(nodePrefab, mapContainerTransform);
                NodeUIButton uiScript = newButtonGO.GetComponent<NodeUIButton>();
                uiScript.Setup(nodeData);

                float posY = -(currentLayerIndex * layerSpacing) - topPadding; 
                float posX = (nodeData.nodeIndexInLayer - (totalNodesInLayer - 1) / 2.0f) * nodeSpacing;

                RectTransform rect = newButtonGO.GetComponent<RectTransform>();
                rect.anchoredPosition = new Vector2(posX, posY);
            }
        }
    }

    private List<List<MapNodeData>> CreateLogicMapData()
    {
        List<List<MapNodeData>> logicMap = new List<List<MapNodeData>>();
        int globalNodeCounter = 0; 
        
        for (int layerIndex = 0; layerIndex < currentChapterData.layers.Count; layerIndex++)
        {
            LayerConfig layerConfig = currentChapterData.layers[layerIndex];
            List<MapNodeData> nodesInCurrentLayer = new List<MapNodeData>();

            int nodeCountInThisLayer = mapRNG.Next(layerConfig.minNodes, layerConfig.maxNodes + 1);

            List<NodeType> availableTypes = new List<NodeType>();
            List<float> weights = new List<float>();
            foreach (var weightConfig in layerConfig.nodeWeights)
            {
                availableTypes.Add(weightConfig.nodeType);
                weights.Add(weightConfig.weightRatio);
            }

            for (int nodeIndex = 0; nodeIndex < nodeCountInThisLayer; nodeIndex++)
            {
                NodeType selectedType = GetRandomWeighted(availableTypes, weights, mapRNG);

                MapNodeData newNode = new MapNodeData
                {
                    nodeID = $"Node_{layerIndex}_{globalNodeCounter}", 
                    type = selectedType,
                    layerIndex = layerIndex,  
                    nodeIndexInLayer = nodeIndex 
                };

                nodesInCurrentLayer.Add(newNode);
                globalNodeCounter++;
            }

            logicMap.Add(nodesInCurrentLayer);
        }

        return logicMap;
    }

    private T GetRandomWeighted<T>(List<T> items, List<float> weights, Random rng)
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

    private void ConnectLayers(List<MapNodeData> upperLayer, List<MapNodeData> lowerLayer, Random rng)
    {
        foreach (MapNodeData upperNode in upperLayer)
        {
            MapNodeData randomLowerNode = lowerLayer[rng.Next(lowerLayer.Count)];
            upperNode.AddOutgoingEdge(randomLowerNode);
            randomLowerNode.AddIncomingEdge(upperNode);
        }

        foreach (MapNodeData lowerNode in lowerLayer)
        {
            if (lowerNode.incomingEdges.Count == 0) 
            {
                MapNodeData randomUpperNode = upperLayer[rng.Next(upperLayer.Count)];
                randomUpperNode.AddOutgoingEdge(lowerNode);
                lowerNode.AddIncomingEdge(randomUpperNode);
            }
        }
    }
}