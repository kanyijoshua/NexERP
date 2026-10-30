using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using ABPmicroservice.Erp.Companies;
using ABPmicroservice.Erp.Exporting;
using ABPmicroservice.Erp.Finance;
using ABPmicroservice.Erp.Numbering;
using ABPmicroservice.Erp.Purchasing;
using ABPmicroservice.Erp.Sales;
using Volo.Abp;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace ABPmicroservice.Erp.CashManagement;

/// <summary>A file made for the bank, as text.</summary>
public sealed class BankFile
{
    public string FileName { get; init; }
    public string ContentType { get; init; }
    public string Content { get; init; }
    public int TransactionCount { get; init; }
    public decimal TotalAmount { get; init; }
}

/// <summary>What the vendor payment suggestion looks for.</summary>
public sealed class VendorPaymentSuggestion
{
    public DateTime PostingDate { get; init; }

    /// <summary>Invoices due on or before this date are paid.</summary>
    public DateTime LastDueDate { get; init; }

    /// <summary>Also pay invoices not due yet while their payment discount can still be taken, less the discount.</summary>
    public bool UsePaymentDiscounts { get; init; }

    /// <summary>The vendors to pay; empty pays every vendor.</summary>
    public IReadOnlyCollection<string> VendorNos { get; init; } = [];

    public string BankAccountNo { get; init; }

    /// <summary>The first document no.; each vendor gets the next one.</summary>
    public string DocumentNo { get; init; }
}

/// <summary>
/// Vendor payments through a general journal: suggests a payment line for every open invoice that
/// is due, applied to it and paid from a bank account, and exports the lines to a file the bank
/// can process, as delimited text or an ISO 20022 credit transfer.
/// </summary>
public class PaymentJournalManager : DomainService
{
    private readonly IRepository<GenJournalBatch, Guid> _batchRepository;
    private readonly IRepository<GenJournalLine, Guid> _lineRepository;
    private readonly IRepository<VendorLedgerEntry, Guid> _vendorEntryRepository;
    private readonly IRepository<Vendor, Guid> _vendorRepository;
    private readonly IRepository<VendorBankAccount, Guid> _vendorBankRepository;
    private readonly IRepository<BankAccount, Guid> _bankRepository;

    private CurrencyExchangeRateManager CurrencyManager => LazyServiceProvider.LazyGetRequiredService<CurrencyExchangeRateManager>();
    private ICurrentCompany CurrentCompany => LazyServiceProvider.LazyGetRequiredService<ICurrentCompany>();

    public PaymentJournalManager(
        IRepository<GenJournalBatch, Guid> batchRepository,
        IRepository<GenJournalLine, Guid> lineRepository,
        IRepository<VendorLedgerEntry, Guid> vendorEntryRepository,
        IRepository<Vendor, Guid> vendorRepository,
        IRepository<VendorBankAccount, Guid> vendorBankRepository,
        IRepository<BankAccount, Guid> bankRepository
    )
    {
        _batchRepository = batchRepository;
        _lineRepository = lineRepository;
        _vendorEntryRepository = vendorEntryRepository;
        _vendorRepository = vendorRepository;
        _vendorBankRepository = vendorBankRepository;
        _bankRepository = bankRepository;
    }

