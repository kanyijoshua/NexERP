using System;
using System.Linq;
using ABPmicroservice.Erp.Finance;
using ABPmicroservice.Erp.Sales;
using Shouldly;
using Volo.Abp;
using Xunit;

namespace ABPmicroservice.Erp.Exporting;

/// <summary>
/// What the registry says a table is. Every export, integration query and webhook payload is
/// built from this, so a field that should not leave the system must not appear here.
/// </summary>
public class ErpEntityRegistry_Tests
{
    private readonly ErpEntityRegistry _registry = new();

    [Theory]
    [InlineData("No", "No")]
    [InlineData("CustomerNo", "Customer No")]
    [InlineData("GLAccountNo", "GL Account No")]
    [InlineData("VATPostingSetup", "VAT Posting Setup")]
    [InlineData("SellToCustomerName", "Sell To Customer Name")]
    public void Turns_A_Property_Name_Into_Something_Readable(string name, string expected)
    {
        ErpEntityField.Humanize(name).ShouldBe(expected);
    }

    [Fact]
    public void Finds_A_Table_Whatever_The_Case()
    {
        _registry.Get("Customer").EntityType.ShouldBe(typeof(Customer));
        _registry.Get("customer").Name.ShouldBe("Customer");
        _registry.Find("NoSuchTable").ShouldBeNull();
    }

    [Fact]
    public void Refuses_A_Table_It_Does_Not_Know()
    {
        Should.Throw<BusinessException>(() => _registry.Get("SecretTable"))
            .Code.ShouldBe(ErpErrorCodes.Exporting.UnknownEntity);
    }

    [Fact]
    public void Exposes_The_Business_Fields_Of_A_Table()
    {
        var customer = _registry.Get("Customer");

        customer.FindField("No").ShouldNotBeNull();
        customer.FindField("Name").ShouldNotBeNull();
        customer.FindField("Balance").ShouldNotBeNull();
        customer.FindField("Blocked").ShouldNotBeNull();
    }

    /// <summary>
    /// Plumbing columns say nothing about the business and would only clutter a file; the tenant
    /// id in particular has no business leaving the system at all.
    /// </summary>
    [Theory]
    [InlineData("TenantId")]
    [InlineData("IsDeleted")]
    [InlineData("DeleterId")]
    [InlineData("DeletionTime")]
    [InlineData("ExtraProperties")]
    [InlineData("ConcurrencyStamp")]
    public void Never_Exposes_A_Plumbing_Column(string fieldName)
    {
        foreach (var definition in _registry.GetAll())
        {
            definition.FindField(fieldName).ShouldBeNull($"{definition.Name} exposes {fieldName}");
        }
    }

    /// <summary>A child collection is its own table, exported separately.</summary>
    [Fact]
    public void Does_Not_Expose_Child_Collections()
    {
        _registry.Get("SalesHeader").FindField("Lines").ShouldBeNull();
        _registry.Get("NoSeries").FindField("Lines").ShouldBeNull();
    }

    [Fact]
    public void Offers_The_Business_Columns_First_And_Keeps_The_Rest_Available()
    {
        var customer = _registry.Get("Customer");

        customer.DefaultFields.ShouldContain(f => f.Name == "No");
        customer.DefaultFields.ShouldNotContain(f => f.Name == "CreationTime");
        customer.Fields.ShouldContain(f => f.Name == "CreationTime");
    }

    [Fact]
    public void Knows_Which_Tables_Belong_To_A_Company()
    {
        _registry.Get("Customer").IsCompanyScoped.ShouldBeTrue();
        _registry.Get("GLEntry").IsCompanyScoped.ShouldBeTrue();
    }

    [Fact]
    public void Reports_The_Underlying_Type_Of_A_Nullable_Field()
    {
        var register = _registry.Get(nameof(GLRegister));
        var userId = register.GetField("UserId");

        userId.ClrType.ShouldBe(typeof(Guid));
        userId.IsNullable.ShouldBeTrue();
    }

    [Fact]
    public void Refuses_A_Field_It_Does_Not_Know()
    {
        Should.Throw<BusinessException>(() => _registry.Get("Customer").GetField("Salary"))
            .Code.ShouldBe(ErpErrorCodes.Exporting.UnknownField);
    }

    [Fact]
    public void Every_Table_Has_Distinct_Field_Names()
    {
        foreach (var definition in _registry.GetAll())
        {
            var duplicates = definition
                .Fields.GroupBy(f => f.Name, StringComparer.OrdinalIgnoreCase)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            duplicates.ShouldBeEmpty($"{definition.Name} has duplicate fields");
        }
    }
}
