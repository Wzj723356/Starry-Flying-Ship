using UnityEngine;
using System.Collections.Generic;

public class HUDManager : MonoBehaviour
{
    public static HUDManager instance;
    
    [Header("瞄准系统")]
    public bool showAimReticle = true;
    public Color reticleColor = Color.cyan;
    
    [Header("雷达系统")]
    public bool showRadar = true;
    public float frontRadarRange = 100000f; // 100km
    public float sideRadarRange = 60000f;   // 60km
    public float rearMissileRange = 40000f; // 40km
    
    [Header("警告系统")]
    public bool showWarnings = true;
    public float warningFlashInterval = 0.2f;
    
    [Header("操作台状态")]
    public bool showStatus = true;
    
    [Header("受损模型")]
    public bool showDamageModel = true;
    public int damageModelSize = 120;
    
    private Transform playerShip;
    private List<Transform> enemiesOnRadar = new List<Transform>();
    private List<Transform> missiles = new List<Transform>();
    private float warningTimer = 0f;
    private bool isLocked = false;
    private bool isMissileWarning = false;
    private float missileDistance = 0f;
    
    // 飞船部件受损状态（模拟）
    private float engineHealth = 1f;      // 发动机
    private float wingHealth = 1f;        // 机翼
    private float hullHealth = 1f;        // 船体
    private float cockpitHealth = 1f;     // 舰桥
    private float landingGearHealth = 1f; // 起落架
    
    void Awake()
    {
        instance = this;
    }
    
    void Start()
    {
        playerShip = GameObject.FindGameObjectWithTag("Player")?.transform;
    }
    
    void Update()
    {
        warningTimer += Time.deltaTime;
        
        // 检测敌人和导弹
        DetectEnemiesAndMissiles();
        
        // 模拟受损变化（测试用）
        if (Input.GetKeyDown(KeyCode.Alpha1)) engineHealth = Mathf.Max(0, engineHealth - 0.2f);
        if (Input.GetKeyDown(KeyCode.Alpha2)) wingHealth = Mathf.Max(0, wingHealth - 0.2f);
        if (Input.GetKeyDown(KeyCode.Alpha3)) hullHealth = Mathf.Max(0, hullHealth - 0.2f);
        if (Input.GetKeyDown(KeyCode.Alpha4)) cockpitHealth = Mathf.Max(0, cockpitHealth - 0.2f);
        if (Input.GetKeyDown(KeyCode.Alpha5)) landingGearHealth = Mathf.Max(0, landingGearHealth - 0.2f);
    }
    
    void DetectEnemiesAndMissiles()
    {
        enemiesOnRadar.Clear();
        missiles.Clear();
        
        // 使用名称查找敌人和导弹（避免标签问题）
        GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();
        
        foreach (GameObject obj in allObjects)
        {
            if (obj.name.StartsWith("Enemy_") && playerShip != null)
            {
                Transform enemy = obj.transform;
                if (enemy == null) continue;
                
                Vector3 toEnemy = enemy.position - playerShip.position;
                float distance = toEnemy.magnitude;
                float forwardDot = Vector3.Dot(playerShip.forward, toEnemy.normalized);
                
                // 前方100km雷达
                if (forwardDot > 0 && distance < frontRadarRange)
                {
                    enemiesOnRadar.Add(enemy);
                }
                // 侧面60km雷达
                else if (Mathf.Abs(forwardDot) < 0.707f && distance < sideRadarRange)
                {
                    enemiesOnRadar.Add(enemy);
                }
            }
            else if (obj.name == "Missile" && playerShip != null)
            {
                Transform missile = obj.transform;
                if (missile == null) continue;
                
                Vector3 toMissile = missile.position - playerShip.position;
                float distance = toMissile.magnitude;
                
                // 后方40km导弹检测
                float forwardDot = Vector3.Dot(playerShip.forward, toMissile.normalized);
                if (forwardDot < -0.5f && distance < rearMissileRange)
                {
                    missiles.Add(missile);
                }
            }
        }
    }
    
