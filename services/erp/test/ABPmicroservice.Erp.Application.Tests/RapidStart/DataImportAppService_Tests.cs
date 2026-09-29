using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Exporting;
using ABPmicroservice.Erp.Inventory;
using ABPmicroservice.Erp.Sales;
using Shouldly;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Xunit;

namespace ABPmicroservice.Erp.RapidStart;

/// <summary>The import wizard, which follows Odoo's: match columns, test, then all or nothing.</summary>
public class DataImportAppService_Tests : ErpApplicationTestBase
{
    private const string CustomersCsv = "No,Name,Post Code,Customer Posting Group,Shoe Size\r\nC97000,Fabrikam,1000,DOMESTIC,44\r\nC97001,Contoso,2000,DOMESTIC,41\r\n";

    private readonly IDataImportAppService _import;

    public DataImportAppService_Tests()
    {
        _import = GetRequiredService<IDataImportAppService>();
    }

    /// <summary>A header may be the field's name or its label; one that matches nothing is left for the user.</summary>
    [Fact]
    public async Task Proposes_A_Field_For_Each_Column_It_Recognises()
    {
        var preview = await InCompanyAsync(DefaultCompanyName, () => _import.ParseFileAsync(File("customers.csv", CustomersCsv, "Customer")));

        preview.Columns.Select(c => c.FieldName).ShouldBe(["No", "Name", "PostCode", "CustomerPostingGroup", null]);
        preview.TotalRows.ShouldBe(2);
        preview.SampleRows[0][1].ShouldBe("Fabrikam");
    }

    [Fact]
    public async Task A_Test_Run_Checks_Everything_And_Writes_Nothing()
    {
        var input = await RunAsync(CustomersCsv);
        var result = await InCompanyAsync(DefaultCompanyName, () => _import.TestImportAsync(input));

        result.DryRun.ShouldBeTrue();
        result.Succeeded.ShouldBeTrue();
        result.Inserted.ShouldBe(2);
        (await CustomersAsync()).ShouldNotContain(c => c.No == "C97000");
    }

    [Fact]
    public async Task Imports_Every_Row_Of_A_Clean_File()
    {
        var input = await RunAsync(CustomersCsv);
        var result = await InCompanyAsync(DefaultCompanyName, () => _import.RunImportAsync(input));

        result.DryRun.ShouldBeFalse();
        result.Succeeded.ShouldBeTrue();
        result.Inserted.ShouldBe(2);

        var customers = await CustomersAsync();
        customers.Single(c => c.No == "C97001").PostCode.ShouldBe("2000");
    }

    /// <summary>One bad row stops the whole file, and the error names the row as the spreadsheet numbers it.</summary>
    [Fact]
    public async Task One_Bad_Row_Imports_Nothing()
    {
        var csv = "No,Name,Customer Posting Group\r\nC97100,Good,DOMESTIC\r\nC97101,Bad,UNKNOWN\r\n";

        var input = await RunAsync(csv);
        var result = await InCompanyAsync(DefaultCompanyName, () => _import.RunImportAsync(input));

        result.Succeeded.ShouldBeFalse();
        result.DryRun.ShouldBeTrue();
        var error = result.Errors.ShouldHaveSingleItem();
        error.RowNo.ShouldBe(3);
        error.FieldName.ShouldBe("CustomerPostingGroup");

        (await CustomersAsync()).ShouldNotContain(c => c.No == "C97100");
    }

    [Fact]
    public async Task The_Key_Must_Be_Mapped()
    {
        var input = await RunAsync("Name\r\nNobody\r\n");

        var exception = await Should.ThrowAsync<BusinessException>(() => InCompanyAsync(DefaultCompanyName, () => _import.RunImportAsync(input)));
        exception.Code.ShouldBe(ErpErrorCodes.RapidStart.KeyFieldNotMapped);
    }

