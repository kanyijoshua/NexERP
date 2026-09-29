using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Inventory;
using ABPmicroservice.Erp.Numbering;
using ABPmicroservice.Erp.Sales;
using Shouldly;
using Volo.Abp.Domain.Repositories;
using Xunit;

namespace ABPmicroservice.Erp.RapidStart;

public class ConfigRecordApplier_Tests : ErpDomainTestBase
{
    private readonly ConfigRecordApplier _applier;
    private readonly ConfigTableRegistry _tables;

    public ConfigRecordApplier_Tests()
    {
        _applier = GetRequiredService<ConfigRecordApplier>();
        _tables = GetRequiredService<ConfigTableRegistry>();
    }

    [Fact]
    public async Task Inserts_A_New_Record_And_Updates_One_With_A_Known_Key()
    {
        var result = await ApplyAsync(
            "Customer",
            ["No", "Name", "CustomerPostingGroup", "CreditLimit"],
            Row(("No", "C90000"), ("Name", "New Customer"), ("CustomerPostingGroup", "DOMESTIC"), ("CreditLimit", "2500")),
            Row(("No", "C00010"), ("Name", "Adatum Renamed"), ("CustomerPostingGroup", "DOMESTIC"), ("CreditLimit", "100"))
        );

        result.Errors.ShouldBeEmpty();
        result.Inserted.ShouldBe(1);
        result.Modified.ShouldBe(1);

        var customers = await CustomersAsync();
        customers.Single(c => c.No == "C90000").CreditLimit.ShouldBe(2500m);
        customers.Single(c => c.No == "C00010").Name.ShouldBe("Adatum Renamed");
    }

    /// <summary>A key matches whatever its case, and the stored key keeps the case it had.</summary>
    [Fact]
    public async Task Matches_An_Existing_Key_Whatever_Its_Case()
    {
        var result = await ApplyAsync("Customer", ["No", "Name"], Row(("No", "c00010"), ("Name", "Adatum")));

        result.Modified.ShouldBe(1);
        (await CustomersAsync()).Count(c => c.No.Equals("C00010", StringComparison.OrdinalIgnoreCase)).ShouldBe(1);
        (await CustomersAsync()).ShouldContain(c => c.No == "C00010");
    }

    /// <summary>BC's own message for a TableRelation that is not met.</summary>
    [Fact]
    public async Task A_Value_Missing_From_The_Related_Table_Is_An_Error()
    {
        var result = await ApplyAsync(
            "Customer",
            ["No", "Name", "CustomerPostingGroup"],
            Row(("No", "C90001"), ("Name", "X"), ("CustomerPostingGroup", "EXPORT"))
        );

        result.Inserted.ShouldBe(0);
        var error = result.Errors.ShouldHaveSingleItem();
        error.FieldName.ShouldBe("CustomerPostingGroup");
        error.Message.ShouldContain("EXPORT");
        error.Message.ShouldContain("CustomerPostingGroup");
        (await CustomersAsync()).ShouldNotContain(c => c.No == "C90001");
    }

    [Fact]
    public async Task Checks_Required_Fields_Lengths_And_Types()
    {
        var result = await ApplyAsync(
            "Customer",
            ["No", "Name", "CreditLimit", "PostCode"],
            Row(("No", "C90002"), ("Name", ""), ("CreditLimit", "lots"), ("PostCode", new string('9', 21)))
        );

        result.Errors.Select(e => e.FieldName).ShouldBe(["Name", "CreditLimit", "PostCode"], ignoreOrder: true);
        result.Errors.ShouldAllBe(e => e.RecordNo == 1);
    }

    [Fact]
    public async Task A_New_Record_Must_Carry_Every_Required_Field()
    {
        var result = await ApplyAsync("Customer", ["No"], Row(("No", "C90003")));

        result.Errors.ShouldHaveSingleItem().FieldName.ShouldBe("Name");
    }

    [Fact]
    public async Task A_Validation_Run_Writes_Nothing()
    {
        var result = await ApplyAsync("Customer", ["No", "Name"], dryRun: true, Row(("No", "C90004"), ("Name", "Dry")));

        result.Inserted.ShouldBe(1);
        (await CustomersAsync()).ShouldNotContain(c => c.No == "C90004");
    }

    [Fact]
    public async Task Field_Mappings_Translate_Values_Before_Anything_Else()
    {
        var result = await InCompanyAsync(
            DefaultCompanyName,
            () =>
                _applier.ApplyAsync(
                    new ConfigApplyRequest(_tables.Get("Customer"), ["No", "Name", "CustomerPostingGroup"])
                    {
                        Mappings = new Dictionary<string, List<ConfigFieldMapping>>
                        {
                            ["CustomerPostingGroup"] = [new ConfigFieldMapping { OldValue = "LOCAL", NewValue = "DOMESTIC" }],
                        },
                    },
                    [new ConfigRecordInput(1, Row(("No", "C90005"), ("Name", "Mapped"), ("CustomerPostingGroup", "local")))]
                )
        );

        result.Errors.ShouldBeEmpty();
        (await CustomersAsync()).Single(c => c.No == "C90005").CustomerPostingGroup.ShouldBe("DOMESTIC");
    }