    void OnGUI()
    {
        if (!showStatus) return;
        
        // 速度显示
        if (playerShip != null)
        {
            float speed = playerShip.GetComponent<Rigidbody>()?.velocity.magnitude ?? 0;
            GUI.Label(new Rect(10, 10, 200, 20), $"速度: {speed:F1} m/s");
            
            // 高度显示
            GUI.Label(new Rect(10, 30, 200, 20), $"高度: {playerShip.position.y:F1} m");
        }
        
        // 在线人数（模拟）
        GUI.Label(new Rect(Screen.width - 150, 10, 150, 20), "在线: 16/32");
        
        // 飞船状态
        GUI.Label(new Rect(Screen.width - 150, 30, 150, 20), "状态: 正常");
        
        // 战斗状态
        GUI.Label(new Rect(Screen.width - 150, 50, 150, 20), "战斗: 进行中");
        
        // 锁定警告
        if (showWarnings && isLocked)
        {
            bool flash = Mathf.Floor(warningTimer / warningFlashInterval) % 2 == 0;
            if (flash)
            {
                GUI.color = Color.red;
                GUI.Label(new Rect(Screen.width / 2 - 50, Screen.height / 2 - 80, 100, 30), "⚠️ 被锁定！");
                GUI.color = Color.white;
            }
        }
        
        // 导弹来袭警告
        if (showWarnings && isMissileWarning)
        {
            bool flash = Mathf.Floor(warningTimer / (warningFlashInterval * 0.5f)) % 2 == 0;
            if (flash)
            {
                GUI.color = Color.magenta;
                GUI.Label(new Rect(Screen.width / 2 - 80, Screen.height - 50, 160, 30), $"🚀 导弹来袭！距离: {missileDistance/1000:F1} km");
                GUI.color = Color.white;
            }
        }
        
        // 雷达显示（简化）
        if (showRadar)
        {
            DrawRadar();
        }
        
        // 受损小模型（左下角）
        if (showDamageModel)
        {
            DrawDamageModel();
        }
        
        // 准星
        if (showAimReticle)
        {
            DrawAimReticle();
        }
    }
    
    void DrawAimReticle()
    {
        int centerX = Screen.width / 2;
        int centerY = Screen.height / 2;
        int size = 30;
        int lineWidth = 2;
        
        GUI.color = reticleColor;
        
        // 十字准星 - 使用Box模拟线条
        GUI.Box(new Rect(centerX - size, centerY - lineWidth/2, size - 10, lineWidth), "");
        GUI.Box(new Rect(centerX + 10, centerY - lineWidth/2, size - 10, lineWidth), "");
        GUI.Box(new Rect(centerX - lineWidth/2, centerY - size, lineWidth, size - 10), "");
        GUI.Box(new Rect(centerX - lineWidth/2, centerY + 10, lineWidth, size - 10), "");
        
        // 瞄准环
        DrawCircle(new Vector2(centerX, centerY), 50, reticleColor);
        
        GUI.color = Color.white;
    }
    
    void DrawCircle(Vector2 center, float radius, Color color)
    {
        GUI.color = color;
        
        // 使用多个矩形模拟圆形
        int segments = 32;
        float angleStep = 360f / segments;
        
        for (int i = 0; i < segments; i++)
        {
            float angle1 = i * angleStep * Mathf.Deg2Rad;
            float angle2 = (i + 1) * angleStep * Mathf.Deg2Rad;
            
            float x1 = center.x + Mathf.Cos(angle1) * radius;
            float y1 = center.y + Mathf.Sin(angle1) * radius;
            float x2 = center.x + Mathf.Cos(angle2) * radius;
            float y2 = center.y + Mathf.Sin(angle2) * radius;
            
            // 绘制小段线
            float dx = x2 - x1;
            float dy = y2 - y1;
            float length = Mathf.Sqrt(dx * dx + dy * dy);
            if (length > 0.1f)
            {
                float angle = Mathf.Atan2(dy, dx) * Mathf.Rad2Deg;
                GUIUtility.RotateAroundPivot(angle, new Vector2(x1, y1));
                GUI.Box(new Rect(x1, y1, length, 2), "");
                GUIUtility.RotateAroundPivot(-angle, new Vector2(x1, y1));
            }
        }
        
        GUI.color = Color.white;
    }
    
