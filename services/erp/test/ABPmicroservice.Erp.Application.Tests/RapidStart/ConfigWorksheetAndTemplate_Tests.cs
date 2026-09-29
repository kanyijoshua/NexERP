using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Exporting;
using ABPmicroservice.Erp.Permissions;
using Shouldly;
using Volo.Abp;
using Xunit;

namespace ABPmicroservice.Erp.RapidStart;

public class ConfigWorksheetAndTemplate_Tests : ErpApplicationTestBase
{
    private readonly IConfigWorksheetAppService _worksheet;
    private readonly IConfigTemplateAppService _templates;

    public ConfigWorksheetAndTemplate_Tests()
    {
        _worksheet = GetRequiredService<IConfigWorksheetAppService>();
        _templates = GetRequiredService<IConfigTemplateAppService>();
    }

    [Fact]
    public async Task Suggesting_Lines_Lists_Each_Area_With_Its_Tables_Once()
    {
        var lines = await InCompanyAsync(DefaultCompanyName, () => _worksheet.SuggestLinesAsync());

        lines.Items.First().LineType.ShouldBe(ConfigLineType.Area);
        lines.Items.ShouldContain(l => l.LineType == ConfigLineType.Area && l.Name == "Sales");

        var customer = lines.Items.Single(l => l.EntityName == "Customer");
        customer.NoOfRecords.ShouldNotBeNull();
        customer.NoOfRecords.Value.ShouldBeGreaterThan(0);

        // Customer sits under the Sales heading.
        var sales = lines.Items.ToList().FindIndex(l => l.Name == "Sales" && l.LineType == ConfigLineType.Area);
        var nextArea = lines.Items.ToList().FindIndex(sales + 1, l => l.LineType == ConfigLineType.Area);
        var index = lines.Items.ToList().FindIndex(l => l.EntityName == "Customer");
        index.ShouldBeGreaterThan(sales);
        (nextArea < 0 || index < nextArea).ShouldBeTrue();

        var again = await InCompanyAsync(DefaultCompanyName, () => _worksheet.SuggestLinesAsync());
        again.Items.Count.ShouldBe(lines.Items.Count);
    }

    [Fact]
    public async Task A_Line_Can_Be_Updated_And_Moved()
    {
        var lines = await InCompanyAsync(DefaultCompanyName, () => _worksheet.SuggestLinesAsync());
        var second = lines.Items[1];

        await InCompanyAsync(
            DefaultCompanyName,
            () =>
                _worksheet.UpdateAsync(
                    second.Id,
                    new UpdateConfigLineDto
                    {
                        Name = second.Name,
                        Status = ConfigLineStatus.Completed,
                        ResponsibleUserName = "admin",
                        PackageCode = "base",
                    }
                )
        );

        var moved = await InCompanyAsync(DefaultCompanyName, () => _worksheet.MoveAsync(second.Id, new MoveConfigLineInput { Up = true }));

        var first = moved.Items.First();
        first.Id.ShouldBe(second.Id);
        first.Status.ShouldBe(ConfigLineStatus.Completed);
        first.PackageCode.ShouldBe("BASE");
        moved.Items.Select(l => l.SortOrder).ShouldBeInOrder();
    }

    [Fact]
    public async Task A_Template_Gives_Defaults_But_Never_For_The_Key()
    {
        var template = await InCompanyAsync(
            DefaultCompanyName,
            () =>
                _templates.CreateAsync(
                    new CreateUpdateConfigTemplateDto
                    {
                        Code = "cust-dom",
                        EntityName = "Customer",
                        Description = "Domestic customers",
                        Lines = [new ConfigTemplateLineDto { FieldName = "customerPostingGroup", DefaultValue = "DOMESTIC" }],
                    }
                )
        );

        template.Code.ShouldBe("CUST-DOM");
        template.Lines.ShouldHaveSingleItem().FieldName.ShouldBe("CustomerPostingGroup");

        var list = await InCompanyAsync(DefaultCompanyName, () => _templates.GetListAsync(new GetConfigTemplatesInput { EntityName = "Customer" }));
        list.Items.ShouldContain(t => t.Id == template.Id);

        var exception = await Should.ThrowAsync<BusinessException>(
            () =>
                InCompanyAsync(
                    DefaultCompanyName,
                    () =>
                        _templates.UpdateAsync(
                            template.Id,
                            new CreateUpdateConfigTemplateDto
                            {
                                Code = template.Code,
                                EntityName = "Customer",
                                Lines = [new ConfigTemplateLineDto { FieldName = "No", DefaultValue = "C1" }],
                            }
                        )
                )
        );
        exception.Code.ShouldBe(ErpErrorCodes.RapidStart.FieldNotImportable);
    }

    /// <summary>A table a package can write to must need the screen's own create/change permissions.</summary>
    [Fact]
    public void Every_Importable_Table_Is_Guarded_By_Write_Permissions_That_Exist()
    {
        var defined = ErpPermissions.GetAll().ToHashSet();

        foreach (var profile in GetRequiredService<ConfigTableRegistry>().GetAll())
        {
            var permissions = ErpEntityPermissions.FindWrite(profile.Name);
            permissions.ShouldNotBeNull($"{profile.Name} can be imported but has no write permissions");
            permissions.ShouldAllBe(p => defined.Contains(p));
        }
    }
}
