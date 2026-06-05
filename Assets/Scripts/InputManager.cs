using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    private PlayerInput playerInput;
    private InputAction pitchAction;
    private InputAction rollAction;
    private InputAction yawAction;
    private InputAction throttleAction;
    private InputAction fireAction;
    private InputAction secondaryFireAction;
    private InputAction flapToggleAction;
    private InputAction flapUpAction;
    private InputAction flapDownAction;
    private InputAction landingGearToggleAction;
    private InputAction brakeAction;
    private InputAction afterburnerToggleAction;
    private InputAction engineToggleAction;
    private InputAction shieldToggleAction;
    private InputAction radarToggleAction;
    private InputAction commsToggleAction;
    private InputAction throttleUpAction;
    private InputAction throttleDownAction;
    
    private float pitch = 0f;
    private float roll = 0f;
    private float yaw = 0f;
    private float throttle = 0.5f;
    private float throttleInput = 0f;
    
    void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        
        if (playerInput == null)
        {
            playerInput = gameObject.AddComponent<PlayerInput>();
            playerInput.actions = CreateInputActions();
        }
        
        BindActions();
    }
    
    void BindActions()
    {
        pitchAction = playerInput.actions["Pitch"];
        rollAction = playerInput.actions["Roll"];
        yawAction = playerInput.actions["Yaw"];
        throttleAction = playerInput.actions["Throttle"];
        fireAction = playerInput.actions["Fire"];
        secondaryFireAction = playerInput.actions["SecondaryFire"];
        flapToggleAction = playerInput.actions["FlapToggle"];
        flapUpAction = playerInput.actions["FlapUp"];
        flapDownAction = playerInput.actions["FlapDown"];
        landingGearToggleAction = playerInput.actions["LandingGearToggle"];
        brakeAction = playerInput.actions["Brake"];
        afterburnerToggleAction = playerInput.actions["AfterburnerToggle"];
        engineToggleAction = playerInput.actions["EngineToggle"];
        shieldToggleAction = playerInput.actions["ShieldToggle"];
        radarToggleAction = playerInput.actions["RadarToggle"];
        commsToggleAction = playerInput.actions["CommsToggle"];
        throttleUpAction = playerInput.actions["ThrottleUp"];
        throttleDownAction = playerInput.actions["ThrottleDown"];
    }
    
    InputActionMap CreateInputActions()
    {
        var actionMap = new InputActionMap("FlightControls");
        
        actionMap.AddAction("Pitch", 
            type: InputActionType.Value, 
            binding: "<Mouse>/y",
            interactions: "Invert");
        actionMap.AddAction("Roll", 
            type: InputActionType.Value, 
            binding: "<Mouse>/x");
        actionMap.AddAction("Yaw", 
            type: InputActionType.Value, 
            binding: "<Keyboard>/a,<Keyboard>/d");
        actionMap.AddAction("Throttle", 
            type: InputActionType.Value, 
            binding: "<Keyboard>/w,<Keyboard>/s",
            interactions: "Invert");
        actionMap.AddAction("Fire", 
            type: InputActionType.Button, 
            binding: "<Mouse>/leftButton");
        actionMap.AddAction("SecondaryFire", 
            type: InputActionType.Button, 
            binding: "<Mouse>/rightButton");
        actionMap.AddAction("FlapToggle", 
            type: InputActionType.Button, 
            binding: "<Keyboard>/f");
        actionMap.AddAction("FlapUp", 
            type: InputActionType.Button, 
            binding: "<Keyboard>/pageUp");
        actionMap.AddAction("FlapDown", 
            type: InputActionType.Button, 
            binding: "<Keyboard>/pageDown");
        actionMap.AddAction("LandingGearToggle", 
            type: InputActionType.Button, 
            binding: "<Keyboard>/g");
        actionMap.AddAction("Brake", 
            type: InputActionType.Button, 
            binding: "<Keyboard>/space");
        actionMap.AddAction("AfterburnerToggle", 
            type: InputActionType.Button, 
            binding: "<Keyboard>/leftShift");
        actionMap.AddAction("EngineToggle", 
            type: InputActionType.Button, 
            binding: "<Keyboard>/e");
        actionMap.AddAction("ShieldToggle", 
            type: InputActionType.Button, 
            binding: "<Keyboard>/s");
        actionMap.AddAction("RadarToggle", 
            type: InputActionType.Button, 
            binding: "<Keyboard>/r");
        actionMap.AddAction("CommsToggle", 
            type: InputActionType.Button, 
            binding: "<Keyboard>/c");
        actionMap.AddAction("ThrottleUp", 
            type: InputActionType.Button, 
            binding: "<Keyboard>/w");
        actionMap.AddAction("ThrottleDown", 
            type: InputActionType.Button, 
            binding: "<Keyboard>/s");
        
        return actionMap;
    }
    
    void Update()
    {
        pitch = pitchAction.ReadValue<float>();
        roll = rollAction.ReadValue<float>();
        yaw = yawAction.ReadValue<float>();
        
        float throttleUpInput = throttleUpAction.IsPressed() ? 1f : 0f;
        float throttleDownInput = throttleDownAction.IsPressed() ? -1f : 0f;
        throttleInput = throttleUpInput + throttleDownInput;
        
        throttle = Mathf.Clamp01(throttle + throttleInput * Time.deltaTime * 0.5f);
    }
    
    public float Pitch => pitch;
    public float Roll => roll;
    public float Yaw => yaw;
    public float Throttle => throttle;
    public float ThrottleInput => throttleInput;
    public bool Fire => fireAction.IsPressed();
    public bool SecondaryFire => secondaryFireAction.IsPressed();
    public bool FlapToggle => flapToggleAction.WasPressedThisFrame();
    public bool FlapUp => flapUpAction.IsPressed();
    public bool FlapDown => flapDownAction.IsPressed();
    public bool LandingGearToggle => landingGearToggleAction.WasPressedThisFrame();
    public bool Brake => brakeAction.IsPressed();
    public bool AfterburnerToggle => afterburnerToggleAction.WasPressedThisFrame();
    public bool AfterburnerActive => afterburnerToggleAction.IsPressed();
    public bool EngineToggle => engineToggleAction.WasPressedThisFrame();
    public bool ShieldToggle => shieldToggleAction.WasPressedThisFrame();
    public bool RadarToggle => radarToggleAction.WasPressedThisFrame();
    public bool CommsToggle => commsToggleAction.WasPressedThisFrame();
    
    void OnEnable()
    {
        playerInput?.Enable();
    }
    
    void OnDisable()
    {
        playerInput?.Disable();
    }
}
