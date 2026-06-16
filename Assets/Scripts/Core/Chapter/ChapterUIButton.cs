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
        UpdateButtonState();
    }

    private void UpdateButtonState()
    {
        if (chapterData != null && int.TryParse(chapterData.chapterID, out int chapterNumber))
        {
            // Chỉ cho phép bấm vào Chapter trùng với tiến trình hiện tại của người chơi
            if (chapterNumber == GameSession.CurrentChapter)
            {
                button.interactable = true;
                // Có thể thêm code đổi màu nút ở đây nếu muốn (ví dụ: nút sáng lên)
            }
            else
            {
                button.interactable = false;
                // Nút sẽ tự động bị mờ đi do tính năng interactable của Unity Button
            }
        }
    }

    public void OnClick()
    {
        if (chapterData != null && button.interactable)
        {
            MapManager.Instance.StartChapterMap(chapterData);
        }
    }

    public void OnDestroy()
    {
        button.onClick.RemoveAllListeners();
    }
}
