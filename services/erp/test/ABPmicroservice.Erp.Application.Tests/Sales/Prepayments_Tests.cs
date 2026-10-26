using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Documents;
using ABPmicroservice.Erp.Finance;
using ABPmicroservice.Erp.Purchasing;
using Shouldly;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Xunit;

namespace ABPmicroservice.Erp.Sales;

/// <summary>
/// Prepayment invoices on orders and their deduction from the final invoice. The seed puts
/// customer prepayments on 2420 and purchase prepayments on 1550 in every general posting setup;
/// item 1000 carries 16% VAT for DOMESTIC parties.
/// </summary>
public class Prepayments_Tests : ErpApplicationTestBase
{
    private static readonly DateTime PostingDate = new(2026, 3, 1);

    private readonly ISalesDocumentAppService _salesDocuments;
    private readonly IPurchaseDocumentAppService _purchaseDocuments;
    private readonly ICustomerAppService _customers;
    private readonly IVendorAppService _vendors;
    private readonly IRepository<GLEntry, Guid> _glEntries;

    public Prepayments_Tests()
    {
        _salesDocuments = GetRequiredService<ISalesDocumentAppService>();
        _purchaseDocuments = GetRequiredService<IPurchaseDocumentAppService>();
        _customers = GetRequiredService<ICustomerAppService>();
        _vendors = GetRequiredService<IVendorAppService>();
        _glEntries = GetRequiredService<IRepository<GLEntry, Guid>>();
    }

    [Fact]
    public async Task A_Sales_Prepayment_Is_Invoiced_Then_Deducted_From_The_Final_Invoice()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            var order = await _salesDocuments.CreateAsync(await SalesOrderAsync("SO-PREP-1", prepaymentPct: 20m));
            order.PrepaymentPct.ShouldBe(20m);
            order.Lines.Single().PrepmtLineAmount.ShouldBe(600m);

            var prepayment = await _salesDocuments.RunPrepaymentPostingAsync(order.Id);
            prepayment.InvoiceNo.ShouldNotBeNull();
            var prepEntries = await _glEntries.GetListAsync(e => e.DocumentNo == prepayment.InvoiceNo);
            Sum(prepEntries, "1200").ShouldBe(696m);   // 600 + 16% VAT
            Sum(prepEntries, "2420").ShouldBe(-600m);
            prepEntries.Sum(e => e.Amount).ShouldBe(0m);
            prepayment.Document.PrepmtInvoicedAmount.ShouldBe(600m);
            prepayment.Document.LastPrepmtInvoiceNo.ShouldBe(prepayment.InvoiceNo);

            // Nothing more to prepay.
            var again = await Should.ThrowAsync<BusinessException>(() => _salesDocuments.RunPrepaymentPostingAsync(order.Id));
            again.Code.ShouldBe(ErpErrorCodes.Documents.NothingToPost);

            // The final invoice charges the rest and releases the prepayment.
            var final = await _salesDocuments.RunOrderPostingAsync(order.Id, new SalesOrderPostingInput { Ship = true, Invoice = true });
            var entries = await _glEntries.GetListAsync(e => e.DocumentNo == final.InvoiceNo);
            Sum(entries, "1200").ShouldBe(2784m);  // (3000 - 600) + 16% VAT
            Sum(entries, "4000").ShouldBe(-3000m);
            Sum(entries, "2420").ShouldBe(600m);
            entries.Sum(e => e.Amount).ShouldBe(0m);
            final.Document.PrepmtDeductedAmount.ShouldBe(600m);
        });
    }

    [Fact]
    public async Task A_Prepayment_Credit_Memo_Reverses_What_Was_Prepaid()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            var order = await _salesDocuments.CreateAsync(await SalesOrderAsync("SO-PREP-2", prepaymentPct: 50m));
            await _salesDocuments.RunPrepaymentPostingAsync(order.Id);

            var credit = await _salesDocuments.RunPrepaymentCreditMemoPostingAsync(order.Id);
            var entries = await _glEntries.GetListAsync(e => e.DocumentNo == credit.InvoiceNo);
            Sum(entries, "1200").ShouldBe(-1740m);
            Sum(entries, "2420").ShouldBe(1500m);
            entries.Sum(e => e.Amount).ShouldBe(0m);
            entries.ShouldAllBe(e => e.DocumentType == GLEntryDocumentType.CreditMemo);

            credit.Document.PrepmtInvoicedAmount.ShouldBe(0m);
            credit.Document.LastPrepmtCrMemoNo.ShouldBe(credit.InvoiceNo);

            // With nothing prepaid the final invoice charges everything.
            var final = await _salesDocuments.RunOrderPostingAsync(order.Id, new SalesOrderPostingInput { Ship = true, Invoice = true });
            Sum(await _glEntries.GetListAsync(e => e.DocumentNo == final.InvoiceNo), "1200").ShouldBe(3480m);
        });
    }

    [Fact]
    public async Task A_Purchase_Prepayment_Is_An_Asset_Until_The_Invoice_Deducts_It()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            var vendor = await _vendors.GetByNoAsync("V00010");
            var order = await _purchaseDocuments.CreateAsync(new CreateUpdatePurchaseHeaderDto
            {
                DocumentType = PurchaseDocumentType.Order,
                No = "PO-PREP-1",
                VendorId = vendor.Id,
                PostingDate = PostingDate,
                VendorInvoiceNo = "VI-PREP-1",
                PrepaymentPct = 30m,
                Lines = [new PurchaseLineInputDto { Type = DocumentLineType.Item, No = "1000", Description = "Bicycle", Quantity = 10, DirectUnitCost = 100m }],
            });

            var prepayment = await _purchaseDocuments.RunPrepaymentPostingAsync(order.Id);
            var prepEntries = await _glEntries.GetListAsync(e => e.DocumentNo == prepayment.InvoiceNo);
            Sum(prepEntries, "1550").ShouldBe(300m);
            prepEntries.Sum(e => e.Amount).ShouldBe(0m);

            var final = await _purchaseDocuments.RunOrderPostingAsync(order.Id, new PurchaseOrderPostingInput { Receive = true, Invoice = true });
            var entries = await _glEntries.GetListAsync(e => e.DocumentNo == final.InvoiceNo);
            Sum(entries, "1550").ShouldBe(-300m);
            entries.Sum(e => e.Amount).ShouldBe(0m);
            final.Document.PrepmtDeductedAmount.ShouldBe(300m);
        });
    }

    private static decimal Sum(IEnumerable<GLEntry> entries, string accountNo) =>
        entries.Where(e => e.GLAccountNo == accountNo).Sum(e => e.Amount);

    private async Task<CreateUpdateSalesHeaderDto> SalesOrderAsync(string no, decimal prepaymentPct)
    {
        var customer = await _customers.GetByNoAsync("C00010");

        return new CreateUpdateSalesHeaderDto
        {
            DocumentType = SalesDocumentType.Order,
            No = no,
            CustomerId = customer.Id,
            PostingDate = PostingDate,
            PrepaymentPct = prepaymentPct,
            Lines =
            [
                new SalesLineInputDto { Type = DocumentLineType.Item, No = "1000", Description = "Bicycle Assembly", Quantity = 10, UnitPrice = 300m },
            ],
        };
    }
}
