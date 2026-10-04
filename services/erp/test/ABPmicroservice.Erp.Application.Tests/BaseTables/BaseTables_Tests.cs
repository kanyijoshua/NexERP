using System;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Companies;
using ABPmicroservice.Erp.Finance;
using ABPmicroservice.Erp.FixedAssets;
using ABPmicroservice.Erp.HumanResources;
using ABPmicroservice.Erp.Purchasing;
using ABPmicroservice.Erp.Sales;
using Shouldly;
using Volo.Abp;
using Xunit;

namespace ABPmicroservice.Erp.BaseTables;

/// <summary>The base tables: code tables, keyed tables with their relations, and the new card fields.</summary>
public class BaseTables_Tests : ErpApplicationTestBase
{
    [Fact]
    public async Task A_Code_Table_Stores_Its_Code_Upper_Case_And_Checks_Its_Relations()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            var subclasses = GetRequiredService<IFASubclassAppService>();

            var subclass = await subclasses.CreateAsync(new CreateUpdateFASubclassDto
            {
                Code = "vehicles",
                Description = "Motor vehicles",
                FAClassCode = "tangible",
            });

            subclass.Code.ShouldBe("VEHICLES");
            subclass.FAClassCode.ShouldBe("TANGIBLE");

            var unknown = await Should.ThrowAsync<BusinessException>(
                () => subclasses.CreateAsync(new CreateUpdateFASubclassDto { Code = "X", FAClassCode = "NOPE" })
            );
            unknown.Code.ShouldBe(ErpErrorCodes.PostingSetup.CodeNotFound);

