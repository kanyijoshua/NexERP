using System;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.CashManagement;
using ABPmicroservice.Erp.Companies;
using ABPmicroservice.Erp.Numbering;
using ABPmicroservice.Erp.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;

namespace ABPmicroservice.Erp.Academics;

/// <summary>What the documents about one student share: finding the student and numbering the document.</summary>
public abstract class StudentDocumentAppService<TEntity, TDto, TInput> : ErpTableAppService<TEntity, TDto, GetStudentDocumentListInput, TInput>
    where TEntity : class, Volo.Abp.Domain.Entities.IEntity<Guid>, IHasNo
    where TDto : Volo.Abp.Application.Dtos.IEntityDto<Guid>
{
    protected AcademicSetupManager SetupManager => LazyServiceProvider.LazyGetRequiredService<AcademicSetupManager>();

    protected NoSeriesManager NoSeriesManager => LazyServiceProvider.LazyGetRequiredService<NoSeriesManager>();

    protected IRepository<Student, Guid> Students => LazyServiceProvider.LazyGetRequiredService<IRepository<Student, Guid>>();

    /// <summary>The table's caption in "already exists" and "not found" messages.</summary>
    protected abstract string Caption { get; }

    protected StudentDocumentAppService(IRepository<TEntity, Guid> repository)
        : base(repository, ErpPermissions.Academics.Default) { }

    protected async Task<Student> GetStudentAsync(string studentNo)
    {
        var no = CodeTableEntity.NormalizeCode(studentNo);
        return await Students.FirstOrDefaultAsync(s => s.No == no)
            ?? throw new BusinessException(ErpErrorCodes.PostingSetup.CodeNotFound).WithData("table", "Student").WithData("code", no ?? string.Empty);
    }

    /// <summary>The number of a new document: the one typed, or the next of the series, and not one already used.</summary>
    protected async Task<string> ResolveNoAsync(string seriesCode, string typedNo, DateTime date)
    {
        var no = (await NoSeriesManager.ResolveNoAsync(seriesCode, typedNo, date)).ToUpperInvariant();
        if (await Repository.AnyAsync(x => x.No == no))
        {
            throw new BusinessException(ErpErrorCodes.BaseTables.RecordAlreadyExists).WithData("table", Caption).WithData("key", no);
        }

        return no;
    }
}

/// <summary>Fee receipts: entered and posted to the bank account and the student's account.</summary>
public class StudentReceiptAppService
    : StudentDocumentAppService<StudentReceipt, StudentReceiptDto, CreateUpdateStudentReceiptDto>,
        IStudentReceiptAppService
{
    private readonly StudentAccountEngine _engine;

    public StudentReceiptAppService(IRepository<StudentReceipt, Guid> repository, StudentAccountEngine engine)
        : base(repository)
    {
        _engine = engine;
    }

    protected override string Caption => "Student Receipt";

    public override async Task<StudentReceiptDto> CreateAsync(CreateUpdateStudentReceiptDto input)
    {
        await CheckCreatePolicyAsync();

        var student = await GetStudentAsync(input.StudentNo);
        var date = input.PostingDate == default ? Clock.Now.Date : input.PostingDate;
        var no = await ResolveNoAsync((await SetupManager.GetAsync()).ReceiptNos, input.No, date);

        var receipt = new StudentReceipt(GuidGenerator.Create(), no, student, date, input.Amount);
        await ApplyAsync(receipt, student, input);

        await Repository.InsertAsync(receipt, autoSave: true);
        return await MapToGetOutputDtoAsync(receipt);
    }

    public override async Task<StudentReceiptDto> UpdateAsync(Guid id, CreateUpdateStudentReceiptDto input)
    {
        await CheckUpdatePolicyAsync();

        var receipt = await GetEntityByIdAsync(id);
        receipt.EnsureOpen();

        var student = await GetStudentAsync(input.StudentNo);
        receipt.SetStudent(student);
        receipt.SetAmount(input.PostingDate == default ? receipt.PostingDate : input.PostingDate, input.Amount);
        await ApplyAsync(receipt, student, input);

        await Repository.UpdateAsync(receipt, autoSave: true);
        return await MapToGetOutputDtoAsync(receipt);
    }

    /// <summary>A posted receipt is the source of ledger entries and stays.</summary>
    public override async Task DeleteAsync(Guid id)
    {
        await CheckDeletePolicyAsync();

        (await GetEntityByIdAsync(id)).EnsureOpen();
        await Repository.DeleteAsync(id, autoSave: true);
    }

    [Authorize(ErpPermissions.Academics.Post)]
    public async Task<StudentReceiptDto> RunPostingAsync(Guid id)
    {
        var receipt = await GetEntityByIdAsync(id);
        await _engine.PostReceiptAsync(receipt);
        return await MapToGetOutputDtoAsync(receipt);
    }

    protected override async Task<IQueryable<StudentReceipt>> CreateFilteredQueryAsync(GetStudentDocumentListInput input)
    {
        var query = await base.CreateFilteredQueryAsync(input);
        var filter = input.Filter?.Trim().ToLower();
        var student = input.StudentNo?.Trim().ToUpperInvariant();

        return query
            .WhereIf(!student.IsNullOrEmpty(), x => x.StudentNo == student)
            .WhereIf(
                !filter.IsNullOrEmpty(),
                x => x.No.ToLower().Contains(filter)
                    || x.StudentNo.ToLower().Contains(filter)
                    || (x.StudentName != null && x.StudentName.ToLower().Contains(filter))
                    || (x.ExternalDocumentNo != null && x.ExternalDocumentNo.ToLower().Contains(filter))
            );
    }

    protected override IQueryable<StudentReceipt> ApplyDefaultSorting(IQueryable<StudentReceipt> query) => query.OrderByDescending(x => x.No);

    private async Task ApplyAsync(StudentReceipt receipt, Student student, CreateUpdateStudentReceiptDto input)
    {
        await Relations.EnsureNoExistsAsync<BankAccount>(input.BankAccountNo);
        await CodeTableChecker.EnsureExistsAsync<PaymentMethod>(input.PayMode);
        await _engine.EnsureBillOfStudentAsync(input.AppliesToBillNo, student.No);

        receipt.SetPayment(input.BankAccountNo, input.PayMode, input.ExternalDocumentNo, input.AppliesToBillNo, input.Description);
    }
}

