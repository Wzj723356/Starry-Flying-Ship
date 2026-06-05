using UnityEngine;
using UnityEngine.UI;

public class GraphicsSettings : MonoBehaviour
{
    public static GraphicsSettings instance;

    [Header("分辨率选项")]
    public Dropdown resolutionDropdown;

    [Header("全屏模式")]
    public Toggle fullscreenToggle;

    [Header("VSync")]
    public Dropdown vsyncDropdown;

    [Header("质量预设")]
    public Dropdown qualityDropdown;

    private Resolution[] resolutions;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        InitializeResolutions();
        LoadSettings();
    }

    // ===== 分辨率系统 =====
    private void InitializeResolutions()
    {
        resolutions = Screen.resolutions;
        resolutionDropdown.ClearOptions();

        List<string> options = new List<string>();
        int currentResolutionIndex = 0;

        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = $"{resolutions[i].width} × {resolutions[i].height} @ {resolutions[i].refreshRate}Hz";
            options.Add(option);

            if (resolutions[i].width == Screen.currentResolution.width &&
                resolutions[i].height == Screen.currentResolution.height)
            {
                currentResolutionIndex = i;
            }
        }

        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();
    }

    public void SetResolution(int resolutionIndex)
    {
        Resolution resolution = resolutions[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
        PlayerPrefs.SetInt("ResolutionIndex", resolutionIndex);
        PlayerPrefs.Save();
    }

    // ===== 8K分辨率支持 =====
    public void Set8KResolution()
    {
        Screen.SetResolution(7680, 4320, Screen.fullScreen);
        Debug.Log("已设置为8K分辨率 (7680×4320)");
    }

    public void Set4KResolution()
    {
        Screen.SetResolution(3840, 2160, Screen.fullScreen);
        Debug.Log("已设置为4K分辨率 (3840×2160)");
    }

    public void Set1080pResolution()
    {
        Screen.SetResolution(1920, 1080, Screen.fullScreen);
        Debug.Log("已设置为1080p分辨率 (1920×1080)");
    }

    // ===== 全屏模式 =====
    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
        PlayerPrefs.SetInt("Fullscreen", isFullscreen ? 1 : 0);
        PlayerPrefs.Save();
    }

    // ===== VSync =====
    public void SetVSync(int vsyncLevel)
    {
        QualitySettings.vSyncCount = vsyncLevel;
        PlayerPrefs.SetInt("VSync", vsyncLevel);
        PlayerPrefs.Save();
    }

    // ===== 图形质量 =====
    public void SetQuality(int qualityIndex)
    {
        QualitySettings.SetQualityLevel(qualityIndex);
        PlayerPrefs.SetInt("Quality", qualityIndex);
        PlayerPrefs.Save();
    }

    // ===== 存档系统 =====
    private void LoadSettings()
    {
        if (PlayerPrefs.HasKey("ResolutionIndex"))
        {
            int index = PlayerPrefs.GetInt("ResolutionIndex");
            resolutionDropdown.value = index;
            SetResolution(index);
        }

        if (PlayerPrefs.HasKey("Fullscreen"))
        {
            bool fullscreen = PlayerPrefs.GetInt("Fullscreen") == 1;
            fullscreenToggle.isOn = fullscreen;
            SetFullscreen(fullscreen);
        }

        if (PlayerPrefs.HasKey("VSync"))
        {
            int vsync = PlayerPrefs.GetInt("VSync");
            vsyncDropdown.value = vsync;
            SetVSync(vsync);
        }

        if (PlayerPrefs.HasKey("Quality"))
        {
            int quality = PlayerPrefs.GetInt("Quality");
            qualityDropdown.value = quality;
            SetQuality(quality);
        }
    }

    // ===== 快速设置 =====
    public void ApplyUltraSettings()
    {
        SetQuality(5);
        SetVSync(1);
        Set8KResolution();
    }

    public void ApplyHighSettings()
    {
        SetQuality(3);
        SetVSync(1);
        Set4KResolution();
    }

    public void ApplyMediumSettings()
    {
        SetQuality(2);
        SetVSync(1);
        Set1080pResolution();
    }

    public void ApplyLowSettings()
    {
        SetQuality(0);
        SetVSync(0);
        Set1080pResolution();
    }
}
