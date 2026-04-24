using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum StudioPanelStyle
{
    Surface,
    Elevated,
    Accent,
    Muted,
    Scrim
}

public enum StudioButtonStyle
{
    Primary,
    Secondary,
    Quiet,
    Warning,
    Danger,
    Qa
}

public static class StudioUiTheme
{
    public static readonly Color Ink = new Color(0.055f, 0.066f, 0.07f, 1f);
    public static readonly Color DeepSurface = new Color(0.09f, 0.115f, 0.105f, 0.96f);
    public static readonly Color Surface = new Color(0.13f, 0.17f, 0.145f, 0.94f);
    public static readonly Color Elevated = new Color(0.18f, 0.215f, 0.18f, 0.96f);
    public static readonly Color MutedSurface = new Color(0.23f, 0.255f, 0.205f, 0.84f);
    public static readonly Color Text = new Color(0.96f, 0.985f, 0.97f, 1f);
    public static readonly Color MutedText = new Color(0.78f, 0.86f, 0.84f, 1f);
    public static readonly Color Gold = new Color(0.94f, 0.72f, 0.28f, 1f);
    public static readonly Color Amber = new Color(0.98f, 0.61f, 0.24f, 1f);
    public static readonly Color Teal = new Color(0.22f, 0.73f, 0.68f, 1f);
    public static readonly Color Moss = new Color(0.41f, 0.66f, 0.35f, 1f);
    public static readonly Color Blue = new Color(0.34f, 0.62f, 0.86f, 1f);
    public static readonly Color Red = new Color(0.78f, 0.26f, 0.22f, 1f);

    private const int RoundedSpriteSize = 64;
    private static readonly System.Collections.Generic.Dictionary<int, Sprite> roundedSprites =
        new System.Collections.Generic.Dictionary<int, Sprite>();

    public static void ApplyPanel(Image image, StudioPanelStyle style = StudioPanelStyle.Surface, float alpha = 1f)
    {
        if (image == null)
            return;

        image.sprite = GetRoundedSprite(18);
        image.type = Image.Type.Sliced;
        image.color = WithAlpha(GetPanelColor(style), GetPanelColor(style).a * alpha);

        Outline outline = EnsureComponent<Outline>(image.gameObject);
        outline.effectColor = new Color(0.92f, 0.78f, 0.44f, style == StudioPanelStyle.Scrim ? 0.03f : 0.13f);
        outline.effectDistance = new Vector2(1.4f, -1.4f);

        Shadow shadow = EnsureComponent<Shadow>(image.gameObject);
        shadow.effectColor = new Color(0f, 0f, 0f, style == StudioPanelStyle.Scrim ? 0.18f : 0.34f);
        shadow.effectDistance = new Vector2(0f, -8f);
        shadow.useGraphicAlpha = true;
    }

    public static void ApplyButton(Button button, StudioButtonStyle style, TMP_Text label = null)
    {
        if (button == null)
            return;

        Color baseColor = GetButtonColor(style);
        Color textColor = style == StudioButtonStyle.Primary || style == StudioButtonStyle.Warning
            ? new Color(0.08f, 0.095f, 0.09f, 1f)
            : Text;

        ApplyButtonChrome(button, baseColor, label, textColor);
    }

    public static void ApplyButtonChrome(Button button, Color baseColor, TMP_Text label = null, Color? textColorOverride = null)
    {
        if (button == null)
            return;

        Image image = button.GetComponent<Image>();
        Color textColor = textColorOverride ?? Text;

        if (image != null)
        {
            image.sprite = GetRoundedSprite(16);
            image.type = Image.Type.Sliced;
            image.color = baseColor;
            button.targetGraphic = image;

            Outline outline = EnsureComponent<Outline>(image.gameObject);
            outline.effectColor = new Color(1f, 0.88f, 0.55f, baseColor == Gold ? 0.28f : 0.12f);
            outline.effectDistance = new Vector2(1f, -1f);

            Shadow shadow = EnsureComponent<Shadow>(image.gameObject);
            shadow.effectColor = new Color(0f, 0f, 0f, 0.26f);
            shadow.effectDistance = new Vector2(0f, -5f);
            shadow.useGraphicAlpha = true;
        }

        ColorBlock colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1.06f, 1.06f, 1.06f, 1f);
        colors.pressedColor = new Color(0.88f, 0.88f, 0.88f, 1f);
        colors.disabledColor = new Color(0.44f, 0.48f, 0.46f, 0.62f);
        colors.colorMultiplier = 1f;
        colors.fadeDuration = 0.08f;
        button.colors = colors;

