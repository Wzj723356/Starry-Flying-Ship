using UnityEngine;
using System.Collections.Generic;

public class EnemyAIController : MonoBehaviour
{
    [Header("=== AI行为配置 ===")]
    public AIState currentState = AIState.Patrol;
    public float detectionRange = 5000f;      // 检测范围
    public float attackRange = 2000f;         // 攻击范围
    public float disengageRange = 3000f;      // 脱离范围
    public float patrolRadius = 10000f;       // 巡逻半径
    
    [Header("=== 飞行参数 ===")]
    public float patrolSpeed = 200f;
    public float combatSpeed = 350f;
    public float evadingSpeed = 450f;
    public float pitchRate = 45f;
    public float rollRate = 90f;
    public float yawRate = 30f;
    
    [Header("=== 武器配置 ===")]
    public float missileFireRate = 3f;
    public float laserFireRate = 10f;
    public float missileDamage = 150f;
    public float laserDamage = 50f;
    public bool hasLaserCannon = true;
    public bool hasMissiles = true;
    
    [Header("=== 规避机动 ===")]
    public bool useEvasionManeuvers = true;
    public float evasionInterval = 2f;
    public float evasionStrength = 0.8f;
    
    [Header("=== 干扰弹 ===")]
    public int chaffCount = 20;
    public int flareCount = 20;
    public float countermeasuresInterval = 3f;
    
    [Header("=== 组件引用 ===")]
    public Rigidbody rb;
    public Damageable damageable;
    public Transform player;
    private float lastMissileTime;
    private float lastLaserTime;
    private float lastEvasionTime;
    private float lastCountermeasureTime;
    private Vector3 patrolCenter;
    private Vector3 evadeDirection;
    
