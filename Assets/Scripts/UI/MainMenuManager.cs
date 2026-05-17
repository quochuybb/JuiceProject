using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public static MainMenuManager Instance { get; private set; }
    [SerializeField] private float transitionDuration = 0.5f;
    [SerializeField] private Ease easeType = Ease.OutExpo;
    private Vector2 centerPosition = Vector2.zero;
    private Vector2 leftOffScreen = new Vector2(-1920f, 0f); 
    private Vector2 rightOffScreen = new Vector2(1920f, 0f);
    [SerializeField] private RectTransform mainMenuPanel;
    [SerializeField] private RectTransform campaignPanel;
    [SerializeField] private RectTransform mapPanel;
    [SerializeField] private RectTransform shopPanel;

    private void Awake()
    {
        Instance = this;
    }
    public void OnPlayChapterButton()
    {
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlayUIClick();
        Debug.Log("Open Chapter");
        SlidePanel(campaignPanel, mapPanel);
    }
    public void OnShopButton()
    {
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlayUIClick();
        Debug.Log("Open Shop");
        SlidePanel(mainMenuPanel, shopPanel);
    }

    public void OnPlayCampaignButton()
    {
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlayUIClick();
        Debug.Log("Open Campaign");
        SlidePanel(mainMenuPanel, campaignPanel);
    }

    public void OnBackMainMenuFromCampaignButton()
    {
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlayUIClick();
        Debug.Log("Back to Main Menu");
        SlidePanel(campaignPanel,mainMenuPanel);
    }
    public void OnBackMainMenuFromShopButton()
    {
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlayUIClick();
        Debug.Log("Back to Main Menu");
        SlidePanel(shopPanel,mainMenuPanel);
    }
    public void OnBackCampaignFromChapterButton()
    {
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlayUIClick();
        Debug.Log("Back to Main Menu");
        SlidePanel(mapPanel,campaignPanel);
    }
    public void OnPlayMultiplayerButton()
    {
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlayUIClick();

        Debug.Log("Open Lobby");
    }
    public void OnSettingButton()
    {
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlayUIClick();

        Debug.Log("Open Settings");
    }
    public void SlidePanel(RectTransform panelOld, RectTransform panelNew)
    {
        panelOld.DOAnchorPos(rightOffScreen, transitionDuration)
            .SetEase(easeType);

        panelNew.DOAnchorPos(centerPosition, transitionDuration)
            .SetEase(easeType);
    } 
    public void OnQuitButton()
    {
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlayUIClick();

        Application.Quit();

    #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
    #endif
    }
}