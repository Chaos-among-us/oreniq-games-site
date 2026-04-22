using UnityEngine;

public readonly struct RuntimeCaveTheme
{
    public readonly int Level;
    public readonly int BiomeIndex;
    public readonly string Name;
    public readonly Color BackgroundTop;
    public readonly Color BackgroundBottom;
    public readonly Color WallColor;
    public readonly Color FogColor;
    public readonly Color CrystalColor;
    public readonly Color AccentColor;
    public readonly float RootFrequency;
    public readonly float AccentFrequency;

    public RuntimeCaveTheme(
        int level,
        int biomeIndex,
        string name,
        Color backgroundTop,
        Color backgroundBottom,
        Color wallColor,
        Color fogColor,
        Color crystalColor,
        Color accentColor,
        float rootFrequency,
        float accentFrequency)
    {
        Level = Mathf.Max(1, level);
        BiomeIndex = Mathf.Max(0, biomeIndex);
        Name = name;
        BackgroundTop = backgroundTop;
        BackgroundBottom = backgroundBottom;
        WallColor = wallColor;
        FogColor = fogColor;
        CrystalColor = crystalColor;
        AccentColor = accentColor;
        RootFrequency = rootFrequency;
        AccentFrequency = accentFrequency;
    }
}

public static class CaveThemeLibrary
{
    public const int LevelsPerBiome = 3;

    private static readonly string[] ThemeNames =
    {
        "Moss Cavern",
        "Crystal Hollow",
        "Ember Fault",
        "Midnight Grotto"
    };

    public static RuntimeCaveTheme GetThemeForLevel(int level)
    {
        level = Mathf.Max(1, level);
        int biomeIndex = GetBiomeIndexForLevel(level);
        int biomeStage = (level - 1) % LevelsPerBiome;
        float levelShift = biomeStage / 2f;
        float moodShift = Mathf.Repeat((biomeIndex * 0.17f) + (biomeStage * 0.11f), 1f);

        float topHue;
        float bottomHue;
        float crystalHue;
        float accentHue;
        float saturation;
        float brightness;
        float darkness;

        switch (biomeIndex)
        {
            case 0:
                topHue = 0.54f + (levelShift * 0.08f);
                bottomHue = 0.39f + (levelShift * 0.05f);
                crystalHue = 0.31f + (moodShift * 0.03f);
                accentHue = 0.15f + (moodShift * 0.04f);
                saturation = 0.28f;
                brightness = 0.3f;
                darkness = 0.1f;
                break;
            case 1:
                topHue = 0.62f + (levelShift * 0.06f);
                bottomHue = 0.77f + (levelShift * 0.04f);
                crystalHue = 0.51f + (moodShift * 0.08f);
                accentHue = 0.83f + (moodShift * 0.05f);
                saturation = 0.24f;
                brightness = 0.31f;
                darkness = 0.11f;
                break;
            case 2:
                topHue = 0.03f + (levelShift * 0.04f);
                bottomHue = 0.1f + (levelShift * 0.05f);
                crystalHue = 0.08f + (moodShift * 0.03f);
                accentHue = 0.15f + (moodShift * 0.02f);
                saturation = 0.32f;
                brightness = 0.33f;
                darkness = 0.12f;
                break;
            default:
                topHue = 0.67f + (levelShift * 0.05f);
                bottomHue = 0.58f + (levelShift * 0.04f);
                crystalHue = 0.74f + (moodShift * 0.05f);
                accentHue = 0.54f + (moodShift * 0.03f);
                saturation = 0.21f;
                brightness = 0.28f;
                darkness = 0.09f;
                break;
        }

        Color backgroundTop = Color.HSVToRGB(Mathf.Repeat(topHue, 1f), saturation * 0.74f, brightness + 0.12f);
        Color backgroundBottom = Color.HSVToRGB(Mathf.Repeat(bottomHue, 1f), saturation * 0.8f, brightness * 0.96f);
        Color wallColor = Color.HSVToRGB(Mathf.Repeat(bottomHue + 0.02f, 1f), saturation * 0.42f, darkness + 0.16f);
        Color fogColor = Color.HSVToRGB(Mathf.Repeat(topHue + 0.01f, 1f), saturation * 0.18f, brightness + 0.2f);
        Color crystalColor = Color.HSVToRGB(Mathf.Repeat(crystalHue, 1f), 0.32f, 0.76f);
        Color accentColor = Color.HSVToRGB(Mathf.Repeat(accentHue, 1f), 0.26f, 0.58f);

        float rootFrequency = GetLoopSafeFrequency(78f + (biomeIndex * 16f) + (level % LevelsPerBiome) * 5f, 12f);
        float accentFrequency = GetLoopSafeFrequency(rootFrequency * 1.5f, 12f);

        return new RuntimeCaveTheme(
            level,
            biomeIndex,
            ThemeNames[biomeIndex] + " L" + level,
            backgroundTop,
            backgroundBottom,
            wallColor,
            fogColor,
            crystalColor,
            accentColor,
            rootFrequency,
            accentFrequency);
    }

    public static RuntimeCaveTheme GetMenuTheme()
    {
        return GetThemeForLevel(1);
    }

    public static int GetBiomeIndexForLevel(int level)
    {
        level = Mathf.Max(1, level);
        return ((level - 1) / LevelsPerBiome) % ThemeNames.Length;
    }

    public static int GetBiomeStartLevel(int level)
    {
        level = Mathf.Max(1, level);
        return (((level - 1) / LevelsPerBiome) * LevelsPerBiome) + 1;
    }

    private static float GetLoopSafeFrequency(float approximateFrequency, float loopLengthSeconds)
    {
        int cycles = Mathf.Max(1, Mathf.RoundToInt(approximateFrequency * loopLengthSeconds));
        return cycles / loopLengthSeconds;
    }
}
