using System;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Companies;
using ABPmicroservice.Erp.Permissions;
using ABPmicroservice.Erp.Sequences;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;

namespace ABPmicroservice.Erp.HumanResources;

[Authorize(ErpPermissions.Recruitment.Default)]
public class JobVacancyAppService : ErpTableAppService<JobVacancy, JobVacancyDto, GetRecruitmentListInput, CreateUpdateJobVacancyDto>, IJobVacancyAppService
{
    private RecruitmentManager Manager => LazyServiceProvider.LazyGetRequiredService<RecruitmentManager>();

    public JobVacancyAppService(IRepository<JobVacancy, Guid> repository)
        : base(repository, ErpPermissions.Recruitment.Default) { }

    public override async Task<JobVacancyDto> CreateAsync(CreateUpdateJobVacancyDto input)
    {
        await CheckCreatePolicyAsync();
        var no = input.No.IsNullOrWhiteSpace() ? await Manager.NextNoAsync("VAC", ErpSequenceNames.VacancyNo) : input.No.Trim().ToUpperInvariant();
        await EnsureNewAsync(no);
        var vacancy = new JobVacancy(GuidGenerator.Create(), no);
        vacancy.Set(input.Title, input.Department, input.Description, input.Positions, input.OpeningDate, input.ClosingDate);
        await Repository.InsertAsync(vacancy, autoSave: true);
        return await MapToGetOutputDtoAsync(vacancy);
    }

    public override async Task<JobVacancyDto> UpdateAsync(Guid id, CreateUpdateJobVacancyDto input)
    {
        await CheckUpdatePolicyAsync();
        var vacancy = await GetEntityByIdAsync(id);
        vacancy.Set(input.Title, input.Department, input.Description, input.Positions, input.OpeningDate, input.ClosingDate);
        await Repository.UpdateAsync(vacancy, autoSave: true);
        return await MapToGetOutputDtoAsync(vacancy);
    }

    [Authorize(ErpPermissions.Recruitment.Update)]
    public async Task<JobVacancyDto> OpenAsync(Guid id) => await ChangeAsync(id, v => v.Open());

    [Authorize(ErpPermissions.Recruitment.Update)]
    public async Task<JobVacancyDto> CloseAsync(Guid id) => await ChangeAsync(id, v => v.Close());

    private async Task<JobVacancyDto> ChangeAsync(Guid id, Action<JobVacancy> change)
    {
        var vacancy = await GetEntityByIdAsync(id);
        change(vacancy);
        await Repository.UpdateAsync(vacancy, autoSave: true);
        return await MapToGetOutputDtoAsync(vacancy);
    }

    private async Task EnsureNewAsync(string no)
    {
        if (await Repository.AnyAsync(v => v.No == no))
        {
            throw new BusinessException(ErpErrorCodes.Documents.DocumentNoAlreadyExists).WithData("documentNo", no);
        }
    }

    protected override async Task<IQueryable<JobVacancy>> CreateFilteredQueryAsync(GetRecruitmentListInput input)
    {
        var query = await Repository.GetQueryableAsync();
        var filter = input.Filter?.Trim().ToLower();
        return query.WhereIf(!filter.IsNullOrEmpty(), v => v.No.ToLower().Contains(filter) || v.Title.ToLower().Contains(filter));
    }

    protected override IQueryable<JobVacancy> ApplyDefaultSorting(IQueryable<JobVacancy> query) => query.OrderByDescending(v => v.No);
}

[Authorize(ErpPermissions.Recruitment.Default)]
public class JobApplicantAppService : ErpTableAppService<JobApplicant, JobApplicantDto, GetRecruitmentListInput, CreateUpdateJobApplicantDto>, IJobApplicantAppService
{
    private RecruitmentManager Manager => LazyServiceProvider.LazyGetRequiredService<RecruitmentManager>();

    public JobApplicantAppService(IRepository<JobApplicant, Guid> repository)
        : base(repository, ErpPermissions.Recruitment.Default) { }

