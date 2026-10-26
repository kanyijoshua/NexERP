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

namespace ABPmicroservice.Erp.Inventory;

/// <summary>
/// Freight and other landed costs: a Charge (Item) line is assigned to receipts and its cost
/// lands on their item entries and on the inventory account. FREIGHT is seeded under RETAIL with
/// no VAT group; item 1000 is stock on inventory account 1400.
/// </summary>
public class ItemCharges_Tests : ErpApplicationTestBase
{
    private static readonly DateTime PostingDate = new(2026, 3, 1);

    private readonly IPurchaseDocumentAppService _purchaseDocuments;
    private readonly IItemChargeAssignmentAppService _assignments;
    private readonly IVendorAppService _vendors;
    private readonly IRepository<GLEntry, Guid> _glEntries;
    private readonly IRepository<ItemLedgerEntry, Guid> _itemEntries;
    private readonly IRepository<ValueEntry, Guid> _valueEntries;

    public ItemCharges_Tests()
    {
        _purchaseDocuments = GetRequiredService<IPurchaseDocumentAppService>();
        _assignments = GetRequiredService<IItemChargeAssignmentAppService>();
        _vendors = GetRequiredService<IVendorAppService>();
        _glEntries = GetRequiredService<IRepository<GLEntry, Guid>>();
        _itemEntries = GetRequiredService<IRepository<ItemLedgerEntry, Guid>>();
        _valueEntries = GetRequiredService<IRepository<ValueEntry, Guid>>();
    }

    [Fact]
    public async Task Freight_Is_Assigned_To_A_Receipt_And_Adds_To_Its_Cost()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            var receiptNo = await ReceiveAsync("PO-CHG-1", quantity: 10, unitCost: 100m);
            var receiptEntry = (await _itemEntries.GetListAsync(e => e.DocumentNo == receiptNo)).Single();

            var invoice = await InvoiceWithFreightAsync("PI-CHG-1", 50m);
            var chargeLine = invoice.Lines.Single();

            // An unassigned charge is not posted.
            var unassigned = await Should.ThrowAsync<BusinessException>(() => _purchaseDocuments.RunPostingAsync(invoice.Id));
            unassigned.Code.ShouldBe(ErpErrorCodes.ItemCharges.NotFullyAssigned);

            var candidates = await _assignments.GetAssignableEntriesAsync(new GetAssignableEntriesInput { SourceType = ItemChargeSourceType.PurchaseLine, SourceLineId = chargeLine.Id });
            candidates.ShouldContain(e => e.Id == receiptEntry.Id);

            var suggested = await _assignments.SuggestAllocationAsync(new SuggestItemChargeAllocationInput
            {
                SourceType = ItemChargeSourceType.PurchaseLine,
                SourceLineId = chargeLine.Id,
                TargetItemLedgerEntryIds = [receiptEntry.Id],
            });
            var assignment = suggested.ShouldHaveSingleItem();
            assignment.QtyToAssign.ShouldBe(1m);
            assignment.AmountToAssign.ShouldBe(50m);
            assignment.AppliesToDocumentNo.ShouldBe(receiptNo);

            var postedNo = (await _purchaseDocuments.RunPostingAsync(invoice.Id)).PostedDocumentNo;

            var entries = await _glEntries.GetListAsync(e => e.DocumentNo == postedNo);
            Sum(entries, "1400").ShouldBe(50m);
            entries.Sum(e => e.Amount).ShouldBe(0m);

            var value = (await _valueEntries.GetListAsync(v => v.DocumentNo == postedNo)).ShouldHaveSingleItem();
            value.EntryType.ShouldBe(ValueEntryType.ItemCharge);
            value.CostAmountActual.ShouldBe(50m);

            var costed = await _itemEntries.GetAsync(receiptEntry.Id);
            costed.CostAmountActual.ShouldBe(1050m);
            costed.UnitCost.ShouldBe(105m);

