using UnityEngine;
using UnityEngine.Rendering;

public class InterstellarEnvironment : MonoBehaviour
{
    [Header("星球设置")]
    public string planetName = "Kepler-442b";
    public float planetRadius = 6371000f;
    public Vector3 planetPosition = Vector3.zero;
    public float gravity = 9.81f;
    
    [Header("大气层设置")]
    public bool hasAtmosphere = true;
    public float atmosphereHeight = 100000f;
    public float atmosphereDensity = 1.225f;
    public Color atmosphereColor = new Color(0.5f, 0.7f, 1.0f);
    public Color skyColor = new Color(0.3f, 0.5f, 0.9f);
    public Color horizonColor = new Color(0.9f, 0.8f, 0.7f);
    
    [Header("光照设置")]
    public Light sunLight;
    public Color sunColor = Color.white;
    public float sunIntensity = 1.5f;
    public Vector3 sunDirection = new Vector3(1, 0.5f, 0);
    
    [Header("星空设置")]
    public GameObject starField;
    public int starCount = 5000;
    public float starFieldRadius = 50000f;
    public bool dynamicStars = true;
    
    [Header("环境特效")]
    public bool enableFog = true;
    public Color fogColor = new Color(0.6f, 0.7f, 0.9f);
    public float fogDensity = 0.0001f;
    public float fogStartDistance = 1000f;
    public float fogEndDistance = 50000f;
    
    [Header("RTX光线追踪设置")]
    public bool enableRayTracing = true;
    public bool enableRTXReflections = true;
    public bool enableRTXShadows = true;
    public bool enableRTXGlobalIllumination = true;
    public bool enableRTXAmbientOcclusion = true;
    public RayTracingQuality rayTracingQuality = RayTracingQuality.Ultra;
    
    [Header("地形生成")]
    public GameObject terrainObject;
    public int terrainChunkSize = 1000;
    public int terrainChunkCount = 9;
    public float terrainHeightScale = 1000f;
    public Material terrainMaterial;
    
    void Awake()
    {
        SetupEnvironment();
        CreateSun();
        CreateStarField();
        CreateAtmosphere();
        GenerateTerrain();
        ConfigureLighting();
        ConfigureFog();
        ConfigureRayTracing();
    }
    
    void SetupEnvironment()
    {
        RenderSettings.ambientMode = AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = skyColor;
        RenderSettings.ambientEquatorColor = horizonColor;
        RenderSettings.ambientGroundColor = atmosphereColor;
        RenderSettings.ambientIntensity = 0.5f;
        
        Physics.gravity = Vector3.down * gravity;
    }
    
    void CreateSun()
    {
        if (sunLight != null) return;
        
        GameObject sunObj = new GameObject("Sun");
        sunObj.transform.parent = transform;
        sunObj.transform.position = sunDirection.normalized * 10000f;
        
        sunLight = sunObj.AddComponent<Light>();
        sunLight.type = LightType.Directional;
        sunLight.color = sunColor;
        sunLight.intensity = sunIntensity;
        sunLight.transform.rotation = Quaternion.LookRotation(-sunDirection.normalized);
        
        sunLight.shadows = enableRTXShadows ? LightShadows.Soft : LightShadows.None;
        sunLight.shadowResolution = UnityEngine.Rendering.LightShadowResolution.High;
        sunLight.shadowBias = 0.1f;
        sunLight.shadowNormalBias = 0.05f;
        sunLight.shadowNearPlane = 10f;
        sunLight.shadowFarPlane = 50000f;
        sunLight.shadowCullingMatrix = Matrix4x4.Ortho(-10000, 10000, -10000, 10000, 0, 10000) * sunLight.transform.worldToLocalMatrix;
    }
    
