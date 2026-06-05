using UnityEngine;
using System.Collections.Generic;

public class TargetingSystem : MonoBehaviour
{
    [Header("=== 锁定系统配置 ===")]
    public float lockOnRange = 3000f;           // 锁定范围
    public float lockOnAngle = 60f;              // 锁定角度（视野内）
    public float lockOnSpeed = 2f;                // 锁定速度
    public float lockOnAngleTolerance = 10f;    // 锁定角度容差
    public int maxLockedTargets = 1;             // 最大锁定目标数
    public float radarRange = 5000f;             // 雷达范围
    
    [Header("=== 导弹锁定 ===")]
    public float missileLockOnTime = 1.5f;      // 导弹锁定时间
    public float missileLockOnAngle = 30f;       // 导弹锁定角度
    public int maxMissileLocks = 4;             // 最大导弹锁定数
    
    [Header("=== 视觉特效 ===")]
    public Color lockBoxColor = Color.red;
    public Color lockedColor = Color.yellow;
    public Color radarFriendlyColor = Color.green;
    public Color radarEnemyColor = Color.red;
    public Color radarNeutralColor = Color.white;
    public float lockBoxSize = 80f;
    
    [Header("=== 组件引用 ===")]
    public Camera mainCamera;
    public Transform missileLauncher;
    
    private HUDManager hud;
    private List<TargetInfo> availableTargets = new List<TargetInfo>();
    private TargetInfo currentTarget;
    private float currentLockProgress = 0f;
    private bool isLocking = false;
    private List<TargetInfo> missileLocks = new List<TargetInfo>();
    
    [System.Serializable]
    public class TargetInfo
    {
        public GameObject target;
        public Transform targetTransform;
        public Vector3 screenPosition;
        public float distance;
        public float lockProgress;       // 0-1 锁定进度
        public bool isLocked;           // 是否已锁定
        public bool isMissileLocked;    // 导弹是否锁定
        public TargetType type;
        public float threatLevel;        // 威胁等级
        
        public enum TargetType
        {
            Enemy,
            Friendly,
            Neutral,
            Structure,
            Missile
        }
    }
    
    void Awake()
    {
        hud = FindObjectOfType<HUDManager>();
        
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
    }
    
    void Update()
    {
        // 扫描目标
        ScanForTargets();
        
        // 锁定控制
        HandleLockInput();
        
        // 更新目标信息
        UpdateTargetInfo();
        
        // 发射导弹
        HandleMissileLaunch();
    }
    
    void ScanForTargets()
    {
        availableTargets.Clear();
        
        // 扫描敌人
        var enemies = FindObjectsOfType<EnemyAIController>();
        foreach (var enemy in enemies)
        {
            if (enemy == null) continue;
            
            float distance = Vector3.Distance(transform.position, enemy.transform.position);
            if (distance > radarRange) continue;
            
            // 检查角度
            Vector3 toTarget = enemy.transform.position - transform.position;
            float angle = Vector3.Angle(transform.forward, toTarget);
            if (angle > lockOnAngle) continue;
            
            var targetInfo = new TargetInfo
            {
                target = enemy.gameObject,
                targetTransform = enemy.transform,
                distance = distance,
                type = TargetInfo.TargetType.Enemy,
                threatLevel = CalculateThreatLevel(enemy)
            };
            
            availableTargets.Add(targetInfo);
        }
        
        // 按威胁等级排序
        availableTargets.Sort((a, b) => b.threatLevel.CompareTo(a.threatLevel));
    }
    
    float CalculateThreatLevel(EnemyAIController enemy)
    {
        float threat = 0f;
        
        // 距离越近威胁越高
        float distanceFactor = 1f - (Vector3.Distance(transform.position, enemy.transform.position) / radarRange);
        threat += distanceFactor * 30f;
        
        // 正在攻击的威胁更高
        if (enemy.currentState == EnemyAIController.AIState.Combat)
        {
            threat += 50f;
        }
        else if (enemy.currentState == EnemyAIController.AIState.Pursue)
        {
            threat += 30f;
        }
        
        // 有导弹的威胁更高
        if (enemy.hasMissiles)
        {
            threat += 20f;
        }
        
        return threat;
    }
    
    void HandleLockInput()
    {
        // Tab键切换目标
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            CycleTarget();
        }
        
