using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using DG.Tweening;
using UnityEngine.EventSystems;

public class DiceUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public bool IsLocked { get; set; }
    public int currentDrawnValue { get; set; } 
    private int remainingRerolls;
    public int RemainingRerolls => remainingRerolls;

    private bool isRolling;
    public bool IsRolling 
    { 
        get => isRolling; 
        set 
        {
            isRolling = value;
            UpdateRerollTextVisibility(); 
        }
    } 

    [Header("UI References")]
    public TextMeshProUGUI valueText;
    public TextMeshProUGUI remainRerollText; 
    
    [Header("Animation References")]
    public RectTransform mover; 
    public Transform originalParent { get; private set; }

    public Action<DiceUI, int> OnRerollRequested; 
    public Action<DiceUI> OnHoverEntered; 
    public Action<DiceUI> OnHoverExited;  

    private Tween scaleTween;
    private Tween colorTween;
    private Tween textFadeTween; 

    private float currentTargetScale = 1.0f;
    private Color currentTargetColor = Color.white;
    private Image moverBackgroundImage; 
    
    private bool isHovered = false; 

    private String countOutText = "리롤 불가!";
    private String canRollText = "남은 횟수: ";

    private void Awake()
    {
        moverBackgroundImage = mover.GetComponent<Image>();
        
        if (remainRerollText != null)
        {
            Color c = remainRerollText.color;
            c.a = 0f;
            remainRerollText.color = c;
            remainRerollText.gameObject.SetActive(true); 
        }
    }

    #region 세팅 관련 함수
    public void Setup(int maxRerolls)
    {
        remainingRerolls = maxRerolls;
        originalParent = mover.parent;
        isHovered = false; 
        
        SetValueInstant(UnityEngine.Random.Range(1, 7));
    }

    public void SetValueInstant(int val)
    {
        currentDrawnValue = val;
        valueText.text = currentDrawnValue.ToString();
        UpdateInteraction();
    }

    public void SetRaycastTarget(bool isActive)
    {
        if (moverBackgroundImage != null)
        {
            moverBackgroundImage.raycastTarget = isActive;
        }
    }
    #endregion

    #region 마우스 감지 로직
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (IsRolling || IsLocked) return; 

        isHovered = true;
        UpdateRerollTextVisibility(); 

        OnHoverEntered?.Invoke(this); 
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (IsRolling || IsLocked) return;

        isHovered = false;
        UpdateRerollTextVisibility(); 

        OnHoverExited?.Invoke(this); 
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (remainingRerolls > 0 && !IsRolling && !IsLocked)
        {
            remainingRerolls--;
            int newValue = UnityEngine.Random.Range(1, 7);
            OnRerollRequested?.Invoke(this, newValue); 
        }
    }

    public void CheckHoverStateAfterRolling()
    {
        Vector2 mousePos = Input.mousePosition;
        RectTransform rectTransform = GetComponent<RectTransform>();
        Camera cam = (rectTransform.root.GetComponent<Canvas>().renderMode == RenderMode.ScreenSpaceOverlay) ? null : Camera.main;

        if (RectTransformUtility.RectangleContainsScreenPoint(rectTransform, mousePos, cam))
        {
            OnPointerEnter(null); 
        }
        else
        {
            if (isHovered) OnPointerExit(null);
        }
    }

    #endregion

    #region 애니메이션, 상태갱신 관련 함수
    private void UpdateRerollTextVisibility()
    {
        if (remainRerollText == null) return;

        textFadeTween?.Kill(); 

        if (IsRolling)
        {
            textFadeTween = remainRerollText.DOFade(0f, 0.2f);
            return;
        }

        if (remainingRerolls <= 0)
        {
            remainRerollText.text = countOutText;
            Color redColor = Color.red;
            redColor.a = remainRerollText.color.a; 
            remainRerollText.color = redColor;     
            textFadeTween = remainRerollText.DOFade(1f, 0.3f); 
        }
        else
        {
            remainRerollText.text = canRollText + remainingRerolls;
            Color blackColor = Color.black;
            blackColor.a = remainRerollText.color.a;
            remainRerollText.color = blackColor;

            float targetAlpha = isHovered ? 1f : 0f;
            textFadeTween = remainRerollText.DOFade(targetAlpha, 0.3f); 
        }
    }

    public void SetHoverVisuals(float targetScale, Color targetColor)
    {
        if (!Mathf.Approximately(currentTargetScale, targetScale))
        {
            currentTargetScale = targetScale;
            scaleTween?.Kill();
            scaleTween = mover.DOScale(targetScale, 0.3f).SetEase(Ease.OutQuad);
        }

        if (moverBackgroundImage != null && currentTargetColor != targetColor)
        {
            currentTargetColor = targetColor;
            colorTween?.Kill();
            colorTween = moverBackgroundImage.DOColor(targetColor, 0.3f); 
        }
    }

    public void ResetVisualsImmediate()
    {
        scaleTween?.Kill();
        colorTween?.Kill();
        
        currentTargetScale = 1.0f;
        
        currentTargetColor = (remainingRerolls <= 0 && !IsRolling) ? new Color(0.6f, 0.6f, 0.6f, 1f) : Color.white;
        
        mover.localScale = Vector3.one; 
        if (moverBackgroundImage != null) moverBackgroundImage.color = currentTargetColor;
        
        if (remainingRerolls > 0 || IsRolling)
        {
            textFadeTween?.Kill(); 
            if (remainRerollText != null)
            {
                Color c = remainRerollText.color;
                c.a = 0f;
                remainRerollText.color = c;
            }
            isHovered = false; 
        }
    }

    public void AnimateToNewLayoutPosition(Vector3 oldWorldPos)
    {
        mover.position = oldWorldPos;
        mover.DOLocalMove(Vector3.zero, 0.4f).SetEase(Ease.OutCubic);
    }

    public void UpdateInteraction() 
    {
        UpdateRerollTextVisibility(); 
    }

    #endregion
}