using System;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Companies;
using ABPmicroservice.Erp.Permissions;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;

namespace ABPmicroservice.Erp.CashManagement;

/// <summary>Bank Acc. Reconciliations.</summary>
public class BankAccReconciliationAppService : ErpTableAppService<BankAccReconciliation, BankAccReconciliationDto, GetBankAccReconciliationListInput, CreateUpdateBankAccReconciliationDto>, IBankAccReconciliationAppService
{
    public BankAccReconciliationAppService(IRepository<BankAccReconciliation, Guid> repository)
        : base(repository, ErpPermissions.BankReconciliations.Default) { }

    public override async Task<BankAccReconciliationDto> CreateAsync(CreateUpdateBankAccReconciliationDto input)
    {
        await CheckCreatePolicyAsync();
        await ValidateAsync(input);

        await EnsureKeyIsUniqueAsync(input.StatementType, CodeTableEntity.NormalizeCode(input.BankAccountNo), CodeTableEntity.NormalizeCode(input.StatementNo), null);

        var entity = new BankAccReconciliation(GuidGenerator.Create(), input.StatementType, CodeTableEntity.NormalizeCode(input.BankAccountNo), CodeTableEntity.NormalizeCode(input.StatementNo));
        entity.Set(
            input.StatementEndingBalance,
            input.StatementDate,
            input.BalanceLastStatement,
            input.ShortcutDimension1Code,
            input.ShortcutDimension2Code,
            input.PostPaymentsOnly
        );

        await Repository.InsertAsync(entity, autoSave: true);
        return await MapToGetOutputDtoAsync(entity);
    }

    public override async Task<BankAccReconciliationDto> UpdateAsync(Guid id, CreateUpdateBankAccReconciliationDto input)
    {
        await CheckUpdatePolicyAsync();

        var entity = await GetEntityByIdAsync(id);
        await ValidateAsync(input);

        await EnsureKeyIsUniqueAsync(input.StatementType, CodeTableEntity.NormalizeCode(input.BankAccountNo), CodeTableEntity.NormalizeCode(input.StatementNo), id);

        entity.SetKey(input.StatementType, CodeTableEntity.NormalizeCode(input.BankAccountNo), CodeTableEntity.NormalizeCode(input.StatementNo));
        entity.Set(
            input.StatementEndingBalance,
            input.StatementDate,
            input.BalanceLastStatement,
            input.ShortcutDimension1Code,
            input.ShortcutDimension2Code,
            input.PostPaymentsOnly
        );

        await Repository.UpdateAsync(entity, autoSave: true);
        return await MapToGetOutputDtoAsync(entity);
    }

    protected override async Task<IQueryable<BankAccReconciliation>> CreateFilteredQueryAsync(GetBankAccReconciliationListInput input)
    {
        var query = await base.CreateFilteredQueryAsync(input);
        var filter = input.Filter?.Trim().ToLower();
        var bankAccountNo = input.BankAccountNo?.Trim().ToUpperInvariant();

        return query
            .WhereIf(!bankAccountNo.IsNullOrEmpty(), x => x.BankAccountNo == bankAccountNo)
            .WhereIf(
                !filter.IsNullOrEmpty(),
                x => x.BankAccountNo.ToLower().Contains(filter)
                    || x.StatementNo.ToLower().Contains(filter)
                    || (x.ShortcutDimension1Code != null && x.ShortcutDimension1Code.ToLower().Contains(filter))
                    || (x.ShortcutDimension2Code != null && x.ShortcutDimension2Code.ToLower().Contains(filter))
            );
    }

    protected override IQueryable<BankAccReconciliation> ApplyDefaultSorting(IQueryable<BankAccReconciliation> query) =>
        query.OrderBy(x => x.StatementType).ThenBy(x => x.BankAccountNo).ThenBy(x => x.StatementNo);

    private async Task ValidateAsync(CreateUpdateBankAccReconciliationDto input)
    {
        await Relations.EnsureNoExistsAsync<BankAccount>(input.BankAccountNo);
    }

    private async Task EnsureKeyIsUniqueAsync(BankAccRecStmtType statementType, string bankAccountNo, string statementNo, Guid? exceptId)
    {
        if (await Repository.AnyAsync(x => x.StatementType == statementType && x.BankAccountNo == bankAccountNo && x.StatementNo == statementNo && x.Id != exceptId))
        {
            throw new BusinessException(ErpErrorCodes.BaseTables.RecordAlreadyExists).WithData("table", "Bank Acc. Reconciliation").WithData("key", statementType.ToString() + " " + bankAccountNo + " " + statementNo);
        }
    }
}

