using UnityEngine;
using System.Collections.Generic;

public class Damageable : MonoBehaviour
{
    [Header("=== 生命值设置 ===")]
    public float maxHealth = 1000f;
    public float currentHealth;
    public float autoRepairRate = 5f;      // 自动修复速度（生命/秒）
    public float autoRepairDelay = 3f;     // 受伤后自动修复延迟（秒）
    
    [Header("=== 部件系统 ===")]
    public List<ShipPart> parts = new List<ShipPart>();
    
    [Header("=== 特效 ===")]
    public GameObject hitEffectPrefab;
    public GameObject explosionEffectPrefab;
    public GameObject repairEffectPrefab;
    
    [Header("=== 音效 ===")]
    public AudioClip hitSound;
    public AudioClip explosionSound;
    public AudioClip warningSound;
    
    private AudioSource audioSource;
    private float lastDamageTime;
    
    [System.Serializable]
    public class ShipPart
    {
        public string partName;           // 部件名称
        public PartType type;            // 部件类型
        public float maxPartHealth = 100f;  // 部件最大生命
        public float currentPartHealth;   // 当前生命
        public bool isDestroyed;          // 是否损毁（灰）
        public bool canAutoRepair = true; // 能否自动修复
        
        // 飞行影响
        public float speedPenalty = 0f;   // 速度惩罚
        public float handlingPenalty = 0f;// 操控惩罚
        public float thrustPenalty = 0f;  // 推力惩罚
        
        public enum PartType
        {
            Bridge,      // 舰桥（要害，秒杀）
            LeftEngine,  // 左发动机
            RightEngine, // 右发动机
            LandingGear, // 起落架
            LeftAileron, // 左副翼
            RightAileron,// 右副翼
            TailSection, // 尾翼
            Hull         // 船体
        }
        
        public ShipPart()
        {
            currentPartHealth = maxPartHealth;
            isDestroyed = false;
        }
        
        // 受损：生命值越低越红
        public Color GetHealthColor(float normalCyan, float warningOrange, float dangerRed, float destroyedGray)
        {
            float ratio = currentPartHealth / maxPartHealth;
            
            if (isDestroyed)
                return destroyedGray; // 灰色（损毁）
            
            if (ratio > 0.6f)
                return normalCyan; // 青色（正常）
            else if (ratio > 0.3f)
                return Color.Lerp(dangerRed, warningOrange, (ratio - 0.3f) / 0.3f); // 橙红渐变
            else
                return dangerRed; // 红色（危急）
        }
        
        // 承受伤害
        public void TakeDamage(float amount)
        {
            if (isDestroyed) return; // 损毁部件不再受伤
            
            currentPartHealth = Mathf.Max(0f, currentPartHealth - amount);
            
            // 舰桥被击中直接秒杀
            if (type == PartType.Bridge && amount > 0)
            {
                isDestroyed = true;
                currentPartHealth = 0f;
                return;
            }
            
            // 检查是否损毁（临界点）
            // 受损严重时变灰
            if (currentPartHealth <= 0f)
            {
                // 根据受损程度判断是否损毁
                // 如果生命值低于20%且再次受伤，则损毁
                if (currentPartHealth <= 0f && !isDestroyed)
                {
                    isDestroyed = true;
                    canAutoRepair = false; // 损毁后不可自动修复
                }
            }
        }
        
        // 自动修复（非损毁部件）
        public void AutoRepair(float rate, float deltaTime)
        {
            if (isDestroyed || !canAutoRepair) return; // 灰的不能修
            
            currentPartHealth = Mathf.Min(currentPartHealth + rate * deltaTime, maxPartHealth);
        }
        
        // 获取飞行影响
        public float GetSpeedMultiplier()
        {
            if (isDestroyed) return 0.5f; // 严重减速
            return 1f - (speedPenalty * (1f - currentPartHealth / maxPartHealth));
        }
        
        public float GetHandlingMultiplier()
        {
            if (isDestroyed) return 0.3f; // 操控极差
            return 1f - (handlingPenalty * (1f - currentPartHealth / maxPartHealth));
        }
        
