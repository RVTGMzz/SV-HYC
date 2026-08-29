using StardewModdingAPI;

namespace HeyYoureCursed;

internal sealed partial class ModEntry
{
    private static ITranslationHelper I18n = null!;

    internal static string T(string key)
    {
        if (I18n is null)
            return key;

        return I18n.Get(key).ToString();
    }

    internal static string T(string key, object tokens)
    {
        if (I18n is null)
            return key;

        return I18n.Get(key, tokens).ToString();
    }
}
