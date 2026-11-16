using System;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Documents;
using ABPmicroservice.Erp.Purchasing;
using Shouldly;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Xunit;

namespace ABPmicroservice.Erp.Sales;

/// <summary>
/// The purchase order card's header fields beyond those posting reads: order date, campaign, VAT
/// date, foreign trade, receipt times and the prepayment terms, which the prepayment invoice uses.
/// </summary>
public class PurchaseOrderCardFields_Tests : ErpApplicationTestBase
{
    private static readonly DateTime PostingDate = new(2026, 3, 1);

    private readonly IPurchaseDocumentAppService _purchaseDocuments;
    private readonly IVendorAppService _vendors;
    private readonly IRepository<PostedPurchaseHeader, Guid> _postedHeaders;

    public PurchaseOrderCardFields_Tests()
    {
        _purchaseDocuments = GetRequiredService<IPurchaseDocumentAppService>();
        _vendors = GetRequiredService<IVendorAppService>();
        _postedHeaders = GetRequiredService<IRepository<PostedPurchaseHeader, Guid>>();
    }

    [Fact]
    public async Task The_Card_Fields_Are_Saved_And_Read_Back()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            var input = await OrderAsync("PO-CARD-1");
            input.OrderDate = new DateTime(2026, 2, 20);
            input.VatReportingDate = new DateTime(2026, 3, 5);
            input.TransactionType = "11";
            input.TransactionSpecification = "a1";
            input.TransportMethod = "3";
            input.EntryPoint = "msa";
            input.Area = "30";
            input.LeadTimeCalculation = "5d";
            input.InboundWhseHandlingTime = "1D";
            input.CompressPrepayment = false;
            input.ShipToName = "Main warehouse";

            var order = await _purchaseDocuments.CreateAsync(input);

            order.OrderDate.ShouldBe(new DateTime(2026, 2, 20));
            order.VatReportingDate.ShouldBe(new DateTime(2026, 3, 5));
            order.TransactionType.ShouldBe("11");
            order.TransactionSpecification.ShouldBe("A1");
            order.EntryPoint.ShouldBe("MSA");
            order.Area.ShouldBe("30");
            order.LeadTimeCalculation.ShouldBe("5D");
            order.InboundWhseHandlingTime.ShouldBe("1D");
            order.CompressPrepayment.ShouldBeFalse();
            order.ShipToName.ShouldBe("Main warehouse");

            // A blank order date on update keeps the one the order has.
            input.OrderDate = null;
            var updated = await _purchaseDocuments.UpdateAsync(order.Id, input);
            updated.OrderDate.ShouldBe(new DateTime(2026, 2, 20));
        });
    }

    [Fact]
    public async Task A_Lead_Time_That_Is_Not_A_Date_Formula_Is_Refused()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            var input = await OrderAsync("PO-CARD-2");
            input.LeadTimeCalculation = "soon";

            var error = await Should.ThrowAsync<BusinessException>(() => _purchaseDocuments.CreateAsync(input));
            error.Code.ShouldBe(ErpErrorCodes.Journals.InvalidDateFormula);
        });
    }

    [Fact]
    public async Task The_Prepayment_Invoice_Falls_Due_By_The_Prepayment_Terms()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            var input = await OrderAsync("PO-CARD-3");
            input.PaymentTermsCode = "30D";
            input.PrepaymentPct = 50m;
            input.PrepmtPaymentTermsCode = "14D";

            var order = await _purchaseDocuments.CreateAsync(input);
            order.DueDate.ShouldBe(PostingDate.AddDays(30));
            order.PrepaymentDueDate.ShouldBe(PostingDate.AddDays(14));

            var prepayment = await _purchaseDocuments.RunPrepaymentPostingAsync(order.Id);
            var posted = await _postedHeaders.SingleAsync(x => x.No == prepayment.InvoiceNo);
            posted.DueDate.ShouldBe(PostingDate.AddDays(14));
        });
    }

    private async Task<CreateUpdatePurchaseHeaderDto> OrderAsync(string no)
    {
        var vendor = await _vendors.GetByNoAsync("V00010");
        return new CreateUpdatePurchaseHeaderDto
        {
            DocumentType = PurchaseDocumentType.Order,
            No = no,
            VendorId = vendor.Id,
            PostingDate = PostingDate,
            VendorInvoiceNo = "VI-" + no,
            Lines = [new PurchaseLineInputDto { Type = DocumentLineType.Item, No = "1000", Description = "Bicycle", Quantity = 2, DirectUnitCost = 100m }],
        };
    }
}
