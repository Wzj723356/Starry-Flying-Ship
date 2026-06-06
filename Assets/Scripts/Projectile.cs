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
        
        // 设置标签
        gameObject.tag = "Missile";
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
        
        // 导弹追踪
        if (isHoming && target != null)
        {
            Vector3 toTarget = target.transform.position - transform.position;
            Quaternion targetRotation = Quaternion.LookRotation(toTarget);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * 3f);
        }
        
        transform.Translate(movement);
        
        // 碰撞检测
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, movement.magnitude))
        {
            OnHit(hit);
        }
    }
    
    void OnHit(RaycastHit hit)
    {
        // 检查是否击中目标
        if (hit.collider.gameObject != owner)
        {
            Damageable damageable = hit.collider.GetComponent<Damageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(damage);
                Debug.Log($"导弹命中 {hit.collider.name}，造成 {damage} 伤害");
            }
            
            // 通知任务系统
            if (MissionManager.instance != null)
            {
                MissionManager.instance.OnEnemyKilled();
            }
        }
        
        Destroy(gameObject);
    }
}