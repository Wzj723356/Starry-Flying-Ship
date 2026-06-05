using UnityEngine;

public class Projectile : MonoBehaviour
{
    private float damage;
    private float speed;
    private float range;
    private GameObject owner;
    private float traveledDistance;
    
    public void Initialize(float damage, float speed, float range, GameObject owner)
    {
        this.damage = damage;
        this.speed = speed;
        this.range = range;
        this.owner = owner;
        traveledDistance = 0f;
    }
    
    void Update()
    {
        Vector3 movement = transform.forward * speed * Time.deltaTime;
        traveledDistance += movement.magnitude;
        
        if (traveledDistance >= range)
        {
            Destroy(gameObject);
            return;
        }
        
        transform.Translate(movement);
        
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, movement.magnitude))
        {
            OnHit(hit);
        }
    }
    
    void OnHit(RaycastHit hit)
    {
        Damageable target = hit.collider.GetComponent<Damageable>();
        
        if (target != null && target.gameObject != owner)
        {
            target.TakeDamage(damage);
        }
        
        Destroy(gameObject);
    }
}
