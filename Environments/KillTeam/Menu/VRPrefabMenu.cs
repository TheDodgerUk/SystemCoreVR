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

    [Header("Spawn")]
    [SerializeField]
    private float spawnDistance = 0.5f;

    private Canvas canvas;
    private RectTransform gridRoot;
    private Image backgroundImage;

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

            canvasObject.transform.SetParent(
                transform, false);
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

        // Only set the initial scale for a new canvas.
        if (existing == null)
            rect.localScale = Vector3.one * canvasScale;

        // Create or update background
        CreateBackground(canvasObject);

        // Create or update grid
        CreateGrid(canvasObject);
    }

    // --------------------------------------------------
    // BACKGROUND
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
            backgroundImage =
                backgroundObject.AddComponent<Image>();

        backgroundImage.color = backgroundColor;
        backgroundImage.sprite = backgroundSprite;
        backgroundImage.enabled = showBackground;

        if (backgroundSprite != null &&
            backgroundSprite.border.sqrMagnitude > 0f)
        {
            backgroundImage.type = Image.Type.Sliced;
        }
        else
        {
            backgroundImage.type = Image.Type.Simple;
        }

        // Background must not block UI buttons.
        backgroundImage.raycastTarget = false;

        // Render behind all thumbnail buttons.
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

        // Ensure buttons render above the background.
        gridObject.transform.SetAsLastSibling();
    }

    // --------------------------------------------------
    // BUTTONS
    // --------------------------------------------------

    private void CreateButtons()
    {
        if (gridRoot == null)
            return;

        // Remove old buttons.
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
        GameObject buttonObject =
            new GameObject(
                entry.ShortName,
                typeof(RectTransform),
                typeof(Image),
                typeof(Button)
            );

        buttonObject.transform.SetParent(
            gridRoot, false);

        Image image =
            buttonObject.GetComponent<Image>();

        image.color = Color.white;
        image.preserveAspect = true;
        image.raycastTarget = true;

        Button button =
            buttonObject.GetComponent<Button>();

        button.targetGraphic = image;

        string prefabGuid = entry.Guid;

        button.onClick.AddListener(() =>
        {
            SpawnPrefab(prefabGuid);
        });

        // Load thumbnail from existing AssetBundle system.
        Core.AssetBundlesRef
            .CatalogueThumbnailAssetBundleRef
            .GetItem(Core.Mono, entry.Guid, (texture) =>
            {
                // The button may have been destroyed while
                // the thumbnail was loading.
                if (image == null)
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

                    image.sprite = sprite;
                    image.color = Color.white;
                    image.preserveAspect = true;
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
}