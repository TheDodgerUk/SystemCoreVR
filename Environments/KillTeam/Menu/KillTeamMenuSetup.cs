using System.Collections.Generic;
using UnityEngine;
using Autohand;

public class KillTeamMenuSetup : MonoBehaviour
{
    [Header("VR Hands")]
    [SerializeField] private Hand leftHand;
    [SerializeField] private Hand rightHand;

    [Header("Menu Items")]
    [SerializeField] private Sprite wallThumbnail;
    [SerializeField] private GameObject wallPrefab;

    [SerializeField] private Sprite buildingThumbnail;
    [SerializeField] private GameObject buildingPrefab;

    [SerializeField] private Sprite containerThumbnail;
    [SerializeField] private GameObject containerPrefab;

    private VRPrefabMenu menu;

    private void Start()
    {
        CreateMenu();
    }

    private void CreateMenu()
    {
        menu = gameObject.AddComponent<VRPrefabMenu>();

        menu.Initialise(
            transform,
            leftHand,
            rightHand
        );

        List<VRMenuItem> items =
            new List<VRMenuItem>
        {
            new VRMenuItem(
                wallThumbnail,
                wallPrefab
            ),

            new VRMenuItem(
                buildingThumbnail,
                buildingPrefab
            ),

            new VRMenuItem(
                containerThumbnail,
                containerPrefab
            )
        };

        menu.SetItems(items);
    }

    // Call this from another system to replace
    // the contents of the menu.

    public void UpdateMenu(List<VRMenuItem> items)
    {
        if (menu == null)
            return;

        menu.SetItems(items);
    }

    public void ShowMenu(bool visible)
    {
        if (menu != null)
            menu.gameObject.SetActive(visible);
    }
}