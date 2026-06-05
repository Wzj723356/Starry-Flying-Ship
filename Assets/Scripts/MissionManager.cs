using UnityEngine;
using System.Collections.Generic;

public class MissionManager : MonoBehaviour
{
    [Header("=== 任务配置 ===")]
    public List<Mission> missions = new List<Mission>();
    public int currentMissionIndex = 0;
    
    [Header("=== UI引用 ===")]
    public GameObject missionUIPrefab;
    public Transform missionUIParent;
    
    [Header("=== 敌人刷新 ===")]
    public int maxActiveEnemies = 5;
    public float spawnInterval = 30f;
    public List<Transform> spawnPoints = new List<Transform>();
    public GameObject enemyPrefab;
    
    private HUDManager hud;
    private int enemiesKilled;
    private float lastSpawnTime;
    
    [System.Serializable]
    public class Mission
    {
        public string missionName;
        public string description;
        public MissionType type;
        public int targetCount;           // 目标数量
        public int currentProgress;       // 当前进度
        public float timeLimit;           // 时间限制（秒），0为无限
        public Vector3 targetPosition;    // 目标位置
        public float radius;              // 目标区域半径
        public Reward reward;
        public bool isCompleted;
        public bool isFailed;
        
        [System.Serializable]
        public class Reward
        {
            public int experience = 100;
            public int credits = 500;
            public List<string> unlocks = new List<string>();
        }
        
        public enum MissionType
        {
            KillAll,      // 消灭所有敌人
            ReachPoint,   // 到达指定位置
            Survive,      // 生存指定时间
            Escort,       // 护送
            Destroy       // 摧毁目标
        }
    }
    
    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
    
    void Start()
    {
        hud = FindObjectOfType<HUDManager>();
        
        // 初始化任务
        if (missions.Count == 0)
        {
            InitializeDefaultMissions();
        }
        
        // 激活第一个任务
        if (missions.Count > 0)
        {
            ActivateMission(0);
        }
    }
    
    void Update()
    {
        // 检查任务状态
        if (currentMissionIndex >= 0 && currentMissionIndex < missions.Count)
        {
            var mission = missions[currentMissionIndex];
            
            if (!mission.isCompleted && !mission.isFailed)
            {
                UpdateMissionProgress(mission);
            }
        }
        
        // 敌人刷新
        UpdateEnemySpawn();
    }
    
    void InitializeDefaultMissions()
    {
        // 任务1：新手训练
        missions.Add(new Mission
        {
            missionName = "初次出击",
            description = "消灭3架敌机，证明你的战斗能力",
            type = Mission.MissionType.KillAll,
            targetCount = 3,
            timeLimit = 180f,
            reward = new Mission.Reward { experience = 100, credits = 500 }
        });
        
        // 任务2：空中优势
        missions.Add(new Mission
        {
            missionName = "空中优势",
            description = "消灭5架敌机，夺取制空权",
            type = Mission.MissionType.KillAll,
            targetCount = 5,
            timeLimit = 240f,
            reward = new Mission.Reward { experience = 200, credits = 1000 }
        });
        
        // 任务3：生存训练
        missions.Add(new Mission
        {
            missionName = "生存训练",
            description = "在敌机包围中生存60秒",
            type = Mission.MissionType.Survive,
            targetCount = 1,
            timeLimit = 60f,
            reward = new Mission.Reward { experience = 150, credits = 750 }
        });
        
        // 任务4：突袭任务
        missions.Add(new Mission
        {
            missionName = "突袭",
            description = "消灭8架敌机完成突袭",
            type = Mission.MissionType.KillAll,
            targetCount = 8,
            timeLimit = 300f,
            reward = new Mission.Reward { experience = 300, credits = 1500 }
        });
        
        // 任务5：精英挑战
        missions.Add(new Mission
        {
            missionName = "精英挑战",
            description = "消灭10架敌机，包括精英单位",
            type = Mission.MissionType.KillAll,
            targetCount = 10,
            timeLimit = 360f,
            reward = new Mission.Reward { experience = 500, credits = 2500 }
        });
    }
    