    public override async Task<JobApplicantDto> CreateAsync(CreateUpdateJobApplicantDto input)
    {
        await CheckCreatePolicyAsync();
        var vacancyNo = input.VacancyNo.Trim().ToUpperInvariant();
        var vacancy = await LazyServiceProvider.LazyGetRequiredService<IRepository<JobVacancy, Guid>>().FirstOrDefaultAsync(v => v.No == vacancyNo)
            ?? throw new BusinessException(ErpErrorCodes.HumanResources.VacancyNotOpen).WithData("no", vacancyNo);
        vacancy.EnsureOpen();

        var no = input.No.IsNullOrWhiteSpace() ? await Manager.NextNoAsync("APP", ErpSequenceNames.ApplicantNo) : input.No.Trim().ToUpperInvariant();
        if (await Repository.AnyAsync(a => a.No == no))
        {
            throw new BusinessException(ErpErrorCodes.Documents.DocumentNoAlreadyExists).WithData("documentNo", no);
        }

        var applicant = new JobApplicant(GuidGenerator.Create(), no, vacancy.No, input.AppliedDate ?? Clock.Now.Date);
        applicant.Set(input.FirstName, input.LastName, input.Email, input.PhoneNo, input.Rating, input.ExpectedSalary, input.Notes);
        await Repository.InsertAsync(applicant, autoSave: true);
        return await MapToGetOutputDtoAsync(applicant);
    }

    public override async Task<JobApplicantDto> UpdateAsync(Guid id, CreateUpdateJobApplicantDto input)
    {
        await CheckUpdatePolicyAsync();
        var applicant = await GetEntityByIdAsync(id);
        applicant.Set(input.FirstName, input.LastName, input.Email, input.PhoneNo, input.Rating, input.ExpectedSalary, input.Notes);
        await Repository.UpdateAsync(applicant, autoSave: true);
        return await MapToGetOutputDtoAsync(applicant);
    }

    [Authorize(ErpPermissions.Recruitment.Update)]
    public async Task<JobApplicantDto> ChangeStageAsync(Guid id, ChangeApplicantStageInput input)
    {
        var applicant = await GetEntityByIdAsync(id);
        applicant.MoveTo(input.Stage);
        await Repository.UpdateAsync(applicant, autoSave: true);
        return await MapToGetOutputDtoAsync(applicant);
    }

    [Authorize(ErpPermissions.Recruitment.Hire)]
    public async Task<JobApplicantDto> HireAsync(Guid id, HireApplicantInput input)
    {
        await Manager.HireAsync(id, input?.EmploymentDate ?? Clock.Now.Date, input?.EmployeeNo);
        return await MapToGetOutputDtoAsync(await GetEntityByIdAsync(id));
    }

    protected override async Task<IQueryable<JobApplicant>> CreateFilteredQueryAsync(GetRecruitmentListInput input)
    {
        var query = await Repository.GetQueryableAsync();
        var filter = input.Filter?.Trim().ToLower();
        var vacancy = input.VacancyNo?.Trim().ToUpperInvariant();
        return query
            .WhereIf(!vacancy.IsNullOrEmpty(), a => a.VacancyNo == vacancy)
            .WhereIf(!filter.IsNullOrEmpty(), a => a.No.ToLower().Contains(filter) || a.FirstName.ToLower().Contains(filter) || (a.LastName != null && a.LastName.ToLower().Contains(filter)));
    }

    protected override IQueryable<JobApplicant> ApplyDefaultSorting(IQueryable<JobApplicant> query) => query.OrderByDescending(a => a.No);
}

public class AppraisalPeriodAppService : CodeTableAppServiceBase<AppraisalPeriod, AppraisalPeriodDto, CreateUpdateAppraisalPeriodDto>, IAppraisalPeriodAppService
{
    public AppraisalPeriodAppService(IRepository<AppraisalPeriod, Guid> repository)
        : base(repository, ErpPermissions.Appraisals.Default) { }

    protected override AppraisalPeriod NewEntity(Guid id, CreateUpdateAppraisalPeriodDto input) => new(id, input.Code, input.Description);

    protected override Task ApplyAsync(AppraisalPeriod entity, CreateUpdateAppraisalPeriodDto input)
    {
        entity.Set(input.StartingDate, input.EndingDate, input.Closed);
        return Task.CompletedTask;
    }
}