    // 状态机
    public enum AIState
    {
        Patrol,     // 巡逻
        Pursue,    // 追击
        Combat,    // 战斗
        Evade,     // 规避
        Retreat,   // 撤退
        Dead       // 死亡
    }
    
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
            rb.mass = 5000f;
            rb.drag = 0.1f;
            rb.useGravity = false;
        }
        
        damageable = GetComponent<Damageable>();
        patrolCenter = transform.position;
    }
    
    void Start()
    {
        // 寻找玩家
        var playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
    }
    
    void Update()
    {
        if (currentState == AIState.Dead) return;
        
        // 检查生命值
        if (damageable != null && damageable.HealthPercentage <= 0f)
        {
            currentState = AIState.Dead;
            OnDeath();
            return;
        }
        
        // 更新AI状态
        UpdateAIState();
        
        // 根据状态执行行为
        ExecuteCurrentState();
    }
    
    void UpdateAIState()
    {
        if (player == null)
        {
            currentState = AIState.Patrol;
            return;
        }
        
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        float healthRatio = damageable != null ? damageable.HealthPercentage : 1f;
        
        // 状态转换逻辑
        if (healthRatio < 0.2f)
        {
            currentState = AIState.Retreat;
        }
        else if (distanceToPlayer > disengageRange)
        {
            if (distanceToPlayer < detectionRange)
                currentState = AIState.Pursue;
            else
                currentState = AIState.Patrol;
        }
        else if (distanceToPlayer < attackRange)
        {
            // 受伤时进入规避
            if (useEvasionManeuvers && Time.time - lastEvasionTime > evasionInterval)
            {
                if (damageable != null && damageable.HealthPercentage < 0.5f)
                {
                    currentState = AIState.Evade;
                    return;
                }
            }
            currentState = AIState.Combat;
        }
        else if (distanceToPlayer < detectionRange)
        {
            currentState = AIState.Pursue;
        }
        else
        {
            currentState = AIState.Patrol;
        }
    }
    
    void ExecuteCurrentState()
    {
        switch (currentState)
        {
            case AIState.Patrol:
                ExecutePatrol();
                break;
            case AIState.Pursue:
                ExecutePursue();
                break;
            case AIState.Combat:
                ExecuteCombat();
                break;
            case AIState.Evade:
                ExecuteEvade();
                break;
            case AIState.Retreat:
                ExecuteRetreat();
                break;
        }
    }
    
    // ===== 巡逻行为 =====
    void ExecutePatrol()
    {
        Vector3 targetPos = patrolCenter + Random.insideUnitSphere * patrolRadius;
        targetPos.y = Mathf.Clamp(targetPos.y, 500f, 10000f);
        
        FlyTowards(targetPos, patrolSpeed);
        
        // 周期性改变巡逻目标
        if (Vector3.Distance(transform.position, targetPos) < 500f)
        {
            patrolCenter = transform.position;
        }
    }
    
    // ===== 追击行为 =====
    void ExecutePursue()
    {
        if (player == null) return;
        
        // 预测玩家位置
        var playerRB = player.GetComponent<Rigidbody>();
        Vector3 predictedPos = player.position;
        if (playerRB != null)
        {
            predictedPos += playerRB.velocity * Time.deltaTime * 2f;
        }
        
        FlyTowards(predictedPos, combatSpeed);
        
        // 接近后转入战斗
        if (Vector3.Distance(transform.position, player.position) < attackRange)
        {
            currentState = AIState.Combat;
        }
    }
    
    // ===== 战斗行为 =====
    void ExecuteCombat()
    {
        if (player == null) return;
        
        // 保持最佳交战距离
        Vector3 toPlayer = player.position - transform.position;
        float distance = toPlayer.magnitude;
        
        Vector3 targetPos;
        
        if (distance < 800f)
        {
            // 太近，后撤
            targetPos = transform.position - toPlayer.normalized * 500f;
        }
        else if (distance > 1500f)
        {
            // 太远，逼近
            targetPos = player.position;
        }
        else
        {
            // 保持距离，绕圈
            Vector3 circleOffset = Vector3.Cross(toPlayer.normalized, Vector3.up).normalized * 500f;
            targetPos = player.position + circleOffset;
        }
        
        FlyTowards(targetPos, combatSpeed);
        
        // 始终朝向玩家
        LookAtTarget(player.position);
        
        // 开火
        TryFireWeapons();
        
        // 发射干扰弹
        TryUseCountermeasures();
    }
    
    // ===== 规避行为 =====
    void ExecuteEvade()
    {
        lastEvasionTime = Time.time;
        
        // 随机规避方向
        evadeDirection = Random.insideUnitSphere.normalized;
        evadeDirection.y = Mathf.Abs(evadeDirection.y); // 保持向上偏移
        
        // 执行规避机动
        FlyTowards(transform.position + evadeDirection * 500f, evadingSpeed);
        
        // 使用干扰弹
        LaunchCountermeasures();
        
        // 2秒后返回战斗
        if (Time.time - lastEvasionTime > 2f)
        {
            currentState = AIState.Combat;
        }
    }
    
    // ===== 撤退行为 =====
    void ExecuteRetreat()
    {
        // 向后飞行
        Vector3 retreatDir = -transform.forward;
        Vector3 targetPos = transform.position + retreatDir * 1000f;
        
        FlyTowards(targetPos, evadingSpeed);
        
        // 使用干扰弹
        LaunchCountermeasures();
    }
    
    // ===== 飞行控制 =====
    void FlyTowards(Vector3 target, float speed)
    {
        Vector3 direction = (target - transform.position).normalized;
        
        // 计算目标旋转
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, yawRate * Time.deltaTime);
        
        // 应用推力
        Vector3 forwardForce = transform.forward * speed * 10f;
        rb.AddForce(forwardForce - rb.velocity * 0.5f, ForceMode.Force);
        
        // 限制速度
        rb.velocity = Vector3.ClampMagnitude(rb.velocity, speed);
    }
    
    void LookAtTarget(Vector3 targetPos)
    {
        Vector3 direction = (targetPos - transform.position).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, yawRate * Time.deltaTime);
    }
    
    // ===== 武器系统 =====
    void TryFireWeapons()
    {
        if (player == null) return;
        
        // 检查是否在射程内
        float distance = Vector3.Distance(transform.position, player.position);
        if (distance > attackRange) return;
        
        // 检查角度
        Vector3 toPlayer = (player.position - transform.position).normalized;
        float angle = Vector3.Angle(transform.forward, toPlayer);
        if (angle > 15f) return; // 角度太大不打
        
        // 导弹攻击
        if (hasMissiles && Time.time - lastMissileTime > missileFireRate)
        {
            FireMissile();
            lastMissileTime = Time.time;
        }
        
        // 激光攻击
        if (hasLaserCannon && Time.time - lastLaserTime > 1f / laserFireRate)
        {
            FireLaser();
            lastLaserTime = Time.time;
        }
    }
    
    void FireMissile()
    {
        if (player == null) return;
        
        // 生成导弹
        GameObject missile = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        missile.transform.position = transform.position + transform.forward * 10f;
        missile.transform.rotation = transform.rotation;
        missile.name = "EnemyMissile";
        
        var missileRB = missile.AddComponent<Rigidbody>();
        missileRB.mass = 10f;
        missileRB.drag = 0.5f;
        
        var missileLogic = missile.AddComponent<EnemyMissile>();
        missileLogic.target = player;
        missileLogic.damage = missileDamage;
        missileLogic.launcher = transform;
        
        // 初始速度
        missileRB.velocity = rb.velocity + transform.forward * 100f;
        
        // 碰撞检测
        var collider = missile.GetComponent<SphereCollider>();
        collider.radius = 0.5f;
        
        // 3秒后销毁
        Destroy(missile, 10f);
    }
    
    void FireLaser()
    {
        if (player == null) return;
        
        // 射线检测
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit, attackRange))
        {
            var playerDamageable = hit.collider.GetComponent<Damageable>();
            if (playerDamageable != null)
            {
                playerDamageable.TakeDamage(laserDamage * Time.deltaTime, Damageable.DamageType.LaserCannon);
            }
        }
    }
    
    // ===== 干扰弹 =====
    void TryUseCountermeasures()
    {
        if (Time.time - lastCountermeasureTime < countermeasuresInterval) return;
        
        // 检测是否有来袭导弹
        var missiles = FindObjectsOfType<EnemyMissile>();
        foreach (var missile in missiles)
        {
            if (missile != null && missile.launcher != transform)
            {
                float distance = Vector3.Distance(missile.transform.position, transform.position);
                if (distance < 500f)
                {
                    LaunchCountermeasures();
                    lastCountermeasureTime = Time.time;
                    break;
                }
            }
        }
    }
    
    void LaunchCountermeasures()
    {
        if (chaffCount > 0)
        {
            chaffCount--;
            // 生成干扰弹效果
            SpawnCountermeasureEffect("Chaff");
        }
        
        if (flareCount > 0)
        {
            flareCount--;
            SpawnCountermeasureEffect("Flare");
        }
    }
    
    void SpawnCountermeasureEffect(string type)
    {
        GameObject effect = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        effect.transform.position = transform.position - transform.forward * 5f + Random.insideUnitSphere * 2f;
        effect.transform.localScale = Vector3.one * 0.5f;
        effect.name = type;
        
        var renderer = effect.GetComponent<Renderer>();
        renderer.material = new Material(Shader.Find("Standard"));
        renderer.material.color = type == "Chaff" ? Color.white : Color.yellow;
        
        Destroy(effect, 3f);
    }
    
    // ===== 死亡 =====
    void OnDeath()
    {
        // 爆炸效果
        GameObject explosion = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        explosion.transform.position = transform.position;
        explosion.transform.localScale = Vector3.one * 20f;
        
        var renderer = explosion.GetComponent<Renderer>();
        renderer.material = new Material(Shader.Find("Standard"));
        renderer.material.color = Color.red;
        renderer.material.EnableKeyword("_EMISSION");
        renderer.material.SetColor("_EmissionColor", Color.red * 2f);
        
        Destroy(explosion, 2f);
        Destroy(gameObject, 0.1f);
        
        // 通知任务系统
        var missionManager = FindObjectOfType<MissionManager>();
        if (missionManager != null)
        {
            missionManager.OnEnemyDestroyed(gameObject);
        }
    }
}

