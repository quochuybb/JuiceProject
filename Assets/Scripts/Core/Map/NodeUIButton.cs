using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class NodeUIButton : MonoBehaviour
{
    private Button button;
    public NodeType nodeType;
    public MapNodeData myData;
    public Image iconImage;
    [SerializeField] private TextMeshProUGUI chapterType;

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    private void Start()
    {
        button.onClick.AddListener(OnClick);
    }

    public void OnClick()
    {
        GameSession.CurrentLayer =  myData.layerIndex;
        GameSession.CurrentChapter = myData.chapterIndex;
        GameSession.SelectedMode = myData.gameMode;
        GameSession.TargetScore = myData.targetScore;
        if (myData.type == NodeType.Hard)
        {
            GameSession.TargetScore = myData.targetScore + myData.targetScore*0.2f;
        }
        GameSession.CurrentNodeID = myData.nodeID;
        GameSession.type = myData.type;
        Debug.Log(myData.gameMode);
        SceneManager.LoadScene("Game");
    }
    public void Setup(MapNodeData data)
    {
        myData = data;
        switch (myData.type)
        {
            case NodeType.Easy:
                iconImage.color = Color.green;
                chapterType.text = "Easy";
                break;
            case NodeType.Hard:
                iconImage.color = Color.red;
                chapterType.text = "Hard";
                break;
            case NodeType.Shop:
                iconImage.color = Color.yellow;
                chapterType.text = "Shop";
                break;
            case NodeType.Boss:
                iconImage.color = Color.blue;
                chapterType.text = "Boss";
                break;
        }
    }
}
