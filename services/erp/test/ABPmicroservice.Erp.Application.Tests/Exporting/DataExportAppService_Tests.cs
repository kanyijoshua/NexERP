using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shouldly;
using Volo.Abp;
using Xunit;

namespace ABPmicroservice.Erp.Exporting;

public class DataExportAppService_Tests : ErpApplicationTestBase
{
    private readonly IDataExportAppService _export;

    public DataExportAppService_Tests()
    {
        _export = GetRequiredService<IDataExportAppService>();
    }

    [Fact]
    public async Task Lists_The_Tables_That_Can_Be_Exported()
    {
        var entities = await InCompanyAsync(DefaultCompanyName, () => _export.GetEntitiesAsync());

        entities.Items.ShouldContain(e => e.Name == "Customer");
        entities.Items.ShouldContain(e => e.Name == "GLEntry");
        entities.Items.ShouldAllBe(e => e.FieldCount > 0);
    }

    [Fact]
    public async Task Describes_The_Fields_Of_A_Table()
    {
        var fields = await InCompanyAsync(DefaultCompanyName, () => _export.GetFieldsAsync("Customer"));

        var no = fields.Items.Single(f => f.Name == "No");
        no.DisplayName.ShouldBe("No");
        no.DataType.ShouldBe("string");
        no.IncludedByDefault.ShouldBeTrue();

        fields.Items.Single(f => f.Name == "Balance").DataType.ShouldBe("number");
        fields.Items.Single(f => f.Name == "Blocked").DataType.ShouldBe("boolean");
    }

    /// <summary>An enum field offers its values, so a filter can be a list rather than free text.</summary>
    [Fact]
    public async Task An_Enum_Field_Offers_Its_Values()
    {
        var fields = await InCompanyAsync(DefaultCompanyName, () => _export.GetFieldsAsync("Item"));

        var type = fields.Items.Single(f => f.Name == "Type");
        type.DataType.ShouldBe("enum");
        type.EnumValues.ShouldContain("Inventory");
    }

    [Fact]
    public async Task Previews_The_Rows_An_Export_Would_Contain()
    {
        var preview = await InCompanyAsync(
            DefaultCompanyName,
            () =>
                _export.GetPreviewAsync(
                    new DataPreviewInput
                    {
                        EntityName = "Customer",
                        Fields = ["No", "Name"],
                        OrderBy = "No",
                    }
                )
        );

        preview.TotalCount.ShouldBeGreaterThan(0);
        Row(preview.Items[0]).Keys.ShouldBe(new[] { "No", "Name" });
        Row(preview.Items[0])["No"].ShouldBe("C00010");
    }

    [Fact]
    public async Task Filters_Narrow_The_Rows()
    {
        var preview = await InCompanyAsync(
            DefaultCompanyName,
            () =>
                _export.GetPreviewAsync(
                    new DataPreviewInput
                    {
                        EntityName = "Customer",
                        Fields = ["No"],
                        Filters =
                        [
                            new EntityFilterDto
                            {
                                Field = "Name",
                                Operator = EntityFilterOperator.Contains,
                                Value = "adatum",
                            },
                        ],
                    }
                )
        );

        preview.TotalCount.ShouldBe(1);
    }

    /// <summary>
    /// The filter value is parsed into the field's own type. Anything that will not parse is
    /// refused rather than passed on to the database.
    /// </summary>
    [Fact]
    public async Task A_Filter_Value_Of_The_Wrong_Type_Is_Refused()
    {
        var exception = await Should.ThrowAsync<BusinessException>(
            () =>
                InCompanyAsync(
                    DefaultCompanyName,
                    () =>
                        _export.GetPreviewAsync(
                            new DataPreviewInput
                            {
                                EntityName = "Customer",
                                Filters =
                                [
                                    new EntityFilterDto
                                    {
                                        Field = "Balance",
                                        Operator = EntityFilterOperator.GreaterThan,
                                        Value = "not-a-number",
                                    },
                                ],
                            }
                        )
                )
        );

        exception.Code.ShouldBe(ErpErrorCodes.Exporting.FilterValueNotValid);
    }

