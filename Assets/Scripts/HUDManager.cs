using UnityEngine;
using System.Collections.Generic;

public class HUDManager : MonoBehaviour
{
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
    
    private Transform playerShip;
    private List<Transform> enemiesOnRadar = new List<Transform>();
    private List<Transform> missiles = new List<Transform>();
    private float warningTimer = 0f;
    private bool isLocked = false;
    private bool isMissileWarning = false;
    private float missileDistance = 0f;
    
    void Start()
    {
        playerShip = GameObject.FindGameObjectWithTag("Player")?.transform;
    }
    
    void Update()
    {
        warningTimer += Time.deltaTime;
        
        // 检测敌人和导弹
        DetectEnemiesAndMissiles();
    }
    
    void DetectEnemiesAndMissiles()
    {
        enemiesOnRadar.Clear();
        missiles.Clear();
        
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        GameObject[] missileObjects = GameObject.FindGameObjectsWithTag("Missile");
        
        foreach (GameObject enemy in enemies)
        {
            if (playerShip != null)
            {
                Vector3 toEnemy = enemy.transform.position - playerShip.position;
                float distance = toEnemy.magnitude;
                
                // 前方100km雷达
                float forwardDot = Vector3.Dot(playerShip.forward, toEnemy.normalized);
                if (forwardDot > 0 && distance < frontRadarRange)
                {
                    enemiesOnRadar.Add(enemy.transform);
                }
                // 侧面60km雷达
                else if (Mathf.Abs(forwardDot) < 0.707f && distance < sideRadarRange)
                {
                    enemiesOnRadar.Add(enemy.transform);
                }
            }
        }
        
        foreach (GameObject missile in missileObjects)
        {
            if (playerShip != null)
            {
                Vector3 toMissile = missile.transform.position - playerShip.position;
                float distance = toMissile.magnitude;
                
                missiles.Add(missile.transform);
                
                // 后方导弹检测
                float forwardDot = Vector3.Dot(playerShip.forward, toMissile.normalized);
                if (forwardDot < 0 && distance < rearMissileRange)
                {
                    isMissileWarning = true;
                    missileDistance = distance;
                }
            }
        }
        
        // 锁定检测（简化版本）
        isLocked = enemiesOnRadar.Count > 0 && Random.value > 0.7f;
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
        
        GUI.color = reticleColor;
        
        // 十字准星
        GUI.DrawLine(new Vector2(centerX - size, centerY), new Vector2(centerX - 10, centerY));
        GUI.DrawLine(new Vector2(centerX + 10, centerY), new Vector2(centerX + size, centerY));
        GUI.DrawLine(new Vector2(centerX, centerY - size), new Vector2(centerX, centerY - 10));
        GUI.DrawLine(new Vector2(centerX, centerY + 10), new Vector2(centerX, centerY + size));
        
        // 瞄准环
        DrawCircle(new Vector2(centerX, centerY), 50, reticleColor);
        
        GUI.color = Color.white;
    }
    
    void DrawCircle(Vector2 center, float radius, Color color)
    {
        GUI.color = color;
        int segments = 32;
        Vector2[] points = new Vector2[segments + 1];
        
        for (int i = 0; i <= segments; i++)
        {
            float angle = (i / (float)segments) * Mathf.PI * 2;
            points[i] = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
        }
        
        for (int i = 0; i < segments; i++)
        {
            GUI.DrawLine(points[i], points[i + 1]);
        }
        
        GUI.color = Color.white;
    }
    
    void DrawRadar()
    {
        int radarSize = 150;
        int radarX = 10;
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
            if (playerShip != null)
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
            if (playerShip != null)
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
        GUI.Label(new Rect(radarX + radarSize + 5, radarY + radarSize/4, 80, 20), "前: 100km");
        GUI.Label(new Rect(radarX + radarSize + 5, radarY + radarSize/2, 80, 20), "侧: 60km");
        GUI.Label(new Rect(radarX + radarSize + 5, radarY + radarSize * 3/4, 80, 20), "后: 40km");
        
        GUI.color = Color.white;
    }
    
    void DrawRadarArc(Vector2 center, float radius, float startAngle, float endAngle, Color color)
    {
        GUI.color = color;
        
        int segments = 20;
        float angleStep = (endAngle - startAngle) / segments;
        
        Vector2 lastPoint = center + new Vector2(
            Mathf.Sin(startAngle * Mathf.Deg2Rad) * radius,
            Mathf.Cos(startAngle * Mathf.Deg2Rad) * radius
        );
        
        for (int i = 1; i <= segments; i++)
        {
            float angle = startAngle + angleStep * i;
            Vector2 point = center + new Vector2(
                Mathf.Sin(angle * Mathf.Deg2Rad) * radius,
                Mathf.Cos(angle * Mathf.Deg2Rad) * radius
            );
            
            GUI.DrawLine(lastPoint, point);
            lastPoint = point;
        }
        
        // 连接线到中心
        Vector2 startPoint = center + new Vector2(
            Mathf.Sin(startAngle * Mathf.Deg2Rad) * radius,
            Mathf.Cos(startAngle * Mathf.Deg2Rad) * radius
        );
        Vector2 endPoint = center + new Vector2(
            Mathf.Sin(endAngle * Mathf.Deg2Rad) * radius,
            Mathf.Cos(endAngle * Mathf.Deg2Rad) * radius
        );
        
        GUI.DrawLine(center, startPoint);
        GUI.DrawLine(center, endPoint);
        
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
}