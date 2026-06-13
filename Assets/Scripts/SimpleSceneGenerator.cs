using UnityEngine;

public class SimpleSceneGenerator : MonoBehaviour
{
    public GameObject playerPrefab;
    
    void Awake()
    {
        GenerateScene();
    }
    
    void GenerateScene()
    {
        // 创建玩家飞船
        GameObject playerShip = GameObject.CreatePrimitive(PrimitiveType.Cube);
        playerShip.name = "PlayerShip";
        playerShip.transform.localScale = new Vector3(4, 1, 2);
        playerShip.transform.position = new Vector3(0, 5, 0);
        
        // 添加刚体
        Rigidbody rb = playerShip.AddComponent<Rigidbody>();
        rb.mass = 1000f;
        rb.drag = 0.1f;
        rb.angularDrag = 0.5f;
        rb.useGravity = true;
        
        // 添加飞行控制脚本
        playerShip.AddComponent<SimpleFlightTest>();
        
        // 创建地面
        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Ground";
        ground.transform.localScale = new Vector3(20, 1, 20);
        
        // 创建一些山峰
        for (int i = 0; i < 10; i++)
        {
            GameObject mountain = GameObject.CreatePrimitive(PrimitiveType.Cone);
            mountain.name = "Mountain_" + i;
            float scale = Random.Range(2, 6);
            mountain.transform.localScale = new Vector3(scale, scale * 1.5f, scale);
            mountain.transform.position = new Vector3(
                Random.Range(-50, 50), 
                scale * 0.75f, 
                Random.Range(-50, 50)
            );
        }
        
        // 创建星星背景
        for (int i = 0; i < 100; i++)
        {
            GameObject star = new GameObject("Star_" + i);
            Light starLight = star.AddComponent<Light>();
            starLight.type = LightType.Point;
            starLight.range = Random.Range(100, 500);
            starLight.intensity = Random.Range(0.1f, 0.5f);
            starLight.color = Color.white;
            star.transform.position = new Vector3(
                Random.Range(-1000, 1000),
                Random.Range(-1000, 1000),
                Random.Range(-1000, 1000)
            );
        }
        
        // 创建主摄像机跟随
        Camera mainCamera = Camera.main;
        CameraFollow cameraFollow = mainCamera.gameObject.AddComponent<CameraFollow>();
        cameraFollow.target = playerShip.transform;
        cameraFollow.distance = 15f;
        cameraFollow.height = 5f;
        
        // 创建游戏管理器
        GameObject gameManager = new GameObject("GameManager");
        gameManager.AddComponent<GameManager>();
        
        Debug.Log("场景生成完成！");
    }
}