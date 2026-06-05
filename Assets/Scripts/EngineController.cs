using UnityEngine;
using UnityEngine.Rendering;

public class EngineController : MonoBehaviour
{
    [Header("引擎参数")]
    public float thrustForce = 5000f;
    public float fuelConsumption = 1.0f;
    public float ignitionTime = 0.5f;
    public float shutdownTime = 0.3f;
    
    [Header("音效")]
    public AudioSource engineAudio;
    public AudioClip engineIdleSound;
    public AudioClip engineThrustSound;
    public AudioClip afterburnerSound;
    
    [Header("视觉特效")]
    public ParticleSystem engineParticles;
    public ParticleSystem afterburnerParticles;
    public Light engineLight;
    public float maxEngineGlow = 2f;
    
    [Header("RTX支持")]
    public bool enableRTXEmission = true;
    public Color engineEmissionColor = new Color(0.3f, 0.5f, 1f);
    public float engineEmissionIntensity = 5f;
    
    private float currentThrustPercentage = 0f;
    private bool isAfterburnerActive = false;
    private bool isIgnited = false;
    private Renderer engineRenderer;
    private Material engineMaterial;
    
    void Awake()
    {
        InitializeEngine();
    }
    
    void InitializeEngine()
    {
        if (engineParticles == null)
        {
            CreateEngineParticles();
        }
        
        if (engineLight == null)
        {
            CreateEngineLight();
        }
        
        if (engineAudio == null)
        {
            CreateEngineAudio();
        }
        
        SetupRTXMaterial();
        
        DisableEffects();
    }
    
