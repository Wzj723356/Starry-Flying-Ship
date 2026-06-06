using UnityEngine;

public class SimpleSceneGenerator : MonoBehaviour
{
    [Header("生成选项")]
    public bool generateStars = true;
    public bool generatePlanets = true;
    public bool generateAsteroids = true;
    public bool generatePlayer = true;
    public bool generateEnemies = true;
    
    void Awake()
    {
        if (generateStars) GenerateStars();
        if (generatePlanets) GeneratePlanets();
        if (generateAsteroids) GenerateAsteroids();
        if (generatePlayer) GeneratePlayer();
        if (generateEnemies) GenerateEnemies();
        
        GenerateCamera();
        SetupGameManager();
    }
    
    void GenerateStars()
    {
        GameObject starField = new GameObject("StarField");
        for (int i = 0; i < 500; i++)
        {
            GameObject star = new GameObject($"Star_{i}");
            star.transform.parent = starField.transform;
            Vector3 pos = Random.insideUnitSphere * 2000f;
            star.transform.position = pos;
            
            Light light = star.AddComponent<Light>();
            light.type = LightType.Point;
            light.range = Random.Range(20f, 100f);
            light.intensity = Random.Range(0.2f, 1f);
            light.color = Random.ColorHSV(0, 1, 0.8f, 1f, 0.8f, 1f);
        }
    }
    
    void GeneratePlanets()
    {
        for (int i = 0; i < 5; i++)
        {
            GameObject planet = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            planet.name = $"Planet_{i}";
            
            float size = Random.Range(50f, 200f);
            planet.transform.localScale = Vector3.one * size;
            
            Vector2 circle = Random.insideUnitCircle.normalized * Random.Range(500f, 1500f);
            planet.transform.position = new Vector3(circle.x, Random.Range(-100f, 100f), circle.y);
            
            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = Random.ColorHSV(0, 1, 0.5f, 0.8f, 0.6f, 0.9f);
            planet.GetComponent<Renderer>().material = mat;
        }
    }
    
    void GenerateAsteroids()
    {
        GameObject belt = new GameObject("AsteroidBelt");
        for (int i = 0; i < 100; i++)
        {
            GameObject asteroid = GameObject.CreatePrimitive(PrimitiveType.Cube);
            asteroid.name = $"Asteroid_{i}";
            asteroid.transform.parent = belt.transform;
            
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            float radius = 400f + Random.Range(-30f, 30f);
            
            asteroid.transform.position = new Vector3(
                Mathf.Cos(angle) * radius,
                Random.Range(-10f, 10f),
                Mathf.Sin(angle) * radius
            );
            
            asteroid.transform.rotation = Random.rotation;
            asteroid.transform.localScale = Vector3.one * Random.Range(2f, 8f);
            
            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = new Color(0.4f, 0.35f, 0.3f);
            asteroid.GetComponent<Renderer>().material = mat;
        }
    }
    
    void GeneratePlayer()
    {
        GameObject ship = GameObject.CreatePrimitive(PrimitiveType.Cube);
        ship.name = "PlayerShip";
        ship.tag = "Player";
        ship.transform.position = new Vector3(0, 50f, 0);
        ship.transform.localScale = new Vector3(2f, 1f, 6f);
        
        // 飞行控制
        ship.AddComponent<StarShipFlightController>();
        
        // 武器系统
        ship.AddComponent<WeaponSystem>();
        
        // 干扰弹
        ship.AddComponent<CountermeasureSystem>();
        
        // 目标系统
        ship.AddComponent<TargetingSystem>();
        
        // 可损坏
        ship.AddComponent<Damageable>();
        
        // 材质
        Material shipMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        shipMat.color = new Color(0.2f, 0.6f, 1f);
        ship.GetComponent<Renderer>().material = shipMat;
    }
    
    void GenerateEnemies()
    {
        for (int i = 0; i < 3; i++)
        {
            GameObject enemy = GameObject.CreatePrimitive(PrimitiveType.Cube);
            enemy.name = $"Enemy_{i}";
            enemy.tag = "Enemy";
            
            Vector2 circle = Random.insideUnitCircle.normalized * Random.Range(200f, 400f);
            enemy.transform.position = new Vector3(circle.x, 50f, circle.y);
            enemy.transform.localScale = new Vector3(2f, 1f, 5f);
            
            enemy.AddComponent<EnemyAIController>();
            enemy.AddComponent<Damageable>();
            
            Material enemyMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            enemyMat.color = new Color(1f, 0.2f, 0.2f);
            enemy.GetComponent<Renderer>().material = enemyMat;
        }
    }
    
    void GenerateCamera()
    {
        Camera mainCam = Camera.main;
        if (mainCam == null)
        {
            GameObject camObj = new GameObject("MainCamera");
            mainCam = camObj.AddComponent<Camera>();
            mainCam.tag = "MainCamera";
        }
        
        mainCam.transform.position = new Vector3(0, 70f, -80f);
        
        CameraFollow follow = mainCam.gameObject.AddComponent<CameraFollow>();
        follow.target = GameObject.Find("PlayerShip")?.transform;
        follow.offset = new Vector3(0, 20f, -30f);
    }
    
    void SetupGameManager()
    {
        GameObject manager = new GameObject("GameManager");
        manager.AddComponent<GameManager>();
        manager.AddComponent<MissionManager>();
        manager.AddComponent<NetworkManager>();
        manager.AddComponent<ChatSystem>();
        manager.AddComponent<FriendSystem>();
    }
}