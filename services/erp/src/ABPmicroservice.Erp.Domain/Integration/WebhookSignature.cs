using System;
using System.Security.Cryptography;
using System.Text;

namespace ABPmicroservice.Erp.Integration;

/// <summary>
/// Signs a webhook body so the receiver can tell a genuine notification from anything else that
/// reaches the same URL.
/// <para>
/// The signature is HMAC-SHA256 of the exact bytes sent, keyed with the subscription's secret,
/// and travels in the <c>X-Erp-Signature</c> header as <c>sha256=&lt;hex&gt;</c>. It is the
/// convention most webhook receivers already implement.
/// </para>
/// </summary>
public static class WebhookSignature
{
    public const string HeaderName = "X-Erp-Signature";

    public static string Compute(string secret, string payload)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret ?? string.Empty));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload ?? string.Empty));

        return "sha256=" + Convert.ToHexString(hash).ToLowerInvariant();
    }

    /// <summary>A new random secret, in the URL-safe base64 form people can copy out of the UI.</summary>
    public static string NewSecret()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)).TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }
}
