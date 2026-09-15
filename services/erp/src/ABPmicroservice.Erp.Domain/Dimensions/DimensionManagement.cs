using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Volo.Abp.Domain.Services;

namespace ABPmicroservice.Erp.Dimensions;

/// <summary>
/// Domain service for Dimension Set ID computation & resolution.
/// Mirrors Business Central Codeunit 408 "DimensionManagement".
/// </summary>
public class DimensionManagement : DomainService
{
    public Guid GetDimensionSetId(IDictionary<string, string> dimensionValues)
    {
        if (dimensionValues == null || !dimensionValues.Any())
        {
            return Guid.Empty;
        }

        var sortedPairs = dimensionValues
            .Where(kv => !string.IsNullOrWhiteSpace(kv.Key) && !string.IsNullOrWhiteSpace(kv.Value))
            .OrderBy(kv => kv.Key)
            .Select(kv => $"{kv.Key.Trim().ToUpperInvariant()}:{kv.Value.Trim().ToUpperInvariant()}");

        var rawString = string.Join(";", sortedPairs);
        using var md5 = MD5.Create();
        var hashBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(rawString));
        return new Guid(hashBytes);
    }
}