            var posted = (await _assignments.GetListAsync(new GetItemChargeAssignmentListInput { SourceLineId = chargeLine.Id, MaxResultCount = 10 })).Items.Single();
            posted.Posted.ShouldBeTrue();
            posted.AmountPostedLcy.ShouldBe(50m);
        });
    }

    [Fact]
    public async Task A_Charge_Is_Split_Over_Receipts_By_Amount()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            var small = await ReceiveAsync("PO-CHG-2", quantity: 2, unitCost: 100m);   // worth 200
            var large = await ReceiveAsync("PO-CHG-3", quantity: 6, unitCost: 100m);   // worth 600
            var smallEntry = (await _itemEntries.GetListAsync(e => e.DocumentNo == small)).Single();
            var largeEntry = (await _itemEntries.GetListAsync(e => e.DocumentNo == large)).Single();

            var invoice = await InvoiceWithFreightAsync("PI-CHG-2", 80m);
            var split = await _assignments.SuggestAllocationAsync(new SuggestItemChargeAllocationInput
            {
                SourceType = ItemChargeSourceType.PurchaseLine,
                SourceLineId = invoice.Lines.Single().Id,
                Method = ItemChargeAllocationMethod.ByAmount,
                TargetItemLedgerEntryIds = [smallEntry.Id, largeEntry.Id],
            });

            split.Single(a => a.AppliesToEntryId == smallEntry.Id).AmountToAssign.ShouldBe(20m);
            split.Single(a => a.AppliesToEntryId == largeEntry.Id).AmountToAssign.ShouldBe(60m);
            split.Sum(a => a.QtyToAssign).ShouldBe(1m);

            // Assigning more than the line holds is refused.
            var over = await Should.ThrowAsync<BusinessException>(() => _assignments.CreateAsync(new CreateItemChargeAssignmentDto
            {
                SourceType = ItemChargeSourceType.PurchaseLine,
                SourceLineId = invoice.Lines.Single().Id,
                AppliesToEntryId = smallEntry.Id,
                QtyToAssign = 1,
            }));
            over.Code.ShouldBe(ErpErrorCodes.ItemCharges.QuantityExceeded);

            var postedNo = (await _purchaseDocuments.RunPostingAsync(invoice.Id)).PostedDocumentNo;
            (await _itemEntries.GetAsync(smallEntry.Id)).CostAmountActual.ShouldBe(220m);
            (await _itemEntries.GetAsync(largeEntry.Id)).CostAmountActual.ShouldBe(660m);
            Sum(await _glEntries.GetListAsync(e => e.DocumentNo == postedNo), "1400").ShouldBe(80m);
        });
    }

    private async Task<string> ReceiveAsync(string orderNo, decimal quantity, decimal unitCost)
    {
        var vendor = await _vendors.GetByNoAsync("V00010");
        var order = await _purchaseDocuments.CreateAsync(new CreateUpdatePurchaseHeaderDto
        {
            DocumentType = PurchaseDocumentType.Order,
            No = orderNo,
            VendorId = vendor.Id,
            PostingDate = PostingDate,
            VendorInvoiceNo = "VI-" + orderNo,
            Lines = [new PurchaseLineInputDto { Type = DocumentLineType.Item, No = "1000", Description = "Bicycle", Quantity = quantity, DirectUnitCost = unitCost }],
        });

        var result = await _purchaseDocuments.RunOrderPostingAsync(order.Id, new PurchaseOrderPostingInput { Receive = true, Invoice = true });
        return result.ReceiptNo;
    }

    private async Task<PurchaseHeaderDto> InvoiceWithFreightAsync(string no, decimal amount)
    {
        var vendor = await _vendors.GetByNoAsync("V00010");
        return await _purchaseDocuments.CreateAsync(new CreateUpdatePurchaseHeaderDto
        {
            DocumentType = PurchaseDocumentType.Invoice,
            No = no,
            VendorId = vendor.Id,
            PostingDate = PostingDate,
            VendorInvoiceNo = "VI-" + no,
            Lines = [new PurchaseLineInputDto { Type = DocumentLineType.ChargeItem, No = "FREIGHT", Description = "Freight", Quantity = 1, DirectUnitCost = amount }],
        });
    }

    private static decimal Sum(IEnumerable<GLEntry> entries, string accountNo) =>
        entries.Where(e => e.GLAccountNo == accountNo).Sum(e => e.Amount);
}
