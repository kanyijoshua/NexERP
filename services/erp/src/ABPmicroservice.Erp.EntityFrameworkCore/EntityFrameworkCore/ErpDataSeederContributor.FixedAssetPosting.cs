using System.Threading.Tasks;
using ABPmicroservice.Erp.Finance;
using ABPmicroservice.Erp.FixedAssets;
using Volo.Abp.Domain.Repositories;

namespace ABPmicroservice.Erp.EntityFrameworkCore;

/// <summary>
/// What posting to fixed assets needs: the asset, depreciation and disposal accounts, an FA posting
/// group on them, the company book integrated with the G/L, and a depreciation journal batch.
/// </summary>
public partial class ErpDataSeederContributor
{
    private async Task SeedFixedAssetPostingAsync()
    {
        await EnsureAccountAsync("1700", "Equipment", GLAccountCategory.Assets, IncomeBalanceType.BalanceSheet);
        await EnsureAccountAsync("1710", "Accumulated Depreciation, Equipment", GLAccountCategory.Assets, IncomeBalanceType.BalanceSheet);
        await EnsureAccountAsync("4960", "Gains on Disposal of Fixed Assets", GLAccountCategory.Income, IncomeBalanceType.IncomeStatement);
        await EnsureAccountAsync("8200", "Depreciation Expense", GLAccountCategory.Expense, IncomeBalanceType.IncomeStatement);
        await EnsureAccountAsync("8220", "Losses on Disposal of Fixed Assets", GLAccountCategory.Expense, IncomeBalanceType.IncomeStatement);

        if (await IsEmptyAsync<FAPostingGroup>())
        {
            var group = new FAPostingGroup(NewId(), "EQUIPMENT", "Equipment");
            group.Set(
                acquisitionCostAccount: "1700",
                accumDepreciationAccount: "1710",
                writeDownAccount: "1710",
                appreciationAccount: "1700",
                acqCostAccOnDisposal: "1700",
                accumDeprAccOnDisposal: "1710",
                writeDownAccOnDisposal: "1710",
                appreciationAccOnDisposal: "1700",
                gainsAccOnDisposal: "4960",
                lossesAccOnDisposal: "8220",
                bookValAccOnDispGain: null,
                salesAccOnDispGain: null,
                writeDownBalAccOnDisp: null,
                apprecBalAccOnDisp: null,
                maintenanceExpenseAccount: null,
                maintenanceBalAcc: null,
                acquisitionCostBalAcc: null,
                depreciationExpenseAcc: "8200",
                writeDownExpenseAcc: "8200",
                appreciationBalAccount: "4960",
                salesBalAcc: null,
                salesAccOnDispLoss: null,
                bookValAccOnDispLoss: null
            );
            await Repo<FAPostingGroup>().InsertAsync(group);
        }

        var book = await Repo<DepreciationBook>().FirstOrDefaultAsync(b => b.Code == "COMPANY");
        if (book != null && !book.GLIntegrationAcqCost && !book.GLIntegrationDepreciation)
        {
            book.Set(true, true, true, true, true, false, book.DisposalCalculationMethod, false, false, true, false, true, 0m, 0m, false, false, false, false);
            await Repo<DepreciationBook>().UpdateAsync(book);
        }

        if (await IsEmptyAsync<FAJournalBatch>())
        {
            await Repo<FAJournalBatch>().InsertAsync(new FAJournalBatch(NewId(), "DEFAULT", "Fixed asset journal", null));
            await Repo<FAJournalBatch>().InsertAsync(new FAJournalBatch(NewId(), "DEPR", "Depreciation", null));
        }
    }
}
