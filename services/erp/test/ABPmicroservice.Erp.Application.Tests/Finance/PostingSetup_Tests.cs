using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Companies;
using ABPmicroservice.Erp.Documents;
using ABPmicroservice.Erp.Inventory;
using ABPmicroservice.Erp.Sales;
using Shouldly;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Xunit;

namespace ABPmicroservice.Erp.Finance;

/// <summary>
/// Posting groups, the setups that turn them into accounts, and the General Ledger Setup: codes
/// the cards point at must exist, and posting takes its accounts and allowed dates from them.
/// </summary>
public class PostingSetup_Tests : ErpApplicationTestBase
{
    private readonly IGenBusinessPostingGroupAppService _busGroups;
    private readonly IGenProductPostingGroupAppService _prodGroups;
    private readonly ICustomerPostingGroupAppService _customerGroups;
    private readonly IGeneralPostingSetupAppService _generalSetup;
    private readonly IInventoryPostingSetupAppService _inventorySetup;
    private readonly IGeneralLedgerSetupAppService _glSetup;
    private readonly ICustomerAppService _customers;
    private readonly IItemAppService _items;
    private readonly ISalesDocumentAppService _salesDocuments;
    private readonly IRepository<GLEntry, Guid> _glEntries;

    public PostingSetup_Tests()
    {
        _busGroups = GetRequiredService<IGenBusinessPostingGroupAppService>();
        _prodGroups = GetRequiredService<IGenProductPostingGroupAppService>();
        _customerGroups = GetRequiredService<ICustomerPostingGroupAppService>();
        _generalSetup = GetRequiredService<IGeneralPostingSetupAppService>();
        _inventorySetup = GetRequiredService<IInventoryPostingSetupAppService>();
        _glSetup = GetRequiredService<IGeneralLedgerSetupAppService>();
        _customers = GetRequiredService<ICustomerAppService>();
        _items = GetRequiredService<IItemAppService>();
        _salesDocuments = GetRequiredService<ISalesDocumentAppService>();
        _glEntries = GetRequiredService<IRepository<GLEntry, Guid>>();
    }

    [Fact]
    public async Task Posting_Group_Codes_Are_Upper_Case_And_Unique()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            var created = await _busGroups.CreateAsync(new CreateUpdatePostingGroupDto { Code = " export ", Description = "Export" });
            created.Code.ShouldBe("EXPORT");

            var duplicate = await Should.ThrowAsync<BusinessException>(
                () => _busGroups.CreateAsync(new CreateUpdatePostingGroupDto { Code = "Export" })
            );
            duplicate.Code.ShouldBe(ErpErrorCodes.PostingSetup.CodeAlreadyExists);

