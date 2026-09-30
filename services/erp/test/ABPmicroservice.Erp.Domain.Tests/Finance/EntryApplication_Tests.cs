using System;
using Shouldly;
using Volo.Abp;
using Xunit;

namespace ABPmicroservice.Erp.Finance;

/// <summary>Applying payments to open entries, and splitting a VAT-inclusive journal amount.</summary>
public class EntryApplication_Tests
{
    private sealed class Entry : IApplicableLedgerEntry
    {
        public Entry(long entryNo, decimal amount, decimal amountLcy, string currencyCode = null)
        {
            EntryNo = entryNo;
            DocumentNo = "DOC" + entryNo;
            CurrencyCode = currencyCode;
            RemainingAmount = amount;
            RemainingAmountLcy = amountLcy;
            Open = true;
        }

        public long EntryNo { get; }
        public string DocumentNo { get; }
        public string CurrencyCode { get; }
        public decimal RemainingAmount { get; private set; }
        public decimal RemainingAmountLcy { get; private set; }
        public bool Open { get; private set; }
        public long ClosedBy { get; private set; }

        public void ReduceRemaining(decimal amount, decimal amountLcy, long closedByEntryNo)
        {
            RemainingAmount -= amount;
            RemainingAmountLcy -= amountLcy;
            if (RemainingAmount == 0m)
            {
                Open = false;
                ClosedBy = closedByEntryNo;
            }
        }
    }

    [Fact]
    public void A_Full_Payment_At_A_Lower_Rate_Closes_Both_And_Realizes_A_Loss()
    {
        var invoice = new Entry(1, 100m, 13000m, "USD");
        var payment = new Entry(2, -100m, -12500m, "USD");

        var realized = EntryApplication.Apply(payment, invoice);

        realized.ShouldBe(500m); // positive: a loss
        invoice.Open.ShouldBeFalse();
        payment.Open.ShouldBeFalse();
        invoice.ClosedBy.ShouldBe(2);
        payment.ClosedBy.ShouldBe(1);
        invoice.RemainingAmountLcy.ShouldBe(0m);
        payment.RemainingAmountLcy.ShouldBe(0m);
    }

    [Fact]
    public void A_Part_Payment_Leaves_The_Rest_Open_At_Its_Own_Rate()
    {
        var invoice = new Entry(1, 100m, 13000m, "USD");
        var payment = new Entry(2, -40m, -5600m, "USD"); // paid at 140

        var realized = EntryApplication.Apply(payment, invoice);

        realized.ShouldBe(-400m); // 40 x 130 booked, 40 x 140 received: a gain
        invoice.Open.ShouldBeTrue();
        invoice.RemainingAmount.ShouldBe(60m);
        invoice.RemainingAmountLcy.ShouldBe(7800m);
        payment.Open.ShouldBeFalse();
    }

    [Fact]
    public void An_Entry_In_Another_Currency_Or_The_Same_Sign_Is_Refused()
    {
        Should.Throw<BusinessException>(() => EntryApplication.Apply(new Entry(2, -100m, -100m), new Entry(1, 100m, 13000m, "USD")))
            .Code.ShouldBe(ErpErrorCodes.Journals.AppliesToEntryMismatch);

        Should.Throw<BusinessException>(() => EntryApplication.Apply(new Entry(2, 50m, 50m), new Entry(1, 100m, 100m)))
            .Code.ShouldBe(ErpErrorCodes.Journals.AppliesToEntryMismatch);
    }

    [Theory]
    [InlineData(VatCalculationType.NormalVat, GeneralPostingType.Purchase, 116, 100, 16)]
    [InlineData(VatCalculationType.NormalVat, GeneralPostingType.Sale, -116, -100, -16)]
    [InlineData(VatCalculationType.ReverseChargeVat, GeneralPostingType.Purchase, 100, 100, 16)]
    [InlineData(VatCalculationType.ReverseChargeVat, GeneralPostingType.Sale, -100, -100, 0)]
    [InlineData(VatCalculationType.FullVat, GeneralPostingType.Purchase, 50, 0, 50)]
    [InlineData(VatCalculationType.NormalVat, GeneralPostingType.None, 116, 116, 0)]
    public void A_Journal_Amount_Includes_Its_VAT(VatCalculationType type, GeneralPostingType postingType, decimal amount, decimal expectedBase, decimal expectedVat)
    {
        var setup = new VatPostingSetup(Guid.NewGuid(), "DOMESTIC", "STANDARD");
        setup.SetRate(type, 16m, "VAT16", null);

        var result = JournalVat.Calculate(amount, setup, postingType);

        result.Base.ShouldBe(expectedBase);
        result.Vat.ShouldBe(expectedVat);
    }
}