    [Fact]
    public async Task A_Template_Fills_What_A_New_Record_Leaves_Blank()
    {
        var template = new ConfigTemplate(Guid.NewGuid(), "CUST", "Customer");
        template.SetLines([("CustomerPostingGroup", "DOMESTIC", false), ("CreditLimit", "5000", false)], Guid.NewGuid);

        var result = await InCompanyAsync(
            DefaultCompanyName,
            () =>
                _applier.ApplyAsync(
                    new ConfigApplyRequest(_tables.Get("Customer"), ["No", "Name", "CreditLimit"]) { Template = template },
                    [new ConfigRecordInput(1, Row(("No", "C90006"), ("Name", "Templated"), ("CreditLimit", "")))]
                )
        );

        result.Errors.ShouldBeEmpty();
        var customer = (await CustomersAsync()).Single(c => c.No == "C90006");
        customer.CustomerPostingGroup.ShouldBe("DOMESTIC");
        customer.CreditLimit.ShouldBe(5000m);
    }

    /// <summary>
    /// A file names a parent category by its code; the parent's id is looked up, and a parent that
    /// comes later in the file is put in place first.
    /// </summary>
    [Fact]
    public async Task Resolves_References_By_Code_And_Puts_Parents_First()
    {
        var result = await ApplyAsync(
            "ItemCategory",
            ["Code", "Description", "ParentCategoryCode"],
            Row(("Code", "CHAIRS"), ("Description", "Chairs"), ("ParentCategoryCode", "FURNITURE")),
            Row(("Code", "FURNITURE"), ("Description", "Furniture"), ("ParentCategoryCode", ""))
        );

        result.Errors.ShouldBeEmpty();

        var categories = await InCompanyAsync(DefaultCompanyName, () => GetRequiredService<IRepository<ItemCategory, Guid>>().GetListAsync());
        var furniture = categories.Single(c => c.Code == "FURNITURE");
        var chairs = categories.Single(c => c.Code == "CHAIRS");

        chairs.ParentCategoryId.ShouldBe(furniture.Id);
        chairs.ParentCategoryCode.ShouldBe("FURNITURE");
    }

    /// <summary>
    /// Lines are keyed by their header's key, not its id, so a number series line from another
    /// company's package lands on this company's series of the same code.
    /// </summary>
    [Fact]
    public async Task A_Line_Is_Attached_To_Its_Header_By_The_Header_Key()
    {
        await ApplyAsync("NoSeries", ["Code", "Description"], Row(("Code", "IMP"), ("Description", "Imported")));
        var result = await ApplyAsync(
            "NoSeriesLine",
            ["NoSeriesId", "LineNo", "StartingNo", "IncrementByNo", "Open"],
            Row(("NoSeriesId", "IMP"), ("LineNo", "10000"), ("StartingNo", "IMP0001"), ("IncrementByNo", "1"), ("Open", "yes"))
        );

        result.Errors.ShouldBeEmpty();

        var series = await InCompanyAsync(
            DefaultCompanyName,
            async () => (await GetRequiredService<IRepository<NoSeries, Guid>>().GetListAsync(includeDetails: true)).Single(s => s.Code == "IMP")
        );
        series.Lines.ShouldHaveSingleItem().StartingNo.ShouldBe("IMP0001");
    }

    /// <summary>Within one run, a record may rely on another table's record the same run creates.</summary>
    [Fact]
    public async Task A_Dry_Run_Remembers_Keys_For_Later_Tables()
    {
        var context = new ConfigApplyContext();

        await InCompanyAsync(
            DefaultCompanyName,
            async () =>
            {
                var group = await _applier.ApplyAsync(
                    new ConfigApplyRequest(_tables.Get("CustomerPostingGroup"), ["Code", "ReceivablesAccountNo"]) { DryRun = true },
                    [new ConfigRecordInput(1, Row(("Code", "EXPORT"), ("ReceivablesAccountNo", "1200")))],
                    context
                );
                group.Errors.ShouldBeEmpty();

                var customer = await _applier.ApplyAsync(
                    new ConfigApplyRequest(_tables.Get("Customer"), ["No", "Name", "CustomerPostingGroup"]) { DryRun = true },
                    [new ConfigRecordInput(1, Row(("No", "C90007"), ("Name", "Exporter"), ("CustomerPostingGroup", "EXPORT")))],
                    context
                );
                customer.Errors.ShouldBeEmpty();
            }
        );
    }

    [Fact]
    public async Task A_Setup_Table_Updates_Its_One_Record()
    {
        var result = await ApplyAsync("SalesReceivablesSetup", ["CustomerNos"], Row(("CustomerNos", "CUST")));

        result.Modified.ShouldBe(1);
        result.Inserted.ShouldBe(0);
    }

    private Task<ConfigApplyResult> ApplyAsync(string table, string[] fields, params Dictionary<string, string>[] rows)
    {
        return ApplyAsync(table, fields, false, rows);
    }

    private Task<ConfigApplyResult> ApplyAsync(string table, string[] fields, bool dryRun, params Dictionary<string, string>[] rows)
    {
        return InCompanyAsync(
            DefaultCompanyName,
            () =>
                _applier.ApplyAsync(
                    new ConfigApplyRequest(_tables.Get(table), fields) { DryRun = dryRun },
                    rows.Select((row, index) => new ConfigRecordInput(index + 1, row)).ToList()
                )
        );
    }

    private Task<List<Customer>> CustomersAsync()
    {
        return InCompanyAsync(DefaultCompanyName, () => GetRequiredService<IRepository<Customer, Guid>>().GetListAsync());
    }

    private static Dictionary<string, string> Row(params (string Field, string Value)[] values)
    {
        return values.ToDictionary(v => v.Field, v => v.Value, StringComparer.OrdinalIgnoreCase);
    }
}
