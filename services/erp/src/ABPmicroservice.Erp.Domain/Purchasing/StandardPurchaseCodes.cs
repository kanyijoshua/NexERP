using System;
using System.Collections.ObjectModel;
using System.Linq;
using ABPmicroservice.Erp.Companies;
using ABPmicroservice.Erp.Documents;
using Volo.Abp;
using Volo.Abp.Domain.Entities;

namespace ABPmicroservice.Erp.Purchasing;

/// <summary>Standard Purchase Code: a set of lines bought again and again (rent, supplies) that can be put on any purchase document.</summary>
public class StandardPurchaseCode : CodeTableEntity
{
    public string CurrencyCode { get; private set; }

    public Collection<StandardPurchaseLine> Lines { get; private set; } = new();

    protected StandardPurchaseCode() { }

    public StandardPurchaseCode(Guid id, string code, string description)
        : base(id, code, description) { }

    public void SetCurrency(string currencyCode) =>
        CurrencyCode = NormalizeCode(Check.Length(currencyCode, nameof(currencyCode), ErpDomainConsts.MaxCurrencyCodeLength));

    public StandardPurchaseLine AddLine(Guid id, DocumentLineType type, string no, string description, decimal quantity, decimal unitAmount, string unitOfMeasureCode)
    {
        var line = new StandardPurchaseLine(id, Id, (Lines.Count == 0 ? 0 : Lines.Max(l => l.LineNo)) + 10000);
        line.Set(type, no, description, quantity, unitAmount, unitOfMeasureCode);
        Lines.Add(line);
        return line;
    }
}

public class StandardPurchaseLine : Entity<Guid>
{
    public Guid StandardPurchaseCodeId { get; private set; }
    public int LineNo { get; private set; }
    public DocumentLineType Type { get; private set; }
    public string No { get; private set; }
    public string Description { get; private set; }
    public decimal Quantity { get; private set; }

    /// <summary>The direct unit cost; zero on an item line takes the item's last direct cost.</summary>
    public decimal UnitAmount { get; private set; }

    public string UnitOfMeasureCode { get; private set; }

    protected StandardPurchaseLine() { }

    public StandardPurchaseLine(Guid id, Guid codeId, int lineNo)
        : base(id)
    {
        StandardPurchaseCodeId = codeId;
        LineNo = lineNo;
    }

    public void Set(DocumentLineType type, string no, string description, decimal quantity, decimal unitAmount, string unitOfMeasureCode)
    {
        Type = type;
        No = no.IsNullOrWhiteSpace() ? null : Check.Length(no.Trim(), nameof(no), ErpDomainConsts.MaxNoLength);
        Description = Check.Length(description, nameof(description), ErpDomainConsts.MaxDescriptionLength);
        Quantity = quantity;
        UnitAmount = unitAmount;
        UnitOfMeasureCode = CodeTableEntity.NormalizeCode(Check.Length(unitOfMeasureCode, nameof(unitOfMeasureCode), ErpDomainConsts.MaxUnitOfMeasureCodeLength));
    }
}

/// <summary>Standard Vendor Purchase Code: a standard code bought from a vendor, put on its new documents by hand or automatically.</summary>
public class StandardVendorPurchaseCode : CompanyEntity
{
    public string VendorNo { get; private set; }
    public string Code { get; private set; }
    public StandardCodeInsertMode InsertOnQuotes { get; private set; }
    public StandardCodeInsertMode InsertOnOrders { get; private set; }
    public StandardCodeInsertMode InsertOnInvoices { get; private set; }

    protected StandardVendorPurchaseCode() { }

    public StandardVendorPurchaseCode(Guid id, string vendorNo, string code)
        : base(id)
    {
        VendorNo = Check.NotNullOrWhiteSpace(vendorNo, nameof(vendorNo), ErpDomainConsts.MaxNoLength).Trim();
        Code = CodeTableEntity.NormalizeCode(Check.NotNullOrWhiteSpace(code, nameof(code), ErpDomainConsts.MaxCodeLength));
    }

    public void Set(StandardCodeInsertMode insertOnQuotes, StandardCodeInsertMode insertOnOrders, StandardCodeInsertMode insertOnInvoices)
    {
        InsertOnQuotes = insertOnQuotes;
        InsertOnOrders = insertOnOrders;
        InsertOnInvoices = insertOnInvoices;
    }

    public bool InsertsAutomaticallyOn(PurchaseDocumentType documentType) => documentType switch
    {
        PurchaseDocumentType.Quote => InsertOnQuotes == StandardCodeInsertMode.Automatic,
        PurchaseDocumentType.Order => InsertOnOrders == StandardCodeInsertMode.Automatic,
        PurchaseDocumentType.Invoice => InsertOnInvoices == StandardCodeInsertMode.Automatic,
        _ => false,
    };
}
