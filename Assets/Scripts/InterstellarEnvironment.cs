using UnityEngine;

public class InterstellarEnvironment : MonoBehaviour
{
    [Header("星空参数")]
    public int starCount = 1000;
    public float starFieldRadius = 5000f;
    
    [Header("星云参数")]
    public int nebulaCount = 10;
    public Material nebulaMaterial;
    
    [Header("小行星带")]
    public int asteroidCount = 200;
    public float asteroidBeltRadius = 800f;
    
    void Start()
    {
        GenerateStarField();
        GenerateNebulae();
        GenerateAsteroidBelt();
    }
    
    void GenerateStarField()
    {
        GameObject starField = new GameObject("StarField");
        
        for (int i = 0; i < starCount; i++)
        {
            GameObject star = new GameObject($"Star_{i}");
            star.transform.parent = starField.transform;
            
            // 随机位置
            Vector3 pos = Random.insideUnitSphere * starFieldRadius;
            star.transform.position = pos;
            
            // 添加光源
            Light light = star.AddComponent<Light>();
            light.type = LightType.Point;
            light.range = Random.Range(10f, 100f);
            light.intensity = Random.Range(0.1f, 1f);
            light.color = Random.ColorHSV(0, 1, 0.8f, 1f, 0.8f, 1f);
        }
    }
    
    void GenerateNebulae()
    {
        for (int i = 0; i < nebulaCount; i++)
        {
            GameObject nebula = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            nebula.name = $"Nebula_{i}";
            
            Vector3 pos = Random.insideUnitSphere * starFieldRadius * 0.5f;
            nebula.transform.position = pos;
            nebula.transform.localScale = Vector3.one * Random.Range(100f, 500f);
            
            if (nebulaMaterial != null)
            {
                nebula.GetComponent<Renderer>().material = nebulaMaterial;
            }
            else
            {
                Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                mat.color = new Color(Random.value, Random.value, Random.value, 0.1f);
                nebula.GetComponent<Renderer>().material = mat;
            }
        }
    }
    
    void GenerateAsteroidBelt()
    {
        GameObject belt = new GameObject("AsteroidBelt");
        
        for (int i = 0; i < asteroidCount; i++)
        {
            GameObject asteroid = GameObject.CreatePrimitive(PrimitiveType.Cube);
            asteroid.name = $"Asteroid_{i}";
            asteroid.transform.parent = belt.transform;
            
            // 环形分布
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            float radius = asteroidBeltRadius + Random.Range(-50f, 50f);
            
            asteroid.transform.position = new Vector3(
                Mathf.Cos(angle) * radius,
                Random.Range(-20f, 20f),
                Mathf.Sin(angle) * radius
            );
            
            asteroid.transform.rotation = Random.rotation;
            asteroid.transform.localScale = Vector3.one * Random.Range(1f, 10f);
            
            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = new Color(0.3f, 0.3f, 0.3f);
            asteroid.GetComponent<Renderer>().material = mat;
        }
    }
}