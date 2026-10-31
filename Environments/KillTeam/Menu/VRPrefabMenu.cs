using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Autohand;

public class VRPrefabMenu : MonoBehaviour
{
    private Canvas canvas;
    private RectTransform buttonContainer;

    private Hand leftHand;
    private Hand rightHand;

    private readonly List<GameObject> buttons =
        new List<GameObject>();

    private const float CanvasWidth = 800f;
    private const float CanvasHeight = 500f;
    private const float CanvasScale = 0.001f;

    private const float ButtonSize = 150f;
    private const float ButtonSpacing = 10f;

    private const float SpawnDistance = 0.15f;

    // --------------------------------------------------
    // INITIALISE
    // --------------------------------------------------

    public void Initialise(
        Transform parent,
        Hand left,
        Hand right)
    {
        leftHand = left;
        rightHand = right;

        CreateCanvas(parent);
        CreateButtonContainer();
        EnsureEventSystem();
    }

    // --------------------------------------------------
    // CREATE CANVAS
    // --------------------------------------------------

    private void CreateCanvas(Transform parent)
    {
        GameObject obj = new GameObject(
            "VR Prefab Canvas",
            typeof(RectTransform),
            typeof(Canvas),
            typeof(CanvasScaler),
            typeof(GraphicRaycaster)
        );

        obj.transform.SetParent(parent, false);

        canvas = obj.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;

        RectTransform rect =
            obj.GetComponent<RectTransform>();

        rect.sizeDelta = new Vector2(
            CanvasWidth,
            CanvasHeight
        );

        rect.localScale =
            Vector3.one * CanvasScale;

        rect.localPosition = Vector3.zero;
        rect.localRotation = Quaternion.identity;

        CanvasScaler scaler =
            obj.GetComponent<CanvasScaler>();

        scaler.dynamicPixelsPerUnit = 10f;

        Image background = obj.AddComponent<Image>();
        background.color = new Color(0, 0, 0, 0.8f);
        background.raycastTarget = false;
    }

    // --------------------------------------------------
    // CREATE GRID
    // --------------------------------------------------

    private void CreateButtonContainer()
    {
        GameObject obj = new GameObject(
            "Button Container",
            typeof(RectTransform),
            typeof(GridLayoutGroup)
        );

        obj.transform.SetParent(
            canvas.transform,
            false
        );

        buttonContainer =
            obj.GetComponent<RectTransform>();

        buttonContainer.anchorMin = Vector2.zero;
        buttonContainer.anchorMax = Vector2.one;

        buttonContainer.offsetMin =
            new Vector2(20, 20);

        buttonContainer.offsetMax =
            new Vector2(-20, -20);

        GridLayoutGroup grid =
            obj.GetComponent<GridLayoutGroup>();

        grid.cellSize = new Vector2(
            ButtonSize,
            ButtonSize
        );

        grid.spacing = new Vector2(
            ButtonSpacing,
            ButtonSpacing
        );

        grid.constraint =
            GridLayoutGroup.Constraint.FixedColumnCount;

        grid.constraintCount = 4;

        grid.childAlignment =
            TextAnchor.UpperLeft;
    }

    // --------------------------------------------------
    // SET MENU ITEMS
    // --------------------------------------------------

    public void SetItems(List<VRMenuItem> items)
    {
        ClearButtons();

        if (items == null)
            return;

        foreach (VRMenuItem item in items)
        {
            if (item == null || item.prefab == null)
                continue;

            CreateButton(item);
        }
    }

    // --------------------------------------------------
    // CREATE BUTTON
    // --------------------------------------------------

    private void CreateButton(VRMenuItem item)
    {
        GameObject obj = new GameObject(
            item.prefab.name,
            typeof(RectTransform),
            typeof(Image),
            typeof(Button)
        );

        obj.transform.SetParent(
            buttonContainer,
            false
        );

        Image image =
            obj.GetComponent<Image>();

        image.sprite = item.thumbnail;
        image.color = Color.white;
        image.preserveAspect = true;
        image.raycastTarget = true;

        Button button =
            obj.GetComponent<Button>();

        button.targetGraphic = image;

        VRMenuItem capturedItem = item;

        button.onClick.AddListener(() =>
        {
            SpawnPrefab(
                capturedItem.prefab,
                rightHand
            );
        });

        buttons.Add(obj);
    }

    // --------------------------------------------------
    // SPAWN AND GRAB
    // --------------------------------------------------

    public GameObject SpawnPrefab(
        GameObject prefab,
        Hand hand)
    {
        if (prefab == null || hand == null)
            return null;

        Vector3 position =
            hand.transform.position +
            hand.transform.forward * SpawnDistance;

        GameObject instance = Instantiate(
            prefab,
            position,
            hand.transform.rotation
        );

        Grabbable grabbable =
            instance.GetComponent<Grabbable>();

        if (grabbable == null)
        {
            Debug.LogError(
                $"Missing Grabbable on {prefab.name}"
            );

            Destroy(instance);
            return null;
        }

        hand.TryGrab(grabbable);

        return instance;
    }

    // --------------------------------------------------
    // CLEAR MENU
    // --------------------------------------------------

    private void ClearButtons()
    {
        foreach (GameObject obj in buttons)
        {
            if (obj != null)
                Destroy(obj);
        }

        buttons.Clear();
    }

    // --------------------------------------------------
    // EVENT SYSTEM
    // --------------------------------------------------

    private void EnsureEventSystem()
    {
        if (EventSystem.current != null)
            return;

        new GameObject(
            "EventSystem",
            typeof(EventSystem),
            typeof(StandaloneInputModule)
        );
    }

    // --------------------------------------------------
    // CLEANUP
    // --------------------------------------------------

    private void OnDestroy()
    {
        if (canvas != null)
            Destroy(canvas.gameObject);
    }
}