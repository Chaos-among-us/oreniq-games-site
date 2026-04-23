using UnityEngine;

public class PlayfieldBorderController : MonoBehaviour
{
    private const string BordersRootName = "Borders";
    private const string LeftBorderName = "LeftBorder";
    private const string RightBorderName = "RightBorder";
    private const string LeftBorderGlowName = "LeftBorderGlow";
    private const string RightBorderGlowName = "RightBorderGlow";
    private const float HeightOverscan = 0.24f;
    private const float GlowHeightMultiplier = 1.02f;

    private GameManager gameManager;
    private Camera targetCamera;
    private Transform leftBorderTransform;
    private Transform rightBorderTransform;
    private Transform leftGlowTransform;
    private Transform rightGlowTransform;
    private SpriteRenderer leftBorderRenderer;
    private SpriteRenderer rightBorderRenderer;
    private SpriteRenderer leftGlowRenderer;
    private SpriteRenderer rightGlowRenderer;
    private float leftBorderWidth = 0.15f;
    private float rightBorderWidth = 0.15f;
    private float leftGlowWidth = 0.35f;
    private float rightGlowWidth = 0.35f;
    private int cachedScreenWidth;
    private int cachedScreenHeight;
    private float cachedOrthographicSize;
    private int appliedBaseLevel = -1;
    private float appliedBlend = -1f;
    private bool widthsCached;

    void Awake()
    {
        EnsureReferences();
        CacheCurrentWidths();
        RefreshLayout(forceRefresh: true);
        RefreshColors(forceRefresh: true);
    }

    void LateUpdate()
    {
        EnsureReferences();

        if (targetCamera == null)
            return;

        bool layoutChanged = DidViewportChange();

        if (layoutChanged)
            RefreshLayout(forceRefresh: true);

        RefreshColors(forceRefresh: layoutChanged);
    }

    private void EnsureReferences()
    {
        if (gameManager == null)
            gameManager = GameManager.instance;

        if (targetCamera == null)
            targetCamera = Camera.main;

        Transform borderRoot = transform.name == BordersRootName ? transform : FindNamedTransform(BordersRootName);

        if (leftBorderTransform == null && borderRoot != null)
            leftBorderTransform = FindChildOrRoot(borderRoot, LeftBorderName);

        if (rightBorderTransform == null && borderRoot != null)
            rightBorderTransform = FindChildOrRoot(borderRoot, RightBorderName);

        if (leftGlowTransform == null && borderRoot != null)
            leftGlowTransform = FindChildOrRoot(borderRoot, LeftBorderGlowName);

        if (rightGlowTransform == null && borderRoot != null)
            rightGlowTransform = FindChildOrRoot(borderRoot, RightBorderGlowName);

        if (leftBorderRenderer == null && leftBorderTransform != null)
            leftBorderRenderer = leftBorderTransform.GetComponent<SpriteRenderer>();

        if (rightBorderRenderer == null && rightBorderTransform != null)
            rightBorderRenderer = rightBorderTransform.GetComponent<SpriteRenderer>();

        if (leftGlowRenderer == null && leftGlowTransform != null)
            leftGlowRenderer = leftGlowTransform.GetComponent<SpriteRenderer>();

        if (rightGlowRenderer == null && rightGlowTransform != null)
            rightGlowRenderer = rightGlowTransform.GetComponent<SpriteRenderer>();
    }

    private Transform FindChildOrRoot(Transform root, string objectName)
    {
        if (root == null)
            return null;

        Transform child = root.Find(objectName);

        if (child != null)
            return child;

        return FindNamedTransform(objectName);
    }

    private Transform FindNamedTransform(string objectName)
    {
        GameObject target = GameObject.Find(objectName);
        return target != null ? target.transform : null;
    }

    private void CacheCurrentWidths()
    {
        if (widthsCached)
            return;

        leftBorderWidth = GetWorldWidth(leftBorderRenderer, leftBorderWidth);
        rightBorderWidth = GetWorldWidth(rightBorderRenderer, rightBorderWidth);
        leftGlowWidth = GetWorldWidth(leftGlowRenderer, leftGlowWidth);
        rightGlowWidth = GetWorldWidth(rightGlowRenderer, rightGlowWidth);
        widthsCached = true;
    }

    private float GetWorldWidth(SpriteRenderer renderer, float fallback)
    {
        if (renderer == null || renderer.sprite == null)
            return fallback;

        return Mathf.Max(0.05f, renderer.sprite.bounds.size.x * renderer.transform.lossyScale.x);
    }

