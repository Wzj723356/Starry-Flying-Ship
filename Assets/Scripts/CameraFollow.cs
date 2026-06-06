using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0, 20f, -30f);
    public float followSpeed = 5f;
    
    void Start()
    {
        if (target == null)
        {
            FindTarget();
        }
    }
    
    void FindTarget()
    {
        GameObject ship = GameObject.Find("PlayerShip");
        if (ship != null)
        {
            target = ship.transform;
        }
    }
    
    void LateUpdate()
    {
        if (target == null)
        {
            FindTarget();
            return;
        }
        
        Vector3 desiredPosition = target.position + target.rotation * offset;
        transform.position = Vector3.Lerp(transform.position, desiredPosition, Time.deltaTime * followSpeed);
        
        Vector3 lookAtPoint = target.position + target.forward * 10f;
        transform.LookAt(lookAtPoint);
    }
}