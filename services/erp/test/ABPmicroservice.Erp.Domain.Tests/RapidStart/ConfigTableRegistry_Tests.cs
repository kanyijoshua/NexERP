using System;
using System.Linq;
using System.Reflection;
using ABPmicroservice.Erp.Companies;
using ABPmicroservice.Erp.Exporting;
using Shouldly;
using Xunit;

namespace ABPmicroservice.Erp.RapidStart;

/// <summary>
/// The table profiles are hand-declared metadata. These tests keep them honest against the
/// entities they describe, so a renamed field fails the build instead of a package at a customer.
/// </summary>
public class ConfigTableRegistry_Tests
{
    private readonly ConfigTableRegistry _tables = new(new ErpEntityRegistry());

    [Fact]
    public void Every_Table_Can_Be_Created_The_Way_The_Database_Creates_It()
    {
        foreach (var profile in _tables.GetAll())
        {
            var constructor = profile.Definition.EntityType.GetConstructor(
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                Type.EmptyTypes
            );

            constructor.ShouldNotBeNull($"{profile.Name} has no parameterless constructor");
        }
    }

    [Fact]
    public void Key_Required_And_Related_Fields_Are_Real_Importable_Fields()
    {
        foreach (var profile in _tables.GetAll())
        {
            foreach (var key in profile.KeyFields)
            {
                profile.FindImportableField(key).ShouldNotBeNull($"{profile.Name}.{key} is a key but cannot be imported");
            }

            foreach (var relation in profile.Relations)
            {
                profile.FindImportableField(relation.FieldName).ShouldNotBeNull($"{profile.Name}.{relation.FieldName}");

                var target = _tables.Find(relation.TargetEntity);
                target.ShouldNotBeNull($"{profile.Name}.{relation.FieldName} relates to {relation.TargetEntity}, which cannot be imported");
                target.KeyFields.ShouldBe([relation.TargetKeyField], $"{relation.TargetEntity} is related to by a field that is not its key");

                if (relation.CodeFieldName != null)
                {
                    profile.FindImportableField(relation.CodeFieldName).ShouldNotBeNull();
                }
            }
        }
    }

    /// <summary>
    /// Ledgers and posted documents only ever come from posting; the database refuses to change a
    /// ledger row, and a package must not be a way of writing one.
    /// </summary>
    [Fact]
    public void No_Ledger_Or_Posted_Table_Can_Be_Imported()
    {
        _tables.GetAll().ShouldNotContain(p => typeof(ILedgerEntry).IsAssignableFrom(p.Definition.EntityType));
        _tables.GetAll().ShouldNotContain(p => p.Name.StartsWith("Posted"));
    }

    [Fact]
    public void Totals_Maintained_By_Posting_Are_Not_Importable()
    {
        _tables.Get("Customer").FindImportableField("Balance").ShouldBeNull();
        _tables.Get("Item").FindImportableField("Inventory").ShouldBeNull();
        _tables.Get("Customer").FindImportableField("Id").ShouldBeNull();
        _tables.Get("Customer").FindImportableField("CompanyId").ShouldBeNull();
        _tables.Get("Customer").FindImportableField("Name").ShouldNotBeNull();
    }

    [Fact]
    public void Orders_Tables_After_The_Tables_They_Relate_To()
    {
        var order = _tables.SortByDependencies(["Customer", "CustomerPostingGroup", "GLAccount", "Item", "UnitOfMeasure", "ItemCategory"]).ToList();

        order.IndexOf("GLAccount").ShouldBeLessThan(order.IndexOf("CustomerPostingGroup"));
        order.IndexOf("CustomerPostingGroup").ShouldBeLessThan(order.IndexOf("Customer"));
        order.IndexOf("UnitOfMeasure").ShouldBeLessThan(order.IndexOf("Item"));
        order.IndexOf("ItemCategory").ShouldBeLessThan(order.IndexOf("Item"));
    }

    [Fact]
    public void Related_Tables_Bring_What_A_Table_Depends_On_And_Its_Lines()
    {
        _tables.GetRelatedTables(["Customer"]).ShouldBe(["Customer", "CustomerPostingGroup", "GLAccount"], ignoreOrder: true);
        _tables.GetRelatedTables(["NoSeries"]).ShouldBe(["NoSeries", "NoSeriesLine"], ignoreOrder: true);
    }

    [Fact]
    public void A_Setup_Table_Has_No_Key_And_Holds_One_Record()
    {
        _tables.Get("SalesReceivablesSetup").IsSingleton.ShouldBeTrue();
        _tables.Get("Customer").IsSingleton.ShouldBeFalse();
    }
}
