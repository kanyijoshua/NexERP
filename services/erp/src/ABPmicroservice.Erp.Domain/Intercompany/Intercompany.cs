using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Companies;
using ABPmicroservice.Erp.Documents;
using ABPmicroservice.Erp.Finance;
using ABPmicroservice.Erp.Inventory;
using ABPmicroservice.Erp.Purchasing;
using ABPmicroservice.Erp.Sales;
using Volo.Abp;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace ABPmicroservice.Erp.Intercompany;

/// <summary>
/// IC Partner: another company of the group, and the customer and vendor that stand for it in this
/// company. A sales or purchase document posted to that customer or vendor goes to the partner.
/// </summary>
public class ICPartner : CodeTableEntity
{
    public Guid PartnerCompanyId { get; private set; }
    public string CustomerNo { get; private set; }
    public string VendorNo { get; private set; }

    /// <summary>Lines whose item or account does not exist here are booked on this account.</summary>
    public string DefaultGLAccountNo { get; private set; }

    public bool Blocked { get; private set; }

    protected ICPartner() { }

    public ICPartner(Guid id, string code, string description, Guid partnerCompanyId)
        : base(id, code, description)
    {
        PartnerCompanyId = partnerCompanyId;
    }

    public void Set(Guid partnerCompanyId, string customerNo, string vendorNo, string defaultGLAccountNo, bool blocked)
    {
        PartnerCompanyId = partnerCompanyId;
        CustomerNo = Clean(customerNo, nameof(customerNo));
        VendorNo = Clean(vendorNo, nameof(vendorNo));
        DefaultGLAccountNo = Clean(defaultGLAccountNo, nameof(defaultGLAccountNo));
        Blocked = blocked;
    }

    private static string Clean(string no, string name) =>
        no.IsNullOrWhiteSpace() ? null : Check.Length(no.Trim(), name, ErpDomainConsts.MaxNoLength);
}

/// <summary>IC Outbox Transaction: a posted document of this company waiting to go to (or gone to) its partner.</summary>
public class ICOutboxTransaction : CompanyEntity
{
    public long TransactionNo { get; private set; }
    public string PartnerCode { get; private set; }
    public ICSourceType SourceType { get; private set; }
    public Guid SourceId { get; private set; }
    public string DocumentNo { get; private set; }
    public DateTime PostingDate { get; private set; }
    public decimal Amount { get; private set; }
    public ICOutboxStatus Status { get; private set; }
    public DateTime? SentDate { get; private set; }

    protected ICOutboxTransaction() { }

    public ICOutboxTransaction(Guid id, long transactionNo, string partnerCode, ICSourceType sourceType, Guid sourceId, string documentNo, DateTime postingDate, decimal amount)
        : base(id)
    {
        TransactionNo = transactionNo;
        PartnerCode = partnerCode;
        SourceType = sourceType;
        SourceId = sourceId;
        DocumentNo = documentNo;
        PostingDate = postingDate.Date;
        Amount = amount;
        Status = ICOutboxStatus.New;
    }

    internal void MarkSent(DateTime date)
    {
        EnsureNew();
        Status = ICOutboxStatus.Sent;
        SentDate = date.Date;
    }

    internal void Cancel()
    {
        EnsureNew();
        Status = ICOutboxStatus.Cancelled;
    }

    private void EnsureNew()
    {
        if (Status != ICOutboxStatus.New)
        {
            throw new BusinessException(ErpErrorCodes.Intercompany.InboxTransactionHandled).WithData("no", TransactionNo).WithData("status", Status.ToString());
        }
    }
}

/// <summary>
/// IC Inbox Transaction: a document a partner sent this company, waiting to be accepted (which
/// makes the matching purchase or sales document here) or rejected.
/// </summary>
public class ICInboxTransaction : CompanyAggregateRoot
{
    public long TransactionNo { get; private set; }
    public Guid FromCompanyId { get; private set; }

    /// <summary>The sending company as this company knows it; null when this company has no partner for it.</summary>
    public string PartnerCode { get; private set; }

    public ICSourceType SourceType { get; private set; }
    public ICDocumentType DocumentType { get; private set; }
    public string PartnerDocumentNo { get; private set; }
    public DateTime PostingDate { get; private set; }
    public DateTime DueDate { get; private set; }
    public string CurrencyCode { get; private set; }
    public decimal Amount { get; private set; }
    public ICInboxStatus Status { get; private set; }

