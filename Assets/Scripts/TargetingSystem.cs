using UnityEngine;

public class TargetingSystem : MonoBehaviour
{
    [Header("雷达参数")]
    public float frontRadarRange = 100f;
    public float sideRadarRange = 60f;
    public float rearMissileDetection = 40f;
    
    [Header("目标信息")]
    public Transform lockedTarget;
    public System.Collections.Generic.List<Transform> detectedTargets = new System.Collections.Generic.List<Transform>();
    
    [Header("锁定参数")]
    public float lockTime = 2f;
    public float lockAngle = 30f;
    private float currentLockProgress = 0f;
    
    void Update()
    {
        ScanForTargets();
        UpdateLock();
        CheckIncomingMissiles();
    }
    
    void ScanForTargets()
    {
        detectedTargets.Clear();
        
        Collider[] hits = Physics.OverlapSphere(transform.position, frontRadarRange);
        
        foreach (Collider hit in hits)
        {
            if (hit.gameObject == gameObject) continue;
            
            Vector3 toTarget = hit.transform.position - transform.position;
            float angle = Vector3.Angle(transform.forward, toTarget);
            float distance = toTarget.magnitude;
            
            // 正前方雷达
            if (angle < 45f && distance <= frontRadarRange)
            {
                detectedTargets.Add(hit.transform);
            }
            // 侧面雷达
            else if (angle < 135f && distance <= sideRadarRange)
            {
                detectedTargets.Add(hit.transform);
            }
        }
    }
    
    void UpdateLock()
    {
        if (lockedTarget == null)
        {
            // 尝试锁定最近目标
            Transform nearest = GetNearestTarget();
            if (nearest != null)
            {
                Vector3 toTarget = nearest.position - transform.position;
                float angle = Vector3.Angle(transform.forward, toTarget);
                
                if (angle < lockAngle)
                {
                    currentLockProgress += Time.deltaTime;
                    
                    if (currentLockProgress >= lockTime)
                    {
                        lockedTarget = nearest;
                        Debug.Log($"目标锁定: {nearest.name}");
                    }
                }
                else
                {
                    currentLockProgress = 0f;
                }
            }
        }
        else
        {
            // 检查锁定是否丢失
            Vector3 toTarget = lockedTarget.position - transform.position;
            float distance = toTarget.magnitude;
            
            if (distance > frontRadarRange * 1.5f)
            {
                lockedTarget = null;
                currentLockProgress = 0f;
                Debug.Log("目标丢失");
            }
        }
    }
    
    void CheckIncomingMissiles()
    {
        Collider[] missiles = Physics.OverlapSphere(transform.position, rearMissileDetection);
        
        foreach (Collider missile in missiles)
        {
            if (missile.CompareTag("Missile"))
            {
                Vector3 toMissile = missile.transform.position - transform.position;
                float angle = Vector3.Angle(transform.forward, toMissile);
                
                // 后方导弹检测
                if (angle > 135f)
                {
                    Debug.Log($"后方导弹来袭！距离: {toMissile.magnitude:F0}m");
                }
            }
        }
    }
    
    Transform GetNearestTarget()
    {
        Transform nearest = null;
        float minDist = Mathf.Infinity;
        
        foreach (Transform target in detectedTargets)
        {
            float dist = Vector3.Distance(transform.position, target.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = target;
            }
        }
        
        return nearest;
    }
    
    public Transform GetLockedTarget() => lockedTarget;
    public float GetLockProgress() => currentLockProgress / lockTime;
}