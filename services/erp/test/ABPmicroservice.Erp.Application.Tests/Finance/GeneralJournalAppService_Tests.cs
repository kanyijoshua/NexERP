using System;
using System.Linq;
using System.Threading.Tasks;
using Shouldly;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Xunit;

namespace ABPmicroservice.Erp.Finance;

public class GeneralJournalAppService_Tests : ErpApplicationTestBase
{
    private readonly IGeneralJournalAppService _journals;
    private readonly IRepository<GLEntry, Guid> _glEntryRepository;

    public GeneralJournalAppService_Tests()
    {
        _journals = GetRequiredService<IGeneralJournalAppService>();
        _glEntryRepository = GetRequiredService<IRepository<GLEntry, Guid>>();
    }

    [Fact]
    public async Task A_Balanced_Journal_Posts_And_Its_GL_Entries_Sum_To_Zero()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            var batch = await NewBatchAsync("BAL");
            await AddLineAsync(batch.Id, "JNL-001", "1010", 500m);
            await AddLineAsync(batch.Id, "JNL-001", "4000", -500m);

            var result = await _journals.RunPostingAsync(batch.Id);

            result.PostedLineCount.ShouldBe(2);
            result.PostedDocumentCount.ShouldBe(1);
            (await _journals.GetLinesAsync(batch.Id)).Items.ShouldBeEmpty();
        });

        var entries = await InCompanyAsync(
            DefaultCompanyName,
            () => _glEntryRepository.GetListAsync(e => e.DocumentNo == "JNL-001")
        );
        entries.Count.ShouldBe(2);
        entries.Sum(e => e.Amount).ShouldBe(0m);
    }

    [Fact]
    public async Task An_Unbalanced_Document_Is_Refused_And_Nothing_Is_Posted()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            var batch = await NewBatchAsync("UNBAL");
            await AddLineAsync(batch.Id, "JNL-002", "1010", 500m);
            await AddLineAsync(batch.Id, "JNL-002", "4000", -499.99m);

            var ex = await Should.ThrowAsync<BusinessException>(() => _journals.RunPostingAsync(batch.Id));
            ex.Code.ShouldBe(ErpErrorCodes.Journals.DocumentOutOfBalance);

            (await _journals.GetLinesAsync(batch.Id)).Items.Count.ShouldBe(2);
        });

        (await InCompanyAsync(DefaultCompanyName, () => _glEntryRepository.CountAsync(e => e.DocumentNo == "JNL-002")))
            .ShouldBe(0);
    }

    [Fact]
    public async Task A_Line_With_A_Balancing_Account_Balances_Itself()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            var batch = await NewBatchAsync("BALACC");
            await _journals.CreateLineAsync(new CreateGenJournalLineDto
            {
                GenJournalBatchId = batch.Id,
                PostingDate = new DateTime(2026, 2, 1),
                DocumentType = GLEntryDocumentType.Payment,
                DocumentNo = "JNL-003",
                AccountType = "G/L Account",
                AccountNo = "1020",
                Description = "Transfer to bank",
                Amount = 250m,
                BalAccountType = "G/L Account",
                BalAccountNo = "1010",
            });

            (await _journals.RunPostingAsync(batch.Id)).PostedLineCount.ShouldBe(1);
        });

        var entries = await InCompanyAsync(
            DefaultCompanyName,
            () => _glEntryRepository.GetListAsync(e => e.DocumentNo == "JNL-003")
        );
        entries.Count.ShouldBe(2);
        entries.Sum(e => e.Amount).ShouldBe(0m);
    }

    [Fact]
    public async Task Line_Numbers_Do_Not_Collide_After_A_Delete()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            var batch = await NewBatchAsync("LINENO");
            var first = await AddLineAsync(batch.Id, "JNL-004", "1010", 10m);
            var second = await AddLineAsync(batch.Id, "JNL-004", "4000", -10m);

            await _journals.DeleteLineAsync(first.Id);
            var third = await AddLineAsync(batch.Id, "JNL-004", "1010", 10m);

            third.LineNo.ShouldBeGreaterThan(second.LineNo);
        });
    }

    [Fact]
    public async Task An_Empty_Batch_Cannot_Be_Posted()
    {
        await InCompanyAsync(DefaultCompanyName, async () =>
        {
            var batch = await NewBatchAsync("EMPTY");

            var ex = await Should.ThrowAsync<BusinessException>(() => _journals.RunPostingAsync(batch.Id));
            ex.Code.ShouldBe(ErpErrorCodes.Journals.NothingToPost);
        });
    }

    private Task<GenJournalBatchDto> NewBatchAsync(string name)
    {
        return _journals.CreateBatchAsync(
            new CreateGenJournalBatchDto { JournalTemplateName = "GENERAL", Name = name }
        );
    }

    private Task<GenJournalLineDto> AddLineAsync(Guid batchId, string documentNo, string accountNo, decimal amount)
    {
        return _journals.CreateLineAsync(new CreateGenJournalLineDto
        {
            GenJournalBatchId = batchId,
            PostingDate = new DateTime(2026, 2, 1),
            DocumentType = GLEntryDocumentType.Invoice,
            DocumentNo = documentNo,
            AccountType = "G/L Account",
            AccountNo = accountNo,
            Description = "Test line",
            Amount = amount,
        });
    }
}