    void CreateStarField()
    {
        if (starField != null) return;
        
        GameObject starFieldObj = new GameObject("StarField");
        starFieldObj.transform.parent = transform;
        starFieldObj.transform.position = Vector3.zero;
        
        MeshFilter meshFilter = starFieldObj.AddComponent<MeshFilter>();
        MeshRenderer renderer = starFieldObj.AddComponent<MeshRenderer>();
        
        int starVertexCount = starCount * 4;
        Vector3[] vertices = new Vector3[starVertexCount];
        Vector2[] uvs = new Vector2[starVertexCount];
        int[] triangles = new int[starCount * 6];
        Color[] colors = new Color[starVertexCount];
        
        for (int i = 0; i < starCount; i++)
        {
            Vector3 starPosition = Random.onUnitSphere * starFieldRadius;
            float starSize = Random.Range(5f, 20f);
            
            Vector3 forward = starPosition.normalized;
            Vector3 up = Vector3.Cross(forward, Vector3.forward).normalized;
            if (up.magnitude < 0.1f)
            {
                up = Vector3.Cross(forward, Vector3.up).normalized;
            }
            Vector3 right = Vector3.Cross(up, forward).normalized;
            
            int baseIndex = i * 4;
            vertices[baseIndex] = starPosition - right * starSize - up * starSize;
            vertices[baseIndex + 1] = starPosition + right * starSize - up * starSize;
            vertices[baseIndex + 2] = starPosition + right * starSize + up * starSize;
            vertices[baseIndex + 3] = starPosition - right * starSize + up * starSize;
            
            uvs[baseIndex] = new Vector2(0, 0);
            uvs[baseIndex + 1] = new Vector2(1, 0);
            uvs[baseIndex + 2] = new Vector2(1, 1);
            uvs[baseIndex + 3] = new Vector2(0, 1);
            
            int triIndex = i * 6;
            triangles[triIndex] = baseIndex;
            triangles[triIndex + 1] = baseIndex + 1;
            triangles[triIndex + 2] = baseIndex + 2;
            triangles[triIndex + 3] = baseIndex;
            triangles[triIndex + 4] = baseIndex + 2;
            triangles[triIndex + 5] = baseIndex + 3;
            
            float brightness = Random.Range(0.3f, 1f);
            float temperature = Random.Range(0f, 1f);
            Color starColor = Color.Lerp(new Color(1f, 0.8f, 0.6f), new Color(0.6f, 0.8f, 1f), temperature) * brightness;
            
            colors[baseIndex] = starColor;
            colors[baseIndex + 1] = starColor;
            colors[baseIndex + 2] = starColor;
            colors[baseIndex + 3] = starColor;
        }
        
        Mesh mesh = new Mesh();
        mesh.name = "StarFieldMesh";
        mesh.vertices = vertices;
        mesh.uv = uvs;
        mesh.triangles = triangles;
        mesh.colors = colors;
        mesh.bounds = new Bounds(Vector3.zero, Vector3.one * starFieldRadius * 2);
        
        meshFilter.mesh = mesh;
        
        Material starMaterial = new Material(Shader.Find("Unlit/StarShader"));
        starMaterial.SetColor("_Color", Color.white);
        starMaterial.SetFloat("_Brightness", 1f);
        renderer.material = starMaterial;
        renderer.shadowCastingMode = ShadowCastingMode.Off;
        renderer.receiveShadows = false;
        
        starField = starFieldObj;
    }
    
    void CreateAtmosphere()
    {
        if (!hasAtmosphere) return;
        
        GameObject atmosphereObj = new GameObject("Atmosphere");
        atmosphereObj.transform.parent = transform;
        atmosphereObj.transform.position = planetPosition;
        atmosphereObj.transform.localScale = Vector3.one * (planetRadius + atmosphereHeight) * 2;
        
        MeshFilter meshFilter = atmosphereObj.AddComponent<MeshFilter>();
        MeshRenderer renderer = atmosphereObj.AddComponent<MeshRenderer>();
        
        Mesh sphere = Resources.GetBuiltinResource<Mesh>("Sphere.fbx");
        if (sphere != null)
        {
            meshFilter.mesh = sphere;
        }
        else
        {
            meshFilter.mesh = CreateProceduralSphere(32, 32);
        }
        
        Material atmosphereMaterial = new Material(Shader.Find("Custom/AtmosphereShader"));
        atmosphereMaterial.SetColor("_AtmosphereColor", atmosphereColor);
        atmosphereMaterial.SetColor("_SkyColor", skyColor);
        atmosphereMaterial.SetColor("_HorizonColor", horizonColor);
        atmosphereMaterial.SetFloat("_AtmosphereDensity", atmosphereDensity);
        atmosphereMaterial.SetFloat("_AtmosphereHeight", atmosphereHeight);
        
        if (enableRayTracing)
        {
            atmosphereMaterial.EnableKeyword("RTX_ATMOSPHERE");
        }
        
        renderer.material = atmosphereMaterial;
        renderer.shadowCastingMode = ShadowCastingMode.Off;
        renderer.receiveShadows = false;
        renderer.allowOcclusionWhenDynamic = false;
    }
    
