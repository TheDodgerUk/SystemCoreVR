using UnityEngine;

public class ObjectInteraction : MonoBehaviour
{
    public enum InteractionState
    {
        Player1Interaction =24,
        Player2Interaction =25 ,
        NoInteraction =14,

    }

    int layerInitilisedstate;

    public void InitiliseState(InteractionState state)
    {
        layerInitilisedstate = (int)state;

        if (layerInitilisedstate == -1)
        {
            Debug.LogError(
                $"Create the '{state}' layer in Unity first.",
                this
            );

            return;
        }

        SetLayerRecursive(transform, layerInitilisedstate);
    }
    public void Enable()
    {
        SetLayerRecursive(transform, layerInitilisedstate);
    }
    public void Disable()
    {
        SetLayerRecursive(transform, (int)InteractionState.NoInteraction);
    }

    private static void SetLayerRecursive(
        Transform target,
        int layer
    )
    {
        target.gameObject.layer = layer;

        foreach (Transform child in target)
        {
            SetLayerRecursive(child, layer);
        }
    }
}