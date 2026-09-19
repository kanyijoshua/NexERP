using System;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Companies;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace ABPmicroservice.Erp.Sales;

/// <summary>
/// Sales &amp; Receivables Setup. Mirrors Business Central table 311: one row per company,
/// naming the number series each kind of sales record draws from. A blank code means
/// numbers are typed by hand.
/// </summary>
public class SalesReceivablesSetup : CompanyEntity
{
    public string CustomerNos { get; private set; }
    public string QuoteNos { get; private set; }
    public string OrderNos { get; private set; }
    public string InvoiceNos { get; private set; }
    public string CreditMemoNos { get; private set; }
    public string PostedInvoiceNos { get; private set; }
    public string PostedCreditMemoNos { get; private set; }

    protected SalesReceivablesSetup() { }

    public SalesReceivablesSetup(Guid id)
        : base(id) { }

    public void SetNumberSeries(
        string customerNos,
        string quoteNos,
        string orderNos,
        string invoiceNos,
        string creditMemoNos,
        string postedInvoiceNos,
        string postedCreditMemoNos
    )
    {
        CustomerNos = Code(customerNos, nameof(customerNos));
        QuoteNos = Code(quoteNos, nameof(quoteNos));
        OrderNos = Code(orderNos, nameof(orderNos));
        InvoiceNos = Code(invoiceNos, nameof(invoiceNos));
        CreditMemoNos = Code(creditMemoNos, nameof(creditMemoNos));
        PostedInvoiceNos = Code(postedInvoiceNos, nameof(postedInvoiceNos));
        PostedCreditMemoNos = Code(postedCreditMemoNos, nameof(postedCreditMemoNos));
    }

    /// <summary>The series that numbers a new, unposted document of this type.</summary>
    public string GetDocumentNos(SalesDocumentType documentType)
    {
        return documentType switch
        {
            SalesDocumentType.Quote => QuoteNos,
            SalesDocumentType.Order => OrderNos,
            SalesDocumentType.Invoice => InvoiceNos,
            SalesDocumentType.CreditMemo => CreditMemoNos,
            _ => null,
        };
    }

    /// <summary>The series that numbers the posted document.</summary>
    public string GetPostedDocumentNos(SalesDocumentType documentType)
    {
        return documentType == SalesDocumentType.CreditMemo ? PostedCreditMemoNos : PostedInvoiceNos;
    }

    private static string Code(string value, string name)
    {
        return value.IsNullOrWhiteSpace() ? null : Check.Length(value.Trim(), name, ErpDomainConsts.MaxNoSeriesCodeLength);
    }
}

public class SalesReceivablesSetupManager : DomainService
{
    private readonly IRepository<SalesReceivablesSetup, Guid> _repository;

    public SalesReceivablesSetupManager(IRepository<SalesReceivablesSetup, Guid> repository)
    {
        _repository = repository;
    }

    /// <summary>The current company's setup, created blank on first use.</summary>
    public async Task<SalesReceivablesSetup> GetAsync()
    {
        return await _repository.FirstOrDefaultAsync()
            ?? await _repository.InsertAsync(new SalesReceivablesSetup(GuidGenerator.Create()), autoSave: true);
    }
}
