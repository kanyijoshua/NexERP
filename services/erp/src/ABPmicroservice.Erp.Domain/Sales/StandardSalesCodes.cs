using System;
using System.Collections.ObjectModel;
using System.Linq;
using ABPmicroservice.Erp.Companies;
using ABPmicroservice.Erp.Documents;
using Volo.Abp;
using Volo.Abp.Domain.Entities;

namespace ABPmicroservice.Erp.Sales;

/// <summary>Standard Sales Code: a set of lines sold again and again (a monthly fee, a service kit) that can be put on any sales document.</summary>
public class StandardSalesCode : CodeTableEntity
{
    public string CurrencyCode { get; private set; }

    public Collection<StandardSalesLine> Lines { get; private set; } = new();

    protected StandardSalesCode() { }

    public StandardSalesCode(Guid id, string code, string description)
        : base(id, code, description) { }

    public void SetCurrency(string currencyCode) =>
        CurrencyCode = NormalizeCode(Check.Length(currencyCode, nameof(currencyCode), ErpDomainConsts.MaxCurrencyCodeLength));

    public StandardSalesLine AddLine(Guid id, DocumentLineType type, string no, string description, decimal quantity, decimal unitAmount, string unitOfMeasureCode)
    {
        var line = new StandardSalesLine(id, Id, (Lines.Count == 0 ? 0 : Lines.Max(l => l.LineNo)) + 10000);
        line.Set(type, no, description, quantity, unitAmount, unitOfMeasureCode);
        Lines.Add(line);
        return line;
    }
}

public class StandardSalesLine : Entity<Guid>
{
    public Guid StandardSalesCodeId { get; private set; }
    public int LineNo { get; private set; }
    public DocumentLineType Type { get; private set; }
    public string No { get; private set; }
    public string Description { get; private set; }
    public decimal Quantity { get; private set; }

    /// <summary>The unit price; zero on an item line takes the price list's price.</summary>
    public decimal UnitAmount { get; private set; }

    public string UnitOfMeasureCode { get; private set; }

    protected StandardSalesLine() { }

    public StandardSalesLine(Guid id, Guid codeId, int lineNo)
        : base(id)
    {
        StandardSalesCodeId = codeId;
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

/// <summary>Standard Customer Sales Code: a standard code a customer buys, put on its new documents by hand or automatically.</summary>
public class StandardCustomerSalesCode : CompanyEntity
{
    public string CustomerNo { get; private set; }
    public string Code { get; private set; }
    public StandardCodeInsertMode InsertOnQuotes { get; private set; }
    public StandardCodeInsertMode InsertOnOrders { get; private set; }
    public StandardCodeInsertMode InsertOnInvoices { get; private set; }

    protected StandardCustomerSalesCode() { }

    public StandardCustomerSalesCode(Guid id, string customerNo, string code)
        : base(id)
    {
        CustomerNo = Check.NotNullOrWhiteSpace(customerNo, nameof(customerNo), ErpDomainConsts.MaxNoLength).Trim();
        Code = CodeTableEntity.NormalizeCode(Check.NotNullOrWhiteSpace(code, nameof(code), ErpDomainConsts.MaxCodeLength));
    }

    public void Set(StandardCodeInsertMode insertOnQuotes, StandardCodeInsertMode insertOnOrders, StandardCodeInsertMode insertOnInvoices)
    {
        InsertOnQuotes = insertOnQuotes;
        InsertOnOrders = insertOnOrders;
        InsertOnInvoices = insertOnInvoices;
    }

    public bool InsertsAutomaticallyOn(SalesDocumentType documentType) => documentType switch
    {
        SalesDocumentType.Quote => InsertOnQuotes == StandardCodeInsertMode.Automatic,
        SalesDocumentType.Order => InsertOnOrders == StandardCodeInsertMode.Automatic,
        SalesDocumentType.Invoice => InsertOnInvoices == StandardCodeInsertMode.Automatic,
        _ => false,
    };
}
