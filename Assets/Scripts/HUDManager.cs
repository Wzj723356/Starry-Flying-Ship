using UnityEngine;
using UnityEngine.UI;

public class HUDManager : MonoBehaviour
{
    [Header("=== 顶部数据区 ===")]
    public Text forceText;          // 力
    public Text velocityText;       // 速度
    public Text powerText;          // 功率
    
    [Header("=== 中间飞船模型区 ===")]
    public Image shipModelCenter;   // 中间飞船模型（第三人称，跟随实际飞船旋转）
    public Transform shipTransform; // 飞船旋转参照
    
    [Header("=== 左下飞船示意图 - 受损显示 ===")]
    public Image shipDiagram;       // 左下飞船示意图底板
    public RectTransform bridgeRT;     // 舰桥（要害！秒杀）
    public RectTransform leftEngineRT;  // 左发动机
    public RectTransform rightEngineRT; // 右发动机
    public RectTransform landingGearRT; // 起落架
    public RectTransform leftAileronRT; // 左副翼
    public RectTransform rightAileronRT;// 右副翼
    public RectTransform tailSectionRT; // 尾翼
    public RectTransform mainHullRT;    // 主船体
    
    [Header("=== 干扰弹 ===")]
    public Text chaffText;          // 干扰弹数量
    public Text flareText;          // 热诱弹数量
    public Image chaffIcon;         // 干扰弹图标
    public Image flareIcon;         // 热诱弹图标
    public int maxChaff = 30;       // 最大干扰弹
    public int maxFlare = 30;       // 最大热诱弹
    
    [Header("=== 右侧雷达区 ===")]
    public Image radarDisplay;      // 雷达显示
    public Text radarRangeText;     // 雷达范围
    public Transform radarCenter;   // 雷达中心点
    public float radarRange = 50000f; // 雷达范围(米)
    
    [Header("=== 底部状态栏 ===")]
    public Text connectionStatus;   // 联机状态
    public Text consoleStatus;      // 操作台状态
    public Text shipStatus;         // 飞船状态
    
    [Header("=== 左侧数据列 ===")]
    public Text speedText;          // 速度
    public Text altitudeText;       // 高度
    public Text targetText;         // 目标
    public Text thrustText;         // 推力
    public Text maintainTargetText; // 维持目标
    public Text navPointText;       // 星一侧点/导航点
    
    [Header("=== 科幻颜色 ===")]
    public Color normalColor = new Color(0f, 0.8f, 1f, 0.9f);     // 青色正常
    public Color warningColor = new Color(1f, 0.6f, 0f, 0.9f);    // 橙色警告
    public Color dangerColor = new Color(1f, 0.2f, 0.2f, 0.9f);   // 红色危险
    public Color engineColor = new Color(0f, 0.7f, 1f, 0.8f);     // 发动机蓝
    public Color damagedColor = new Color(1f, 0.3f, 0f, 0.9f);    // 受损橙
    public Color destroyedColor = new Color(0.3f, 0.3f, 0.3f, 0.5f); // 损毁灰
    public Color bgDark = new Color(0f, 0.05f, 0.1f, 0.6f);       // 深色背景
    public Color radarColor = new Color(0.5f, 0.8f, 1f, 0.7f);    // 雷达淡蓝色
    
    [Header("=== 被锁定警告 ===")]
    public Image lockWarningPanel;    // 锁定警告面板
    public Text lockWarningText;      // 锁定警告文字
    public Image[] missileIndicators; // 导弹来袭指示器
    public Text maneuverHintText;     // 39机动提示
    
    private bool isBeingLocked = false;
    private bool isMissileIncoming = false;
    private int incomingMissileCount = 0;
    private float lockWarningTimer = 0f;
    
    [Header("=== 组件引用 ===")]
    public StarShipFlightController shipController;
    public Damageable damageable;
    public WeaponSystem weaponSystem;
    public GameManager gameManager;
    
    private int currentChaff = 30;
    private int currentFlare = 30;
    
    void Awake()
    {
        FindComponents();
    }
    
    void Start()
    {
        InitializeHUD();
    }
    
    void Update()
    {
        UpdateTopData();
        UpdateShipModel();      // 中间飞船模型
        UpdateDamageDiagram();  // 左下受损示意图
        UpdateCountermeasures();// 干扰弹显示
        UpdateRadar();
        UpdateLockWarning();    // 被锁定警告
        UpdateBottomStatus();
        UpdateLeftData();
    }
    