    /// <summary>
    /// Adds a payment line per open invoice that is due (or still in its discount period), applied
    /// to it. Invoices on hold, and invoices the batch already pays, are left out. Returns the lines added.
    /// </summary>
    public async Task<int> SuggestVendorPaymentsAsync(Guid batchId, VendorPaymentSuggestion request)
    {
        var batch = await _batchRepository.GetAsync(batchId);
        var bankNo = request.BankAccountNo?.Trim();
        var bank = await _bankRepository.FirstOrDefaultAsync(b => b.No == bankNo)
            ?? throw new BusinessException(ErpErrorCodes.CashManagement.VoucherAccountNotFound).WithData("accountNo", bankNo ?? "");

        var vendorNos = request.VendorNos.Select(v => v.Trim()).Where(v => v.Length > 0).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var entries = (await _vendorEntryRepository.GetListAsync(e => e.Open && e.RemainingAmount < 0m && e.DocumentType == "Invoice"))
            .Where(e => vendorNos.Count == 0 || vendorNos.Contains(e.VendorNo))
            .Where(e => e.OnHold.IsNullOrWhiteSpace())
            .Where(e => e.DueDate.Date <= request.LastDueDate.Date || DiscountApplies(e, request))
            .OrderBy(e => e.VendorNo)
            .ThenBy(e => e.DueDate)
            .ThenBy(e => e.EntryNo)
            .ToList();

        var existing = await _lineRepository.GetListAsync(l => l.GenJournalBatchId == batchId);
        var paid = existing.Where(l => l.AccountType == GenJournalAccountType.Vendor && l.AppliesToDocNo != null).Select(l => (l.AccountNo, l.AppliesToDocNo)).ToHashSet();
        var lineNo = existing.Count == 0 ? 0 : existing.Max(l => l.LineNo);
        var documentNo = request.DocumentNo.IsNullOrWhiteSpace() ? "PAYMENT-0001" : request.DocumentNo.Trim().ToUpperInvariant();
        string lastVendor = null;
        var added = 0;

        foreach (var entry in entries.Where(e => !paid.Contains((e.VendorNo, e.DocumentNo))))
        {
            var vendor = await _vendorRepository.GetAsync(entry.VendorId);
            if (lastVendor != null && lastVendor != vendor.No)
            {
                documentNo = NoSeriesIncrement.Increment(documentNo);
            }

            lastVendor = vendor.No;
            var discount = DiscountApplies(entry, request) ? entry.RemainingPmtDiscPossible : 0m;
            var amount = -(entry.RemainingAmount - discount);
            lineNo += 10000;
            var line = new GenJournalLine(
                GuidGenerator.Create(),
                batch.Id,
                lineNo,
                request.PostingDate,
                GLEntryDocumentType.Payment,
                documentNo,
                GenJournalAccountType.Vendor,
                vendor.No,
                ($"{vendor.Name} {entry.ExternalDocumentNo ?? entry.DocumentNo}").Truncate(ErpDomainConsts.MaxDescriptionLength),
                amount,
                GenJournalAccountType.BankAccount,
                bank.No
            );
            line.Update(
                request.PostingDate,
                GLEntryDocumentType.Payment,
                documentNo,
                GenJournalAccountType.Vendor,
                vendor.No,
                line.Description,
                amount,
                GenJournalAccountType.BankAccount,
                bank.No,
                externalDocumentNo: entry.ExternalDocumentNo,
                appliesToDocNo: entry.DocumentNo
            );
            var currency = await CurrencyManager.NormalizeAsync(entry.CurrencyCode);
            if (currency != null)
            {
                line.SetCurrency(currency, await CurrencyManager.GetCurrencyFactorAsync(currency, request.PostingDate));
            }

            line.SetAdditionalFields(
                null,
                null,
                null,
                null,
                null,
                null,
                vendor.PaymentMethodCode,
                null,
                BankPaymentType.ElectronicPayment,
                false,
                0m,
                entry.DueDate,
                null,
                null,
                null,
                (entry.ExternalDocumentNo ?? entry.DocumentNo).Truncate(50),
                (entry.ExternalDocumentNo ?? entry.DocumentNo).Truncate(140)
            );
            line.SetRecipientBankAccount(vendor.PreferredBankAccountCode);
            await _lineRepository.InsertAsync(line, autoSave: true);
            added++;
        }

        return added;
    }

    /// <summary>
    /// Writes the vendor payments of a batch that are not in a file yet to one, and marks them
    /// exported. Every payment needs the vendor's bank account and a bank account to pay from.
    /// </summary>
    public async Task<BankFile> ExportAsync(Guid batchId, PaymentFileFormat format, DateTime createdAt)
    {
        var batch = await _batchRepository.GetAsync(batchId);
        var lines = (await _lineRepository.GetListAsync(l =>
                l.GenJournalBatchId == batchId && l.AccountType == GenJournalAccountType.Vendor && l.Amount > 0m && !l.ExportedToPaymentFile))
            .OrderBy(l => l.LineNo)
            .ToList();
        if (lines.Count == 0)
        {
            throw new BusinessException(ErpErrorCodes.CashManagement.NothingToExport).WithData("batchName", batch.Name);
        }

        var payments = new List<(GenJournalLine Line, Vendor Vendor, VendorBankAccount To, BankAccount From)>();
        foreach (var line in lines)
        {
            if (line.BalAccountType != GenJournalAccountType.BankAccount)
            {
                throw new BusinessException(ErpErrorCodes.CashManagement.PaymentBalAccountNotBank).WithData("lineNo", line.LineNo);
            }

            var vendor = await _vendorRepository.FirstAsync(v => v.No == line.AccountNo);
            var code = line.RecipientBankAccount ?? vendor.PreferredBankAccountCode;
            var to = code == null ? null : await _vendorBankRepository.FirstOrDefaultAsync(b => b.VendorNo == vendor.No && b.Code == code);
            if (to == null || (to.Iban.IsNullOrWhiteSpace() && to.BankAccountNo.IsNullOrWhiteSpace()))
            {
                throw new BusinessException(ErpErrorCodes.CashManagement.PaymentBankAccountMissing).WithData("lineNo", line.LineNo).WithData("vendorNo", vendor.No);
            }

            var from = await _bankRepository.FirstAsync(b => b.No == line.BalAccountNo);
            payments.Add((line, vendor, to, from));
        }

        var file = format == PaymentFileFormat.Csv ? WriteCsv(batch, payments, createdAt) : WriteCreditTransfer(batch, payments, createdAt);
        foreach (var (line, _, _, _) in payments)
        {
            line.MarkExportedToPaymentFile(true);
            await _lineRepository.UpdateAsync(line, autoSave: true);
        }

        return file;
    }

