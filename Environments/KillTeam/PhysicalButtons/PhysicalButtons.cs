
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

    [Header("Button Settings")]
    [SerializeField, Min(0f)]
    private float buttonCooldown = 1f;

    private readonly Dictionary<
        (KillTeamGameManager.Player player, ButtonType button),
        Action> buttonCallbacks =
        new Dictionary<
            (KillTeamGameManager.Player player, ButtonType button),
            Action>();

    private readonly Dictionary<
        (KillTeamGameManager.Player player, ButtonType button),
        float> lastButtonPress =
        new Dictionary<
            (KillTeamGameManager.Player player, ButtonType button),
            float>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        SetupButtons();
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
        ButtonType button)
    {
        var key = (player, button);

        float currentTime = Time.unscaledTime;

        // Ignore repeated presses during cooldown.
        if (lastButtonPress.TryGetValue(
            key,
            out float lastTime))
        {
            if (currentTime - lastTime < buttonCooldown)
                return;
        }

        if (buttonCallbacks.TryGetValue(
            key,
            out Action callback))
        {
            if (callback == null)
                return;

            // Start cooldown before invoking callback.
            lastButtonPress[key] = currentTime;

            callback.Invoke();
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

            Transform buttonRoot =
                child.FindChildRecursive("ButtonBase1_Prefab");

            if (buttonRoot == null)
            {
                Debug.LogError(
                    $"ButtonBase1_Prefab not found inside {child.name}",
                    child
                );
                continue;
            }

            VrInteraction interaction =
                buttonRoot.GetComponent<VrInteraction>();

            if (interaction == null)
            {
                Debug.LogError(
                    $"VrInteraction missing on {buttonRoot.name}",
                    buttonRoot
                );
                continue;
            }

            VrInteractionBaseButton button =
                interaction.GetVrInteractionFromRoot(
                    MetaDataType.ContentButton,
                    "Button_Off"
                ) as VrInteractionBaseButton;

            if (button == null)
            {
                Debug.LogError(
                    $"Button_Off not found inside {child.name}",
                    child
                );
                continue;
            }

            // Capture values for this specific button.
            KillTeamGameManager.Player capturedPlayer = player;
            ButtonType capturedButton = buttonType;

            button.ClearAddCallback(pressed =>
            {
                if (pressed ==
                    VrInteractionBaseButton.ButtonStateEnum.Down)
                {
                    InvokeButtonPressed(
                        capturedPlayer,
                        capturedButton
                    );
                }
            });
        }
    }
}