    void FindComponents()
    {
        if (shipController == null)
            shipController = FindObjectOfType<StarShipFlightController>();
        if (damageable == null)
            damageable = FindObjectOfType<Damageable>();
        if (weaponSystem == null)
            weaponSystem = FindObjectOfType<WeaponSystem>();
        if (gameManager == null)
            gameManager = GameManager.instance;
    }
    
    void InitializeHUD()
    {
        // 初始化科幻风格颜色
        if (shipModelCenter != null) shipModelCenter.color = normalColor;
        if (shipDiagram != null) shipDiagram.color = normalColor;
        if (radarDisplay != null) radarDisplay.color = radarColor; // 雷达淡蓝色
        if (chaffIcon != null) chaffIcon.color = radarColor;
        if (flareIcon != null) flareIcon.color = warningColor;
        
        // 初始化底部状态
        if (connectionStatus != null) connectionStatus.text = "联机: 在线";
        if (consoleStatus != null) consoleStatus.text = "操作台: 正常";
        if (shipStatus != null) shipStatus.text = "飞船: 就绪";
        
        // 初始化干扰弹
        currentChaff = maxChaff;
        currentFlare = maxFlare;
        
        // 初始化锁定警告
        if (lockWarningPanel != null) lockWarningPanel.enabled = false;
        if (maneuverHintText != null) maneuverHintText.enabled = false;
    }
    
    // ===== 顶部数据区 =====
    void UpdateTopData()
    {
        if (shipController != null)
        {
            float speed = shipController.GetSpeed();
            float thrust = shipController.GetThrustPercentage();
            
            if (forceText != null)
                forceText.text = $"F: {CalculateForce():F1} kN";
            
            if (velocityText != null)
                velocityText.text = $"V: {speed:F1} m/s";
            
            if (powerText != null)
                powerText.text = $"P: {CalculatePower():F0} MW";
        }
    }
    
    float CalculateForce()
    {
        if (shipController != null)
        {
            return shipController.GetThrustPercentage() * 150f; // 150kN max
        }
        return 0f;
    }
    
    float CalculatePower()
    {
        if (shipController != null)
        {
            float thrust = shipController.GetThrustPercentage();
            return thrust * 500f; // 500MW max
        }
        return 0f;
    }
    
    // ===== 中间飞船模型区 - 跟随实际飞船旋转 =====
    void UpdateShipModel()
    {
        if (shipController == null) return;
        
        // 中间飞船模型随实际飞船姿态旋转
        if (shipTransform != null)
        {
            float rotX = shipController.GetPitch() * 15f;
            float rotY = shipController.GetYaw() * 15f;
            float rotZ = -shipController.GetRoll() * 15f;
            shipTransform.localRotation = Quaternion.Euler(rotX, rotY, rotZ);
        }
        
        // 模型颜色随机动状态变化
        if (shipModelCenter != null)
        {
            float thrust = shipController.GetThrustPercentage();
            if (shipController.IsAfterburnerActive())
                shipModelCenter.color = Color.Lerp(normalColor, warningColor, thrust);
            else
                shipModelCenter.color = normalColor;
        }
    }
    
