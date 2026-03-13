using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class MouseInputHandler : MonoBehaviour
{
    public static MouseInputHandler Instance;

    // 마우스 위치 / 클릭 / 스크롤 이벤트
    public event Action<Vector3> OnMouseMove;
    public event Action<Vector3> OnMouseLeftDown;
    public event Action<Vector3> OnMouseLeftUp;
    public event Action<Vector3> OnMouseClick; // ← 새로운 클릭 이벤트 추가
    public event Action<float> OnMouseScroll;

    // 드래그 상태 플래그
    public bool IsDragging { get; private set; } = false;

    [SerializeField] private float dragThreshold = 5f;

    private Vector3 dragStartPos;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Update()
    {
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }
        
        Vector3 mousePos = Input.mousePosition;

        // 마우스 이동
        OnMouseMove?.Invoke(mousePos);

        // 마우스 버튼 눌림
        if (Input.GetMouseButtonDown(0))
        {
            dragStartPos = mousePos;
            IsDragging = false; 
            OnMouseLeftDown?.Invoke(mousePos);
        }

        // 마우스 이동 중 드래그 판단
        if (Input.GetMouseButton(0))
        {
            if (!IsDragging && (mousePos - dragStartPos).magnitude > dragThreshold)
            {
                IsDragging = true;
            }
        }

        // 마우스 버튼 올라감 (순서 주의: 드래그 판단 로직 아래에 배치)
        if (Input.GetMouseButtonUp(0))
        {
            // 드래그 상태가 아니었다면 순수 클릭으로 판정
            if (!IsDragging)
            {
                OnMouseClick?.Invoke(mousePos);
            }

            IsDragging = false; // 이벤트 발생 후 드래그 상태 초기화
            OnMouseLeftUp?.Invoke(mousePos);
        }

        // 마우스 스크롤
        float scroll = Input.mouseScrollDelta.y;
        if (scroll != 0f)
            OnMouseScroll?.Invoke(scroll);
    }
}