// ===== 敌方导弹 =====
public class EnemyMissile : MonoBehaviour
{
    public Transform target;
    public float damage = 150f;
    public Transform launcher;
    public float speed = 300f;
    public float turnRate = 180f;
    public float lifetime = 10f;
    
    private Rigidbody rb;
    private bool isChaffed = false;
    
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
        rb.useGravity = false;
        rb.drag = 1f;
    }
    
    void Update()
    {
        lifetime -= Time.deltaTime;
        if (lifetime <= 0f)
        {
            Destroy(gameObject);
            return;
        }
        
        // 检测干扰弹
        if (!isChaffed)
        {
            var countermeasure = FindObjectOfType<CountermeasureSystem>();
            // 简化的诱骗检测
        }
        
        // 追踪目标
        if (target != null && !isChaffed)
        {
            Vector3 direction = (target.position - transform.position).normalized;
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, turnRate * Time.deltaTime);
            
            rb.velocity = transform.forward * speed;
        }
        else
        {
            // 无目标时直线飞行
            rb.velocity = transform.forward * speed;
        }
    }
    
    void OnCollisionEnter(Collision collision)
    {
        // 造成伤害
        var damageable = collision.collider.GetComponent<Damageable>();
        if (damageable != null)
        {
            damageable.TakeDamage(damage, Damageable.DamageType.Missile);
        }
        
        // 爆炸效果
        GameObject explosion = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        explosion.transform.position = transform.position;
        explosion.transform.localScale = Vector3.one * 5f;
        
        var renderer = explosion.GetComponent<Renderer>();
        renderer.material = new Material(Shader.Find("Standard"));
        renderer.material.color = Color.red;
        
        Destroy(explosion, 1f);
        Destroy(gameObject);
    }
}
