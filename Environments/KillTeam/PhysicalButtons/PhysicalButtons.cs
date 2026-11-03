using KillTeam;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Oculus.Interaction.Context;

public class PhysicalButtons : MonoBehaviour
{
    public static PhysicalButtons Instance { get; private set; }


    public enum ButtonType
    {
        Next,
        Left,
        Right
    }

    private readonly Dictionary<(KillTeamGameManager.Player player, ButtonType button), Action>
        buttonCallbacks =
            new Dictionary<(KillTeamGameManager.Player player, ButtonType button), Action>();

    public void OnButtonPressed(
        KillTeamGameManager.Player player,
        ButtonType button,
        Action callback)
    {
        buttonCallbacks[(player, button)] = callback;
    }

    public void ClearButtonPressed(KillTeamGameManager.Player player, ButtonType button)
    {
        buttonCallbacks.Remove((player, button));
    }

    public void ClearAllButtonCallbacks()
    {
        buttonCallbacks.Clear();
    }

    private void InvokeButtonPressed(KillTeamGameManager.Player player, ButtonType button)
    {
        if (buttonCallbacks.TryGetValue((player, button), out Action callback))
        {
            callback?.Invoke();
        }
    }



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

    private void SetupButtons()
    {
        foreach (Transform child in transform)
        {
            // Example: "Player1_Next"
            string[] parts = child.name.Split('_');

            if (parts.Length != 2)
                continue;

            if (!System.Enum.TryParse(
                    parts[0],
                    out KillTeamGameManager.Player player))
            {
                continue;
            }

            if (!System.Enum.TryParse(
                    parts[1],
                    out ButtonType buttonType))
            {
                continue;
            }

            // Find the button prefab inside this specific child.
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

            var button = interaction.GetVrInteractionFromRoot(
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

            // Capture values separately for each callback.
            KillTeamGameManager.Player capturedPlayer = player;
            ButtonType capturedButton = buttonType;

            button.ClearAddCallback(pressed =>
            {
                if (pressed == VrInteractionBaseButton.ButtonStateEnum.Down)
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