/// <summary>Refunds of what students have paid ahead. Approving one raises its payment voucher.</summary>
public class StudentRefundAppService
    : StudentDocumentAppService<StudentRefund, StudentRefundDto, CreateUpdateStudentRefundDto>,
        IStudentRefundAppService
{
    private readonly StudentAccountEngine _engine;

    public StudentRefundAppService(IRepository<StudentRefund, Guid> repository, StudentAccountEngine engine)
        : base(repository)
    {
        _engine = engine;
    }

    protected override string Caption => "Student Refund";

    public override async Task<StudentRefundDto> CreateAsync(CreateUpdateStudentRefundDto input)
    {
        await CheckCreatePolicyAsync();

        var student = await GetStudentAsync(input.StudentNo);
        var date = input.DocumentDate == default ? Clock.Now.Date : input.DocumentDate;
        var no = await ResolveNoAsync((await SetupManager.GetAsync()).RefundNos, input.No, date);

        var refund = new StudentRefund(GuidGenerator.Create(), no, student, date, input.Amount);
        refund.Set(student, date, input.Amount, input.Reason);

        await Repository.InsertAsync(refund, autoSave: true);
        return await MapToGetOutputDtoAsync(refund);
    }

    public override async Task<StudentRefundDto> UpdateAsync(Guid id, CreateUpdateStudentRefundDto input)
    {
        await CheckUpdatePolicyAsync();

        var refund = await GetEntityByIdAsync(id);
        var student = await GetStudentAsync(input.StudentNo);
        refund.Set(student, input.DocumentDate == default ? refund.DocumentDate : input.DocumentDate, input.Amount, input.Reason);

        await Repository.UpdateAsync(refund, autoSave: true);
        return await MapToGetOutputDtoAsync(refund);
    }

    /// <summary>An approved refund has a payment voucher and stays.</summary>
    public override async Task DeleteAsync(Guid id)
    {
        await CheckDeletePolicyAsync();

        (await GetEntityByIdAsync(id)).EnsureOpen();
        await Repository.DeleteAsync(id, autoSave: true);
    }

    [Authorize(ErpPermissions.Academics.Post)]
    public async Task<StudentRefundDto> ApproveAsync(Guid id)
    {
        var refund = await GetEntityByIdAsync(id);
        await _engine.ApproveRefundAsync(refund);
        return await MapToGetOutputDtoAsync(refund);
    }

    protected override async Task<IQueryable<StudentRefund>> CreateFilteredQueryAsync(GetStudentDocumentListInput input)
    {
        var query = await base.CreateFilteredQueryAsync(input);
        var filter = input.Filter?.Trim().ToLower();
        var student = input.StudentNo?.Trim().ToUpperInvariant();

        return query
            .WhereIf(!student.IsNullOrEmpty(), x => x.StudentNo == student)
            .WhereIf(
                !filter.IsNullOrEmpty(),
                x => x.No.ToLower().Contains(filter)
                    || x.StudentNo.ToLower().Contains(filter)
                    || (x.StudentName != null && x.StudentName.ToLower().Contains(filter))
                    || (x.PaymentVoucherNo != null && x.PaymentVoucherNo.ToLower().Contains(filter))
            );
    }

    protected override IQueryable<StudentRefund> ApplyDefaultSorting(IQueryable<StudentRefund> query) => query.OrderByDescending(x => x.No);
}

