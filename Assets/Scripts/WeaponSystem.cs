using UnityEngine;

public class WeaponSystem : MonoBehaviour
{
    [Header("武器配置")]
    public Weapon[] primaryWeapons;
    public Weapon[] secondaryWeapons;
    
    [Header("射击参数")]
    public float fireRate = 0.1f;
    public float secondaryFireRate = 1.5f;
    
    private float primaryFireCooldown = 0f;
    private float secondaryFireCooldown = 0f;
    private InputManager input;
    
    void Awake()
    {
        input = GetComponent<InputManager>();
        
        if (input == null)
            input = gameObject.AddComponent<InputManager>();
        
        InitializeWeapons();
    }
    
    void InitializeWeapons()
    {
        foreach (var weapon in primaryWeapons)
        {
            if (weapon != null)
            {
                weapon.Initialize(transform);
            }
        }
        
        foreach (var weapon in secondaryWeapons)
        {
            if (weapon != null)
            {
                weapon.Initialize(transform);
            }
        }
    }
    
    void Update()
    {
        primaryFireCooldown -= Time.deltaTime;
        secondaryFireCooldown -= Time.deltaTime;
        
        if (input.Fire && primaryFireCooldown <= 0f)
        {
            FirePrimaryWeapons();
            primaryFireCooldown = fireRate;
        }
        
        if (input.SecondaryFire && secondaryFireCooldown <= 0f)
        {
            FireSecondaryWeapons();
            secondaryFireCooldown = secondaryFireRate;
        }
    }
    
    void FirePrimaryWeapons()
    {
        foreach (var weapon in primaryWeapons)
        {
            if (weapon != null && weapon.HasAmmo)
            {
                weapon.Fire();
            }
        }
    }
    
    void FireSecondaryWeapons()
    {
        foreach (var weapon in secondaryWeapons)
        {
            if (weapon != null && weapon.HasAmmo)
            {
                weapon.Fire();
            }
        }
    }
    
    public int GetPrimaryAmmo()
    {
        int total = 0;
        foreach (var weapon in primaryWeapons)
        {
            if (weapon != null)
                total += weapon.currentAmmo;
        }
        return total;
    }
    
    public int GetSecondaryAmmo()
    {
        int total = 0;
        foreach (var weapon in secondaryWeapons)
        {
            if (weapon != null)
                total += weapon.currentAmmo;
        }
        return total;
    }
}

[System.Serializable]
public class Weapon
{
    public string weaponName;
    public WeaponType weaponType;
    public int maxAmmo = 100;
    public int currentAmmo = 100;
    public float damage = 20f;
    public float projectileSpeed = 1000f;
    public float range = 2000f;
    public Transform muzzlePosition;
    public GameObject projectilePrefab;
    
    [HideInInspector] public Transform ownerTransform;
    
    public bool HasAmmo => currentAmmo > 0;
    
    public void Initialize(Transform owner)
    {
        ownerTransform = owner;
    }
    
    public void Fire()
    {
        if (!HasAmmo || muzzlePosition == null) return;
        
        currentAmmo--;
        
        GameObject projectile = Instantiate(projectilePrefab, muzzlePosition.position, muzzlePosition.rotation);
        Projectile proj = projectile.GetComponent<Projectile>();
        
        if (proj != null)
        {
            proj.Initialize(damage, projectileSpeed, range, ownerTransform.gameObject);
        }
        else
        {
            Rigidbody rb = projectile.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.velocity = muzzlePosition.forward * projectileSpeed;
            }
            Destroy(projectile, range / projectileSpeed);
        }
    }
}

public enum WeaponType
{
    MachineGun,
    Cannon,
    Rocket,
    Missile,
    Bomb
}
