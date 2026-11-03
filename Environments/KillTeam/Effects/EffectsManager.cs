
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
#endif

public class EffectsManager : MonoBehaviour
{
    public static EffectsManager Instance { get; private set; }

    public enum EffectType
    {
        Electricity_01,
        Explosion_01,
        Explosion_02,
        Flames_01,
        Flamethrower_01,
        Flamethrower_02,
        Heal_01,
        Heal_02,
        Hyperdrive_01,
        Impact_01,
        Implosion_01,
        Lightning_01,
        Lightning_02,
        LootDrop_01,
        LootDrop_02,
        MeteorRain_01,
        MeteorRain_02,
        MuzzleFlash_01,
        MuzzleFlash_02,
        Portal_01,
        Portal_02,
        Projectile_01,
        Projectile_02,
        Rain_01,
        Rain_02,
        Shield_01,
        Shockwave_01,
        Smoke_01,
        Sparks_01,
        Tornado_01
    }

    [Header("Editor Testing")]
    [SerializeField, Min(0.01f)]
    private float m_TestTime = 3f;

    [SerializeField]
    private Vector3 m_TestPosition = new Vector3(0f, 0.6f, 0f);

    [SerializeField]
    private EffectType m_EffectType = EffectType.Electricity_01;

    [Header("Effect Settings")]
    [SerializeField, Min(0.0001f)]
    private float m_EffectScale = 0.04f;

    private readonly Dictionary<EffectType, ParticleSystem>
        effectDictionary = new Dictionary<EffectType, ParticleSystem>();

    private readonly Dictionary<ParticleSystem, Coroutine>
        stopCoroutines = new Dictionary<ParticleSystem, Coroutine>();

#if UNITY_EDITOR
    private ParticleSystem m_PreviewParticle;
    private double m_PreviewStartTime;
    private float m_PreviewDuration;
    private bool m_IsPreviewing;
#endif

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        InitializeEffects();
        StopAllEffects();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;

#if UNITY_EDITOR
        StopEditorPreview();
#endif
    }

    private void InitializeEffects()
    {
        effectDictionary.Clear();

        foreach (Transform child in transform)
        {
            if (!Enum.TryParse(
                child.name,
                out EffectType effect))
                continue;

            if (!Enum.IsDefined(typeof(EffectType), effect))
                continue;

            ParticleSystem rootParticle =
                child.GetComponent<ParticleSystem>();

            if (rootParticle == null)
            {
                Debug.LogWarning(
                    $"No root ParticleSystem on {child.name}",
                    child
                );
                continue;
            }

            effectDictionary[effect] = rootParticle;
        }
    }

    public void ApplyEffectSettings()
    {
        foreach (Transform child in transform)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                Undo.RecordObject(child, "Apply Effect Scale");
#endif

            child.localScale = Vector3.one * m_EffectScale;

#if UNITY_EDITOR
            if (!Application.isPlaying)
                SaveEditorObject(child);
#endif

            ParticleSystem[] systems =
                child.GetComponentsInChildren<ParticleSystem>(true);

            foreach (ParticleSystem ps in systems)
            {
#if UNITY_EDITOR
                if (!Application.isPlaying)
                    Undo.RecordObject(ps, "Apply Particle Settings");
#endif

                var main = ps.main;

                main.loop = false;
                main.playOnAwake = false;

                ps.Stop(
                    false,
                    ParticleSystemStopBehavior.StopEmittingAndClear
                );

#if UNITY_EDITOR
                if (!Application.isPlaying)
                    SaveEditorObject(ps);
#endif
            }
        }

#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            SaveEditorChanges();
            SceneView.RepaintAll();
        }
#endif

        InitializeEffects();
    }

    public void StopAllEffects()
    {
        foreach (Coroutine routine in stopCoroutines.Values)
        {
            if (routine != null)
                StopCoroutine(routine);
        }

        stopCoroutines.Clear();

        foreach (Transform child in transform)
        {
            ParticleSystem[] systems =
                child.GetComponentsInChildren<ParticleSystem>(true);

            foreach (ParticleSystem ps in systems)
            {
                ps.Stop(
                    false,
                    ParticleSystemStopBehavior.StopEmittingAndClear
                );
            }
        }
    }

    public void PlayEffect(EffectType effect, Vector3 position)
    {
        PlayEffect(effect, position, -1f);
    }

    public void PlayEffect(
        EffectType effect,
        Vector3 position,
        float time)
    {
        if (effectDictionary.Count == 0)
            InitializeEffects();

        if (!effectDictionary.TryGetValue(
            effect,
            out ParticleSystem particleSystem))
        {
            Debug.LogWarning(
                $"Effect not found: {effect}",
                this
            );
            return;
        }

        // Cancel a previous stop timer for this effect.
        if (stopCoroutines.TryGetValue(
            particleSystem,
            out Coroutine existingRoutine))
        {
            if (existingRoutine != null)
                StopCoroutine(existingRoutine);

            stopCoroutines.Remove(particleSystem);
        }

        particleSystem.transform.position = position;

        // Restart only the selected effect.
        particleSystem.Stop(
            true,
            ParticleSystemStopBehavior.StopEmittingAndClear
        );

        particleSystem.Play(true);

        if (time > 0f && Application.isPlaying)
        {
            Coroutine routine = StartCoroutine(
                StopEffectAfterTime(particleSystem, time)
            );

            stopCoroutines[particleSystem] = routine;
        }
    }

    private IEnumerator StopEffectAfterTime(
        ParticleSystem particleSystem,
        float time)
    {
        yield return new WaitForSeconds(time);

        if (particleSystem != null)
        {
            particleSystem.Stop(
                true,
                ParticleSystemStopBehavior.StopEmittingAndClear
            );

            stopCoroutines.Remove(particleSystem);
        }
    }

    public void TestEffect()
    {
        if (Application.isPlaying)
        {
            PlayEffect(
                m_EffectType,
                m_TestPosition,
                m_TestTime
            );
            return;
        }

#if UNITY_EDITOR
        StartEditorPreview();
#endif
    }

