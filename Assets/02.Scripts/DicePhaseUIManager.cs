using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

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
                dice.OnRollCompleted -= HandleDiceRerolled;
            }
        }
    }

    private void StartDicePhase()
    {
        dicePhasePanel.SetActive(true);

        foreach (var dice in diceList)
        {
            dice.OnRollCompleted -= HandleDiceRerolled;
            dice.OnRollCompleted += HandleDiceRerolled;
            
            dice.Setup(defaultRerollCount); 
        }
    }

    private void HandleDiceRerolled(DiceUI rerolledDice)
    {
        SortDice();
    }

    private void SortDice()
    {
        var sortedDice = diceList.OrderBy(d => d.currentDrawnValue).ToList();
        for (int i = 0; i < sortedDice.Count; i++)
        {
            sortedDice[i].transform.SetSiblingIndex(i);
        }
    }

    private void OnConfirmClicked()
    {
        // 1. 주사위 5개의 최종 값을 배열로 추출
        int[] finalValues = diceList.Select(d => d.currentDrawnValue).ToArray();

        // 2. 추출된 값으로 족보 계산
        DiceHandResult result = EvaluateDice(finalValues);

        // 3. 타워 매니저에 족보 결과 전달하여 타워 생성
        if (towerPlacementManager != null)
        {
            towerPlacementManager.ConfirmAndPlaceTower(result);
        }

        // 4. 배치 완료되면 다음 페이즈로
        if (InGameFlowManager.Instance != null)
        {
            InGameFlowManager.Instance.GoToNextPhase();
        }
    }

    private DiceHandResult EvaluateDice(int[] diceValues)
    {
        return DiceHandEvaluator.Evaluate(diceValues);
    }
}