    Mesh CreateProceduralSphere(int widthSegments, int heightSegments)
    {
        Mesh mesh = new Mesh();
        mesh.name = "ProceduralSphere";
        
        int horizontalSegmentCount = widthSegments;
        int verticalSegmentCount = heightSegments;
        
        int vertexCount = (horizontalSegmentCount + 1) * (verticalSegmentCount + 1);
        Vector3[] vertices = new Vector3[vertexCount];
        Vector3[] normals = new Vector3[vertexCount];
        Vector2[] uvs = new Vector2[vertexCount];
        
        for (int y = 0; y <= verticalSegmentCount; y++)
        {
            for (int x = 0; x <= horizontalSegmentCount; x++)
            {
                float u = (float)x / horizontalSegmentCount;
                float v = (float)y / verticalSegmentCount;
                
                float theta = u * 2f * Mathf.PI;
                float phi = v * Mathf.PI;
                
                float sinPhi = Mathf.Sin(phi);
                float cosPhi = Mathf.Cos(phi);
                float sinTheta = Mathf.Sin(theta);
                float cosTheta = Mathf.Cos(theta);
                
                Vector3 vertex = new Vector3(cosTheta * sinPhi, cosPhi, sinTheta * sinPhi);
                
                int index = y * (horizontalSegmentCount + 1) + x;
                vertices[index] = vertex;
                normals[index] = vertex.normalized;
                uvs[index] = new Vector2(u, v);
            }
        }
        
        int indexCount = horizontalSegmentCount * verticalSegmentCount * 6;
        int[] triangles = new int[indexCount];
        
        int triangleIndex = 0;
        for (int y = 0; y < verticalSegmentCount; y++)
        {
            for (int x = 0; x < horizontalSegmentCount; x++)
            {
                int firstIndex = y * (horizontalSegmentCount + 1) + x;
                int secondIndex = firstIndex + horizontalSegmentCount + 1;
                
                triangles[triangleIndex++] = firstIndex;
                triangles[triangleIndex++] = secondIndex;
                triangles[triangleIndex++] = firstIndex + 1;
                
                triangles[triangleIndex++] = secondIndex;
                triangles[triangleIndex++] = secondIndex + 1;
                triangles[triangleIndex++] = firstIndex + 1;
            }
        }
        
        mesh.vertices = vertices;
        mesh.normals = normals;
        mesh.uv = uvs;
        mesh.triangles = triangles;
        
        return mesh;
    }
    
    void GenerateTerrain()
    {
        if (terrainObject != null) return;
        
        GameObject terrainParent = new GameObject("Terrain");
        terrainParent.transform.parent = transform;
        
        for (int x = -1; x <= 1; x++)
        {
            for (int z = -1; z <= 1; z++)
            {
                CreateTerrainChunk(terrainParent, x, z);
            }
        }
        
        terrainObject = terrainParent;
    }
    
    void CreateTerrainChunk(GameObject parent, int chunkX, int chunkZ)
    {
        GameObject chunk = new GameObject($"TerrainChunk_{chunkX}_{chunkZ}");
        chunk.transform.parent = parent.transform;
        chunk.transform.position = new Vector3(chunkX * terrainChunkSize, 0, chunkZ * terrainChunkSize);
        
        MeshFilter meshFilter = chunk.AddComponent<MeshFilter>();
        MeshRenderer renderer = chunk.AddComponent<MeshRenderer>();
        MeshCollider collider = chunk.AddComponent<MeshCollider>();
        
        Mesh mesh = GenerateTriangularTerrain(terrainChunkSize, 20);
        meshFilter.mesh = mesh;
        collider.sharedMesh = mesh;
        
        if (terrainMaterial == null)
        {
            terrainMaterial = CreateTerrainMaterial();
        }
        renderer.material = terrainMaterial;
        renderer.shadowCastingMode = ShadowCastingMode.On;
        renderer.receiveShadows = true;
        renderer.motionVectorGenerationMode = MotionVectorGenerationMode.Camera;
    }
    
