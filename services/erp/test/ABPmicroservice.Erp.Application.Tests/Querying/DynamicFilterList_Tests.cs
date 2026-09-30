using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Companies;
using ABPmicroservice.Erp.Finance;
using ABPmicroservice.Erp.Sales;
using Shouldly;
using Volo.Abp;
using Xunit;

namespace ABPmicroservice.Erp.Querying;

/// <summary>The filter pane's conditions applied by the list services, through EF Core.</summary>
public class DynamicFilterList_Tests : ErpApplicationTestBase
{
    private readonly IGLAccountAppService _accounts;
    private readonly ICustomerAppService _customers;
    private readonly IPaymentTermsAppService _paymentTerms;

    public DynamicFilterList_Tests()
    {
        _accounts = GetRequiredService<IGLAccountAppService>();
        _customers = GetRequiredService<ICustomerAppService>();
        _paymentTerms = GetRequiredService<IPaymentTermsAppService>();
    }

    [Fact]
    public async Task A_Range_Expression_Filters_The_Chart_Of_Accounts()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            var result = await _accounts.GetListAsync(new GetGLAccountListInput
            {
                MaxResultCount = 100,
                Sorting = "no",
                DynamicFilter = """{"conditions":[{"field":"no","operator":"expression","value":"1000..1999"}]}""",
            });

            result.Items.Select(a => a.No).ShouldBe(["1010", "1020", "1200", "1400"]);
            result.TotalCount.ShouldBe(4);
        });
    }

    [Fact]
    public async Task Conditions_On_Enums_And_Text_Combine()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            // Expense accounts (category 5) or anything named like "revenue".
            var result = await _accounts.GetListAsync(new GetGLAccountListInput
            {
                MaxResultCount = 100,
                Sorting = "no",
                DynamicFilter = """
                    {"logic":"or","conditions":[
                      {"field":"accountCategory","operator":"equals","value":5},
                      {"field":"name","operator":"contains","value":"REVENUE"}]}
                    """,
            });

            result.Items.Select(a => a.No).ShouldBe(["4000", "6100", "8910"]);
        });
    }

    [Fact]
    public async Task Code_Tables_And_Master_Data_Take_The_Filter_Too()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            var terms = await _paymentTerms.GetListAsync(new GetCodeTableListInput
            {
                MaxResultCount = 100,
                DynamicFilter = """{"conditions":[{"field":"code","operator":"expression","value":"*D"}]}""",
            });
            terms.Items.Select(t => t.Code).OrderBy(c => c).ShouldBe(["14D", "30D", "COD"]);

            var customers = await _customers.GetListAsync(new GetCustomerListInput
            {
                MaxResultCount = 100,
                DynamicFilter = """{"conditions":[{"field":"blocked","operator":"isFalse"},{"field":"name","operator":"startsWith","value":"adatum"}]}""",
            });
            customers.Items.ShouldHaveSingleItem().No.ShouldBe("C00010");
        });
    }

    [Fact]
    public async Task A_Field_The_List_Cannot_Filter_Is_Refused()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            var ex = await Should.ThrowAsync<BusinessException>(() => _accounts.GetListAsync(new GetGLAccountListInput
            {
                DynamicFilter = """{"conditions":[{"field":"doesNotExist","operator":"equals","value":"1"}]}""",
            }));
            ex.Code.ShouldBe(ErpErrorCodes.Querying.FieldNotFilterable);
        });
    }
}
