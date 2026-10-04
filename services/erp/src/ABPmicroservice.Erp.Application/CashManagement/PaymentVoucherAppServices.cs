using System;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Companies;
using ABPmicroservice.Erp.Documents;
using ABPmicroservice.Erp.Finance;
using ABPmicroservice.Erp.Numbering;
using ABPmicroservice.Erp.Permissions;
using ABPmicroservice.Erp.Workflows;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Authorization;
using Volo.Abp.Domain.Repositories;

namespace ABPmicroservice.Erp.CashManagement;

/// <summary>Cash Management Setup: one record per company, created on first read.</summary>
[Authorize(ErpPermissions.PaymentVoucherSetup.Default)]
public class CashManagementSetupAppService : ErpAppService, ICashManagementSetupAppService
{
    private readonly CashManagementSetupManager _setupManager;
    private readonly IRepository<CashManagementSetup, Guid> _repository;
    private readonly NoSeriesCodeValidator _seriesValidator;

    public CashManagementSetupAppService(
        CashManagementSetupManager setupManager,
        IRepository<CashManagementSetup, Guid> repository,
        NoSeriesCodeValidator seriesValidator
    )
    {
        _setupManager = setupManager;
        _repository = repository;
        _seriesValidator = seriesValidator;
    }

    public async Task<CashManagementSetupDto> GetAsync()
    {
        return ObjectMapper.Map<CashManagementSetup, CashManagementSetupDto>(await _setupManager.GetAsync());
    }

    [Authorize(ErpPermissions.PaymentVoucherSetup.Update)]
    public async Task<CashManagementSetupDto> UpdateAsync(CashManagementSetupDto input)
    {
        await _seriesValidator.EnsureExistAsync(input.PaymentVoucherNos);

        var setup = await _setupManager.GetAsync();
        setup.SetNumbering(input.PaymentVoucherNos);

        await _repository.UpdateAsync(setup, autoSave: true);
        return ObjectMapper.Map<CashManagementSetup, CashManagementSetupDto>(setup);
    }
}

/// <summary>Deduction codes: withholding tax, withholding VAT and retention, with their rates and payable accounts.</summary>
public class PaymentDeductionCodeAppService
    : CodeTableAppServiceBase<PaymentDeductionCode, PaymentDeductionCodeDto, CreateUpdatePaymentDeductionCodeDto>,
        IPaymentDeductionCodeAppService
{
    private readonly TableRelationChecker _relations;
    private readonly IRepository<PaymentType, Guid> _paymentTypes;
    private readonly IRepository<PaymentVoucherLine, Guid> _lines;

    public PaymentDeductionCodeAppService(
        IRepository<PaymentDeductionCode, Guid> repository,
        TableRelationChecker relations,
        IRepository<PaymentType, Guid> paymentTypes,
        IRepository<PaymentVoucherLine, Guid> lines
    )
        : base(repository, ErpPermissions.PaymentVoucherSetup.Default)
    {
        _relations = relations;
        _paymentTypes = paymentTypes;
        _lines = lines;
    }

    protected override PaymentDeductionCode NewEntity(Guid id, CreateUpdatePaymentDeductionCodeDto input) => new(id, input.Code, input.Description);

    protected override async Task ApplyAsync(PaymentDeductionCode entity, CreateUpdatePaymentDeductionCodeDto input)
    {
        await _relations.EnsureGLAccountsExistAsync(input.PayableAccountNo);
        entity.Set(input.DeductionType, input.RatePct, input.PayableAccountNo);
    }

    /// <summary>A code that payment types or voucher lines name stays, so that what was withheld can still be explained.</summary>
    public override async Task DeleteAsync(Guid id)
    {
        await CheckDeletePolicyAsync();

        var code = (await Repository.GetAsync(id)).Code;
        if (await _paymentTypes.AnyAsync(t => t.WithholdingTaxCode == code || t.WithholdingVatCode == code || t.RetentionCode == code)
            || await _lines.AnyAsync(l => l.WithholdingTaxCode == code || l.WithholdingVatCode == code || l.RetentionCode == code))
        {
            throw new BusinessException(ErpErrorCodes.CashManagement.DeductionCodeInUse).WithData("code", code);
        }

        await Repository.DeleteAsync(id, autoSave: true);
    }
}

