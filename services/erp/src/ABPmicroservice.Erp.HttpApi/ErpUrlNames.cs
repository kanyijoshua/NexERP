using System.Linq;
using System.Text.RegularExpressions;

namespace ABPmicroservice.Erp;

/// <summary>
/// Controller names for conventional routing. ABP kebab-cases the controller name for the URL,
/// but it splits only before capitals, so "GLAccount" becomes "g-lAccount". Normalising acronyms
/// first ("GLAccount" to "GlAccount", "VATPostingSetup" to "VatPostingSetup") yields "gl-account".
/// The result must stay a PascalCase identifier: it is also the controller name in the API
/// definition, from which client proxies take their class names.
/// </summary>
public static partial class ErpUrlNames
{
    [GeneratedRegex("(?<=[a-z0-9])(?=[A-Z])|(?<=[A-Z])(?=[A-Z][a-z])")]
    private static partial Regex WordBoundary();

    public static string NormalizeAcronyms(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return name;
        }

        return string.Concat(
            WordBoundary().Split(name).Select(word => char.ToUpperInvariant(word[0]) + word[1..].ToLowerInvariant())
        );
    }
}
