using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Purchasing;
using ABPmicroservice.Erp.Sales;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;

namespace ABPmicroservice.Erp.Finance;

/// <summary>
/// Reads back the entries a register wrote, across the three ledgers.
/// <para>
/// The same reader serves the posting preview and the navigate action on a register, so what a
/// user is shown before posting is exactly what they can look at afterwards.
/// </para>
/// </summary>
public class RegisterEntryReader : ITransientDependency
{
    private readonly IRepository<GLEntry, Guid> _glEntryRepository;
    private readonly IRepository<CustomerLedgerEntry, Guid> _customerLedgerRepository;
    private readonly IRepository<VendorLedgerEntry, Guid> _vendorLedgerRepository;

    public RegisterEntryReader(
        IRepository<GLEntry, Guid> glEntryRepository,
        IRepository<CustomerLedgerEntry, Guid> customerLedgerRepository,
        IRepository<VendorLedgerEntry, Guid> vendorLedgerRepository
    )
    {
        _glEntryRepository = glEntryRepository;
        _customerLedgerRepository = customerLedgerRepository;
        _vendorLedgerRepository = vendorLedgerRepository;
    }

    public async Task<PostingPreviewDto> ReadAsync(long registerNo)
    {
        var glEntries = await _glEntryRepository.GetListAsync(e => e.RegisterNo == registerNo);
        var customerEntries = await _customerLedgerRepository.GetListAsync(e => e.RegisterNo == registerNo);
        var vendorEntries = await _vendorLedgerRepository.GetListAsync(e => e.RegisterNo == registerNo);

        var lines = new List<PostingPreviewLineDto>();

        lines.AddRange(
            glEntries
                .OrderBy(e => e.EntryNo)
                .Select(e => Line("GLEntry", e.PostingDate, e.DocumentNo, e.GLAccountNo, e.Description, e.Amount))
        );

        lines.AddRange(
            customerEntries
                .OrderBy(e => e.EntryNo)
                .Select(e =>
                    Line("CustomerLedgerEntry", e.PostingDate, e.DocumentNo, e.CustomerNo, e.Description, e.Amount)
                )
        );

        lines.AddRange(
            vendorEntries
                .OrderBy(e => e.EntryNo)
                .Select(e => Line("VendorLedgerEntry", e.PostingDate, e.DocumentNo, e.VendorNo, e.Description, e.Amount))
        );

        return new PostingPreviewDto
        {
            Lines = lines,
            // Only the G/L side has to net to zero; the subledgers mirror their control accounts.
            GLBalance = glEntries.Sum(e => e.Amount),
        };
    }

    private static PostingPreviewLineDto Line(
        string ledger,
        DateTime postingDate,
        string documentNo,
        string accountNo,
        string description,
        decimal amount
    )
    {
        return new PostingPreviewLineDto
        {
            Ledger = ledger,
            PostingDate = postingDate,
            DocumentNo = documentNo,
            AccountNo = accountNo,
            Description = description,
            DebitAmount = amount > 0m ? amount : 0m,
            CreditAmount = amount < 0m ? -amount : 0m,
        };
    }
}
