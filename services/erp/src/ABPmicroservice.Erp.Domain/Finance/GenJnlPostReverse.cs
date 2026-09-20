using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Purchasing;
using ABPmicroservice.Erp.Sales;
using ABPmicroservice.Erp.Sequences;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace ABPmicroservice.Erp.Finance;

/// <summary>Outcome of reversing a register.</summary>
public class ReversalResult
{
    public long ReversedRegisterNo { get; set; }

    public long ReversalRegisterNo { get; set; }

    public int GLEntryCount { get; set; }

    public int CustomerEntryCount { get; set; }

    public int VendorEntryCount { get; set; }
}

/// <summary>
/// Reverses a posting run. Mirrors Business Central codeunit 17 "Gen. Jnl.-Post Reverse".
/// <para>
/// Nothing is deleted: the reversal writes mirror-image entries on the original posting date and
/// marks both sides reversed, so the audit trail keeps both halves. This is why the ledgers can
/// stay append-only.
/// </para>
/// </summary>
public class GenJnlPostReverse : DomainService
{
    private readonly IRepository<GLRegister, Guid> _registerRepository;
    private readonly IRepository<GLEntry, Guid> _glEntryRepository;
    private readonly IRepository<GLAccount, Guid> _glAccountRepository;
    private readonly IRepository<CustomerLedgerEntry, Guid> _custLedgerEntryRepository;
    private readonly IRepository<VendorLedgerEntry, Guid> _vendorLedgerEntryRepository;
    private readonly IRepository<Customer, Guid> _customerRepository;
    private readonly IRepository<Vendor, Guid> _vendorRepository;
    private readonly IEntryNoGenerator _entryNoGenerator;
    private readonly GLRegisterManager _registerManager;

    public GenJnlPostReverse(
        IRepository<GLRegister, Guid> registerRepository,
        IRepository<GLEntry, Guid> glEntryRepository,
        IRepository<GLAccount, Guid> glAccountRepository,
        IRepository<CustomerLedgerEntry, Guid> custLedgerEntryRepository,
        IRepository<VendorLedgerEntry, Guid> vendorLedgerEntryRepository,
        IRepository<Customer, Guid> customerRepository,
        IRepository<Vendor, Guid> vendorRepository,
        IEntryNoGenerator entryNoGenerator,
        GLRegisterManager registerManager
    )
    {
        _registerRepository = registerRepository;
        _glEntryRepository = glEntryRepository;
        _glAccountRepository = glAccountRepository;
        _custLedgerEntryRepository = custLedgerEntryRepository;
        _vendorLedgerEntryRepository = vendorLedgerEntryRepository;
        _customerRepository = customerRepository;
        _vendorRepository = vendorRepository;
        _entryNoGenerator = entryNoGenerator;
        _registerManager = registerManager;
    }

    public async Task<ReversalResult> ReverseRegisterAsync(long registerNo, string description = null)
    {
        var register = await _registerRepository.FirstOrDefaultAsync(r => r.No == registerNo);
        if (register == null)
        {
            throw new BusinessException(ErpErrorCodes.Registers.RegisterNotFound).WithData("registerNo", registerNo);
        }

        if (register.Reversed)
        {
            throw new BusinessException(ErpErrorCodes.Registers.AlreadyReversed).WithData("registerNo", registerNo);
        }

        if (!register.IsReversible)
        {
            throw new BusinessException(ErpErrorCodes.Registers.NotReversible).WithData("registerNo", registerNo);
        }

        var glEntries = await _glEntryRepository.GetListAsync(e => e.RegisterNo == registerNo);
        var customerEntries = await _custLedgerEntryRepository.GetListAsync(e => e.RegisterNo == registerNo);
        var vendorEntries = await _vendorLedgerEntryRepository.GetListAsync(e => e.RegisterNo == registerNo);

        if (glEntries.Count == 0 && customerEntries.Count == 0 && vendorEntries.Count == 0)
        {
            throw new BusinessException(ErpErrorCodes.Registers.NotReversible).WithData("registerNo", registerNo);
        }

        EnsureNotApplied(customerEntries, vendorEntries);

        var reversalRegister = await _registerManager.OpenAsync(
            register.PostingDate,
            register.SourceCode,
            register.JournalBatchName
        );

        reversalRegister.ReversedRegisterNo = register.No;

        var text = description.IsNullOrWhiteSpace() ? $"Reversal of register {register.No}" : description;

        var result = new ReversalResult
        {
            ReversedRegisterNo = register.No,
            ReversalRegisterNo = reversalRegister.No,
            GLEntryCount = glEntries.Count,
            CustomerEntryCount = customerEntries.Count,
            VendorEntryCount = vendorEntries.Count,
        };

        foreach (var entry in glEntries.OrderBy(e => e.EntryNo))
        {
            await ReverseGLEntryAsync(entry, reversalRegister, text);
        }

        foreach (var entry in customerEntries.OrderBy(e => e.EntryNo))
        {
            await ReverseCustomerEntryAsync(entry, reversalRegister, text);
        }

        foreach (var entry in vendorEntries.OrderBy(e => e.EntryNo))
        {
            await ReverseVendorEntryAsync(entry, reversalRegister, text);
        }

        register.Reversed = true;
        register.ReversedByRegisterNo = reversalRegister.No;
        await _registerRepository.UpdateAsync(register);
        await _registerManager.CloseAsync(reversalRegister);

        return result;
    }

