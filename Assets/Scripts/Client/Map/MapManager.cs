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
    
    public void ReloadCurrentMap()
    {
        if (GameSession.CurrentChapterData == null) return;

        currentChapterData = GameSession.CurrentChapterData;
        currentSeed = GameSession.CurrentMapSeed;

        foreach (Transform child in mapContainerTransform)
        {
            Destroy(child.gameObject);
        }

        GenerateMapGraph();
    }

    public void StartChapterMap(ChapterData chapter)
    {
        MainMenuManager.Instance.OnPlayChapterButton();
        
        if ((GameSession.CurrentChapterData == chapter || GameSession.CurrentChapterID == chapter.chapterID) && GameSession.CurrentMapSeed != 0)
        {
            currentSeed = GameSession.CurrentMapSeed;
        }
        else
        {
            currentSeed = chapter.chapterID.GetHashCode() + UnityEngine.Random.Range(0, 1000);
            GameSession.CurrentMapSeed = currentSeed;
            GameSession.CompletedNodes.Clear(); 
        }

        currentChapterData = chapter;
        GameSession.CurrentChapterData = chapter;
        GameSession.CurrentChapterID = chapter.chapterID;
        
        foreach (Transform child in mapContainerTransform)
        {
            Destroy(child.gameObject);
        }

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
        UpdateNodeStates(logicMap);
        DrawMapToScreen(logicMap);
    }
    private void UpdateNodeStates(List<List<MapNodeData>> logicMap)
    {
        // 1. Trường hợp mới bắt đầu (Chưa hoàn thành Node nào)
        if (GameSession.CompletedNodes.Count == 0)
        {
            // Mở khóa toàn bộ Node ở Tầng đầu tiên (Layer 0)
            foreach (MapNodeData node in logicMap[0])
            {
                node.state = NodeState.Available;
            }
            return;
        }

        // 2. Đã có tiến trình chơi
        MapNodeData lastCompletedNode = null;

        // Quét toàn bộ bản đồ để đánh dấu các Node đã hoàn thành
        foreach (var layer in logicMap)
        {
            foreach (var node in layer)
            {
                if (GameSession.CompletedNodes.Contains(node.nodeID))
                {
                    node.state = NodeState.Completed;
                    lastCompletedNode = node; // Node cuối cùng trong list chính là vị trí hiện tại
                }
            }
        }

        // 3. Mở khóa các con đường kết nối với Node vừa hoàn thành
        if (lastCompletedNode != null)
        {
            foreach (MapNodeData nextNode in lastCompletedNode.outgoingEdges)
            {
                if (nextNode.state != NodeState.Completed)
                {
                    nextNode.state = NodeState.Available;
                }
            }
        }
    }

    private void DrawMapToScreen(List<List<MapNodeData>> logicMap)
    {
        // Xóa toàn bộ Node cũ trước khi vẽ lại để tránh bị chồng chất
        foreach (Transform child in mapContainerTransform)
        {
            Destroy(child.gameObject);
        }

        float layerSpacing = 250f; 
        float nodeSpacing = 200f;  
        float topPadding = 150f;   
        float bottomPadding = 300f;

        float totalHeight = ((logicMap.Count - 1) * layerSpacing) + topPadding + bottomPadding;
    
        contentRectTransform.sizeDelta = new Vector2(contentRectTransform.sizeDelta.x, totalHeight);

        // 1. Tính toán và lưu vị trí của tất cả các node
        Dictionary<MapNodeData, Vector2> nodePositions = new Dictionary<MapNodeData, Vector2>();
        
        for (int currentLayerIndex = 0; currentLayerIndex < logicMap.Count; currentLayerIndex++)
        {
            List<MapNodeData> currentLayerNodes = logicMap[currentLayerIndex];
            int totalNodesInLayer = currentLayerNodes.Count;

            foreach (var nodeData in currentLayerNodes)
            {
                float posY = -(currentLayerIndex * layerSpacing) - topPadding; 
                float posX = (nodeData.nodeIndexInLayer - (totalNodesInLayer - 1) / 2.0f) * nodeSpacing;
                nodePositions[nodeData] = new Vector2(posX, posY);
            }
        }

        // 2. Vẽ các đường nối (Draw Lines) trước để nó nằm dưới các Node Button
        foreach (var layer in logicMap)
        {
            foreach (var node in layer)
            {
                if (node.outgoingEdges != null && nodePositions.ContainsKey(node))
                {
                    Vector2 startPos = nodePositions[node];
                    foreach (var nextNode in node.outgoingEdges)
                    {
                        if (nodePositions.ContainsKey(nextNode))
                        {
                            Vector2 endPos = nodePositions[nextNode];
                            DrawLineUI(startPos, endPos);
                        }
                    }
                }
            }
        }

        // 3. Khởi tạo và đặt vị trí các Node Buttons
        for (int currentLayerIndex = 0; currentLayerIndex < logicMap.Count; currentLayerIndex++)
        {
            List<MapNodeData> currentLayerNodes = logicMap[currentLayerIndex];

            foreach (var nodeData in currentLayerNodes)
            {
                GameObject newButtonGO = Instantiate(nodePrefab, mapContainerTransform);
                NodeUIButton uiScript = newButtonGO.GetComponent<NodeUIButton>();
                uiScript.Setup(nodeData);

                RectTransform rect = newButtonGO.GetComponent<RectTransform>();
                rect.anchoredPosition = nodePositions[nodeData];
            }
        }
    }

    private void DrawLineUI(Vector2 startPos, Vector2 endPos)
    {
        GameObject lineObj = new GameObject("Line_Connection", typeof(Image));
        lineObj.transform.SetParent(mapContainerTransform, false);
        
        Image img = lineObj.GetComponent<Image>();
        img.color = new Color(0.3f, 0.3f, 0.3f, 0.8f); // Màu xám mờ để phân biệt với nút
        img.raycastTarget = false; // Tắt raycast để không block các lượt click chuột
        
        RectTransform rect = lineObj.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 1f); // Giống với nodePrefab
        rect.anchorMax = new Vector2(0.5f, 1f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        
        Vector2 dir = endPos - startPos;
        float distance = dir.magnitude;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        
        // Độ dày của vạch kẻ (height) là 15, chiều dài (width) là khoảng cách giữa 2 điểm
        rect.sizeDelta = new Vector2(distance, 15f); 
        rect.anchoredPosition = startPos + dir / 2f; // Vị trí điểm giữa
        rect.localRotation = Quaternion.Euler(0, 0, angle); // Xoay hướng
    }

    private List<List<MapNodeData>> CreateLogicMapData()
    {
        List<List<MapNodeData>> logicMap = new List<List<MapNodeData>>();
        int globalNodeCounter = 0; 
        
        int chapterNumber = 1;
        if (!int.TryParse(currentChapterData.chapterID, out chapterNumber))
        {
            Debug.LogWarning($"Chapter ID '{currentChapterData.chapterID}' invalid number, default use 1");
        }

        List<GameModeType> availableGameModes = new List<GameModeType> 
        { 
            GameModeType.TargetScore, 
            GameModeType.GemMission, 
            GameModeType.RecipeCrafting 
        };
        List<float> gameModeWeights = new List<float> { 0.7f, 0.2f, 0.1f };
        
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
                GameModeType selectedMode = GetRandomWeighted(availableGameModes, gameModeWeights, mapRNG);
                
                int calculatedScore = 0;
                if (selectedMode == GameModeType.TargetScore)
                {
                    calculatedScore = (chapterNumber * 100) + (layerIndex * 10);
                }

                MapNodeData newNode = new MapNodeData
                {
                    nodeID = $"Node_{layerIndex}_{globalNodeCounter}", 
                    type = selectedType,
                    layerIndex = layerIndex,  
                    nodeIndexInLayer = nodeIndex,
                    gameMode = selectedMode,
                    targetScore = calculatedScore,
                    chapterIndex = chapterNumber
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