    [Fact]
    public async Task A_Field_Cannot_Take_Two_Columns()
    {
        var input = await RunAsync("No,Name,Name\r\nC97200,A,B\r\n");
        input.Columns = [new() { Index = 0, FieldName = "No" }, new() { Index = 1, FieldName = "Name" }, new() { Index = 2, FieldName = "name" }];

        var exception = await Should.ThrowAsync<BusinessException>(() => InCompanyAsync(DefaultCompanyName, () => _import.RunImportAsync(input)));
        exception.Code.ShouldBe(ErpErrorCodes.RapidStart.ColumnMappedTwice);
    }

    /// <summary>Semicolons, as Excel writes CSV in a comma-decimal locale, and a reference given by code.</summary>
    [Fact]
    public async Task Reads_A_Semicolon_File_And_Resolves_References_By_Code()
    {
        var categories = await RunAsync("Code;Description\r\nTOOLS;Tools\r\n", "ItemCategory");
        await InCompanyAsync(DefaultCompanyName, () => _import.RunImportAsync(categories));

        var items = await RunAsync("No;Description;Item Category Code;Unit Price\r\nI97000;Hammer;TOOLS;12.5\r\n", "Item");
        var result = await InCompanyAsync(DefaultCompanyName, () => _import.RunImportAsync(items));

        result.Succeeded.ShouldBeTrue();
        var item = await InCompanyAsync(
            DefaultCompanyName,
            async () => (await GetRequiredService<IRepository<Item, Guid>>().GetListAsync()).Single(i => i.No == "I97000")
        );
        item.ItemCategoryId.ShouldNotBeNull();
        item.UnitPrice.ShouldBe(12.5m);
    }

    [Fact]
    public async Task Reads_An_Excel_File_Too()
    {
        var workbook = SpreadsheetWriter.Write("Units", ["Code", "Description"], [["BOX", "Box"]]);
        var input = new RunImportInput
        {
            FileName = "units.xlsx",
            ContentBase64 = Convert.ToBase64String(workbook),
            EntityName = "UnitOfMeasure",
            Columns = [new() { Index = 0, FieldName = "Code" }, new() { Index = 1, FieldName = "Description" }],
        };

        var result = await InCompanyAsync(DefaultCompanyName, () => _import.RunImportAsync(input));

        result.Succeeded.ShouldBeTrue();
        result.Inserted.ShouldBe(1);
    }

    [Fact]
    public async Task Offers_An_Empty_Workbook_With_A_Column_For_Each_Field()
    {
        var file = await InCompanyAsync(DefaultCompanyName, () => _import.GetTemplateFileAsync("Customer"));

        using var buffer = new MemoryStream();
        await file.GetStream().CopyToAsync(buffer);
        var sheet = SpreadsheetReader.Read(buffer.ToArray()).Single();

        sheet.Rows.ShouldHaveSingleItem().ShouldContain("No");
        sheet.Rows[0].ShouldNotContain("Balance");
    }

    [Fact]
    public async Task Lists_Only_Tables_The_Caller_Can_Write_To()
    {
        var entities = await InCompanyAsync(DefaultCompanyName, () => _import.GetEntitiesAsync());

        entities.Items.ShouldContain(e => e.EntityName == "Customer");
        entities.Items.ShouldAllBe(e => e.CanWrite);
    }

    private async Task<RunImportInput> RunAsync(string csv, string entityName = "Customer")
    {
        var input = File("import.csv", csv, entityName);
        var preview = await InCompanyAsync(DefaultCompanyName, () => _import.ParseFileAsync(input));

        return new RunImportInput
        {
            FileName = input.FileName,
            ContentBase64 = input.ContentBase64,
            EntityName = entityName,
            HasHeaders = true,
            Columns = preview.Columns,
        };
    }

    private static ImportFileInput File(string fileName, string text, string entityName)
    {
        return new ImportFileInput
        {
            FileName = fileName,
            ContentBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(text)),
            EntityName = entityName,
        };
    }

    private Task<System.Collections.Generic.List<Customer>> CustomersAsync()
    {
        return InCompanyAsync(DefaultCompanyName, () => GetRequiredService<IRepository<Customer, Guid>>().GetListAsync());
    }
}