    /// <summary>The document accepting made.</summary>
    public string CreatedDocumentNo { get; private set; }

    public Collection<ICInboxLine> Lines { get; private set; } = new();

    protected ICInboxTransaction() { }

    public ICInboxTransaction(
        Guid id,
        long transactionNo,
        Guid fromCompanyId,
        string partnerCode,
        ICSourceType sourceType,
        string partnerDocumentNo,
        DateTime postingDate,
        DateTime dueDate,
        string currencyCode,
        decimal amount
    )
        : base(id)
    {
        TransactionNo = transactionNo;
        FromCompanyId = fromCompanyId;
        PartnerCode = partnerCode;
        SourceType = sourceType;
        DocumentType = sourceType switch
        {
            ICSourceType.SalesInvoice => ICDocumentType.PurchaseInvoice,
            ICSourceType.SalesCreditMemo => ICDocumentType.PurchaseCreditMemo,
            ICSourceType.PurchaseInvoice => ICDocumentType.SalesInvoice,
            _ => ICDocumentType.SalesCreditMemo,
        };
        PartnerDocumentNo = partnerDocumentNo;
        PostingDate = postingDate.Date;
        DueDate = dueDate.Date;
        CurrencyCode = currencyCode;
        Amount = amount;
        Status = ICInboxStatus.Pending;
    }

    public void AddLine(Guid id, ICLineType type, string no, string description, decimal quantity, decimal unitAmount, decimal lineDiscountPct, string unitOfMeasureCode)
    {
        Lines.Add(new ICInboxLine(id, Id, (Lines.Count + 1) * 10000, type, no, description, quantity, unitAmount, lineDiscountPct, unitOfMeasureCode));
    }

    internal void Accept(string createdDocumentNo)
    {
        EnsurePending();
        CreatedDocumentNo = createdDocumentNo;
        Status = ICInboxStatus.Accepted;
    }

    internal void Reject()
    {
        EnsurePending();
        Status = ICInboxStatus.Rejected;
    }

    private void EnsurePending()
    {
        if (Status != ICInboxStatus.Pending)
        {
            throw new BusinessException(ErpErrorCodes.Intercompany.InboxTransactionHandled).WithData("no", TransactionNo).WithData("status", Status.ToString());
        }
    }
}

public class ICInboxLine : Entity<Guid>
{
    public Guid ICInboxTransactionId { get; private set; }
    public int LineNo { get; private set; }
    public ICLineType Type { get; private set; }
    public string No { get; private set; }
    public string Description { get; private set; }
    public decimal Quantity { get; private set; }

    /// <summary>The partner's unit price or direct unit cost, which is the other side's cost or price.</summary>
    public decimal UnitAmount { get; private set; }

    public decimal LineDiscountPct { get; private set; }
    public string UnitOfMeasureCode { get; private set; }

    protected ICInboxLine() { }

    public ICInboxLine(Guid id, Guid transactionId, int lineNo, ICLineType type, string no, string description, decimal quantity, decimal unitAmount, decimal lineDiscountPct, string unitOfMeasureCode)
        : base(id)
    {
        ICInboxTransactionId = transactionId;
        LineNo = lineNo;
        Type = type;
        No = no;
        Description = description;
        Quantity = quantity;
        UnitAmount = unitAmount;
        LineDiscountPct = lineDiscountPct;
        UnitOfMeasureCode = unitOfMeasureCode;
    }
}

/// <summary>
/// Intercompany: a sales or purchase document posted to a partner's customer or vendor is put in
/// this company's outbox; sending it puts it in the partner's inbox, where accepting it makes the
/// mirror document (a sales invoice becomes the partner's purchase invoice, and the other way
/// round) for the partner to post.
/// </summary>
public class IntercompanyManager : DomainService
{
    private readonly IRepository<ICPartner, Guid> _partnerRepository;
    private readonly IRepository<ICOutboxTransaction, Guid> _outboxRepository;
    private readonly IRepository<ICInboxTransaction, Guid> _inboxRepository;

    private ICurrentCompany CurrentCompany => LazyServiceProvider.LazyGetRequiredService<ICurrentCompany>();
    private Sequences.IEntryNoGenerator EntryNos => LazyServiceProvider.LazyGetRequiredService<Sequences.IEntryNoGenerator>();
    private IRepository<T, Guid> Repo<T>() where T : class, IEntity<Guid> => LazyServiceProvider.LazyGetRequiredService<IRepository<T, Guid>>();