    private static bool DiscountApplies(VendorLedgerEntry entry, VendorPaymentSuggestion request) =>
        request.UsePaymentDiscounts && entry.PmtDiscountDate.HasValue && entry.PmtDiscountDate.Value.Date >= request.PostingDate.Date && entry.RemainingPmtDiscPossible != 0m;

    private static BankFile WriteCsv(GenJournalBatch batch, List<(GenJournalLine Line, Vendor Vendor, VendorBankAccount To, BankAccount From)> payments, DateTime createdAt)
    {
        var rows = payments.Select(p => (IReadOnlyList<object>)
        [
            p.Line.PostingDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            p.From.No,
            p.Vendor.No,
            p.To.Name ?? p.Vendor.Name,
            p.To.BankAccountNo,
            p.To.Iban,
            p.To.SwiftCode,
            p.To.BankBranchNo,
            p.Line.CurrencyCode ?? p.From.CurrencyCode ?? "",
            p.Line.Amount,
            p.Line.PaymentReference ?? p.Line.DocumentNo,
            p.Line.MessageToRecipient,
        ]);
        var content = DelimitedWriter.Write(
            ["PaymentDate", "FromBankAccount", "VendorNo", "Beneficiary", "BankAccountNo", "Iban", "SwiftCode", "BankBranchNo", "Currency", "Amount", "Reference", "Message"],
            rows
        );
        return new BankFile
        {
            FileName = $"{batch.Name}-{createdAt:yyyyMMddHHmmss}.csv",
            ContentType = "text/csv",
            Content = content,
            TransactionCount = payments.Count,
            TotalAmount = payments.Sum(p => p.Line.Amount),
        };
    }

    /// <summary>One credit transfer message, with a payment information block per bank account paid from.</summary>
    private BankFile WriteCreditTransfer(GenJournalBatch batch, List<(GenJournalLine Line, Vendor Vendor, VendorBankAccount To, BankAccount From)> payments, DateTime createdAt)
    {
        XNamespace ns = "urn:iso:std:iso:20022:tech:xsd:pain.001.001.03";
        var messageId = ($"{batch.Name}-{createdAt:yyyyMMddHHmmss}").Truncate(35);
        var company = CurrentCompany.Name ?? "";
        var total = payments.Sum(p => p.Line.Amount);

        var blocks = payments.GroupBy(p => p.From.No).Select((group, index) =>
        {
            var from = group.First().From;
            return new XElement(ns + "PmtInf",
                new XElement(ns + "PmtInfId", ($"{messageId}-{index + 1}").Truncate(35)),
                new XElement(ns + "PmtMtd", "TRF"),
                new XElement(ns + "NbOfTxs", group.Count()),
                new XElement(ns + "CtrlSum", Amount(group.Sum(p => p.Line.Amount))),
                new XElement(ns + "ReqdExctnDt", group.Min(p => p.Line.PostingDate).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)),
                new XElement(ns + "Dbtr", new XElement(ns + "Nm", (company).Truncate(70))),
                new XElement(ns + "DbtrAcct", AccountId(ns, from.Iban, from.BankAccountNo)),
                new XElement(ns + "DbtrAgt", Agent(ns, from.SwiftCode)),
                group.Select(p => new XElement(ns + "CdtTrfTxInf",
                    new XElement(ns + "PmtId", new XElement(ns + "EndToEndId", (p.Line.PaymentReference ?? p.Line.DocumentNo).Truncate(35))),
                    new XElement(ns + "Amt", new XElement(ns + "InstdAmt", new XAttribute("Ccy", p.Line.CurrencyCode ?? from.CurrencyCode ?? "EUR"), Amount(p.Line.Amount))),
                    new XElement(ns + "CdtrAgt", Agent(ns, p.To.SwiftCode)),
                    new XElement(ns + "Cdtr", new XElement(ns + "Nm", (p.To.Name ?? p.Vendor.Name).Truncate(70))),
                    new XElement(ns + "CdtrAcct", AccountId(ns, p.To.Iban, p.To.BankAccountNo)),
                    new XElement(ns + "RmtInf", new XElement(ns + "Ustrd", (p.Line.MessageToRecipient ?? p.Line.DocumentNo).Truncate(140)))))
            );
        });