    // ===== 左下飞船示意图 - 受损部件：该弯的弯，该断的断 =====
    // 颜色逻辑：青色正常 → 橙色警告 → 红色危险 → 灰色损毁
    // 自动修复：非灰色部件可自动修复，灰色只能降落维修
    void UpdateDamageDiagram()
    {
        if (damageable == null) return;
        
        Image img;
        
        // ===== 舰桥（要害部位，被击中秒杀）=====
        var bridgePart = damageable.GetPart(Damageable.ShipPart.PartType.Bridge);
        if (bridgeRT != null)
        {
            img = bridgeRT.GetComponent<Image>();
            if (bridgePart != null)
            {
                Color col = bridgePart.GetHealthColor(normalColor.rgb, warningColor.rgb, dangerColor.rgb, destroyedColor.rgb);
                if (img != null) img.color = col;
                
                // 舰桥危急闪烁警告
                if (!bridgePart.isDestroyed && bridgePart.currentPartHealth / bridgePart.maxPartHealth < 0.3f)
                {
                    float flash = Mathf.PingPong(Time.time * 5f, 1f);
                    if (img != null) img.color = Color.Lerp(dangerColor, Color.white, flash);
                }
            }
        }
        
        // ===== 舰桥损毁 =====
        if (bridgePart != null && bridgePart.isDestroyed)
        {
            // 舰桥被击中！触发秒杀效果（由Damageable处理）
        }
        
        // ===== 左发动机 =====
        var leftEnginePart = damageable.GetPart(Damageable.ShipPart.PartType.LeftEngine);
        if (leftEngineRT != null)
        {
            img = leftEngineRT.GetComponent<Image>();
            if (leftEnginePart != null)
            {
                Color col = leftEnginePart.GetHealthColor(normalColor.rgb, warningColor.rgb, dangerColor.rgb, destroyedColor.rgb);
                
                if (leftEnginePart.isDestroyed)
                {
                    // 损毁：灰显，断裂消失
                    leftEngineRT.localScale = Vector3.zero;
                    if (img != null) img.color = destroyedColor;
                }
                else
                {
                    // 非损毁：弯曲变形 + 颜色
                    leftEngineRT.localScale = Vector3.one;
                    
                    float ratio = leftEnginePart.currentPartHealth / leftEnginePart.maxPartHealth;
                    float bendAngle = Mathf.Lerp(30f, 0f, ratio);
                    leftEngineRT.localRotation = Quaternion.Euler(0, 0, bendAngle);
                    
                    if (img != null) img.color = col;
                }
            }
        }
        
        // ===== 右发动机 =====
        var rightEnginePart = damageable.GetPart(Damageable.ShipPart.PartType.RightEngine);
        if (rightEngineRT != null)
        {
            img = rightEngineRT.GetComponent<Image>();
            if (rightEnginePart != null)
            {
                Color col = rightEnginePart.GetHealthColor(normalColor.rgb, warningColor.rgb, dangerColor.rgb, destroyedColor.rgb);
                
                if (rightEnginePart.isDestroyed)
                {
                    rightEngineRT.localScale = Vector3.zero;
                    if (img != null) img.color = destroyedColor;
                }
                else
                {
                    rightEngineRT.localScale = Vector3.one;
                    
                    float ratio = rightEnginePart.currentPartHealth / rightEnginePart.maxPartHealth;
                    float bendAngle = Mathf.Lerp(-30f, 0f, ratio);
                    rightEngineRT.localRotation = Quaternion.Euler(0, 0, bendAngle);
                    
                    if (img != null) img.color = col;
                }
            }
        }
        
        // ===== 起落架 =====
        var landingGearPart = damageable.GetPart(Damageable.ShipPart.PartType.LandingGear);
        if (landingGearRT != null)
        {
            img = landingGearRT.GetComponent<Image>();
            if (landingGearPart != null)
            {
                Color col = landingGearPart.GetHealthColor(normalColor.rgb, warningColor.rgb, dangerColor.rgb, destroyedColor.rgb);
                
                if (landingGearPart.isDestroyed)
                {
                    // 起落架损毁：只能硬着陆
                    landingGearRT.localScale = Vector3.one * 0.5f;
                    if (img != null) img.color = destroyedColor;
                }
                else
                {
                    landingGearRT.localScale = Vector3.one;
                    if (img != null) img.color = col;
                }
            }
        }
        
        // ===== 主船体 =====
        var hullPart = damageable.GetPart(Damageable.ShipPart.PartType.Hull);
        if (mainHullRT != null)
        {
            img = mainHullRT.GetComponent<Image>();
            if (hullPart != null)
            {
                Color col = hullPart.GetHealthColor(normalColor.rgb, warningColor.rgb, dangerColor.rgb, destroyedColor.rgb);
                
                if (hullPart.isDestroyed)
                {
                    mainHullRT.localScale = Vector3.one * 0.3f;
                    if (img != null) img.color = destroyedColor;
                }
                else
                {
                    float ratio = hullPart.currentPartHealth / hullPart.maxPartHealth;
                    mainHullRT.localScale = Vector3.Lerp(new Vector3(0.7f, 0.8f, 1f), Vector3.one, ratio);
                    
                    // 危急闪烁
                    if (!hullPart.isDestroyed && ratio < 0.3f)
                    {
                        float flash = Mathf.PingPong(Time.time * 4f, 1f);
                        col = Color.Lerp(dangerColor, destroyedColor, flash);
                    }
                    
                    if (img != null) img.color = col;
                }
            }
        }
        
        // ===== 副翼 =====
        var leftAileronPart = damageable.GetPart(Damageable.ShipPart.PartType.LeftAileron);
        if (leftAileronRT != null)
        {
            img = leftAileronRT.GetComponent<Image>();
            if (leftAileronPart != null)
            {
                Color col = leftAileronPart.GetHealthColor(normalColor.rgb, warningColor.rgb, dangerColor.rgb, destroyedColor.rgb);
                
                if (leftAileronPart.isDestroyed)
                {
                    leftAileronRT.localScale = new Vector3(0.2f, 0.3f, 1f);
                    if (img != null) img.color = destroyedColor;
                }
                else
                {
                    leftAileronRT.localScale = Vector3.one;
                    
                    float ratio = leftAileronPart.currentPartHealth / leftAileronPart.maxPartHealth;
                    float bendAngle = Mathf.Lerp(40f, 0f, ratio);
                    leftAileronRT.localRotation = Quaternion.Euler(0, 0, bendAngle);
                    
                    if (img != null) img.color = col;
                }
            }
        }
        
        var rightAileronPart = damageable.GetPart(Damageable.ShipPart.PartType.RightAileron);
        if (rightAileronRT != null)
        {
            img = rightAileronRT.GetComponent<Image>();
            if (rightAileronPart != null)
            {
                Color col = rightAileronPart.GetHealthColor(normalColor.rgb, warningColor.rgb, dangerColor.rgb, destroyedColor.rgb);
                
                if (rightAileronPart.isDestroyed)
                {
                    rightAileronRT.localScale = new Vector3(0.2f, 0.3f, 1f);
                    if (img != null) img.color = destroyedColor;
                }
                else
                {
                    rightAileronRT.localScale = Vector3.one;
                    
                    float ratio = rightAileronPart.currentPartHealth / rightAileronPart.maxPartHealth;
                    float bendAngle = Mathf.Lerp(-40f, 0f, ratio);
                    rightAileronRT.localRotation = Quaternion.Euler(0, 0, bendAngle);
                    
                    if (img != null) img.color = col;
                }
            }
        }
        
        // ===== 尾翼 =====
        var tailPart = damageable.GetPart(Damageable.ShipPart.PartType.TailSection);
        if (tailSectionRT != null)
        {
            img = tailSectionRT.GetComponent<Image>();
            if (tailPart != null)
            {
                Color col = tailPart.GetHealthColor(normalColor.rgb, warningColor.rgb, dangerColor.rgb, destroyedColor.rgb);
                
                if (tailPart.isDestroyed)
                {
                    tailSectionRT.localScale = new Vector3(0.4f, 0.3f, 1f);
                    if (img != null) img.color = destroyedColor;
                }
                else
                {
                    tailSectionRT.localScale = Vector3.one;
                    
                    float ratio = tailPart.currentPartHealth / tailPart.maxPartHealth;
                    float bendAngle = Mathf.Lerp(25f, 0f, ratio);
                    tailSectionRT.localRotation = Quaternion.Euler(0, 0, bendAngle);
                    
                    if (img != null) img.color = col;
                }
            }
        }
    }
    