        public float GetThrustMultiplier()
        {
            if (isDestroyed) return 0.2f; // 推力严重下降
            return 1f - (thrustPenalty * (1f - currentPartHealth / maxPartHealth));
        }
    }
    
    void Awake()
    {
        currentHealth = maxHealth;
        audioSource = GetComponent<AudioSource>();
        
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
        
        // 初始化部件
        InitializeParts();
    }
    
    void InitializeParts()
    {
        if (parts.Count == 0)
        {
            // 默认部件配置
            parts.Add(new ShipPart { partName = "舰桥", type = ShipPart.PartType.Bridge, maxPartHealth = 150f, canAutoRepair = false });
            parts.Add(new ShipPart { partName = "左发动机", type = ShipPart.PartType.LeftEngine, maxPartHealth = 200f, thrustPenalty = 0.4f });
            parts.Add(new ShipPart { partName = "右发动机", type = ShipPart.PartType.RightEngine, maxPartHealth = 200f, thrustPenalty = 0.4f });
            parts.Add(new ShipPart { partName = "起落架", type = ShipPart.PartType.LandingGear, maxPartHealth = 100f, canAutoRepair = false });
            parts.Add(new ShipPart { partName = "左副翼", type = ShipPart.PartType.LeftAileron, maxPartHealth = 120f, handlingPenalty = 0.3f });
            parts.Add(new ShipPart { partName = "右副翼", type = ShipPart.PartType.RightAileron, maxPartHealth = 120f, handlingPenalty = 0.3f });
            parts.Add(new ShipPart { partName = "尾翼", type = ShipPart.PartType.TailSection, maxPartHealth = 100f, handlingPenalty = 0.2f });
            parts.Add(new ShipPart { partName = "主船体", type = ShipPart.PartType.Hull, maxPartHealth = 300f, speedPenalty = 0.3f });
        }
        
        // 同步总生命值
        UpdateTotalHealth();
    }
    
    void Update()
    {
        // 自动修复（非损毁部件）
        if (Time.time - lastDamageTime > autoRepairDelay)
        {
            foreach (var part in parts)
            {
                part.AutoRepair(autoRepairRate, Time.deltaTime);
            }
            UpdateTotalHealth();
        }
    }
    
    void UpdateTotalHealth()
    {
        float total = 0f;
        float totalMax = 0f;
        
        foreach (var part in parts)
        {
            if (!part.isDestroyed) // 损毁部件不计入
            {
                total += part.currentPartHealth;
                totalMax += part.maxPartHealth;
            }
            else
            {
                totalMax += part.maxPartHealth; // 损毁部件仍计入最大值
            }
        }
        
        currentHealth = total;
        maxHealth = totalMax;
    }
    
    // 承受伤害（支持导弹和激光舰炮）
    public void TakeDamage(float amount, DamageType type = DamageType.Missile, ShipPart.PartType? targetPart = null)
    {
        lastDamageTime = Time.time;
        
        // 激光舰炮：持续伤害，穿透性强
        if (type == DamageType.LaserCannon)
        {
            amount *= 1.2f; // 激光伤害略高
        }
        
        // 导弹：爆炸伤害，可被干扰弹诱骗
        if (type == DamageType.Missile)
        {
            amount *= 1.5f; // 导弹伤害较高
        }
        
        // 指定部件受伤
        if (targetPart.HasValue)
        {
            var part = parts.Find(p => p.type == targetPart.Value);
            if (part != null)
            {
                part.TakeDamage(amount);
                
                if (part.type == ShipPart.PartType.Bridge && part.isDestroyed)
                {
                    // 舰桥被击中秒杀！
                    OnBridgeDestroyed();
                    return;
                }
            }
        }
        else
        {
            // 随机部件受伤
            var healthyParts = parts.FindAll(p => !p.isDestroyed);
            if (healthyParts.Count > 0)
            {
                var randomPart = healthyParts[Random.Range(0, healthyParts.Count)];
                randomPart.TakeDamage(amount);
                
                if (randomPart.type == ShipPart.PartType.Bridge && randomPart.isDestroyed)
                {
                    OnBridgeDestroyed();
                    return;
                }
            }
        }
        
        UpdateTotalHealth();
        
        // 特效
        if (hitEffectPrefab != null)
        {
            Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
        }
        
        if (hitSound != null)
        {
            audioSource.PlayOneShot(hitSound);
        }
        
        // 警告音效
        float healthRatio = HealthPercentage;
        if (healthRatio < 0.3f && warningSound != null)
        {
            audioSource.PlayOneShot(warningSound);
        }
        
        if (currentHealth <= 0f)
        {
            OnDeath();
        }
    }
    