/// <summary>Changes of a student's standing: deferment, readmission, suspension, discontinuation and graduation.</summary>
public class StudentStatusChangeAppService
    : StudentDocumentAppService<StudentStatusChange, StudentStatusChangeDto, CreateUpdateStudentStatusChangeDto>,
        IStudentStatusChangeAppService
{
    private readonly StudentStatusChangeManager _manager;

    public StudentStatusChangeAppService(IRepository<StudentStatusChange, Guid> repository, StudentStatusChangeManager manager)
        : base(repository)
    {
        _manager = manager;
    }

    protected override string Caption => "Student Status Change";

    public override async Task<StudentStatusChangeDto> CreateAsync(CreateUpdateStudentStatusChangeDto input)
    {
        await CheckCreatePolicyAsync();

        var student = await GetStudentAsync(input.StudentNo);
        var date = input.EffectiveDate == default ? Clock.Now.Date : input.EffectiveDate;
        var no = await ResolveNoAsync((await SetupManager.GetAsync()).StatusChangeNos, input.No, date);

        var change = new StudentStatusChange(GuidGenerator.Create(), no, student, input.ChangeType, date);
        change.Set(student, input.ChangeType, date, input.ResumeDate, input.Reason);

        await Repository.InsertAsync(change, autoSave: true);
        return await MapToGetOutputDtoAsync(change);
    }

    public override async Task<StudentStatusChangeDto> UpdateAsync(Guid id, CreateUpdateStudentStatusChangeDto input)
    {
        await CheckUpdatePolicyAsync();

        var change = await GetEntityByIdAsync(id);
        var student = await GetStudentAsync(input.StudentNo);
        change.Set(student, input.ChangeType, input.EffectiveDate == default ? change.EffectiveDate : input.EffectiveDate, input.ResumeDate, input.Reason);

        await Repository.UpdateAsync(change, autoSave: true);
        return await MapToGetOutputDtoAsync(change);
    }

    /// <summary>An approved change is part of the student's history and stays.</summary>
    public override async Task DeleteAsync(Guid id)
    {
        await CheckDeletePolicyAsync();

        (await GetEntityByIdAsync(id)).EnsureOpen();
        await Repository.DeleteAsync(id, autoSave: true);
    }

    [Authorize(ErpPermissions.Academics.Post)]
    public async Task<StudentStatusChangeDto> ApproveAsync(Guid id)
    {
        var change = await GetEntityByIdAsync(id);
        await _manager.ApproveAsync(change);
        return await MapToGetOutputDtoAsync(change);
    }

    protected override async Task<IQueryable<StudentStatusChange>> CreateFilteredQueryAsync(GetStudentDocumentListInput input)
    {
        var query = await base.CreateFilteredQueryAsync(input);
        var filter = input.Filter?.Trim().ToLower();
        var student = input.StudentNo?.Trim().ToUpperInvariant();

        return query
            .WhereIf(!student.IsNullOrEmpty(), x => x.StudentNo == student)
            .WhereIf(
                !filter.IsNullOrEmpty(),
                x => x.No.ToLower().Contains(filter)
                    || x.StudentNo.ToLower().Contains(filter)
                    || (x.StudentName != null && x.StudentName.ToLower().Contains(filter))
            );
    }

    protected override IQueryable<StudentStatusChange> ApplyDefaultSorting(IQueryable<StudentStatusChange> query) => query.OrderByDescending(x => x.No);
}