            (await subclasses.GetListAsync(new GetCodeTableListInput { Filter = "motor" })).TotalCount.ShouldBe(1);
        });
    }

    [Fact]
    public async Task Lines_Of_An_Employee_Are_Numbered_In_Steps_Of_Ten_Thousand()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            var employees = GetRequiredService<IEmployeeAppService>();
            var relatives = GetRequiredService<IEmployeeRelativeAppService>();

            var employee = await employees.CreateAsync(new CreateUpdateEmployeeDto { FirstName = "Mary", LastName = "Achieng" });

            var first = await relatives.CreateAsync(new CreateUpdateEmployeeRelativeDto { EmployeeNo = employee.No, RelativeCode = "spouse", FirstName = "John" });
            var second = await relatives.CreateAsync(new CreateUpdateEmployeeRelativeDto { EmployeeNo = employee.No, RelativeCode = "CHILD", FirstName = "Amani" });

            first.LineNo.ShouldBe(10000);
            first.RelativeCode.ShouldBe("SPOUSE");
            second.LineNo.ShouldBe(20000);

            var list = await relatives.GetListAsync(new GetEmployeeRelativeListInput { EmployeeNo = employee.No });
            list.TotalCount.ShouldBe(2);

            var stranger = await Should.ThrowAsync<BusinessException>(
                () => relatives.CreateAsync(new CreateUpdateEmployeeRelativeDto { EmployeeNo = "NOBODY", FirstName = "X" })
            );
            stranger.Code.ShouldBe(ErpErrorCodes.PostingSetup.CodeNotFound);

            var duplicate = await Should.ThrowAsync<BusinessException>(
                () => relatives.CreateAsync(new CreateUpdateEmployeeRelativeDto { EmployeeNo = employee.No, LineNo = 10000, FirstName = "Again" })
            );
            duplicate.Code.ShouldBe(ErpErrorCodes.BaseTables.RecordAlreadyExists);
        });
    }

    [Fact]
    public async Task A_Fixed_Asset_Takes_Its_Number_From_The_FA_Setup_Series()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            var assets = GetRequiredService<IFixedAssetAppService>();
            var books = GetRequiredService<IFADepreciationBookAppService>();

            (await GetRequiredService<IFASetupAppService>().GetAsync()).FixedAssetNos.ShouldBe("FA");

            var asset = await assets.CreateAsync(new CreateUpdateFixedAssetDto
            {
                Description = "Delivery van",
                FAClassCode = "TANGIBLE",
                SerialNo = "KDA 123A",
            });

            asset.No.ShouldBe("FA000010");

            var book = await books.CreateAsync(new CreateUpdateFADepreciationBookDto
            {
                FANo = asset.No,
                DepreciationBookCode = "company",
                DepreciationMethod = FADepreciationMethod.StraightLine,
                DepreciationStartingDate = new DateTime(2026, 1, 1),
                NoOfDepreciationYears = 4,
            });

            book.DepreciationBookCode.ShouldBe("COMPANY");

            var noAsset = await Should.ThrowAsync<BusinessException>(
                () => books.CreateAsync(new CreateUpdateFADepreciationBookDto { FANo = "FA-NONE", DepreciationBookCode = "COMPANY" })
            );
            noAsset.Code.ShouldBe(ErpErrorCodes.PostingSetup.CodeNotFound);
        });
    }

    [Fact]
    public async Task A_Budget_Entry_Needs_A_Budget_And_An_Account_That_Exist()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            var budgets = GetRequiredService<IGLBudgetNameAppService>();
            var entries = GetRequiredService<IGLBudgetEntryAppService>();
            var account = (await GetRequiredService<IGLAccountAppService>().GetListAsync(new GetGLAccountListInput { MaxResultCount = 1 })).Items[0];

            await budgets.CreateAsync(new CreateUpdateGLBudgetNameDto { Name = "2026", Description = "Budget 2026" });

            var first = await entries.CreateAsync(new CreateUpdateGLBudgetEntryDto
            {
                BudgetName = "2026",
                GLAccountNo = account.No,
                Date = new DateTime(2026, 1, 1),
                Amount = 1200m,
            });
            var second = await entries.CreateAsync(new CreateUpdateGLBudgetEntryDto
            {
                BudgetName = "2026",
                GLAccountNo = account.No,
                Date = new DateTime(2026, 2, 1),
                Amount = 800m,
            });

            first.EntryNo.ShouldBe(1);
            second.EntryNo.ShouldBe(2);

            var noBudget = await Should.ThrowAsync<BusinessException>(
                () => entries.CreateAsync(new CreateUpdateGLBudgetEntryDto { BudgetName = "1999", GLAccountNo = account.No, Date = new DateTime(2026, 1, 1) })
            );
            noBudget.Code.ShouldBe(ErpErrorCodes.PostingSetup.CodeNotFound);
        });
    }

    [Fact]
    public async Task The_Card_Fields_Of_A_Customer_And_A_Vendor_Round_Trip()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            var customers = GetRequiredService<ICustomerAppService>();
            var vendors = GetRequiredService<IVendorAppService>();

            var customer = await customers.CreateAsync(new CreateUpdateCustomerDto
            {
                Name = "Savannah Traders",
                SearchName = "savannah",
                Address2 = "2nd Floor",
                County = "Nairobi",
                VatRegistrationNo = "p051234567x",
                ApplicationMethod = ApplicationMethod.ApplyToOldest,
                PrepaymentPct = 25m,
                PrintStatements = true,
            });

            customer.SearchName.ShouldBe("SAVANNAH");
            customer.VatRegistrationNo.ShouldBe("p051234567x");
            customer.ApplicationMethod.ShouldBe(ApplicationMethod.ApplyToOldest);
            customer.PrepaymentPct.ShouldBe(25m);

            var reloaded = await customers.GetAsync(customer.Id);
            reloaded.County.ShouldBe("Nairobi");
            reloaded.PrintStatements.ShouldBeTrue();

            var vendor = await vendors.CreateAsync(new CreateUpdateVendorDto { Name = "Rift Supplies", County = "Nakuru", Priority = 3, ResponsibilityCenter = "hq" });
            vendor.County.ShouldBe("Nakuru");
            vendor.Priority.ShouldBe(3);
            vendor.ResponsibilityCenter.ShouldBe("HQ");
        });
    }
}
