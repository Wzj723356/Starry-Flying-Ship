using UnityEngine;

public class Damageable : MonoBehaviour
{
    [Header("生命值")]
    public float maxHealth = 100f;
    public float currentHealth;
    
    [Header("部件损坏")]
    public float engineHealth = 100f;
    public float wingHealth = 100f;
    public float bridgeHealth = 100f;
    
    [Header("损坏阈值")]
    public float criticalThreshold = 30f;
    public float destroyedThreshold = 0f;
    
    public event System.Action<float> OnDamageTaken;
    public event System.Action OnDestroyed;
    
    void Start()
    {
        currentHealth = maxHealth;
    }
    
    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0f);
        
        OnDamageTaken?.Invoke(damage);
        
        if (currentHealth <= 0f)
        {
            OnDestroyed?.Invoke();
            HandleDestruction();
        }
    }
    
    public void DamageEngine(float damage)
    {
        engineHealth -= damage;
        engineHealth = Mathf.Max(engineHealth, 0f);
        
        if (engineHealth <= destroyedThreshold)
        {
            Debug.Log("引擎已损坏！无法推进");
        }
    }
    
    public void DamageWing(float damage)
    {
        wingHealth -= damage;
        wingHealth = Mathf.Max(wingHealth, 0f);
        
        if (wingHealth <= destroyedThreshold)
        {
            Debug.Log("机翼已损坏！机动性降低");
        }
    }
    
    public void DamageBridge(float damage)
    {
        bridgeHealth -= damage;
        bridgeHealth = Mathf.Max(bridgeHealth, 0f);
        
        if (bridgeHealth <= destroyedThreshold)
        {
            Debug.Log("舰桥被击中！立即坠毁");
            TakeDamage(maxHealth);
        }
    }
    
    void HandleDestruction()
    {
        // 播放爆炸效果
        Debug.Log($"{gameObject.name} 已被摧毁");
        
        // 延迟销毁
        Destroy(gameObject, 2f);
    }
    
    public float GetHealthPercentage() => currentHealth / maxHealth;
    
    public bool IsCritical() => currentHealth <= criticalThreshold;
    
    public bool IsDestroyed() => currentHealth <= destroyedThreshold;
}