    void DrawRadar()
    {
        int radarSize = 150;
        int radarX = Screen.width - radarSize - 10;  // 移到右边
        int radarY = Screen.height - radarSize - 10;
        
        // 雷达背景
        GUI.Box(new Rect(radarX, radarY, radarSize, radarSize), "雷达");
        
        GUI.color = Color.cyan;
        
        // 雷达范围指示
        float innerRadius = radarSize * 0.3f;
        float outerRadius = radarSize * 0.45f;
        
        // 前方扇形（100km）
        DrawRadarArc(new Vector2(radarX + radarSize/2, radarY + radarSize/2), outerRadius, -45, 45, Color.cyan);
        
        // 侧面区域（60km）
        DrawRadarArc(new Vector2(radarX + radarSize/2, radarY + radarSize/2), innerRadius, 45, 135, Color.blue);
        DrawRadarArc(new Vector2(radarX + radarSize/2, radarY + radarSize/2), innerRadius, -135, -45, Color.blue);
        
        // 敌人点
        foreach (Transform enemy in enemiesOnRadar)
        {
            if (playerShip != null && enemy != null)
            {
                Vector3 relativePos = playerShip.InverseTransformPoint(enemy.position);
                float distance = relativePos.magnitude;
                float angle = Mathf.Atan2(relativePos.x, relativePos.z) * Mathf.Rad2Deg;
                
                float normalizedDistance = Mathf.Clamp(distance / frontRadarRange, 0, 1);
                float pointRadius = outerRadius * normalizedDistance;
                
                float x = Mathf.Sin(angle * Mathf.Deg2Rad) * pointRadius;
                float y = Mathf.Cos(angle * Mathf.Deg2Rad) * pointRadius;
                
                GUI.color = Color.red;
                GUI.DrawTexture(new Rect(radarX + radarSize/2 + x - 3, radarY + radarSize/2 + y - 3, 6, 6), Texture2D.whiteTexture);
            }
        }
        
        // 导弹点
        foreach (Transform missile in missiles)
        {
            if (playerShip != null && missile != null)
            {
                Vector3 relativePos = playerShip.InverseTransformPoint(missile.position);
                float distance = relativePos.magnitude;
                float angle = Mathf.Atan2(relativePos.x, relativePos.z) * Mathf.Rad2Deg;
                
                float normalizedDistance = Mathf.Clamp(distance / rearMissileRange, 0, 1);
                float pointRadius = outerRadius * normalizedDistance;
                
                float x = Mathf.Sin(angle * Mathf.Deg2Rad) * pointRadius;
                float y = Mathf.Cos(angle * Mathf.Deg2Rad) * pointRadius;
                
                GUI.color = Color.magenta;
                GUI.DrawTexture(new Rect(radarX + radarSize/2 + x - 4, radarY + radarSize/2 + y - 4, 8, 8), Texture2D.whiteTexture);
            }
        }
        
        // 距离标签
        GUI.color = Color.white;
        GUI.Label(new Rect(radarX - 85, radarY + radarSize/4, 80, 20), "前: 100km");
        GUI.Label(new Rect(radarX - 85, radarY + radarSize/2, 80, 20), "侧: 60km");
        GUI.Label(new Rect(radarX - 85, radarY + radarSize * 3/4, 80, 20), "后: 40km");
        
        GUI.color = Color.white;
    }
    
    void DrawRadarArc(Vector2 center, float radius, float startAngle, float endAngle, Color color)
    {
        GUI.color = color;
        
        int segments = 20;
        float angleStep = (endAngle - startAngle) / segments;
        
        for (int i = 0; i < segments; i++)
        {
            float angle1 = startAngle + angleStep * i;
            float angle2 = startAngle + angleStep * (i + 1);
            
            float x1 = center.x + Mathf.Sin(angle1 * Mathf.Deg2Rad) * radius;
            float y1 = center.y + Mathf.Cos(angle1 * Mathf.Deg2Rad) * radius;
            float x2 = center.x + Mathf.Sin(angle2 * Mathf.Deg2Rad) * radius;
            float y2 = center.y + Mathf.Cos(angle2 * Mathf.Deg2Rad) * radius;
            
            // 使用Box模拟线条
            float dx = x2 - x1;
            float dy = y2 - y1;
            float length = Mathf.Sqrt(dx * dx + dy * dy);
            if (length > 0.1f)
            {
                float angle = Mathf.Atan2(dy, dx) * Mathf.Rad2Deg;
                GUIUtility.RotateAroundPivot(angle, new Vector2(x1, y1));
                GUI.Box(new Rect(x1, y1, length, 2), "");
                GUIUtility.RotateAroundPivot(-angle, new Vector2(x1, y1));
            }
        }
        
        // 连接线到中心
        float startX = center.x + Mathf.Sin(startAngle * Mathf.Deg2Rad) * radius;
        float startY = center.y + Mathf.Cos(startAngle * Mathf.Deg2Rad) * radius;
        float endX = center.x + Mathf.Sin(endAngle * Mathf.Deg2Rad) * radius;
        float endY = center.y + Mathf.Cos(endAngle * Mathf.Deg2Rad) * radius;
        
        // 中心到起点
        float dx1 = startX - center.x;
        float dy1 = startY - center.y;
        float len1 = Mathf.Sqrt(dx1 * dx1 + dy1 * dy1);
        if (len1 > 0.1f)
        {
            float angle1 = Mathf.Atan2(dy1, dx1) * Mathf.Rad2Deg;
            GUIUtility.RotateAroundPivot(angle1, center);
            GUI.Box(new Rect(center.x, center.y, len1, 2), "");
            GUIUtility.RotateAroundPivot(-angle1, center);
        }
        
        // 中心到终点
        float dx2 = endX - center.x;
        float dy2 = endY - center.y;
        float len2 = Mathf.Sqrt(dx2 * dx2 + dy2 * dy2);
        if (len2 > 0.1f)
        {
            float angle2 = Mathf.Atan2(dy2, dx2) * Mathf.Rad2Deg;
            GUIUtility.RotateAroundPivot(angle2, center);
            GUI.Box(new Rect(center.x, center.y, len2, 2), "");
            GUIUtility.RotateAroundPivot(-angle2, center);
        }
        
        GUI.color = Color.white;
    }
    