    private bool DidViewportChange()
    {
        if (targetCamera == null || !targetCamera.orthographic)
            return false;

        bool changed = cachedScreenWidth != Screen.width ||
                       cachedScreenHeight != Screen.height ||
                       !Mathf.Approximately(cachedOrthographicSize, targetCamera.orthographicSize);

        if (changed)
        {
            cachedScreenWidth = Screen.width;
            cachedScreenHeight = Screen.height;
            cachedOrthographicSize = targetCamera.orthographicSize;
        }

        return changed;
    }

    private void RefreshLayout(bool forceRefresh)
    {
        if (!forceRefresh || targetCamera == null || !targetCamera.orthographic)
            return;

        float referenceZ = leftBorderTransform != null ? leftBorderTransform.position.z : 0f;
        float cameraDistance = Mathf.Abs(targetCamera.transform.position.z - referenceZ);
        Vector3 bottom = targetCamera.ViewportToWorldPoint(new Vector3(0.5f, 0f, cameraDistance));
        Vector3 top = targetCamera.ViewportToWorldPoint(new Vector3(0.5f, 1f, cameraDistance));
        float targetHeight = (top.y - bottom.y) + HeightOverscan;
        float centerY = (top.y + bottom.y) * 0.5f;

        ApplyElementLayout(leftBorderTransform, leftBorderRenderer, leftBorderWidth, targetHeight, centerY);
        ApplyElementLayout(rightBorderTransform, rightBorderRenderer, rightBorderWidth, targetHeight, centerY);
        ApplyElementLayout(leftGlowTransform, leftGlowRenderer, leftGlowWidth, targetHeight * GlowHeightMultiplier, centerY);
        ApplyElementLayout(rightGlowTransform, rightGlowRenderer, rightGlowWidth, targetHeight * GlowHeightMultiplier, centerY);
    }

    private void ApplyElementLayout(Transform target, SpriteRenderer renderer, float width, float height, float centerY)
    {
        if (target == null || renderer == null || renderer.sprite == null)
            return;

        Vector3 position = target.position;
        position.y = centerY;
        target.position = position;

        Vector2 spriteSize = renderer.sprite.bounds.size;

        if (spriteSize.x <= 0.0001f || spriteSize.y <= 0.0001f)
            return;

        target.localScale = new Vector3(width / spriteSize.x, height / spriteSize.y, target.localScale.z);
    }

    private void RefreshColors(bool forceRefresh)
    {
        int baseLevel = GetBaseThemeLevel();
        float blend = GetBiomeTransitionBlend01();

        if (!forceRefresh &&
            baseLevel == appliedBaseLevel &&
            Mathf.Abs(blend - appliedBlend) < 0.01f)
        {
            return;
        }

        RuntimeCaveTheme currentTheme = CaveThemeLibrary.GetThemeForLevel(baseLevel);
        RuntimeCaveTheme nextTheme = CaveThemeLibrary.GetThemeForLevel(baseLevel + CaveThemeLibrary.LevelsPerBiome);

        Color currentBorderColor = Color.Lerp(currentTheme.WallColor, currentTheme.AccentColor, 0.18f);
        currentBorderColor.a = 0.96f;

        Color nextBorderColor = Color.Lerp(nextTheme.WallColor, nextTheme.AccentColor, 0.18f);
        nextBorderColor.a = 0.96f;

        Color currentGlowColor = Color.Lerp(currentTheme.CrystalColor, currentTheme.FogColor, 0.22f);
        currentGlowColor.a = 0.26f;

        Color nextGlowColor = Color.Lerp(nextTheme.CrystalColor, nextTheme.FogColor, 0.22f);
        nextGlowColor.a = 0.26f;

        SetRendererColor(leftBorderRenderer, Color.Lerp(currentBorderColor, nextBorderColor, blend));
        SetRendererColor(rightBorderRenderer, Color.Lerp(currentBorderColor, nextBorderColor, blend));
        SetRendererColor(leftGlowRenderer, Color.Lerp(currentGlowColor, nextGlowColor, blend));
        SetRendererColor(rightGlowRenderer, Color.Lerp(currentGlowColor, nextGlowColor, blend));

        appliedBaseLevel = baseLevel;
        appliedBlend = blend;
    }

    private void SetRendererColor(SpriteRenderer renderer, Color color)
    {
        if (renderer != null)
            renderer.color = color;
    }

    private int GetBaseThemeLevel()
    {
        if (gameManager == null)
            return 1;

        return CaveThemeLibrary.GetBiomeStartLevel(Mathf.Max(1, gameManager.GetDifficultyLevel()));
    }

    private float GetBiomeTransitionBlend01()
    {
        if (gameManager == null)
            return 0f;

        float continuousLevel = Mathf.Max(1f, gameManager.GetContinuousDifficultyLevel());
        float biomeProgress = Mathf.Repeat(continuousLevel - 1f, CaveThemeLibrary.LevelsPerBiome);
        return Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(1.8f, 2.95f, biomeProgress));
    }
}
