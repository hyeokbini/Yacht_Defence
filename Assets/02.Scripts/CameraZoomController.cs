using UnityEngine;

public class CameraZoomController : PhaseListener
{
    private Camera targetCamera;

    [SerializeField] private float zoomSpeed = 2f;
    [SerializeField] private float minZoom = 4f;
    [SerializeField] private float maxZoom = 12f;

    private void Awake()
    {
        if (targetCamera == null)
            targetCamera = Camera.main;
    }

    protected override void HandlePhaseEntered(InGamePhase phase)
    {
        Debug.Log("PhaseEntered: " + phase);

        if (IsZoomPhase(phase))
        {
            Debug.Log("Zoom 구독");
            MouseInputHandler.Instance.OnMouseScroll += HandleZoom;
        }
    }

    protected override void HandlePhaseExited(InGamePhase phase)
    {
        if (IsZoomPhase(phase))
            MouseInputHandler.Instance.OnMouseScroll -= HandleZoom;
    }

    private bool IsZoomPhase(InGamePhase phase)
    {
        return phase == InGamePhase.BuildSelect ||
               phase == InGamePhase.DiceRoll ||
               phase == InGamePhase.TowerApply ||
               phase == InGamePhase.Combat;
    }

    private void HandleZoom(float scroll)
    {
        if (scroll == 0f)
            return;
        Debug.Log("여기도 인식됨");
        Vector3 mouseScreen = Input.mousePosition;

        Vector3 worldBefore = targetCamera.ScreenToWorldPoint(mouseScreen);

        float size = targetCamera.orthographicSize;
        size -= scroll * zoomSpeed;
        size = Mathf.Clamp(size, minZoom, maxZoom);

        targetCamera.orthographicSize = size;

        Vector3 worldAfter = targetCamera.ScreenToWorldPoint(mouseScreen);

        targetCamera.transform.position += worldBefore - worldAfter;
    }
}