/// <summary>Payment types: the kinds of payment a voucher line can be, with the account and deductions each normally carries.</summary>
public class PaymentTypeAppService
    : CodeTableAppServiceBase<PaymentType, PaymentTypeDto, CreateUpdatePaymentTypeDto>,
        IPaymentTypeAppService
{
    private readonly PaymentVoucherEngine _engine;

    public PaymentTypeAppService(IRepository<PaymentType, Guid> repository, PaymentVoucherEngine engine)
        : base(repository, ErpPermissions.PaymentVoucherSetup.Default)
    {
        _engine = engine;
    }

    protected override PaymentType NewEntity(Guid id, CreateUpdatePaymentTypeDto input) => new(id, input.Code, input.Description);

    protected override async Task ApplyAsync(PaymentType entity, CreateUpdatePaymentTypeDto input)
    {
        if (!input.AccountNo.IsNullOrWhiteSpace())
        {
            await _engine.GetAccountNameAsync(input.AccountType, input.AccountNo);
        }

        await EnsureTypeAsync(input.WithholdingTaxCode, PaymentDeductionType.WithholdingTax);
        await EnsureTypeAsync(input.WithholdingVatCode, PaymentDeductionType.WithholdingVat);
        await EnsureTypeAsync(input.RetentionCode, PaymentDeductionType.Retention);

        entity.SetAccount(input.AccountType, input.AccountNo);
        entity.SetDeductions(input.VatRatePct, input.WithholdingTaxCode, input.WithholdingVatCode, input.RetentionCode);
        entity.SetBlocked(input.Blocked);
    }

    private async Task EnsureTypeAsync(string code, PaymentDeductionType expected)
    {
        var deduction = await _engine.GetDeductionCodeAsync(code);
        if (deduction != null && deduction.DeductionType != expected)
        {
            throw new BusinessException(ErpErrorCodes.CashManagement.WrongDeductionType).WithData("code", deduction.Code).WithData("type", expected.ToString());
        }
    }
}