    void CreateEngineParticles()
    {
        GameObject particlesObj = new GameObject("EngineParticles");
        particlesObj.transform.parent = transform;
        particlesObj.transform.localPosition = Vector3.zero;
        particlesObj.transform.localRotation = Quaternion.Euler(90, 0, 0);
        
        engineParticles = particlesObj.AddComponent<ParticleSystem>();
        
        var main = engineParticles.main;
        main.startLifetime = 0.5f;
        main.startSpeed = 50f;
        main.startSize = 1f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = 1000;
        
        var emission = engineParticles.emission;
        emission.rateOverTime = 100;
        
        var shape = engineParticles.shape;
        shape.shapeType = ParticleSystemShapeType.Cone;
        shape.angle = 15f;
        shape.radius = 0.1f;
        
        var colorOverLifetime = engineParticles.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(Color.blue, 0f), new GradientColorKey(Color.cyan, 1f) },
            new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) }
        );
        colorOverLifetime.color = gradient;
        
        engineParticles.Stop();
    }
    
    void CreateAfterburnerParticles()
    {
        if (afterburnerParticles != null) return;
        
        GameObject afterburnerObj = new GameObject("AfterburnerParticles");
        afterburnerObj.transform.parent = transform;
        afterburnerObj.transform.localPosition = Vector3.zero;
        afterburnerObj.transform.localRotation = Quaternion.Euler(90, 0, 0);
        
        afterburnerParticles = afterburnerObj.AddComponent<ParticleSystem>();
        
        var main = afterburnerParticles.main;
        main.startLifetime = 1f;
        main.startSpeed = 100f;
        main.startSize = 2f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.maxParticles = 2000;
        
        var emission = afterburnerParticles.emission;
        emission.rateOverTime = 500;
        
        var colorOverLifetime = afterburnerParticles.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.yellow, 0.5f), new GradientColorKey(Color.red, 1f) },
            new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0.5f, 0.5f), new GradientAlphaKey(0f, 1f) }
        );
        colorOverLifetime.color = gradient;
        
        afterburnerParticles.Stop();
    }
    
    void CreateEngineLight()
    {
        GameObject lightObj = new GameObject("EngineLight");
        lightObj.transform.parent = transform;
        lightObj.transform.localPosition = Vector3.zero;
        
        engineLight = lightObj.AddComponent<Light>();
        engineLight.type = LightType.Point;
        engineLight.color = engineEmissionColor;
        engineLight.intensity = 0;
        engineLight.range = 50f;
        engineLight.shadows = LightShadows.Soft;
        engineLight.shadowResolution = UnityEngine.Rendering.LightShadowResolution.High;
        engineLight.shadowBias = 0.1f;
        engineLight.normalizedShadowAnchor = transform;
    }
    
    void CreateEngineAudio()
    {
        engineAudio = gameObject.AddComponent<AudioSource>();
        engineAudio.loop = true;
        engineAudio.playOnAwake = false;
        
        if (engineIdleSound != null)
        {
            engineAudio.clip = engineIdleSound;
        }
    }
    
    void SetupRTXMaterial()
    {
        engineRenderer = GetComponent<Renderer>();
        if (engineRenderer == null)
        {
            engineRenderer = gameObject.AddComponent<MeshRenderer>();
        }
        
        engineMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        
        if (enableRTXEmission)
        {
            engineMaterial.EnableKeyword("_EMISSION");
            engineMaterial.SetColor("_EmissionColor", engineEmissionColor * engineEmissionIntensity);
        }
        
        engineRenderer.material = engineMaterial;
    }
    
    void Update()
    {
        UpdateEffects();
        UpdateAudio();
    }
    
    void UpdateEffects()
    {
        float thrustIntensity = currentThrustPercentage;
        
        if (engineParticles != null)
        {
            if (thrustIntensity > 0.1f && !engineParticles.isPlaying)
            {
                engineParticles.Play();
            }
            else if (thrustIntensity <= 0.1f && engineParticles.isPlaying)
            {
                engineParticles.Stop();
            }
            
            var main = engineParticles.main;
            main.startSpeed = 50f * thrustIntensity;
            main.startSize = 1f * thrustIntensity;
        }
        
        if (isAfterburnerActive && afterburnerParticles != null)
        {
            if (!afterburnerParticles.isPlaying)
            {
                afterburnerParticles.Play();
            }
            
            var main = afterburnerParticles.main;
            main.startSpeed = 100f * thrustIntensity;
            main.startSize = 2f * thrustIntensity;
        }
        else if (afterburnerParticles != null && afterburnerParticles.isPlaying)
        {
            afterburnerParticles.Stop();
        }
        
        if (engineLight != null)
        {
            float lightIntensity = thrustIntensity * maxEngineGlow;
            
            if (isAfterburnerActive)
            {
                lightIntensity *= 2f;
                engineLight.color = Color.yellow;
            }
            else
            {
                engineLight.color = engineEmissionColor;
            }
            
            engineLight.intensity = lightIntensity;
        }
        
        if (engineMaterial != null && enableRTXEmission)
        {
            float emissionIntensity = thrustIntensity * engineEmissionIntensity;
            
            if (isAfterburnerActive)
            {
                emissionIntensity *= 3f;
                engineMaterial.SetColor("_EmissionColor", Color.yellow * emissionIntensity);
            }
            else
            {
                engineMaterial.SetColor("_EmissionColor", engineEmissionColor * emissionIntensity);
            }
        }
    }
    
    void UpdateAudio()
    {
        if (engineAudio == null) return;
        
        if (currentThrustPercentage > 0.1f && !engineAudio.isPlaying)
        {
            engineAudio.Play();
        }
        else if (currentThrustPercentage <= 0.1f && engineAudio.isPlaying)
        {
            engineAudio.Stop();
        }
        
        engineAudio.pitch = 0.5f + currentThrustPercentage * 0.5f;
        engineAudio.volume = currentThrustPercentage * 0.8f;
    }
    
    void DisableEffects()
    {
        if (engineParticles != null && engineParticles.isPlaying)
        {
            engineParticles.Stop();
        }
        
        if (afterburnerParticles != null && afterburnerParticles.isPlaying)
        {
            afterburnerParticles.Stop();
        }
        
        if (engineLight != null)
        {
            engineLight.intensity = 0;
        }
    }
    
    public void SetThrustPercentage(float percentage)
    {
        currentThrustPercentage = Mathf.Clamp01(percentage);
    }
    
    public void SetAfterburner(bool active)
    {
        if (active && afterburnerParticles == null)
        {
            CreateAfterburnerParticles();
        }
        
        isAfterburnerActive = active;
    }
    
    public float GetCurrentThrust()
    {
        return thrustForce * currentThrustPercentage;
    }
    
    public float GetFuelConsumption()
    {
        return fuelConsumption * currentThrustPercentage * Time.deltaTime;
    }
    
    public bool IsIgnited => isIgnited;
    public bool IsAfterburnerActive => isAfterburnerActive;
}
