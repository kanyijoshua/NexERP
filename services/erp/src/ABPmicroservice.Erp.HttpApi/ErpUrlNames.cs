using System.Text.RegularExpressions;

namespace ABPmicroservice.Erp;

/// <summary>
/// Acronym-aware kebab-casing for conventional controller routes.
/// ABP's default turns "GLAccount" into "g-lAccount"; this yields "gl-account"
/// (and "VATPostingSetup" into "vat-posting-setup").
/// </summary>
public static partial class ErpUrlNames
{
    [GeneratedRegex("(?<=[a-z0-9])(?=[A-Z])|(?<=[A-Z])(?=[A-Z][a-z])")]
    private static partial Regex WordBoundary();

    public static string Kebab(string name)
    {
        return string.IsNullOrEmpty(name)
            ? name
            : WordBoundary().Replace(name, "-").ToLowerInvariant();
    }
}
