using System;

namespace ABPmicroservice.Erp.Documents;

/// <summary>
/// Splits a line amount over partial postings of its quantity. Each part is the rounded running
/// total up to that point minus the rounded running total before it, so the parts of a line that
/// is invoiced in several goes always add up to the line amount exactly.
/// </summary>
public static class DocumentAmounts
{
    /// <summary>
    /// The share of <paramref name="total"/> for <paramref name="now"/> units of a line of
    /// <paramref name="quantity"/>, of which <paramref name="done"/> units were posted before.
    /// </summary>
    public static decimal Portion(decimal total, decimal quantity, decimal done, decimal now)
    {
        if (quantity == 0m || now == 0m)
        {
            return 0m;
        }

        if (done + now == quantity)
        {
            return total - Round(total * done / quantity);
        }

        return Round(total * (done + now) / quantity) - Round(total * done / quantity);
    }

    public static decimal Round(decimal value) => Math.Round(value, 2, MidpointRounding.AwayFromZero);
}