    [Fact]
    public async Task An_Unknown_Field_Is_Refused()
    {
        var exception = await Should.ThrowAsync<BusinessException>(
            () =>
                InCompanyAsync(
                    DefaultCompanyName,
                    () => _export.GetPreviewAsync(new DataPreviewInput { EntityName = "Customer", Fields = ["Salary"] })
                )
        );

        exception.Code.ShouldBe(ErpErrorCodes.Exporting.UnknownField);
    }

    [Fact]
    public async Task An_Unknown_Table_Is_Refused()
    {
        var exception = await Should.ThrowAsync<BusinessException>(
            () =>
                InCompanyAsync(
                    DefaultCompanyName,
                    () => _export.GetPreviewAsync(new DataPreviewInput { EntityName = "SecretTable" })
                )
        );

        exception.Code.ShouldBe(ErpErrorCodes.Exporting.UnknownEntity);
    }

    [Fact]
    public async Task Exports_A_Table_As_Csv()
    {
        var content = await InCompanyAsync(
            DefaultCompanyName,
            async () =>
            {
                using var stream = await _export.RunExportAsync(
                    new DataExportInput
                    {
                        EntityName = "Customer",
                        Fields = ["No", "Name"],
                        Format = ExportFormat.Csv,
                    }
                );

                using var reader = new StreamReader(stream.GetStream(), Encoding.UTF8);
                return await reader.ReadToEndAsync();
            }
        );

        content.ShouldContain("No,Name");
        content.ShouldContain("C00010,Adatum Corporation");
    }

    [Fact]
    public async Task Exports_A_Table_As_Xlsx()
    {
        var bytes = await InCompanyAsync(
            DefaultCompanyName,
            async () =>
            {
                using var stream = await _export.RunExportAsync(
                    new DataExportInput
                    {
                        EntityName = "Customer",
                        Fields = ["No", "Name"],
                        Format = ExportFormat.Xlsx,
                    }
                );

                using var buffer = new MemoryStream();
                await stream.GetStream().CopyToAsync(buffer);
                return buffer.ToArray();
            }
        );

        // A real workbook, not an empty file.
        using var archive = new ZipArchive(new MemoryStream(bytes));
        archive.Entries.Select(e => e.FullName).ShouldContain("xl/worksheets/sheet1.xml");
    }

    /// <summary>
    /// An export template is a saved column set, so the same export can be repeated without
    /// picking the fields again.
    /// </summary>
    [Fact]
    public async Task Saves_And_Reads_Back_An_Export_Template()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            var created = await _export.CreateTemplateAsync(
                new CreateUpdateExportTemplateDto
                {
                    Name = "Customer contacts",
                    EntityName = "Customer",
                    Fields = ["No", "Name", "Email"],
                    Format = ExportFormat.Csv,
                    IsShared = true,
                }
            );

            created.Fields.ShouldBe(new[] { "No", "Name", "Email" });

            var templates = await _export.GetTemplatesAsync(new GetExportTemplatesInput { EntityName = "Customer" });
            templates.Items.ShouldContain(t => t.Id == created.Id);
        });
    }

    [Fact]
    public async Task A_Template_Cannot_Name_A_Field_That_Does_Not_Exist()
    {
        var exception = await Should.ThrowAsync<BusinessException>(
            () =>
                InCompanyAsync(
                    DefaultCompanyName,
                    () =>
                        _export.CreateTemplateAsync(
                            new CreateUpdateExportTemplateDto
                            {
                                Name = "Broken",
                                EntityName = "Customer",
                                Fields = ["No", "Salary"],
                            }
                        )
                )
        );

        exception.Code.ShouldBe(ErpErrorCodes.Exporting.UnknownField);
    }

    /// <summary>An export reads only the active company, like every other query.</summary>
    /// <summary>A row travels as a plain field-name to value map, so it reads like the file does.</summary>
    private static IReadOnlyDictionary<string, object> Row(object item)
    {
        return (IReadOnlyDictionary<string, object>)item;
    }

    [Fact]
    public async Task An_Export_Sees_Only_The_Active_Company()
    {
        var other = await InCompanyAsync(
            SecondCompanyName,
            () => _export.GetPreviewAsync(new DataPreviewInput { EntityName = "Customer" })
        );

        other.TotalCount.ShouldBe(0);
    }
}
