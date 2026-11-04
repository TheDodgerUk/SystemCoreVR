
using KillTeam;
using System;
using System.Collections.Generic;
using UnityEngine;

public class PhysicalButtons : MonoBehaviour
{
    public static PhysicalButtons Instance { get; private set; }

    public enum ButtonType
    {
        Next,
        Left,
        Right
    }

    private const string BUTTON_NAME = "buttonDome";
    private const string SOUND_NAME = "ButtonClick";

    [Header("Button Settings")]
    [SerializeField, Min(0f)]
    private float buttonCooldown = 1f;

    [Header("Audio Settings")]
    [SerializeField, Range(0f, 1f)]
    private float audioVolume = 1f;

    [SerializeField, Min(0.01f)]
    private float audioMinDistance = 0.1f;

    [SerializeField, Min(0.1f)]
    private float audioMaxDistance = 5f;

    private readonly Dictionary<
        (KillTeamGameManager.Player player, ButtonType button),
        Action> buttonCallbacks = new();

    private readonly Dictionary<
        (KillTeamGameManager.Player player, ButtonType button),
        float> lastButtonPress = new();

    private AudioClip audioClip;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        Core.AssetBundlesRef.AudioClipAssetBundleRef.GetItem(
            Core.Mono,
            SOUND_NAME,
            (item) =>
            {
                if (this == null)
                {
                    Debug.LogError($"PhysicalButtons , could not find {SOUND_NAME}");
                    return;
                }

                audioClip = item;
                SetupButtons();
            }
        );
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public void OnButtonPressed(
        KillTeamGameManager.Player player,
        ButtonType button,
        Action callback)
    {
        buttonCallbacks[(player, button)] = callback;
    }

    public void ClearButtonPressed(
        KillTeamGameManager.Player player,
        ButtonType button)
    {
        buttonCallbacks.Remove((player, button));
    }

    public void ClearAllButtonCallbacks()
    {
        buttonCallbacks.Clear();
    }

    private void InvokeButtonPressed(
        KillTeamGameManager.Player player,
        ButtonType button,
        AudioSource audio)
    {
        var key = (player, button);

        float currentTime = Time.unscaledTime;

        // Ignore repeated presses during cooldown.
        if (lastButtonPress.TryGetValue(
            key, out float lastTime))
        {
            if (currentTime - lastTime < buttonCooldown)
                return;
        }

        // Start cooldown.
        lastButtonPress[key] = currentTime;

        // Play sound on the button that was pressed.
        if (audio != null && audio.clip != null)
        {
            audio.PlayOneShot(audio.clip);
        }

        // Invoke registered callback if available.
        if (buttonCallbacks.TryGetValue(
            key, out Action callback))
        {
            callback?.Invoke();
        }
    }

    private void SetupButtons()
    {
        foreach (Transform child in transform)
        {
            // Expected names:
            // Player1_Next
            // Player1_Left
            // Player1_Right
            // Player2_Next
            // Player2_Left
            // Player2_Right

            string[] parts = child.name.Split('_');

            if (parts.Length != 2)
                continue;

            if (!Enum.TryParse(
                parts[0],
                out KillTeamGameManager.Player player))
            {
                continue;
            }

            if (!Enum.TryParse(
                parts[1],
                out ButtonType buttonType))
            {
                continue;
            }

            // Get VrInteraction directly from the child.
            VrInteraction interaction =
                child.GetComponent<VrInteraction>();

            if (interaction == null)
            {
                Debug.LogError(
                    $"VrInteraction missing on {child.name}",
                    child
                );
                continue;
            }
            
            VrInteractionBaseButton button =
                interaction.GetVrInteractionFromRoot(
                    MetaDataType.ContentButton,
                    BUTTON_NAME // gamepbject name
                ) as VrInteractionBaseButton;

            if (button == null)
            {
                Debug.LogError(
                    $"{BUTTON_NAME} not found inside {child.name}",
                    child
                );
                continue;
            }

            // Add or reuse AudioSource on this button.
            AudioSource audio =
                child.GetComponent<AudioSource>();

            if (audio == null)
            {
                audio = child.gameObject
                    .AddComponent<AudioSource>();
            }

            // Configure 3D positional audio.
            audio.clip = audioClip;
            audio.playOnAwake = false;
            audio.spatialBlend = 1f;
            audio.volume = audioVolume;
            audio.minDistance = audioMinDistance;
            audio.maxDistance = audioMaxDistance;
            audio.rolloffMode =
                AudioRolloffMode.Logarithmic;

            // Capture values for this specific button.
            KillTeamGameManager.Player capturedPlayer = player;
            ButtonType capturedButton = buttonType;
            AudioSource capturedAudio = audio;

            button.ClearAddCallback(pressed =>
            {
                if (pressed ==
                    VrInteractionBaseButton.ButtonStateEnum.Down)
                {
                    InvokeButtonPressed(
                        capturedPlayer,
                        capturedButton,
                        capturedAudio
                    );
                }
            });
        }
    }
}
