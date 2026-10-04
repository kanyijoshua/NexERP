using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Finance;
using Volo.Abp;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace ABPmicroservice.Erp.Companies;

/// <summary>
/// A master record other tables point at by its number: a customer, a vendor, an employee, a
/// fixed asset.
/// </summary>
public interface IHasNo
{
    string No { get; }
}

/// <summary>
/// The table relation on a field that points at a master record rather than a
/// code table: the record must exist. Blank always passes, since blank means "none".
/// <see cref="CodeTableChecker"/> does the same for code tables.
/// </summary>
public class TableRelationChecker : DomainService
{
    /// <summary>The record numbered <paramref name="no"/> exists. Numbers compare without regard to case.</summary>
    public async Task EnsureNoExistsAsync<TTable>(string no)
        where TTable : class, IEntity<Guid>, IHasNo
    {
        var normalized = CodeTableEntity.NormalizeCode(no);
        if (normalized == null)
        {
            return;
        }

        var repository = LazyServiceProvider.LazyGetRequiredService<IRepository<TTable, Guid>>();
        if (!await repository.AnyAsync(t => t.No.ToUpper() == normalized))
        {
            throw NotFound(CodeTableChecker.CaptionOf(typeof(TTable)), normalized);
        }
    }

    /// <summary>A record matching <paramref name="predicate"/> exists; for tables keyed by something other than "No." or "Code".</summary>
    public async Task EnsureExistsAsync<TTable>(Expression<Func<TTable, bool>> predicate, string caption, string value)
        where TTable : class, IEntity<Guid>
    {
        if (value.IsNullOrWhiteSpace())
        {
            return;
        }

        var repository = LazyServiceProvider.LazyGetRequiredService<IRepository<TTable, Guid>>();
        if (!await repository.AnyAsync(predicate))
        {
            throw NotFound(caption, value.Trim().ToUpperInvariant());
        }
    }

    /// <summary>Every account named exists in the chart of accounts; blanks are skipped.</summary>
    public Task EnsureGLAccountsExistAsync(params string[] accountNos)
    {
        return LazyServiceProvider.LazyGetRequiredService<PostingSetupManager>().EnsureGLAccountsExistAsync(accountNos);
    }

    private static BusinessException NotFound(string table, string code)
    {
        return new BusinessException(ErpErrorCodes.PostingSetup.CodeNotFound).WithData("table", table).WithData("code", code);
    }
}
