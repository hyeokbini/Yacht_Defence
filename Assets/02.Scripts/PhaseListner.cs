using UnityEngine;

public abstract class PhaseListener : MonoBehaviour
{
    protected virtual void Start()
    {
        // FlowManager가 준비되어 있으면 구독
        if (InGameFlowManager.Instance != null)
        {
            Subscribe();
        }
        else
        {
            Debug.LogWarning($"{name}: FlowManager 아직 준비 안됨. Start 시 다시 확인 필요.");
        }
    }

    protected virtual void OnDisable()
    {
        if (InGameFlowManager.Instance != null)
        {
            InGameFlowManager.Instance.OnPhaseEntered -= HandlePhaseEntered;
            InGameFlowManager.Instance.OnPhaseExited -= HandlePhaseExited;
        }
    }

    private void Subscribe()
    {
        InGameFlowManager.Instance.OnPhaseEntered += HandlePhaseEntered;
        InGameFlowManager.Instance.OnPhaseExited += HandlePhaseExited;
    }

    protected virtual void HandlePhaseEntered(InGamePhase phase) { }
    protected virtual void HandlePhaseExited(InGamePhase phase) { }
}