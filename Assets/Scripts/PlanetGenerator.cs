using UnityEngine;
using System.Collections.Generic;

public class PlanetGenerator : MonoBehaviour
{
    [Header("星球配置")]
    public string planetName = "Kepler-442b";
    public float radius = 6371000f;
    public Vector3 position = Vector3.zero;
    public Color baseColor = new Color(0.3f, 0.5f, 0.7f);
    public Color atmosphereColor = new Color(0.5f, 0.7f, 1f);
    
    [Header("地形参数")]
    public int terrainResolution = 64;
    public float terrainHeightScale = 1000f;
    public Material terrainMaterial;
    
    [Header("大气层")]
    public bool hasAtmosphere = true;
    public float atmosphereHeight = 100000f;
    public float cloudHeight = 5000f;
    public float cloudDensity = 0.5f;
    
    [Header("表面特征")]
    public bool generateMountains = true;
    public bool generateCraters = true;
    public bool generateRivers = true;
    public bool generateOceans = true;
    
    [Header("海洋设置")]
    public float oceanDepth = 1000f;
    public Color oceanColor = new Color(0.1f, 0.2f, 0.5f);
    public float oceanReflectivity = 0.6f;
    
    [Header("山系设置 (三角形)")]
    public int mountainCount = 50;
    public float mountainHeightScale = 3000f;
    public float mountainBaseRadius = 500f;
    public bool triangularMountains = true;
    
    [Header("陨石坑设置")]
    public int craterCount = 30;
    public float craterDepthScale = 500f;
    public float craterWidthScale = 1000f;
    
    [Header("效果")]
    public bool enableVolcanoes = false;
    public bool enableAuroras = false;
    public bool enableDustStorms = false;
    
    private GameObject planetObject;
    private GameObject atmosphereObject;
    private GameObject terrainObject;
    private List<GameObject> mountainObjects = new List<GameObject>();
    private List<GameObject> craterObjects = new List<GameObject>();
    
    void Awake()
    {
        GeneratePlanet();
    }
    
    public void GeneratePlanet()
    {
        CreatePlanetCore();
        CreateAtmosphere();
        CreateTerrain();
        
        if (generateMountains)
        {
            GenerateTriangularMountains();
        }
        
        if (generateCraters)
        {
            GenerateCraters();
        }
        
        if (generateOceans)
        {
            GenerateOceans();
        }
        
        if (generateRivers)
        {
            GenerateRivers();
        }
        
        if (enableVolcanoes)
        {
            GenerateVolcanoes();
        }
        
        if (enableAuroras)
        {
            GenerateAuroras();
        }
    }
    
    void CreatePlanetCore()
    {
        planetObject = new GameObject(planetName);
        planetObject.transform.parent = transform;
        planetObject.transform.position = position;
        planetObject.transform.localScale = Vector3.one * radius * 2;
        
        MeshFilter meshFilter = planetObject.AddComponent<MeshFilter>();
        MeshRenderer renderer = planetObject.AddComponent<MeshRenderer>();
        MeshCollider collider = planetObject.AddComponent<MeshCollider>();
        
        Mesh sphere = CreateProceduralSphere(32, 32);
        meshFilter.mesh = sphere;
        collider.sharedMesh = sphere;
        
        Material coreMaterial = new Material(Shader.Find("Standard"));
        coreMaterial.color = baseColor;
        coreMaterial.metallic = 0.1f;
        coreMaterial.smoothness = 0.2f;
        renderer.material = coreMaterial;
    }
    
    void CreateAtmosphere()
    {
        if (!hasAtmosphere) return;
        
        atmosphereObject = new GameObject("Atmosphere");
        atmosphereObject.transform.parent = planetObject.transform;
        atmosphereObject.transform.position = position;
        atmosphereObject.transform.localScale = Vector3.one * (radius + atmosphereHeight) * 2;
        
        MeshFilter meshFilter = atmosphereObject.AddComponent<MeshFilter>();
        MeshRenderer renderer = atmosphereObject.AddComponent<MeshRenderer>();
        
        Mesh sphere = CreateProceduralSphere(32, 32);
        meshFilter.mesh = sphere;
        
        Material atmosphereMaterial = new Material(Shader.Find("Custom/AtmosphereShader"));
        atmosphereMaterial.SetColor("_AtmosphereColor", atmosphereColor);
        atmosphereMaterial.SetFloat("_Opacity", 0.3f);
        atmosphereMaterial.SetFloat("_Radius", radius + atmosphereHeight);
        
        renderer.material = atmosphereMaterial;
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
    }
    