/// <summary>Payment vouchers: prepared, approved or released, and posted.</summary>
public class PaymentVoucherAppService
    : ErpTableAppService<PaymentVoucherHeader, PaymentVoucherHeaderDto, GetPaymentVoucherListInput, CreateUpdatePaymentVoucherHeaderDto>,
        IPaymentVoucherAppService
{
    private const ApprovalDocumentKind ApprovalKind = ApprovalDocumentKind.PaymentVoucher;

    private readonly PaymentVoucherEngine _engine;
    private readonly CashManagementSetupManager _setupManager;
    private readonly NoSeriesManager _noSeriesManager;
    private readonly ApprovalsManager _approvalsManager;
    private readonly IRepository<PaymentVoucherLine, Guid> _lines;

    public PaymentVoucherAppService(
        IRepository<PaymentVoucherHeader, Guid> repository,
        PaymentVoucherEngine engine,
        CashManagementSetupManager setupManager,
        NoSeriesManager noSeriesManager,
        ApprovalsManager approvalsManager,
        IRepository<PaymentVoucherLine, Guid> lines
    )
        : base(repository, ErpPermissions.PaymentVouchers.Default)
    {
        _engine = engine;
        _setupManager = setupManager;
        _noSeriesManager = noSeriesManager;
        _approvalsManager = approvalsManager;
        _lines = lines;
    }

    public override async Task<PaymentVoucherHeaderDto> CreateAsync(CreateUpdatePaymentVoucherHeaderDto input)
    {
        await CheckCreatePolicyAsync();

        var setup = await _setupManager.GetAsync();
        if (setup.PaymentVoucherNos.IsNullOrWhiteSpace() && input.No.IsNullOrWhiteSpace())
        {
            throw new BusinessException(ErpErrorCodes.CashManagement.VoucherNosMissing);
        }

        var documentDate = input.DocumentDate == default ? Clock.Now.Date : input.DocumentDate;
        var no = (await _noSeriesManager.ResolveNoAsync(setup.PaymentVoucherNos, input.No, documentDate)).ToUpperInvariant();

        if (await Repository.AnyAsync(x => x.No == no))
        {
            throw new BusinessException(ErpErrorCodes.BaseTables.RecordAlreadyExists).WithData("table", "Payment Voucher").WithData("key", no);
        }

        var header = new PaymentVoucherHeader(GuidGenerator.Create(), no, documentDate, input.PostingDate == default ? documentDate : input.PostingDate);
        await ApplyAsync(header, input);

        await Repository.InsertAsync(header, autoSave: true);
        return await MapToGetOutputDtoAsync(header);
    }

    public override async Task<PaymentVoucherHeaderDto> UpdateAsync(Guid id, CreateUpdatePaymentVoucherHeaderDto input)
    {
        await CheckUpdatePolicyAsync();

        var header = await GetEntityByIdAsync(id);
        header.EnsureOpen();

        var documentDate = input.DocumentDate == default ? header.DocumentDate : input.DocumentDate;
        header.SetDates(documentDate, input.PostingDate == default ? documentDate : input.PostingDate);
        await ApplyAsync(header, input);

        await Repository.UpdateAsync(header, autoSave: true);
        return await MapToGetOutputDtoAsync(header);
    }

    /// <summary>Only an open voucher can be deleted, and it goes with its lines.</summary>
    public override async Task DeleteAsync(Guid id)
    {
        await CheckDeletePolicyAsync();

        var header = await GetEntityByIdAsync(id);
        header.EnsureOpen();

        await _lines.DeleteAsync(l => l.DocumentNo == header.No, autoSave: true);
        await Repository.DeleteAsync(id, autoSave: true);
    }

    [Authorize(ErpPermissions.PaymentVouchers.Update)]
    public async Task<ApprovalRequestResultDto> SendApprovalRequestAsync(Guid id)
    {
        var header = await GetEntityByIdAsync(id);
        await _engine.UpdateTotalsAsync(header);
        await _engine.CheckAsync(header);

        var result = await _approvalsManager.SendApprovalRequestAsync(ApprovalKind, id, GetUserId());

        return new ApprovalRequestResultDto
        {
            AutoApproved = result.AutoApproved,
            ApproverCount = result.ApproverCount,
            FirstApproverUserName = result.FirstApproverUserName,
        };
    }

    [Authorize(ErpPermissions.PaymentVouchers.Update)]
    public async Task CancelApprovalRequestAsync(Guid id)
    {
        await _approvalsManager.CancelApprovalRequestAsync(ApprovalKind, id, GetUserId());
    }

    [Authorize(ErpPermissions.PaymentVouchers.Update)]
    public async Task<PaymentVoucherHeaderDto> ReleaseAsync(Guid id)
    {
        var header = await GetEntityByIdAsync(id);
        await _engine.UpdateTotalsAsync(header);

        // With an approval workflow in force, only a completed approval releases the voucher.
        await _approvalsManager.EnsureCanReleaseAsync(ApprovalKind, header);
        await _engine.ReleaseAsync(header);
        return await MapToGetOutputDtoAsync(header);
    }

    [Authorize(ErpPermissions.PaymentVouchers.Update)]
    public async Task<PaymentVoucherHeaderDto> ReopenAsync(Guid id)
    {
        var header = await GetEntityByIdAsync(id);

        // A pending request has to be canceled, not bypassed by reopening.
        if (header.Status == DocumentStatus.PendingApproval)
        {
            throw new BusinessException(ErpErrorCodes.Approvals.PendingApproval).WithData("documentNo", header.No);
        }

        header.Reopen();
        await Repository.UpdateAsync(header, autoSave: true);
        return await MapToGetOutputDtoAsync(header);
    }

    [Authorize(ErpPermissions.PaymentVouchers.Update)]
    public async Task<PaymentVoucherHeaderDto> RecordChequeAsync(Guid id, PaymentVoucherChequeInput input)
    {
        var header = await GetEntityByIdAsync(id);
        header.SetCheque(input?.ChequeNo, input?.ChequeDate);

        await Repository.UpdateAsync(header, autoSave: true);
        return await MapToGetOutputDtoAsync(header);
    }

    [Authorize(ErpPermissions.PaymentVouchers.Post)]
    public async Task<PaymentVoucherHeaderDto> RunPostingAsync(Guid id)
    {
        var header = await GetEntityByIdAsync(id);

        // Posting releases an open voucher first, so the same approval rule applies.
        if (header.Status == DocumentStatus.Open)
        {
            await _engine.UpdateTotalsAsync(header);
            await _approvalsManager.EnsureCanReleaseAsync(ApprovalKind, header);
            await _engine.ReleaseAsync(header);
        }

        await _engine.PostAsync(header);
        return await MapToGetOutputDtoAsync(header);
    }

    protected override async Task<IQueryable<PaymentVoucherHeader>> CreateFilteredQueryAsync(GetPaymentVoucherListInput input)
    {
        var query = await base.CreateFilteredQueryAsync(input);
        var filter = input.Filter?.Trim().ToLower();
        var bank = input.PayingBankAccountNo?.Trim();

        return query
            .WhereIf(!bank.IsNullOrEmpty(), x => x.PayingBankAccountNo == bank)
            .WhereIf(input.Status.HasValue, x => x.Status == input.Status.Value)
            .WhereIf(
                !filter.IsNullOrEmpty(),
                x => x.No.ToLower().Contains(filter)
                    || (x.Payee != null && x.Payee.ToLower().Contains(filter))
                    || (x.ChequeNo != null && x.ChequeNo.ToLower().Contains(filter))
                    || (x.SourceNo != null && x.SourceNo.ToLower().Contains(filter))
            );
    }

    protected override IQueryable<PaymentVoucherHeader> ApplyDefaultSorting(IQueryable<PaymentVoucherHeader> query) => query.OrderByDescending(x => x.No);

    private async Task ApplyAsync(PaymentVoucherHeader header, CreateUpdatePaymentVoucherHeaderDto input)
    {
        await CodeTableChecker.EnsureExistsAsync<PaymentMethod>(input.PayMode);
        await _engine.SetPaymentAsync(header, input.PayMode, input.PayingBankAccountNo);
        header.SetPayee(input.Payee, input.OnBehalfOf, input.PaymentNarration);
        header.SetCheque(input.ChequeNo, input.ChequeDate);
    }

    private Guid GetUserId()
    {
        return CurrentUser.Id ?? throw new AbpAuthorizationException();
    }
}

