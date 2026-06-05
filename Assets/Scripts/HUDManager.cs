using UnityEngine;
using UnityEngine.UI;

public class HUDManager : MonoBehaviour
{
    [Header("HUD元素")]
    public Text speedText;
    public Text altitudeText;
    public Text throttleText;
    public Text healthText;
    public Text ammoText;
    public Text scoreText;
    public Image crosshair;
    public Image radar;
    
    [Header("颜色设置")]
    public Color lowHealthColor = Color.red;
    public Color normalHealthColor = Color.green;
    
    private FlightController flightController;
    private Damageable damageable;
    private WeaponSystem weaponSystem;
    private GameManager gameManager;
    
    void Awake()
    {
        flightController = FindObjectOfType<FlightController>();
        damageable = FindObjectOfType<Damageable>();
        weaponSystem = FindObjectOfType<WeaponSystem>();
        gameManager = GameManager.instance;
    }
    
    void Update()
    {
        UpdateFlightData();
        UpdateHealth();
        UpdateAmmo();
        UpdateScore();
    }
    
    void UpdateFlightData()
    {
        if (flightController != null)
        {
            speedText.text = $"速度: {Mathf.Round(flightController.CurrentSpeed * 3.6f)} km/h";
            altitudeText.text = $"高度: {Mathf.Round(flightController.CurrentAltitude)} m";
            throttleText.text = $"油门: {Mathf.Round(flightController.ThrustPercentage * 100)}%";
        }
    }
    
    void UpdateHealth()
    {
        if (damageable != null)
        {
            float healthPercent = damageable.HealthPercentage;
            healthText.text = $"生命值: {Mathf.Round(damageable.currentHealth)}/{Mathf.Round(damageable.maxHealth)}";
            healthText.color = healthPercent < 0.3f ? lowHealthColor : normalHealthColor;
        }
    }
    
    void UpdateAmmo()
    {
        if (weaponSystem != null)
        {
            int primaryAmmo = weaponSystem.GetPrimaryAmmo();
            int secondaryAmmo = weaponSystem.GetSecondaryAmmo();
            ammoText.text = $"弹药: 主武器 {primaryAmmo} | 副武器 {secondaryAmmo}";
        }
    }
    
    void UpdateScore()
    {
        if (gameManager != null)
        {
            scoreText.text = $"得分: {gameManager.GetScore(0)}";
        }
    }
}