    Mesh GenerateTriangularTerrain(int size, int subdivisions)
    {
        Mesh mesh = new Mesh();
        mesh.name = "TriangularTerrain";
        
        int vertexCount = (subdivisions + 1) * (subdivisions + 1);
        Vector3[] vertices = new Vector3[vertexCount];
        Vector3[] normals = new Vector3[vertexCount];
        Vector2[] uvs = new Vector2[vertexCount];
        
        for (int z = 0; z <= subdivisions; z++)
        {
            for (int x = 0; x <= subdivisions; x++)
            {
                float u = (float)x / subdivisions;
                float v = (float)z / subdivisions;
                
                float height = GenerateHeight(u, v) * terrainHeightScale;
                
                int index = z * (subdivisions + 1) + x;
                vertices[index] = new Vector3(u * size - size * 0.5f, height, v * size - size * 0.5f);
                normals[index] = Vector3.up;
                uvs[index] = new Vector2(u, v);
            }
        }
        
        int triangleCount = subdivisions * subdivisions * 6;
        int[] triangles = new int[triangleCount];
        
        int triangleIndex = 0;
        for (int z = 0; z < subdivisions; z++)
        {
            for (int x = 0; x < subdivisions; x++)
            {
                int topLeft = z * (subdivisions + 1) + x;
                int topRight = topLeft + 1;
                int bottomLeft = topLeft + (subdivisions + 1);
                int bottomRight = bottomLeft + 1;
                
                triangles[triangleIndex++] = topLeft;
                triangles[triangleIndex++] = bottomLeft;
                triangles[triangleIndex++] = topRight;
                
                triangles[triangleIndex++] = topRight;
                triangles[triangleIndex++] = bottomLeft;
                triangles[triangleIndex++] = bottomRight;
            }
        }
        
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.normals = normals;
        mesh.uv = uvs;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        
        return mesh;
    }
    
    float GenerateHeight(float x, float z)
    {
        float height = 0;
        
        height += Mathf.PerlinNoise(x * 2f, z * 2f) * 1.0f;
        height += Mathf.PerlinNoise(x * 4f, z * 4f) * 0.5f;
        height += Mathf.PerlinNoise(x * 8f, z * 8f) * 0.25f;
        height += Mathf.PerlinNoise(x * 16f, z * 16f) * 0.125f;
        
        height *= 0.5f;
        
        float mountainHeight = Mathf.Pow(Mathf.PerlinNoise(x * 1f, z * 1f), 2f) * 2f;
        height += mountainHeight;
        
        return height;
    }
    
    Material CreateTerrainMaterial()
    {
        Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        
        mat.SetColor("_BaseColor", new Color(0.3f, 0.25f, 0.2f));
        mat.SetFloat("_Metallic", 0.1f);
        mat.SetFloat("_Smoothness", 0.1f);
        
        if (enableRayTracing)
        {
            mat.EnableKeyword("RTX_TERRAIN");
        }
        
        return mat;
    }
    
    void ConfigureLighting()
    {
        if (sunLight != null)
        {
            RenderSettings.sun = sunLight;
        }
        
        RenderSettings.defaultReflectionMode = DefaultReflectionMode.Custom;
        
        if (enableRayTracing && enableRTXReflections)
        {
            RenderSettings.reflectionIntensity = 1f;
        }
        else
        {
            RenderSettings.reflectionIntensity = 0.5f;
        }
    }
    
    void ConfigureFog()
    {
        if (enableFog)
        {
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Exponential;
            RenderSettings.fogColor = fogColor;
            RenderSettings.fogDensity = fogDensity;
            RenderSettings.fogStartDistance = fogStartDistance;
            RenderSettings.fogEndDistance = fogEndDistance;
        }
        else
        {
            RenderSettings.fog = false;
        }
    }
    
    void ConfigureRayTracing()
    {
        if (!enableRayTracing) return;
        
        if (SystemInfo.supportsRayTracing)
        {
            Debug.Log("RTX光线追踪已启用");
            
            RenderPipelineManager.scriptableRenderPipelineRequested += OnPipelineRequested;
        }
        else
        {
            Debug.LogWarning("当前平台不支持光线追踪");
        }
    }
    
    bool OnPipelineRequested()
    {
        return enableRayTracing;
    }
    
    void OnDestroy()
    {
        RenderPipelineManager.scriptableRenderPipelineRequested -= OnPipelineRequested;
    }
    
    public Vector3 GetGravityAtPosition(Vector3 position)
    {
        Vector3 direction = (planetPosition - position).normalized;
        return direction * gravity;
    }
    
    public float GetAltitude(Vector3 position)
    {
        float distanceFromPlanet = Vector3.Distance(position, planetPosition);
        return Mathf.Max(0, distanceFromPlanet - planetRadius);
    }
    
    public bool IsInAtmosphere(Vector3 position)
    {
        float altitude = GetAltitude(position);
        return hasAtmosphere && altitude < atmosphereHeight;
    }
    
    public float GetAtmosphericDensityAtPosition(Vector3 position)
    {
        float altitude = GetAltitude(position);
        
        if (!hasAtmosphere || altitude >= atmosphereHeight)
            return 0f;
        
        float heightRatio = altitude / atmosphereHeight;
        return atmosphereDensity * (1f - heightRatio);
    }
}

public enum RayTracingQuality
{
    Low,
    Medium,
    High,
    Ultra
}
