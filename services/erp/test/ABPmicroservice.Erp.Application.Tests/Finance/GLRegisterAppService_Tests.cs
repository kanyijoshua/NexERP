using System;
using System.Linq;
using System.Threading.Tasks;
using Shouldly;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Xunit;

namespace ABPmicroservice.Erp.Finance;

public class GLRegisterAppService_Tests : ErpApplicationTestBase
{
    private readonly IGLRegisterAppService _registers;
    private readonly IGeneralJournalAppService _journals;
    private readonly IRepository<GLEntry, Guid> _glEntryRepository;

    public GLRegisterAppService_Tests()
    {
        _registers = GetRequiredService<IGLRegisterAppService>();
        _journals = GetRequiredService<IGeneralJournalAppService>();
        _glEntryRepository = GetRequiredService<IRepository<GLEntry, Guid>>();
    }

    [Fact]
    public async Task A_Posting_Run_Leaves_A_Register_Listing_Its_Entries()
    {
        var registerNo = await PostAsync("REG1", "REG-001", 400m);

        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            var registers = await _registers.GetListAsync(new GetGLRegistersInput());
            var register = registers.Items.Single(r => r.No == registerNo);

            register.JournalBatchName.ShouldBe("GENERAL/REG1");
            register.IsReversible.ShouldBeTrue();

            var entries = await _registers.GetEntriesAsync(registerNo);
            entries.Items.Count.ShouldBe(2);
            entries.Items.Sum(e => e.DebitAmount - e.CreditAmount).ShouldBe(0m);
        });
    }

    /// <summary>
    /// A reversal writes the mirror image of every entry and marks both sides, rather than
    /// deleting anything. That is what lets the ledgers stay append-only and still be corrected.
    /// </summary>
    [Fact]
    public async Task Reversing_A_Register_Writes_Counter_Entries_And_Marks_Both_Sides()
    {
        var registerNo = await PostAsync("REG2", "REG-002", 250m);

        var result = await InCompanyAsync(
            DefaultCompanyName,
            () => _registers.RunReversalAsync(new ReverseRegisterInput { RegisterNo = registerNo })
        );

        result.GLEntryCount.ShouldBe(2);
        result.ReversalRegisterNo.ShouldNotBe(registerNo);

        var entries = await InCompanyAsync(
            DefaultCompanyName,
            () => _glEntryRepository.GetListAsync(e => e.DocumentNo == "REG-002")
        );

        // Two original entries and two corrections, netting to nothing.
        entries.Count.ShouldBe(4);
        entries.Sum(e => e.Amount).ShouldBe(0m);
        entries.ShouldAllBe(e => e.Reversed);

        var originals = entries.Where(e => e.ReversedEntryNo == 0).ToList();
        originals.Count.ShouldBe(2);
        originals.ShouldAllBe(e => e.ReversedByEntryNo > 0);
    }

    [Fact]
    public async Task A_Register_Cannot_Be_Reversed_Twice()
    {
        var registerNo = await PostAsync("REG3", "REG-003", 100m);

        await InCompanyAsync(
            DefaultCompanyName,
            () => _registers.RunReversalAsync(new ReverseRegisterInput { RegisterNo = registerNo })
        );

        var exception = await Should.ThrowAsync<BusinessException>(
            () =>
                InCompanyAsync(
                    DefaultCompanyName,
                    () => _registers.RunReversalAsync(new ReverseRegisterInput { RegisterNo = registerNo })
                )
        );

        exception.Code.ShouldBe(ErpErrorCodes.Registers.AlreadyReversed);
    }

    [Fact]
    public async Task A_Reversal_Register_Cannot_Itself_Be_Reversed()
    {
        var registerNo = await PostAsync("REG4", "REG-004", 100m);

        var result = await InCompanyAsync(
            DefaultCompanyName,
            () => _registers.RunReversalAsync(new ReverseRegisterInput { RegisterNo = registerNo })
        );

        var exception = await Should.ThrowAsync<BusinessException>(
            () =>
                InCompanyAsync(
                    DefaultCompanyName,
                    () =>
                        _registers.RunReversalAsync(
                            new ReverseRegisterInput { RegisterNo = result.ReversalRegisterNo }
                        )
                )
        );

        exception.Code.ShouldBe(ErpErrorCodes.Registers.NotReversible);
    }

    [Fact]
    public async Task An_Unknown_Register_Is_Refused()
    {
        var exception = await Should.ThrowAsync<BusinessException>(
            () =>
                InCompanyAsync(
                    DefaultCompanyName,
                    () => _registers.RunReversalAsync(new ReverseRegisterInput { RegisterNo = 999_999 })
                )
        );

        exception.Code.ShouldBe(ErpErrorCodes.Registers.RegisterNotFound);
    }

    [Fact]
    public async Task Only_Reversible_Registers_Are_Listed_When_Asked_For()
    {
        var registerNo = await PostAsync("REG5", "REG-005", 100m);

        await InCompanyAsync(
            DefaultCompanyName,
            () => _registers.RunReversalAsync(new ReverseRegisterInput { RegisterNo = registerNo })
        );

        var reversible = await InCompanyAsync(
            DefaultCompanyName,
            () => _registers.GetListAsync(new GetGLRegistersInput { OnlyReversible = true })
        );

        reversible.Items.ShouldNotContain(r => r.No == registerNo);
    }

    private async Task<long> PostAsync(string batchName, string documentNo, decimal amount)
    {
        return await InCompanyAsync(
            DefaultCompanyName,
            async () =>
            {
                var batch = await _journals.CreateBatchAsync(
                    new CreateGenJournalBatchDto { JournalTemplateName = "GENERAL", Name = batchName }
                );

                await AddAsync(batch.Id, documentNo, "1010", amount);
                await AddAsync(batch.Id, documentNo, "4000", -amount);

                return (await _journals.RunPostingAsync(batch.Id)).RegisterNo;
            }
        );
    }

    private Task AddAsync(Guid batchId, string documentNo, string accountNo, decimal amount)
    {
        return _journals.CreateLineAsync(
            new CreateUpdateGenJournalLineDto
            {
                GenJournalBatchId = batchId,
                PostingDate = new DateTime(2026, 2, 1),
                DocumentType = GLEntryDocumentType.Invoice,
                DocumentNo = documentNo,
                AccountType = GenJournalAccountType.GLAccount,
                AccountNo = accountNo,
                Description = "Register test",
                Amount = amount,
            }
        );
    }
}
