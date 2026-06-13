using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class HUDManager : MonoBehaviour
{
    [Header("HUD Canvas")]
    public Canvas hudCanvas;
    
    [Header("瞄准系统")]
    public Image aimingReticle;      // 准星（屏幕中心固定）
    public Image aimingRing;        // 瞄准环（跟随鼠标）
    public Text speedText;          // 速度显示
    public Text altitudeText;        // 高度显示
    
    [Header("雷达系统")]
    public Image frontRadar;         // 前方雷达（100km）
    public Image sideRadar;          // 侧面雷达（60km）
    public Text frontRadarText;      // 前方雷达标签
    public Text sideRadarText;       // 侧面雷达标签
    
    [Header("飞船状态")]
    public Image shipModelDisplay;   // 飞船小模型
    public Image[] partStatus;       // 部件状态指示器
    public Text[] partStatusTexts;   // 部件状态文字
    public Slider hullHealthBar;     // 船体血条
    public Slider enginePowerBar;    // 引擎功率条
    
    [Header("警告系统")]
    public Image lockWarning;        // 锁定警告（红色圆点）
    public Image missileWarning;     // 导弹来袭警告
    public Text missileWarningText;  // 导弹警告文字
    public Image[] warningIndicators;// 警告指示器
    
    [Header("操作台状态")]
    public Text onlinePlayersText;   // 在线人数
    public Text shipStatusText;      // 飞船状态
    public Text battleStatusText;     // 战斗状态
    public Text weaponStatusText;     // 武器状态
    
    [Header("颜色配置")]
    public Color normalColor = Color.white;
    public Color warningColor = Color.yellow;
    public Color criticalColor = Color.red;
    public Color radarFriendlyColor = Color.cyan;
    public Color radarEnemyColor = Color.red;
    public Color radarMissileColor = Color.magenta;
    
    private List<Transform> enemiesOnRadar = new List<Transform>();
    private List<Transform> missiles = new List<Transform>();
    private bool isLocked = false;
    private float warningFlashSpeed = 5f;
    
    void Start()
    {
        InitializeHUD();
    }
    
    void Update()
    {
        UpdateFlightInfo();
        UpdateRadar();
        UpdateWarnings();
        UpdateShipStatus();
    }
    
    void InitializeHUD()
    {
        // 创建Canvas
        CreateCanvas();
        
        // 创建瞄准系统
        CreateAimingSystem();
        
        // 创建雷达系统
        CreateRadarSystem();
        
        // 创建飞船状态显示
        CreateShipStatusDisplay();
        
        // 创建警告系统
        CreateWarningSystem();
        
        // 创建操作台状态
        CreateOperationPanel();
    }
    
    void CreateCanvas()
    {
        GameObject canvasObj = new GameObject("HUDCanvas");
        canvasObj.transform.SetParent(transform);
        hudCanvas = canvasObj.AddComponent<Canvas>();
        hudCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        hudCanvas.sortingOrder = 100;
        
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        
        canvasObj.AddComponent<GraphicRaycaster>();
    }
    
    void CreateAimingSystem()
    {
        // 准星（屏幕中心）
        aimingReticle = CreateImage("AimingReticle");
        aimingReticle.rectTransform.anchoredPosition = Vector2.zero;
        aimingReticle.rectTransform.sizeDelta = new Vector2(40, 40);
        aimingReticle.color = Color.cyan;
        aimingReticle.sprite = CreateCircleSprite();
        
        // 瞄准环（跟随鼠标）
        aimingRing = CreateImage("AimingRing");
        aimingRing.rectTransform.sizeDelta = new Vector2(150, 150);
        aimingRing.color = new Color(0.3f, 0.8f, 1f, 0.8f);
        aimingRing.sprite = CreateRingSprite();
        
        // 速度文字
        speedText = CreateText("SpeedText");
        speedText.rectTransform.anchoredPosition = new Vector2(0, -50);
        speedText.fontSize = 24;
        speedText.color = Color.white;
        speedText.text = "SPD: 0 m/s";
        
        // 高度文字
        altitudeText = CreateText("AltitudeText");
        altitudeText.rectTransform.anchoredPosition = new Vector2(0, -80);
        altitudeText.fontSize = 20;
        altitudeText.color = new Color(0.7f, 0.9f, 1f);
        altitudeText.text = "ALT: 0 m";
    }
    
    void CreateRadarSystem()
    {
        // 前方雷达（右上角）- 100km
        GameObject frontRadarObj = new GameObject("FrontRadar");
        frontRadarObj.transform.SetParent(hudCanvas.transform);
        frontRadar = frontRadarObj.AddComponent<Image>();
        frontRadar.rectTransform.anchorMax = new Vector2(1, 1);
        frontRadar.rectTransform.anchorMin = new Vector2(1, 1);
        frontRadar.rectTransform.anchoredPosition = new Vector2(-120, -80);
        frontRadar.rectTransform.sizeDelta = new Vector2(120, 120);
        frontRadar.color = new Color(0.1f, 0.3f, 0.5f, 0.7f);
        frontRadar.sprite = CreateRadarSprite();
        
        frontRadarText = CreateText("FrontRadarText");
        frontRadarText.rectTransform.SetParent(frontRadarObj.transform);
        frontRadarText.rectTransform.anchoredPosition = new Vector2(0, -70);
        frontRadarText.fontSize = 14;
        frontRadarText.color = new Color(0.5f, 0.8f, 1f);
        frontRadarText.text = "前方 100km";
        
        // 侧面雷达（左上角）- 60km
        GameObject sideRadarObj = new GameObject("SideRadar");
        sideRadarObj.transform.SetParent(hudCanvas.transform);
        sideRadar = sideRadarObj.AddComponent<Image>();
        sideRadar.rectTransform.anchorMax = new Vector2(0, 1);
        sideRadar.rectTransform.anchorMin = new Vector2(0, 1);
        sideRadar.rectTransform.anchoredPosition = new Vector2(120, -80);
        sideRadar.rectTransform.sizeDelta = new Vector2(100, 100);
        sideRadar.color = new Color(0.1f, 0.3f, 0.5f, 0.7f);
        sideRadar.sprite = CreateRadarSprite();
        
        sideRadarText = CreateText("SideRadarText");
        sideRadarText.rectTransform.SetParent(sideRadarObj.transform);
        sideRadarText.rectTransform.anchoredPosition = new Vector2(0, -60);
        sideRadarText.fontSize = 12;
        sideRadarText.color = new Color(0.5f, 0.8f, 1f);
        sideRadarText.text = "侧面 60km";
    }
    
    void CreateShipStatusDisplay()
    {
        // 飞船小模型背景（左下角）
        GameObject modelBg = new GameObject("ShipModelDisplay");
        modelBg.transform.SetParent(hudCanvas.transform);
        Image modelBgImg = modelBg.AddComponent<Image>();
        modelBgImg.rectTransform.anchorMax = new Vector2(0, 0);
        modelBgImg.rectTransform.anchorMin = new Vector2(0, 0);
        modelBgImg.rectTransform.anchoredPosition = new Vector2(100, 80);
        modelBgImg.rectTransform.sizeDelta = new Vector2(150, 120);
        modelBgImg.color = new Color(0f, 0.2f, 0.3f, 0.5f);
        
        // 飞船小模型
        shipModelDisplay = CreateImage("ShipModel");
        shipModelDisplay.transform.SetParent(modelBg.transform);
        shipModelDisplay.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        shipModelDisplay.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        shipModelDisplay.rectTransform.anchoredPosition = Vector2.zero;
        shipModelDisplay.rectTransform.sizeDelta = new Vector2(80, 40);
        shipModelDisplay.color = new Color(0.3f, 0.7f, 1f, 0.9f);
        
        // 船体血条
        GameObject healthBarBg = new GameObject("HealthBarBg");
        healthBarBg.transform.SetParent(modelBg.transform);
        Image healthBg = healthBarBg.AddComponent<Image>();
        healthBg.rectTransform.anchoredPosition = new Vector2(0, -40);
        healthBg.rectTransform.sizeDelta = new Vector2(120, 15);
        healthBg.color = new Color(0.3f, 0.3f, 0.3f, 0.7f);
        
        hullHealthBar = CreateSlider("HealthBar");
        hullHealthBar.transform.SetParent(healthBarBg.transform);
        hullHealthBar.rectTransform.anchoredPosition = Vector2.zero;
        hullHealthBar.fillRect.GetComponent<Image>().color = Color.green;
        
        // 引擎功率条
        GameObject engineBarBg = new GameObject("EngineBarBg");
        engineBarBg.transform.SetParent(modelBg.transform);
        Image engineBg = engineBarBg.AddComponent<Image>();
        engineBg.rectTransform.anchoredPosition = new Vector2(0, -55);
        engineBg.rectTransform.sizeDelta = new Vector2(100, 10);
        engineBg.color = new Color(0.3f, 0.3f, 0.3f, 0.7f);
        
        enginePowerBar = CreateSlider("EngineBar");
        enginePowerBar.transform.SetParent(engineBarBg.transform);
        enginePowerBar.rectTransform.anchoredPosition = Vector2.zero;
        enginePowerBar.fillRect.GetComponent<Image>().color = Color.cyan;
    }
    
    void CreateWarningSystem()
    {
        // 锁定警告（红色圆点闪烁）
        lockWarning = CreateImage("LockWarning");
        lockWarning.rectTransform.anchorMax = new Vector2(0.5f, 1);
        lockWarning.rectTransform.anchorMin = new Vector2(0.5f, 1);
        lockWarning.rectTransform.anchoredPosition = new Vector2(0, -20);
        lockWarning.rectTransform.sizeDelta = new Vector2(30, 30);
        lockWarning.color = Color.red;
        lockWarning.enabled = false;
        
        // 导弹来袭警告
        missileWarning = CreateImage("MissileWarning");
        missileWarning.rectTransform.anchoredPosition = new Vector2(0, 100);
        missileWarning.rectTransform.sizeDelta = new Vector2(60, 60);
        missileWarning.color = new Color(1f, 0.3f, 0f, 0.8f);
        missileWarning.enabled = false;
        
        missileWarningText = CreateText("MissileWarningText");
        missileWarningText.rectTransform.anchoredPosition = new Vector2(0, 130);
        missileWarningText.fontSize = 18;
        missileWarningText.color = Color.red;
        missileWarningText.text = "导弹来袭！";
        missileWarningText.enabled = false;
    }
    
    void CreateOperationPanel()
    {
        // 操作台状态面板（右下角）
        GameObject panelBg = new GameObject("OperationPanel");
        panelBg.transform.SetParent(hudCanvas.transform);
        Image panelImg = panelBg.AddComponent<Image>();
        panelImg.rectTransform.anchorMax = new Vector2(1, 0);
        panelImg.rectTransform.anchorMin = new Vector2(1, 0);
        panelImg.rectTransform.anchoredPosition = new Vector2(-120, 100);
        panelImg.rectTransform.sizeDelta = new Vector2(200, 150);
        panelImg.color = new Color(0f, 0.15f, 0.25f, 0.7f);
        
        // 在线人数
        onlinePlayersText = CreateText("OnlinePlayers");
        onlinePlayersText.rectTransform.SetParent(panelBg.transform);
        onlinePlayersText.rectTransform.anchoredPosition = new Vector2(0, 55);
        onlinePlayersText.fontSize = 16;
        onlinePlayersText.color = Color.cyan;
        onlinePlayersText.text = "在线: 0人";
        
        // 飞船状态
        shipStatusText = CreateText("ShipStatus");
        shipStatusText.rectTransform.SetParent(panelBg.transform);
        shipStatusText.rectTransform.anchoredPosition = new Vector2(0, 25);
        shipStatusText.fontSize = 14;
        shipStatusText.color = Color.white;
        shipStatusText.text = "飞船: 正常";
        
        // 战斗状态
        battleStatusText = CreateText("BattleStatus");
        battleStatusText.rectTransform.SetParent(panelBg.transform);
        battleStatusText.rectTransform.anchoredPosition = new Vector2(0, -5);
        battleStatusText.fontSize = 14;
        battleStatusText.color = new Color(1f, 0.8f, 0f);
        battleStatusText.text = "战斗状态: 待机";
        
        // 武器状态
        weaponStatusText = CreateText("WeaponStatus");
        weaponStatusText.rectTransform.SetParent(panelBg.transform);
        weaponStatusText.rectTransform.anchoredPosition = new Vector2(0, -35);
        weaponStatusText.fontSize = 12;
        weaponStatusText.color = Color.green;
        weaponStatusText.text = "激光炮: 就绪";
    }
    
    void UpdateFlightInfo()
    {
        Rigidbody rb = GameObject.FindGameObjectWithTag("Player")?.GetComponent<Rigidbody>();
        if (rb != null)
        {
            float speed = rb.velocity.magnitude;
            float altitude = rb.position.y;
            
            speedText.text = $"SPD: {speed:F0} m/s";
            altitudeText.text = $"ALT: {altitude:F0} m";
            
            // 更新引擎功率条
            float throttle = speed / 400f; // 假设最大速度400
            enginePowerBar.value = throttle;
        }
    }
    
    void UpdateRadar()
    {
        // 更新雷达上的敌人标记
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        // 简化的雷达更新逻辑
    }
    
    void UpdateWarnings()
    {
        // 锁定警告闪烁
        if (isLocked)
        {
            float flash = Mathf.Sin(Time.time * warningFlashSpeed) * 0.5f + 0.5f;
            lockWarning.color = new Color(1f, 0f, 0f, flash);
            lockWarning.enabled = true;
        }
        else
        {
            lockWarning.enabled = false;
        }
        
        // 导弹来袭警告
        if (missiles.Count > 0)
        {
            missileWarning.enabled = true;
            missileWarningText.enabled = true;
            float flash = Mathf.Sin(Time.time * 10f) * 0.5f + 0.5f;
            missileWarning.color = new Color(1f, 0.3f, 0f, flash);
        }
        else
        {
            missileWarning.enabled = false;
            missileWarningText.enabled = false;
        }
    }
    
    void UpdateShipStatus()
    {
        Damageable damageable = GameObject.FindGameObjectWithTag("Player")?.GetComponent<Damageable>();
        if (damageable != null)
        {
            float healthPercent = damageable.currentHealth / damageable.maxHealth;
            hullHealthBar.value = healthPercent;
            
            // 根据血量改变颜色
            if (healthPercent > 0.6f)
                hullHealthBar.fillRect.GetComponent<Image>().color = Color.green;
            else if (healthPercent > 0.3f)
                hullHealthBar.fillRect.GetComponent<Image>().color = Color.yellow;
            else
                hullHealthBar.fillRect.GetComponent<Image>().color = Color.red;
        }
    }
    
    // 辅助方法
    Image CreateImage(string name)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(hudCanvas.transform);
        return obj.AddComponent<Image>();
    }
    
    Text CreateText(string name)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(hudCanvas.transform);
        Text text = obj.AddComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.alignment = TextAnchor.MiddleCenter;
        return text;
    }
    
    Slider CreateSlider(string name)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(hudCanvas.transform);
        return obj.AddComponent<Slider>();
    }
    
    Sprite CreateCircleSprite()
    {
        // 创建简单的圆形精灵
        Texture2D tex = new Texture2D(64, 64);
        Color[] colors = new Color[64 * 64];
        float center = 32;
        float radius = 30;
        
        for (int y = 0; y < 64; y++)
        {
            for (int x = 0; x < 64; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                if (dist < radius && dist > radius - 3)
                    colors[y * 64 + x] = Color.white;
                else
                    colors[y * 64 + x] = Color.clear;
            }
        }
        
        tex.SetPixels(colors);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 64, 64), new Vector2(0.5f, 0.5f), 64);
    }
    
    Sprite CreateRingSprite()
    {
        Texture2D tex = new Texture2D(128, 128);
        Color[] colors = new Color[128 * 128];
        float center = 64;
        float radius = 60;
        
        for (int y = 0; y < 128; y++)
        {
            for (int x = 0; x < 128; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                if (dist < radius && dist > radius - 2)
                    colors[y * 128 + x] = new Color(0.3f, 0.8f, 1f, 0.8f);
                else
                    colors[y * 128 + x] = Color.clear;
            }
        }
        
        tex.SetPixels(colors);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 128, 128), new Vector2(0.5f, 0.5f), 128);
    }
    
    Sprite CreateRadarSprite()
    {
        Texture2D tex = new Texture2D(128, 128);
        Color[] colors = new Color[128 * 128];
        float center = 64;
        float radius = 60;
        
        // 绘制圆形边框
        for (int y = 0; y < 128; y++)
        {
            for (int x = 0; x < 128; x++)
            {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                if (dist < radius && dist > radius - 2)
                    colors[y * 128 + x] = new Color(0.3f, 0.6f, 0.9f, 0.6f);
                else if (dist < radius * 0.5f && dist > radius * 0.5f - 1)
                    colors[y * 128 + x] = new Color(0.3f, 0.6f, 0.9f, 0.4f);
                else
                    colors[y * 128 + x] = Color.clear;
            }
        }
        
        tex.SetPixels(colors);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 128, 128), new Vector2(0.5f, 0.5f), 128);
    }
    
    // 公开方法供其他脚本调用
    public void SetLocked(bool locked)
    {
        isLocked = locked;
    }
    
    public void AddMissile(Transform missile)
    {
        if (!missiles.Contains(missile))
            missiles.Add(missile);
    }
    
    public void RemoveMissile(Transform missile)
    {
        missiles.Remove(missile);
    }
    
    public void SetOnlinePlayers(int count)
    {
        onlinePlayersText.text = $"在线: {count}人";
    }
    
    public void SetBattleStatus(string status)
    {
        battleStatusText.text = $"战斗状态: {status}";
    }
    
    public void SetWeaponStatus(string status, Color color)
    {
        weaponStatusText.text = status;
        weaponStatusText.color = color;
    }
}
