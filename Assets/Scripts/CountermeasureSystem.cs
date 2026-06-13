using UnityEngine;

public class CountermeasureSystem : MonoBehaviour
{
    [Header("干扰弹配置")]
    public int countermeasureCount = 8;
    public int maxCountermeasures = 8;
    public GameObject countermeasurePrefab;
    public float deployCooldown = 1f;
    
    [Header("投放参数")]
    public float deploySpeed = 50f;
    public float deploySpread = 30f;
    
    private float lastDeployTime = 0f;
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C) || Input.GetKeyDown(KeyCode.X))
        {
            DeployCountermeasure();
        }
    }
    
    public void DeployCountermeasure()
    {
        if (countermeasureCount <= 0)
        {
            Debug.Log("干扰弹已耗尽！");
            return;
        }
        
        if (Time.time - lastDeployTime < deployCooldown)
            return;
        
        // 投放干扰弹
        if (countermeasurePrefab != null)
        {
            Vector3 deployPos = transform.position - transform.forward * 5f;
            deployPos += Random.insideUnitSphere * 2f;
            
            GameObject cm = Instantiate(countermeasurePrefab, deployPos, Random.rotation);
            Rigidbody rb = cm.GetComponent<Rigidbody>();
            if (rb == null)
                rb = cm.AddComponent<Rigidbody>();
            
            rb.velocity = -transform.forward * deploySpeed + Random.insideUnitSphere * deploySpread;
        }
        
        countermeasureCount--;
        lastDeployTime = Time.time;
        
        Debug.Log($"干扰弹投放！剩余: {countermeasureCount}/{maxCountermeasures}");
    }
    
    public void ReloadCountermeasures()
    {
        countermeasureCount = maxCountermeasures;
        Debug.Log("干扰弹装填完成！");
    }
    
    public int GetCountermeasureCount() => countermeasureCount;
    public int GetMaxCountermeasures() => maxCountermeasures;
}