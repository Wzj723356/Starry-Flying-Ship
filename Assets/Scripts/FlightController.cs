using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(AircraftPhysics))]
public class FlightController : MonoBehaviour
{
    [Header("飞行参数")]
    public float maxThrust = 15000f;
    public float maxSpeed = 800f;
    public float maxAltitude = 12000f;
    
    [Header("操控灵敏度")]
    public float pitchSensitivity = 2f;
    public float rollSensitivity = 3f;
    public float yawSensitivity = 1.5f;
    
    [Header("襟翼设置")]
    public float flapExtensionSpeed = 5f;
    public float flapMaxExtension = 0.5f;
    
    private Rigidbody rb;
    private AircraftPhysics physics;
    private InputManager input;
    
    private float currentThrust = 0f;
    private float currentFlapAngle = 0f;
    private bool isLandingGearExtended = false;
    
    private float pitchInput = 0f;
    private float rollInput = 0f;
    private float yawInput = 0f;
    
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        physics = GetComponent<AircraftPhysics>();
        input = GetComponent<InputManager>();
        
        if (input == null)
            input = gameObject.AddComponent<InputManager>();
    }
    
    void Update()
    {
        ReadInputs();
        UpdateFlaps();
        UpdateLandingGear();
    }
    
    void FixedUpdate()
    {
        ApplyThrust();
        ApplyControlForces();
        ApplyDrag();
        ClampSpeedAndAltitude();
    }
    
    void ReadInputs()
    {
        pitchInput = input.Pitch;
        rollInput = input.Roll;
        yawInput = input.Yaw;
        currentThrust = Mathf.Clamp01(input.Throttle) * maxThrust;
        
        if (input.FlapToggle)
            ToggleFlaps();
        
        if (input.LandingGearToggle)
            ToggleLandingGear();
    }
    
    void ApplyThrust()
    {
        Vector3 thrustForce = transform.forward * currentThrust;
        rb.AddForce(thrustForce);
    }
    
    void ApplyControlForces()
    {
        float currentSpeed = rb.velocity.magnitude;
        float speedFactor = Mathf.Clamp01(currentSpeed / 100f);
        
        float pitchTorque = pitchInput * pitchSensitivity * speedFactor * Time.fixedDeltaTime * 100f;
        float rollTorque = rollInput * rollSensitivity * speedFactor * Time.fixedDeltaTime * 100f;
        float yawTorque = yawInput * yawSensitivity * speedFactor * Time.fixedDeltaTime * 100f;
        
        rb.AddTorque(transform.right * pitchTorque);
        rb.AddTorque(transform.forward * rollTorque);
        rb.AddTorque(transform.up * yawTorque);
    }
    
    void ApplyDrag()
    {
        float currentSpeed = rb.velocity.magnitude;
        float dragCoefficient = 0.01f + (currentFlapAngle * 0.02f);
        
        if (isLandingGearExtended)
            dragCoefficient += 0.03f;
        
        Vector3 dragForce = -rb.velocity.normalized * currentSpeed * currentSpeed * dragCoefficient;
        rb.AddForce(dragForce);
    }
    
    void ClampSpeedAndAltitude()
    {
        if (rb.velocity.magnitude > maxSpeed)
        {
            rb.velocity = rb.velocity.normalized * maxSpeed;
        }
        
        if (transform.position.y > maxAltitude)
        {
            Vector3 pos = transform.position;
            pos.y = maxAltitude;
            transform.position = pos;
            
            if (rb.velocity.y > 0)
            {
                Vector3 vel = rb.velocity;
                vel.y = 0;
                rb.velocity = vel;
            }
        }
    }
    
    void UpdateFlaps()
    {
        float targetFlapAngle = currentFlapAngle;
        
        if (input.FlapUp)
            targetFlapAngle = 0f;
        else if (input.FlapDown)
            targetFlapAngle = flapMaxExtension;
        
        currentFlapAngle = Mathf.MoveTowards(currentFlapAngle, targetFlapAngle, flapExtensionSpeed * Time.deltaTime);
    }
    
    void ToggleFlaps()
    {
        currentFlapAngle = currentFlapAngle > 0 ? 0f : flapMaxExtension;
    }
    
    void UpdateLandingGear()
    {
    }
    
    void ToggleLandingGear()
    {
        isLandingGearExtended = !isLandingGearExtended;
    }
    
    public float CurrentSpeed => rb.velocity.magnitude;
    public float CurrentAltitude => transform.position.y;
    public float ThrustPercentage => currentThrust / maxThrust;
    public float FlapAngle => currentFlapAngle;
    public bool LandingGearExtended => isLandingGearExtended;
}
