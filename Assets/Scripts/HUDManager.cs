using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class HUDManager : MonoBehaviour
{
    public static HUDManager instance;
    
    [Header("雷达显示")]
    public Image radarBackground;
    public Image radarSweep;
    public float radarRange = 100f;
    
    [Header("锁定警告")]
    public Image radarWarningPanel;
    public Text radarWarningText;
    public Image missileWarningDot;
    public Text missileDirectionText;
    
    [Header("速度显示")]
    public Text speedText;
    public Text altitudeText;
    
    [Header("操作台状态")]
    public Text radarStatusText;
    
    [Header("颜色设置")]
    public Color normalColor = new Color(0.2f, 0.6f, 1f);
    public Color warningColor = new Color(1f, 0.5f, 0f);
    public Color dangerColor = new Color(1f, 0f, 0f);
    
    private bool isLockedByEnemy = false;
    private bool isMissileIncoming = false;
    private Transform playerShip;
    
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }
    
    void Start()
    {
        playerShip = GameObject.Find("PlayerShip")?.transform;
        
        if (radarStatusText != null)
        {
            radarStatusText.text = "正前雷达: 100km | 侧面雷达: 60km | 后方导弹检测: 40km";
        }
    }
    
    void Update()
    {
        UpdateRadar();
        UpdateSpeedDisplay();
        
        if (isLockedByEnemy || isMissileIncoming)
        {
            UpdateRadarWarningBox();
        }
    }
    
    void UpdateRadar()
    {
        if (radarSweep != null)
        {
            radarSweep.transform.Rotate(0, 0, -180f * Time.deltaTime);
        }
    }
    
    void UpdateSpeedDisplay()
    {
        if (playerShip == null)
        {
            playerShip = GameObject.Find("PlayerShip")?.transform;
            return;
        }
        
        Rigidbody rb = playerShip.GetComponent<Rigidbody>();
        if (rb != null)
        {
            float speed = rb.velocity.magnitude * 3.6f; // m/s to km/h
            
            if (speedText != null)
                speedText.text = $"速度: {speed:F0} km/h";
            
            if (altitudeText != null)
                altitudeText.text = $"高度: {playerShip.position.y:F0} m";
        }
    }
    
    void UpdateRadarWarningBox()
    {
        if (radarWarningPanel != null)
        {
            radarWarningPanel.enabled = true;
            
            float flash = Mathf.PingPong(Time.time * 5f, 1f);
            radarWarningPanel.color = Color.Lerp(
                new Color(0.8f, 0f, 0f, 0.6f),
                new Color(1f, 0.2f, 0.2f, 0.9f),
                flash
            );
        }
        
        if (radarWarningText != null)
        {
            radarWarningText.enabled = true;
            radarWarningText.text = isMissileIncoming ? "⚠️ 导弹来袭 ⚠️" : "⚠️ 被锁定 ⚠️";
            
            float flash = Mathf.PingPong(Time.time * 5f, 1f);
            radarWarningText.color = Color.Lerp(dangerColor, Color.white, flash);
        }
        
        if (isMissileIncoming && missileDirectionText != null)
        {
            missileDirectionText.enabled = true;
            missileDirectionText.text = $"导弹方向: {GetMissileDirection()}";
            
            float flash = Mathf.PingPong(Time.time * 5f, 1f);
            missileDirectionText.color = Color.Lerp(warningColor, dangerColor, flash);
        }
        
        UpdateMissileWarningDot();
    }
    
    void UpdateMissileWarningDot()
    {
        if (missileWarningDot != null && isMissileIncoming)
        {
            missileWarningDot.enabled = true;
            
            float flash = Mathf.PingPong(Time.time * 8f, 1f);
            missileWarningDot.color = Color.Lerp(
                new Color(1f, 0f, 0f, 0.5f),
                new Color(1f, 0f, 0f, 1f),
                flash
            );
            
            missileWarningDot.transform.localScale = Vector3.one * (1f + flash * 0.5f);
        }
    }
    
    string GetMissileDirection()
    {
        if (playerShip == null) return "未知";
        
        // 模拟导弹方向检测
        float angle = Random.Range(0f, 360f);
        
        if (angle < 45 || angle >= 315) return "正前方";
        if (angle < 135) return "右侧";
        if (angle < 225) return "后方";
        return "左侧";
    }
    
    public void SetLockedByEnemy(bool locked)
    {
        isLockedByEnemy = locked;
    }
    
    public void SetMissileIncoming(bool incoming)
    {
        isMissileIncoming = incoming;
    }
}