    void ActivateMission(int index)
    {
        if (index < 0 || index >= missions.Count) return;
        
        currentMissionIndex = index;
        var mission = missions[index];
        
        // 重置任务状态
        mission.isCompleted = false;
        mission.isFailed = false;
        mission.currentProgress = 0;
        enemiesKilled = 0;
        
        Debug.Log($"任务开始: {mission.missionName}");
        
        // 更新HUD
        if (hud != null)
        {
            hud.SetNavPoint(mission.missionName);
        }
        
        // 刷新敌人
        SpawnInitialEnemies();
    }
    
    void UpdateMissionProgress(Mission mission)
    {
        // 时间限制检查
        if (mission.timeLimit > 0)
        {
            mission.timeLimit -= Time.deltaTime;
            
            if (mission.timeLimit <= 0)
            {
                FailMission("时间耗尽");
                return;
            }
        }
        
        // 根据任务类型更新进度
        switch (mission.type)
        {
            case Mission.MissionType.KillAll:
                mission.currentProgress = enemiesKilled;
                if (enemiesKilled >= mission.targetCount)
                {
                    CompleteMission();
                }
                break;
                
            case Mission.MissionType.Survive:
                if (mission.timeLimit <= 0)
                {
                    CompleteMission();
                }
                break;
                
            case Mission.MissionType.ReachPoint:
                CheckReachPoint(mission);
                break;
        }
    }
    
    void CheckReachPoint(Mission mission)
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;
        
