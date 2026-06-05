using UnityEngine;

public class Damageable : MonoBehaviour
{
    [Header("生命值设置")]
    public float maxHealth = 1000f;
    public float currentHealth;
    
    [Header("伤害特效")]
    public GameObject hitEffectPrefab;
    public GameObject explosionEffectPrefab;
    
    [Header("音效")]
    public AudioClip hitSound;
    public AudioClip explosionSound;
    
    private AudioSource audioSource;
    
    void Awake()
    {
        currentHealth = maxHealth;
        audioSource = GetComponent<AudioSource>();
        
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }
    
    public void TakeDamage(float amount)
    {
        currentHealth = Mathf.Max(0f, currentHealth - amount);
        
        if (hitEffectPrefab != null)
        {
            Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
        }
        
        if (hitSound != null)
        {
            audioSource.PlayOneShot(hitSound);
        }
        
        if (currentHealth <= 0f)
        {
            OnDeath();
        }
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
        
        Destroy(gameObject);
    }
    
    public float HealthPercentage => currentHealth / maxHealth;
}