    public IntercompanyManager(IRepository<ICPartner, Guid> partnerRepository, IRepository<ICOutboxTransaction, Guid> outboxRepository, IRepository<ICInboxTransaction, Guid> inboxRepository)
    {
        _partnerRepository = partnerRepository;
        _outboxRepository = outboxRepository;
        _inboxRepository = inboxRepository;
    }

    /// <summary>Puts a posted sales invoice or credit memo to a partner's customer in the outbox.</summary>
    public async Task QueueSalesAsync(PostedSalesHeader posted, string customerNo)
    {
        var partner = await _partnerRepository.FirstOrDefaultAsync(p => p.CustomerNo == customerNo && !p.Blocked);
        if (partner != null)
        {
            await QueueAsync(partner, posted.CreditMemo ? ICSourceType.SalesCreditMemo : ICSourceType.SalesInvoice, posted.Id, posted.No, posted.PostingDate, posted.TotalAmountIncludingVat);
        }
    }

    /// <summary>Puts a posted purchase invoice or credit memo from a partner's vendor in the outbox.</summary>
    public async Task QueuePurchaseAsync(PostedPurchaseHeader posted)
    {
        var vendorNo = posted.BuyFromVendorNo;
        var partner = await _partnerRepository.FirstOrDefaultAsync(p => p.VendorNo == vendorNo && !p.Blocked);
        if (partner != null)
        {
            await QueueAsync(partner, posted.CreditMemo ? ICSourceType.PurchaseCreditMemo : ICSourceType.PurchaseInvoice, posted.Id, posted.No, posted.PostingDate, posted.TotalAmountIncludingVat);
        }
    }

    /// <summary>Sends an outbox transaction to the partner's inbox; returns the inbox transaction number there.</summary>
    public async Task<long> SendAsync(Guid outboxId)
    {
        var outbox = await _outboxRepository.GetAsync(outboxId);
        var code = outbox.PartnerCode;
        var partner = await _partnerRepository.FirstOrDefaultAsync(p => p.Code == code)
            ?? throw new BusinessException(ErpErrorCodes.Intercompany.PartnerNotFound).WithData("code", code);
        var senderId = CurrentCompany.Id ?? Guid.Empty;
        if (partner.PartnerCompanyId == Guid.Empty || partner.PartnerCompanyId == senderId)
        {
            throw new BusinessException(ErpErrorCodes.Intercompany.PartnerCompanyMissing).WithData("code", partner.Code);
        }

        var (header, lines) = await ReadSourceAsync(outbox);
        long inboxNo;
        using (CurrentCompany.Change(partner.PartnerCompanyId))
        {
            // The sender as the partner knows it.
            var back = await _partnerRepository.FirstOrDefaultAsync(p => p.PartnerCompanyId == senderId);
            inboxNo = await EntryNos.NextAsync(Sequences.ErpSequenceNames.ICInboxTransaction);
            var inbox = new ICInboxTransaction(GuidGenerator.Create(), inboxNo, senderId, back?.Code, outbox.SourceType, outbox.DocumentNo, header.PostingDate, header.DueDate, header.CurrencyCode, outbox.Amount);
            foreach (var line in lines)
            {
                inbox.AddLine(GuidGenerator.Create(), line.Type, line.No, line.Description, line.Quantity, line.UnitAmount, line.LineDiscountPct, line.UnitOfMeasureCode);
            }

            await _inboxRepository.InsertAsync(inbox, autoSave: true);
        }

        outbox.MarkSent(Clock.Now);
        await _outboxRepository.UpdateAsync(outbox, autoSave: true);
        return inboxNo;
    }

    public async Task CancelAsync(Guid outboxId)
    {
        var outbox = await _outboxRepository.GetAsync(outboxId);
        outbox.Cancel();
        await _outboxRepository.UpdateAsync(outbox, autoSave: true);
    }