        // 鼠标中键快速锁定最近目标
        if (Input.GetMouseButtonDown(2))
        {
            LockNearestTarget();
        }
        
        // 按住左键进行锁定
        if (Input.GetMouseButton(0))
        {
            StartLocking();
        }
        else
        {
            CancelLocking();
        }
    }
    
    void CycleTarget()
    {
        if (availableTargets.Count == 0) return;
        
        int currentIndex = -1;
        if (currentTarget != null)
        {
            currentIndex = availableTargets.IndexOf(currentTarget);
        }
        
        currentIndex = (currentIndex + 1) % availableTargets.Count;
        currentTarget = availableTargets[currentIndex];
        currentLockProgress = 0f;
    }
    
    void LockNearestTarget()
    {
        if (availableTargets.Count == 0) return;
        
        currentTarget = availableTargets[0];
        currentLockProgress = 1f;
        currentTarget.isLocked = true;
        currentTarget.lockProgress = 1f;
    }
    
    void StartLocking()
    {
        if (currentTarget == null && availableTargets.Count > 0)
        {
            currentTarget = availableTargets[0];
        }
        
        if (currentTarget == null) return;
        
        // 检查是否在锁定角度内
        Vector3 toTarget = currentTarget.targetTransform.position - transform.position;
        float angle = Vector3.Angle(transform.forward, toTarget);
        
        if (angle <= lockOnAngleTolerance)
        {
            isLocking = true;
            currentLockProgress += (1f / lockOnSpeed) * Time.deltaTime;
            currentLockProgress = Mathf.Clamp01(currentLockProgress);
            
            currentTarget.lockProgress = currentLockProgress;
            
            if (currentLockProgress >= 1f)
            {
                currentTarget.isLocked = true;
                isLocking = false;
                
                // 锁定音效/特效
                OnTargetLocked(currentTarget);
            }
        }
        else
        {
            // 目标偏离视线，锁定进度下降
            currentLockProgress -= Time.deltaTime * 0.5f;
            currentLockProgress = Mathf.Max(0f, currentLockProgress);
        }
    }
    
    void CancelLocking()
    {
        isLocking = false;
        
        // 未完成的锁定进度下降
        if (currentTarget != null && !currentTarget.isLocked)
        {
            currentLockProgress -= Time.deltaTime * 0.3f;
            currentLockProgress = Mathf.Max(0f, currentLockProgress);
            currentTarget.lockProgress = currentLockProgress;
        }
    }
    
    void OnTargetLocked(TargetInfo target)
    {
        Debug.Log($"锁定目标: {target.target.name}");
        
        // HUD更新
        if (hud != null)
        {
            hud.SetTarget($"已锁定: {target.target.name}");
        }
    }
    
    void UpdateTargetInfo()
    {
        foreach (var target in availableTargets)
        {
            if (target.targetTransform == null)
            {
                target.target = null;
                continue;
            }
            
            // 计算屏幕位置
            Vector3 screenPos = Vector3.zero;
            if (mainCamera != null)
            {
                screenPos = mainCamera.WorldToScreenPoint(target.targetTransform.position);
            }
            
            target.screenPosition = screenPos;
            target.distance = Vector3.Distance(transform.position, target.targetTransform.position);
            
            // 丢失锁定检测
            if (target.isLocked)
            {
                Vector3 toTarget = target.targetTransform.position - transform.position;
                float angle = Vector3.Angle(transform.forward, toTarget);
                
                if (angle > lockOnAngle * 1.5f || target.distance > lockOnRange * 1.2f)
                {
                    // 丢失锁定
                    target.isLocked = false;
                    target.lockProgress = 0f;
                    
                    if (currentTarget == target)
                    {
                        currentTarget = null;
                    }
                }
            }
        }
        
        // 移除无效目标
        availableTargets.RemoveAll(t => t.target == null);
    }
    
    // ===== 导弹锁定 =====
    void HandleMissileLaunch()
    {
        // 鼠标滚轮切换导弹目标
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll > 0)
        {
            CycleMissileTarget();
        }
        else if (scroll < 0)
        {
            CycleMissileTargetReverse();
        }
        
        // 空格键发射已锁定的导弹
        if (Input.GetKeyDown(KeyCode.Space))
        {
            LaunchMissiles();
        }
    }
    
    void CycleMissileTarget()
    {
        if (availableTargets.Count == 0) return;
        
        int currentIndex = -1;
        if (missileLocks.Count > 0)
        {
            currentIndex = availableTargets.IndexOf(missileLocks[missileLocks.Count - 1]);
        }
        
        currentIndex = (currentIndex + 1) % availableTargets.Count;
        
        if (missileLocks.Count < maxMissileLocks)
        {
            missileLocks.Add(availableTargets[currentIndex]);
            availableTargets[currentIndex].isMissileLocked = true;
        }
    }
    
    void CycleMissileTargetReverse()
    {
        if (missileLocks.Count > 0)
        {
            var last = missileLocks[missileLocks.Count - 1];
            last.isMissileLocked = false;
            missileLocks.RemoveAt(missileLocks.Count - 1);
        }
    }
    
    void LaunchMissiles()
    {
        if (missileLocks.Count == 0) return;
        
        // 获取武器系统
        var weaponSystem = GetComponent<WeaponSystem>();
        if (weaponSystem == null)
        {
            weaponSystem = gameObject.AddComponent<WeaponSystem>();
        }
        
        foreach (var target in missileLocks)
        {
            if (target.target != null)
            {
                weaponSystem.FireMissile(target.target);
                Debug.Log($"发射导弹: {target.target.name}");
            }
        }
        
        missileLocks.Clear();
    }
    
    // ===== 公共方法 =====
    public TargetInfo GetCurrentTarget()
    {
        return currentTarget;
    }
    
    public List<TargetInfo> GetAvailableTargets()
    {
        return availableTargets;
    }
    
    public int GetMissileLockCount()
    {
        return missileLocks.Count;
    }
    
    public bool IsTargetLocked()
    {
        return currentTarget != null && currentTarget.isLocked;
    }
    
    public float GetLockProgress()
    {
        return currentLockProgress;
    }
    
    // 获取HUD显示用的目标列表
    public List<TargetInfo> GetRadarTargets()
    {
        return availableTargets;
    }
}

