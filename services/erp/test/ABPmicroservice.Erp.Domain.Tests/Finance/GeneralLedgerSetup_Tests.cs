using System;
using ABPmicroservice.Erp.Inventory;
using Shouldly;
using Volo.Abp;
using Xunit;

namespace ABPmicroservice.Erp.Finance;

public class GeneralLedgerSetup_Tests
{
    [Fact]
    public void Blank_Posting_Dates_Mean_No_Limit()
    {
        var setup = new GeneralLedgerSetup(Guid.NewGuid());
        setup.IsPostingDateAllowed(new DateTime(1990, 1, 1)).ShouldBeTrue();

        setup.SetAllowedPostingDates(new DateTime(2026, 1, 1), null);
        setup.IsPostingDateAllowed(new DateTime(2025, 12, 31)).ShouldBeFalse();
        setup.IsPostingDateAllowed(new DateTime(2099, 1, 1)).ShouldBeTrue();

        setup.SetAllowedPostingDates(new DateTime(2026, 1, 1), new DateTime(2026, 1, 31, 14, 0, 0));
        setup.IsPostingDateAllowed(new DateTime(2026, 1, 31, 23, 59, 0)).ShouldBeTrue();
        setup.IsPostingDateAllowed(new DateTime(2026, 2, 1)).ShouldBeFalse();
    }

    [Fact]
    public void Invalid_Ranges_Precisions_And_Dimensions_Are_Refused()
    {
        var setup = new GeneralLedgerSetup(Guid.NewGuid());

        Should.Throw<BusinessException>(() => setup.SetAllowedPostingDates(new DateTime(2026, 2, 1), new DateTime(2026, 1, 1)))
            .Code.ShouldBe(ErpErrorCodes.GeneralLedger.InvalidPostingDateRange);
        Should.Throw<BusinessException>(() => setup.SetRoundingPrecisions(0, 0.01m, 0.01m))
            .Code.ShouldBe(ErpErrorCodes.GeneralLedger.InvalidRoundingPrecision);
        Should.Throw<BusinessException>(() => setup.SetGlobalDimensions("area", "AREA"))
            .Code.ShouldBe(ErpErrorCodes.GeneralLedger.SameGlobalDimensions);
    }

    [Fact]
    public void Posting_Setup_Keys_Are_Upper_Case_And_Blank_Accounts_Are_Null()
    {
        var setup = new GeneralPostingSetup(Guid.NewGuid(), " ", "retail");
        setup.GenBusPostingGroup.ShouldBeNull();
        setup.GenProdPostingGroup.ShouldBe("RETAIL");

        setup.SetSalesAccounts("4000", " ", null);
        setup.SalesAccountNo.ShouldBe("4000");
        setup.SalesCreditMemoAccountNo.ShouldBeNull();

        new InventoryPostingSetup(Guid.NewGuid(), "resale", "").InventoryAccountNo.ShouldBeNull();
        new GenBusinessPostingGroup(Guid.NewGuid(), " export ").Code.ShouldBe("EXPORT");
    }
}