/// <summary>The lines of payment vouchers. They can be changed only while their voucher is open.</summary>
public class PaymentVoucherLineAppService
    : ErpTableAppService<PaymentVoucherLine, PaymentVoucherLineDto, GetPaymentVoucherLineListInput, CreateUpdatePaymentVoucherLineDto>,
        IPaymentVoucherLineAppService
{
    private readonly PaymentVoucherEngine _engine;
    private readonly IRepository<PaymentVoucherHeader, Guid> _headers;
    private readonly IRepository<PaymentType, Guid> _paymentTypes;

    public PaymentVoucherLineAppService(
        IRepository<PaymentVoucherLine, Guid> repository,
        PaymentVoucherEngine engine,
        IRepository<PaymentVoucherHeader, Guid> headers,
        IRepository<PaymentType, Guid> paymentTypes
    )
        : base(repository, ErpPermissions.PaymentVouchers.Default)
    {
        _engine = engine;
        _headers = headers;
        _paymentTypes = paymentTypes;
    }

    public override async Task<PaymentVoucherLineDto> CreateAsync(CreateUpdatePaymentVoucherLineDto input)
    {
        await CheckCreatePolicyAsync();

        var header = await GetOpenHeaderAsync(input.DocumentNo);

        var lineNo = input.LineNo;
        if (lineNo <= 0)
        {
            var query = await Repository.GetQueryableAsync();
            var last = await AsyncExecuter.FirstOrDefaultAsync(query.Where(x => x.DocumentNo == header.No).OrderByDescending(x => x.LineNo));
            lineNo = (last?.LineNo ?? 0) + 10000;
        }

        var line = new PaymentVoucherLine(GuidGenerator.Create(), header.No, lineNo);
        await ApplyAsync(line, input);

        await Repository.InsertAsync(line, autoSave: true);
        await _engine.UpdateTotalsAsync(header);
        return await MapToGetOutputDtoAsync(line);
    }

    public override async Task<PaymentVoucherLineDto> UpdateAsync(Guid id, CreateUpdatePaymentVoucherLineDto input)
    {
        await CheckUpdatePolicyAsync();

        var line = await GetEntityByIdAsync(id);
        var header = await GetOpenHeaderAsync(line.DocumentNo);
        await ApplyAsync(line, input);

        await Repository.UpdateAsync(line, autoSave: true);
        await _engine.UpdateTotalsAsync(header);
        return await MapToGetOutputDtoAsync(line);
    }

    public override async Task DeleteAsync(Guid id)
    {
        await CheckDeletePolicyAsync();

        var line = await GetEntityByIdAsync(id);
        var header = await GetOpenHeaderAsync(line.DocumentNo);

        await Repository.DeleteAsync(id, autoSave: true);
        await _engine.UpdateTotalsAsync(header);
    }

    protected override async Task<IQueryable<PaymentVoucherLine>> CreateFilteredQueryAsync(GetPaymentVoucherLineListInput input)
    {
        var query = await base.CreateFilteredQueryAsync(input);
        var filter = input.Filter?.Trim().ToLower();
        var document = input.DocumentNo?.Trim().ToUpperInvariant();

        return query
            .WhereIf(!document.IsNullOrEmpty(), x => x.DocumentNo == document)
            .WhereIf(
                !filter.IsNullOrEmpty(),
                x => x.DocumentNo.ToLower().Contains(filter)
                    || x.AccountNo.ToLower().Contains(filter)
                    || (x.AccountName != null && x.AccountName.ToLower().Contains(filter))
                    || (x.Description != null && x.Description.ToLower().Contains(filter))
            );
    }

    protected override IQueryable<PaymentVoucherLine> ApplyDefaultSorting(IQueryable<PaymentVoucherLine> query) =>
        query.OrderBy(x => x.DocumentNo).ThenBy(x => x.LineNo);

    /// <summary>Fills the line from the input, with the payment type supplying whatever the input leaves blank.</summary>
    private async Task ApplyAsync(PaymentVoucherLine line, CreateUpdatePaymentVoucherLineDto input)
    {
        var typeCode = CodeTableEntity.NormalizeCode(input.PaymentTypeCode);
        PaymentType type = null;
        if (typeCode != null)
        {
            type = await _paymentTypes.FirstOrDefaultAsync(t => t.Code == typeCode)
                ?? throw new BusinessException(ErpErrorCodes.PostingSetup.CodeNotFound).WithData("table", "Payment Type").WithData("code", typeCode);

            if (type.Blocked)
            {
                throw new BusinessException(ErpErrorCodes.CashManagement.PaymentTypeBlocked).WithData("code", type.Code);
            }
        }

        var accountNo = input.AccountNo.IsNullOrWhiteSpace() ? type?.AccountNo : input.AccountNo;
        var accountType = input.AccountNo.IsNullOrWhiteSpace() && type?.AccountNo != null
            ? type.AccountType
            : input.AccountType ?? type?.AccountType ?? GenJournalAccountType.GLAccount;

        if (accountNo.IsNullOrWhiteSpace())
        {
            throw new BusinessException(ErpErrorCodes.CashManagement.VoucherFieldMissing).WithData("field", "Account No.").WithData("documentNo", line.DocumentNo);
        }

        line.SetAccount(typeCode, accountType, accountNo, await _engine.GetAccountNameAsync(accountType, accountNo));
        line.SetDetails(input.Description.IsNullOrWhiteSpace() ? type?.Description : input.Description, input.AppliesToDocNo);
        line.SetAmounts(
            input.Amount,
            input.VatRatePct ?? type?.VatRatePct ?? 0m,
            await _engine.GetDeductionCodeAsync(input.WithholdingTaxCode ?? type?.WithholdingTaxCode),
            await _engine.GetDeductionCodeAsync(input.WithholdingVatCode ?? type?.WithholdingVatCode),
            await _engine.GetDeductionCodeAsync(input.RetentionCode ?? type?.RetentionCode)
        );
    }

    private async Task<PaymentVoucherHeader> GetOpenHeaderAsync(string documentNo)
    {
        var no = CodeTableEntity.NormalizeCode(documentNo);
        var header = await _headers.FirstOrDefaultAsync(h => h.No == no)
            ?? throw new BusinessException(ErpErrorCodes.PostingSetup.CodeNotFound).WithData("table", "Payment Voucher").WithData("code", no ?? string.Empty);

        header.EnsureOpen();
        return header;
    }
}