    /// <summary>
    /// Makes the document an inbox transaction stands for: a purchase invoice (or credit memo) from
    /// the partner's vendor, or a sales invoice (or credit memo) to its customer. Items that exist
    /// here keep their number; other lines go to the G/L account of the same number, else to the
    /// partner's default account. Returns the document made.
    /// </summary>
    public async Task<string> AcceptAsync(Guid inboxId, DateTime postingDate)
    {
        var inbox = await _inboxRepository.GetAsync(inboxId, includeDetails: true);
        var code = inbox.PartnerCode;
        var partner = code == null ? null : await _partnerRepository.FirstOrDefaultAsync(p => p.Code == code);
        if (partner == null)
        {
            throw new BusinessException(ErpErrorCodes.Intercompany.PartnerNotFound).WithData("code", code ?? "");
        }

        var purchase = inbox.DocumentType is ICDocumentType.PurchaseInvoice or ICDocumentType.PurchaseCreditMemo;
        var lines = new List<(DocumentLineType Type, string No, ICInboxLine Line)>();
        foreach (var line in inbox.Lines.OrderBy(l => l.LineNo))
        {
            lines.Add(await MapLineAsync(partner, line));
        }

        var documentNo = purchase ? await MakePurchaseAsync(inbox, partner, lines, postingDate) : await MakeSalesAsync(inbox, partner, lines, postingDate);
        inbox.Accept(documentNo);
        await _inboxRepository.UpdateAsync(inbox, autoSave: true);
        return documentNo;
    }

    public async Task RejectAsync(Guid inboxId)
    {
        var inbox = await _inboxRepository.GetAsync(inboxId);
        inbox.Reject();
        await _inboxRepository.UpdateAsync(inbox, autoSave: true);
    }

    private async Task QueueAsync(ICPartner partner, ICSourceType sourceType, Guid sourceId, string documentNo, DateTime postingDate, decimal amount)
    {
        var no = await EntryNos.NextAsync(Sequences.ErpSequenceNames.ICOutboxTransaction);
        await _outboxRepository.InsertAsync(new ICOutboxTransaction(GuidGenerator.Create(), no, partner.Code, sourceType, sourceId, documentNo, postingDate, amount), autoSave: true);
    }

    private sealed record SourceLine(ICLineType Type, string No, string Description, decimal Quantity, decimal UnitAmount, decimal LineDiscountPct, string UnitOfMeasureCode);

    private sealed record SourceHeader(DateTime PostingDate, DateTime DueDate, string CurrencyCode);

    private async Task<(SourceHeader Header, List<SourceLine> Lines)> ReadSourceAsync(ICOutboxTransaction outbox)
    {
        if (outbox.SourceType is ICSourceType.SalesInvoice or ICSourceType.SalesCreditMemo)
        {
            var posted = await Repo<PostedSalesHeader>().GetAsync(outbox.SourceId, includeDetails: true);
            return (
                new SourceHeader(posted.PostingDate, posted.DueDate, posted.CurrencyCode),
                posted.Lines.Where(l => !l.No.IsNullOrWhiteSpace()).OrderBy(l => l.LineNo)
                    .Select(l => new SourceLine(LineType(l.Type), l.No, l.Description, l.Quantity, l.UnitPrice, l.LineDiscountPercent, l.UnitOfMeasureCode))
                    .ToList()
            );
        }

        var purchase = await Repo<PostedPurchaseHeader>().GetAsync(outbox.SourceId, includeDetails: true);
        return (
            new SourceHeader(purchase.PostingDate, purchase.DueDate, purchase.CurrencyCode),
            purchase.Lines.Where(l => !l.No.IsNullOrWhiteSpace()).OrderBy(l => l.LineNo)
                .Select(l => new SourceLine(LineType(l.Type), l.No, l.Description, l.Quantity, l.DirectUnitCost, l.LineDiscountPercent, l.UnitOfMeasureCode))
                .ToList()
        );
    }

    private static ICLineType LineType(string postedType) => postedType == nameof(DocumentLineType.Item) ? ICLineType.Item : ICLineType.GLAccount;

    private async Task<(DocumentLineType, string, ICInboxLine)> MapLineAsync(ICPartner partner, ICInboxLine line)
    {
        if (line.Type == ICLineType.Item && await Repo<Item>().AnyAsync(i => i.No == line.No))
        {
            return (DocumentLineType.Item, line.No, line);
        }

        var no = line.No;
        if (line.Type == ICLineType.GLAccount && await Repo<GLAccount>().AnyAsync(a => a.No == no))
        {
            return (DocumentLineType.GLAccount, no, line);
        }

        if (partner.DefaultGLAccountNo == null)
        {
            throw new BusinessException(ErpErrorCodes.Intercompany.ICLineAccountMissing).WithData("lineNo", line.LineNo).WithData("no", line.No).WithData("code", partner.Code);
        }

        return (DocumentLineType.GLAccount, partner.DefaultGLAccountNo, line);
    }

