using System;
using UnityEngine;

public enum InGamePhase
{
    None,
    Tutorial,
    BuildSelect,
    DiceRoll,
    TowerApply,
    Combat,
    ResultCheck,
    GameOver,
    Victory
}

public class InGameFlowManager : MonoBehaviour
{
    public static InGameFlowManager Instance;
    public InGamePhase CurrentPhase { get; private set; }

    public event Action<InGamePhase> OnPhaseEntered;
    public event Action<InGamePhase> OnPhaseExited;

    private void Awake()
    {
        if(null == Instance)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    public void EnterPhase(InGamePhase nextPhase)
    {
        if (CurrentPhase == nextPhase)
            return;

        if (CurrentPhase != InGamePhase.None)
        {
            OnPhaseExited?.Invoke(CurrentPhase);
        }

        CurrentPhase = nextPhase;
        OnPhaseEntered?.Invoke(CurrentPhase);
    }

    public void GoToNextPhase()
    {
        switch (CurrentPhase)
        {
            case InGamePhase.BuildSelect:
                EnterPhase(InGamePhase.DiceRoll);
                break;

            case InGamePhase.DiceRoll:
                EnterPhase(InGamePhase.TowerApply);
                break;

            case InGamePhase.TowerApply:
                EnterPhase(InGamePhase.Combat);
                break;

            case InGamePhase.Combat:
                EnterPhase(InGamePhase.ResultCheck);
                break;

            case InGamePhase.ResultCheck:
                EnterPhase(InGamePhase.BuildSelect);
                break;
        }
    }
}