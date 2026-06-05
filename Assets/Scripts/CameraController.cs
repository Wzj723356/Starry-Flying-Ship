using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform target;
    public float distance = 30f;
    public float height = 10f;
    public float minDistance = 10f;
    public float maxDistance = 100f;
    public float minVerticalAngle = -80f;
    public float maxVerticalAngle = 80f;
    public float followSpeed = 5f;
    public float rotationSpeed = 2f;
    
    private float currentDistance;
    private float currentHeight;
    private float currentRotationX = 0f;
    private float currentRotationY = 0f;
    
    void Start()
    {
        if (target != null)
        {
            Vector3 angles = transform.eulerAngles;
            currentRotationX = angles.y;
            currentRotationY = angles.x;
        }
        
        currentDistance = distance;
        currentHeight = height;
    }
    
    void LateUpdate()
    {
        if (target == null) return;
        
        HandleInput();
        UpdateCameraPosition();
    }
    
    void HandleInput()
    {
        if (Input.GetMouseButton(1))
        {
            currentRotationY += Input.GetAxis("Mouse X") * rotationSpeed;
            currentRotationX -= Input.GetAxis("Mouse Y") * rotationSpeed;
            
            currentRotationX = Mathf.Clamp(currentRotationX, minVerticalAngle, maxVerticalAngle);
        }
        
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        distance -= scroll * 5f;
        distance = Mathf.Clamp(distance, minDistance, maxDistance);
    }
    
    void UpdateCameraPosition()
    {
        float rotationY = Quaternion.Euler(0, currentRotationY, 0).eulerAngles.y;
        Quaternion rotation = Quaternion.Euler(currentRotationX, currentRotationY, 0);
        
        Vector3 offset = rotation * new Vector3(0, 0, -distance);
        offset.y += height;
        
        Vector3 targetPosition = target.position + offset;
        
        transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);
        transform.LookAt(target.position + Vector3.up * height * 0.5f);
    }
    
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
    
    public void ResetCamera()
    {
        if (target != null)
        {
            currentRotationY = target.eulerAngles.y;
            currentRotationX = 30f;
            distance = 30f;
            height = 10f;
        }
    }
    
    public void ToggleFollowMode()
    {
        enabled = !enabled;
    }
}