    // ===== 干扰弹显示 =====
    void UpdateCountermeasures()
    {
        if (chaffText != null)
            chaffText.text = $"{currentChaff}";
        
        if (flareText != null)
            flareText.text = $"{currentFlare}";
        
        // 数量低时变红警告
        if (chaffIcon != null)
            chaffIcon.color = currentChaff < 5 ? dangerColor : normalColor;
        
        if (flareIcon != null)
            flareIcon.color = currentFlare < 5 ? dangerColor : warningColor;
    }
    
    // ===== 右侧雷达区 =====
    void UpdateRadar()
    {
        if (radarDisplay == null) return;
        
        // 雷达范围显示
        if (radarRangeText != null)
        {
            float rangeKm = radarRange / 1000f;
            radarRangeText.text = $"{rangeKm:F0}km";
        }
        
        // 雷达脉冲动画
        float radarPulse = Mathf.PingPong(Time.time * 0.5f, 1f);
        radarDisplay.color = Color.Lerp(normalColor * 0.5f, normalColor, radarPulse);
    }
    
    // ===== 底部状态栏 =====
    void UpdateBottomStatus()
    {
        if (shipController == null) return;
        
        // 联机状态
        if (connectionStatus != null)
        {
            bool online = NetworkManager.instance != null && NetworkManager.instance.IsConnected();
            connectionStatus.text = online ? "联机: 在线" : "联机: 离线";
            connectionStatus.color = online ? normalColor : warningColor;
        }
        
        // 操作台状态
        if (consoleStatus != null)
        {
            float thrust = shipController.GetThrustPercentage();
            if (thrust > 0.9f)
            {
                consoleStatus.text = "操作台: 超载";
                consoleStatus.color = warningColor;
            }
            else
            {
                consoleStatus.text = "操作台: 正常";
                consoleStatus.color = normalColor;
            }
        }
        
        // 飞船状态
        if (shipStatus != null && damageable != null)
        {
            float health = damageable.HealthPercentage;
            if (health > 0.7f)
            {
                shipStatus.text = "飞船: 良好";
                shipStatus.color = normalColor;
            }
            else if (health > 0.3f)
            {
                shipStatus.text = "飞船: 受损";
                shipStatus.color = warningColor;
            }
            else
            {
                shipStatus.text = "飞船: 危急";
                shipStatus.color = dangerColor;
            }
        }
    }
    