    private async Task<string> MakePurchaseAsync(ICInboxTransaction inbox, ICPartner partner, List<(DocumentLineType Type, string No, ICInboxLine Line)> lines, DateTime postingDate)
    {
        var vendorNo = partner.VendorNo;
        var vendor = vendorNo == null ? null : await Repo<Vendor>().FirstOrDefaultAsync(v => v.No == vendorNo);
        if (vendor == null)
        {
            throw new BusinessException(ErpErrorCodes.Intercompany.PartnerVendorMissing).WithData("code", partner.Code);
        }

        var type = inbox.DocumentType == ICDocumentType.PurchaseCreditMemo ? PurchaseDocumentType.CreditMemo : PurchaseDocumentType.Invoice;
        var setup = await LazyServiceProvider.LazyGetRequiredService<PurchasesPayablesSetupManager>().GetAsync();
        var series = setup.GetDocumentNos(type);
        var no = series.IsNullOrWhiteSpace()
            ? $"IC-{inbox.TransactionNo:D5}"
            : await LazyServiceProvider.LazyGetRequiredService<Numbering.NoSeriesManager>().GetNextNoAsync(series, postingDate);

        var document = new PurchaseHeader(GuidGenerator.Create(), type, no, vendor.Id, vendor.No, vendor.Name, postingDate);
        document.SetDates(postingDate, inbox.DueDate, null);
        document.SetCurrency(inbox.CurrencyCode);
        document.SetPaymentTerms(vendor.PaymentTermsCode);
        document.SetPostingGroups(vendor.VendorPostingGroup, vendor.GenBusPostingGroup, vendor.VatBusPostingGroup);
        document.SetVendorInvoiceNo(inbox.PartnerDocumentNo);
        document.SetYourReference($"IC {inbox.TransactionNo}");
        foreach (var (lineType, lineNo, line) in lines)
        {
            document.AddLine(GuidGenerator.Create(), lineType, lineNo, line.Description, line.Quantity, line.UnitAmount, line.LineDiscountPct, lineType == DocumentLineType.Item ? line.UnitOfMeasureCode : null);
        }

        await LazyServiceProvider.LazyGetRequiredService<DocumentVatCalculator>().ApplyAsync(document, vendor);
        await Repo<PurchaseHeader>().InsertAsync(document, autoSave: true);
        return document.No;
    }

    private async Task<string> MakeSalesAsync(ICInboxTransaction inbox, ICPartner partner, List<(DocumentLineType Type, string No, ICInboxLine Line)> lines, DateTime postingDate)
    {
        var customerNo = partner.CustomerNo;
        var customer = customerNo == null ? null : await Repo<Customer>().FirstOrDefaultAsync(c => c.No == customerNo);
        if (customer == null)
        {
            throw new BusinessException(ErpErrorCodes.Intercompany.PartnerCustomerMissing).WithData("code", partner.Code);
        }

        var type = inbox.DocumentType == ICDocumentType.SalesCreditMemo ? SalesDocumentType.CreditMemo : SalesDocumentType.Invoice;
        var setup = await LazyServiceProvider.LazyGetRequiredService<SalesReceivablesSetupManager>().GetAsync();
        var series = setup.GetDocumentNos(type);
        var no = series.IsNullOrWhiteSpace()
            ? $"IC-{inbox.TransactionNo:D5}"
            : await LazyServiceProvider.LazyGetRequiredService<Numbering.NoSeriesManager>().GetNextNoAsync(series, postingDate);

        var document = new SalesHeader(GuidGenerator.Create(), type, no, customer.Id, customer.No, customer.Name, postingDate);
        document.SetDates(postingDate, inbox.DueDate);
        document.SetCurrency(inbox.CurrencyCode);
        document.SetPaymentTerms(customer.PaymentTermsCode);
        document.SetExternalDocumentNo(inbox.PartnerDocumentNo);
        foreach (var (lineType, lineNo, line) in lines)
        {
            document.AddLine(GuidGenerator.Create(), lineType, lineNo, line.Description, line.Quantity, line.UnitAmount, line.LineDiscountPct, lineType == DocumentLineType.Item ? line.UnitOfMeasureCode : null);
        }

        await LazyServiceProvider.LazyGetRequiredService<DocumentVatCalculator>().ApplyAsync(document, customer);
        await Repo<SalesHeader>().InsertAsync(document, autoSave: true);
        return document.No;
    }
}
