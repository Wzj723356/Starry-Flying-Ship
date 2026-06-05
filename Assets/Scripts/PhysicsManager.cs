using UnityEngine;

public class PhysicsManager : MonoBehaviour
{
    [Header("全局物理设置")]
    public float globalGravity = -9.81f;
    public float windSpeed = 10f;
    public Vector3 windDirection = Vector3.right;
    
    [Header("大气设置")]
    public float seaLevelPressure = 101325f;
    public float seaLevelTemperature = 288.15f;
    public float temperatureLapseRate = 0.0065f;
    
    void Awake()
    {
        Physics.gravity = new Vector3(0, globalGravity, 0);
    }
    
    void Update()
    {
        UpdateWind();
    }
    
    void UpdateWind()
    {
        Vector3 windVelocity = windDirection.normalized * windSpeed;
        
        Collider[] colliders = Physics.OverlapSphere(Vector3.zero, 10000f);
        
        foreach (var collider in colliders)
        {
            Rigidbody rb = collider.GetComponent<Rigidbody>();
            if (rb != null && !rb.isKinematic)
            {
                rb.AddForce(windVelocity * Time.deltaTime, ForceMode.VelocityChange);
            }
        }
    }
    
    public float GetAirPressure(float altitude)
    {
        return seaLevelPressure * Mathf.Pow(1 - (temperatureLapseRate * altitude) / seaLevelTemperature, 5.256f);
    }
    
    public float GetAirTemperature(float altitude)
    {
        return Mathf.Max(216.65f, seaLevelTemperature - temperatureLapseRate * altitude);
    }
    
    public float GetAirDensity(float altitude)
    {
        float pressure = GetAirPressure(altitude);
        float temperature = GetAirTemperature(altitude);
        return pressure / (287.058f * temperature);
    }
    
    public float GetSpeedOfSound(float altitude)
    {
        float temperature = GetAirTemperature(altitude);
        return Mathf.Sqrt(1.4f * 287.058f * temperature);
    }
}
