using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class NodeUIButton : MonoBehaviour
{
    private Button button;
    public NodeType nodeType;
    public MapNodeData myData;
    public Image iconImage;

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
        SceneManager.LoadScene("Game");
    }
    public void Setup(MapNodeData data)
    {
        myData = data;
        switch (myData.type)
        {
            case NodeType.Easy:
                iconImage.color = Color.green;
                break;
            case NodeType.Hard:
                iconImage.color = Color.red;
                break;
            case NodeType.Shop:
                iconImage.color = Color.yellow;
                break;
        }
    }
}
