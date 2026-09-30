using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Exporting;
using ABPmicroservice.Erp.Sales;
using Shouldly;
using Volo.Abp;
using Volo.Abp.Content;
using Volo.Abp.Domain.Repositories;
using Xunit;

namespace ABPmicroservice.Erp.RapidStart;

public class ConfigPackageAppService_Tests : ErpApplicationTestBase
{
    private readonly IConfigPackageAppService _packages;

    public ConfigPackageAppService_Tests()
    {
        _packages = GetRequiredService<IConfigPackageAppService>();
    }

    [Fact]
    public async Task The_Catalog_Lists_Importable_Tables_By_Area()
    {
        var catalog = await InCompanyAsync(DefaultCompanyName, () => _packages.GetTableCatalogAsync());

        catalog.Items.ShouldContain(t => t.EntityName == "Customer" && t.Area == "Sales" && t.CanWrite);
        catalog.Items.ShouldNotContain(t => t.EntityName == "GLEntry");

        var customer = catalog.Items.Single(t => t.EntityName == "Customer");
        customer.KeyFields.ShouldBe(["No"]);
        customer.RelatedTables.ShouldContain("CustomerPostingGroup");
    }

    [Fact]
    public async Task Including_A_Table_With_Related_Tables_Orders_Them_By_Dependency()
    {
        var package = await CreatePackageAsync(DefaultCompanyName, "CUST", ["Customer"], includeRelated: true);

        var order = package.Tables.Select(t => t.EntityName).ToList();
        order.ShouldContain("GenBusinessPostingGroup");
        order.ShouldContain("VatBusinessPostingGroup");
        order.ShouldContain("PaymentTerms");
        order.IndexOf("GLAccount").ShouldBeLessThan(order.IndexOf("CustomerPostingGroup"));
        order.Last().ShouldBe("Customer");
        package.Tables.Select(t => t.ProcessingOrder).ShouldBeInOrder();

        var no = package.Tables.Single(t => t.EntityName == "Customer").Fields.Single(f => f.FieldName == "No");
        no.PrimaryKey.ShouldBeTrue();
        package.Tables.Single(t => t.EntityName == "Customer").Fields.ShouldNotContain(f => f.FieldName == "Balance");
    }

    [Fact]
    public async Task A_Package_Code_Is_Unique_In_A_Company()
    {
        await CreatePackageAsync(DefaultCompanyName, "DUP", ["UnitOfMeasure"]);

        var exception = await Should.ThrowAsync<BusinessException>(() => CreatePackageAsync(DefaultCompanyName, "dup", ["UnitOfMeasure"]));
        exception.Code.ShouldBe(ErpErrorCodes.RapidStart.PackageCodeAlreadyExists);
    }

    /// <summary>
    /// The central use of a package: take one company's setup and master data to another. The file
    /// carries codes, not ids, so it means the same thing on the other side.
    /// </summary>
    [Fact]
    public async Task A_Package_Carries_Data_From_One_Company_To_Another()
    {
        var package = await CreatePackageAsync(DefaultCompanyName, "COPY", ["Customer"], includeRelated: true);
        await InCompanyAsync(DefaultCompanyName, () => _packages.FillFromDatabaseAsync(package.Id, new ConfigPackageTablesInput()));

        var file = await InCompanyAsync(
            DefaultCompanyName,
            async () => await ReadAsync(await _packages.ExportPackageAsync(package.Id, new ExportConfigPackageInput { Format = ConfigPackageFileFormat.Json }))
        );

        using (var json = JsonDocument.Parse(file))
        {
            json.RootElement.GetProperty("code").GetString().ShouldBe("COPY");
            json.RootElement.GetProperty("tables").GetArrayLength().ShouldBe(package.Tables.Count);
        }

        var imported = await InCompanyAsync(SecondCompanyName, () => _packages.ImportPackageAsync(Upload("COPY.json", file)));
        imported.Code.ShouldBe("COPY");
        imported.Tables.Single(t => t.EntityName == "Customer").NoOfRecords.ShouldBeGreaterThan(0);

        var result = await InCompanyAsync(SecondCompanyName, () => _packages.ApplyPackageAsync(imported.Id, new ConfigPackageTablesInput()));

        result.Errors.ShouldBe(0);
        result.Tables.Single(t => t.EntityName == "Customer").Inserted.ShouldBeGreaterThan(0);

        var customers = await InCompanyAsync(SecondCompanyName, () => GetRequiredService<IRepository<Customer, Guid>>().GetListAsync());
        customers.ShouldContain(c => c.No == "C00010" && c.Name == "Adatum Corporation");
    }

