
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VRPrefabMenu : MonoBehaviour
{
    [Header("Prefab Data")]
    [SerializeField]
    private List<CatalogueEntry> entries =
        new List<CatalogueEntry>();

    [Header("VR Camera")]
    [SerializeField]
    private Transform vrCamera;

    [Header("Canvas")]
    [SerializeField]
    private Vector2 canvasSize = new Vector2(600, 400);

    [SerializeField]
    private float canvasScale = 0.001f;

    [Header("Background")]
    [SerializeField]
    private bool showBackground = true;

    [SerializeField]
    private Color backgroundColor =
        new Color(0.08f, 0.08f, 0.08f, 0.9f);

    [SerializeField]
    private Sprite backgroundSprite;

    [Header("Buttons")]
    [SerializeField]
    private Vector2 buttonSize = new Vector2(100, 100);

    [SerializeField]
    private Vector2 spacing = new Vector2(10, 10);

    [SerializeField]
    private int columns = 5;

    [SerializeField]
    private float gridPadding = 15f;

    [Header("Button Background")]
    [SerializeField]
    private Color buttonBackgroundColor = Color.blue;

    [SerializeField]
    private Vector2 thumbnailPadding = new Vector2(5f, 5f);

    [Header("Spawn")]
    [SerializeField]
    private float spawnDistance = 0.5f;

    private Canvas canvas;
    private RectTransform gridRoot;
    private Image backgroundImage;

    private readonly List<Image> buttonBackgrounds =
        new List<Image>();

    [Header("Inspector Testing")]
    [SerializeField, Min(0)]
    private int testButtonIndex = 0;

    private readonly List<Button> createdButtons = new List<Button>();

    public void TestButtonClick()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("Test Button Click requires Play Mode.", this);
            return;
        }

        if (testButtonIndex < 0 || testButtonIndex >= createdButtons.Count)
        {
            Debug.LogWarning($"Invalid test button index {testButtonIndex}. Created buttons: {createdButtons.Count}. Call SetEntries first.", this);
            return;
        }

        Button button = createdButtons[testButtonIndex];
        if (button == null)
        {
            Debug.LogWarning($"Button {testButtonIndex} no longer exists.", this);
            return;
        }

        Debug.Log($"Testing button {testButtonIndex}: {button.name}", button);
        button.onClick.Invoke();
    }

    private const string CanvasName = "VR Prefab Canvas";
    private const string BackgroundName = "Background";
    private const string GridName = "Thumbnail Grid";

    // --------------------------------------------------
    // PUBLIC
    // --------------------------------------------------

    public void SetEntries(List<CatalogueEntry> newEntries)
    {
        if (vrCamera == null && Camera.main != null)
            vrCamera = Camera.main.transform;

        entries = newEntries ?? new List<CatalogueEntry>();

        CreateCanvas();
        CreateButtons();
    }

    private void Update()
    {
        if (Camera.main == null)
            return;

        Vector3 direction =
            Camera.main.transform.position - transform.position;

        direction.y = 0f;

        if (direction.sqrMagnitude < 0.0001f)
            return;

        transform.rotation = Quaternion.LookRotation(direction);
    }

    // --------------------------------------------------
    // CANVAS
    // --------------------------------------------------

    private void CreateCanvas()
    {
        Transform existing = transform.Find(CanvasName);

        GameObject canvasObject;

        if (existing != null)
        {
            canvasObject = existing.gameObject;
        }
        else
        {
            canvasObject = new GameObject(
                CanvasName,
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster)
            );

            canvasObject.transform.SetParent(transform, false);
        }

        canvas = canvasObject.GetComponent<Canvas>();

        if (canvas == null)
            canvas = canvasObject.AddComponent<Canvas>();

        canvas.renderMode = RenderMode.WorldSpace;

        if (canvasObject.GetComponent<CanvasScaler>() == null)
            canvasObject.AddComponent<CanvasScaler>();

        if (canvasObject.GetComponent<GraphicRaycaster>() == null)
            canvasObject.AddComponent<GraphicRaycaster>();

        RectTransform rect =
            canvasObject.GetComponent<RectTransform>();

        rect.sizeDelta = canvasSize;

        if (existing == null)
            rect.localScale = Vector3.one * canvasScale;

        CreateBackground(canvasObject);
        CreateGrid(canvasObject);
    }

    // --------------------------------------------------
    // CANVAS BACKGROUND
    // --------------------------------------------------

    private void CreateBackground(GameObject canvasObject)
    {
        Transform existing =
            canvasObject.transform.Find(BackgroundName);

        GameObject backgroundObject;

        if (existing != null)
        {
            backgroundObject = existing.gameObject;
        }
        else
        {
            backgroundObject = new GameObject(
                BackgroundName,
                typeof(RectTransform),
                typeof(Image)
            );

            backgroundObject.transform.SetParent(
                canvasObject.transform, false);
        }

        RectTransform rect =
            backgroundObject.GetComponent<RectTransform>();

        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;

        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        rect.localPosition = Vector3.zero;
        rect.localRotation = Quaternion.identity;
        rect.localScale = Vector3.one;

        backgroundImage =
            backgroundObject.GetComponent<Image>();

        if (backgroundImage == null)
            backgroundImage = backgroundObject.AddComponent<Image>();

        backgroundImage.color = backgroundColor;
        backgroundImage.sprite = backgroundSprite;
        backgroundImage.enabled = showBackground;

        backgroundImage.type =
            backgroundSprite != null &&
            backgroundSprite.border.sqrMagnitude > 0f
                ? Image.Type.Sliced
                : Image.Type.Simple;

        backgroundImage.raycastTarget = false;

        backgroundObject.transform.SetAsFirstSibling();
    }

    // --------------------------------------------------
    // GRID
    // --------------------------------------------------

    private void CreateGrid(GameObject canvasObject)
    {
        Transform existing =
            canvasObject.transform.Find(GridName);

        GameObject gridObject;

        if (existing != null)
        {
            gridObject = existing.gameObject;
        }
        else
        {
            gridObject = new GameObject(
                GridName,
                typeof(RectTransform),
                typeof(GridLayoutGroup)
            );

            gridObject.transform.SetParent(
                canvasObject.transform, false);
        }

        gridRoot =
            gridObject.GetComponent<RectTransform>();

        gridRoot.anchorMin = Vector2.zero;
        gridRoot.anchorMax = Vector2.one;

        gridRoot.offsetMin =
            new Vector2(gridPadding, gridPadding);

        gridRoot.offsetMax =
            new Vector2(-gridPadding, -gridPadding);

        gridRoot.localRotation = Quaternion.identity;
        gridRoot.localScale = Vector3.one;

        GridLayoutGroup grid =
            gridObject.GetComponent<GridLayoutGroup>();

        if (grid == null)
            grid = gridObject.AddComponent<GridLayoutGroup>();

        grid.cellSize = buttonSize;
        grid.spacing = spacing;

        grid.constraint =
            GridLayoutGroup.Constraint.FixedColumnCount;

        grid.constraintCount = Mathf.Max(1, columns);

        grid.childAlignment = TextAnchor.UpperLeft;

        grid.startCorner =
            GridLayoutGroup.Corner.UpperLeft;

        grid.startAxis =
            GridLayoutGroup.Axis.Horizontal;

        grid.padding = new RectOffset(0, 0, 0, 0);

        gridObject.transform.SetAsLastSibling();
    }

    // --------------------------------------------------
    // BUTTONS
    // --------------------------------------------------

    private void CreateButtons()
    {
        if (gridRoot == null)
            return;

        buttonBackgrounds.Clear();
        createdButtons.Clear();

        for (int i = gridRoot.childCount - 1; i >= 0; i--)
        {
            GameObject child =
                gridRoot.GetChild(i).gameObject;

            child.transform.SetParent(null);

            if (Application.isPlaying)
                Destroy(child);
            else
                DestroyImmediate(child);
        }

        foreach (CatalogueEntry entry in entries)
        {
            if (entry == null)
                continue;

            CreateButton(entry);
        }
    }

    private void CreateButton(CatalogueEntry entry)
    {
        // Main button - blue background
        GameObject buttonObject = new GameObject(
            entry.ShortName,
            typeof(RectTransform),
            typeof(Image),
            typeof(Button)
        );

        buttonObject.transform.SetParent(gridRoot, false);

        Image buttonBackground =
            buttonObject.GetComponent<Image>();

        buttonBackground.color = buttonBackgroundColor;
        buttonBackground.raycastTarget = true;

        buttonBackgrounds.Add(buttonBackground);

        Button button =
            buttonObject.GetComponent<Button>();

        createdButtons.Add(button);
        button.targetGraphic = buttonBackground;

        // Keep the chosen background colour visible.
        // Button colour transitions would otherwise tint it.
        button.transition = Selectable.Transition.None;

        string prefabGuid = entry.Guid;

        button.onClick.AddListener(() =>
        {
            SpawnPrefab(prefabGuid);
        });

        // ----------------------------------------------
        // THUMBNAIL CHILD
        // ----------------------------------------------

        GameObject thumbnailObject = new GameObject(
            "Thumbnail",
            typeof(RectTransform),
            typeof(Image)
        );

        thumbnailObject.transform.SetParent(
            buttonObject.transform, false);

        RectTransform thumbnailRect =
            thumbnailObject.GetComponent<RectTransform>();

        thumbnailRect.anchorMin = Vector2.zero;
        thumbnailRect.anchorMax = Vector2.one;

        thumbnailRect.offsetMin = thumbnailPadding;
        thumbnailRect.offsetMax = -thumbnailPadding;

        thumbnailRect.localRotation = Quaternion.identity;
        thumbnailRect.localScale = Vector3.one;

        Image thumbnailImage =
            thumbnailObject.GetComponent<Image>();

        thumbnailImage.color = Color.white;
        thumbnailImage.preserveAspect = true;

        // Let the parent button receive clicks.
        thumbnailImage.raycastTarget = false;

        // ----------------------------------------------
        // LOAD THUMBNAIL
        // ----------------------------------------------

        Core.AssetBundlesRef
            .CatalogueThumbnailAssetBundleRef
            .GetItem(Core.Mono, entry.Guid, (texture) =>
            {
                if (thumbnailImage == null)
                    return;

                if (texture != null)
                {
                    Sprite sprite = Sprite.Create(
                        texture,
                        new Rect(
                            0,
                            0,
                            texture.width,
                            texture.height
                        ),
                        new Vector2(0.5f, 0.5f)
                    );

                    thumbnailImage.sprite = sprite;
                    thumbnailImage.color = Color.white;
                    thumbnailImage.preserveAspect = true;
                }
                else
                {
                    Debug.LogError(
                        $"Can't find Thumbnail for " +
                        $"{entry.FullName} {entry.Guid}"
                    );
                }
            });
    }

    // --------------------------------------------------
    // SPAWN
    // --------------------------------------------------

    public void SpawnPrefab(string prefabGuid)
    {
        if (string.IsNullOrEmpty(prefabGuid))
            return;

        if (vrCamera == null)
        {
            Debug.LogError("VR Camera not assigned.");
            return;
        }

        Vector3 position =
            vrCamera.position +
            vrCamera.forward * spawnDistance;

        Core.Scene.SpawnObject(prefabGuid, (item) =>
        {
            if (item == null)
                return;

            item.transform.position = position;
            item.transform.rotation = Quaternion.identity;

            Debug.Log($"Spawned: {item.name}");
        });
    }

    // --------------------------------------------------
    // BACKGROUND CONTROLS
    // --------------------------------------------------

    public void SetBackgroundColor(Color color)
    {
        backgroundColor = color;

        if (backgroundImage != null)
            backgroundImage.color = color;
    }

    public void SetBackgroundSprite(Sprite sprite)
    {
        backgroundSprite = sprite;

        if (backgroundImage != null)
        {
            backgroundImage.sprite = sprite;

            backgroundImage.type =
                sprite != null &&
                sprite.border.sqrMagnitude > 0f
                    ? Image.Type.Sliced
                    : Image.Type.Simple;
        }
    }

    public void ShowBackground(bool visible)
    {
        showBackground = visible;

        if (backgroundImage != null)
            backgroundImage.enabled = visible;
    }

    // --------------------------------------------------
    // BUTTON COLOUR CONTROLS
    // --------------------------------------------------

    public void SetButtonBackgroundColor(Color color)
    {
        buttonBackgroundColor = color;

        foreach (Image image in buttonBackgrounds)
        {
            if (image != null)
                image.color = color;
        }
    }

    public void SetButtonBackgroundColor(int index, Color color)
    {
        if (index < 0 || index >= buttonBackgrounds.Count)
            return;

        Image image = buttonBackgrounds[index];

        if (image != null)
            image.color = color;
    }
}