    /// <summary>
    /// An entry that has been settled against another one cannot be reversed: undoing it would
    /// leave the application dangling. Business Central refuses the same case.
    /// </summary>
    private static void EnsureNotApplied(
        IEnumerable<CustomerLedgerEntry> customerEntries,
        IEnumerable<VendorLedgerEntry> vendorEntries
    )
    {
        var appliedCustomer = customerEntries.FirstOrDefault(e => e.RemainingAmount != e.Amount);
        if (appliedCustomer != null)
        {
            throw new BusinessException(ErpErrorCodes.Registers.AppliedEntryCannotBeReversed)
                .WithData("entryNo", appliedCustomer.EntryNo);
        }

        var appliedVendor = vendorEntries.FirstOrDefault(e => e.RemainingAmount != e.Amount);
        if (appliedVendor != null)
        {
            throw new BusinessException(ErpErrorCodes.Registers.AppliedEntryCannotBeReversed)
                .WithData("entryNo", appliedVendor.EntryNo);
        }
    }

    private async Task ReverseGLEntryAsync(GLEntry entry, GLRegister reversalRegister, string description)
    {
        if (entry.Reversed)
        {
            throw new BusinessException(ErpErrorCodes.Registers.EntryAlreadyReversed).WithData("entryNo", entry.EntryNo);
        }

        var counter = new GLEntry(
            GuidGenerator.Create(),
            entry.GLAccountId,
            entry.GLAccountNo,
            entry.PostingDate,
            entry.DocumentType,
            entry.DocumentNo,
            description,
            -entry.Amount,
            entry.SourceNo,
            documentDate: entry.DocumentDate
        )
        {
            EntryNo = await _entryNoGenerator.NextAsync(ErpSequenceNames.GLEntry),
            TransactionNo = reversalRegister.TransactionNo,
            RegisterNo = reversalRegister.No,
            SourceCode = entry.SourceCode,
            ReasonCode = entry.ReasonCode,
            ReversedEntryNo = entry.EntryNo,
            Reversed = true,
        };

        reversalRegister.NoteGLEntry(counter.EntryNo);
        await _glEntryRepository.InsertAsync(counter);

        entry.Reversed = true;
        entry.ReversedByEntryNo = counter.EntryNo;
        await _glEntryRepository.UpdateAsync(entry);

        var account = await _glAccountRepository.FindAsync(entry.GLAccountId);
        if (account != null)
        {
            account.ApplyEntry(-entry.Amount);
            await _glAccountRepository.UpdateAsync(account);
        }
    }

    private async Task ReverseCustomerEntryAsync(
        CustomerLedgerEntry entry,
        GLRegister reversalRegister,
        string description
    )
    {
        if (entry.Reversed)
        {
            throw new BusinessException(ErpErrorCodes.Registers.EntryAlreadyReversed).WithData("entryNo", entry.EntryNo);
        }

        var counter = new CustomerLedgerEntry(
            GuidGenerator.Create(),
            entry.CustomerId,
            entry.CustomerNo,
            entry.PostingDate,
            entry.DocumentType,
            entry.DocumentNo,
            description,
            -entry.Amount,
            entry.DueDate,
            entry.DimensionSetId
        )
        {
            EntryNo = await _entryNoGenerator.NextAsync(ErpSequenceNames.CustomerLedgerEntry),
            TransactionNo = reversalRegister.TransactionNo,
            RegisterNo = reversalRegister.No,
            ReversedEntryNo = entry.EntryNo,
            Reversed = true,
        };

        // The two entries settle each other, so neither is left open for application.
        counter.RemainingAmount = 0m;
        counter.Open = false;

        reversalRegister.NoteCustomerEntry(counter.EntryNo);
        await _custLedgerEntryRepository.InsertAsync(counter);

        entry.Reversed = true;
        entry.ReversedByEntryNo = counter.EntryNo;
        entry.RemainingAmount = 0m;
        entry.Open = false;
        await _custLedgerEntryRepository.UpdateAsync(entry);

        var customer = await _customerRepository.FindAsync(entry.CustomerId);
        if (customer != null)
        {
            customer.ApplyBalance(-entry.Amount);
            await _customerRepository.UpdateAsync(customer);
        }
    }

    private async Task ReverseVendorEntryAsync(
        VendorLedgerEntry entry,
        GLRegister reversalRegister,
        string description
    )
    {
        if (entry.Reversed)
        {
            throw new BusinessException(ErpErrorCodes.Registers.EntryAlreadyReversed).WithData("entryNo", entry.EntryNo);
        }

        var counter = new VendorLedgerEntry(
            GuidGenerator.Create(),
            entry.VendorId,
            entry.VendorNo,
            entry.PostingDate,
            entry.DocumentType,
            entry.DocumentNo,
            description,
            -entry.Amount,
            entry.DueDate,
            entry.DimensionSetId
        )
        {
            EntryNo = await _entryNoGenerator.NextAsync(ErpSequenceNames.VendorLedgerEntry),
            TransactionNo = reversalRegister.TransactionNo,
            RegisterNo = reversalRegister.No,
            ReversedEntryNo = entry.EntryNo,
            Reversed = true,
        };

        counter.RemainingAmount = 0m;
        counter.Open = false;

        reversalRegister.NoteVendorEntry(counter.EntryNo);
        await _vendorLedgerEntryRepository.InsertAsync(counter);

        entry.Reversed = true;
        entry.ReversedByEntryNo = counter.EntryNo;
        entry.RemainingAmount = 0m;
        entry.Open = false;
        await _vendorLedgerEntryRepository.UpdateAsync(entry);

        var vendor = await _vendorRepository.FindAsync(entry.VendorId);
        if (vendor != null)
        {
            vendor.ApplyBalance(-entry.Amount);
            await _vendorRepository.UpdateAsync(vendor);
        }
    }
}