    void CreateTerrain()
    {
        terrainObject = new GameObject("Terrain");
        terrainObject.transform.parent = planetObject.transform;
        terrainObject.transform.position = position;
        terrainObject.transform.localScale = Vector3.one * (radius * 2 + 100);
        
        MeshFilter meshFilter = terrainObject.AddComponent<MeshFilter>();
        MeshRenderer renderer = terrainObject.AddComponent<MeshRenderer>();
        MeshCollider collider = terrainObject.AddComponent<MeshCollider>();
        
        Mesh terrainMesh = GenerateTerrainMesh();
        meshFilter.mesh = terrainMesh;
        collider.sharedMesh = terrainMesh;
        
        if (terrainMaterial == null)
        {
            terrainMaterial = CreateTerrainMaterial();
        }
        renderer.material = terrainMaterial;
    }
    
    Mesh GenerateTerrainMesh()
    {
        Mesh mesh = new Mesh();
        mesh.name = "PlanetTerrain";
        
        int segments = terrainResolution;
        int vertexCount = (segments + 1) * (segments + 1);
        Vector3[] vertices = new Vector3[vertexCount];
        Vector3[] normals = new Vector3[vertexCount];
        Vector2[] uvs = new Vector2[vertexCount];
        
        for (int y = 0; y <= segments; y++)
        {
            for (int x = 0; x <= segments; x++)
            {
                float u = (float)x / segments;
                float v = (float)y / segments;
                
                float theta = u * 2f * Mathf.PI;
                float phi = v * Mathf.PI;
                
                float height = CalculateTerrainHeight(u, v);
                float surfaceRadius = radius + height;
                
                float sinPhi = Mathf.Sin(phi);
                float cosPhi = Mathf.Cos(phi);
                float sinTheta = Mathf.Sin(theta);
                float cosTheta = Mathf.Cos(theta);
                
                Vector3 vertex = new Vector3(
                    cosTheta * sinPhi * surfaceRadius,
                    cosPhi * surfaceRadius,
                    sinTheta * sinPhi * surfaceRadius
                );
                
                int index = y * (segments + 1) + x;
                vertices[index] = vertex;
                normals[index] = vertex.normalized;
                uvs[index] = new Vector2(u, v);
            }
        }
        
        int triangleCount = segments * segments * 6;
        int[] triangles = new int[triangleCount];
        
        int triangleIndex = 0;
        for (int y = 0; y < segments; y++)
        {
            for (int x = 0; x < segments; x++)
            {
                int topLeft = y * (segments + 1) + x;
                int topRight = topLeft + 1;
                int bottomLeft = topLeft + (segments + 1);
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
        
        return mesh;
    }
    
    float CalculateTerrainHeight(float u, float v)
    {
        float height = 0f;
        
        height += Mathf.PerlinNoise(u * 4f, v * 4f) * terrainHeightScale * 0.5f;
        height += Mathf.PerlinNoise(u * 8f, v * 8f) * terrainHeightScale * 0.25f;
        height += Mathf.PerlinNoise(u * 16f, v * 16f) * terrainHeightScale * 0.125f;
        
        float oceanLevel = oceanDepth * 0.1f;
        if (height < oceanLevel && generateOceans)
        {
            height = oceanLevel;
        }
        
        return height;
    }
    
    void GenerateTriangularMountains()
    {
        for (int i = 0; i < mountainCount; i++)
        {
            CreateTriangularMountain(i);
        }
    }
    
    void CreateTriangularMountain(int index)
    {
        GameObject mountain = new GameObject($"Mountain_{index}");
        mountain.transform.parent = planetObject.transform;
        
        float u = Random.Range(0f, 1f);
        float v = Random.Range(0f, 1f);
        
        Vector3 direction = new Vector3(
            Mathf.Cos(u * 2f * Mathf.PI) * Mathf.Sin(v * Mathf.PI),
            Mathf.Cos(v * Mathf.PI),
            Mathf.Sin(u * 2f * Mathf.PI) * Mathf.Sin(v * Mathf.PI)
        );
        
        float height = mountainHeightScale * Random.Range(0.5f, 1.5f);
        float baseRadius = mountainBaseRadius * Random.Range(0.5f, 1.5f);
        
        mountain.transform.position = position + direction * (radius + height * 0.3f);
        mountain.transform.up = direction;
        mountain.transform.localScale = new Vector3(baseRadius, height, baseRadius);
        
        MeshFilter meshFilter = mountain.AddComponent<MeshFilter>();
        MeshRenderer renderer = mountain.AddComponent<MeshRenderer>();
        MeshCollider collider = mountain.AddComponent<MeshCollider>();
        
        Mesh mesh = CreateTriangularPyramid();
        meshFilter.mesh = mesh;
        collider.sharedMesh = mesh;
        
        Material mountainMaterial = new Material(Shader.Find("Standard"));
        mountainMaterial.color = Color.Lerp(baseColor, Color.gray, Random.Range(0f, 0.5f));
        mountainMaterial.metallic = 0.05f;
        mountainMaterial.smoothness = 0.1f;
        renderer.material = mountainMaterial;
        
        mountainObjects.Add(mountain);
    }
    
    Mesh CreateTriangularPyramid()
    {
        Mesh mesh = new Mesh();
        mesh.name = "TriangularPyramid";
        
        Vector3[] vertices = new Vector3[]
        {
            new Vector3(-0.5f, 0, -0.5f),
            new Vector3(0.5f, 0, -0.5f),
            new Vector3(0.5f, 0, 0.5f),
            new Vector3(-0.5f, 0, 0.5f),
            new Vector3(0, 1, 0)
        };
        
        int[] triangles = new int[]
        {
            0, 4, 1,
            1, 4, 2,
            2, 4, 3,
            3, 4, 0,
            0, 1, 2,
            0, 2, 3
        };
        
        Vector3[] normals = new Vector3[vertices.Length];
        for (int i = 0; i < normals.Length; i++)
        {
            normals[i] = vertices[i].normalized;
        }
        
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.normals = normals;
        
        return mesh;
    }
    
    void GenerateCraters()
    {
        for (int i = 0; i < craterCount; i++)
        {
            CreateCrater(i);
        }
    }
    
    void CreateCrater(int index)
    {
        GameObject crater = new GameObject($"Crater_{index}");
        crater.transform.parent = planetObject.transform;
        
        float u = Random.Range(0f, 1f);
        float v = Random.Range(0f, 1f);
        
        Vector3 direction = new Vector3(
            Mathf.Cos(u * 2f * Mathf.PI) * Mathf.Sin(v * Mathf.PI),
            Mathf.Cos(v * Mathf.PI),
            Mathf.Sin(u * 2f * Mathf.PI) * Mathf.Sin(v * Mathf.PI)
        );
        
        float width = craterWidthScale * Random.Range(0.5f, 1.5f);
        float depth = craterDepthScale * Random.Range(0.3f, 0.7f);
        
        crater.transform.position = position + direction * radius;
        crater.transform.up = direction;
        crater.transform.localScale = new Vector3(width, depth, width);
        
        MeshFilter meshFilter = crater.AddComponent<MeshFilter>();
        MeshRenderer renderer = crater.AddComponent<MeshRenderer>();
        
        Mesh mesh = CreateCraterMesh();
        meshFilter.mesh = mesh;
        
        Material craterMaterial = new Material(Shader.Find("Standard"));
        craterMaterial.color = Color.Lerp(Color.black, Color.gray, 0.5f);
        craterMaterial.metallic = 0.1f;
        craterMaterial.smoothness = 0.05f;
        renderer.material = craterMaterial;
        
        craterObjects.Add(crater);
    }
    
    Mesh CreateCraterMesh()
    {
        Mesh mesh = new Mesh();
        mesh.name = "CraterMesh";
        
        int segments = 16;
        int vertexCount = segments + 2;
        Vector3[] vertices = new Vector3[vertexCount];
        Vector2[] uvs = new Vector2[vertexCount];
        
        vertices[0] = Vector3.zero;
        uvs[0] = new Vector2(0.5f, 0.5f);
        
        for (int i = 0; i < segments; i++)
        {
            float angle = i * 2f * Mathf.PI / segments;
            float x = Mathf.Cos(angle);
            float z = Mathf.Sin(angle);
            
            vertices[i + 1] = new Vector3(x, 0, z);
            uvs[i + 1] = new Vector2((x + 1) * 0.5f, (z + 1) * 0.5f);
        }
        
        vertices[vertexCount - 1] = new Vector3(0, 1, 0);
        uvs[vertexCount - 1] = new Vector2(0.5f, 1f);
        
        int[] triangles = new int[segments * 3];
        
        for (int i = 0; i < segments; i++)
        {
            int baseIndex = i * 3;
            triangles[baseIndex] = 0;
            triangles[baseIndex + 1] = i + 1;
            triangles[baseIndex + 2] = (i + 1) % segments + 1;
        }
        
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.RecalculateNormals();
        
        return mesh;
    }
    
    void GenerateOceans()
    {
        if (!generateOceans) return;
        
        GameObject ocean = new GameObject("Ocean");
        ocean.transform.parent = planetObject.transform;
        ocean.transform.position = position;
        ocean.transform.localScale = Vector3.one * (radius * 2 + oceanDepth * 2);
        
        MeshFilter meshFilter = ocean.AddComponent<MeshFilter>();
        MeshRenderer renderer = ocean.AddComponent<MeshRenderer>();
        
        Mesh sphere = CreateProceduralSphere(32, 32);
        meshFilter.mesh = sphere;
        
        Material oceanMaterial = new Material(Shader.Find("Standard"));
        oceanMaterial.color = oceanColor;
        oceanMaterial.metallic = 0.9f;
        oceanMaterial.smoothness = oceanReflectivity;
        oceanMaterial.SetFloat("_ReflectionStrength", oceanReflectivity);
        oceanMaterial.EnableKeyword("_REFLECTION");
        
        renderer.material = oceanMaterial;
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
    }
    
    void GenerateRivers()
    {
    }
    
    void GenerateVolcanoes()
    {
        if (!enableVolcanoes) return;
        
        for (int i = 0; i < 10; i++)
        {
            CreateVolcano(i);
        }
    }
    
    void CreateVolcano(int index)
    {
        GameObject volcano = new GameObject($"Volcano_{index}");
        volcano.transform.parent = planetObject.transform;
        
        float u = Random.Range(0f, 1f);
        float v = Random.Range(0f, 1f);
        
        Vector3 direction = new Vector3(
            Mathf.Cos(u * 2f * Mathf.PI) * Mathf.Sin(v * Mathf.PI),
            Mathf.Cos(v * Mathf.PI),
            Mathf.Sin(u * 2f * Mathf.PI) * Mathf.Sin(v * Mathf.PI)
        );
        
        float height = 2000f * Random.Range(0.8f, 1.5f);
        float baseRadius = 300f * Random.Range(0.8f, 1.2f);
        
        volcano.transform.position = position + direction * (radius + height * 0.4f);
        volcano.transform.up = direction;
        volcano.transform.localScale = new Vector3(baseRadius, height, baseRadius);
        
        MeshFilter meshFilter = volcano.AddComponent<MeshFilter>();
        MeshRenderer renderer = volcano.AddComponent<MeshRenderer>();
        
        Mesh mesh = CreateTriangularPyramid();
        meshFilter.mesh = mesh;
        
        Material volcanoMaterial = new Material(Shader.Find("Standard"));
        volcanoMaterial.color = Color.red;
        volcanoMaterial.emissionColor = Color.red * 0.5f;
        volcanoMaterial.EnableKeyword("_EMISSION");
        renderer.material = volcanoMaterial;
        
        GameObject lava = new GameObject("Lava");
        lava.transform.parent = volcano.transform;
        lava.transform.localPosition = Vector3.up * 0.8f;
        
        ParticleSystem lavaParticles = lava.AddComponent<ParticleSystem>();
        var main = lavaParticles.main;
        main.startColor = Color.red;
        main.startSize = 5f;
        main.startSpeed = 10f;
        main.emissionRate = 50;
    }
    
    void GenerateAuroras()
    {
        if (!enableAuroras) return;
        
        GameObject aurora = new GameObject("Aurora");
        aurora.transform.parent = planetObject.transform;
        aurora.transform.position = position;
        aurora.transform.localScale = Vector3.one * (radius + atmosphereHeight) * 2;
        
        MeshFilter meshFilter = aurora.AddComponent<MeshFilter>();
        MeshRenderer renderer = aurora.AddComponent<MeshRenderer>();
        
        Mesh auroraMesh = CreateProceduralSphere(32, 32);
        meshFilter.mesh = auroraMesh;
        
        Material auroraMaterial = new Material(Shader.Find("Custom/AuroraShader"));
        auroraMaterial.SetColor("_AuroraColor", new Color(0.1f, 1f, 0.3f));
        auroraMaterial.SetFloat("_Opacity", 0.5f);
        
        renderer.material = auroraMaterial;
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        renderer.allowOcclusionWhenDynamic = false;
    }
    
    Material CreateTerrainMaterial()
    {
        Material mat = new Material(Shader.Find("Standard"));
        mat.color = baseColor;
        mat.metallic = 0.1f;
        mat.smoothness = 0.2f;
        return mat;
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
}