        if (label != null)
        {
            label.enableAutoSizing = true;
            label.fontSizeMin = Mathf.Max(12f, label.fontSizeMin);
            label.fontSizeMax = Mathf.Max(label.fontSizeMax, 24f);
            label.alignment = TextAlignmentOptions.Center;
            label.color = textColor;
            label.fontStyle = FontStyles.Bold;
            label.lineSpacing = 0f;
            label.margin = new Vector4(8f, 0f, 8f, 0f);
        }
    }

    public static void ApplyInput(TMP_InputField input)
    {
        if (input == null)
            return;

        Image image = input.GetComponent<Image>();

        if (image != null)
        {
            image.sprite = GetRoundedSprite(14);
            image.type = Image.Type.Sliced;
            image.color = new Color(0.06f, 0.08f, 0.075f, 0.96f);

            Outline outline = EnsureComponent<Outline>(image.gameObject);
            outline.effectColor = new Color(0.72f, 0.84f, 0.76f, 0.18f);
            outline.effectDistance = new Vector2(1f, -1f);
        }

        if (input.textComponent != null)
        {
            input.textComponent.color = Text;
            input.textComponent.enableAutoSizing = true;
            input.textComponent.fontSizeMin = 14f;
            input.textComponent.fontSizeMax = 22f;
        }

        if (input.placeholder is TMP_Text placeholder)
        {
            placeholder.color = new Color(0.7f, 0.78f, 0.74f, 0.86f);
            placeholder.enableAutoSizing = true;
            placeholder.fontSizeMin = 13f;
            placeholder.fontSizeMax = 20f;
        }
    }

    public static void ApplyText(TMP_Text text, float minSize, float maxSize, Color color, FontStyles style = FontStyles.Normal)
    {
        if (text == null)
            return;

        text.enableAutoSizing = true;
        text.fontSizeMin = minSize;
        text.fontSizeMax = maxSize;
        text.color = color;
        text.fontStyle = style;
        text.lineSpacing = 0f;
    }

    public static Color WithAlpha(Color color, float alpha)
    {
        color.a = alpha;
        return color;
    }

    public static Color Dim(Color color, float amount)
    {
        return Color.Lerp(color, Ink, amount);
    }

    private static Color GetPanelColor(StudioPanelStyle style)
    {
        switch (style)
        {
            case StudioPanelStyle.Elevated:
                return Elevated;
            case StudioPanelStyle.Accent:
                return new Color(0.2f, 0.24f, 0.18f, 0.96f);
            case StudioPanelStyle.Muted:
                return MutedSurface;
            case StudioPanelStyle.Scrim:
                return new Color(0.015f, 0.02f, 0.018f, 0.88f);
            default:
                return Surface;
        }
    }

    private static Color GetButtonColor(StudioButtonStyle style)
    {
        switch (style)
        {
            case StudioButtonStyle.Primary:
                return Gold;
            case StudioButtonStyle.Warning:
                return Amber;
            case StudioButtonStyle.Danger:
                return new Color(0.52f, 0.2f, 0.18f, 0.98f);
            case StudioButtonStyle.Qa:
                return new Color(0.23f, 0.43f, 0.5f, 0.98f);
            case StudioButtonStyle.Quiet:
                return new Color(0.2f, 0.24f, 0.22f, 0.92f);
            default:
                return new Color(0.22f, 0.33f, 0.29f, 0.97f);
        }
    }

    private static Sprite GetRoundedSprite(int radius)
    {
        radius = Mathf.Clamp(radius, 8, 28);

        if (roundedSprites.TryGetValue(radius, out Sprite cached) && cached != null)
            return cached;

        Texture2D texture = new Texture2D(RoundedSpriteSize, RoundedSpriteSize, TextureFormat.RGBA32, false);
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Bilinear;

        Color[] pixels = new Color[RoundedSpriteSize * RoundedSpriteSize];
        float r = radius;
        float max = RoundedSpriteSize - 1f;

        for (int y = 0; y < RoundedSpriteSize; y++)
        {
            for (int x = 0; x < RoundedSpriteSize; x++)
            {
                float cx = Mathf.Clamp(x, r, max - r);
                float cy = Mathf.Clamp(y, r, max - r);
                float distance = Vector2.Distance(new Vector2(x, y), new Vector2(cx, cy));
                float alpha = distance <= r - 1f ? 1f : 1f - Mathf.SmoothStep(r - 1f, r + 0.8f, distance);
                pixels[(y * RoundedSpriteSize) + x] = new Color(1f, 1f, 1f, Mathf.Clamp01(alpha));
            }
        }

        texture.SetPixels(pixels);
        texture.Apply(false, true);
        texture.hideFlags = HideFlags.HideAndDontSave;

        Sprite sprite = Sprite.Create(
            texture,
            new Rect(0f, 0f, RoundedSpriteSize, RoundedSpriteSize),
            new Vector2(0.5f, 0.5f),
            100f,
            0,
            SpriteMeshType.FullRect,
            new Vector4(radius, radius, radius, radius));

        sprite.hideFlags = HideFlags.HideAndDontSave;
        roundedSprites[radius] = sprite;
        return sprite;
    }

    private static T EnsureComponent<T>(GameObject gameObject) where T : Component
    {
        T component = gameObject.GetComponent<T>();
        return component != null ? component : gameObject.AddComponent<T>();
    }
}
