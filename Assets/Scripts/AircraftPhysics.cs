using UnityEngine;

public class AircraftPhysics : MonoBehaviour
{
    [Header("空气动力学参数")]
    public float wingArea = 20f;
    public float liftCoefficient = 1.2f;
    public float dragCoefficient = 0.02f;
    public float inducedDragCoefficient = 0.05f;
    
    [Header("稳定性参数")]
    public float pitchStability = 0.1f;
    public float rollStability = 0.15f;
    public float yawStability = 0.08f;
    
    [Header("飞行包线")]
    public float stallAngle = 15f;
    public float criticalMach = 0.85f;
    
    private Rigidbody rb;
    private float airDensity = 1.225f;
    
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }
    
    void FixedUpdate()
    {
        UpdateAirDensity();
        ApplyAerodynamicForces();
        ApplyStability();
    }
    
    void UpdateAirDensity()
    {
        float altitude = transform.position.y;
        airDensity = 1.225f * Mathf.Pow(1 - altitude / 44330f, 4.256f);
        
        if (altitude > 44330f)
            airDensity = 0.001f;
    }
    
    void ApplyAerodynamicForces()
    {
        Vector3 airVelocity = -rb.velocity;
        float airspeed = airVelocity.magnitude;
        
        if (airspeed < 1f) return;
        
        float dynamicPressure = 0.5f * airDensity * airspeed * airspeed;
        
        float alpha = CalculateAngleOfAttack(airVelocity);
        float alphaRad = Mathf.Deg2Rad * alpha;
        
        float actualLiftCoeff = CalculateLiftCoefficient(alpha);
        float actualDragCoeff = CalculateDragCoefficient(alpha, airspeed);
        
        Vector3 liftForce = CalculateLift(actualLiftCoeff, dynamicPressure, alphaRad);
        Vector3 dragForce = CalculateDrag(actualDragCoeff, dynamicPressure, airVelocity);
        
        rb.AddForce(liftForce + dragForce);
    }
    
    float CalculateAngleOfAttack(Vector3 airVelocity)
    {
        float dot = Vector3.Dot(transform.forward, airVelocity.normalized);
        float angle = Mathf.Acos(Mathf.Clamp(dot, -1f, 1f));
        return Mathf.Rad2Deg * angle;
    }
    
    float CalculateLiftCoefficient(float alpha)
    {
        if (Mathf.Abs(alpha) >= stallAngle)
            return liftCoefficient * 0.3f;
        
        return liftCoefficient * (1 + alpha * 0.02f);
    }
    
    float CalculateDragCoefficient(float alpha, float airspeed)
    {
        float mach = airspeed / 343f;
        float waveDrag = 0f;
        
        if (mach > criticalMach)
            waveDrag = Mathf.Pow((mach - criticalMach) / 0.15f, 3f) * 0.1f;
        
        float inducedDrag = inducedDragCoefficient * Mathf.Pow(Mathf.Sin(Mathf.Deg2Rad * alpha), 2);
        
        return dragCoefficient + inducedDrag + waveDrag;
    }
    
    Vector3 CalculateLift(float cl, float q, float alphaRad)
    {
        float lift = cl * q * wingArea;
        Vector3 liftDirection = Vector3.Cross(transform.right, -rb.velocity).normalized;
        return liftDirection * lift;
    }
    
    Vector3 CalculateDrag(float cd, float q, Vector3 airVelocity)
    {
        float drag = cd * q * wingArea;
        return -airVelocity.normalized * drag;
    }
    
    void ApplyStability()
    {
        Vector3 localVelocity = transform.InverseTransformDirection(rb.velocity);
        
        float pitchTorque = -localVelocity.z * pitchStability;
        float rollTorque = -localVelocity.x * rollStability;
        float yawTorque = -localVelocity.x * yawStability;
        
        rb.AddRelativeTorque(pitchTorque, yawTorque, rollTorque);
    }
    
    public float Airspeed => rb.velocity.magnitude;
    public float AngleOfAttack => CalculateAngleOfAttack(-rb.velocity);
    public float MachNumber => Airspeed / 343f;
    public float CurrentAirDensity => airDensity;
}