    // ===== 左侧数据列 =====
    void UpdateLeftData()
    {
        if (shipController == null) return;
        
        float speed = shipController.GetSpeed();
        float altitude = shipController.GetAltitude();
        float thrust = shipController.GetThrustPercentage();
        
        if (speedText != null)
            speedText.text = $"{speed:F0} m/s";
        
        if (altitudeText != null)
            altitudeText.text = $"{altitude:F0} m";
        
        if (targetText != null)
            targetText.text = "目标: 无"; // 待实现目标锁定
        
        if (thrustText != null)
            thrustText.text = $"{thrust * 100:F0}%";
        
        if (maintainTargetText != null)
            maintainTargetText.text = shipController.IsAfterburnerActive() ? "维持: 加速器" : "维持: 巡航";
        
        if (navPointText != null)
            navPointText.text = "导航: 星一侧点"; // 默认导航点
    }
    
    // ===== 公共方法 =====
    public void SetTarget(string targetName)
    {
        if (targetText != null)
            targetText.text = $"目标: {targetName}";
    }
    
    public void SetNavPoint(string navName)
    {
        if (navPointText != null)
            navPointText.text = $"导航: {navName}";
    }
    
    public void SetConnectionStatus(bool online)
    {
        if (connectionStatus != null)
        {
            connectionStatus.text = online ? "联机: 在线" : "联机: 离线";
            connectionStatus.color = online ? normalColor : warningColor;
        }
    }
    
    public void SetShipStatus(string status, Color color)
    {
        if (shipStatus != null)
        {
            shipStatus.text = $"飞船: {status}";
            shipStatus.color = color;
        }
    }
    
    // ===== 干扰弹操作 =====
    public bool LaunchChaff()
    {
        if (currentChaff > 0)
        {
            currentChaff--;
            return true;
        }
        return false;
    }
    
    public bool LaunchFlare()
    {
        if (currentFlare > 0)
        {
            currentFlare--;
            return true;
        }
        return false;
    }
    
    public void ReloadCountermeasures(int chaffAmount, int flareAmount)
    {
        currentChaff = Mathf.Min(currentChaff + chaffAmount, maxChaff);
        currentFlare = Mathf.Min(currentFlare + flareAmount, maxFlare);
    }
    
    public int GetChaffCount() => currentChaff;
    public int GetFlareCount() => currentFlare;
    
    // ===== 被锁定警告系统 =====
    // 被敌方火控雷达或导引头锁定 = 随时可能被攻击，需要39机动
    void UpdateLockWarning()
    {
        // 检测是否有敌方正在锁定或导弹来袭
        bool currentlyLocked = IsBeingLockedByEnemy();
        bool missileIncoming = IsMissileIncoming();
        
        // 更新锁定状态
        if (currentlyLocked || missileIncoming)
        {
            isBeingLocked = true;
            lockWarningTimer += Time.deltaTime;
            
            // 显示警告面板
            if (lockWarningPanel != null)
            {
                lockWarningPanel.enabled = true;
                
                // 红色闪烁警告
                float flash = Mathf.PingPong(Time.time * 4f, 1f);
                lockWarningPanel.color = Color.Lerp(dangerColor, warningColor, flash);
            }
            
            // 显示锁定警告文字
            if (lockWarningText != null)
            {
                if (missileIncoming)
                {
                    lockWarningText.text = $"⚠️ 导弹来袭！[{incomingMissileCount}]";
                    lockWarningText.color = dangerColor;
                }
                else
                {
                    lockWarningText.text = "⚠️ 被火控锁定！";
                    lockWarningText.color = warningColor;
                }
            }
            
            // 显示39机动提示
            if (maneuverHintText != null)
            {
                maneuverHintText.enabled = true;
                maneuverHintText.text = "[ 执行39机动 ]";
                maneuverHintText.color = Color.Lerp(dangerColor, normalColor, Mathf.Sin(Time.time * 6f) * 0.5f + 0.5f);
            }
            
            // 更新导弹来袭指示器
            UpdateMissileIndicators();
        }
        else
        {
            // 解除警告
            isBeingLocked = false;
            lockWarningTimer = 0f;
            
            if (lockWarningPanel != null)
                lockWarningPanel.enabled = false;
            
            if (lockWarningText != null)
                lockWarningText.text = "";
            
            if (maneuverHintText != null)
                maneuverHintText.enabled = false;
            
            // 隐藏导弹指示器
            if (missileIndicators != null)
            {
                foreach (var indicator in missileIndicators)
                {
                    if (indicator != null)
                        indicator.enabled = false;
                }
            }
        }
    }
    
