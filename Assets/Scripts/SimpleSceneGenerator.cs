using UnityEngine;

public class SimpleSceneGenerator : MonoBehaviour
{
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
        rb.useGravity = false;
        
        // 添加飞行控制脚本
        playerShip.AddComponent<TestFlight>();
        
        // 创建地面
        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Ground";
        ground.transform.localScale = new Vector3(20, 1, 20);
        
        // 创建摄像机跟随
        Camera mainCamera = Camera.main;
        CameraFollow cameraFollow = mainCamera.gameObject.AddComponent<CameraFollow>();
        cameraFollow.target = playerShip.transform;
        cameraFollow.distance = 15f;
        cameraFollow.height = 5f;
        
        Debug.Log("场景生成完成！");
    }
}