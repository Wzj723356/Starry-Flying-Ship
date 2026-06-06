using UnityEngine;

public class PlanetGenerator : MonoBehaviour
{
    [Header("星球参数")]
    public int planetCount = 5;
    public float minPlanetSize = 50f;
    public float maxPlanetSize = 200f;
    public float minDistance = 500f;
    public float maxDistance = 2000f;
    
    [Header("材质")]
    public Material planetMaterial;
    
    void Start()
    {
        GeneratePlanets();
    }
    
    void GeneratePlanets()
    {
        for (int i = 0; i < planetCount; i++)
        {
            GeneratePlanet(i);
        }
    }
    
    void GeneratePlanet(int index)
    {
        // 创建星球
        GameObject planet = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        planet.name = $"Planet_{index}";
        
        // 随机大小
        float size = Random.Range(minPlanetSize, maxPlanetSize);
        planet.transform.localScale = Vector3.one * size;
        
        // 随机位置
        Vector2 randomCircle = Random.insideUnitCircle.normalized * Random.Range(minDistance, maxDistance);
        planet.transform.position = new Vector3(randomCircle.x, Random.Range(-200f, 200f), randomCircle.y);
        
        // 材质
        if (planetMaterial != null)
        {
            planet.GetComponent<Renderer>().material = planetMaterial;
        }
        else
        {
            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = Random.ColorHSV(0, 1, 0.5f, 0.8f, 0.6f, 0.9f);
            planet.GetComponent<Renderer>().material = mat;
        }
        
        // 添加大气层
        GenerateAtmosphere(planet, size);
        
        // 添加光环（随机）
        if (Random.value > 0.7f)
        {
            GenerateRing(planet, size);
        }
    }
    
    void GenerateAtmosphere(GameObject planet, float size)
    {
        GameObject atmosphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        atmosphere.name = "Atmosphere";
        atmosphere.transform.parent = planet.transform;
        atmosphere.transform.localPosition = Vector3.zero;
        atmosphere.transform.localScale = Vector3.one * 1.1f;
        
        Material atmoMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        atmoMat.color = new Color(0.5f, 0.7f, 1f, 0.2f);
        atmosphere.GetComponent<Renderer>().material = atmoMat;
    }
    
    void GenerateRing(GameObject planet, float size)
    {
        GameObject ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        ring.name = "Ring";
        ring.transform.parent = planet.transform;
        ring.transform.localPosition = Vector3.zero;
        ring.transform.localScale = new Vector3(size * 2f, 0.1f, size * 2f);
        ring.transform.rotation = Quaternion.Euler(90f, 0f, Random.Range(0f, 30f));
        
        Material ringMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        ringMat.color = new Color(0.8f, 0.7f, 0.5f, 0.5f);
        ring.GetComponent<Renderer>().material = ringMat;
    }
}