        float distance = Vector3.Distance(player.transform.position, mission.targetPosition);
        if (distance <= mission.radius)
        {
            mission.currentProgress++;
            if (mission.currentProgress >= mission.targetCount)
            {
                CompleteMission();
            }
            else
            {
                // 生成新目标
                GenerateNewTarget(mission);
            }
        }
    }
    
    void CompleteMission()
    {
        if (currentMissionIndex < 0 || currentMissionIndex >= missions.Count) return;
        
        var mission = missions[currentMissionIndex];
        mission.isCompleted = true;
        
        Debug.Log($"任务完成: {mission.missionName}");
        Debug.Log($"奖励: 经验+{mission.reward.experience}, 货币+{mission.reward.credits}");
        
        // 显示奖励
        ShowReward(mission.reward);
        
        // 下一任务
        if (currentMissionIndex + 1 < missions.Count)
        {
            // 延迟激活下一任务
            Invoke(nameof(NextMission), 5f);
        }
        else
        {
            Debug.Log("所有任务完成！");
        }
    }
    
    void FailMission(string reason)
    {
        if (currentMissionIndex < 0 || currentMissionIndex >= missions.Count) return;
        
        var mission = missions[currentMissionIndex];
        mission.isFailed = true;
        
        Debug.Log($"任务失败: {mission.missionName} - {reason}");
        
        // 可以选择重试或下一任务
        Invoke(nameof(RetryMission), 3f);
    }
    
    void NextMission()
    {
        ActivateMission(currentMissionIndex + 1);
    }
    
    void RetryMission()
    {
        ActivateMission(currentMissionIndex);
    }
    
    void ShowReward(Mission.Reward reward)
    {
        // 可以在这里添加UI显示奖励的逻辑
        Debug.Log($"获得经验: {reward.experience}");
        Debug.Log($"获得货币: {reward.credits}");
        
        foreach (var unlock in reward.unlocks)
        {
            Debug.Log($"解锁: {unlock}");
        }
    }
    
    // ===== 敌人管理 =====
    void SpawnInitialEnemies()
    {
        if (currentMissionIndex < 0 || currentMissionIndex >= missions.Count) return;
        
        var mission = missions[currentMissionIndex];
        int spawnCount = Mathf.Min(mission.targetCount, maxActiveEnemies);
        
        for (int i = 0; i < spawnCount; i++)
        {
            SpawnEnemy();
        }
    }
    
    void UpdateEnemySpawn()
    {
        if (currentMissionIndex < 0 || currentMissionIndex >= missions.Count) return;
        
        var mission = missions[currentMissionIndex];
        if (mission.isCompleted || mission.isFailed) return;
        
        // 检查是否需要刷新敌人
        var activeEnemies = FindObjectsOfType<EnemyAIController>();
        
        if (activeEnemies.Length < maxActiveEnemies && Time.time - lastSpawnTime > spawnInterval)
        {
            if (mission.type == Mission.MissionType.KillAll)
            {
                int remaining = mission.targetCount - enemiesKilled;
                if (remaining > 0)
                {
                    SpawnEnemy();
                    lastSpawnTime = Time.time;
                }
            }
        }
    }
    
    void SpawnEnemy()
    {
        if (enemyPrefab == null)
        {
            // 创建默认敌人
            GameObject enemy = GameObject.CreatePrimitive(PrimitiveType.Cube);
            enemy.name = "Enemy";
            enemy.transform.localScale = new Vector3(10, 3, 15);
            
            var renderer = enemy.GetComponent<Renderer>();
            renderer.material = new Material(Shader.Find("Standard"));
            renderer.material.color = Color.red;
            
            enemy.AddComponent<EnemyAIController>();
            enemy.AddComponent<Damageable>();
            
            PositionEnemy(enemy.transform);
        }
        else
        {
            GameObject enemy = Instantiate(enemyPrefab);
            PositionEnemy(enemy.transform);
        }
    }
    
    void PositionEnemy(Transform enemy)
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        Vector3 spawnPos;
        
        if (player != null)
        {
            // 在玩家周围生成
            float angle = Random.Range(0f, 360f);
            float distance = Random.Range(3000f, 5000f);
            
            spawnPos = player.transform.position + 
                       Quaternion.Euler(0, angle, 0) * Vector3.forward * distance;
        }
        else
        {
            spawnPos = new Vector3(Random.Range(-5000f, 5000f), 
                                   Random.Range(500f, 3000f), 
                                   Random.Range(-5000f, 5000f));
        }
        
        enemy.position = spawnPos;
        
        // 随机朝向
        enemy.rotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
    }
    
    public void OnEnemyDestroyed(GameObject enemy)
    {
        if (currentMissionIndex < 0 || currentMissionIndex >= missions.Count) return;
        
        var mission = missions[currentMissionIndex];
        
        if (mission.type == Mission.MissionType.KillAll)
        {
            enemiesKilled++;
            mission.currentProgress = enemiesKilled;
            
            // 更新HUD
            if (hud != null)
            {
                hud.SetTarget($"击杀: {enemiesKilled}/{mission.targetCount}");
            }
            
            Debug.Log($"击杀: {enemiesKilled}/{mission.targetCount}");
        }
        
        // 检查是否完成
        if (enemiesKilled >= mission.targetCount && mission.type == Mission.MissionType.KillAll)
        {
            CompleteMission();
        }
    }
    
    // ===== 公共方法 =====
    public Mission GetCurrentMission()
    {
        if (currentMissionIndex >= 0 && currentMissionIndex < missions.Count)
        {
            return missions[currentMissionIndex];
        }
        return null;
    }
    
    public int GetTotalProgress()
    {
        int total = 0;
        foreach (var m in missions)
        {
            if (m.isCompleted) total++;
        }
        return total;
    }
    
    public void ResetAllMissions()
    {
        foreach (var m in missions)
        {
            m.isCompleted = false;
            m.isFailed = false;
            m.currentProgress = 0;
        }
        currentMissionIndex = 0;
        enemiesKilled = 0;
        
        if (missions.Count > 0)
        {
            ActivateMission(0);
        }
    }
}