        var document = new XDocument(
            new XDeclaration("1.0", "UTF-8", null),
            new XElement(ns + "Document",
                new XElement(ns + "CstmrCdtTrfInitn",
                    new XElement(ns + "GrpHdr",
                        new XElement(ns + "MsgId", messageId),
                        new XElement(ns + "CreDtTm", createdAt.ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture)),
                        new XElement(ns + "NbOfTxs", payments.Count),
                        new XElement(ns + "CtrlSum", Amount(total)),
                        new XElement(ns + "InitgPty", new XElement(ns + "Nm", (company).Truncate(70)))),
                    blocks)));

        return new BankFile
        {
            FileName = $"{messageId}.xml",
            ContentType = "application/xml",
            Content = Iso20022.Write(document),
            TransactionCount = payments.Count,
            TotalAmount = total,
        };
    }

    private static string Amount(decimal amount) => amount.ToString("0.00", CultureInfo.InvariantCulture);

    private static XElement AccountId(XNamespace ns, string iban, string accountNo) =>
        new(ns + "Id", iban.IsNullOrWhiteSpace()
            ? new XElement(ns + "Othr", new XElement(ns + "Id", accountNo?.Trim()))
            : new XElement(ns + "IBAN", iban.Replace(" ", "").ToUpperInvariant()));

    private static XElement Agent(XNamespace ns, string bic) =>
        new(ns + "FinInstnId", bic.IsNullOrWhiteSpace()
            ? new XElement(ns + "Othr", new XElement(ns + "Id", "NOTPROVIDED"))
            : new XElement(ns + "BIC", bic.Trim().ToUpperInvariant()));
}

/// <summary>Writes ISO 20022 documents as UTF-8 text with their declaration.</summary>
internal static class Iso20022
{
    public static string Write(XDocument document)
    {
        var builder = new StringBuilder();
        using (var writer = new Utf8StringWriter(builder))
        {
            document.Save(writer);
        }

        return builder.ToString();
    }

    private sealed class Utf8StringWriter : System.IO.StringWriter
    {
        public Utf8StringWriter(StringBuilder builder)
            : base(builder, CultureInfo.InvariantCulture) { }

        public override Encoding Encoding => Encoding.UTF8;
    }
}

/// <summary>
/// Direct Debit Mandate: the customer's permission to collect from one of its bank accounts, from
/// a date, for one collection or for as many as it allows.
/// </summary>
public class DirectDebitMandate : CompanyEntity
{
    public string Code { get; private set; }
    public string CustomerNo { get; private set; }
    public string CustomerBankAccountCode { get; private set; }
    public DateTime ValidFrom { get; private set; }
    public DateTime? ValidTo { get; private set; }
    public DirectDebitMandateType Type { get; private set; }

    /// <summary>How many collections the mandate allows; 0 is no limit.</summary>
    public int ExpectedUses { get; private set; }

    public int UsedCount { get; private set; }
    public bool Blocked { get; private set; }

    protected DirectDebitMandate() { }

    public DirectDebitMandate(Guid id, string code, string customerNo)
        : base(id)
    {
        Code = CodeTableEntity.NormalizeCode(Check.NotNullOrWhiteSpace(code, nameof(code), 35));
        CustomerNo = Check.NotNullOrWhiteSpace(customerNo, nameof(customerNo), ErpDomainConsts.MaxNoLength).Trim();
    }

