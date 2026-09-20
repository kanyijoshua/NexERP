using System;
using Shouldly;
using Xunit;

namespace ABPmicroservice.Erp.Reporting;

/// <summary>
/// Which ageing bucket an open entry falls into. The boundaries are the part people check first
/// on an aged report, and an off-by-one would move money between columns.
/// </summary>
public class AgedBuckets_Tests
{
    private static readonly DateTime AsOf = new(2026, 3, 31);

    [Theory]
    // Not yet due, including the day it falls due.
    [InlineData("2026-04-15", 0)]
    [InlineData("2026-03-31", 0)]
    // First period: 1 to 30 days overdue.
    [InlineData("2026-03-30", 1)]
    [InlineData("2026-03-01", 1)]
    // Second: 31 to 60.
    [InlineData("2026-02-28", 2)]
    [InlineData("2026-01-31", 2)]
    // 60 days overdue is the last day of the second period, 61 the first of the third.
    [InlineData("2026-01-30", 2)]
    [InlineData("2026-01-29", 3)]
    // Anything older lands in the last bucket.
    [InlineData("2025-06-01", 4)]
    public void Places_An_Entry_By_How_Long_It_Has_Been_Overdue(string dueDate, int expectedBucket)
    {
        AgedAccountsEngine.BucketOf(DateTime.Parse(dueDate), AsOf, periodLength: 30).ShouldBe(expectedBucket);
    }

    [Fact]
    public void The_Period_Length_Moves_The_Boundaries()
    {
        // With a 7-day period, 8 days overdue is already the second bucket.
        AgedAccountsEngine.BucketOf(AsOf.AddDays(-8), AsOf, periodLength: 7).ShouldBe(2);
        AgedAccountsEngine.BucketOf(AsOf.AddDays(-7), AsOf, periodLength: 7).ShouldBe(1);
    }

    [Fact]
    public void The_Time_Of_Day_Does_Not_Change_The_Bucket()
    {
        var dueLate = new DateTime(2026, 3, 30, 23, 59, 0);

        AgedAccountsEngine.BucketOf(dueLate, AsOf, periodLength: 30).ShouldBe(1);
    }
}
