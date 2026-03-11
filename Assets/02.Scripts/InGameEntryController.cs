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