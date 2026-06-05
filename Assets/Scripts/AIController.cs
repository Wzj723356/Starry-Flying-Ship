using UnityEngine;

public class AIController : MonoBehaviour
{
    [Header("AI参数")]
    public float reactionTime = 0.5f;
    public float attackRange = 1500f;
    public float pursuitRange = 2000f;
    public float evadeRange = 500f;
    
    [Header("操控参数")]
    public float maxTurnRate = 90f;
    public float maxClimbRate = 30f;
    
    private FlightController flightController;
    private WeaponSystem weaponSystem;
    private Transform target;
    private float lastReactionTime;
    
    void Awake()
    {
        flightController = GetComponent<FlightController>();
        weaponSystem = GetComponent<WeaponSystem>();
    }
    
    void Update()
    {
        if (Time.time - lastReactionTime > reactionTime)
        {
            UpdateTarget();
            lastReactionTime = Time.time;
        }
        
        if (target != null)
        {
            ControlAircraft();
        }
        else
        {
            Patrol();
        }
    }
    
    void UpdateTarget()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        
        if (players.Length == 0)
        {
            target = null;
            return;
        }
        
        float closestDistance = Mathf.Infinity;
        Transform closestPlayer = null;
        
        foreach (var player in players)
        {
            float distance = Vector3.Distance(transform.position, player.transform.position);
            
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestPlayer = player.transform;
            }
        }
        
        target = closestPlayer;
    }
    
    void ControlAircraft()
    {
        float distance = Vector3.Distance(transform.position, target.position);
        
        if (distance < evadeRange)
        {
            Evade();
        }
        else if (distance < attackRange)
        {
            Attack();
        }
        else if (distance < pursuitRange)
        {
            Pursuit();
        }
        else
        {
            Patrol();
        }
    }
    
    void Attack()
    {
        Vector3 targetDirection = (target.position - transform.position).normalized;
        Vector3 localTarget = transform.InverseTransformDirection(targetDirection);
        
        float pitch = Mathf.Clamp(localTarget.y * 2f, -1f, 1f);
        float yaw = Mathf.Clamp(localTarget.x * 2f, -1f, 1f);
        
        ApplyControl(pitch, yaw);
        weaponSystem.FirePrimaryWeapons();
    }
    
    void Pursuit()
    {
        Vector3 targetDirection = (target.position - transform.position).normalized;
        Vector3 localTarget = transform.InverseTransformDirection(targetDirection);
        
        float pitch = Mathf.Clamp(localTarget.y * 1.5f, -1f, 1f);
        float yaw = Mathf.Clamp(localTarget.x * 1.5f, -1f, 1f);
        
        ApplyControl(pitch, yaw);
    }
    
    void Evade()
    {
        Vector3 targetDirection = (transform.position - target.position).normalized;
        Vector3 localTarget = transform.InverseTransformDirection(targetDirection);
        
        float pitch = Mathf.Clamp(localTarget.y * 2f, -1f, 1f);
        float yaw = Mathf.Clamp(localTarget.x * 2f, -1f, 1f);
        
        ApplyControl(pitch, yaw);
    }
    
    void Patrol()
    {
        float time = Time.time * 0.5f;
        float pitch = Mathf.Sin(time) * 0.3f;
        float yaw = Mathf.Cos(time * 0.7f) * 0.2f;
        
        ApplyControl(pitch, yaw);
    }
    
    void ApplyControl(float pitch, float yaw)
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        
        float pitchTorque = pitch * maxTurnRate * Time.deltaTime;
        float yawTorque = yaw * maxTurnRate * Time.deltaTime;
        
        rb.AddTorque(transform.right * pitchTorque);
        rb.AddTorque(transform.up * yawTorque);
    }
}
