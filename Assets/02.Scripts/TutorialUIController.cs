using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[Serializable]
public class TutorialPageData
{
    public string header;
    public Sprite image;

    [TextArea(2, 5)]
    public string description;
}

public class TutorialUIController : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject promptPanel;
    [SerializeField] private GameObject tutorialPanel;

    [Header("Tutorial Contents")]
    [SerializeField] private Image tutorialImage;
    [SerializeField] private TutorialPageData[] tutorialPages;

    [Header("Buttons")]
    [SerializeField] private Button prevButton;
    [SerializeField] private Button nextButton;
    [SerializeField] private Button closeButton;

    [Header("Texts")]
    [SerializeField] private TMP_Text pageIndicatorText;
    [SerializeField] private TMP_Text nextButtonText;

    [Header("Button Labels")]
    [SerializeField] private string nextLabel = "다음";
    [SerializeField] private string startLabel = "시작";

    [Header("Options")]
    [SerializeField] private bool allowEscSkip = true;

    private Action onClickYes;
    private Action onClickNo;
    private Action onTutorialFinished;

    private int currentPageIndex;
    private bool isTutorialOpen;

    private void Awake()
    {
        HideAll();

        if (prevButton != null)
            prevButton.onClick.AddListener(OnClickPrevButton);

        if (nextButton != null)
            nextButton.onClick.AddListener(OnClickNextButton);

        if (closeButton != null)
            closeButton.onClick.AddListener(OnClickCloseButton);
    }

    private void Update()
    {
        if (!isTutorialOpen)
            return;

        if (!allowEscSkip)
            return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CloseTutorial();
        }
    }

    public void ShowPrompt(Action onYes, Action onNo)
    {
        onClickYes = onYes;
        onClickNo = onNo;

        isTutorialOpen = false;
        HideAll();

        if (promptPanel != null)
            promptPanel.SetActive(true);
    }

    public void ShowTutorialPages(Action onFinished)
    {
        onTutorialFinished = onFinished;
        currentPageIndex = 0;
        isTutorialOpen = true;

        HideAll();

        if (tutorialPanel != null)
            tutorialPanel.SetActive(true);

        RefreshTutorialPage();
    }

    public void HideAll()
    {
        if (promptPanel != null)
            promptPanel.SetActive(false);

        if (tutorialPanel != null)
            tutorialPanel.SetActive(false);
    }

    public void OnClickYesButton()
    {
        if (promptPanel != null)
            promptPanel.SetActive(false);

        onClickYes?.Invoke();
    }

    public void OnClickNoButton()
    {
        HideAll();
        onClickNo?.Invoke();
    }

    private void OnClickPrevButton()
    {
        if (tutorialPages == null || tutorialPages.Length == 0)
            return;

        currentPageIndex--;

        if (currentPageIndex < 0)
            currentPageIndex = 0;

        RefreshTutorialPage();
    }

    private void OnClickNextButton()
    {
        if (tutorialPages == null || tutorialPages.Length == 0)
        {
            CloseTutorial();
            return;
        }

        currentPageIndex++;

        if (currentPageIndex >= tutorialPages.Length)
        {
            CloseTutorial();
            return;
        }

        RefreshTutorialPage();
    }

    private void OnClickCloseButton()
    {
        CloseTutorial();
    }

    private void RefreshTutorialPage()
    {
        if (tutorialPages == null || tutorialPages.Length == 0)
        {
            UpdatePageIndicator();
            UpdateNextButtonText();
            return;
        }

        if (tutorialImage != null)
            tutorialImage.sprite = tutorialPages[currentPageIndex].image;

        if (prevButton != null)
            prevButton.interactable = currentPageIndex > 0;

        UpdatePageIndicator();
        UpdateNextButtonText();
    }

    private void UpdatePageIndicator()
    {
        if (pageIndicatorText == null)
            return;

        if (tutorialPages == null || tutorialPages.Length == 0)
        {
            pageIndicatorText.text = "0 / 0";
            return;
        }

        pageIndicatorText.text = $"{currentPageIndex + 1} / {tutorialPages.Length}";
    }

    private void UpdateNextButtonText()
    {
        if (nextButtonText == null)
            return;

        if (tutorialPages == null || tutorialPages.Length == 0)
        {
            nextButtonText.text = startLabel;
            return;
        }

        bool isLastPage = currentPageIndex == tutorialPages.Length - 1;
        nextButtonText.text = isLastPage ? startLabel : nextLabel;
    }

    private void CloseTutorial()
    {
        isTutorialOpen = false;
        HideAll();
        onTutorialFinished?.Invoke();
    }
}