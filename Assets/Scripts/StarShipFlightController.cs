using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class StarShipFlightController : MonoBehaviour
{
    [Header("引擎参数")]
    public float maxThrust = 50000f;
    public float maxSpeed = 300f;
    public float acceleration = 20f;
    
    [Header("机动参数")]
    public float pitchRate = 5f;
    public float yawRate = 3f;
    public float rollRate = 5f;
    
    [Header("物理参数")]
    public float mass = 2000f;
    public float dragCoefficient = 0.1f;
    public float liftCoefficient = 0.5f;
    
    private Rigidbody rb;
    private float currentThrottle = 0f;
    
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.mass = mass;
        rb.useGravity = false;
        rb.drag = 0.1f;
        rb.angularDrag = 0.5f;
    }
    
    void FixedUpdate()
    {
        HandleInput();
        ApplyThrust();
        ApplyAerodynamics();
        LimitSpeed();
    }
    
    void HandleInput()
    {
        // 节流控制
        float throttleInput = Input.GetAxis("Vertical");
        currentThrottle = Mathf.Clamp01(currentThrottle + throttleInput * Time.deltaTime);
        
        // 旋转控制
        float pitch = Input.GetAxis("Mouse Y") * pitchRate;
        float yaw = Input.GetAxis("Mouse X") * yawRate;
        float roll = 0f;
        
        if (Input.GetKey(KeyCode.Q)) roll = -rollRate;
        if (Input.GetKey(KeyCode.E)) roll = rollRate;
        
        rb.AddTorque(transform.right * -pitch * 50f);
        rb.AddTorque(transform.up * yaw * 50f);
        rb.AddTorque(transform.forward * roll * 50f);
        
        // 垂直控制
        if (Input.GetKey(KeyCode.Space))
            rb.AddForce(Vector3.up * maxThrust * 0.5f);
        if (Input.GetKey(KeyCode.LeftControl))
            rb.AddForce(Vector3.down * maxThrust * 0.5f);
    }
    
    void ApplyThrust()
    {
        Vector3 thrust = transform.forward * currentThrottle * maxThrust;
        rb.AddForce(thrust);
    }
    
    void ApplyAerodynamics()
    {
        // 空气阻力
        float speed = rb.velocity.magnitude;
        float dragForce = 0.5f * 1.225f * speed * speed * dragCoefficient * 10f;
        Vector3 drag = -rb.velocity.normalized * dragForce;
        rb.AddForce(drag);
        
        // 升力
        if (speed > 10f)
        {
            Vector3 lift = transform.up * liftCoefficient * speed * 10f;
            rb.AddForce(lift);
        }
    }
    
    void LimitSpeed()
    {
        if (rb.velocity.magnitude > maxSpeed)
        {
            rb.velocity = rb.velocity.normalized * maxSpeed;
        }
    }
    
    public float GetCurrentSpeed() => rb.velocity.magnitude;
    public float GetThrottle() => currentThrottle;
}