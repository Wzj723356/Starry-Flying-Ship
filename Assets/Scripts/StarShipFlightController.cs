using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(Rigidbody))]
public class StarShipFlightController : MonoBehaviour
{
    [Header("星舰基础参数")]
    public float shipMass = 50000f;
    public Vector3 shipDimensions = new Vector3(20f, 5f, 50f);
    
    [Header("飞行参数")]
    public float maxThrust = 15000f;
    public float maxSpeed = 500f;
    public float maxAltitude = 50000f;
    public float minAltitude = 100f;
    public float gravity = 9.81f;
    
    [Header("操控灵敏度")]
    public float pitchSensitivity = 60f;
    public float rollSensitivity = 120f;
    public float yawSensitivity = 45f;
    public float thrustSensitivity = 100f;
    
    [Header("引擎设置")]
    public int engineCount = 4;
    public float[] engineThrust = new float[4] { 5000f, 5000f, 5000f, 5000f };
    public bool afterburnerEnabled = false;
    public float afterburnerThrustMultiplier = 1.5f;
    
    [Header("惯性参数")]
    public Vector3 inertiaTensor = new Vector3(10000f, 20000f, 5000f);
    public Vector3 centerOfMass = new Vector3(0, 0, 2f);
    
    [Header("环境设置")]
    public bool useSpaceFlight = true;
    public bool useAtmosphericFlight = true;
    public float atmosphericDensity = 1.225f;
    public float windSpeed = 10f;
    
    private Rigidbody rb;
    private InputManager input;
    private EngineController[] engines;
    private bool afterburnerActive = false;
    private float currentThrust = 0f;
    private float targetThrust = 0f;
    
