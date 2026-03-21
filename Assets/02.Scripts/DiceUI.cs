using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System; // Action 사용을 위해 추가

public class DiceUI : MonoBehaviour
{
    public int currentDrawnValue { get; private set; }
    private int remainingRerolls;

    [Header("UI References")]
    public TextMeshProUGUI valueText;
    public Button rerollButton;

    public Action<DiceUI> OnRollCompleted; 

    public void Setup(int maxRerolls)
    {
        remainingRerolls = maxRerolls;
        rerollButton.onClick.RemoveAllListeners();
        rerollButton.onClick.AddListener(RequestReroll);
        Roll(() => {
            OnRollCompleted?.Invoke(this); 
        });
    }

    public void Roll(Action onComplete = null)
    {
        StartCoroutine(RollRoutine(onComplete));
    }

    private IEnumerator RollRoutine(Action onComplete)
    {
        // 1. 애니메이션 시작 전: 조작 방지
        rerollButton.interactable = false;
        // TODO: 나중에 여기에 주사위 굴러가는 연출
        
        yield return null;

        // 2. 값 결정 및 UI 갱신
        currentDrawnValue = UnityEngine.Random.Range(1, 7);
        valueText.text = currentDrawnValue.ToString();

        // 3. 리롤 횟수에 따라 버튼 다시 활성화
        rerollButton.interactable = (remainingRerolls > 0);

        // 4. 콜백 실행
        onComplete?.Invoke();
    }

    private void RequestReroll()
    {
        if (remainingRerolls > 0)
        {
            remainingRerolls--;
            Roll(() => {
                OnRollCompleted?.Invoke(this); 
            });
        }
    }
}