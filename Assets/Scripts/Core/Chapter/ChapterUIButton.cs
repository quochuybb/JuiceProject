using System;
using UnityEngine;
using UnityEngine.UI;

public class ChapterUIButton : MonoBehaviour
{
    private Button button;
    public ChapterData chapterData;

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
        if (chapterData != null)
        {
            MapManager.Instance.GenerateSeed(chapterData);
        }
    }

    public void OnDestroy()
    {
        button.onClick.RemoveAllListeners();
    }
}