/// <summary>Bank Acc. Reconciliation Lines.</summary>
public class BankAccReconciliationLineAppService : ErpTableAppService<BankAccReconciliationLine, BankAccReconciliationLineDto, GetBankAccReconciliationLineListInput, CreateUpdateBankAccReconciliationLineDto>, IBankAccReconciliationLineAppService
{
    public BankAccReconciliationLineAppService(IRepository<BankAccReconciliationLine, Guid> repository)
        : base(repository, ErpPermissions.BankReconciliations.Default) { }

    public override async Task<BankAccReconciliationLineDto> CreateAsync(CreateUpdateBankAccReconciliationLineDto input)
    {
        await CheckCreatePolicyAsync();
        await ValidateAsync(input);

        var statementLineNo = input.StatementLineNo > 0 ? input.StatementLineNo : await NextStatementLineNoAsync(input);
        await EnsureKeyIsUniqueAsync(input.StatementType, CodeTableEntity.NormalizeCode(input.BankAccountNo), CodeTableEntity.NormalizeCode(input.StatementNo), statementLineNo, null);

        var entity = new BankAccReconciliationLine(GuidGenerator.Create(), input.StatementType, CodeTableEntity.NormalizeCode(input.BankAccountNo), CodeTableEntity.NormalizeCode(input.StatementNo), statementLineNo);
        entity.Set(
            input.DocumentNo,
            input.TransactionDate,
            input.Description,
            input.StatementAmount,
            input.Difference,
            input.AppliedAmount,
            input.ValueDate,
            input.ReadyForApplication,
            input.CheckNo,
            input.RelatedPartyName,
            input.AdditionalTransactionInfo,
            input.AccountType,
            input.AccountNo,
            input.TransactionText,
            input.RelatedPartyBankAccNo,
            input.RelatedPartyAddress,
            input.RelatedPartyCity,
            input.PaymentReferenceNo,
            input.ShortcutDimension1Code,
            input.ShortcutDimension2Code,
            input.TransactionId
        );

        await Repository.InsertAsync(entity, autoSave: true);
        return await MapToGetOutputDtoAsync(entity);
    }

    public override async Task<BankAccReconciliationLineDto> UpdateAsync(Guid id, CreateUpdateBankAccReconciliationLineDto input)
    {
        await CheckUpdatePolicyAsync();

        var entity = await GetEntityByIdAsync(id);
        await ValidateAsync(input);

        var statementLineNo = input.StatementLineNo > 0 ? input.StatementLineNo : entity.StatementLineNo;
        await EnsureKeyIsUniqueAsync(input.StatementType, CodeTableEntity.NormalizeCode(input.BankAccountNo), CodeTableEntity.NormalizeCode(input.StatementNo), statementLineNo, id);

        entity.SetKey(input.StatementType, CodeTableEntity.NormalizeCode(input.BankAccountNo), CodeTableEntity.NormalizeCode(input.StatementNo), statementLineNo);
        entity.Set(
            input.DocumentNo,
            input.TransactionDate,
            input.Description,
            input.StatementAmount,
            input.Difference,
            input.AppliedAmount,
            input.ValueDate,
            input.ReadyForApplication,
            input.CheckNo,
            input.RelatedPartyName,
            input.AdditionalTransactionInfo,
            input.AccountType,
            input.AccountNo,
            input.TransactionText,
            input.RelatedPartyBankAccNo,
            input.RelatedPartyAddress,
            input.RelatedPartyCity,
            input.PaymentReferenceNo,
            input.ShortcutDimension1Code,
            input.ShortcutDimension2Code,
            input.TransactionId
        );

        await Repository.UpdateAsync(entity, autoSave: true);
        return await MapToGetOutputDtoAsync(entity);
    }

