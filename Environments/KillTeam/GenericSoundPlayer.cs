
using System;
using System.Collections.Generic;
using UnityEngine;

public class GenericSoundPlayer : MonoBehaviour
{
    public static GenericSoundPlayer Instance { get; private set; }

    public enum SoundType
    {
        PassSound,
        FailSound,
        Hit
    }

    private Dictionary<SoundType, AudioClip> audioClips =
        new Dictionary<SoundType, AudioClip>();

    [Header("Audio Settings")]
    [SerializeField] private int sourceCount = 5;
    [SerializeField, Range(0f, 1f)] private float volume = 1f;
    [SerializeField, Range(0f, 1f)] private float spatialBlend = 1f;

    private AudioSource[] audioSources;
    private int nextSourceIndex = 0;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        CreateAudioSources();
        LoadSounds();
    }

    private void CreateAudioSources()
    {
        audioSources = new AudioSource[sourceCount];

        for (int i = 0; i < sourceCount; i++)
        {
            GameObject child = new GameObject($"AudioSource_{i}");

            child.transform.SetParent(transform, false);

            AudioSource source = child.AddComponent<AudioSource>();

            source.playOnAwake = false;
            source.loop = false;
            source.spatialBlend = spatialBlend;
            source.volume = volume;

            audioSources[i] = source;
        }
    }

    private void LoadSounds()
    {
        foreach (SoundType soundType in Enum.GetValues(typeof(SoundType)))
        {
            string soundName = soundType.ToString();

            Core.AssetBundlesRef.AudioClipAssetBundleRef.GetItem(
                Core.Mono,
                soundName,
                (item) =>
                {
                    if (item == null)
                    {
                        Debug.Log($"Sound not found: {soundName}");
                        return;
                    }

                    audioClips[soundType] = item;
                }
            );
        }
    }

    public void PlaySound(SoundType soundType, Vector3 position)
    {
        if (!audioClips.TryGetValue(soundType, out AudioClip clip))
        {
            Debug.LogWarning($"Sound not loaded: {soundType}");
            return;
        }

        if (audioSources == null || audioSources.Length == 0)
            return;

        AudioSource source = audioSources[nextSourceIndex];

        // Move source to requested world position.
        source.transform.position = position;

        // Stop previous sound if this source is still playing.
        source.Stop();

        source.clip = clip;
        source.Play();

        // Move to next source, wrapping back to zero.
        nextSourceIndex =
            (nextSourceIndex + 1) % audioSources.Length;
    }
}