    public void SetLockWarning(bool locked)
    {
        isLocked = locked;
    }
    
    public void SetMissileWarning(bool warning, float distance = 0)
    {
        isMissileWarning = warning;
        missileDistance = distance;
    }
    
    public void AddMissile(Transform missile)
    {
        if (!missiles.Contains(missile))
        {
            missiles.Add(missile);
        }
    }
    
    void DrawDamageModel()
    {
        int modelX = 10;
        int modelY = Screen.height - damageModelSize - 10;
        
        // 模型背景框
        GUI.Box(new Rect(modelX, modelY, damageModelSize, damageModelSize), "飞船状态");
        
        // 飞船中心点
        int centerX = modelX + damageModelSize / 2;
        int centerY = modelY + damageModelSize / 2;
        
        // 飞船主体（简化为长方形）
        int bodyWidth = 80;
        int bodyHeight = 20;
        GUI.color = GetDamageColor(hullHealth);
        GUI.Box(new Rect(centerX - bodyWidth/2, centerY - bodyHeight/2, bodyWidth, bodyHeight), "");
        
        // 左侧发动机
        int engineWidth = 25;
        int engineHeight = 15;
        GUI.color = GetDamageColor(engineHealth);
        GUI.Box(new Rect(centerX - bodyWidth/2 - engineWidth, centerY - engineHeight/2, engineWidth, engineHeight), "");
        
        // 右侧发动机
        GUI.Box(new Rect(centerX + bodyWidth/2, centerY - engineHeight/2, engineWidth, engineHeight), "");
        
        // 机翼（上下）
        int wingWidth = 15;
        int wingHeight = 30;
        GUI.color = GetDamageColor(wingHealth);
        GUI.Box(new Rect(centerX - wingWidth/2, centerY - bodyHeight/2 - wingHeight, wingWidth, wingHeight), "");
        GUI.Box(new Rect(centerX - wingWidth/2, centerY + bodyHeight/2, wingWidth, wingHeight), "");
        
        // 舰桥（顶部）
        int cockpitWidth = 15;
        int cockpitHeight = 15;
        GUI.color = GetDamageColor(cockpitHealth);
        GUI.Box(new Rect(centerX - cockpitWidth/2, centerY - bodyHeight/2 - wingHeight - cockpitHeight, cockpitWidth, cockpitHeight), "");
        
        // 起落架指示（底部）
        int gearWidth = 10;
        int gearHeight = 12;
        GUI.color = GetDamageColor(landingGearHealth);
        GUI.Box(new Rect(centerX - 20, centerY + bodyHeight/2 + wingHeight, gearWidth, gearHeight), "");
        GUI.Box(new Rect(centerX + 10, centerY + bodyHeight/2 + wingHeight, gearWidth, gearHeight), "");
        
        // 状态标签
        GUI.color = Color.white;
        GUI.Label(new Rect(modelX, modelY - 20, damageModelSize, 20), $"船体: {Mathf.RoundToInt(hullHealth * 100)}%");
        GUI.Label(new Rect(modelX, modelY + damageModelSize + 5, 80, 20), $"发动机: {Mathf.RoundToInt(engineHealth * 100)}%");
        GUI.Label(new Rect(modelX + 85, modelY + damageModelSize + 5, 80, 20), $"机翼: {Mathf.RoundToInt(wingHealth * 100)}%");
        GUI.Label(new Rect(modelX, modelY + damageModelSize + 25, 80, 20), $"舰桥: {Mathf.RoundToInt(cockpitHealth * 100)}%");
        GUI.Label(new Rect(modelX + 85, modelY + damageModelSize + 25, 80, 20), $"起落架: {Mathf.RoundToInt(landingGearHealth * 100)}%");
        
        GUI.color = Color.white;
    }
    
    Color GetDamageColor(float health)
    {
        if (health > 0.7f)
            return Color.green;
        else if (health > 0.4f)
            return Color.yellow;
        else if (health > 0.1f)
            return Color.red;
        else
            return Color.gray;
    }
    
    public void RemoveMissile(Transform missile)
    {
        missiles.Remove(missile);
    }
    
    public void SetMissileIncoming(bool incoming, float distance = 0)
    {
        isMissileWarning = incoming;
        missileDistance = distance;
    }
    
    public void SetLockedByEnemy(bool locked)
    {
        isLocked = locked;
    }
}