using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class SimpleFlightTest : MonoBehaviour
{
    [Header("推力参数")]
    public float maxThrust = 60000f;
    public float idleThrust = 10000f;      // 怠速推力
    public float maxSpeed = 400f;
    public float stallSpeed = 50f;          // 失速速度
    
    [Header("空气动力学")]
    public float liftCoefficient = 0.02f;  // 升力系数
    public float dragCoefficient = 0.001f; // 阻力系数
    public float inducedDrag = 0.005f;      // 诱导阻力
    public float parasiteDrag = 0.003f;     // 寄生阻力
    public float stallAngle = 30f;         // 失速迎角（度）
    
    [Header("控制灵敏度")]
    public float pitchSpeed = 2.5f;
    public float yawSpeed = 1.5f;
    public float rollSpeed = 3f;
    public float autoLevelSpeed = 3f;
    public float controlSurfaceResponse = 5f; // 舵面响应
    
    [Header("鼠标控制")]
    public float maxReticleOffset = 200f;
    public float trackingSpeed = 3f;
    public float returnSpeed = 5f;           // 回正速度
    
    [Header("惯性")]
    public float pitchInertia = 2f;
    public float yawInertia = 1.5f;
    public float rollInertia = 1f;
    
    private Rigidbody rb;
    private Vector2 reticleOffset = Vector2.zero;
    private Vector3 angularVelocitySmooth = Vector3.zero;
    private float currentThrottle = 0.8f;  // 默认80%节流
    
    // 键盘状态检测
    private bool keyboardActive = false;
    
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.mass = 3000f;
        rb.useGravity = false;
        rb.drag = 0f;  // 我们自己计算阻力
        rb.angularDrag = 0f;  // 我们自己计算阻力
        
        // 惯性张量
        rb.inertiaTensor = new Vector3(pitchInertia, yawInertia, rollInertia) * 1000f;
        rb.inertiaTensorRotation = Quaternion.identity;
        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    
    void Update()
    {
        UpdateReticle();
        HandleThrottle();
    }
    
    void FixedUpdate()
    {
        // 检测键盘输入
        bool aKey = Input.GetKey(KeyCode.A);
        bool dKey = Input.GetKey(KeyCode.D);
        bool qKey = Input.GetKey(KeyCode.Q);
        bool eKey = Input.GetKey(KeyCode.E);
        bool wKey = Input.GetKey(KeyCode.W);
        bool sKey = Input.GetKey(KeyCode.S);
        bool shiftKey = Input.GetKey(KeyCode.LeftShift);
        
        keyboardActive = aKey || dKey || qKey || eKey || wKey || sKey || shiftKey;
        
        // 计算空气动力学
        CalculateAerodynamics();
        
        // 计算控制输入
        float rollInput = 0f;
        float yawInput = 0f;
        float pitchInput = 0f;
        
        // 键盘控制（叠加）
        if (aKey) rollInput -= 1f;
        if (dKey) rollInput += 1f;
        if (qKey) yawInput -= 1f;
        if (eKey) yawInput += 1f;
        
        // 鼠标控制（万能 - 始终生效）
        // 瞄准环控制机体转向
        pitchInput += -reticleOffset.y / maxReticleOffset * pitchSpeed;
        yawInput += reticleOffset.x / maxReticleOffset * yawSpeed;
        
        // 应用控制（带惯性的平滑）
        ApplyControls(pitchInput, yawInput, rollInput);
        
        // 应用推力
        ApplyThrust();
        
        // 速度限制
        if (rb.velocity.magnitude > maxSpeed)
        {
            rb.velocity = rb.velocity.normalized * maxSpeed;
        }
    }
    
    void CalculateAerodynamics()
    {
        float speed = rb.velocity.magnitude;
        Vector3 velocity = rb.velocity;
        
        // 相对于飞机的速度
        Vector3 localVelocity = transform.InverseTransformDirection(velocity);
        
        // 迎角（Angle of Attack）
        float aoa = 0f;
        if (Mathf.Abs(localVelocity.z) > 0.1f)
        {
            aoa = Mathf.Atan2(-localVelocity.y, localVelocity.z) * Mathf.Rad2Deg;
        }
        
        // 升力 = 0.5 * rho * v^2 * S * Cl
        float dynamicPressure = 0.5f * 1.225f * speed * speed; // 空气密度 * 速度^2
        float lift = dynamicPressure * liftCoefficient * (1f - Mathf.Abs(aoa) / 90f);
        
        // 失速特性
        float stallFactor = 1f;
        if (Mathf.Abs(aoa) > stallAngle)
        {
            stallFactor = 1f - (Mathf.Abs(aoa) - stallAngle) / (90f - stallAngle);
            stallFactor = Mathf.Clamp01(stallFactor);
        }
        
        // 阻力
        float drag = dynamicPressure * (parasiteDrag + inducedDrag * (aoa * aoa));
        
        // 升力方向（垂直于速度和飞机向上）
        Vector3 liftDir = Vector3.Cross(velocity.normalized, transform.right).normalized;
        if (velocity.sqrMagnitude < 0.1f) liftDir = transform.up;
        
        // 应用升力
        rb.AddForce(liftDir * lift * stallFactor);
        
        // 应用阻力（减速方向）
        rb.AddForce(-velocity.normalized * drag);
        
        // 失速时降低控制效率
        if (stallFactor < 0.5f)
        {
            controlSurfaceResponse *= stallFactor;
        }
    }
    
    void ApplyControls(float pitchInput, float yawInput, float rollInput)
    {
        // 协调转弯：自动根据偏航产生横滚
        float coordinatedRoll = -rb.angularVelocity.y * 0.8f;
        
        // 混合横滚输入和协调横滚
        if (Mathf.Abs(rollInput) < 0.1f)
        {
            rollInput = coordinatedRoll;
        }
        
        // 平滑控制
        Vector3 targetAngularVel = new Vector3(
            pitchInput * pitchSpeed,
            yawInput * yawSpeed,
            rollInput * rollSpeed
        );
        
        // Lerp实现惯性
        angularVelocitySmooth = Vector3.Lerp(
            angularVelocitySmooth, 
            targetAngularVel, 
            Time.fixedDeltaTime * controlSurfaceResponse
        );
        
        rb.angularVelocity = angularVelocitySmooth;
        
        // 失速时自动配平
        float speed = rb.velocity.magnitude;
        if (speed < stallSpeed)
        {
            // 轻微自动俯仰修正
            float pitchCorrection = (stallSpeed - speed) / stallSpeed * 0.1f;
            rb.AddTorque(transform.right * pitchCorrection);
        }
    }
    
    void ApplyThrust()
    {
        // 节流控制
        float thrustMultiplier = Mathf.Lerp(idleThrust / maxThrust, 1f, currentThrottle);
        float thrust = maxThrust * thrustMultiplier;
        
        // 低速时保持最低推力（修复无法启动的问题）
        float speed = rb.velocity.magnitude;
        float speedFactor = Mathf.Max(0.3f, speed / 50f); // 最低30%推力
        thrust *= speedFactor;
        
        // 应用推力
        rb.AddForce(transform.forward * thrust);
    }
    
    void HandleThrottle()
    {
        // W/S 调整节流
        if (Input.GetKey(KeyCode.W))
        {
            currentThrottle = Mathf.Clamp01(currentThrottle + Time.deltaTime * 0.5f);
        }
        if (Input.GetKey(KeyCode.S))
        {
            currentThrottle = Mathf.Clamp01(currentThrottle - Time.deltaTime * 0.5f);
        }
        
        // Shift 应急推力
        if (Input.GetKey(KeyCode.LeftShift))
        {
            currentThrottle = 1f;
        }
    }
    
    void UpdateReticle()
    {
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");
        
        reticleOffset.x += mouseX * trackingSpeed * Time.deltaTime * 100f;
        reticleOffset.y += mouseY * trackingSpeed * Time.deltaTime * 100f;
        
        reticleOffset.x = Mathf.Clamp(reticleOffset.x, -maxReticleOffset, maxReticleOffset);
        reticleOffset.y = Mathf.Clamp(reticleOffset.y, -maxReticleOffset, maxReticleOffset);
        
        // 自动回正
        if (Mathf.Abs(mouseX) < 0.01f) reticleOffset.x = Mathf.Lerp(reticleOffset.x, 0f, Time.deltaTime * returnSpeed);
        if (Mathf.Abs(mouseY) < 0.01f) reticleOffset.y = Mathf.Lerp(reticleOffset.y, 0f, Time.deltaTime * returnSpeed);
    }
    
    public Vector2 GetReticleOffset() => reticleOffset;
    public float GetThrottle() => currentThrottle;
    public float GetSpeed() => rb.velocity.magnitude;
    public float GetStallFactor()
    {
        Vector3 localVel = transform.InverseTransformDirection(rb.velocity);
        float aoa = 0f;
        if (Mathf.Abs(localVel.z) > 0.1f)
        {
            aoa = Mathf.Atan2(-localVel.y, localVel.z) * Mathf.Rad2Deg;
        }
        if (Mathf.Abs(aoa) > stallAngle)
        {
            return 1f - (Mathf.Abs(aoa) - stallAngle) / (90f - stallAngle);
        }
        return 1f;
    }
    public bool IsKeyboardActive() => keyboardActive;
    
    void OnDestroy()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
