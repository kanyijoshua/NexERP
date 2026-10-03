using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace ABPmicroservice.Erp.Finance;

/// <summary>
/// Domain service for G/L Account invariants (uniqueness, posting rules).
/// </summary>
public class GLAccountManager : DomainService
{
    private readonly IRepository<GLAccount, Guid> _glAccountRepository;

    public GLAccountManager(IRepository<GLAccount, Guid> glAccountRepository)
    {
        _glAccountRepository = glAccountRepository;
    }

    public async Task<GLAccount> CreateAsync(
        string no,
        string name,
        GLAccountType accountType,
        GLAccountCategory accountCategory,
        IncomeBalanceType incomeBalance,
        string subcategory = null,
        bool directPosting = true,
        string searchName = null,
        GLAccountDebitCredit debitCredit = GLAccountDebitCredit.Both,
        bool reconciliationAccount = false,
        string totaling = null,
        string genBusPostingGroup = null,
        string genProdPostingGroup = null,
        bool automaticExtTexts = false,
        string taxAreaCode = null,
        bool taxLiable = false,
        string taxGroupCode = null,
        ConsolidationTranslationMethod consolTranslationMethod = ConsolidationTranslationMethod.Average,
        string consolDebitAcc = null,
        string consolCreditAcc = null,
        string costTypeNo = null,
        string defaultDeferralTemplateCode = null,
        bool omitDefaultDescrInJnl = false
    )
    {
        await EnsureNoIsUniqueAsync(no);

        return new GLAccount(
            GuidGenerator.Create(),
            no,
            name,
            accountType,
            accountCategory,
            incomeBalance,
            subcategory,
            directPosting,
            searchName,
            debitCredit,
            reconciliationAccount,
            totaling,
            genBusPostingGroup,
            genProdPostingGroup,
            automaticExtTexts,
            taxAreaCode,
            taxLiable,
            taxGroupCode,
            consolTranslationMethod,
            consolDebitAcc,
            consolCreditAcc,
            costTypeNo,
            defaultDeferralTemplateCode,
            omitDefaultDescrInJnl
        );
    }

    public async Task EnsureNoIsUniqueAsync(string no, Guid? excludeId = null)
    {
        var existing = await _glAccountRepository.FirstOrDefaultAsync(x => x.No == no);
        if (existing != null && existing.Id != excludeId)
        {
            throw new BusinessException(ErpErrorCodes.GLAccounts.GLAccountAlreadyExists).WithData(
                "no",
                no
            );
        }
    }

    /// <summary>Validates that an account can accept a direct posting.</summary>
    public void EnsureCanPost(GLAccount account)
    {
        if (account.Blocked)
        {
            throw new BusinessException(ErpErrorCodes.GLAccounts.AccountBlocked).WithData(
                "no",
                account.No
            );
        }

        if (!account.DirectPosting || account.AccountType != GLAccountType.Posting)
        {
            throw new BusinessException(ErpErrorCodes.GLAccounts.DirectPostingNotAllowed).WithData(
                "no",
                account.No
            );
        }
    }
}