    // 检测是否被敌方火控雷达锁定
    bool IsBeingLockedByEnemy()
    {
        // 检查所有敌人AI
        var enemies = Object.FindObjectsOfType<EnemyAIController>();
        foreach (var enemy in enemies)
        {
            if (enemy == null) continue;
            
            float distance = Vector3.Distance(transform.position, enemy.transform.position);
            if (distance > 3000f) continue; // 超出锁定范围
            
            // 检查敌人是否正在锁定玩家
            if (enemy.currentState == EnemyAIController.AIState.Combat)
            {
                Vector3 toPlayer = (transform.position - enemy.transform.position).normalized;
                float angle = Vector3.Angle(enemy.transform.forward, toPlayer);
                
                if (angle < 15f) // 敌方正对着玩家
                {
                    return true;
                }
            }
        }
        
        return false;
    }
    
    // 检测是否有导弹来袭
    bool IsMissileIncoming()
    {
        // 检查所有敌方导弹
        var missiles = Object.FindObjectsOfType<EnemyMissile>();
        incomingMissileCount = 0;
        
        foreach (var missile in missiles)
        {
            if (missile == null || missile.launcher == null) continue;
            
            // 检测导弹是否朝向玩家
            Vector3 toPlayer = (transform.position - missile.transform.position).normalized;
            float angle = Vector3.Angle(missile.transform.forward, toPlayer);
            
            float distance = Vector3.Distance(transform.position, missile.transform.position);
            
            if (angle < 30f && distance < 2000f) // 导弹朝向玩家且在范围内
            {
                incomingMissileCount++;
            }
        }
        
        return incomingMissileCount > 0;
    }
    
    // 更新导弹来袭指示器
    void UpdateMissileIndicators()
    {
        if (missileIndicators == null || missileIndicators.Length == 0) return;
        
        var missiles = Object.FindObjectsOfType<EnemyMissile>();
        int indicatorIndex = 0;
        
        foreach (var missile in missiles)
        {
            if (missile == null || missile.launcher == null) continue;
            if (indicatorIndex >= missileIndicators.Length) break;
            
            Vector3 toPlayer = (transform.position - missile.transform.position).normalized;
            float angle = Vector3.Angle(missile.transform.forward, toPlayer);
            float distance = Vector3.Distance(transform.position, missile.transform.position);
            
            if (angle < 45f && distance < 3000f)
            {
                if (missileIndicators[indicatorIndex] != null)
                {
                    missileIndicators[indicatorIndex].enabled = true;
                    
                    // 根据距离显示不同颜色
                    if (distance < 500f)
                        missileIndicators[indicatorIndex].color = dangerColor;
                    else if (distance < 1000f)
                        missileIndicators[indicatorIndex].color = warningColor;
                    else
                        missileIndicators[indicatorIndex].color = normalColor;
                }
                indicatorIndex++;
            }
        }
        
        // 隐藏未使用的指示器
        for (int i = indicatorIndex; i < missileIndicators.Length; i++)
        {
            if (missileIndicators[i] != null)
                missileIndicators[i].enabled = false;
        }
    }
    
    // ===== 公共锁定警告方法 =====
    public bool IsCurrentlyLocked() => isBeingLocked;
    public bool IsMissileApproaching() => isMissileIncoming;
    public int GetIncomingMissileCount() => incomingMissileCount;
    public float GetLockWarningTime() => lockWarningTimer;
    
    // 被锁定时自动建议干扰弹使用
    public void OnLockedWarning()
    {
        // 可以在这里添加自动弹出干扰弹提示的逻辑
        Debug.Log("警告：被锁定！建议发射干扰弹或执行规避机动！");
    }
}
