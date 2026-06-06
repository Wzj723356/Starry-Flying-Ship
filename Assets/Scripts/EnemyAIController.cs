using UnityEngine;
using System.Collections.Generic;

public class EnemyAIController : MonoBehaviour
{
    [Header("AI状态")]
    public AIState currentState = AIState.Patrol;
    
    [Header("巡逻参数")]
    public List<Transform> patrolPoints;
    public float patrolSpeed = 50f;
    private int currentPatrolIndex = 0;
    
    [Header("追击参数")]
    public float chaseSpeed = 100f;
    public float detectionRange = 200f;
    public float attackRange = 50f;
    
    [Header("战斗参数")]
    public float fireRate = 2f;
    public GameObject missilePrefab;
    private float lastFireTime = 0f;
    
    [Header("规避参数")]
    public float evadeDistance = 30f;
    
    private Transform target;
    private Rigidbody rb;
    
    public enum AIState
    {
        Patrol,
        Chase,
        Attack,
        Evade
    }
    
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
            rb = gameObject.AddComponent<Rigidbody>();
        
        FindTarget();
    }
    
    void FindTarget()
    {
        GameObject player = GameObject.Find("PlayerShip");
        if (player != null)
            target = player.transform;
    }
    
    void Update()
    {
        if (target == null)
        {
            FindTarget();
            return;
        }
        
        float distanceToTarget = Vector3.Distance(transform.position, target.position);
        
        // 状态机
        switch (currentState)
        {
            case AIState.Patrol:
                Patrol();
                if (distanceToTarget < detectionRange)
                    currentState = AIState.Chase;
                break;
                
            case AIState.Chase:
                Chase();
                if (distanceToTarget < attackRange)
                    currentState = AIState.Attack;
                else if (distanceToTarget > detectionRange * 1.5f)
                    currentState = AIState.Patrol;
                break;
                
            case AIState.Attack:
                Attack();
                if (distanceToTarget > attackRange * 1.5f)
                    currentState = AIState.Chase;
                break;
                
            case AIState.Evade:
                Evade();
                break;
        }
    }
    
    void Patrol()
    {
        if (patrolPoints.Count == 0) return;
        
        Transform targetPoint = patrolPoints[currentPatrolIndex];
        Vector3 direction = (targetPoint.position - transform.position).normalized;
        
        rb.velocity = direction * patrolSpeed;
        
        if (Vector3.Distance(transform.position, targetPoint.position) < 10f)
        {
            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Count;
        }
    }
    
    void Chase()
    {
        if (target == null) return;
        
        Vector3 direction = (target.position - transform.position).normalized;
        rb.velocity = direction * chaseSpeed;
        
        // 锁定玩家
        if (HUDManager.instance != null)
            HUDManager.instance.SetLockedByEnemy(true);
    }
    
    void Attack()
    {
        if (target == null) return;
        
        // 面向目标
        Vector3 direction = (target.position - transform.position).normalized;
        transform.rotation = Quaternion.LookRotation(direction);
        
        // 发射导弹
        if (Time.time - lastFireTime > fireRate)
        {
            FireMissile();
            lastFireTime = Time.time;
        }
    }
    
    void FireMissile()
    {
        if (missilePrefab != null)
        {
            GameObject missile = Instantiate(missilePrefab, transform.position, transform.rotation);
            Projectile proj = missile.GetComponent<Projectile>();
            if (proj != null)
            {
                proj.Initialize(25f, 150f, 500f, gameObject);
                proj.SetTarget(target.gameObject);
            }
        }
        
        Debug.Log("敌方发射导弹！");
    }
    
    void Evade()
    {
        // 39机动规避
        Vector3 evadeDirection = transform.right * Random.Range(-1f, 1f) + transform.up * Random.Range(0.5f, 1f);
        rb.velocity = evadeDirection.normalized * chaseSpeed;
    }
    
    public void TriggerEvade()
    {
        currentState = AIState.Evade;
        Invoke(nameof(ReturnToPatrol), 3f);
    }
    
    void ReturnToPatrol()
    {
        currentState = AIState.Patrol;
    }
}