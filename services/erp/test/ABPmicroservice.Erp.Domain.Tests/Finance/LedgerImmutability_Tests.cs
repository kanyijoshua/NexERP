using System;
using System.Threading.Tasks;
using ABPmicroservice.Erp.EntityFrameworkCore;
using Shouldly;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.EntityFrameworkCore;
using Xunit;

namespace ABPmicroservice.Erp.Finance;

public class LedgerImmutability_Tests : ErpDomainTestBase
{
    private readonly IRepository<GLEntry, Guid> _glEntryRepository;
    private readonly IRepository<GLAccount, Guid> _glAccountRepository;

    public LedgerImmutability_Tests()
    {
        _glEntryRepository = GetRequiredService<IRepository<GLEntry, Guid>>();
        _glAccountRepository = GetRequiredService<IRepository<GLAccount, Guid>>();
    }

    [Fact]
    public async Task Posted_GL_Entries_Cannot_Be_Deleted()
    {
        var entryId = await InsertEntryAsync(125.5m);

        var ex = await Should.ThrowAsync<BusinessException>(
            () => InCompanyAsync(DefaultCompanyName, () => _glEntryRepository.DeleteAsync(entryId))
        );
        ex.Code.ShouldBe(ErpErrorCodes.Ledgers.LedgerEntryIsImmutable);

        (await InCompanyAsync(DefaultCompanyName, () => _glEntryRepository.AnyAsync(e => e.Id == entryId)))
            .ShouldBeTrue();
    }

    [Fact]
    public async Task Posted_GL_Entry_Amounts_Cannot_Be_Changed()
    {
        var entryId = await InsertEntryAsync(200m);

        var ex = await Should.ThrowAsync<BusinessException>(
            () =>
                InCompanyAsync(
                    DefaultCompanyName,
                    async () =>
                    {
                        // Amount has a private setter; go through the change tracker as a rogue caller would.
                        var dbContext = await GetRequiredService<IDbContextProvider<ErpDbContext>>()
                            .GetDbContextAsync();
                        var entry = await _glEntryRepository.GetAsync(entryId);
                        dbContext.Entry(entry).Property(nameof(GLEntry.Amount)).CurrentValue = 1m;
                        await dbContext.SaveChangesAsync();
                    }
                )
        );
        ex.Code.ShouldBe(ErpErrorCodes.Ledgers.LedgerEntryIsImmutable);

        var stored = await InCompanyAsync(DefaultCompanyName, () => _glEntryRepository.GetAsync(entryId));
        stored.Amount.ShouldBe(200m);
    }

    [Fact]
    public async Task Amounts_Keep_Five_Decimals()
    {
        var entryId = await InsertEntryAsync(10.12345m);

        var stored = await InCompanyAsync(DefaultCompanyName, () => _glEntryRepository.GetAsync(entryId));
        stored.Amount.ShouldBe(10.12345m);
    }

    private Task<Guid> InsertEntryAsync(decimal amount)
    {
        return InCompanyAsync(
            DefaultCompanyName,
            async () =>
            {
                var cash = await _glAccountRepository.SingleAsync(a => a.No == "1010");
                var entry = await _glEntryRepository.InsertAsync(
                    new GLEntry(
                        Guid.NewGuid(),
                        cash.Id,
                        cash.No,
                        new DateTime(2026, 1, 15),
                        GLEntryDocumentType.Invoice,
                        "TEST-001",
                        "Immutability test",
                        amount,
                        null
                    ),
                    autoSave: true
                );
                return entry.Id;
            }
        );
    }
}
