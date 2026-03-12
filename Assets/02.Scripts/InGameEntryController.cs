using UnityEngine;

public class InGameEntryController : MonoBehaviour
{
    [SerializeField] private TutorialUIController tutorialUIController;

    private const string HasEnteredGameSceneKey = "HasEnteredGameScene";

    private void Start()
    {
        bool hasEnteredBefore = PlayerPrefs.GetInt(HasEnteredGameSceneKey, 0) == 1;

        if (hasEnteredBefore)
        {
            InGameFlowManager.Instance.EnterPhase(InGamePhase.BuildSelect);
            return;
        }

        tutorialUIController.ShowPrompt(
            onYes: HandleTutorialYes,
            onNo: HandleTutorialNo
        );
    }

#if UNITY_EDITOR
    private void Update()
    {
        // 디버깅용: 튜토리얼 시작 강제 테스트
        if (Input.GetKeyDown(KeyCode.T))
        {
            Debug.Log("튜토리얼 시작 테스트");

            InGameFlowManager.Instance.EnterPhase(InGamePhase.Tutorial);
            tutorialUIController.ShowTutorialPages(HandleTutorialFinished);
        }

        // 디버깅용: 최초 진입 기록 초기화
        if (Input.GetKeyDown(KeyCode.R))
        {
            Debug.Log("플레이어프렙 초기화");

            PlayerPrefs.DeleteKey(HasEnteredGameSceneKey);
            PlayerPrefs.Save();
        }
    }
#endif

    private void HandleTutorialYes()
    {
        MarkAsEntered();

        InGameFlowManager.Instance.EnterPhase(InGamePhase.Tutorial);
        tutorialUIController.ShowTutorialPages(HandleTutorialFinished);
    }

    private void HandleTutorialNo()
    {
        MarkAsEntered();
        InGameFlowManager.Instance.EnterPhase(InGamePhase.BuildSelect);
    }

    private void HandleTutorialFinished()
    {
        InGameFlowManager.Instance.EnterPhase(InGamePhase.BuildSelect);
    }

    private void MarkAsEntered()
    {
        PlayerPrefs.SetInt(HasEnteredGameSceneKey, 1);
        PlayerPrefs.Save();
    }
}