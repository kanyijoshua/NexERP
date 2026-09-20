using System.Linq;
using Shouldly;
using Volo.Abp;
using Xunit;

namespace ABPmicroservice.Erp.Reporting;

public class RowFormula_Tests
{
    [Fact]
    public void Reads_Additions_And_Subtractions()
    {
        var terms = RowFormula.Parse("R10+R20-R30");

        terms.Count.ShouldBe(3);
        terms[0].ShouldBe(new RowFormulaTerm(1, "R10"));
        terms[1].ShouldBe(new RowFormulaTerm(1, "R20"));
        terms[2].ShouldBe(new RowFormulaTerm(-1, "R30"));
    }

    [Fact]
    public void A_Leading_Minus_Negates_The_First_Term()
    {
        RowFormula.Parse("-R10").Single().ShouldBe(new RowFormulaTerm(-1, "R10"));
    }

    [Fact]
    public void Spaces_And_Case_Do_Not_Matter()
    {
        RowFormula.Parse(" r10 + r20 ").Select(t => t.RowNo).ShouldBe(new[] { "R10", "R20" });
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("+")]
    [InlineData("R10+")]
    public void Refuses_A_Formula_It_Cannot_Read(string formula)
    {
        // A mistyped reference must be reported, not quietly treated as zero: it would
        // otherwise change a financial statement without any sign that something was wrong.
        var exception = Should.Throw<BusinessException>(() => RowFormula.Parse(formula, "R99"));

        exception.Code.ShouldBe(ErpErrorCodes.Reports.InvalidRowFormula);
    }
}
