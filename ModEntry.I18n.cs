using StardewModdingAPI;

namespace HeyYoureCursed;

internal sealed partial class ModEntry
{
    private static ITranslationHelper I18n = null!;

    internal static string T(string key)
    {
        if (I18n is null)
            return Alpha3TranslationFallback.Get(key, english: false);

        string translated = I18n.Get(key).ToString();
        bool missing = string.Equals(translated, key, StringComparison.OrdinalIgnoreCase)
            || translated.StartsWith("(no translation:", StringComparison.OrdinalIgnoreCase);
        if (!missing)
            return translated;

        string localeProbe = I18n.Get("hub.option.play").ToString();
        bool english = string.Equals(localeProbe, "Play Sudoku", StringComparison.OrdinalIgnoreCase);
        return Alpha3TranslationFallback.Get(key, english);
    }

    internal static string T(string key, object tokens)
    {
        if (I18n is null)
            return FormatFallback(Alpha3TranslationFallback.Get(key, english: false), tokens);

        string translated = I18n.Get(key, tokens).ToString();
        bool missing = string.Equals(translated, key, StringComparison.OrdinalIgnoreCase)
            || translated.StartsWith("(no translation:", StringComparison.OrdinalIgnoreCase);
        if (!missing)
            return translated;

        string localeProbe = I18n.Get("hub.option.play").ToString();
        bool english = string.Equals(localeProbe, "Play Sudoku", StringComparison.OrdinalIgnoreCase);
        return FormatFallback(Alpha3TranslationFallback.Get(key, english), tokens);
    }

    private static string FormatFallback(string template, object tokens)
    {
        string result = template;
        foreach (System.Reflection.PropertyInfo property in tokens.GetType().GetProperties())
        {
            string value = property.GetValue(tokens)?.ToString() ?? string.Empty;
            result = result.Replace("{{" + property.Name + "}}", value, StringComparison.OrdinalIgnoreCase);
        }

        return result;
    }
}
