using UnityEngine;

public class CountermeasureSystem : MonoBehaviour
{
    [Header("=== 干扰弹配置 ===")]
    public GameObject chaffPrefab;      // 干扰弹预制体
    public GameObject flarePrefab;      // 热诱弹预制体
    public Transform launchPoint;       // 发射点
    public int launchCount = 4;         // 每次发射数量
    public float launchInterval = 0.1f; // 发射间隔
    public float chaffLifetime = 5f;    // 干扰弹存在时间
    public float flareLifetime = 4f;    // 热诱弹存在时间
    
    [Header("=== 物理参数 ===")]
    public float ejectForce = 20f;      // 弹出初速度
    public float spreadAngle = 30f;     // 散布角度
    public float drag = 2f;             // 空气阻力
    
    private HUDManager hud;
    private float lastLaunchTime;
    private bool isLaunching;
    
    void Start()
    {
        hud = FindObjectOfType<HUDManager>();
        if (launchPoint == null)
            launchPoint = transform;
    }
    
    void Update()
    {
        // 输入检测（可绑定到InputManager）
        if (Input.GetKeyDown(KeyCode.C) && !isLaunching)
        {
            LaunchChaff();
        }
        if (Input.GetKeyDown(KeyCode.F) && !isLaunching)
        {
            LaunchFlare();
        }
    }
    
    // 发射干扰弹（对抗雷达制导导弹）
    public void LaunchChaff()
    {
        if (hud != null && hud.LaunchChaff())
        {
            StartCoroutine(LaunchSequence(chaffPrefab, chaffLifetime));
        }
    }
    
    // 发射热诱弹（对抗红外制导导弹）
    public void LaunchFlare()
    {
        if (hud != null && hud.LaunchFlare())
        {
            StartCoroutine(LaunchSequence(flarePrefab, flareLifetime));
        }
    }
    
    System.Collections.IEnumerator LaunchSequence(GameObject prefab, float lifetime)
    {
        isLaunching = true;
        
        for (int i = 0; i < launchCount; i++)
        {
            if (prefab != null)
            {
                // 计算随机散布方向
                Vector3 randomDir = Random.insideUnitSphere * spreadAngle;
                Quaternion rotation = Quaternion.Euler(randomDir) * launchPoint.rotation;
                
                // 生成干扰弹/热诱弹
                GameObject cm = Instantiate(prefab, launchPoint.position, rotation);
                
                // 添加物理
                Rigidbody rb = cm.GetComponent<Rigidbody>();
                if (rb == null)
                    rb = cm.AddComponent<Rigidbody>();
                
                rb.useGravity = false;
                rb.drag = drag;
                
                // 向后下方弹出
                Vector3 ejectDir = -launchPoint.forward + Vector3.down * 0.5f;
                ejectDir = ejectDir.normalized;
                rb.AddForce(ejectDir * ejectForce + Random.insideUnitSphere * 5f, ForceMode.Impulse);
                
                // 添加随机旋转
                rb.AddTorque(Random.insideUnitSphere * 10f, ForceMode.Impulse);
                
                // 延迟销毁
                Destroy(cm, lifetime + Random.Range(-0.5f, 0.5f));
            }
            
            yield return new WaitForSeconds(launchInterval);
        }
        
        isLaunching = false;
    }
    
    // 自动发射（被导弹锁定时）
    public void AutoLaunch(bool isRadarMissile)
    {
        if (Time.time - lastLaunchTime < 1f) return; // 冷却1秒
        
        lastLaunchTime = Time.time;
        
        if (isRadarMissile)
            LaunchChaff();
        else
            LaunchFlare();
    }
    
    // 补给
    public void Reload(int chaff, int flare)
    {
        if (hud != null)
            hud.ReloadCountermeasures(chaff, flare);
    }
}
