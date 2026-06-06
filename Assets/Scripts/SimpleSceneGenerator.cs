using UnityEngine;

public class SimpleSceneGenerator : MonoBehaviour
{
    void Awake()
    {
        // 生成星星
        GenerateStars();
        
        // 生成地面
        GenerateGround();
        
        // 生成飞船
        GenerateShip();
        
        // 生成相机
        GenerateCamera();
    }
    
    void GenerateStars()
    {
        GameObject starField = new GameObject("StarField");
        for (int i = 0; i < 300; i++)
        {
            GameObject star = new GameObject($"Star_{i}");
            star.transform.parent = starField.transform;
            Vector3 pos = Random.insideUnitSphere * 500f;
            pos.y = Mathf.Abs(pos.y);
            star.transform.position = pos;
            
            Light light = star.AddComponent<Light>();
            light.type = LightType.Point;
            light.range = Random.Range(5f, 30f);
            light.intensity = Random.Range(0.1f, 0.5f);
            light.color = Random.ColorHSV(0, 1, 0.8f, 1f, 0.8f, 1f);
        }
    }
    
    void GenerateGround()
    {
        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Ground";
        ground.transform.localScale = Vector3.one * 50f;
        
        Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        mat.color = new Color(0.1f, 0.15f, 0.2f);
        ground.GetComponent<Renderer>().material = mat;
    }
    
    void GenerateShip()
    {
        GameObject ship = GameObject.CreatePrimitive(PrimitiveType.Cube);
        ship.name = "PlayerShip";
        ship.transform.position = new Vector3(0, 30f, 0);
        ship.transform.localScale = new Vector3(2f, 1f, 6f);
        
        // 添加物理飞行控制
        ship.AddComponent<SimpleFlightTest>();
        
        // 添加材质
        Material shipMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        shipMat.color = new Color(0.2f, 0.6f, 1f);
        ship.GetComponent<Renderer>().material = shipMat;
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
        
        mainCam.transform.position = new Vector3(0, 25f, -40f);
        mainCam.transform.LookAt(Vector3.zero);
        
        // 添加相机跟随
        CameraFollow follow = mainCam.gameObject.AddComponent<CameraFollow>();
        follow.target = GameObject.Find("PlayerShip")?.transform;
        follow.offset = new Vector3(0, 15f, -25f);
    }
}