    /// <summary>BC's round trip: export to Excel, change it there, import it back into the package.</summary>
    [Fact]
    public async Task An_Excel_Export_Can_Be_Imported_Back()
    {
        var package = await CreatePackageAsync(DefaultCompanyName, "XL", ["UnitOfMeasure", "Customer"]);

        var workbook = await InCompanyAsync(
            DefaultCompanyName,
            async () => await ReadAsync(await _packages.ExportPackageAsync(package.Id, new ExportConfigPackageInput { Format = ConfigPackageFileFormat.Xlsx }))
        );

        SpreadsheetReader.Read(workbook).Select(s => s.Name).ShouldBe(["UnitOfMeasure", "Customer"], ignoreOrder: true);

        var imported = await InCompanyAsync(DefaultCompanyName, () => _packages.ImportDataAsync(package.Id, Upload("XL.xlsx", workbook)));
        imported.NoOfRecords.ShouldBeGreaterThan(0);

        var validated = await InCompanyAsync(DefaultCompanyName, () => _packages.ValidatePackageAsync(package.Id, new ConfigPackageTablesInput()));
        validated.Errors.ShouldBe(0);
        validated.Inserted.ShouldBe(0);
        validated.Modified.ShouldBe(imported.NoOfRecords);
    }

    /// <summary>
    /// Validation marks the bad record and says why, next to the field; correcting the value
    /// clears it, and applying then writes the record.
    /// </summary>
    [Fact]
    public async Task Errors_Are_Shown_Per_Field_And_Cleared_By_Correcting_The_Record()
    {
        var package = await CreatePackageAsync(DefaultCompanyName, "FIX", ["Customer"]);
        var tableId = package.Tables.Single().Id;

        var workbook = SpreadsheetWriter.Write("Customer", ["No", "Name", "CustomerPostingGroup"], [["C95000", "Fixable", "NOPE"]]);
        await InCompanyAsync(DefaultCompanyName, () => _packages.ImportDataAsync(package.Id, Upload("fix.xlsx", workbook)));

        var validated = await InCompanyAsync(DefaultCompanyName, () => _packages.ValidatePackageAsync(package.Id, new ConfigPackageTablesInput()));
        validated.Errors.ShouldBe(1);

        var records = await InCompanyAsync(
            DefaultCompanyName,
            () => _packages.GetRecordsAsync(new GetConfigPackageRecordsInput { PackageId = package.Id, TableId = tableId, ErrorsOnly = true })
        );
        var record = records.Items.ShouldHaveSingleItem();
        record.Invalid.ShouldBeTrue();
        record.Errors.ShouldHaveSingleItem().FieldName.ShouldBe("CustomerPostingGroup");

        var errors = await InCompanyAsync(DefaultCompanyName, () => _packages.GetErrorsAsync(package.Id));
        errors.Items.ShouldHaveSingleItem().EntityName.ShouldBe("Customer");

        await InCompanyAsync(
            DefaultCompanyName,
            () => _packages.UpdateRecordAsync(record.Id, new UpdateConfigPackageRecordDto { Values = new() { ["CustomerPostingGroup"] = "DOMESTIC" } })
        );

        var applied = await InCompanyAsync(DefaultCompanyName, () => _packages.ApplyPackageAsync(package.Id, new ConfigPackageTablesInput()));
        applied.Errors.ShouldBe(0);
        applied.Inserted.ShouldBe(1);

        var detail = await InCompanyAsync(DefaultCompanyName, () => _packages.GetAsync(package.Id));
        detail.LastAppliedTime.ShouldNotBeNull();
        detail.Tables.Single().NoOfErrors.ShouldBe(0);
    }

