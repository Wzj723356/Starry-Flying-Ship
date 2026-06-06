using UnityEngine;

public class WeaponSystem : MonoBehaviour
{
    [Header("武器配置")]
    public GameObject missilePrefab;
    public GameObject laserPrefab;
    public Transform[] weaponMounts;
    
    [Header("导弹参数")]
    public int missileCount = 10;
    public float missileCooldown = 1f;
    private float lastMissileTime = 0f;
    
    [Header("激光参数")]
    public float laserDamage = 30f;
    public float laserRange = 500f;
    public float laserCooldown = 0.5f;
    private float lastLaserTime = 0f;
    
    [Header("干扰弹")]
    public int countermeasureCount = 8;
    public GameObject countermeasurePrefab;
    
    void Update()
    {
        HandleInput();
    }
    
    void HandleInput()
    {
        // 导弹发射
        if (Input.GetKeyDown(KeyCode.Space) && CanFireMissile())
        {
            FireMissile();
        }
        
        // 激光炮
        if (Input.GetMouseButton(0) && CanFireLaser())
        {
            FireLaser();
        }
        
        // 干扰弹
        if (Input.GetKeyDown(KeyCode.C) && countermeasureCount > 0)
        {
            DeployCountermeasure();
        }
    }
    
    bool CanFireMissile()
    {
        return missileCount > 0 && Time.time - lastMissileTime > missileCooldown;
    }
    
    bool CanFireLaser()
    {
        return Time.time - lastLaserTime > laserCooldown;
    }
    
    void FireMissile()
    {
        if (missilePrefab == null || weaponMounts.Length == 0) return;
        
        Transform mount = weaponMounts[0];
        GameObject missile = Instantiate(missilePrefab, mount.position, mount.rotation);
        
        Projectile proj = missile.GetComponent<Projectile>();
        if (proj != null)
        {
            proj.Initialize(50f, 200f, 1000f, gameObject);
            
            // 自动锁定最近目标
            GameObject target = FindNearestTarget();
            if (target != null)
                proj.SetTarget(target);
        }
        
        missileCount--;
        lastMissileTime = Time.time;
        
        Debug.Log($"导弹发射！剩余: {missileCount}");
    }
    
    void FireLaser()
    {
        RaycastHit hit;
        Vector3 origin = transform.position;
        Vector3 direction = transform.forward;
        
        if (Physics.Raycast(origin, direction, out hit, laserRange))
        {
            Damageable target = hit.collider.GetComponent<Damageable>();
            if (target != null)
            {
                target.TakeDamage(laserDamage);
                Debug.Log($"激光命中！造成 {laserDamage} 伤害");
            }
            
            // 激光视觉效果
            Debug.DrawRay(origin, direction * hit.distance, Color.red, 0.1f);
        }
        
        lastLaserTime = Time.time;
    }
    
    void DeployCountermeasure()
    {
        if (countermeasurePrefab != null)
        {
            Instantiate(countermeasurePrefab, transform.position - transform.forward * 5f, Quaternion.identity);
        }
        
        countermeasureCount--;
        Debug.Log($"干扰弹投放！剩余: {countermeasureCount}");
    }
    
    GameObject FindNearestTarget()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        GameObject nearest = null;
        float minDist = Mathf.Infinity;
        
        foreach (GameObject enemy in enemies)
        {
            float dist = Vector3.Distance(transform.position, enemy.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = enemy;
            }
        }
        
        return nearest;
    }
    
    public int GetMissileCount() => missileCount;
    public int GetCountermeasureCount() => countermeasureCount;
}