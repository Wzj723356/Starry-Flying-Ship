using UnityEngine;

public class InputManager : MonoBehaviour
{
    [Header("=== 灵敏度设置 ===")]
    public float mouseSensitivity = 1f;
    public float throttleSensitivity = 0.5f;
    
    private float pitch = 0f;
    private float roll = 0f;
    private float yaw = 0f;
    private float throttle = 0.5f;
    private float throttleInput = 0f;
    
    void Update()
    {
        // 鼠标输入
        pitch = Input.GetAxis("Mouse Y") * mouseSensitivity;
        yaw = Input.GetAxis("Mouse X") * mouseSensitivity;
        roll = (Input.GetKey(KeyCode.Q) ? -1f : 0f) + (Input.GetKey(KeyCode.E) ? 1f : 0f);
        
        // 键盘节流控制
        throttleInput = 0f;
        if (Input.GetKey(KeyCode.W)) throttleInput += 1f;
        if (Input.GetKey(KeyCode.S)) throttleInput -= 1f;
        
        throttle = Mathf.Clamp01(throttle + throttleInput * Time.deltaTime * throttleSensitivity);
    }
    
    public float Pitch => pitch;
    public float Roll => roll;
    public float Yaw => yaw;
    public float Throttle => throttle;
    public float ThrottleInput => throttleInput;
    
    // 按钮输入
    public bool Fire => Input.GetMouseButton(0);
    public bool SecondaryFire => Input.GetMouseButton(1);
    public bool FlapToggle => Input.GetKeyDown(KeyCode.F);
    public bool LandingGearToggle => Input.GetKeyDown(KeyCode.G);
    public bool Brake => Input.GetKey(KeyCode.Space);
    public bool AfterburnerToggle => Input.GetKeyDown(KeyCode.LeftShift);
    public bool AfterburnerActive => Input.GetKey(KeyCode.LeftShift);
    public bool EngineToggle => Input.GetKeyDown(KeyCode.E);
    public bool ShieldToggle => Input.GetKeyDown(KeyCode.X);
    public bool RadarToggle => Input.GetKeyDown(KeyCode.R);
    public bool CommsToggle => Input.GetKeyDown(KeyCode.C);
    
    // 基础移动输入
    public float Horizontal => Input.GetAxis("Horizontal");
    public float Vertical => Input.GetAxis("Vertical");
    
    // 辅助输入
    public bool LookUp => Input.GetKey(KeyCode.UpArrow);
    public bool LookDown => Input.GetKey(KeyCode.DownArrow);
    public bool LookLeft => Input.GetKey(KeyCode.LeftArrow);
    public bool LookRight => Input.GetKey(KeyCode.RightArrow);
    
    public bool LockTarget => Input.GetKey(KeyCode.Tab);
    public bool LaunchMissile => Input.GetKeyDown(KeyCode.Space);
    public bool DeployChaff => Input.GetKeyDown(KeyCode.Alpha1);
    public bool DeployFlare => Input.GetKeyDown(KeyCode.Alpha2);
}