    private Vector3 velocity;
    private Vector3 angularVelocity;
    
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }
        
        ConfigureRigidbody();
        
        input = GetComponent<InputManager>();
        if (input == null)
        {
            input = gameObject.AddComponent<InputManager>();
        }
        
        InitializeEngines();
        CreateRectangularTestModel();
    }
    
    void ConfigureRigidbody()
    {
        rb.mass = shipMass;
        rb.drag = 0.1f;
        rb.angularDrag = 0.5f;
        rb.useGravity = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rb.inertiaTensor = inertiaTensor;
        rb.centerOfMass = centerOfMass;
    }
    
    void InitializeEngines()
    {
        engines = new EngineController[engineCount];
        
        for (int i = 0; i < engineCount; i++)
        {
            GameObject engineObj = new GameObject($"Engine_{i}");
            engineObj.transform.parent = transform;
            
            Vector3[] positions = {
                new Vector3(-3f, -1f, -10f),
                new Vector3(3f, -1f, -10f),
                new Vector3(-5f, 0f, -8f),
                new Vector3(5f, 0f, -8f)
            };
            
            engineObj.transform.localPosition = positions[i % positions.Length];
            engineObj.transform.localRotation = Quaternion.Euler(90, 0, 0);
            
            engines[i] = engineObj.AddComponent<EngineController>();
            engines[i].thrustForce = engineThrust[i];
            engines[i].fuelConsumption = 1.0f;
            engines[i].ignitionTime = 0.5f;
        }
    }
    
    void CreateRectangularTestModel()
    {
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        if (meshFilter == null)
        {
            meshFilter = gameObject.AddComponent<MeshFilter>();
        }
        
        MeshRenderer renderer = GetComponent<MeshRenderer>();
        if (renderer == null)
        {
            renderer = gameObject.AddComponent<MeshRenderer>();
        }
        
        Mesh mesh = new Mesh();
        mesh.name = "RectangularStarShip";
        
        Vector3 size = shipDimensions;
        Vector3 halfSize = size * 0.5f;
        
        Vector3[] vertices = new Vector3[]
        {
            new Vector3(-halfSize.x, 0, halfSize.z),
            new Vector3(halfSize.x, 0, halfSize.z),
            new Vector3(halfSize.x, 0, -halfSize.z),
            new Vector3(-halfSize.x, 0, -halfSize.z),
            
            new Vector3(-halfSize.x, halfSize.y * 0.3f, halfSize.z),
            new Vector3(halfSize.x, halfSize.y * 0.3f, halfSize.z),
            new Vector3(halfSize.x, halfSize.y * 0.3f, -halfSize.z),
            new Vector3(-halfSize.x, halfSize.y * 0.3f, -halfSize.z),
            
            new Vector3(0, halfSize.y, 0),
        };
        
        int[] triangles = new int[]
        {
            0, 1, 4,
            1, 5, 4,
            1, 2, 5,
            2, 6, 5,
            2, 3, 6,
            3, 7, 6,
            3, 0, 7,
            0, 4, 7,
            4, 5, 8,
            5, 6, 8,
            6, 7, 8,
            7, 4, 8,
        };
        
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        
        meshFilter.mesh = mesh;
        
        Material shipMaterial = new Material(Shader.Find("Standard"));
        shipMaterial.color = new Color(0.8f, 0.9f, 1.0f);
        shipMaterial.metallic = 0.8f;
        shipMaterial.smoothness = 0.9f;
        renderer.material = shipMaterial;
        
        BoxCollider collider = GetComponent<BoxCollider>();
        if (collider == null)
        {
            collider = gameObject.AddComponent<BoxCollider>();
        }
        collider.size = size;
        collider.center = Vector3.zero;
    }
    
    void Update()
    {
        ReadInputs();
        UpdateEngines();
        UpdateEnvironmentEffects();
    }
    
    void FixedUpdate()
    {
        ApplyThrust();
        ApplyControlForces();
        ApplyAtmosphericDrag();
        ClampPosition();
        UpdateVelocityTracking();
    }
    
    void ReadInputs()
    {
        float pitch = input.Pitch;
        float roll = input.Roll;
        float yaw = input.Yaw;
        
        targetThrust = input.Throttle * maxThrust;
        
        if (input.AfterburnerToggle)
        {
            afterburnerActive = !afterburnerActive;
        }
        
        ApplyFlightControls(pitch, roll, yaw);
    }
    
    void ApplyFlightControls(float pitch, float roll, float yaw)
    {
        float speedFactor = CalculateSpeedFactor();
        
        Vector3 pitchTorque = transform.right * pitch * pitchSensitivity * speedFactor * Time.fixedDeltaTime;
        Vector3 rollTorque = transform.forward * roll * rollSensitivity * speedFactor * Time.fixedDeltaTime;
        Vector3 yawTorque = transform.up * yaw * yawSensitivity * speedFactor * Time.fixedDeltaTime;
        
        rb.AddTorque(pitchTorque + rollTorque + yawTorque);
    }
    
    float CalculateSpeedFactor()
    {
        float currentSpeed = rb.velocity.magnitude;
        float minSpeed = 10f;
        
        if (currentSpeed < minSpeed)
        {
            return currentSpeed / minSpeed;
        }
        
        return Mathf.Clamp01(currentSpeed / 200f);
    }
    
    void ApplyThrust()
    {
        currentThrust = Mathf.MoveTowards(currentThrust, targetThrust, thrustSensitivity * Time.fixedDeltaTime);
        
        float thrustMultiplier = afterburnerActive ? afterburnerThrustMultiplier : 1f;
        Vector3 thrustForce = transform.forward * currentThrust * thrustMultiplier;
        
        rb.AddForce(thrustForce, ForceMode.Force);
    }
    
    void ApplyControlForces()
    {
    }
    
    void ApplyAtmosphericDrag()
    {
        if (!useAtmosphericFlight || useSpaceFlight)
            return;
        
        float altitude = transform.position.y;
        if (altitude > 10000)
        {
            useAtmosphericFlight = false;
            return;
        }
        
        float dragCoefficient = 0.1f;
        float dynamicPressure = 0.5f * atmosphericDensity * rb.velocity.magnitude * rb.velocity.magnitude;
        float dragForce = dragCoefficient * dynamicPressure;
        
        Vector3 drag = -rb.velocity.normalized * dragForce;
        rb.AddForce(drag);
    }
    
    void UpdateEnvironmentEffects()
    {
        if (!useSpaceFlight)
            return;
        
        float altitude = transform.position.y;
        
        float gravityStrength = useAtmosphericFlight ? gravity : 0f;
        
        if (altitude > 5000)
        {
            float spaceFactor = Mathf.Clamp01((altitude - 5000) / 5000);
            rb.useGravity = spaceFactor < 0.1f;
        }
    }
    
    void ClampPosition()
    {
        Vector3 pos = transform.position;
        
        if (pos.y > maxAltitude)
        {
            pos.y = maxAltitude;
            if (rb.velocity.y > 0)
            {
                Vector3 vel = rb.velocity;
                vel.y = 0;
                rb.velocity = vel;
            }
        }
        
        if (pos.y < minAltitude)
        {
            pos.y = minAltitude;
            if (rb.velocity.y < 0)
            {
                Vector3 vel = rb.velocity;
                vel.y = 0;
                rb.velocity = vel;
            }
        }
        
        transform.position = pos;
        
        if (rb.velocity.magnitude > maxSpeed)
        {
            rb.velocity = rb.velocity.normalized * maxSpeed;
        }
    }
    
    void UpdateVelocityTracking()
    {
        velocity = rb.velocity;
        angularVelocity = rb.angularVelocity;
    }
    
    void UpdateEngines()
    {
        float thrustPercentage = currentThrust / maxThrust;
        
        foreach (var engine in engines)
        {
            if (engine != null)
            {
                engine.SetThrustPercentage(thrustPercentage);
                engine.SetAfterburner(afterburnerActive);
            }
        }
    }
    
    public Vector3 GetVelocity() => velocity;
    public float GetSpeed() => velocity.magnitude;
    public float GetAltitude() => transform.position.y;
    public float GetThrustPercentage() => currentThrust / maxThrust;
    public bool IsAfterburnerActive() => afterburnerActive;
    public Vector3 GetAngularVelocity() => angularVelocity;
    public float GetPitch() => angularVelocity.x;
    public float GetRoll() => angularVelocity.z;
    public float GetYaw() => angularVelocity.y;
}