[Authorize(ErpPermissions.Appraisals.Default)]
public class AppraisalAppService : ErpTableAppService<Appraisal, AppraisalDto, GetAppraisalListInput, CreateUpdateAppraisalDto>, IAppraisalAppService
{
    public AppraisalAppService(IRepository<Appraisal, Guid> repository)
        : base(repository, ErpPermissions.Appraisals.Default) { }

    protected override async Task<Appraisal> GetEntityByIdAsync(Guid id) => await Repository.GetAsync(id, includeDetails: true);

    public override async Task<AppraisalDto> CreateAsync(CreateUpdateAppraisalDto input)
    {
        await CheckCreatePolicyAsync();
        var employeeNo = input.EmployeeNo.Trim().ToUpperInvariant();
        var employee = await LazyServiceProvider.LazyGetRequiredService<IRepository<Employee, Guid>>().FirstOrDefaultAsync(e => e.No == employeeNo)
            ?? throw new BusinessException(ErpErrorCodes.HumanResources.EmployeeNotFound).WithData("no", employeeNo);
        var periodCode = CodeTableEntity.NormalizeCode(input.PeriodCode);
        var period = await LazyServiceProvider.LazyGetRequiredService<IRepository<AppraisalPeriod, Guid>>().FirstOrDefaultAsync(p => p.Code == periodCode)
            ?? throw new BusinessException(ErpErrorCodes.PostingSetup.CodeNotFound).WithData("table", "AppraisalPeriod").WithData("code", periodCode ?? "");
        if (period.Closed)
        {
            throw new BusinessException(ErpErrorCodes.HumanResources.AppraisalPeriodClosed).WithData("code", period.Code);
        }

        var no = input.No.IsNullOrWhiteSpace()
            ? $"APR{await LazyServiceProvider.LazyGetRequiredService<IEntryNoGenerator>().NextAsync(ErpSequenceNames.AppraisalNo):D5}"
            : input.No.Trim().ToUpperInvariant();
        var appraisal = new Appraisal(GuidGenerator.Create(), no, employee, period.Code);
        appraisal.Set(input.ReviewerEmployeeNo, input.EmployeeComment, input.ReviewerComment);
        await Repository.InsertAsync(appraisal, autoSave: true);
        return await MapToGetOutputDtoAsync(appraisal);
    }

    public override async Task<AppraisalDto> UpdateAsync(Guid id, CreateUpdateAppraisalDto input)
    {
        await CheckUpdatePolicyAsync();
        var appraisal = await GetEntityByIdAsync(id);
        appraisal.Set(input.ReviewerEmployeeNo, input.EmployeeComment, input.ReviewerComment);
        await Repository.UpdateAsync(appraisal, autoSave: true);
        return await MapToGetOutputDtoAsync(appraisal);
    }

    [Authorize(ErpPermissions.Appraisals.Update)]
    public async Task<AppraisalDto> StartSelfReviewAsync(Guid id) => await ChangeAsync(id, a => a.StartSelfReview());

    [Authorize(ErpPermissions.Appraisals.Update)]
    public async Task<AppraisalDto> SubmitToReviewerAsync(Guid id) => await ChangeAsync(id, a => a.SubmitToReviewer());

    [Authorize(ErpPermissions.Appraisals.Review)]
    public async Task<AppraisalDto> CompleteAsync(Guid id) => await ChangeAsync(id, a => a.Complete(Clock.Now));

    [Authorize(ErpPermissions.Appraisals.Review)]
    public async Task<AppraisalDto> ReopenAsync(Guid id) => await ChangeAsync(id, a => a.Reopen());

    private async Task<AppraisalDto> ChangeAsync(Guid id, Action<Appraisal> change)
    {
        var appraisal = await GetEntityByIdAsync(id);
        change(appraisal);
        await Repository.UpdateAsync(appraisal, autoSave: true);
        return await MapToGetOutputDtoAsync(appraisal);
    }

