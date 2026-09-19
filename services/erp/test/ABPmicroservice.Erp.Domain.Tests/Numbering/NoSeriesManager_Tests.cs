using System;
using System.Linq;
using System.Threading.Tasks;
using Shouldly;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Xunit;

namespace ABPmicroservice.Erp.Numbering;

public class NoSeriesManager_Tests : ErpDomainTestBase
{
    private static readonly DateTime Today = new(2026, 6, 15);

    private readonly NoSeriesManager _manager;
    private readonly IRepository<NoSeries, Guid> _repository;

    public NoSeriesManager_Tests()
    {
        _manager = GetRequiredService<NoSeriesManager>();
        _repository = GetRequiredService<IRepository<NoSeries, Guid>>();
    }

    [Fact]
    public async Task Hands_Out_Consecutive_Numbers_Starting_At_The_Starting_No()
    {
        await CreateSeriesAsync("T-SEQ", s => s.AddLine(Guid.NewGuid(), null, "T-0001"));

        (await NextAsync("T-SEQ")).ShouldBe("T-0001");
        (await NextAsync("T-SEQ")).ShouldBe("T-0002");
        (await NextAsync("T-SEQ")).ShouldBe("T-0003");

        var line = (await GetSeriesAsync("T-SEQ")).Lines.Single();
        line.LastNoUsed.ShouldBe("T-0003");
        line.LastDateUsed.ShouldBe(Today);
    }

    [Fact]
    public async Task Peeking_Does_Not_Consume_A_Number()
    {
        await CreateSeriesAsync("T-PEEK", s => s.AddLine(Guid.NewGuid(), null, "P100", incrementByNo: 10));

        (await InCompanyAsync(DefaultCompanyName, () => _manager.PeekNextNoAsync("T-PEEK", Today))).ShouldBe("P100");
        (await InCompanyAsync(DefaultCompanyName, () => _manager.PeekNextNoAsync("T-PEEK", Today))).ShouldBe("P100");
        (await NextAsync("T-PEEK")).ShouldBe("P100");
        (await NextAsync("T-PEEK")).ShouldBe("P110");
    }

    [Fact]
    public async Task Stops_At_The_Ending_No_And_Closes_The_Line()
    {
        await CreateSeriesAsync("T-END", s => s.AddLine(Guid.NewGuid(), null, "E1", endingNo: "E2", warningNo: "E2"));

        (await NextAsync("T-END")).ShouldBe("E1");
        (await GetSeriesAsync("T-END")).Lines.Single().IsPastWarningNo().ShouldBeFalse();

        (await NextAsync("T-END")).ShouldBe("E2");
        var line = (await GetSeriesAsync("T-END")).Lines.Single();
        line.Open.ShouldBeFalse();
        line.IsPastWarningNo().ShouldBeTrue();

        // The used-up line is closed, so there is no line in force any more.
        var ex = await Should.ThrowAsync<BusinessException>(() => NextAsync("T-END"));
        ex.Code.ShouldBe(ErpErrorCodes.NoSeries.NoOpenLine);
    }

    [Fact]
    public async Task Uses_The_Line_In_Force_On_The_Usage_Date()
    {
        await CreateSeriesAsync("T-YEAR", s =>
        {
            s.AddLine(Guid.NewGuid(), new DateTime(2026, 1, 1), "INV26-0001");
            s.AddLine(Guid.NewGuid(), new DateTime(2027, 1, 1), "INV27-0001");
        });

        (await NextAsync("T-YEAR", new DateTime(2026, 12, 31))).ShouldBe("INV26-0001");
        (await NextAsync("T-YEAR", new DateTime(2027, 1, 1))).ShouldBe("INV27-0001");
        (await NextAsync("T-YEAR", new DateTime(2026, 6, 1))).ShouldBe("INV26-0002");

        var ex = await Should.ThrowAsync<BusinessException>(() => NextAsync("T-YEAR", new DateTime(2025, 12, 31)));
        ex.Code.ShouldBe(ErpErrorCodes.NoSeries.NoOpenLine);
    }

    [Fact]
    public async Task Date_Order_Refuses_A_Number_For_An_Earlier_Date()
    {
        await CreateSeriesAsync("T-DATE", s => s.AddLine(Guid.NewGuid(), null, "D1"), dateOrder: true);

        await NextAsync("T-DATE", new DateTime(2026, 6, 15));

        var ex = await Should.ThrowAsync<BusinessException>(() => NextAsync("T-DATE", new DateTime(2026, 6, 14)));
        ex.Code.ShouldBe(ErpErrorCodes.NoSeries.DateOrderViolation);

        (await NextAsync("T-DATE", new DateTime(2026, 6, 15))).ShouldBe("D2");
    }