    protected override async Task<IQueryable<BankAccReconciliationLine>> CreateFilteredQueryAsync(GetBankAccReconciliationLineListInput input)
    {
        var query = await base.CreateFilteredQueryAsync(input);
        var filter = input.Filter?.Trim().ToLower();
        var bankAccountNo = input.BankAccountNo?.Trim().ToUpperInvariant();
        var statementNo = input.StatementNo?.Trim().ToUpperInvariant();

        return query
            .WhereIf(!bankAccountNo.IsNullOrEmpty(), x => x.BankAccountNo == bankAccountNo)
            .WhereIf(!statementNo.IsNullOrEmpty(), x => x.StatementNo == statementNo)
            .WhereIf(
                !filter.IsNullOrEmpty(),
                x => x.BankAccountNo.ToLower().Contains(filter)
                    || x.StatementNo.ToLower().Contains(filter)
                    || (x.DocumentNo != null && x.DocumentNo.ToLower().Contains(filter))
                    || (x.Description != null && x.Description.ToLower().Contains(filter))
                    || (x.CheckNo != null && x.CheckNo.ToLower().Contains(filter))
                    || (x.RelatedPartyName != null && x.RelatedPartyName.ToLower().Contains(filter))
            );
    }

    protected override IQueryable<BankAccReconciliationLine> ApplyDefaultSorting(IQueryable<BankAccReconciliationLine> query) =>
        query.OrderBy(x => x.StatementType).ThenBy(x => x.BankAccountNo).ThenBy(x => x.StatementNo).ThenBy(x => x.StatementLineNo);

    private async Task ValidateAsync(CreateUpdateBankAccReconciliationLineDto input)
    {
        await Relations.EnsureNoExistsAsync<BankAccount>(input.BankAccountNo);
    }

    private async Task EnsureKeyIsUniqueAsync(BankAccRecStmtType statementType, string bankAccountNo, string statementNo, int statementLineNo, Guid? exceptId)
    {
        if (await Repository.AnyAsync(x => x.StatementType == statementType && x.BankAccountNo == bankAccountNo && x.StatementNo == statementNo && x.StatementLineNo == statementLineNo && x.Id != exceptId))
        {
            throw new BusinessException(ErpErrorCodes.BaseTables.RecordAlreadyExists).WithData("table", "Bank Acc. Reconciliation Line").WithData("key", statementType.ToString() + " " + bankAccountNo + " " + statementNo + " " + statementLineNo.ToString());
        }
    }

    /// <summary>The next Statement Line No. of the parent record: 10000 above the last.</summary>
    private async Task<int> NextStatementLineNoAsync(CreateUpdateBankAccReconciliationLineDto input)
    {
        var statementType = input.StatementType;
        var bankAccountNo = CodeTableEntity.NormalizeCode(input.BankAccountNo);
        var statementNo = CodeTableEntity.NormalizeCode(input.StatementNo);
        var query = await Repository.GetQueryableAsync();
        var last = await AsyncExecuter.FirstOrDefaultAsync(query.Where(x => x.StatementType == statementType && x.BankAccountNo == bankAccountNo && x.StatementNo == statementNo).OrderByDescending(x => x.StatementLineNo));

        return (last?.StatementLineNo ?? 0) + 10000;
    }
}

/// <summary>Bank Account Statements.</summary>
public class BankAccountStatementAppService : ErpReadOnlyAppService<BankAccountStatement, BankAccountStatementDto, Guid, GetBankAccountStatementListInput>, IBankAccountStatementAppService
{
    public BankAccountStatementAppService(IRepository<BankAccountStatement, Guid> repository)
        : base(repository)
    {
        GetPolicyName = ErpPermissions.BankReconciliations.Default;
        GetListPolicyName = ErpPermissions.BankReconciliations.Default;
    }

    protected override async Task<IQueryable<BankAccountStatement>> CreateFilteredQueryAsync(GetBankAccountStatementListInput input)
    {
        var query = await base.CreateFilteredQueryAsync(input);
        var filter = input.Filter?.Trim().ToLower();
        var bankAccountNo = input.BankAccountNo?.Trim().ToUpperInvariant();

        return query
            .WhereIf(!bankAccountNo.IsNullOrEmpty(), x => x.BankAccountNo == bankAccountNo)
            .WhereIf(
                !filter.IsNullOrEmpty(),
                x => (x.BankAccountNo != null && x.BankAccountNo.ToLower().Contains(filter))
                    || (x.StatementNo != null && x.StatementNo.ToLower().Contains(filter))
            );
    }