#if UNITY_EDITOR

    private void StartEditorPreview()
    {
        StopEditorPreview();

        InitializeEffects();

        if (!effectDictionary.TryGetValue(
            m_EffectType,
            out ParticleSystem particleSystem))
        {
            Debug.LogWarning(
                $"Effect not found: {m_EffectType}",
                this
            );
            return;
        }

        m_PreviewParticle = particleSystem;
        m_PreviewParticle.transform.position = m_TestPosition;

        m_PreviewParticle.Stop(
            true,
            ParticleSystemStopBehavior.StopEmittingAndClear
        );

        m_PreviewDuration = Mathf.Max(0.01f, m_TestTime);
        m_PreviewStartTime = EditorApplication.timeSinceStartup;
        m_IsPreviewing = true;

        m_PreviewParticle.Simulate(
            0f,
            true,
            true,
            true
        );

        EditorApplication.update -= UpdateEditorPreview;
        EditorApplication.update += UpdateEditorPreview;

        SceneView.RepaintAll();
    }

    private void UpdateEditorPreview()
    {
        if (!m_IsPreviewing || m_PreviewParticle == null)
        {
            StopEditorPreview();
            return;
        }

        float elapsed =
            (float)(EditorApplication.timeSinceStartup -
                    m_PreviewStartTime);

        if (elapsed >= m_PreviewDuration)
        {
            StopEditorPreview();
            return;
        }

        m_PreviewParticle.Simulate(
            elapsed,
            true,
            true,
            true
        );

        SceneView.RepaintAll();
    }

    public void StopEditorPreview()
    {
        EditorApplication.update -= UpdateEditorPreview;

        m_IsPreviewing = false;

        if (m_PreviewParticle != null)
        {
            m_PreviewParticle.Stop(
                true,
                ParticleSystemStopBehavior.StopEmittingAndClear
            );
        }

        m_PreviewParticle = null;

        SceneView.RepaintAll();
    }

    private static void SaveEditorObject(UnityEngine.Object obj)
    {
        EditorUtility.SetDirty(obj);

        if (PrefabUtility.IsPartOfPrefabInstance(obj))
        {
            PrefabUtility.RecordPrefabInstancePropertyModifications(obj);
        }
    }

    private void SaveEditorChanges()
    {
        // Editing a prefab directly in Prefab Mode.
        if (PrefabStageUtility.GetCurrentPrefabStage() != null)
        {
            PrefabStage stage =
                PrefabStageUtility.GetCurrentPrefabStage();

            if (gameObject.scene == stage.scene)
            {
                EditorSceneManager.MarkSceneDirty(stage.scene);
                EditorSceneManager.SaveScene(stage.scene);
                return;
            }
        }

        // Editing a prefab asset directly.
        if (PrefabUtility.IsPartOfPrefabAsset(gameObject))
        {
            GameObject root = transform.root.gameObject;

            PrefabUtility.SavePrefabAsset(root);
            return;
        }

        // Editing an object in a normal scene.
        if (gameObject.scene.IsValid() &&
            gameObject.scene.isLoaded)
        {
            EditorSceneManager.MarkSceneDirty(gameObject.scene);
            EditorSceneManager.SaveScene(gameObject.scene);
        }
    }

#endif
}

#if UNITY_EDITOR

[CustomEditor(typeof(EffectsManager))]
public class EffectsManagerEditor : Editor
{
    private SerializedProperty testPosition;

    private void OnEnable()
    {
        testPosition =
            serializedObject.FindProperty("m_TestPosition");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        DrawDefaultInspector();

        serializedObject.ApplyModifiedProperties();

        EffectsManager manager =
            (EffectsManager)target;

        EditorGUILayout.Space(10);

        if (GUILayout.Button(
            "Apply Settings To All Effects",
            GUILayout.Height(35)))
        {
            manager.ApplyEffectSettings();
        }

        EditorGUILayout.Space(5);

        if (GUILayout.Button(
            "Test Effect",
            GUILayout.Height(35)))
        {
            manager.TestEffect();
        }

        if (GUILayout.Button(
            "Stop Effect",
            GUILayout.Height(25)))
        {
            if (Application.isPlaying)
                manager.StopAllEffects();
            else
                manager.StopEditorPreview();
        }
    }

    private void OnSceneGUI()
    {
        serializedObject.Update();

        Vector3 position =
            testPosition.vector3Value;

        EditorGUI.BeginChangeCheck();

        Vector3 newPosition =
            Handles.PositionHandle(
                position,
                Quaternion.identity
            );

        if (EditorGUI.EndChangeCheck())
        {
            testPosition.vector3Value = newPosition;
            serializedObject.ApplyModifiedProperties();

            SceneView.RepaintAll();
        }

        Handles.color = Color.green;

        Handles.SphereHandleCap(
            0,
            newPosition,
            Quaternion.identity,
            HandleUtility.GetHandleSize(newPosition) * 0.15f,
            EventType.Repaint
        );

        Handles.Label(
            newPosition + Vector3.up * 0.3f,
            "Effect Test Position"
        );
    }

    private void OnDisable()
    {
        if (target != null && !Application.isPlaying)
        {
            EffectsManager manager =
                (EffectsManager)target;

            manager.StopEditorPreview();
        }
    }
}

#endif
