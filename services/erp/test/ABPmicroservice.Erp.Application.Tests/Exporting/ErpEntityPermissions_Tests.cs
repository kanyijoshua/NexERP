using System.Linq;
using ABPmicroservice.Erp.Permissions;
using Shouldly;
using Xunit;

namespace ABPmicroservice.Erp.Exporting;

/// <summary>
/// The registry says what a table is; the permission map says who may see it. If the two drift
/// apart, a table either becomes unreachable or — far worse — reachable to anyone who can export.
/// This is the test that keeps them together.
/// </summary>
public class ErpEntityPermissions_Tests : ErpApplicationTestBase
{
    private readonly ErpEntityRegistry _registry;

    public ErpEntityPermissions_Tests()
    {
        _registry = GetRequiredService<ErpEntityRegistry>();
    }

    [Fact]
    public void Every_Registered_Table_Has_A_Permission()
    {
        var unguarded = _registry
            .GetAll()
            .Where(d => ErpEntityPermissions.Find(d.Name) == null)
            .Select(d => d.Name)
            .ToList();

        unguarded.ShouldBeEmpty();
    }

    [Fact]
    public void Every_Permission_In_The_Map_Really_Exists()
    {
        var defined = ErpPermissions.GetAll().ToHashSet();

        var unknown = _registry
            .GetAll()
            .Select(d => ErpEntityPermissions.Find(d.Name))
            .Where(p => p != null && !defined.Contains(p))
            .Distinct()
            .ToList();

        unknown.ShouldBeEmpty();
    }

    /// <summary>
    /// Exporting a table must need the same permission as reading it on screen, or the export
    /// would be a way around the permission rather than a use of it.
    /// </summary>
    [Theory]
    [InlineData("Customer", ErpPermissions.Customers.Default)]
    [InlineData("Vendor", ErpPermissions.Vendors.Default)]
    [InlineData("GLEntry", ErpPermissions.GLEntries.Default)]
    [InlineData("SalesHeader", ErpPermissions.SalesDocuments.Default)]
    [InlineData("Item", ErpPermissions.Items.Default)]
    public void A_Table_Is_Guarded_By_The_Permission_Of_Its_Own_Screen(string entityName, string expected)
    {
        ErpEntityPermissions.Find(entityName).ShouldBe(expected);
    }
}
