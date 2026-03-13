using UnityEngine;

public class CameraDragController : PhaseListener
{
    private Camera targetCamera;
    [SerializeField] private float dragSpeed = 1f;

    private bool isDragging = false;
    private Vector3 lastMouseWorldPos;

    private void Awake()
    {
        if (targetCamera == null)
            targetCamera = Camera.main;
    }

    protected override void HandlePhaseEntered(InGamePhase phase)
    {
        if (IsDragPhase(phase))
        {
            MouseInputHandler.Instance.OnMouseLeftDown += StartDrag;
            MouseInputHandler.Instance.OnMouseLeftUp += EndDrag;
            MouseInputHandler.Instance.OnMouseMove += HandleMouseMove;
        }
    }

    protected override void HandlePhaseExited(InGamePhase phase)
    {
        if (IsDragPhase(phase))
        {
            MouseInputHandler.Instance.OnMouseLeftDown -= StartDrag;
            MouseInputHandler.Instance.OnMouseLeftUp -= EndDrag;
            MouseInputHandler.Instance.OnMouseMove -= HandleMouseMove;
        }
    }

    private bool IsDragPhase(InGamePhase phase)
    {
        return phase == InGamePhase.BuildSelect ||
               phase == InGamePhase.DiceRoll ||
               phase == InGamePhase.TowerApply ||
               phase == InGamePhase.Combat;
    }

    private void StartDrag(Vector3 mousePos)
    {
        lastMouseWorldPos = targetCamera.ScreenToWorldPoint(mousePos);
        lastMouseWorldPos.z = targetCamera.transform.position.z;
        isDragging = true;
    }

    private void EndDrag(Vector3 mousePos)
    {
        isDragging = false;
    }

    private void HandleMouseMove(Vector3 mousePos)
    {
        if (!isDragging) return;

        Vector3 mouseWorld = targetCamera.ScreenToWorldPoint(mousePos);
        mouseWorld.z = targetCamera.transform.position.z;

        Vector3 delta = lastMouseWorldPos - mouseWorld;
        targetCamera.transform.position += delta * dragSpeed;

        lastMouseWorldPos = targetCamera.ScreenToWorldPoint(mousePos);
        lastMouseWorldPos.z = targetCamera.transform.position.z;
    }
}