            var found = await _busGroups.GetListAsync(new GetCodeTableListInput { Filter = "expo", MaxResultCount = 10 });
            found.Items.ShouldHaveSingleItem().Code.ShouldBe("EXPORT");
        });
    }

    [Fact]
    public async Task A_Customer_Posting_Group_Needs_An_Existing_Receivables_Account()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            var ex = await Should.ThrowAsync<BusinessException>(
                () => _customerGroups.CreateAsync(new CreateUpdateCustomerPostingGroupDto { Code = "FOREIGN", ReceivablesAccountNo = "9999" })
            );
            ex.Code.ShouldBe(ErpErrorCodes.PostingSetup.GLAccountNotFound);

            (await _customerGroups.CreateAsync(new CreateUpdateCustomerPostingGroupDto { Code = "FOREIGN", ReceivablesAccountNo = "1200" }))
                .ReceivablesAccountNo.ShouldBe("1200");
        });
    }

    [Fact]
    public async Task Cards_Only_Take_Posting_Groups_That_Exist()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            var unknown = await Should.ThrowAsync<BusinessException>(
                () => _customers.CreateAsync(new CreateUpdateCustomerDto { No = "PG-C1", Name = "Posting", GenBusPostingGroup = "NOPE" })
            );
            unknown.Code.ShouldBe(ErpErrorCodes.PostingSetup.CodeNotFound);
            unknown.Data["code"].ShouldBe("NOPE");

            var customer = await _customers.CreateAsync(
                new CreateUpdateCustomerDto { No = "PG-C1", Name = "Posting", CustomerPostingGroup = "domestic", GenBusPostingGroup = "DOMESTIC" }
            );
            customer.CustomerPostingGroup.ShouldBe("DOMESTIC");

            var item = await Should.ThrowAsync<BusinessException>(
                () => _items.CreateAsync(
                    new CreateUpdateItemDto
                    {
                        No = "PG-I1",
                        Description = "Posting",
                        Type = ItemType.Inventory,
                        BaseUnitOfMeasureCode = "PCS",
                        InventoryPostingGroup = "NOPE",
                    }
                )
            );
            item.Code.ShouldBe(ErpErrorCodes.PostingSetup.CodeNotFound);
        });
    }

    [Fact]
    public async Task General_Posting_Setup_Checks_Its_Groups_Accounts_And_Key()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            var missingGroup = await Should.ThrowAsync<BusinessException>(
                () => _generalSetup.CreateAsync(new CreateUpdateGeneralPostingSetupDto { GenBusPostingGroup = "DOMESTIC", GenProdPostingGroup = "NOPE" })
            );
            missingGroup.Code.ShouldBe(ErpErrorCodes.PostingSetup.CodeNotFound);

            var duplicate = await Should.ThrowAsync<BusinessException>(
                () => _generalSetup.CreateAsync(new CreateUpdateGeneralPostingSetupDto { GenBusPostingGroup = "DOMESTIC", GenProdPostingGroup = "RETAIL" })
            );
            duplicate.Code.ShouldBe(ErpErrorCodes.PostingSetup.SetupAlreadyExists);

            // A blank business group is the row for parties without one.
            var blankBus = await _generalSetup.CreateAsync(
                new CreateUpdateGeneralPostingSetupDto { GenProdPostingGroup = "retail", SalesAccountNo = "4000" }
            );
            blankBus.GenBusPostingGroup.ShouldBeNull();
            blankBus.GenProdPostingGroup.ShouldBe("RETAIL");

            var badAccount = await Should.ThrowAsync<BusinessException>(
                () => _inventorySetup.CreateAsync(new CreateUpdateInventoryPostingSetupDto { InventoryPostingGroup = "RETAIL", InventoryAccountNo = "9999" })
            );
            badAccount.Code.ShouldBe(ErpErrorCodes.PostingSetup.GLAccountNotFound);
        });
    }

    [Fact]
    public async Task Sales_Posting_Credits_The_Sales_Account_Of_The_General_Posting_Setup()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            // Point DOMESTIC / RETAIL at another revenue account and post a sale of item 1000 (RETAIL).
            var setup = (await _generalSetup.GetListAsync(new GetCodeTableListInput { Filter = "RETAIL", MaxResultCount = 10 }))
                .Items.Single(s => s.GenBusPostingGroup == "DOMESTIC");
            await _generalSetup.UpdateAsync(
                setup.Id,
                new CreateUpdateGeneralPostingSetupDto
                {
                    GenBusPostingGroup = "DOMESTIC",
                    GenProdPostingGroup = "RETAIL",
                    SalesAccountNo = "1010",
                    PurchAccountNo = setup.PurchAccountNo,
                    COGSAccountNo = setup.COGSAccountNo,
                    InventoryAdjmtAccountNo = setup.InventoryAdjmtAccountNo,
                }
            );

            var invoice = await _salesDocuments.CreateAsync(await NewInvoiceAsync("PG-SI-1"));
            var posted = await _salesDocuments.RunPostingAsync(invoice.Id);

            var entries = await _glEntries.GetListAsync(e => e.DocumentNo == posted.PostedDocumentNo);
            entries.Where(e => e.GLAccountNo == "1010").Sum(e => e.Amount).ShouldBe(-600m);
            // The G/L account line posts to its own account.
            entries.Where(e => e.GLAccountNo == "4000").Sum(e => e.Amount).ShouldBe(-50m);
            entries.Sum(e => e.Amount).ShouldBe(0m);
        });
    }

    [Fact]
    public async Task A_Missing_General_Posting_Setup_Stops_The_Posting()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            await _prodGroups.CreateAsync(new CreateUpdatePostingGroupDto { Code = "NOSETUP" });
            var item = await _items.GetByNoAsync("1000");
            await _items.UpdateAsync(
                item.Id,
                new CreateUpdateItemDto
                {
                    No = item.No,
                    Description = item.Description,
                    Type = item.Type,
                    BaseUnitOfMeasureCode = item.BaseUnitOfMeasureCode,
                    UnitPrice = item.UnitPrice,
                    UnitCost = item.UnitCost,
                    GenProdPostingGroup = "NOSETUP",
                    InventoryPostingGroup = item.InventoryPostingGroup,
                }
            );

            var invoice = await _salesDocuments.CreateAsync(await NewInvoiceAsync("PG-SI-2"));
            var ex = await Should.ThrowAsync<BusinessException>(() => _salesDocuments.RunPostingAsync(invoice.Id));
            ex.Code.ShouldBe(ErpErrorCodes.PostingSetup.GeneralPostingSetupMissing);
            ex.Data["genProdPostingGroup"].ShouldBe("NOSETUP");
        });
    }

    [Fact]
    public async Task General_Ledger_Setup_Limits_The_Posting_Dates()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            var setup = await _glSetup.GetAsync();
            setup.AmountRoundingPrecision.ShouldBe(0.01m);
            setup.GlobalDimension1Code.ShouldBe("DEPARTMENT");

            setup.AllowPostingFrom = new DateTime(2026, 4, 1);
            setup.AllowPostingTo = new DateTime(2026, 4, 30);
            setup.LcyCode = "kes";
            var saved = await _glSetup.UpdateAsync(setup);
            saved.LcyCode.ShouldBe("KES");

            var invoice = await _salesDocuments.CreateAsync(await NewInvoiceAsync("PG-SI-3"));
            var ex = await Should.ThrowAsync<BusinessException>(() => _salesDocuments.RunPostingAsync(invoice.Id));
            ex.Code.ShouldBe(ErpErrorCodes.GeneralLedger.PostingDateNotAllowed);

            setup.AllowPostingTo = new DateTime(2026, 3, 1);
            (await Should.ThrowAsync<BusinessException>(() => _glSetup.UpdateAsync(setup))).Code
                .ShouldBe(ErpErrorCodes.GeneralLedger.InvalidPostingDateRange);

            setup.AllowPostingTo = null;
            setup.GlobalDimension2Code = "NOPE";
            (await Should.ThrowAsync<BusinessException>(() => _glSetup.UpdateAsync(setup))).Code
                .ShouldBe(ErpErrorCodes.GeneralLedger.DimensionNotFound);
        });
    }

    private async Task<CreateUpdateSalesHeaderDto> NewInvoiceAsync(string no)
    {
        var customer = await _customers.GetByNoAsync("C00010");

        return new CreateUpdateSalesHeaderDto
        {
            DocumentType = SalesDocumentType.Invoice,
            No = no,
            CustomerId = customer.Id,
            PostingDate = new DateTime(2026, 3, 1),
            Lines = new List<SalesLineInputDto>
            {
                new() { Type = DocumentLineType.Item, No = "1000", Description = "Bicycle Assembly", Quantity = 2, UnitPrice = 300m },
                new() { Type = DocumentLineType.GLAccount, No = "4000", Description = "Delivery charge", Quantity = 1, UnitPrice = 50m },
            },
        };
    }
}
