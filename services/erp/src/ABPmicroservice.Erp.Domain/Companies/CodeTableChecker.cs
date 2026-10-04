using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace ABPmicroservice.Erp.Companies;

/// <summary>
/// The table relation on a code field: a code a record points at must exist in its
/// code table. Blank always passes, since blank means "none".
/// </summary>
public class CodeTableChecker : DomainService
{
    /// <summary>The captions that are not simply the class name split into words.</summary>
    private static readonly Dictionary<string, string> Captions = new()
    {
        ["GenBusinessPostingGroup"] = "Gen. Bus. Posting Group",
        ["GenProductPostingGroup"] = "Gen. Prod. Posting Group",
        ["VatBusinessPostingGroup"] = "VAT Bus. Posting Group",
        ["VatProductPostingGroup"] = "VAT Prod. Posting Group",
        ["SalespersonPurchaser"] = "Salesperson/Purchaser",
        ["HumanResourceUnitOfMeasure"] = "Human Resource Unit of Measure",
    };

    public async Task EnsureExistsAsync<TTable>(string code)
        where TTable : CodeTableEntity
    {
        var normalized = CodeTableEntity.NormalizeCode(code);
        if (normalized == null)
        {
            return;
        }

        var repository = LazyServiceProvider.LazyGetRequiredService<IRepository<TTable, Guid>>();
        if (!await repository.AnyAsync(t => t.Code == normalized))
        {
            throw new BusinessException(ErpErrorCodes.PostingSetup.CodeNotFound)
                .WithData("table", CaptionOf(typeof(TTable)))
                .WithData("code", normalized);
        }
    }

    public static string CaptionOf(Type table)
    {
        return Captions.TryGetValue(table.Name, out var caption)
            ? caption
            : Regex.Replace(table.Name, "(?<=[a-z])(?=[A-Z])", " ");
    }
}
