using UnityEngine;

public class MissionManager : MonoBehaviour
{
    public static MissionManager instance;
    
    [Header("当前任务")]
    public Mission currentMission;
    public bool missionActive = false;
    
    [Header("任务进度")]
    public int enemiesKilled = 0;
    public float timeElapsed = 0f;
    
    public System.Action<Mission> OnMissionComplete;
    public System.Action<Mission> OnMissionFailed;
    
    public enum MissionType
    {
        Elimination,
        Survival,
        Escort,
        Recon
    }
    
    [System.Serializable]
    public class Mission
    {
        public string name;
        public MissionType type;
        public int targetCount;
        public float timeLimit;
        public int reward;
    }
    
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }
    
    void Update()
    {
        if (!missionActive) return;
        
        timeElapsed += Time.deltaTime;
        CheckMissionProgress();
    }
    
    public void StartMission(Mission mission)
    {
        currentMission = mission;
        missionActive = true;
        enemiesKilled = 0;
        timeElapsed = 0f;
        
        Debug.Log($"任务开始: {mission.name}");
    }
    
    void CheckMissionProgress()
    {
        if (currentMission == null) return;
        
        switch (currentMission.type)
        {
            case MissionType.Elimination:
                if (enemiesKilled >= currentMission.targetCount)
                    CompleteMission();
                break;
                
            case MissionType.Survival:
                if (timeElapsed >= currentMission.timeLimit)
                    CompleteMission();
                break;
        }
        
        // 检查失败条件
        if (currentMission.timeLimit > 0 && timeElapsed > currentMission.timeLimit)
        {
            if (currentMission.type == MissionType.Elimination)
                FailMission();
        }
    }
    
    void CompleteMission()
    {
        Debug.Log($"任务完成: {currentMission.name} 奖励: {currentMission.reward}");
        
        if (GameManager.instance != null)
            GameManager.instance.AddScore(currentMission.reward);
        
        OnMissionComplete?.Invoke(currentMission);
        missionActive = false;
    }
    
    void FailMission()
    {
        Debug.Log($"任务失败: {currentMission.name}");
        
        OnMissionFailed?.Invoke(currentMission);
        missionActive = false;
    }
    
    public void OnEnemyKilled()
    {
        enemiesKilled++;
        
        if (GameManager.instance != null)
            GameManager.instance.AddKill();
    }
    
    public string GetMissionStatus()
    {
        if (!missionActive || currentMission == null)
            return "无任务";
        
        return $"{currentMission.name}: {enemiesKilled}/{currentMission.targetCount}";
    }
}