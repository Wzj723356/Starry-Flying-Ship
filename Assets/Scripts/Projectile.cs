using UnityEngine;

public class Projectile : MonoBehaviour
{
    private float damage;
    private float speed;
    private float range;
    private GameObject owner;
    private float traveledDistance;
    private GameObject target;
    private bool isHoming = false;
    
    public void Initialize(float damage, float speed, float range, GameObject owner)
    {
        this.damage = damage;
        this.speed = speed;
        this.range = range;
        this.owner = owner;
        traveledDistance = 0f;
    }
    
    public void SetTarget(GameObject newTarget)
    {
        target = newTarget;
        isHoming = true;
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
        
        if (isHoming && target != null)
        {
            Vector3 toTarget = target.transform.position - transform.position;
            Quaternion targetRotation = Quaternion.LookRotation(toTarget);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * 2f);
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
        Destroy(gameObject);
    }
}