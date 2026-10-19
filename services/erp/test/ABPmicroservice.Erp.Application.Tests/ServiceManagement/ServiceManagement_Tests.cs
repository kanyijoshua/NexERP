using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Finance;
using ABPmicroservice.Erp.Inventory;
using ABPmicroservice.Erp.Projects;
using Shouldly;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Xunit;

namespace ABPmicroservice.Erp.ServiceManagement;

/// <summary>
/// Service orders posted through a sales invoice, warranty work at no charge, contract periods
/// invoiced on their own, and serial numbers of service items. The seed has customer C00010,
/// location MAIN, service sales on 4300 and contract sales on 4310.
/// </summary>
public class ServiceManagement_Tests : ErpApplicationTestBase
{
    private readonly IServiceItemAppService _serviceItems;
    private readonly IServiceOrderAppService _orders;
    private readonly IServiceItemLineAppService _itemLines;
    private readonly IServiceLineAppService _lines;
    private readonly IServiceContractAppService _contracts;
    private readonly IServiceContractLineAppService _contractLines;
    private readonly IRepository<GLEntry, Guid> _glEntries;
    private readonly IRepository<Item, Guid> _itemRepository;

    public ServiceManagement_Tests()
    {
        _serviceItems = GetRequiredService<IServiceItemAppService>();
        _orders = GetRequiredService<IServiceOrderAppService>();
        _itemLines = GetRequiredService<IServiceItemLineAppService>();
        _lines = GetRequiredService<IServiceLineAppService>();
        _contracts = GetRequiredService<IServiceContractAppService>();
        _contractLines = GetRequiredService<IServiceContractLineAppService>();
        _glEntries = GetRequiredService<IRepository<GLEntry, Guid>>();
        _itemRepository = GetRequiredService<IRepository<Item, Guid>>();
    }

    [Fact]
    public async Task Posting_An_Order_Invoices_Parts_And_Labour_And_Balances()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            await StockedPartAsync("S1-PART", 20m);
            await ResourceAsync("R-S1");
            var serviceItem = await _serviceItems.CreateAsync(new CreateUpdateServiceItemDto { No = "S1-PUMP", CustomerNo = "C00010", Description = "Pump", WarrantyStartingDate = new DateTime(2024, 1, 1), WarrantyEndingDate = new DateTime(2024, 12, 31) });

            var order = await _orders.CreateAsync(new CreateUpdateServiceOrderDto { No = "SO-S1", CustomerNo = "C00010", OrderDate = new DateTime(2026, 4, 10), ResourceNo = "R-S1" });
            var itemLine = await _itemLines.CreateAsync(new CreateUpdateServiceItemLineDto { ServiceOrderId = order.Id, ServiceItemNo = serviceItem.No });
            itemLine.Warranty.ShouldBeFalse();
            await _lines.CreateAsync(new CreateUpdateServiceLineDto { ServiceOrderId = order.Id, ServiceItemNo = serviceItem.No, Type = ServiceLineType.Item, No = "S1-PART", LocationCode = "MAIN", Quantity = 2m, UnitPrice = 50m });
            var labour = await _lines.CreateAsync(new CreateUpdateServiceLineDto { ServiceOrderId = order.Id, ServiceItemNo = serviceItem.No, Type = ServiceLineType.Resource, No = "R-S1", Quantity = 3m });
            labour.UnitPrice.ShouldBe(80m); // the resource's price

            var result = await _orders.RunPostingAsync(order.Id, new ServicePostingInput { PostingDate = new DateTime(2026, 4, 10) });

            result.Amount.ShouldBe(100m + 240m);
            result.InvoiceNo.ShouldNotBeNullOrWhiteSpace();
            var entries = await _glEntries.GetListAsync(e => e.DocumentNo == result.InvoiceNo);
            entries.Sum(e => e.Amount).ShouldBe(0m);
            Sum(entries, "4300").ShouldBe(-240m);
            (await _itemRepository.FirstAsync(i => i.No == "S1-PART")).Inventory.ShouldBe(8m);

            var posted = await _orders.GetAsync(order.Id);
            posted.Status.ShouldBe(ServiceOrderStatus.Finished);
            posted.Lines.ShouldAllBe(l => l.QuantityPosted == l.Quantity);
            (await GetRequiredService<IRepository<ServiceLedgerEntry, Guid>>().GetListAsync(e => e.ServiceOrderNo == "SO-S1")).Sum(e => e.Amount).ShouldBe(340m);
            (await GetRequiredService<IRepository<ResourceLedgerEntry, Guid>>().GetListAsync(e => e.ResourceNo == "R-S1")).Single().TotalCost.ShouldBe(90m);