// ===== 雷达显示组件 =====
public class RadarDisplay : MonoBehaviour
{
    public TargetingSystem targetingSystem;
    public RectTransform radarRect;
    public float radarSize = 150f;
    public float radarRange = 5000f;
    public Color playerColor = Color.cyan;
    public Color enemyColor = Color.red;
    public Color friendlyColor = Color.green;
    public float blipSize = 10f;
    
    void Update()
    {
        if (targetingSystem == null)
        {
            targetingSystem = FindObjectOfType<TargetingSystem>();
        }
    }
    
    // 在OnGUI或使用Unity UI绘制雷达
    void OnGUI()
    {
        if (radarRect == null) return;
        
        // 绘制雷达背景
        Rect radarRectScreen = new Rect(
            radarRect.position.x,
            radarRect.position.y,
            radarSize * 2,
            radarSize * 2
        );
        
        // 绘制玩家位置（中心点）
        GUI.color = playerColor;
        GUI.DrawTexture(new Rect(radarRectScreen.x + radarSize - 5, radarRectScreen.y + radarSize - 5, 10, 10), Texture2D.whiteTexture);
        
        // 绘制目标
        if (targetingSystem != null)
        {
            foreach (var target in targetingSystem.GetRadarTargets())
            {
                Vector3 relativePos = target.targetTransform.position - Camera.main.transform.position;
                
                // 计算相对位置（简化的2D雷达）
                float x = relativePos.x / radarRange * radarSize;
                float y = relativePos.z / radarRange * radarSize;
                
                // 距离判断
                float distance = target.distance / radarRange;
                if (distance > 1f) continue;
                
                Rect blipRect = new Rect(
                    radarRectScreen.x + radarSize + x - blipSize / 2,
                    radarRectScreen.y + radarSize - y - blipSize / 2,
                    blipSize,
                    blipSize
                );
                
                // 颜色
                if (target.type == TargetingSystem.TargetInfo.TargetType.Enemy)
                {
                    GUI.color = enemyColor;
                    if (target.isLocked)
                    {
                        GUI.color = Color.yellow;
                    }
                }
                else if (target.type == TargetingSystem.TargetInfo.TargetType.Friendly)
                {
                    GUI.color = friendlyColor;
                }
                
                // 绘制
                GUI.DrawTexture(blipRect, Texture2D.whiteTexture);
            }
        }
        
        GUI.color = Color.white;
    }
}
