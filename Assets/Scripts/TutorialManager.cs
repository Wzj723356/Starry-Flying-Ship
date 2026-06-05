using UnityEngine;
using UnityEngine.UI;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager instance;
    
    [Header("教程面板")]
    public GameObject tutorialPanel;
    public Text tutorialTitle;
    public Text tutorialDescription;
    public Image tutorialImage;
    public Button nextButton;
    public Button skipButton;
    
    [Header("教程步骤")]
    public TutorialStep[] tutorialSteps;
    
    private int currentStep = 0;
    
    void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }
    
    void Start()
    {
        if (PlayerPrefs.GetInt("FirstTime", 1) == 1)
        {
            ShowTutorial();
            PlayerPrefs.SetInt("FirstTime", 0);
            PlayerPrefs.Save();
        }
    }
    
    public void ShowTutorial()
    {
        tutorialPanel.SetActive(true);
        Time.timeScale = 0;
        LoadStep(0);
    }
    
    void LoadStep(int stepIndex)
    {
        if (stepIndex >= tutorialSteps.Length)
        {
            EndTutorial();
            return;
        }
        
        currentStep = stepIndex;
        TutorialStep step = tutorialSteps[stepIndex];
        
        tutorialTitle.text = step.title;
        tutorialDescription.text = step.description;
        
        if (step.image != null)
        {
            tutorialImage.enabled = true;
            tutorialImage.sprite = step.image;
        }
        else
        {
            tutorialImage.enabled = false;
        }
        
        nextButton.GetComponentInChildren<Text>().text = 
            stepIndex == tutorialSteps.Length - 1 ? "开始游戏" : "下一步";
    }
    
    public void NextStep()
    {
        LoadStep(currentStep + 1);
    }
    
    public void SkipTutorial()
    {
        EndTutorial();
    }
    
    void EndTutorial()
    {
        tutorialPanel.SetActive(false);
        Time.timeScale = 1;
    }
    
    public void ResetTutorial()
    {
        PlayerPrefs.SetInt("FirstTime", 1);
        PlayerPrefs.Save();
    }
}

[System.Serializable]
public class TutorialStep
{
    public string title;
    [TextArea]
    public string description;
    public Sprite image;
}