    protected override IQueryable<BankAccountStatement> ApplyDefaultSorting(IQueryable<BankAccountStatement> query) =>
        query.OrderBy(x => x.BankAccountNo).ThenBy(x => x.StatementNo);
}

/// <summary>Bank Account Statement Lines.</summary>
public class BankAccountStatementLineAppService : ErpReadOnlyAppService<BankAccountStatementLine, BankAccountStatementLineDto, Guid, GetBankAccountStatementLineListInput>, IBankAccountStatementLineAppService
{
    public BankAccountStatementLineAppService(IRepository<BankAccountStatementLine, Guid> repository)
        : base(repository)
    {
        GetPolicyName = ErpPermissions.BankReconciliations.Default;
        GetListPolicyName = ErpPermissions.BankReconciliations.Default;
    }

    protected override async Task<IQueryable<BankAccountStatementLine>> CreateFilteredQueryAsync(GetBankAccountStatementLineListInput input)
    {
        var query = await base.CreateFilteredQueryAsync(input);
        var filter = input.Filter?.Trim().ToLower();
        var bankAccountNo = input.BankAccountNo?.Trim().ToUpperInvariant();
        var statementNo = input.StatementNo?.Trim().ToUpperInvariant();

        return query
            .WhereIf(!bankAccountNo.IsNullOrEmpty(), x => x.BankAccountNo == bankAccountNo)
            .WhereIf(!statementNo.IsNullOrEmpty(), x => x.StatementNo == statementNo)
            .WhereIf(
                !filter.IsNullOrEmpty(),
                x => (x.BankAccountNo != null && x.BankAccountNo.ToLower().Contains(filter))
                    || (x.StatementNo != null && x.StatementNo.ToLower().Contains(filter))
                    || (x.DocumentNo != null && x.DocumentNo.ToLower().Contains(filter))
                    || (x.Description != null && x.Description.ToLower().Contains(filter))
                    || (x.CheckNo != null && x.CheckNo.ToLower().Contains(filter))
                    || (x.TransactionId != null && x.TransactionId.ToLower().Contains(filter))
            );
    }

    protected override IQueryable<BankAccountStatementLine> ApplyDefaultSorting(IQueryable<BankAccountStatementLine> query) =>
        query.OrderBy(x => x.BankAccountNo).ThenBy(x => x.StatementNo).ThenBy(x => x.StatementLineNo);
}

/// <summary>Check Ledger Entries.</summary>
public class CheckLedgerEntryAppService : ErpReadOnlyAppService<CheckLedgerEntry, CheckLedgerEntryDto, Guid, GetCheckLedgerEntryListInput>, ICheckLedgerEntryAppService
{
    public CheckLedgerEntryAppService(IRepository<CheckLedgerEntry, Guid> repository)
        : base(repository)
    {
        GetPolicyName = ErpPermissions.BankAccounts.Default;
        GetListPolicyName = ErpPermissions.BankAccounts.Default;
    }

    protected override async Task<IQueryable<CheckLedgerEntry>> CreateFilteredQueryAsync(GetCheckLedgerEntryListInput input)
    {
        var query = await base.CreateFilteredQueryAsync(input);
        var filter = input.Filter?.Trim().ToLower();
        var bankAccountNo = input.BankAccountNo?.Trim().ToUpperInvariant();
        var documentNo = input.DocumentNo?.Trim().ToUpperInvariant();

        return query
            .WhereIf(!bankAccountNo.IsNullOrEmpty(), x => x.BankAccountNo == bankAccountNo)
            .WhereIf(!documentNo.IsNullOrEmpty(), x => x.DocumentNo == documentNo)
            .WhereIf(
                !filter.IsNullOrEmpty(),
                x => (x.BankAccountNo != null && x.BankAccountNo.ToLower().Contains(filter))
                    || (x.DocumentNo != null && x.DocumentNo.ToLower().Contains(filter))
                    || (x.Description != null && x.Description.ToLower().Contains(filter))
                    || (x.CheckNo != null && x.CheckNo.ToLower().Contains(filter))
                    || (x.BalAccountNo != null && x.BalAccountNo.ToLower().Contains(filter))
                    || (x.StatementNo != null && x.StatementNo.ToLower().Contains(filter))
            );
    }

    protected override IQueryable<CheckLedgerEntry> ApplyDefaultSorting(IQueryable<CheckLedgerEntry> query) =>
        query.OrderByDescending(x => x.EntryNo);
}