    /// <summary>A field mapping turns an old system's code into this one's (BC's "Config. Field Mapping").</summary>
    [Fact]
    public async Task Field_Mappings_And_Excluded_Fields_Are_Honoured()
    {
        var package = await CreatePackageAsync(DefaultCompanyName, "MAP", ["Customer"]);
        var table = package.Tables.Single();

        await InCompanyAsync(
            DefaultCompanyName,
            () =>
                _packages.UpdateTableAsync(
                    package.Id,
                    new UpdateConfigPackageTableDto
                    {
                        TableId = table.Id,
                        ProcessingOrder = 10,
                        Fields =
                        [
                            new UpdateConfigPackageFieldDto
                            {
                                FieldName = "CustomerPostingGroup",
                                IncludeField = true,
                                ValidateField = true,
                                ProcessingOrder = 1,
                                Mappings = [new ConfigFieldMappingDto { OldValue = "LOCAL", NewValue = "DOMESTIC" }],
                            },
                            new UpdateConfigPackageFieldDto { FieldName = "CreditLimit", IncludeField = false, ValidateField = true, ProcessingOrder = 2 },
                        ],
                    }
                )
        );

        var workbook = SpreadsheetWriter.Write("Customer", ["No", "Name", "CustomerPostingGroup", "CreditLimit"], [["C95100", "Mapped", "LOCAL", "not a number"]]);
        await InCompanyAsync(DefaultCompanyName, () => _packages.ImportDataAsync(package.Id, Upload("map.xlsx", workbook)));

        var applied = await InCompanyAsync(DefaultCompanyName, () => _packages.ApplyPackageAsync(package.Id, new ConfigPackageTablesInput()));

        // CreditLimit is excluded, so its bad value is never read.
        applied.Errors.ShouldBe(0);
        var customer = await InCompanyAsync(
            DefaultCompanyName,
            async () => (await GetRequiredService<IRepository<Customer, Guid>>().GetListAsync()).Single(c => c.No == "C95100")
        );
        customer.CustomerPostingGroup.ShouldBe("DOMESTIC");
    }

    [Fact]
    public async Task A_Filter_Narrows_What_Is_Taken_From_The_Database()
    {
        var package = await CreatePackageAsync(DefaultCompanyName, "FLT", ["GLAccount"]);
        var table = package.Tables.Single();

        await InCompanyAsync(
            DefaultCompanyName,
            () =>
                _packages.UpdateTableAsync(
                    package.Id,
                    new UpdateConfigPackageTableDto
                    {
                        TableId = table.Id,
                        ProcessingOrder = 10,
                        Filters = [new EntityFilterDto { Field = "No", Operator = EntityFilterOperator.StartsWith, Value = "1" }],
                    }
                )
        );

        var filled = await InCompanyAsync(DefaultCompanyName, () => _packages.FillFromDatabaseAsync(package.Id, new ConfigPackageTablesInput()));

        var records = await InCompanyAsync(
            DefaultCompanyName,
            () => _packages.GetRecordsAsync(new GetConfigPackageRecordsInput { PackageId = package.Id, TableId = table.Id, MaxResultCount = 100 })
        );

        filled.Tables.Single().NoOfRecords.ShouldBe(records.Items.Count);
        records.Items.ShouldNotBeEmpty();
        records.Items.ShouldAllBe(r => r.Values["No"].StartsWith("1"));
    }

    [Fact]
    public async Task A_File_That_Is_Not_A_Package_Is_Refused()
    {
        var exception = await Should.ThrowAsync<BusinessException>(
            () => InCompanyAsync(DefaultCompanyName, () => _packages.ImportPackageAsync(Upload("x.json", Encoding.UTF8.GetBytes("{\"hello\":1}"))))
        );

        exception.Code.ShouldBe(ErpErrorCodes.RapidStart.FileNotValid);
    }

    [Fact]
    public async Task Deleting_A_Package_Removes_Its_Staged_Data()
    {
        var package = await CreatePackageAsync(DefaultCompanyName, "DEL", ["UnitOfMeasure"]);
        await InCompanyAsync(DefaultCompanyName, () => _packages.FillFromDatabaseAsync(package.Id, new ConfigPackageTablesInput()));

        await InCompanyAsync(DefaultCompanyName, () => _packages.DeleteAsync(package.Id));

        var remaining = await InCompanyAsync(
            DefaultCompanyName,
            () => GetRequiredService<IRepository<ConfigPackageRecord, Guid>>().CountAsync(r => r.ConfigPackageId == package.Id)
        );
        remaining.ShouldBe(0);
    }

    private async Task<ConfigPackageDetailDto> CreatePackageAsync(string company, string code, List<string> tables, bool includeRelated = false)
    {
        return await InCompanyAsync(
            company,
            async () =>
            {
                var package = await _packages.CreateAsync(new CreateConfigPackageDto { Code = code, PackageName = code + " package" });
                return await _packages.IncludeTablesAsync(
                    package.Id,
                    new IncludeConfigTablesInput { EntityNames = tables, IncludeRelatedTables = includeRelated }
                );
            }
        );
    }

    private static UploadFileInput Upload(string fileName, byte[] content)
    {
        return new UploadFileInput { FileName = fileName, ContentBase64 = Convert.ToBase64String(content) };
    }

    private static async Task<byte[]> ReadAsync(IRemoteStreamContent content)
    {
        using var buffer = new MemoryStream();
        await content.GetStream().CopyToAsync(buffer);
        return buffer.ToArray();
    }
}
