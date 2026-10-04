using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Finance;
using ABPmicroservice.Erp.Purchasing;
using ABPmicroservice.Erp.Sales;
using Shouldly;
using Xunit;

namespace ABPmicroservice.Erp.Inventory;

/// <summary>
/// The lookups of the front end search as the user types, in any case.
/// </summary>
public class MasterDataSearch_Tests : ErpApplicationTestBase
{
    private readonly ICustomerAppService _customers;
    private readonly IVendorAppService _vendors;
    private readonly IItemAppService _items;
    private readonly IGLAccountAppService _glAccounts;
    private readonly IUnitOfMeasureAppService _units;
    private readonly IItemCategoryAppService _categories;

    public MasterDataSearch_Tests()
    {
        _customers = GetRequiredService<ICustomerAppService>();
        _vendors = GetRequiredService<IVendorAppService>();
        _items = GetRequiredService<IItemAppService>();
        _glAccounts = GetRequiredService<IGLAccountAppService>();
        _units = GetRequiredService<IUnitOfMeasureAppService>();
        _categories = GetRequiredService<IItemCategoryAppService>();
    }

    [Fact]
    public async Task Customers_Vendors_Items_And_Accounts_Are_Found_By_No_Or_Name_In_Any_Case()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            await _customers.CreateAsync(new CreateUpdateCustomerDto { No = "LK-C1", Name = "Lookup Trading Ltd" });
            await _vendors.CreateAsync(new CreateUpdateVendorDto { No = "LK-V1", Name = "Lookup Supplies" });
            await _units.CreateAsync(new CreateUpdateUnitOfMeasureDto { Code = "LKBOX", Description = "Lookup box" });
            await _items.CreateAsync(
                new CreateUpdateItemDto
                {
                    No = "LK-I1",
                    Description = "Lookup Widget",
                    Type = ItemType.Inventory,
                    BaseUnitOfMeasureCode = "LKBOX",
                }
            );
            await _glAccounts.CreateAsync(
                new CreateUpdateGLAccountDto
                {
                    No = "LK-9999",
                    Name = "Lookup Clearing",
                    AccountType = GLAccountType.Posting,
                    AccountCategory = GLAccountCategory.Assets,
                    IncomeBalance = IncomeBalanceType.BalanceSheet,
                    DirectPosting = true,
                }
            );

            (await _customers.GetListAsync(new GetCustomerListInput { Filter = "TRADING" }))
                .Items.Select(c => c.No).ShouldBe(new[] { "LK-C1" });
            (await _customers.GetListAsync(new GetCustomerListInput { Filter = "lk-c" }))
                .Items.Select(c => c.No).ShouldBe(new[] { "LK-C1" });
            (await _vendors.GetListAsync(new GetVendorListInput { Filter = " lookup supplies " }))
                .Items.Select(v => v.No).ShouldBe(new[] { "LK-V1" });
            (await _items.GetListAsync(new GetItemListInput { Filter = "widget" }))
                .Items.Select(i => i.No).ShouldBe(new[] { "LK-I1" });
            (await _glAccounts.GetListAsync(new GetGLAccountListInput { Filter = "clearing" }))
                .Items.Select(a => a.No).ShouldBe(new[] { "LK-9999" });
        });
    }

    [Fact]
    public async Task Units_Of_Measure_And_Item_Categories_Can_Be_Searched_By_Code_Or_Description()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            await _units.CreateAsync(new CreateUpdateUnitOfMeasureDto { Code = "LKPAL", Description = "Euro pallet" });
            await _units.CreateAsync(new CreateUpdateUnitOfMeasureDto { Code = "LKDRM" });
            var parent = await _categories.CreateAsync(
                new CreateUpdateItemCategoryDto { Code = "LK-FURN", Description = "Lookup furniture" }
            );
            await _categories.CreateAsync(
                new CreateUpdateItemCategoryDto { Code = "LK-CHAIR", Description = "Chairs", ParentCategoryId = parent.Id }
            );

            (await _units.GetListAsync(new GetUnitOfMeasureListInput { Filter = "PALLET" }))
                .Items.Select(u => u.Code).ShouldBe(new[] { "LKPAL" });
            // A unit without a description is still found by its code.
            (await _units.GetListAsync(new GetUnitOfMeasureListInput { Filter = "lkd" }))
                .Items.Select(u => u.Code).ShouldBe(new[] { "LKDRM" });
            (await _units.GetListAsync(new GetUnitOfMeasureListInput { Filter = "LK" })).TotalCount.ShouldBe(2);

            (await _categories.GetListAsync(new GetItemCategoryListInput { Filter = "chairs" }))
                .Items.Single().ParentCategoryCode.ShouldBe("LK-FURN");
            (await _categories.GetListAsync(new GetItemCategoryListInput { Filter = "lk-" })).TotalCount.ShouldBe(2);
        });
    }
}
