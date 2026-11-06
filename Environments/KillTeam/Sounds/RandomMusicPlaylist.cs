
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomMusicPlaylist : MonoBehaviour
{
    [Header("Music Tracks")]
    [SerializeField]
    private List<AudioClip> tracks = new List<AudioClip>();

    [Header("Settings")]
    [SerializeField, Range(0f, 1f)]
    private float volume = 0.7f;

    [SerializeField, Min(0f)]
    private float fadeDuration = 3f;

    [SerializeField]
    private bool playOnStart = true;

    [SerializeField]
    private bool shuffle = true;

    private AudioSource[] sources = new AudioSource[2];

    private List<int> playlist = new List<int>();
    private int playlistIndex;
    private int activeSourceIndex;

    private Coroutine musicCoroutine;
    public static RandomMusicPlaylist Instance { get; private set; }

    const string BACKGROUND_NAME = "Background";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);

        for (int i = 0; i < 2; i++)
        {
            sources[i] = gameObject.AddComponent<AudioSource>();

            sources[i].playOnAwake = false;
            sources[i].loop = false;
            sources[i].spatialBlend = 0f;
            sources[i].volume = 0f;
        }
        LoadTracks();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    private void LoadTracks()
    {
        Core.AssetBundlesRef.AudioClipAssetBundleRef.GetItemList(Core.Mono, (itemList) =>
        {
            List<string> names = new List<string>();
            foreach (var item in itemList)
            {
                if (item.CaseInsensitiveContains(BACKGROUND_NAME))
                {
                    names.Add(item);
                }
            }
            TaskAction task = new TaskAction(names.Count, () =>
            {
                Play();
            });


            for (int i = 0; i < names.Count; i++)
            {

                Core.AssetBundlesRef.AudioClipAssetBundleRef.GetItem(Core.Mono, names[i], (clip) =>
                {
                    if (clip.name.Contains(BACKGROUND_NAME))
                    {
                        tracks.Add(clip);
                    }
                    task.Increment();

                });
            }
        });
    }

    public void Play()
    {
        if (tracks.Count == 0)
            return;

        Stop();

        BuildPlaylist();

        musicCoroutine = StartCoroutine(PlayPlaylist());
    }

    public void Stop()
    {
        if (musicCoroutine != null)
        {
            StopCoroutine(musicCoroutine);
            musicCoroutine = null;
        }

        foreach (AudioSource source in sources)
        {
            if (source != null)
            {
                source.Stop();
                source.volume = 0f;
            }
        }
    }

    public void StopWithFade()
    {
        if (musicCoroutine != null)
        {
            StopCoroutine(musicCoroutine);
            musicCoroutine = null;
        }

        StartCoroutine(FadeOutAll());
    }

    private void BuildPlaylist()
    {
        playlist.Clear();

        for (int i = 0; i < tracks.Count; i++)
        {
            if (tracks[i] != null)
                playlist.Add(i);
        }

        if (shuffle)
        {
            for (int i = playlist.Count - 1; i > 0; i--)
            {
                int randomIndex = Random.Range(0, i + 1);

                int temp = playlist[i];
                playlist[i] = playlist[randomIndex];
                playlist[randomIndex] = temp;
            }
        }

        playlistIndex = 0;
    }

    private AudioClip GetNextTrack()
    {
        if (playlistIndex >= playlist.Count)
            BuildPlaylist();

        if (playlist.Count == 0)
            return null;

        return tracks[playlist[playlistIndex++]];
    }

    private IEnumerator PlayPlaylist()
    {
        AudioClip firstClip = GetNextTrack();

        if (firstClip == null)
            yield break;

        AudioSource current = sources[activeSourceIndex];

        current.clip = firstClip;
        current.volume = 0f;
        current.Play();

        yield return Fade(current, 0f, volume, fadeDuration);

        while (true)
        {
            float remainingTime =
                current.clip.length - current.time;

            float waitTime =
                Mathf.Max(0f, remainingTime - fadeDuration);

            yield return new WaitForSeconds(waitTime);

            AudioClip nextClip = GetNextTrack();

            if (nextClip == null)
                yield break;

            int nextSourceIndex = 1 - activeSourceIndex;

            AudioSource next = sources[nextSourceIndex];

            next.clip = nextClip;
            next.volume = 0f;
            next.Play();

            yield return CrossFade(
                current,
                next,
                fadeDuration
            );

            current.Stop();

            activeSourceIndex = nextSourceIndex;
            current = next;
        }
    }

    private IEnumerator Fade(
        AudioSource source,
        float from,
        float to,
        float duration)
    {
        if (duration <= 0f)
        {
            source.volume = to;
            yield break;
        }

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / duration);

            source.volume = Mathf.Lerp(from, to, t);

            yield return null;
        }

        source.volume = to;
    }

    private IEnumerator CrossFade(
        AudioSource from,
        AudioSource to,
        float duration)
    {
        if (duration <= 0f)
        {
            from.volume = 0f;
            to.volume = volume;
            yield break;
        }

        float elapsed = 0f;
        float startVolume = from.volume;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / duration);

            from.volume = Mathf.Lerp(
                startVolume, 0f, t);

            to.volume = Mathf.Lerp(
                0f, volume, t);

            yield return null;
        }

        from.volume = 0f;
        to.volume = volume;
    }

    private IEnumerator FadeOutAll()
    {
        float[] startVolumes =
        {
            sources[0].volume,
            sources[1].volume
        };

        float elapsed = 0f;
        float duration = Mathf.Max(0f, fadeDuration);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / duration);

            for (int i = 0; i < 2; i++)
            {
                sources[i].volume =
                    Mathf.Lerp(startVolumes[i], 0f, t);
            }

            yield return null;
        }

        foreach (AudioSource source in sources)
        {
            source.Stop();
            source.volume = 0f;
        }
    }
}
