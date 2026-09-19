using System;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Companies;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace ABPmicroservice.Erp.Purchasing;

/// <summary>
/// Purchases &amp; Payables Setup. Mirrors Business Central table 312: one row per company,
/// naming the number series each kind of purchase record draws from. A blank code means
/// numbers are typed by hand.
/// </summary>
public class PurchasesPayablesSetup : CompanyEntity
{
    public string VendorNos { get; private set; }
    public string QuoteNos { get; private set; }
    public string OrderNos { get; private set; }
    public string InvoiceNos { get; private set; }
    public string CreditMemoNos { get; private set; }
    public string PostedInvoiceNos { get; private set; }
    public string PostedCreditMemoNos { get; private set; }

    protected PurchasesPayablesSetup() { }

    public PurchasesPayablesSetup(Guid id)
        : base(id) { }

    public void SetNumberSeries(
        string vendorNos,
        string quoteNos,
        string orderNos,
        string invoiceNos,
        string creditMemoNos,
        string postedInvoiceNos,
        string postedCreditMemoNos
    )
    {
        VendorNos = Code(vendorNos, nameof(vendorNos));
        QuoteNos = Code(quoteNos, nameof(quoteNos));
        OrderNos = Code(orderNos, nameof(orderNos));
        InvoiceNos = Code(invoiceNos, nameof(invoiceNos));
        CreditMemoNos = Code(creditMemoNos, nameof(creditMemoNos));
        PostedInvoiceNos = Code(postedInvoiceNos, nameof(postedInvoiceNos));
        PostedCreditMemoNos = Code(postedCreditMemoNos, nameof(postedCreditMemoNos));
    }

    /// <summary>The series that numbers a new, unposted document of this type.</summary>
    public string GetDocumentNos(PurchaseDocumentType documentType)
    {
        return documentType switch
        {
            PurchaseDocumentType.Quote => QuoteNos,
            PurchaseDocumentType.Order => OrderNos,
            PurchaseDocumentType.Invoice => InvoiceNos,
            PurchaseDocumentType.CreditMemo => CreditMemoNos,
            _ => null,
        };
    }

    /// <summary>The series that numbers the posted document.</summary>
    public string GetPostedDocumentNos(PurchaseDocumentType documentType)
    {
        return documentType == PurchaseDocumentType.CreditMemo ? PostedCreditMemoNos : PostedInvoiceNos;
    }

    private static string Code(string value, string name)
    {
        return value.IsNullOrWhiteSpace() ? null : Check.Length(value.Trim(), name, ErpDomainConsts.MaxNoSeriesCodeLength);
    }
}

public class PurchasesPayablesSetupManager : DomainService
{
    private readonly IRepository<PurchasesPayablesSetup, Guid> _repository;

    public PurchasesPayablesSetupManager(IRepository<PurchasesPayablesSetup, Guid> repository)
    {
        _repository = repository;
    }

    /// <summary>The current company's setup, created blank on first use.</summary>
    public async Task<PurchasesPayablesSetup> GetAsync()
    {
        return await _repository.FirstOrDefaultAsync()
            ?? await _repository.InsertAsync(new PurchasesPayablesSetup(GuidGenerator.Create()), autoSave: true);
    }
}