    public void Set(string customerBankAccountCode, DateTime validFrom, DateTime? validTo, DirectDebitMandateType type, int expectedUses, bool blocked)
    {
        CustomerBankAccountCode = CodeTableEntity.NormalizeCode(Check.NotNullOrWhiteSpace(customerBankAccountCode, nameof(customerBankAccountCode), ErpDomainConsts.MaxCodeLength));
        ValidFrom = validFrom.Date;
        ValidTo = validTo?.Date;
        Type = type;
        ExpectedUses = type == DirectDebitMandateType.OneOff ? 1 : Math.Max(0, expectedUses);
        Blocked = blocked;
    }

    public bool IsValidOn(DateTime date) =>
        !Blocked && date.Date >= ValidFrom && (!ValidTo.HasValue || date.Date <= ValidTo.Value) && (ExpectedUses == 0 || UsedCount < ExpectedUses);

    /// <summary>The sequence type of the next collection: the first, a recurring one, the last, or a one-off.</summary>
    public DirectDebitSequenceType NextSequenceType =>
        Type == DirectDebitMandateType.OneOff
            ? DirectDebitSequenceType.OneOff
            : UsedCount == 0
                ? DirectDebitSequenceType.First
                : ExpectedUses > 0 && UsedCount == ExpectedUses - 1 ? DirectDebitSequenceType.Final : DirectDebitSequenceType.Recurrent;

    internal void RecordUse() => UsedCount++;
}

/// <summary>Direct Debit Collection: the customer invoices collected from their banks in one go, through one of the company's bank accounts.</summary>
public class DirectDebitCollection : CompanyAggregateRoot, IHasNo
{
    public string No { get; private set; }
    public string BankAccountNo { get; private set; }

    /// <summary>The company's creditor identifier with its bank, which the file carries.</summary>
    public string CreditorIdentifier { get; private set; }

    public DateTime CreatedDate { get; private set; }
    public DirectDebitCollectionStatus Status { get; private set; }

    public Collection<DirectDebitCollectionEntry> Entries { get; private set; } = new();

    protected DirectDebitCollection() { }

    public DirectDebitCollection(Guid id, string no, string bankAccountNo, string creditorIdentifier, DateTime createdDate)
        : base(id)
    {
        No = Check.NotNullOrWhiteSpace(no, nameof(no), ErpDomainConsts.MaxNoLength);
        BankAccountNo = Check.NotNullOrWhiteSpace(bankAccountNo, nameof(bankAccountNo), ErpDomainConsts.MaxNoLength);
        CreditorIdentifier = Check.Length(creditorIdentifier?.Trim(), nameof(creditorIdentifier), 35);
        CreatedDate = createdDate.Date;
        Status = DirectDebitCollectionStatus.New;
    }

    internal void MoveTo(DirectDebitCollectionStatus status, params DirectDebitCollectionStatus[] from)
    {
        if (!from.Contains(Status))
        {
            throw new BusinessException(ErpErrorCodes.CashManagement.CollectionStatusWrong).WithData("no", No).WithData("status", Status.ToString());
        }

        Status = status;
    }
}

public class DirectDebitCollectionEntry : Entity<Guid>
{
    public Guid DirectDebitCollectionId { get; private set; }
    public int LineNo { get; private set; }
    public Guid CustomerLedgerEntryId { get; private set; }
    public string CustomerNo { get; private set; }
    public string CustomerName { get; private set; }
    public string DocumentNo { get; private set; }
    public DateTime DueDate { get; private set; }
    public string CurrencyCode { get; private set; }
    public decimal Amount { get; private set; }
    public string MandateCode { get; private set; }
    public DirectDebitSequenceType SequenceType { get; private set; }

    protected DirectDebitCollectionEntry() { }

    public DirectDebitCollectionEntry(
        Guid id,
        Guid collectionId,
        int lineNo,
        CustomerLedgerEntry entry,
        string customerName,
        DirectDebitMandate mandate
    )
        : base(id)
    {
        DirectDebitCollectionId = collectionId;
        LineNo = lineNo;
        CustomerLedgerEntryId = entry.Id;
        CustomerNo = entry.CustomerNo;
        CustomerName = (customerName).Truncate(ErpDomainConsts.MaxNameLength);
        DocumentNo = entry.DocumentNo;
        DueDate = entry.DueDate.Date;
        CurrencyCode = entry.CurrencyCode;
        Amount = entry.RemainingAmount;
        MandateCode = mandate.Code;
        SequenceType = mandate.NextSequenceType;
    }
}

