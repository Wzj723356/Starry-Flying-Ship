using UnityEngine;

public class TestFlight : MonoBehaviour
{
    public float speed = 50f;
    public float rotationSpeed = 2f;
    
    private Rigidbody rb;
    
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
            rb = gameObject.AddComponent<Rigidbody>();
        rb.useGravity = false;
        rb.drag = 0.1f;
    }
    
    void FixedUpdate()
    {
        // 前进/后退
        float forward = Input.GetAxis("Vertical");
        rb.AddForce(transform.forward * forward * speed * 10f);
        
        // 左右旋转
        float turn = Input.GetAxis("Horizontal");
        transform.Rotate(0, turn * rotationSpeed, 0);
        
        // 上升/下降
        if (Input.GetKey(KeyCode.Space))
            rb.AddForce(Vector3.up * speed * 5f);
        if (Input.GetKey(KeyCode.LeftControl))
            rb.AddForce(Vector3.down * speed * 5f);
    }
}