    // 舰桥被击中秒杀
    void OnBridgeDestroyed()
    {
        Debug.Log("舰桥被击中！飞船秒毁！");
        
        if (explosionEffectPrefab != null)
        {
            Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
        }
        
        if (explosionSound != null)
        {
            audioSource.PlayOneShot(explosionSound);
        }
        
        OnDeath();
    }
    
    void OnDeath()
    {
        if (explosionEffectPrefab != null)
        {
            Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
        }
        
        if (explosionSound != null)
        {
            audioSource.PlayOneShot(explosionSound);
        }
        
        // 通知飞船控制器
        var flightController = GetComponent<StarShipFlightController>();
        if (flightController != null)
        {
            flightController.enabled = false;
        }
        
        Destroy(gameObject, 0.1f);
    }
    
    public float HealthPercentage => currentHealth / maxHealth;
    
    // 获取部件状态
    public ShipPart GetPart(ShipPart.PartType type)
    {
        return parts.Find(p => p.type == type);
    }
    
    // 获取飞行综合影响
    public float GetSpeedMultiplier()
    {
        float multi = 1f;
        foreach (var part in parts)
        {
            if (!part.isDestroyed && part.type != ShipPart.PartType.LandingGear)
            {
                multi *= part.GetSpeedMultiplier();
            }
        }
        return multi;
    }
    
    public float GetHandlingMultiplier()
    {
        float multi = 1f;
        foreach (var part in parts)
        {
            if (!part.isDestroyed && part.type != ShipPart.PartType.LandingGear)
            {
                multi *= part.GetHandlingMultiplier();
            }
        }
        return multi;
    }
    
    public float GetThrustMultiplier()
    {
        float multi = 1f;
        int enginesDestroyed = 0;
        int totalEngines = 0;
        
        foreach (var part in parts)
        {
            if (part.type == ShipPart.PartType.LeftEngine || part.type == ShipPart.PartType.RightEngine)
            {
                totalEngines++;
                if (part.isDestroyed)
                {
                    enginesDestroyed++;
                }
                else
                {
                    multi *= part.GetThrustMultiplier();
                }
            }
        }
        
        // 双发损毁推力极低
        if (enginesDestroyed == totalEngines)
        {
            multi = 0.1f; // 单靠备用动力
        }
        else if (enginesDestroyed > 0)
        {
            multi *= 0.5f; // 单发推力减半
        }
        
        return multi;
    }
    
    public bool IsLandingGearDestroyed()
    {
        var landingGear = GetPart(ShipPart.PartType.LandingGear);
        return landingGear != null && landingGear.isDestroyed;
    }
    
    public bool AreEnginesDestroyed()
    {
        var left = GetPart(ShipPart.PartType.LeftEngine);
        var right = GetPart(ShipPart.PartType.RightEngine);
        return (left != null && left.isDestroyed) && (right != null && right.isDestroyed);
    }
    
    // 维修（地面）
    public void Repair(float amount)
    {
        foreach (var part in parts)
        {
            if (!part.isDestroyed) // 只能修复未损毁部件
            {
                part.currentPartHealth = Mathf.Min(part.currentPartHealth + amount, part.maxPartHealth);
            }
        }
        UpdateTotalHealth();
    }
    
    public enum DamageType
    {
        Missile,      // 导弹
        LaserCannon,  // 激光舰炮
        Projectile,    // 普通弹丸
        Collision      // 碰撞
    }
}