            // A finished order cannot be posted or changed again.
            await Should.ThrowAsync<BusinessException>(() => _orders.RunPostingAsync(order.Id, new ServicePostingInput { PostingDate = new DateTime(2026, 4, 11) }));
        });
    }

    [Fact]
    public async Task Warranty_Work_Is_Free_To_The_Customer_And_Goes_To_The_Warranty_Ledger()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            await StockedPartAsync("S2-PART", 20m);
            await ResourceAsync("R-S2");
            var serviceItem = await _serviceItems.CreateAsync(new CreateUpdateServiceItemDto { No = "S2-PUMP", CustomerNo = "C00010", ItemNo = "S2-PART", SerialNo = "SN-S2", InstallationDate = new DateTime(2026, 1, 1) });
            serviceItem.WarrantyEndingDate.ShouldBe(new DateTime(2026, 12, 31)); // twelve months from the seed's setup

            var order = await _orders.CreateAsync(new CreateUpdateServiceOrderDto { No = "SO-S2", CustomerNo = "C00010", OrderDate = new DateTime(2026, 4, 10) });
            (await _itemLines.CreateAsync(new CreateUpdateServiceItemLineDto { ServiceOrderId = order.Id, ServiceItemNo = serviceItem.No })).Warranty.ShouldBeTrue();
            var part = await _lines.CreateAsync(new CreateUpdateServiceLineDto { ServiceOrderId = order.Id, ServiceItemNo = serviceItem.No, Type = ServiceLineType.Item, No = "S2-PART", LocationCode = "MAIN", Quantity = 1m, UnitPrice = 50m });
            part.Warranty.ShouldBeTrue();
            await _lines.CreateAsync(new CreateUpdateServiceLineDto { ServiceOrderId = order.Id, ServiceItemNo = serviceItem.No, Type = ServiceLineType.Resource, No = "R-S2", Quantity = 1m });

            var result = await _orders.RunPostingAsync(order.Id, new ServicePostingInput { PostingDate = new DateTime(2026, 4, 10) });

            result.Amount.ShouldBe(0m);
            result.WarrantyAmount.ShouldBe(50m + 80m);
            (await _itemRepository.FirstAsync(i => i.No == "S2-PART")).Inventory.ShouldBe(9m); // the part still leaves stock
            var warranty = await GetRequiredService<IRepository<WarrantyLedgerEntry, Guid>>().GetListAsync(e => e.ServiceOrderNo == "SO-S2");
            warranty.Count.ShouldBe(2);
            warranty.ShouldAllBe(e => e.SerialNo == "SN-S2" && e.ItemNo == "S2-PART");
            warranty.Sum(e => e.Amount).ShouldBe(130m);
            if (result.InvoiceNo != null)
            {
                var entries = await _glEntries.GetListAsync(e => e.DocumentNo == result.InvoiceNo);
                entries.Sum(e => e.Amount).ShouldBe(0m);
                Sum(entries, "4300").ShouldBe(0m);
            }
        });
    }

    [Fact]
    public async Task A_Signed_Contract_Is_Invoiced_Once_Per_Started_Period()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            var serviceItem = await _serviceItems.CreateAsync(new CreateUpdateServiceItemDto { No = "S3-PUMP", CustomerNo = "C00010", Description = "Pump" });
            var contract = await _contracts.CreateAsync(new CreateUpdateServiceContractDto { No = "SC-S3", CustomerNo = "C00010", Description = "Pump care", StartingDate = new DateTime(2026, 1, 1), InvoicePeriod = ServiceInvoicePeriod.Month });
            await _contractLines.CreateAsync(new CreateUpdateServiceContractLineDto { ServiceContractId = contract.Id, ServiceItemNo = serviceItem.No, LineValue = 1200m });
            await _contractLines.CreateAsync(new CreateUpdateServiceContractLineDto { ServiceContractId = contract.Id, Description = "Call-outs", LineValue = 600m });

            // Nothing is invoiced before signing.
            (await _contracts.InvoiceDueAsync(contract.Id, new InvoiceServiceContractsInput { AsOfDate = new DateTime(2026, 3, 15) })).InvoicedPeriods.ShouldBe(0);

            contract = await _contracts.SignAsync(contract.Id);
            contract.AnnualAmount.ShouldBe(1800m);
            contract.PeriodAmount.ShouldBe(150m);
            await Should.ThrowAsync<BusinessException>(() => _contractLines.CreateAsync(new CreateUpdateServiceContractLineDto { ServiceContractId = contract.Id, Description = "Late", LineValue = 1m }));

            (await _contracts.InvoiceDueAsync(contract.Id, new InvoiceServiceContractsInput { AsOfDate = new DateTime(2026, 3, 15) })).InvoicedPeriods.ShouldBe(3);
            (await _contracts.InvoiceDueAsync(contract.Id, new InvoiceServiceContractsInput { AsOfDate = new DateTime(2026, 3, 31) })).InvoicedPeriods.ShouldBe(0);

            contract = await _contracts.GetAsync(contract.Id);
            contract.NextInvoiceDate.ShouldBe(new DateTime(2026, 4, 1));
            contract.LastInvoiceDate.ShouldBe(new DateTime(2026, 3, 1));
            var ledger = await GetRequiredService<IRepository<ServiceLedgerEntry, Guid>>().GetListAsync(e => e.ServiceContractNo == "SC-S3");
            ledger.Count.ShouldBe(3);
            ledger.ShouldAllBe(e => e.EntryType == ServiceLedgerEntryType.Contract && e.Amount == 150m);

            var documentNos = ledger.Select(e => e.DocumentNo).ToList();
            var entries = await _glEntries.GetListAsync(e => documentNos.Contains(e.DocumentNo));
            entries.Sum(e => e.Amount).ShouldBe(0m);
            Sum(entries, "4310").ShouldBe(-450m);
        });
    }

    [Fact]
    public async Task A_Serial_Number_Belongs_To_One_Service_Item_Of_An_Item()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            await StockedPartAsync("S4-PART", 20m);
            await _serviceItems.CreateAsync(new CreateUpdateServiceItemDto { No = "S4-A", CustomerNo = "C00010", ItemNo = "S4-PART", SerialNo = "SN-S4" });

            var duplicate = await Should.ThrowAsync<BusinessException>(() =>
                _serviceItems.CreateAsync(new CreateUpdateServiceItemDto { No = "S4-B", CustomerNo = "C00010", ItemNo = "S4-PART", SerialNo = "SN-S4" }));
            duplicate.Code.ShouldBe(ErpErrorCodes.Service.SerialNoInUse);

            // The description comes from the item when none is given.
            (await _serviceItems.CreateAsync(new CreateUpdateServiceItemDto { No = "S4-C", CustomerNo = "C00010", ItemNo = "S4-PART", SerialNo = "SN-S4-2" })).Description.ShouldBe("S4-PART");
        });
    }

    private async Task StockedPartAsync(string no, decimal unitCost)
    {
        await GetRequiredService<IItemAppService>().CreateAsync(new CreateUpdateItemDto
        {
            No = no,
            Description = no,
            Type = ItemType.Inventory,
            BaseUnitOfMeasureCode = "PCS",
            GenProdPostingGroup = "RETAIL",
            InventoryPostingGroup = "RETAIL",
            VatProdPostingGroup = "STANDARD",
            CostingMethod = CostingMethod.Fifo,
            UnitCost = unitCost,
        });
        var item = await _itemRepository.FirstAsync(i => i.No == no);
        await GetRequiredService<ItemJnlPostLine>().PostAsync(new ItemPostingRequest
        {
            ItemId = item.Id, ItemNo = item.No, PostingDate = new DateTime(2026, 3, 1), EntryType = ItemLedgerEntryType.PositiveAdjmt, DocumentNo = no + "-STOCK", Quantity = 10m, UnitCost = unitCost, LocationCode = "MAIN",
        });
    }

    private async Task ResourceAsync(string no) =>
        await GetRequiredService<IResourceAppService>().CreateAsync(new CreateUpdateResourceDto { No = no, Name = "Technician " + no, UnitCost = 30m, UnitPrice = 80m });

    private static decimal Sum(IEnumerable<GLEntry> entries, string accountNo) =>
        entries.Where(e => e.GLAccountNo == accountNo).Sum(e => e.Amount);
}
