using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    [Header("菜单面板")]
    public GameObject mainMenu;
    public GameObject settingsMenu;
    public GameObject controlsMenu;
    public GameObject creditsMenu;
    public GameObject loadingScreen;
    
    [Header("加载设置")]
    public Slider loadingSlider;
    public Text loadingText;
    
    [Header("设置选项")]
    public Toggle fullscreenToggle;
    public Dropdown resolutionDropdown;
    public Dropdown qualityDropdown;
    public Slider volumeSlider;
    
    private Resolution[] resolutions;
    
    void Awake()
    {
        resolutions = Screen.resolutions;
        resolutionDropdown.ClearOptions();
        
        foreach (Resolution res in resolutions)
        {
            resolutionDropdown.options.Add(new Dropdown.OptionData(res.ToString()));
        }
        
        LoadSettings();
    }
    
    void Start()
    {
        ShowMainMenu();
    }
    
    public void ShowMainMenu()
    {
        HideAllMenus();
        mainMenu.SetActive(true);
    }
    
    public void ShowSettings()
    {
        HideAllMenus();
        settingsMenu.SetActive(true);
    }
    
    public void ShowControls()
    {
        HideAllMenus();
        controlsMenu.SetActive(true);
    }
    
    public void ShowCredits()
    {
        HideAllMenus();
        creditsMenu.SetActive(true);
    }
    
    void HideAllMenus()
    {
        mainMenu.SetActive(false);
        settingsMenu.SetActive(false);
        controlsMenu.SetActive(false);
        creditsMenu.SetActive(false);
        loadingScreen.SetActive(false);
    }
    
    public void StartGame()
    {
        HideAllMenus();
        loadingScreen.SetActive(true);
        loadingSlider.value = 0;
        
        StartCoroutine(LoadGameAsync());
    }
    
    System.Collections.IEnumerator LoadGameAsync()
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync("StarShipTestScene");
        
        while (!operation.isDone)
        {
            float progress = Mathf.Clamp01(operation.progress / 0.9f);
            loadingSlider.value = progress;
            loadingText.text = $"加载中... {Mathf.Round(progress * 100)}%";
            yield return null;
        }
    }
    
    public void SaveSettings()
    {
        Screen.fullScreen = fullscreenToggle.isOn;
        
        Resolution selectedResolution = resolutions[resolutionDropdown.value];
        Screen.SetResolution(selectedResolution.width, selectedResolution.height, fullscreenToggle.isOn);
        
        QualitySettings.SetQualityLevel(qualityDropdown.value);
        
        AudioListener.volume = volumeSlider.value;
        
        PlayerPrefs.SetInt("Fullscreen", fullscreenToggle.isOn ? 1 : 0);
        PlayerPrefs.SetInt("Resolution", resolutionDropdown.value);
        PlayerPrefs.SetInt("Quality", qualityDropdown.value);
        PlayerPrefs.SetFloat("Volume", volumeSlider.value);
        PlayerPrefs.Save();
    }
    
    void LoadSettings()
    {
        fullscreenToggle.isOn = PlayerPrefs.GetInt("Fullscreen", 1) == 1;
        resolutionDropdown.value = PlayerPrefs.GetInt("Resolution", resolutions.Length - 1);
        qualityDropdown.value = PlayerPrefs.GetInt("Quality", QualitySettings.GetQualityLevel());
        volumeSlider.value = PlayerPrefs.GetFloat("Volume", 1f);
        
        SaveSettings();
    }
    
    public void QuitGame()
    {
        Application.Quit();
    }
}
