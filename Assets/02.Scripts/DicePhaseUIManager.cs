using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using DG.Tweening;

public class DicePhaseUIManager : PhaseListener
{
    [Header("UI Panels")]
    public GameObject dicePhasePanel;
    public Transform diceContainer;
    public List<DiceUI> diceList;
    public Button confirmButton;

    [Header("Dependencies")]
    public TowerPlacementManager towerPlacementManager;

    [Header("Settings")]
    public int defaultRerollCount = 1;

    private DiceUI currentlyHoveredDice = null;

    protected override void Start()
    {
        base.Start(); 
        confirmButton.onClick.AddListener(OnConfirmClicked); 
        dicePhasePanel.SetActive(false); 
    }

    protected override void HandlePhaseEntered(InGamePhase phase)
    {
        if (phase == InGamePhase.DiceRoll) 
        {
            StartDicePhase();
        }
    }

    protected override void HandlePhaseExited(InGamePhase phase)
    {
        if (phase == InGamePhase.DiceRoll)
        {
            dicePhasePanel.SetActive(false);
            foreach (var dice in diceList)
            {
                dice.OnRerollRequested -= HandleDiceRerollRequest;
                dice.OnHoverEntered -= HandleDiceHoverEntered;
                dice.OnHoverExited -= HandleDiceHoverExited;
            }
        }
    }

    #region 초기화 / 셋업
    private void StartDicePhase()
    {
        dicePhasePanel.SetActive(true);
        DiceSetup();
        SortDiceInstant();
    }

    private void DiceSetup()
    {
        foreach (var dice in diceList)
        {
            dice.OnRerollRequested -= HandleDiceRerollRequest;
            dice.OnRerollRequested += HandleDiceRerollRequest;
            
            dice.OnHoverEntered -= HandleDiceHoverEntered;
            dice.OnHoverEntered += HandleDiceHoverEntered;
            
            dice.OnHoverExited -= HandleDiceHoverExited;
            dice.OnHoverExited += HandleDiceHoverExited;

            dice.Setup(defaultRerollCount);
        }
    }

    private void SortDiceInstant()
    {
        var sortedDice = diceList
            .OrderBy(d => d.currentDrawnValue)
            .ThenBy(d => d.transform.GetSiblingIndex())
            .ToList();
            
        for (int i = 0; i < sortedDice.Count; i++)
        {
            sortedDice[i].transform.SetSiblingIndex(i);
        }
        LayoutRebuilder.ForceRebuildLayoutImmediate(diceContainer.GetComponent<RectTransform>());
    }
    #endregion

    #region 호버링 지휘 관련
    private bool IsAnyDiceRolling()
    {
        return diceList.Any(d => d.IsRolling);
    }

    private void HandleDiceHoverEntered(DiceUI hoveredDice)
    {
        if (IsAnyDiceRolling()) return; 

        currentlyHoveredDice = hoveredDice;
        UpdateHoverVisuals(); 
    }

    private void HandleDiceHoverExited(DiceUI hoveredDice)
    {
        if (IsAnyDiceRolling()) return; 

        if (currentlyHoveredDice == hoveredDice)
        {
            currentlyHoveredDice = null;
            UpdateHoverVisuals();
        }
    }

    private void UpdateHoverVisuals()
    {
        Color dimmedColor = new Color(0.6f, 0.6f, 0.6f, 1f);

        // 현재 포커스된 주사위가 리롤 가능 주사위인지 확인
        bool isHoveringAliveDice = currentlyHoveredDice != null && currentlyHoveredDice.RemainingRerolls > 0;

        foreach (var dice in diceList)
        {
            float targetScale;
            Color targetColor;

            // 1 살아있는 주사위를 가리키고 있지 않다면 -> 기본 밝은 상태로
            if (!isHoveringAliveDice)
            {
                targetScale = 1.0f;
                targetColor = Color.white;
            }
            // 2 살아있는 주사위를 가리키고 있고, 그게 나라면 -> 강조
            else if (dice == currentlyHoveredDice)
            {
                targetScale = 1.1f;
                targetColor = Color.white;
            }
            // 3 살아있는 주사위를 가리키고 있는데, 나는 아니라면 -> 회색
            else
            {
                targetScale = 1.0f;
                targetColor = dimmedColor;
            }

            // 내 횟수가 0이면 무조건 회색, 원래크기
            if (dice.RemainingRerolls <= 0 && !dice.IsRolling)
            {
                targetColor = dimmedColor;
                targetScale = 1.0f; 
            }

            dice.SetHoverVisuals(targetScale, targetColor);
        }
    }
    #endregion

