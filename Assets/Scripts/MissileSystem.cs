using UnityEngine;

public class MissileSystem : MonoBehaviour
{
    [Header("导弹配置")]
    public float missileSpeed = 300f;
    public float missileTurnRate = 180f;
    public float lockOnTime = 2f;
    public float lockOnRange = 500f;
    public float missileDamage = 50f;
    public float missileLifetime = 10f;
    
    [Header("追踪配置")]
    public Transform target;
    public bool isTracking = true;
    
    private Rigidbody rb;
    private Vector3 launchDirection;
    private float launchTime;
    private bool hasLaunched = false;
    private HUDManager hud;
    
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
        
        rb.useGravity = false;
        rb.isKinematic = false;
        rb.detectCollisions = true;
        
        hud = FindObjectOfType<HUDManager>();
    }
    
    void Update()
    {
        if (!hasLaunched) return;
        
        // 追踪目标
        if (isTracking && target != null)
        {
            TrackTarget();
        }
        
        // 超时销毁
        if (Time.time - launchTime > missileLifetime)
        {
            DestroyMissile();
        }
    }
    
    void FixedUpdate()
    {
        if (!hasLaunched) return;
        
        // 保持前进
        rb.velocity = transform.forward * missileSpeed;
    }
    
    public void Launch(Transform targetTransform)
    {
        target = targetTransform;
        hasLaunched = true;
        launchTime = Time.time;
        launchDirection = transform.forward;
        
        // 向HUD报告导弹发射
        if (hud != null)
        {
            hud.AddMissile(transform);
        }
    }
    
    void TrackTarget()
    {
        Vector3 targetDirection = (target.position - transform.position).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, missileTurnRate * Time.deltaTime);
    }
    
    void OnCollisionEnter(Collision collision)
    {
        // 命中检测
        if (collision.gameObject.CompareTag("Player") || collision.gameObject.CompareTag("Enemy"))
        {
            Damageable damageable = collision.gameObject.GetComponent<Damageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(missileDamage);
            }
        }
        
        // 导弹爆炸效果
        Explode();
        DestroyMissile();
    }
    
    void Explode()
    {
        // 创建爆炸效果
        GameObject explosion = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        explosion.transform.position = transform.position;
        explosion.transform.localScale = Vector3.one * 5f;
        
        Renderer renderer = explosion.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material = new Material(Shader.Find("Unlit/Color"));
            renderer.material.color = Color.red;
        }
        
        // 延迟销毁爆炸效果
        Destroy(explosion, 0.5f);
    }
    
    void DestroyMissile()
    {
        // 从HUD移除
        if (hud != null)
        {
            hud.RemoveMissile(transform);
        }
        
        Destroy(gameObject);
    }
}

public class MissileLauncher : MonoBehaviour
{
    [Header("发射配置")]
    public float fireRate = 2f;
    public int maxAmmo = 20;
    public int currentAmmo;
    public float reloadTime = 3f;
    
    [Header("导弹配置")]
    public GameObject missilePrefab;
    public Transform launchPoint;
    
    private float nextFireTime = 0f;
    private bool isReloading = false;
    
    void Start()
    {
        currentAmmo = maxAmmo;
    }
    
    void Update()
    {
        // 检测发射
        if (Input.GetMouseButton(1) && Time.time >= nextFireTime && currentAmmo > 0 && !isReloading)
        {
            FireMissile();
        }
        
        // 重新装填
        if (currentAmmo <= 0 && !isReloading)
        {
            StartReload();
        }
    }
    
    void FireMissile()
    {
        if (missilePrefab == null || launchPoint == null) return;
        
        // 创建导弹
        GameObject missile = Instantiate(missilePrefab, launchPoint.position, launchPoint.rotation);
        MissileSystem missileSystem = missile.GetComponent<MissileSystem>();
        
        if (missileSystem != null)
        {
            // 寻找最近敌人作为目标
            Transform target = FindNearestEnemy();
            missileSystem.Launch(target);
        }
        
        currentAmmo--;
        nextFireTime = Time.time + fireRate;
    }
    
    Transform FindNearestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        Transform nearest = null;
        float nearestDist = float.MaxValue;
        
        foreach (GameObject enemy in enemies)
        {
            float dist = Vector3.Distance(transform.position, enemy.transform.position);
            if (dist < nearestDist)
            {
                nearestDist = dist;
                nearest = enemy.transform;
            }
        }
        
        return nearest;
    }
    
    void StartReload()
    {
        isReloading = true;
        Invoke("FinishReload", reloadTime);
    }
    
    void FinishReload()
    {
        currentAmmo = maxAmmo;
        isReloading = false;
    }
    
    public bool IsReloading()
    {
        return isReloading;
    }
    
    public int GetCurrentAmmo()
    {
        return currentAmmo;
    }
}
