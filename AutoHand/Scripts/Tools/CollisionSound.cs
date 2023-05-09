using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class CollisionSound : MonoBehaviour
{
    private static List<string> m_fGuid = new List<string>();

    [Tooltip("The layers that cause the sound to play")]
    public LayerMask collisionTriggers = ~0;
    [Tooltip("Source to play sound from")]
    public AudioSource source;
    [Tooltip("Source to play sound from")]
    public AudioClip clip;
    [Space]
    [Tooltip("Source to play sound from")]
    public AnimationCurve velocityVolumeCurve = AnimationCurve.Linear(0, 0, 1, 1);


    private float m_VolumeAmpMultiplyer = 1f;
    public float volumeAmp = 0.8f;
    public float velocityAmp = 0.5f;
    public float soundRepeatDelay = 0.2f;

    [SerializeField]
    public Collider[] m_Colliders = null;

    Rigidbody body;
    bool canPlaySound = true;
    Coroutine playSoundRoutine;


    [Range(0, 1)]
    public float DEBUG_SOUND_VOLUME = 1f;
    [EditorButton]
    private void DEBUG_Sound() => source.PlayOneShot(clip, DEBUG_SOUND_VOLUME);

    private void Reset()
    {
        source = GetComponent<AudioSource>();
        source.playOnAwake = false;
        source.spatialBlend = 1f;
        if (source.maxDistance == 500)
        {
            source.maxDistance = 10f;
        }
    }

    private void Start()
    {
        body = GetComponent<Rigidbody>();
        source = GetComponent<AudioSource>();
        //So the sound doesn't play when falling in place on start
        StartCoroutine(SoundPlayBuffer(1f));

        InternalInitialise();
    }

    private void InternalInitialise()
    {
        if (clip == null)
        {
            Core.Mono.WaitFor(5, () => // long wait because of loading in via bundles
            {
                VrInteraction vr = null;
                if (this.CompareTag(Layers.RootCatTag) == true)
                {
                    vr = this.GetComponent<VrInteraction>();
                }
                else
                {
                    vr = this.GetComponentInParent<VrInteraction>();
                }

                if (vr != null)
                {
                    if (m_fGuid.Contains(vr.CatalogueEntryRef.Guid) == false)
                    {
                        m_fGuid.Add(vr.CatalogueEntryRef.Guid);
                        DebugBeep.Log($"No CollisionSound clip here, {vr.CatalogueEntryRef.ShortName}, GlobalCount: {m_fGuid.Count}", DebugBeep.MessageLevel.Medium, this.gameObject);
                    }
                }
                this.enabled = false;
                source.enabled = false;
            });
        }
    }


    public void SetVolumeAmp(float amount) => m_VolumeAmpMultiplyer = amount;
    public void ResetToOriginalState()
    {
        m_VolumeAmpMultiplyer = 1f;
        if (this.gameObject.activeInHierarchy == true)
        {
            StartCoroutine(SoundPlayBuffer(1f));
        }
    }

    private void OnDisable()
    {
        if (playSoundRoutine != null)
            StopCoroutine(playSoundRoutine);
    }


    private bool CollisionValid(Collision collision)
    {
        if (m_Colliders == null || m_Colliders.Length == 0)
        {
            return true;
        }

        foreach (var item in collision.contacts)
        {
            foreach (var myColliders in m_Colliders)
            {
                if (item.thisCollider.gameObject == myColliders.gameObject)
                {
                    return true;
                }
            }
        }
        return false;
    }


    void OnCollisionEnter(Collision collision)
    {
        if (CollisionValid(collision) == false)
        {
            return;
        }


        if (body == null)
        {
            body = GetComponent<Rigidbody>();
        }
        if (body == null)
        {
            return;
        }


        if (canPlaySound && collisionTriggers == (collisionTriggers | (1 << collision.gameObject.layer)))
        {
            if (source != null && source.enabled)
            {
                if (collision.collider.attachedRigidbody == null || collision.collider.attachedRigidbody.mass > 0.0000001f)
                {
                    if (clip != null || source.clip != null)
                        source.PlayOneShot(clip == null ? source.clip : clip, velocityVolumeCurve.Evaluate(collision.relativeVelocity.magnitude * velocityAmp) * (volumeAmp * m_VolumeAmpMultiplyer));
                    if (playSoundRoutine != null)
                        StopCoroutine(playSoundRoutine);
                    playSoundRoutine = StartCoroutine(SoundPlayBuffer());
                }
            }
        }
    }

    IEnumerator SoundPlayBuffer()
    {
        canPlaySound = false;
        yield return new WaitForSeconds(soundRepeatDelay);
        canPlaySound = true;
        playSoundRoutine = null;
    }

    IEnumerator SoundPlayBuffer(float time)
    {
        canPlaySound = false;
        yield return new WaitForSeconds(time);
        canPlaySound = true;
        playSoundRoutine = null;
    }
}
