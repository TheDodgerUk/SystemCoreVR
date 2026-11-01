
using glTFLoader2.Schema;
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
    private Vector2 buttonSizeStart = new Vector2(100, 100);


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

    [Header("Inspector Testing")]
    [SerializeField, Min(0)]
    private int testButtonIndex = 0;

    private Canvas canvas;
    private RectTransform gridRoot;
    private Image backgroundImage;

    private readonly List<Image> buttonBackgrounds =
        new List<Image>();

    private readonly List<Button> createdButtons =
        new List<Button>();

    private readonly List<Sprite> generatedSprites =
        new List<Sprite>();

    private const string CanvasName = "VR Prefab Canvas";
    private const string BackgroundName = "Background";
    private const string GridName = "Thumbnail Grid";

    // Incremented whenever the menu is rebuilt.
    // Prevents old asynchronous thumbnail requests
    // from modifying the new menu.
    private int menuVersion = 0;

    // ----------------------------------------------
    // EVENTS
    // ----------------------------------------------

    public event Action<int, Texture2D> OnTextureSelected;

    // ----------------------------------------------
    // SET ENTRIES - CATALOGUE
    // ----------------------------------------------

    public void SetEntries(List<CatalogueEntry> newEntries)
    {
        SetupCamera();
        buttonSize = buttonSizeStart;
        entries = newEntries != null
            ? new List<CatalogueEntry>(newEntries)
            : new List<CatalogueEntry>();

        RebuildCanvas();
        CreateButtons();
    }

    // ----------------------------------------------
    // SET ENTRIES - TEXTURES
    // ----------------------------------------------

    public void SetEntries(List<Texture2D> textures)
    {
        SetEntries(textures, null);
    }

    // ----------------------------------------------
    // SET ENTRIES - TEXTURES WITH CALLBACK
    // ----------------------------------------------

    public void SetEntries(
        List<Texture2D> textures,
        Action<int, Texture2D> onButtonClicked)
    {

        buttonSize = buttonSizeStart;



        float aspectRatio = (float)textures[0].width / textures[0].height;

        if (aspectRatio > 1f)
        {
            // Wider than tall
            buttonSize = new Vector2(buttonSize.x, buttonSize.y / aspectRatio);
        }
        else
        {
            // Taller than wide
            buttonSize = new Vector2(buttonSize.x * aspectRatio, buttonSize.y);
        }



        SetupCamera();

        entries.Clear();

        RebuildCanvas();

        if (textures == null)
            return;

        for (int i = 0; i < textures.Count; i++)
        {
            Texture2D texture = textures[i];

            if (texture == null)
                continue;

            CreateTextureButton(
                texture,
                i,
                onButtonClicked
            );
        }
    }

    // ----------------------------------------------
    // CAMERA
    // ----------------------------------------------

    private void SetupCamera()
    {
        if (vrCamera == null && Camera.main != null)
            vrCamera = Camera.main.transform;
    }

    // ----------------------------------------------
    // REBUILD CANVAS
    // ----------------------------------------------

    private void RebuildCanvas()
    {
        ClearCanvas();
        CreateCanvas();
    }

    // ----------------------------------------------
    // CLEAR CANVAS
    // ----------------------------------------------

    private void ClearCanvas()
    {
        menuVersion++;

        buttonBackgrounds.Clear();
        createdButtons.Clear();

        canvas = null;
        gridRoot = null;
        backgroundImage = null;

        Transform existing = transform.Find(CanvasName);

        if (existing != null)
        {
            GameObject canvasObject = existing.gameObject;

            // Immediately disable and detach the old canvas.
            canvasObject.SetActive(false);
            canvasObject.transform.SetParent(null);

            if (Application.isPlaying)
                Destroy(canvasObject);
            else
                DestroyImmediate(canvasObject);
        }

        // Destroy sprites created by this class.
        // Do not destroy the source Texture2D objects.
        foreach (Sprite sprite in generatedSprites)
        {
            if (sprite == null)
                continue;

            if (Application.isPlaying)
                Destroy(sprite);
            else
                DestroyImmediate(sprite);
        }

        generatedSprites.Clear();
    }

    private void OnDestroy()
    {
        ClearCanvas();
    }

    // ----------------------------------------------
    // LOOK AT CAMERA
    // ----------------------------------------------

    private void Update()
    {
        Transform target = vrCamera;

        if (target == null && Camera.main != null)
            target = Camera.main.transform;

        if (target == null)
            return;

        Vector3 direction =
            target.position - transform.position;

        direction.y = 0f;

        if (direction.sqrMagnitude < 0.0001f)
            return;

        transform.rotation =
            Quaternion.LookRotation(direction);
    }

    // ----------------------------------------------
    // CREATE CANVAS
    // ----------------------------------------------

    private void CreateCanvas()
    {
        GameObject canvasObject = new GameObject(
            CanvasName,
            typeof(RectTransform),
            typeof(Canvas),
            typeof(CanvasScaler),
            typeof(GraphicRaycaster)
        );

        canvasObject.transform.SetParent(transform, false);

        canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;

        RectTransform rect =
            canvasObject.GetComponent<RectTransform>();

        rect.sizeDelta = canvasSize;
        rect.localPosition = Vector3.zero;
        rect.localRotation = Quaternion.identity;
        rect.localScale = Vector3.one * canvasScale;

        CreateBackground(canvasObject);
        CreateGrid(canvasObject);
    }

    // ----------------------------------------------
    // CREATE BACKGROUND
    // ----------------------------------------------

    private void CreateBackground(GameObject canvasObject)
    {
        GameObject backgroundObject = new GameObject(
            BackgroundName,
            typeof(RectTransform),
            typeof(Image)
        );

        backgroundObject.transform.SetParent(
            canvasObject.transform, false
        );

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

    // ----------------------------------------------
    // CREATE GRID
    // ----------------------------------------------

    private void CreateGrid(GameObject canvasObject)
    {
        GameObject gridObject = new GameObject(
            GridName,
            typeof(RectTransform),
            typeof(GridLayoutGroup)
        );

        gridObject.transform.SetParent(
            canvasObject.transform, false
        );

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

    // ----------------------------------------------
    // CREATE CATALOGUE BUTTONS
    // ----------------------------------------------

    private void CreateButtons()
    {
        if (gridRoot == null)
            return;

        foreach (CatalogueEntry entry in entries)
        {
            if (entry == null)
                continue;

            CreateButton(entry);
        }
    }

    // ----------------------------------------------
    // CREATE CATALOGUE BUTTON
    // ----------------------------------------------

    private void CreateButton(CatalogueEntry entry)
    {
        Image thumbnailImage;

        Button button = CreateBaseButton(
            entry.ShortName,
            out thumbnailImage
        );

        string prefabGuid = entry.Guid;

        button.onClick.AddListener(() =>
        {
            SpawnPrefab(prefabGuid);
        });

        int requestVersion = menuVersion;

        // Load thumbnail asynchronously.
        Core.AssetBundlesRef
            .CatalogueThumbnailAssetBundleRef
            .GetItem(Core.Mono, entry.Guid, (texture) =>
            {
                // Ignore callbacks belonging to an old menu.
                if (requestVersion != menuVersion)
                    return;

                if (thumbnailImage == null)
                    return;

                if (texture == null)
                {
                    Debug.LogError(
                        $"Can't find Thumbnail for " +
                        $"{entry.FullName} {entry.Guid}"
                    );

                    return;
                }

                Sprite sprite = CreateRuntimeSprite(texture);

                thumbnailImage.sprite = sprite;
                thumbnailImage.color = Color.white;
                thumbnailImage.preserveAspect = true;
            });
    }

    // ----------------------------------------------
    // CREATE TEXTURE BUTTON
    // ----------------------------------------------

    private void CreateTextureButton(
        Texture2D texture,
        int index,
        Action<int, Texture2D> onButtonClicked)
    {
        Image thumbnailImage;

        Button button = CreateBaseButton(
            texture.name,
            out thumbnailImage
        );

        int selectedIndex = index;
        Texture2D selectedTexture = texture;

        button.onClick.AddListener(() =>
        {
            Debug.Log(
                $"Texture button clicked: " +
                $"{selectedIndex} - {selectedTexture.name}"
            );

            // Callback supplied to SetEntries().
            onButtonClicked?.Invoke(
                selectedIndex,
                selectedTexture
            );

            // Existing event for other subscribers.
            OnTextureSelected?.Invoke(
                selectedIndex,
                selectedTexture
            );
        });

        Sprite sprite = CreateRuntimeSprite(texture);

        thumbnailImage.sprite = sprite;
        thumbnailImage.color = Color.white;
        thumbnailImage.preserveAspect = true;
    }

    // ----------------------------------------------
    // SHARED BUTTON CREATION
    // ----------------------------------------------

    private Button CreateBaseButton(
        string buttonName,
        out Image thumbnailImage)
    {
        GameObject buttonObject = new GameObject(
            buttonName,
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

        // Prevent Unity's button transition
        // from changing the background colour.
        button.transition = Selectable.Transition.None;

        // Thumbnail child.
        GameObject thumbnailObject = new GameObject(
            "Thumbnail",
            typeof(RectTransform),
            typeof(Image)
        );

        thumbnailObject.transform.SetParent(
            buttonObject.transform, false
        );

        RectTransform thumbnailRect =
            thumbnailObject.GetComponent<RectTransform>();

        thumbnailRect.anchorMin = Vector2.zero;
        thumbnailRect.anchorMax = Vector2.one;

        thumbnailRect.offsetMin = thumbnailPadding;
        thumbnailRect.offsetMax = -thumbnailPadding;

        thumbnailRect.localRotation = Quaternion.identity;
        thumbnailRect.localScale = Vector3.one;

        thumbnailImage =
            thumbnailObject.GetComponent<Image>();

        thumbnailImage.color = Color.white;
        thumbnailImage.preserveAspect = true;

        // Allow the parent button to receive clicks.
        thumbnailImage.raycastTarget = false;

        return button;
    }

    // ----------------------------------------------
    // CREATE RUNTIME SPRITE
    // ----------------------------------------------

    private Sprite CreateRuntimeSprite(Texture2D texture)
    {
        if (texture == null)
            return null;

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

        generatedSprites.Add(sprite);

        return sprite;
    }

    // ----------------------------------------------
    // SPAWN PREFAB
    // ----------------------------------------------

    public void SpawnPrefab(string prefabGuid)
    {
        if (string.IsNullOrEmpty(prefabGuid))
            return;

        SetupCamera();

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

    // ----------------------------------------------
    // INSPECTOR TEST BUTTON
    // ----------------------------------------------

    public void TestButtonClick()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning(
                "Test Button Click requires Play Mode.",
                this
            );

            return;
        }

        if (testButtonIndex < 0 ||
            testButtonIndex >= createdButtons.Count)
        {
            Debug.LogWarning(
                $"Invalid test button index {testButtonIndex}. " +
                $"Created buttons: {createdButtons.Count}.",
                this
            );

            return;
        }

        Button button = createdButtons[testButtonIndex];

        if (button == null)
        {
            Debug.LogWarning(
                $"Button {testButtonIndex} no longer exists.",
                this
            );

            return;
        }

        Debug.Log(
            $"Testing button {testButtonIndex}: {button.name}",
            button
        );

        button.onClick.Invoke();
    }

    // ----------------------------------------------
    // BACKGROUND CONTROLS
    // ----------------------------------------------

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

    // ----------------------------------------------
    // BUTTON COLOUR CONTROLS
    // ----------------------------------------------

    public void SetButtonBackgroundColor(Color color)
    {
        buttonBackgroundColor = color;

        foreach (Image image in buttonBackgrounds)
        {
            if (image != null)
                image.color = color;
        }
    }

    public void SetButtonBackgroundColor(
        int index,
        Color color)
    {
        if (index < 0 ||
            index >= buttonBackgrounds.Count)
            return;

        Image image = buttonBackgrounds[index];

        if (image != null)
            image.color = color;
    }
}