/// <summary>
/// Direct debits: collects the open invoices of customers that have a valid mandate, writes the
/// ISO 20022 direct debit (pain.008.001.02) for the bank, and makes the cash receipt lines that
/// apply the collected money to the invoices.
/// </summary>
public class DirectDebitManager : DomainService
{
    private readonly IRepository<DirectDebitCollection, Guid> _collectionRepository;
    private readonly IRepository<DirectDebitMandate, Guid> _mandateRepository;
    private readonly IRepository<CustomerLedgerEntry, Guid> _customerEntryRepository;
    private readonly IRepository<Customer, Guid> _customerRepository;

    private IRepository<CustomerBankAccount, Guid> CustomerBankRepository => LazyServiceProvider.LazyGetRequiredService<IRepository<CustomerBankAccount, Guid>>();
    private IRepository<BankAccount, Guid> BankRepository => LazyServiceProvider.LazyGetRequiredService<IRepository<BankAccount, Guid>>();
    private IRepository<GenJournalLine, Guid> LineRepository => LazyServiceProvider.LazyGetRequiredService<IRepository<GenJournalLine, Guid>>();
    private ICurrentCompany CurrentCompany => LazyServiceProvider.LazyGetRequiredService<ICurrentCompany>();

    public DirectDebitManager(
        IRepository<DirectDebitCollection, Guid> collectionRepository,
        IRepository<DirectDebitMandate, Guid> mandateRepository,
        IRepository<CustomerLedgerEntry, Guid> customerEntryRepository,
        IRepository<Customer, Guid> customerRepository
    )
    {
        _collectionRepository = collectionRepository;
        _mandateRepository = mandateRepository;
        _customerEntryRepository = customerEntryRepository;
        _customerRepository = customerRepository;
    }

    /// <summary>
    /// Collects the open invoices due between the dates of customers with a valid mandate (the
    /// first valid one by code), leaving out invoices an earlier collection already has.
    /// </summary>
    public async Task<DirectDebitCollection> CreateCollectionAsync(DateTime fromDueDate, DateTime toDueDate, string bankAccountNo, string creditorIdentifier, DateTime createdDate)
    {
        var bankNo = bankAccountNo?.Trim();
        var bank = await BankRepository.FirstOrDefaultAsync(b => b.No == bankNo)
            ?? throw new BusinessException(ErpErrorCodes.CashManagement.VoucherAccountNotFound).WithData("accountNo", bankNo ?? "");

        var from = fromDueDate.Date;
        var to = toDueDate.Date;
        var entries = await _customerEntryRepository.GetListAsync(e =>
            e.Open && e.RemainingAmount > 0m && e.DocumentType == "Invoice" && e.DueDate >= from && e.DueDate <= to);
        var query = await _collectionRepository.WithDetailsAsync(c => c.Entries);
        var taken = (await AsyncExecuter.ToListAsync(query.Where(c => c.Status != DirectDebitCollectionStatus.Closed)))
            .SelectMany(c => c.Entries.Select(e => e.CustomerLedgerEntryId))
            .ToHashSet();
        var mandates = (await _mandateRepository.GetListAsync()).OrderBy(m => m.Code).ToList();

        var collection = new DirectDebitCollection(GuidGenerator.Create(), $"DD-{createdDate:yyyyMMdd}-{await _collectionRepository.CountAsync() + 1:D3}", bank.No, creditorIdentifier, createdDate);
        var lineNo = 0;
        foreach (var entry in entries.Where(e => !taken.Contains(e.Id)).OrderBy(e => e.CustomerNo).ThenBy(e => e.DueDate))
        {
            var mandate = mandates.FirstOrDefault(m => m.CustomerNo == entry.CustomerNo && m.IsValidOn(entry.DueDate));
            if (mandate == null)
            {
                continue;
            }

            var customer = await _customerRepository.GetAsync(entry.CustomerId);
            lineNo += 10000;
            collection.Entries.Add(new DirectDebitCollectionEntry(GuidGenerator.Create(), collection.Id, lineNo, entry, customer.Name, mandate));
        }

        if (collection.Entries.Count == 0)
        {
            throw new BusinessException(ErpErrorCodes.CashManagement.NothingToCollect)
                .WithData("fromDate", from.ToString("yyyy-MM-dd"))
                .WithData("toDate", to.ToString("yyyy-MM-dd"));
        }

        return await _collectionRepository.InsertAsync(collection, autoSave: true);
    }