    [Fact]
    public async Task Resolve_Follows_The_Default_And_Manual_Flags()
    {
        await CreateSeriesAsync("T-AUTO", s => s.AddLine(Guid.NewGuid(), null, "A1"), defaultNos: true, manualNos: false);
        await CreateSeriesAsync("T-BOTH", s => s.AddLine(Guid.NewGuid(), null, "B1"), defaultNos: true, manualNos: true);
        await CreateSeriesAsync("T-MANUAL", s => s.AddLine(Guid.NewGuid(), null, "M1"), defaultNos: false, manualNos: true);

        // Blank takes the next number; a typed one is refused unless manual numbers are allowed.
        (await ResolveAsync("T-AUTO", null)).ShouldBe("A1");
        (await Should.ThrowAsync<BusinessException>(() => ResolveAsync("T-AUTO", "MINE"))).Code
            .ShouldBe(ErpErrorCodes.NoSeries.ManualNumbersNotAllowed);

        (await ResolveAsync("T-BOTH", " MINE ")).ShouldBe("MINE");
        (await ResolveAsync("T-BOTH", "")).ShouldBe("B1");

        // No default numbers: the user has to type one.
        (await Should.ThrowAsync<BusinessException>(() => ResolveAsync("T-MANUAL", null))).Code
            .ShouldBe(ErpErrorCodes.NoSeries.NumberRequired);

        // No series configured at all: the typed number is used as is, and is required.
        (await ResolveAsync(null, "FREE-1")).ShouldBe("FREE-1");
        (await Should.ThrowAsync<BusinessException>(() => ResolveAsync(null, " "))).Code
            .ShouldBe(ErpErrorCodes.NoSeries.NumberRequired);
    }

    [Fact]
    public async Task Series_Are_Separate_Per_Company()
    {
        await CreateSeriesAsync("T-CO", s => s.AddLine(Guid.NewGuid(), null, "X1"));
        await NextAsync("T-CO");

        var ex = await Should.ThrowAsync<BusinessException>(
            () => InCompanyAsync(SecondCompanyName, () => _manager.GetNextNoAsync("T-CO", Today))
        );
        ex.Code.ShouldBe(ErpErrorCodes.NoSeries.NoSeriesNotFound);
    }

    [Fact]
    public void A_Line_Rejects_Nonsense()
    {
        var series = new NoSeries(Guid.NewGuid(), "T-BAD", "Bad lines");

        Should.Throw<BusinessException>(() => series.AddLine(Guid.NewGuid(), null, "NODIGITS"))
            .Code.ShouldBe(ErpErrorCodes.NoSeries.InvalidLine);
        Should.Throw<BusinessException>(() => series.AddLine(Guid.NewGuid(), null, "N10", endingNo: "N05"))
            .Code.ShouldBe(ErpErrorCodes.NoSeries.InvalidLine);
        Should.Throw<BusinessException>(() => series.AddLine(Guid.NewGuid(), null, "N1", incrementByNo: 0))
            .Code.ShouldBe(ErpErrorCodes.NoSeries.InvalidLine);
    }

    private Task CreateSeriesAsync(
        string code,
        Action<NoSeries> addLines,
        bool defaultNos = true,
        bool manualNos = false,
        bool dateOrder = false
    )
    {
        return InCompanyAsync(DefaultCompanyName, async () =>
        {
            var series = new NoSeries(Guid.NewGuid(), code, code + " test series", defaultNos, manualNos, dateOrder);
            addLines(series);
            await _repository.InsertAsync(series, autoSave: true);
        });
    }

    private Task<string> NextAsync(string code, DateTime? date = null)
    {
        return InCompanyAsync(DefaultCompanyName, () => _manager.GetNextNoAsync(code, date ?? Today));
    }

    private Task<string> ResolveAsync(string code, string typedNo)
    {
        return InCompanyAsync(DefaultCompanyName, () => _manager.ResolveNoAsync(code, typedNo, Today));
    }

    private Task<NoSeries> GetSeriesAsync(string code)
    {
        return InCompanyAsync(DefaultCompanyName, () => _repository.FindAsync(s => s.Code == code, includeDetails: true));
    }
}