    protected override async Task<IQueryable<Appraisal>> CreateFilteredQueryAsync(GetAppraisalListInput input)
    {
        var query = await Repository.GetQueryableAsync();
        var employee = input.EmployeeNo?.Trim().ToUpperInvariant();
        var period = CodeTableEntity.NormalizeCode(input.PeriodCode);
        return query.WhereIf(!employee.IsNullOrEmpty(), a => a.EmployeeNo == employee).WhereIf(period != null, a => a.PeriodCode == period);
    }

    protected override IQueryable<Appraisal> ApplyDefaultSorting(IQueryable<Appraisal> query) => query.OrderByDescending(a => a.No);
}

[Authorize(ErpPermissions.Appraisals.Default)]
public class AppraisalLineAppService
    : ErpCrudAppService<AppraisalLine, AppraisalLineDto, Guid, GetAppraisalLineListInput, CreateUpdateAppraisalLineDto, CreateUpdateAppraisalLineDto>,
        IAppraisalLineAppService
{
    private readonly IRepository<Appraisal, Guid> _appraisals;

    public AppraisalLineAppService(IRepository<AppraisalLine, Guid> repository, IRepository<Appraisal, Guid> appraisals)
        : base(repository)
    {
        _appraisals = appraisals;
        GetPolicyName = ErpPermissions.Appraisals.Default;
        GetListPolicyName = ErpPermissions.Appraisals.Default;
        CreatePolicyName = ErpPermissions.Appraisals.Update;
        UpdatePolicyName = ErpPermissions.Appraisals.Update;
        DeletePolicyName = ErpPermissions.Appraisals.Update;
    }

    public override async Task<AppraisalLineDto> CreateAsync(CreateUpdateAppraisalLineDto input)
    {
        await CheckCreatePolicyAsync();
        var appraisal = await _appraisals.GetAsync(input.AppraisalId, includeDetails: true);
        var line = appraisal.AddLine(GuidGenerator.Create(), input.Objective, input.Weight, input.Target);
        line.SetScores(input.SelfScore, input.ManagerScore, input.Comment);
        await _appraisals.UpdateAsync(appraisal, autoSave: true);
        return await MapToGetOutputDtoAsync(line);
    }

    /// <summary>
    /// While objectives are set the line can change in full; in the self review the employee's
    /// score, in the manager review the reviewer's; a completed appraisal does not change.
    /// </summary>
    public override async Task<AppraisalLineDto> UpdateAsync(Guid id, CreateUpdateAppraisalLineDto input)
    {
        await CheckUpdatePolicyAsync();
        var line = await GetEntityByIdAsync(id);
        var appraisal = await _appraisals.GetAsync(line.AppraisalId);
        appraisal.EnsureStatus(AppraisalStatus.Draft, AppraisalStatus.SelfReview, AppraisalStatus.ManagerReview);
        if (appraisal.Status == AppraisalStatus.Draft)
        {
            line.SetObjective(input.Objective, input.Weight, input.Target);
        }

        var self = appraisal.Status == AppraisalStatus.ManagerReview ? line.SelfScore : input.SelfScore;
        var manager = appraisal.Status == AppraisalStatus.ManagerReview ? input.ManagerScore : line.ManagerScore;
        line.SetScores(self, manager, input.Comment);
        await Repository.UpdateAsync(line, autoSave: true);
        return await MapToGetOutputDtoAsync(line);
    }

    public override async Task DeleteAsync(Guid id)
    {
        await CheckDeletePolicyAsync();
        var line = await GetEntityByIdAsync(id);
        (await _appraisals.GetAsync(line.AppraisalId)).EnsureStatus(AppraisalStatus.Draft);
        await Repository.DeleteAsync(line, autoSave: true);
    }

    protected override async Task<IQueryable<AppraisalLine>> CreateFilteredQueryAsync(GetAppraisalLineListInput input)
    {
        var query = await Repository.GetQueryableAsync();
        return query.WhereIf(input.AppraisalId.HasValue, l => l.AppraisalId == input.AppraisalId.Value);
    }

    protected override IQueryable<AppraisalLine> ApplyDefaultSorting(IQueryable<AppraisalLine> query) => query.OrderBy(l => l.LineNo);
}