    /// <summary>The pain.008 file of a collection, one payment information block per sequence type.</summary>
    public async Task<BankFile> ExportAsync(Guid collectionId, DateTime createdAt)
    {
        var collection = await _collectionRepository.GetAsync(collectionId, includeDetails: true);
        collection.MoveTo(DirectDebitCollectionStatus.FileCreated, DirectDebitCollectionStatus.New, DirectDebitCollectionStatus.FileCreated);
        var bank = await BankRepository.FirstAsync(b => b.No == collection.BankAccountNo);

        XNamespace ns = "urn:iso:std:iso:20022:tech:xsd:pain.008.001.02";
        var company = CurrentCompany.Name ?? "";
        var mandates = (await _mandateRepository.GetListAsync()).ToDictionary(m => m.Code);
        var debtors = new Dictionary<string, CustomerBankAccount>();
        foreach (var entry in collection.Entries)
        {
            var mandate = mandates[entry.MandateCode];
            var key = $"{mandate.CustomerNo}|{mandate.CustomerBankAccountCode}";
            if (!debtors.ContainsKey(key))
            {
                debtors[key] = await CustomerBankRepository.FirstOrDefaultAsync(b => b.CustomerNo == mandate.CustomerNo && b.Code == mandate.CustomerBankAccountCode)
                    ?? throw new BusinessException(ErpErrorCodes.CashManagement.CustomerBankAccountMissing).WithData("customerNo", mandate.CustomerNo).WithData("code", mandate.CustomerBankAccountCode);
            }
        }

        var total = collection.Entries.Sum(e => e.Amount);
        var blocks = collection.Entries.GroupBy(e => e.SequenceType).Select((group, index) => new XElement(ns + "PmtInf",
            new XElement(ns + "PmtInfId", ($"{collection.No}-{index + 1}").Truncate(35)),
            new XElement(ns + "PmtMtd", "DD"),
            new XElement(ns + "NbOfTxs", group.Count()),
            new XElement(ns + "CtrlSum", Amount(group.Sum(e => e.Amount))),
            new XElement(ns + "PmtTpInf",
                new XElement(ns + "SvcLvl", new XElement(ns + "Cd", "SEPA")),
                new XElement(ns + "LclInstrm", new XElement(ns + "Cd", "CORE")),
                new XElement(ns + "SeqTp", SequenceCode(group.Key))),
            new XElement(ns + "ReqdColltnDt", group.Min(e => e.DueDate).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)),
            new XElement(ns + "Cdtr", new XElement(ns + "Nm", (company).Truncate(70))),
            new XElement(ns + "CdtrAcct", AccountId(ns, bank.Iban, bank.BankAccountNo)),
            new XElement(ns + "CdtrAgt", Agent(ns, bank.SwiftCode)),
            new XElement(ns + "CdtrSchmeId", new XElement(ns + "Id", new XElement(ns + "PrvtId", new XElement(ns + "Othr",
                new XElement(ns + "Id", collection.CreditorIdentifier ?? bank.No),
                new XElement(ns + "SchmeNm", new XElement(ns + "Prtry", "SEPA")))))),
            group.OrderBy(e => e.LineNo).Select(e =>
            {
                var mandate = mandates[e.MandateCode];
                var debtor = debtors[$"{mandate.CustomerNo}|{mandate.CustomerBankAccountCode}"];
                return new XElement(ns + "DrctDbtTxInf",
                    new XElement(ns + "PmtId", new XElement(ns + "EndToEndId", (e.DocumentNo).Truncate(35))),
                    new XElement(ns + "InstdAmt", new XAttribute("Ccy", e.CurrencyCode ?? bank.CurrencyCode ?? "EUR"), Amount(e.Amount)),
                    new XElement(ns + "DrctDbtTx", new XElement(ns + "MndtRltdInf",
                        new XElement(ns + "MndtId", mandate.Code),
                        new XElement(ns + "DtOfSgntr", mandate.ValidFrom.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)))),
                    new XElement(ns + "DbtrAgt", Agent(ns, debtor.SwiftCode)),
                    new XElement(ns + "Dbtr", new XElement(ns + "Nm", (debtor.Name ?? e.CustomerName).Truncate(70))),
                    new XElement(ns + "DbtrAcct", AccountId(ns, debtor.Iban, debtor.BankAccountNo)),
                    new XElement(ns + "RmtInf", new XElement(ns + "Ustrd", ($"{e.DocumentNo} {e.CustomerName}").Truncate(140))));
            })));

        var document = new XDocument(
            new XDeclaration("1.0", "UTF-8", null),
            new XElement(ns + "Document",
                new XElement(ns + "CstmrDrctDbtInitn",
                    new XElement(ns + "GrpHdr",
                        new XElement(ns + "MsgId", (collection.No).Truncate(35)),
                        new XElement(ns + "CreDtTm", createdAt.ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture)),
                        new XElement(ns + "NbOfTxs", collection.Entries.Count),
                        new XElement(ns + "CtrlSum", Amount(total)),
                        new XElement(ns + "InitgPty", new XElement(ns + "Nm", (company).Truncate(70)))),
                    blocks)));

        await _collectionRepository.UpdateAsync(collection, autoSave: true);
        return new BankFile
        {
            FileName = $"{collection.No}.xml",
            ContentType = "application/xml",
            Content = Iso20022.Write(document),
            TransactionCount = collection.Entries.Count,
            TotalAmount = total,
        };
    }

    /// <summary>
    /// The bank has collected: a cash receipt line per entry in the batch, applied to its invoice
    /// and received on the collection's bank account. Posting the batch pays the invoices.
    /// </summary>
    public async Task<int> CreateReceiptLinesAsync(Guid collectionId, Guid batchId, DateTime postingDate)
    {
        var collection = await _collectionRepository.GetAsync(collectionId, includeDetails: true);
        collection.MoveTo(DirectDebitCollectionStatus.Posted, DirectDebitCollectionStatus.New, DirectDebitCollectionStatus.FileCreated);
        var existing = await LineRepository.GetListAsync(l => l.GenJournalBatchId == batchId);
        var lineNo = existing.Count == 0 ? 0 : existing.Max(l => l.LineNo);
        var mandates = (await _mandateRepository.GetListAsync()).ToDictionary(m => m.Code);

        foreach (var entry in collection.Entries.OrderBy(e => e.LineNo))
        {
            lineNo += 10000;
            var line = new GenJournalLine(
                GuidGenerator.Create(),
                batchId,
                lineNo,
                postingDate,
                GLEntryDocumentType.Payment,
                collection.No,
                GenJournalAccountType.Customer,
                entry.CustomerNo,
                ($"{entry.CustomerName} {entry.DocumentNo}").Truncate(ErpDomainConsts.MaxDescriptionLength),
                -entry.Amount,
                GenJournalAccountType.BankAccount,
                collection.BankAccountNo
            );
            line.Update(postingDate, GLEntryDocumentType.Payment, collection.No, GenJournalAccountType.Customer, entry.CustomerNo, line.Description, -entry.Amount,
                GenJournalAccountType.BankAccount, collection.BankAccountNo, appliesToDocNo: entry.DocumentNo);
            await LineRepository.InsertAsync(line, autoSave: true);

            var mandate = mandates[entry.MandateCode];
            mandate.RecordUse();
            await _mandateRepository.UpdateAsync(mandate, autoSave: true);
        }

        await _collectionRepository.UpdateAsync(collection, autoSave: true);
        return collection.Entries.Count;
    }

    public async Task<DirectDebitCollection> CloseAsync(Guid collectionId)
    {
        var collection = await _collectionRepository.GetAsync(collectionId, includeDetails: true);
        collection.MoveTo(DirectDebitCollectionStatus.Closed, DirectDebitCollectionStatus.New, DirectDebitCollectionStatus.FileCreated, DirectDebitCollectionStatus.Posted);
        return await _collectionRepository.UpdateAsync(collection, autoSave: true);
    }

    private static string SequenceCode(DirectDebitSequenceType type) => type switch
    {
        DirectDebitSequenceType.First => "FRST",
        DirectDebitSequenceType.Final => "FNAL",
        DirectDebitSequenceType.OneOff => "OOFF",
        _ => "RCUR",
    };

    private static string Amount(decimal amount) => amount.ToString("0.00", CultureInfo.InvariantCulture);

    private static XElement AccountId(XNamespace ns, string iban, string accountNo) =>
        new(ns + "Id", iban.IsNullOrWhiteSpace()
            ? new XElement(ns + "Othr", new XElement(ns + "Id", accountNo?.Trim()))
            : new XElement(ns + "IBAN", iban.Replace(" ", "").ToUpperInvariant()));

    private static XElement Agent(XNamespace ns, string bic) =>
        new(ns + "FinInstnId", bic.IsNullOrWhiteSpace()
            ? new XElement(ns + "Othr", new XElement(ns + "Id", "NOTPROVIDED"))
            : new XElement(ns + "BIC", bic.Trim().ToUpperInvariant()));
}
