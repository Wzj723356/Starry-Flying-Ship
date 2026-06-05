using UnityEngine;

public class LaserCannonWeapon : MonoBehaviour
{
    [Header("=== 激光舰炮配置 ===")]
    public float damagePerSecond = 50f;      // 每秒伤害
    public float range = 3000f;               // 射程
    public float heatPerShot = 2f;            // 每秒热量
    public float maxHeat = 100f;              // 最大热量
    public float coolingRate = 10f;           // 冷却速度（热量/秒）
    public float overheatThreshold = 90f;     // 过热阈值
    public float fireRate = 10f;              // 射速（发/秒）
    
    [Header("=== 视觉特效 ===")]
    public LineRenderer laserBeam;            // 激光束渲染
    public GameObject impactEffect;           // 击中特效
    public Color laserColor = Color.red;      // 激光颜色
    public float laserWidth = 0.1f;           // 激光宽度
    
    [Header("=== 音效 ===")]
    public AudioSource audioSource;
    public AudioClip fireSound;
    public AudioClip overheatSound;
    
    private float currentHeat = 0f;
    private bool isOverheated = false;
    private bool isFiring = false;
    private float lastFireTime;
    
    // HUD引用
    private HUDManager hud;
    
    void Start()
    {
        hud = FindObjectOfType<HUDManager>();
        
        // 初始化激光束
        if (laserBeam == null)
        {
            laserBeam = gameObject.AddComponent<LineRenderer>();
            laserBeam.material = new Material(Shader.Find("Sprites/Default"));
            laserBeam.startWidth = laserWidth;
            laserBeam.endWidth = laserWidth * 0.5f;
            laserBeam.startColor = laserColor;
            laserBeam.endColor = laserColor * 0.8f;
            laserBeam.enabled = false;
        }
        
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }
    
    void Update()
    {
        // 射击输入
        if (Input.GetMouseButton(1)) // 右键射击
        {
            Fire();
        }
        else
        {
            isFiring = false;
            laserBeam.enabled = false;
        }
        
        // 热量管理
        if (!isFiring || isOverheated)
        {
            currentHeat = Mathf.Max(0f, currentHeat - coolingRate * Time.deltaTime);
            
            // 过热冷却后恢复
            if (isOverheated && currentHeat < overheatThreshold - 10f)
            {
                isOverheated = false;
            }
        }
    }
    
    public void Fire()
    {
        if (isOverheated)
        {
            // 过热时播放警告
            if (!audioSource.isPlaying && overheatSound != null)
            {
                audioSource.PlayOneShot(overheatSound);
            }
            return;
        }
        
        if (Time.time - lastFireTime < 1f / fireRate)
            return;
        
        lastFireTime = Time.time;
        isFiring = true;
        
        // 添加热量
        currentHeat += heatPerShot;
        if (currentHeat >= maxHeat)
        {
            currentHeat = maxHeat;
            isOverheated = true;
        }
        
        // 播放音效
        if (fireSound != null && !audioSource.isPlaying)
        {
            audioSource.PlayOneShot(fireSound);
        }
        
        // 射线检测
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit, range))
        {
            // 显示激光束
            laserBeam.enabled = true;
            laserBeam.SetPosition(0, transform.position);
            laserBeam.SetPosition(1, hit.point);
            
            // 造成持续伤害
            var damageable = hit.collider.GetComponent<Damageable>();
            if (damageable != null)
            {
                // 激光伤害
                damageable.TakeDamage(damagePerSecond * Time.deltaTime, Damageable.DamageType.LaserCannon);
            }
            
            // 击中特效
            if (impactEffect != null)
            {
                GameObject effect = Instantiate(impactEffect, hit.point, Quaternion.LookRotation(hit.normal));
                Destroy(effect, 1f);
            }
        }
        else
        {
            // 激光射向远方
            laserBeam.enabled = true;
            laserBeam.SetPosition(0, transform.position);
            laserBeam.SetPosition(1, transform.position + transform.forward * range);
        }
    }
    
    // 热量百分比
    public float HeatPercentage => currentHeat / maxHeat;
    public bool IsOverheated => isOverheated;
    public bool IsFiring => isFiring;
    
    // 强制冷却
    public void ForceCooling()
    {
        currentHeat = 0f;
        isOverheated = false;
    }
}
