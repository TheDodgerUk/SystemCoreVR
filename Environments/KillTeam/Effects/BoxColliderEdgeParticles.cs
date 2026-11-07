
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(BoxCollider))]
public class BoxColliderEdgeParticles : MonoBehaviour
{
    [Header("References")]
    public BoxCollider boxCollider;
    public ParticleSystem particles;

    [Header("Particle Settings")]
    [Min(1)] public int particlesPerEdge = 20;
    [Min(0f)] public float speed = 1f;
    [Min(0.001f)] public float particleSize = 0.0025f;
    public Color highlightColor = Color.green;

    [Header("Glow Settings")]
    [Range(0.1f, 5f)]
    public float glowIntensity = 1.5f;

    [Range(0.1f, 4f)]
    public float glowSharpness = 1.5f;

    [Header("Options")]
    public bool highlightEnabled = true;

    [Header("Debug")]
    public bool debugMode = false;
    public Material debugMaterial;

    private ParticleSystem.Particle[] buffer;
    private Material runtimeMaterial;
    private Texture2D runtimeTexture;

    private int particleCount;
    private float animationTime;

    // All twelve edges of the BoxCollider.
    private static readonly int[,] edges =
    {
        {0,1}, {1,2}, {2,3}, {3,0},
        {4,5}, {5,6}, {6,7}, {7,4},
        {0,4}, {1,5}, {2,6}, {3,7}
    };

    private void Awake()
    {
        ReinitialiseParticles();
    }

    private void LateUpdate()
    {
        if (particles == null)
            return;

        var renderer =
            particles.GetComponent<ParticleSystemRenderer>();

        renderer.enabled = highlightEnabled;

        if (!highlightEnabled)
            return;

        if (particleCount != 12 * particlesPerEdge ||
            buffer == null)
        {
            ReinitialiseParticles();
        }

        animationTime += Time.deltaTime;

        UpdateParticlePositions();
    }

    // Automatically create a child ParticleSystem.
    private void CreateParticleSystem()
    {
        if (particles != null)
            return;

        GameObject child = new GameObject("Edge Particles");

        child.transform.SetParent(transform, false);
        child.transform.localPosition = Vector3.zero;
        child.transform.localRotation = Quaternion.identity;
        child.transform.localScale = Vector3.one;

        particles = child.AddComponent<ParticleSystem>();

        Debug.Log("Created Edge ParticleSystem", this);
    }

