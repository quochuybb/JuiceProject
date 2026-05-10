using System;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class CellUI : MonoBehaviour
{
    public CellData cell = new CellData(); 
    
    [SerializeField] private Image background; 
    [SerializeField] private Image number;
    [SerializeField] private Image gem;
    
    public float animDuration = 0.125f; 
    private Coroutine shakeCoroutine;
    
    private bool isUsingActiveColor = false;
    private Sequence currentMatchSeq;

    public event Action<CellUI> OnCellClicked;

    void Start()
    {
        if (background != null)
        {
            Color currentColor = background.color;
            currentColor.a = 0f; 
            background.color = currentColor;
        }
    }

    public void ClickCell()
    {
        if (background == null || cell.isCleared) return; 
        
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlayChooseNumber();

        OnCellClicked?.Invoke(this);
    }

    public void OnMatchSuccess()
    {
        cell.isCleared = true; 
        if (cell.hasGem)
        {
            Color cGem = gem.color;
            cGem.a = 0f;
            gem.color = cGem;
        }
        if (currentMatchSeq != null) currentMatchSeq.Kill();
        background.transform.DOKill();
        if (number != null) number.DOKill();

        isUsingActiveColor = false;

        Color cBg = background.color;
        cBg.a = 1f;
        background.color = cBg;

        float matchAnimTime = animDuration * 2.5f; 

        currentMatchSeq = DOTween.Sequence();

        currentMatchSeq.AppendInterval(0.15f)
            .Append(background.transform.DOScale(Vector3.zero, matchAnimTime).SetEase(Ease.InBack))
            .Join(number.DOFade(0.25f, matchAnimTime)) 
            .AppendCallback(() => 
            {
                cBg.a = 0f;
                background.color = cBg;
                background.transform.localScale = Vector3.one; 
            });
    }

    public void ResetVisualState()
    {
        if (currentMatchSeq != null) currentMatchSeq.Kill();
        background.transform.DOKill();
        if (number != null) number.DOKill();

        isUsingActiveColor = false;

        if (background != null)
        {
            Color cBg = background.color;
            cBg.a = 0f;
            background.color = cBg;
            background.transform.localScale = Vector3.one;
        }

        if (number != null)
        {
            Color cNum = number.color;
            cNum.a = 1f; 
            number.color = cNum;
            number.transform.localScale = Vector3.one;
            number.gameObject.SetActive(true); 
        }
    }
    
    public void ToggleSelection(bool isSelected)
    {
        if (isUsingActiveColor == isSelected) return; 
        isUsingActiveColor = isSelected;

        background.transform.DOKill(); 

        Sequence popSeq = DOTween.Sequence();
        popSeq.Append(background.transform.DOScale(Vector3.zero, animDuration)) 
            .AppendCallback(() => 
            {
                Color currentColor = background.color;
                currentColor.a = isUsingActiveColor ? 1f : 0f;
                background.color = currentColor;
            })
            .Append(background.transform.DOScale(Vector3.one, animDuration));                          
    }

    public void Shake()
    {
        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
        }
        shakeCoroutine = StartCoroutine(ShakeRoutine());
    }

    private System.Collections.IEnumerator ShakeRoutine()
    {
        Vector3 originalPos = transform.localPosition;
        float elapsed = 0f;
        float duration = 0.25f; 
        float magnitude = 5f;  

        while (elapsed < duration)
        {
            float offsetX = UnityEngine.Random.Range(-1f, 1f) * magnitude;
            transform.localPosition = new Vector3(originalPos.x + offsetX, originalPos.y, originalPos.z);
            
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = originalPos;
        shakeCoroutine = null;
    }
    private void OnDestroy()
    {
        if (currentMatchSeq != null) currentMatchSeq.Kill();
        if (background != null) background.transform.DOKill();
        if (number != null) number.DOKill();
        
        transform.DOKill(); 
    }
}