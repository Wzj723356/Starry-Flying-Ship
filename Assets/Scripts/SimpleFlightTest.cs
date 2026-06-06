using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class SimpleFlightTest : MonoBehaviour
{
    public float maxThrust = 50000f;
    public float maxSpeed = 300f;
    public float rotationSpeed = 5f;
    
    private Rigidbody rb;
    
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.mass = 2000f;
        rb.useGravity = false;
        rb.drag = 0.1f;
        rb.angularDrag = 0.5f;
    }
    
    void FixedUpdate()
    {
        float throttle = Input.GetAxis("Vertical");
        float vertical = Input.GetKey(KeyCode.Space) ? 1f : (Input.GetKey(KeyCode.LeftControl) ? -1f : 0f);
        
        // 推力
        rb.AddForce(transform.forward * throttle * maxThrust);
        rb.AddForce(Vector3.up * vertical * maxThrust * 0.5f);
        
        // 旋转
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");
        float roll = Input.GetKey(KeyCode.Q) ? -1f : (Input.GetKey(KeyCode.E) ? 1f : 0f);
        
        rb.AddTorque(transform.right * -mouseY * rotationSpeed * 50f);
        rb.AddTorque(transform.up * mouseX * rotationSpeed * 50f);
        rb.AddTorque(transform.forward * roll * rotationSpeed * 50f);
        
        // 速度限制
        if (rb.velocity.magnitude > maxSpeed)
        {
            rb.velocity = rb.velocity.normalized * maxSpeed;
        }
    }
}