    // Generate a soft circular particle texture.
    private Texture2D CreateCircleTexture()
    {
        const int size = 64;

        Texture2D texture = new Texture2D(
            size,
            size,
            TextureFormat.RGBA32,
            false);

        texture.name = "Generated Circular Particle";
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Bilinear;

        Color[] pixels = new Color[size * size];

        Vector2 center = new Vector2(
            (size - 1) * 0.5f,
            (size - 1) * 0.5f);

        float radius = size * 0.5f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float distance = Vector2.Distance(
                    new Vector2(x, y),
                    center) / radius;

                // Smooth circular falloff.
                float alpha = Mathf.Clamp01(1f - distance);

                alpha = Mathf.Pow(
                    alpha,
                    glowSharpness);

                pixels[y * size + x] =
                    new Color(1f, 1f, 1f, alpha);
            }
        }

        texture.SetPixels(pixels);
        texture.Apply();

        return texture;
    }

    // Create a URP material using the circular texture.
    private void CreateParticleMaterial()
    {
        if (runtimeTexture != null)
            Destroy(runtimeTexture);

        if (runtimeMaterial != null)
            Destroy(runtimeMaterial);

        runtimeTexture = CreateCircleTexture();

        Shader shader = Shader.Find(
            "Universal Render Pipeline/Particles/Unlit");

        if (shader == null)
        {
            Debug.LogError(
                "URP Particles/Unlit shader not found!",
                this);
            return;
        }

        runtimeMaterial = new Material(shader);
        runtimeMaterial.name = "Generated Round Particle Material";

        // Configure transparent additive rendering.
        runtimeMaterial.SetFloat("_Surface", 1f);
        runtimeMaterial.SetFloat("_Blend", 2f);
        runtimeMaterial.SetFloat("_ZWrite", 0f);

        runtimeMaterial.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");

        runtimeMaterial.renderQueue = 3000;

        if (runtimeMaterial.HasProperty("_BaseMap"))
            runtimeMaterial.SetTexture("_BaseMap", runtimeTexture);

        if (runtimeMaterial.HasProperty("_BaseColor"))
        {
            runtimeMaterial.SetColor(
                "_BaseColor",
                Color.white * glowIntensity);
        }
    }

    private void ConfigureParticleSystem()
    {
        var main = particles.main;

        main.playOnAwake = false;
        main.loop = false;

        main.simulationSpace =
            ParticleSystemSimulationSpace.World;

        main.startSpeed = 0f;
        main.startLifetime = 10000f;
        main.startSize = particleSize;
        main.startColor = Color.white;
        main.maxParticles = 12 * particlesPerEdge;

        var emission = particles.emission;
        emission.enabled = false;

        var shape = particles.shape;
        shape.enabled = false;

        var velocity = particles.velocityOverLifetime;
        velocity.enabled = false;

        var force = particles.forceOverLifetime;
        force.enabled = false;

        var sizeOverLifetime = particles.sizeOverLifetime;
        sizeOverLifetime.enabled = false;

        var colorOverLifetime = particles.colorOverLifetime;
        colorOverLifetime.enabled = false;

        var trails = particles.trails;
        trails.enabled = false;

        var renderer =
            particles.GetComponent<ParticleSystemRenderer>();

        renderer.enabled = highlightEnabled;
        renderer.renderMode =
            ParticleSystemRenderMode.Billboard;

        renderer.alignment =
            ParticleSystemRenderSpace.View;

        // Debug material overrides the generated material.
        if (debugMaterial != null)
        {
            renderer.sharedMaterial = debugMaterial;
        }
        else
        {
            CreateParticleMaterial();
            renderer.sharedMaterial = runtimeMaterial;
        }
    }

    // Rebuild particles and apply the selected material.
    public void ReinitialiseParticles()
    {
        if (boxCollider == null)
            boxCollider = GetComponent<BoxCollider>();

        if (particles == null)
            particles = GetComponentInChildren<ParticleSystem>();

        CreateParticleSystem();
        ConfigureParticleSystem();
        CreateParticles();

        animationTime = 0f;

        Debug.Log(
            $"Particles reinitialised. " +
            $"Material: " +
            $"{particles.GetComponent<ParticleSystemRenderer>().sharedMaterial?.name}",
            this);
    }

    private void CreateParticles()
    {
        particleCount = 12 * Mathf.Max(1, particlesPerEdge);

        var main = particles.main;
        main.maxParticles = particleCount;

        particles.Stop(
            true,
            ParticleSystemStopBehavior.StopEmittingAndClear);

        buffer = new ParticleSystem.Particle[particleCount];

        if (!Application.isPlaying)
            return;

        particles.Play();

        for (int i = 0; i < particleCount; i++)
        {
            ParticleSystem.EmitParams emit =
                new ParticleSystem.EmitParams
                {
                    position = transform.position,
                    velocity = Vector3.zero,
                    startLifetime = 10000f,
                    startSize = particleSize,
                    startColor = highlightColor
                };

            particles.Emit(emit, 1);
        }
    }

    // Animate particles along all twelve edges.
    private void UpdateParticlePositions()
    {
        if (boxCollider == null || particles == null)
            return;

        int alive = particles.GetParticles(buffer);

        if (alive != particleCount)
        {
            CreateParticles();
            alive = particles.GetParticles(buffer);

            if (alive != particleCount)
                return;
        }

        Vector3[] corners = GetCorners();

        int index = 0;

        for (int edge = 0; edge < 12; edge++)
        {
            Vector3 start = corners[edges[edge, 0]];
            Vector3 end = corners[edges[edge, 1]];

            float length = Vector3.Distance(start, end);

            for (int i = 0; i < particlesPerEdge; i++)
            {
                float offset =
                    i / (float)particlesPerEdge;

                float t = length > 0.00001f
                    ? Mathf.Repeat(
                        animationTime * speed / length + offset,
                        1f)
                    : 0f;

                Vector3 position =
                    Vector3.Lerp(start, end, t);

                buffer[index].position = position;
                buffer[index].velocity = Vector3.zero;

                buffer[index].startSize =
                    debugMode ? 0.1f : particleSize;

                buffer[index].startColor =
                    debugMode ? Color.red : highlightColor;

                buffer[index].remainingLifetime = 9999f;

                index++;
            }
        }

        particles.SetParticles(buffer, particleCount);
    }

    // Get BoxCollider corners in world space.
    private Vector3[] GetCorners()
    {
        Vector3 center = boxCollider.center;
        Vector3 half = boxCollider.size * 0.5f;

        Vector3[] corners =
        {
            center + new Vector3(-half.x,-half.y,-half.z),
            center + new Vector3( half.x,-half.y,-half.z),
            center + new Vector3( half.x,-half.y, half.z),
            center + new Vector3(-half.x,-half.y, half.z),

            center + new Vector3(-half.x, half.y,-half.z),
            center + new Vector3( half.x, half.y,-half.z),
            center + new Vector3( half.x, half.y, half.z),
            center + new Vector3(-half.x, half.y, half.z)
        };

        for (int i = 0; i < corners.Length; i++)
        {
            corners[i] =
                boxCollider.transform.TransformPoint(corners[i]);
        }

        return corners;
    }

    public void SetHighlight(bool enabled)
    {
        highlightEnabled = enabled;

        if (particles != null)
        {
            var renderer =
                particles.GetComponent<ParticleSystemRenderer>();

            renderer.enabled = enabled;
        }
    }

    private void OnDestroy()
    {
        if (runtimeMaterial != null)
            Destroy(runtimeMaterial);

        if (runtimeTexture != null)
            Destroy(runtimeTexture);
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(BoxColliderEdgeParticles))]
    private class BoxColliderEdgeParticlesEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            var script =
                (BoxColliderEdgeParticles)target;

            GUILayout.Space(10);

            EditorGUILayout.LabelField(
                "Particle Controls",
                EditorStyles.boldLabel);

            if (GUILayout.Button(
                "Reinitialise Particles",
                GUILayout.Height(30)))
            {
                script.ReinitialiseParticles();
            }

            GUILayout.Space(5);

            if (GUILayout.Button("Enable Highlight"))
                script.SetHighlight(true);

            if (GUILayout.Button("Disable Highlight"))
                script.SetHighlight(false);

            GUILayout.Space(10);

            if (script.particles != null)
            {
                var renderer =
                    script.particles.GetComponent<ParticleSystemRenderer>();

                EditorGUILayout.LabelField(
                    "Current Material",
                    renderer.sharedMaterial != null
                        ? renderer.sharedMaterial.name
                        : "None");

                EditorGUILayout.LabelField(
                    "Particle Count",
                    script.particles.particleCount.ToString());
            }
        }
    }
#endif
}