    #region 리롤 애니메이션 루틴 관련
    private void HandleDiceRerollRequest(DiceUI rerolledDice, int newValue)
    {
        StartCoroutine(RerollAnimationRoutine(rerolledDice, newValue));
    }

    private IEnumerator RerollAnimationRoutine(DiceUI dice, int newValue)
    {
        dice.IsRolling = true;
        
        // 애니메이션 시작 - 모든 주사위 강제 잠금
        foreach (var d in diceList) 
        {
            d.IsLocked = true; 
            d.SetRaycastTarget(false); 
            d.ResetVisualsImmediate();
        }

        CheckMove(dice, newValue, out List<DiceUI> sortedDice, out bool needsToMove);

        dice.valueText.text = newValue.ToString();

        yield return PlayPopOutAnimation(dice, needsToMove);

        MoveDice(dice, sortedDice);

        yield return PlayInsertAnimation(dice, needsToMove);

        dice.IsRolling = false;
        
        // 애니메이션 종료 - 모든 주사위 잠금 해제
        foreach (var d in diceList)
        {
            d.IsLocked = false;
        }

        yield return null; // 1프레임 대기 (마우스 이벤트 갱신용)

        currentlyHoveredDice = null;
        UpdateHoverVisuals();
        
        // 잠금이 풀린 상태에서 현재 마우스가 어디에 있는지 재검사
        foreach (var d in diceList)
        {
            d.SetRaycastTarget(true); 
            d.UpdateInteraction();    
            d.CheckHoverStateAfterRolling();
        }
    }

    private void CheckMove(DiceUI dice, int newValue, out List<DiceUI> sortedDice, out bool needsToMove)
    {
        int oldIndex = dice.transform.GetSiblingIndex();
        dice.currentDrawnValue = newValue;

        sortedDice = diceList
            .OrderBy(d => d.currentDrawnValue)
            .ThenBy(d => d.transform.GetSiblingIndex()) 
            .ToList();

        int newIndex = sortedDice.IndexOf(dice);

        needsToMove = oldIndex != newIndex;
    }

    private IEnumerator PlayPopOutAnimation(DiceUI dice, bool needsToMove)
    {
        if (needsToMove)
        {
            dice.mover.SetParent(dicePhasePanel.transform, true);
            dice.mover.DOAnchorPosY(dice.mover.anchoredPosition.y - 150f, 0.3f).SetEase(Ease.OutQuad);
            yield return new WaitForSeconds(0.3f);
        }
        else
        {
            dice.mover.DOPunchScale(Vector3.one * 0.2f, 0.3f, 10, 1);
            yield return new WaitForSeconds(0.15f);
        }
    }

    private void MoveDice(DiceUI dice, List<DiceUI> sortedDice)
    {
        Dictionary<DiceUI, Vector3> oldPositions = new Dictionary<DiceUI, Vector3>();
        foreach (var d in diceList)
        {
            if (d != dice) oldPositions[d] = d.mover.position;
        }

        for (int i = 0; i < sortedDice.Count; i++)
        {
            sortedDice[i].transform.SetSiblingIndex(i);
        }
        LayoutRebuilder.ForceRebuildLayoutImmediate(diceContainer.GetComponent<RectTransform>());

        foreach (var d in diceList)
        {
            if (d != dice && oldPositions.ContainsKey(d))
            {
                d.AnimateToNewLayoutPosition(oldPositions[d]);
            }
        }
    }

    private IEnumerator PlayInsertAnimation(DiceUI dice, bool needsToMove)
    {
        if (needsToMove)
        {
            dice.mover.DOMoveX(dice.originalParent.position.x, 0.4f).SetEase(Ease.InOutSine);
            yield return new WaitForSeconds(0.4f);

            dice.mover.DOMoveY(dice.originalParent.position.y, 0.3f).SetEase(Ease.InQuad);
            yield return new WaitForSeconds(0.3f);

            dice.mover.SetParent(dice.originalParent, true);
            dice.mover.anchoredPosition = Vector2.zero;
        }
        else
        {
            yield return new WaitForSeconds(0.15f);
        }
    }
    #endregion

    #region 결과 확정 및 족보 계산
    private void OnConfirmClicked()
    {
        int[] finalValues = diceList.Select(d => d.currentDrawnValue).ToArray();
        DiceHandResult result = EvaluateDice(finalValues);
        towerPlacementManager.ConfirmAndPlaceTower(result);
        InGameFlowManager.Instance.GoToNextPhase();
    }

    private DiceHandResult EvaluateDice(int[] diceValues)
    {
        return DiceHandEvaluator.Evaluate(diceValues);
    }
    #endregion
}