using System;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Companies;
using ABPmicroservice.Erp.Finance;
using ABPmicroservice.Erp.Numbering;
using ABPmicroservice.Erp.Permissions;
using ABPmicroservice.Erp.Sales;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;

namespace ABPmicroservice.Erp.Academics;

/// <summary>Academic Setup: one record per company, created on first read.</summary>
[Authorize(ErpPermissions.AcademicSetup.Default)]
public class AcademicSetupAppService : ErpAppService, IAcademicSetupAppService
{
    private readonly AcademicSetupManager _setupManager;
    private readonly IRepository<AcademicSetup, Guid> _repository;
    private readonly NoSeriesCodeValidator _seriesValidator;
    private readonly CodeTableChecker _codeTables;

    public AcademicSetupAppService(
        AcademicSetupManager setupManager,
        IRepository<AcademicSetup, Guid> repository,
        NoSeriesCodeValidator seriesValidator,
        CodeTableChecker codeTables
    )
    {
        _setupManager = setupManager;
        _repository = repository;
        _seriesValidator = seriesValidator;
        _codeTables = codeTables;
    }

    public async Task<AcademicSetupDto> GetAsync()
    {
        return ObjectMapper.Map<AcademicSetup, AcademicSetupDto>(await _setupManager.GetAsync());
    }

    [Authorize(ErpPermissions.AcademicSetup.Update)]
    public async Task<AcademicSetupDto> UpdateAsync(AcademicSetupDto input)
    {
        await _seriesValidator.EnsureExistAsync(input.ApplicationNos, input.StudentNos, input.RegistrationNos, input.BillingNos, input.ExamResultNos);
        await _seriesValidator.EnsureExistAsync(input.ReceiptNos, input.RefundNos, input.StatusChangeNos);
        await _codeTables.EnsureExistsAsync<CustomerPostingGroup>(input.StudentPostingGroup);
        await _codeTables.EnsureExistsAsync<GenBusinessPostingGroup>(input.StudentGenBusPostingGroup);

        var setup = await _setupManager.GetAsync();
        setup.SetNumbering(input.ApplicationNos, input.StudentNos, input.RegistrationNos, input.BillingNos, input.ExamResultNos);
        setup.SetStudentDocumentNumbering(input.ReceiptNos, input.RefundNos, input.StatusChangeNos);
        setup.SetPostingGroups(input.StudentPostingGroup, input.StudentGenBusPostingGroup);
        setup.SetOptions(input.CheckStudentBalance, input.MaxFeeBalanceToRegister, input.BillOnRegistration, input.ExamRoundingDecimals);

        await _seriesValidator.EnsureExistAsync(input.AttendanceNos, input.HostelAllocationNos, input.ClinicVisitNos, input.LaundryNos, input.ShortCourseNos);
        await _codeTables.EnsureExistsAsync<FeeItem>(input.MedicalFeeItemCode);
        setup.SetCampusNumbering(input.AttendanceNos, input.HostelAllocationNos, input.ClinicVisitNos, input.LaundryNos, input.ShortCourseNos);
        setup.SetCampusOptions(input.MinAttendancePct, input.LaundryExpressChargePct, input.MedicalFeeItemCode);

        await _repository.UpdateAsync(setup, autoSave: true);
        return ObjectMapper.Map<AcademicSetup, AcademicSetupDto>(setup);
    }
}

/// <summary>An academic code table under the Academic Setup permissions.</summary>
[Authorize(ErpPermissions.AcademicSetup.Default)]
public abstract class AcademicCodeTableAppService<TEntity, TDto, TInput> : CodeTableAppServiceBase<TEntity, TDto, TInput>
    where TEntity : CodeTableEntity
    where TDto : CodeTableDto
    where TInput : CreateUpdateCodeTableDto
{
    protected AcademicCodeTableAppService(IRepository<TEntity, Guid> repository)
        : base(repository, ErpPermissions.AcademicSetup.Default) { }

    /// <summary>Refuses to delete or recode a record other records point at.</summary>
    protected static void EnsureNotInUse(bool inUse, string table, string code)
    {
        if (inUse)
        {
            throw new BusinessException(ErpErrorCodes.Academics.RecordInUse).WithData("table", table).WithData("code", code);
        }
    }
}

/// <summary>
/// What the calendar tables share: at most one record is the current one, so marking a record
/// current takes the mark off the record that had it.
/// </summary>
public abstract class AcademicPeriodAppService<TEntity, TDto, TInput> : AcademicCodeTableAppService<TEntity, TDto, TInput>
    where TEntity : AcademicPeriodEntity
    where TDto : CodeTableDto
    where TInput : CreateUpdateCodeTableDto
{
    protected AcademicPeriodAppService(IRepository<TEntity, Guid> repository)
        : base(repository) { }

    public override async Task<TDto> CreateAsync(TInput input)
    {
        var dto = await base.CreateAsync(input);
        await KeepOneCurrentAsync(dto.Id);
        return dto;
    }

    public override async Task<TDto> UpdateAsync(Guid id, TInput input)
    {
        var dto = await base.UpdateAsync(id, input);
        await KeepOneCurrentAsync(id);
        return dto;
    }

    private async Task KeepOneCurrentAsync(Guid id)
    {
        if (!(await Repository.GetAsync(id)).Current)
        {
            return;
        }

        foreach (var other in await Repository.GetListAsync(x => x.Current && x.Id != id))
        {
            other.ClearCurrent();
            await Repository.UpdateAsync(other, autoSave: true);
        }
    }
}

public class AcademicYearAppService
    : AcademicPeriodAppService<AcademicYear, AcademicYearDto, CreateUpdateAcademicYearDto>,
        IAcademicYearAppService
{
    public AcademicYearAppService(IRepository<AcademicYear, Guid> repository)
        : base(repository) { }

    protected override AcademicYear NewEntity(Guid id, CreateUpdateAcademicYearDto input) => new(id, input.Code, input.Description);

    protected override Task ApplyAsync(AcademicYear entity, CreateUpdateAcademicYearDto input)
    {
        entity.SetPeriod(input.StartDate, input.EndDate, input.Current);
        return Task.CompletedTask;
    }
}

public class SemesterAppService
    : AcademicPeriodAppService<Semester, SemesterDto, CreateUpdateSemesterDto>,
        ISemesterAppService
{
    private readonly IRepository<SemesterRegistration, Guid> _registrations;

    public SemesterAppService(IRepository<Semester, Guid> repository, IRepository<SemesterRegistration, Guid> registrations)
        : base(repository)
    {
        _registrations = registrations;
    }

    protected override Semester NewEntity(Guid id, CreateUpdateSemesterDto input) => new(id, input.Code, input.Description);

    protected override async Task ApplyAsync(Semester entity, CreateUpdateSemesterDto input)
    {
        await CodeTableChecker.EnsureExistsAsync<AcademicYear>(input.AcademicYearCode);
        entity.SetPeriod(input.StartDate, input.EndDate, input.Current);
        entity.Set(input.AcademicYearCode, input.RegistrationFrom, input.RegistrationTo);
    }

    public override async Task DeleteAsync(Guid id)
    {
        await CheckDeletePolicyAsync();

        var semester = await Repository.GetAsync(id);
        EnsureNotInUse(await _registrations.AnyAsync(r => r.SemesterCode == semester.Code), "Semester", semester.Code);

        await Repository.DeleteAsync(id, autoSave: true);
    }
}

public class IntakeAppService
    : AcademicPeriodAppService<Intake, IntakeDto, CreateUpdateIntakeDto>,
        IIntakeAppService
{
    public IntakeAppService(IRepository<Intake, Guid> repository)
        : base(repository) { }

    protected override Intake NewEntity(Guid id, CreateUpdateIntakeDto input) => new(id, input.Code, input.Description);

    protected override async Task ApplyAsync(Intake entity, CreateUpdateIntakeDto input)
    {
        await CodeTableChecker.EnsureExistsAsync<AcademicYear>(input.AcademicYearCode);
        entity.SetPeriod(input.StartDate, input.EndDate, input.Current);
        entity.Set(input.AcademicYearCode);
    }
}

/// <summary>Exam categories: the grading schemes programmes are examined under.</summary>
public class ExamCategoryAppService
    : AcademicCodeTableAppService<ExamCategory, ExamCategoryDto, CreateUpdateExamCategoryDto>,
        IExamCategoryAppService
{
    private readonly IRepository<Programme, Guid> _programmes;

    public ExamCategoryAppService(IRepository<ExamCategory, Guid> repository, IRepository<Programme, Guid> programmes)
        : base(repository)
    {
        _programmes = programmes;
    }

    protected override ExamCategory NewEntity(Guid id, CreateUpdateExamCategoryDto input) => new(id, input.Code, input.Description);

    protected override Task ApplyAsync(ExamCategory entity, CreateUpdateExamCategoryDto input)
    {
        entity.Set(input.BlockResultsEntry);
        return Task.CompletedTask;
    }

    public override async Task DeleteAsync(Guid id)
    {
        await CheckDeletePolicyAsync();

        var category = await Repository.GetAsync(id);
        EnsureNotInUse(await _programmes.AnyAsync(p => p.ExamCategoryCode == category.Code), "Exam Category", category.Code);

        await Repository.DeleteAsync(id, autoSave: true);
    }
}

/// <summary>The grades of the exam categories.</summary>
public class GradingBandAppService
    : ErpTableAppService<GradingBand, GradingBandDto, GetExamCategoryTableListInput, CreateUpdateGradingBandDto>,
        IGradingBandAppService
{
    public GradingBandAppService(IRepository<GradingBand, Guid> repository)
        : base(repository, ErpPermissions.AcademicSetup.Default) { }

    public override async Task<GradingBandDto> CreateAsync(CreateUpdateGradingBandDto input)
    {
        await CheckCreatePolicyAsync();

        var band = new GradingBand(GuidGenerator.Create(), input.ExamCategoryCode, input.Grade);
        await ApplyAsync(band, input);

        await Repository.InsertAsync(band, autoSave: true);
        return await MapToGetOutputDtoAsync(band);
    }

    public override async Task<GradingBandDto> UpdateAsync(Guid id, CreateUpdateGradingBandDto input)
    {
        await CheckUpdatePolicyAsync();

        var band = await GetEntityByIdAsync(id);
        band.SetKey(input.ExamCategoryCode, input.Grade);
        await ApplyAsync(band, input);

        await Repository.UpdateAsync(band, autoSave: true);
        return await MapToGetOutputDtoAsync(band);
    }

    protected override async Task<IQueryable<GradingBand>> CreateFilteredQueryAsync(GetExamCategoryTableListInput input)
    {
        var query = await base.CreateFilteredQueryAsync(input);
        var category = (input.ExamCategoryCode ?? input.Filter)?.Trim().ToUpperInvariant();

        return query.WhereIf(!category.IsNullOrEmpty(), x => x.ExamCategoryCode == category);
    }

    protected override IQueryable<GradingBand> ApplyDefaultSorting(IQueryable<GradingBand> query) =>
        query.OrderBy(x => x.ExamCategoryCode).ThenByDescending(x => x.FromMark);

    private async Task ApplyAsync(GradingBand band, CreateUpdateGradingBandDto input)
    {
        await CodeTableChecker.EnsureExistsAsync<ExamCategory>(band.ExamCategoryCode);
        band.Set(input.Description, input.FromMark, input.ToMark, input.Points, input.Remarks, input.Passed);

        var others = await Repository.GetListAsync(x => x.ExamCategoryCode == band.ExamCategoryCode && x.Id != band.Id);
        if (others.Any(x => x.Grade == band.Grade))
        {
            throw new BusinessException(ErpErrorCodes.BaseTables.RecordAlreadyExists)
                .WithData("table", "Grading Band")
                .WithData("key", $"{band.ExamCategoryCode} {band.Grade}");
        }

        // A score must earn one grade only.
        if (others.Any(x => x.FromMark <= band.ToMark && x.ToMark >= band.FromMark))
        {
            throw new BusinessException(ErpErrorCodes.Academics.InvalidGradingBand).WithData("grade", band.Grade);
        }
    }
}

/// <summary>The assessment parts of the exam categories.</summary>
public class ExamComponentAppService
    : ErpTableAppService<ExamComponent, ExamComponentDto, GetExamCategoryTableListInput, CreateUpdateExamComponentDto>,
        IExamComponentAppService
{
    public ExamComponentAppService(IRepository<ExamComponent, Guid> repository)
        : base(repository, ErpPermissions.AcademicSetup.Default) { }

    public override async Task<ExamComponentDto> CreateAsync(CreateUpdateExamComponentDto input)
    {
        await CheckCreatePolicyAsync();

        var component = new ExamComponent(GuidGenerator.Create(), input.ExamCategoryCode, input.ExamType);
        await ApplyAsync(component, input);

        await Repository.InsertAsync(component, autoSave: true);
        return await MapToGetOutputDtoAsync(component);
    }

    public override async Task<ExamComponentDto> UpdateAsync(Guid id, CreateUpdateExamComponentDto input)
    {
        await CheckUpdatePolicyAsync();

        var component = await GetEntityByIdAsync(id);
        component.SetKey(input.ExamCategoryCode, input.ExamType);
        await ApplyAsync(component, input);

        await Repository.UpdateAsync(component, autoSave: true);
        return await MapToGetOutputDtoAsync(component);
    }

    protected override async Task<IQueryable<ExamComponent>> CreateFilteredQueryAsync(GetExamCategoryTableListInput input)
    {
        var query = await base.CreateFilteredQueryAsync(input);
        var category = (input.ExamCategoryCode ?? input.Filter)?.Trim().ToUpperInvariant();

        return query.WhereIf(!category.IsNullOrEmpty(), x => x.ExamCategoryCode == category);
    }

    protected override IQueryable<ExamComponent> ApplyDefaultSorting(IQueryable<ExamComponent> query) =>
        query.OrderBy(x => x.ExamCategoryCode).ThenBy(x => x.ExamType);

    private async Task ApplyAsync(ExamComponent component, CreateUpdateExamComponentDto input)
    {
        await CodeTableChecker.EnsureExistsAsync<ExamCategory>(component.ExamCategoryCode);
        component.Set(input.Description, input.MaxScore, input.ContributionPct);

        var others = await Repository.GetListAsync(x => x.ExamCategoryCode == component.ExamCategoryCode && x.Id != component.Id);
        if (others.Any(x => x.ExamType == component.ExamType))
        {
            throw new BusinessException(ErpErrorCodes.BaseTables.RecordAlreadyExists)
                .WithData("table", "Exam Component")
                .WithData("key", $"{component.ExamCategoryCode} {component.ExamType}");
        }

        // The parts of a category can never contribute more than the whole score.
        if (others.Sum(x => x.ContributionPct) + component.ContributionPct > 100m)
        {
            throw new BusinessException(ErpErrorCodes.Academics.InvalidExamComponent).WithData("examType", component.ExamType);
        }
    }
}

/// <summary>Programmes of study.</summary>
public class ProgrammeAppService
    : AcademicCodeTableAppService<Programme, ProgrammeDto, CreateUpdateProgrammeDto>,
        IProgrammeAppService
{
    private readonly NoSeriesCodeValidator _seriesValidator;
    private readonly IRepository<Student, Guid> _students;
    private readonly IRepository<StudentApplication, Guid> _applications;
    private readonly IRepository<ProgrammeStage, Guid> _stages;
    private readonly IRepository<CourseUnit, Guid> _units;

    public ProgrammeAppService(
        IRepository<Programme, Guid> repository,
        NoSeriesCodeValidator seriesValidator,
        IRepository<Student, Guid> students,
        IRepository<StudentApplication, Guid> applications,
        IRepository<ProgrammeStage, Guid> stages,
        IRepository<CourseUnit, Guid> units
    )
        : base(repository)
    {
        _seriesValidator = seriesValidator;
        _students = students;
        _applications = applications;
        _stages = stages;
        _units = units;
    }

    protected override Programme NewEntity(Guid id, CreateUpdateProgrammeDto input) => new(id, input.Code, input.Description);

    protected override async Task ApplyAsync(Programme entity, CreateUpdateProgrammeDto input)
    {
        await CodeTableChecker.EnsureExistsAsync<ExamCategory>(input.ExamCategoryCode);
        await _seriesValidator.EnsureExistAsync(input.StudentNos);

        entity.Set(input.Level, input.ExamCategoryCode, input.DurationMonths, input.MinimumCapacity, input.MaximumCapacity, input.Active);
        entity.SetStudentNumbering(input.StudentNos, input.StudentNoPrefix, input.StudentNoSuffix);
    }

    public override async Task<ProgrammeDto> UpdateAsync(Guid id, CreateUpdateProgrammeDto input)
    {
        var existing = await Repository.GetAsync(id);

        // The code is on every stage, unit, student and document of the programme, so it cannot change once used.
        if (existing.Code != CodeTableEntity.NormalizeCode(input.Code))
        {
            EnsureNotInUse(await IsInUseAsync(existing.Code), "Programme", existing.Code);
        }

        return await base.UpdateAsync(id, input);
    }

    public override async Task DeleteAsync(Guid id)
    {
        await CheckDeletePolicyAsync();

        var programme = await Repository.GetAsync(id);
        EnsureNotInUse(await IsInUseAsync(programme.Code), "Programme", programme.Code);

        await Repository.DeleteAsync(id, autoSave: true);
    }

    private async Task<bool> IsInUseAsync(string code)
    {
        return await _students.AnyAsync(s => s.ProgrammeCode == code)
            || await _applications.AnyAsync(a => a.ProgrammeCode == code)
            || await _stages.AnyAsync(s => s.ProgrammeCode == code)
            || await _units.AnyAsync(u => u.ProgrammeCode == code);
    }
}

/// <summary>The stages of the programmes.</summary>
public class ProgrammeStageAppService
    : ErpTableAppService<ProgrammeStage, ProgrammeStageDto, GetProgrammeTableListInput, CreateUpdateProgrammeStageDto>,
        IProgrammeStageAppService
{
    private readonly IRepository<CourseUnit, Guid> _units;
    private readonly IRepository<SemesterRegistration, Guid> _registrations;

    public ProgrammeStageAppService(
        IRepository<ProgrammeStage, Guid> repository,
        IRepository<CourseUnit, Guid> units,
        IRepository<SemesterRegistration, Guid> registrations
    )
        : base(repository, ErpPermissions.AcademicSetup.Default)
    {
        _units = units;
        _registrations = registrations;
    }

    public override async Task<ProgrammeStageDto> CreateAsync(CreateUpdateProgrammeStageDto input)
    {
        await CheckCreatePolicyAsync();
        await CodeTableChecker.EnsureExistsAsync<Programme>(input.ProgrammeCode);

        var stage = new ProgrammeStage(GuidGenerator.Create(), input.ProgrammeCode, input.Code);
        await EnsureKeyIsUniqueAsync(stage);
        stage.Set(input.Description, input.Sequence, input.FinalStage, input.MinimumUnits, input.MaximumUnits);

        await Repository.InsertAsync(stage, autoSave: true);
        return await MapToGetOutputDtoAsync(stage);
    }

    public override async Task<ProgrammeStageDto> UpdateAsync(Guid id, CreateUpdateProgrammeStageDto input)
    {
        await CheckUpdatePolicyAsync();

        var stage = await GetEntityByIdAsync(id);
        var (programme, code) = (stage.ProgrammeCode, stage.Code);

        await CodeTableChecker.EnsureExistsAsync<Programme>(input.ProgrammeCode);
        stage.SetKey(input.ProgrammeCode, input.Code);

        if (stage.ProgrammeCode != programme || stage.Code != code)
        {
            await EnsureNotInUseAsync(programme, code);
            await EnsureKeyIsUniqueAsync(stage);
        }

        stage.Set(input.Description, input.Sequence, input.FinalStage, input.MinimumUnits, input.MaximumUnits);

        await Repository.UpdateAsync(stage, autoSave: true);
        return await MapToGetOutputDtoAsync(stage);
    }

    public override async Task DeleteAsync(Guid id)
    {
        await CheckDeletePolicyAsync();

        var stage = await GetEntityByIdAsync(id);
        await EnsureNotInUseAsync(stage.ProgrammeCode, stage.Code);

        await Repository.DeleteAsync(id, autoSave: true);
    }

    protected override async Task<IQueryable<ProgrammeStage>> CreateFilteredQueryAsync(GetProgrammeTableListInput input)
    {
        var query = await base.CreateFilteredQueryAsync(input);
        var filter = input.Filter?.Trim().ToLower();
        var programme = input.ProgrammeCode?.Trim().ToUpperInvariant();

        return query
            .WhereIf(!programme.IsNullOrEmpty(), x => x.ProgrammeCode == programme)
            .WhereIf(
                !filter.IsNullOrEmpty(),
                x => x.ProgrammeCode.ToLower().Contains(filter)
                    || x.Code.ToLower().Contains(filter)
                    || (x.Description != null && x.Description.ToLower().Contains(filter))
            );
    }

    protected override IQueryable<ProgrammeStage> ApplyDefaultSorting(IQueryable<ProgrammeStage> query) =>
        query.OrderBy(x => x.ProgrammeCode).ThenBy(x => x.Sequence).ThenBy(x => x.Code);

    private async Task EnsureKeyIsUniqueAsync(ProgrammeStage stage)
    {
        if (await Repository.AnyAsync(x => x.ProgrammeCode == stage.ProgrammeCode && x.Code == stage.Code && x.Id != stage.Id))
        {
            throw new BusinessException(ErpErrorCodes.BaseTables.RecordAlreadyExists)
                .WithData("table", "Programme Stage")
                .WithData("key", $"{stage.ProgrammeCode} {stage.Code}");
        }
    }

    private async Task EnsureNotInUseAsync(string programme, string code)
    {
        if (await _units.AnyAsync(u => u.ProgrammeCode == programme && u.StageCode == code)
            || await _registrations.AnyAsync(r => r.ProgrammeCode == programme && r.StageCode == code))
        {
            throw new BusinessException(ErpErrorCodes.Academics.RecordInUse).WithData("table", "Programme Stage").WithData("code", $"{programme} {code}");
        }
    }
}

/// <summary>The units of the programmes.</summary>
public class CourseUnitAppService
    : ErpTableAppService<CourseUnit, CourseUnitDto, GetProgrammeTableListInput, CreateUpdateCourseUnitDto>,
        ICourseUnitAppService
{
    private readonly IRepository<ProgrammeStage, Guid> _stages;
    private readonly IRepository<StudentUnit, Guid> _studentUnits;

    public CourseUnitAppService(IRepository<CourseUnit, Guid> repository, IRepository<ProgrammeStage, Guid> stages, IRepository<StudentUnit, Guid> studentUnits)
        : base(repository, ErpPermissions.AcademicSetup.Default)
    {
        _stages = stages;
        _studentUnits = studentUnits;
    }

    public override async Task<CourseUnitDto> CreateAsync(CreateUpdateCourseUnitDto input)
    {
        await CheckCreatePolicyAsync();

        var unit = new CourseUnit(GuidGenerator.Create(), input.ProgrammeCode, input.Code, input.StageCode);
        await EnsureKeyIsUniqueAsync(unit);
        await ApplyAsync(unit, input);

        await Repository.InsertAsync(unit, autoSave: true);
        return await MapToGetOutputDtoAsync(unit);
    }

    public override async Task<CourseUnitDto> UpdateAsync(Guid id, CreateUpdateCourseUnitDto input)
    {
        await CheckUpdatePolicyAsync();

        var unit = await GetEntityByIdAsync(id);
        var (programme, code) = (unit.ProgrammeCode, unit.Code);

        unit.SetKey(input.ProgrammeCode, input.Code);
        if (unit.ProgrammeCode != programme || unit.Code != code)
        {
            await EnsureNotInUseAsync(programme, code);
            await EnsureKeyIsUniqueAsync(unit);
        }

        await ApplyAsync(unit, input);

        await Repository.UpdateAsync(unit, autoSave: true);
        return await MapToGetOutputDtoAsync(unit);
    }

    /// <summary>A unit students have taken stays; block it to take it off offer.</summary>
    public override async Task DeleteAsync(Guid id)
    {
        await CheckDeletePolicyAsync();

        var unit = await GetEntityByIdAsync(id);
        await EnsureNotInUseAsync(unit.ProgrammeCode, unit.Code);

        await Repository.DeleteAsync(id, autoSave: true);
    }

    protected override async Task<IQueryable<CourseUnit>> CreateFilteredQueryAsync(GetProgrammeTableListInput input)
    {
        var query = await base.CreateFilteredQueryAsync(input);
        var filter = input.Filter?.Trim().ToLower();
        var programme = input.ProgrammeCode?.Trim().ToUpperInvariant();
        var stage = input.StageCode?.Trim().ToUpperInvariant();

        return query
            .WhereIf(!programme.IsNullOrEmpty(), x => x.ProgrammeCode == programme)
            .WhereIf(!stage.IsNullOrEmpty(), x => x.StageCode == stage)
            .WhereIf(
                !filter.IsNullOrEmpty(),
                x => x.ProgrammeCode.ToLower().Contains(filter)
                    || x.Code.ToLower().Contains(filter)
                    || (x.Description != null && x.Description.ToLower().Contains(filter))
            );
    }

    protected override IQueryable<CourseUnit> ApplyDefaultSorting(IQueryable<CourseUnit> query) =>
        query.OrderBy(x => x.ProgrammeCode).ThenBy(x => x.StageCode).ThenBy(x => x.Code);

    private async Task ApplyAsync(CourseUnit unit, CreateUpdateCourseUnitDto input)
    {
        await CodeTableChecker.EnsureExistsAsync<Programme>(unit.ProgrammeCode);
        await CodeTableChecker.EnsureExistsAsync<Semester>(input.SemesterCode);

        var stage = CodeTableEntity.NormalizeCode(input.StageCode);
        await Relations.EnsureExistsAsync<ProgrammeStage>(s => s.ProgrammeCode == unit.ProgrammeCode && s.Code == stage, "Programme Stage", stage);

        var prerequisite = CodeTableEntity.NormalizeCode(input.PrerequisiteUnitCode);
        await Relations.EnsureExistsAsync<CourseUnit>(u => u.ProgrammeCode == unit.ProgrammeCode && u.Code == prerequisite, "Course Unit", prerequisite);

        unit.Set(input.Description, input.StageCode, input.SemesterCode, input.UnitType, input.CreditHours, input.PrerequisiteUnitCode, input.Blocked);
    }

    private async Task EnsureKeyIsUniqueAsync(CourseUnit unit)
    {
        if (await Repository.AnyAsync(x => x.ProgrammeCode == unit.ProgrammeCode && x.Code == unit.Code && x.Id != unit.Id))
        {
            throw new BusinessException(ErpErrorCodes.BaseTables.RecordAlreadyExists)
                .WithData("table", "Course Unit")
                .WithData("key", $"{unit.ProgrammeCode} {unit.Code}");
        }
    }

    private async Task EnsureNotInUseAsync(string programme, string code)
    {
        if (await _studentUnits.AnyAsync(u => u.ProgrammeCode == programme && u.UnitCode == code))
        {
            throw new BusinessException(ErpErrorCodes.Academics.RecordInUse).WithData("table", "Course Unit").WithData("code", $"{programme} {code}");
        }
    }
}

/// <summary>Fee items: what students are charged for and where the income goes.</summary>
public class FeeItemAppService
    : AcademicCodeTableAppService<FeeItem, FeeItemDto, CreateUpdateFeeItemDto>,
        IFeeItemAppService
{
    private readonly TableRelationChecker _relations;
    private readonly IRepository<FeeStructureLine, Guid> _feeStructure;
    private readonly IRepository<StudentBillLine, Guid> _billLines;

    public FeeItemAppService(
        IRepository<FeeItem, Guid> repository,
        TableRelationChecker relations,
        IRepository<FeeStructureLine, Guid> feeStructure,
        IRepository<StudentBillLine, Guid> billLines
    )
        : base(repository)
    {
        _relations = relations;
        _feeStructure = feeStructure;
        _billLines = billLines;
    }

    protected override FeeItem NewEntity(Guid id, CreateUpdateFeeItemDto input) => new(id, input.Code, input.Description);

    protected override async Task ApplyAsync(FeeItem entity, CreateUpdateFeeItemDto input)
    {
        await _relations.EnsureGLAccountsExistAsync(input.GLAccountNo);
        entity.Set(input.FeeType, input.GLAccountNo, input.DefaultAmount, input.TuitionFee);
    }

    public override async Task DeleteAsync(Guid id)
    {
        await CheckDeletePolicyAsync();

        var feeItem = await Repository.GetAsync(id);
        EnsureNotInUse(
            await _feeStructure.AnyAsync(f => f.FeeItemCode == feeItem.Code) || await _billLines.AnyAsync(l => l.FeeItemCode == feeItem.Code),
            "Fee Item",
            feeItem.Code
        );

        await Repository.DeleteAsync(id, autoSave: true);
    }
}

/// <summary>The fee structure: what each fee item costs for each stage of each programme.</summary>
public class FeeStructureLineAppService
    : ErpTableAppService<FeeStructureLine, FeeStructureLineDto, GetProgrammeTableListInput, CreateUpdateFeeStructureLineDto>,
        IFeeStructureLineAppService
{
    public FeeStructureLineAppService(IRepository<FeeStructureLine, Guid> repository)
        : base(repository, ErpPermissions.AcademicSetup.Default) { }

    public override async Task<FeeStructureLineDto> CreateAsync(CreateUpdateFeeStructureLineDto input)
    {
        await CheckCreatePolicyAsync();

        var line = new FeeStructureLine(GuidGenerator.Create(), input.ProgrammeCode, input.StageCode, input.FeeItemCode);
        await ApplyAsync(line, input);

        await Repository.InsertAsync(line, autoSave: true);
        return await MapToGetOutputDtoAsync(line);
    }

    public override async Task<FeeStructureLineDto> UpdateAsync(Guid id, CreateUpdateFeeStructureLineDto input)
    {
        await CheckUpdatePolicyAsync();

        var line = await GetEntityByIdAsync(id);
        await ApplyAsync(line, input);

        await Repository.UpdateAsync(line, autoSave: true);
        return await MapToGetOutputDtoAsync(line);
    }

    protected override async Task<IQueryable<FeeStructureLine>> CreateFilteredQueryAsync(GetProgrammeTableListInput input)
    {
        var query = await base.CreateFilteredQueryAsync(input);
        var filter = input.Filter?.Trim().ToLower();
        var programme = input.ProgrammeCode?.Trim().ToUpperInvariant();
        var stage = input.StageCode?.Trim().ToUpperInvariant();

        return query
            .WhereIf(!programme.IsNullOrEmpty(), x => x.ProgrammeCode == programme)
            .WhereIf(!stage.IsNullOrEmpty(), x => x.StageCode == stage)
            .WhereIf(!filter.IsNullOrEmpty(), x => x.ProgrammeCode.ToLower().Contains(filter) || x.FeeItemCode.ToLower().Contains(filter));
    }

    protected override IQueryable<FeeStructureLine> ApplyDefaultSorting(IQueryable<FeeStructureLine> query) =>
        query.OrderBy(x => x.ProgrammeCode).ThenBy(x => x.StageCode).ThenBy(x => x.SemesterCode).ThenBy(x => x.FeeItemCode);

    private async Task ApplyAsync(FeeStructureLine line, CreateUpdateFeeStructureLineDto input)
    {
        line.SetKey(input.ProgrammeCode, input.StageCode, input.SemesterCode, input.StudyMode, input.FeeItemCode);
        line.SetAmount(input.Amount);

        await CodeTableChecker.EnsureExistsAsync<Programme>(line.ProgrammeCode);
        await CodeTableChecker.EnsureExistsAsync<Semester>(line.SemesterCode);
        await CodeTableChecker.EnsureExistsAsync<FeeItem>(line.FeeItemCode);

        var (programme, stage) = (line.ProgrammeCode, line.StageCode);
        await Relations.EnsureExistsAsync<ProgrammeStage>(s => s.ProgrammeCode == programme && s.Code == stage, "Programme Stage", stage);

        var (semester, studyMode, feeItem, id) = (line.SemesterCode, line.StudyMode, line.FeeItemCode, line.Id);
        if (await Repository.AnyAsync(x =>
                x.ProgrammeCode == programme
                && x.StageCode == stage
                && x.SemesterCode == semester
                && x.StudyMode == studyMode
                && x.FeeItemCode == feeItem
                && x.Id != id
            ))
        {
            throw new BusinessException(ErpErrorCodes.BaseTables.RecordAlreadyExists)
                .WithData("table", "Fee Structure Line")
                .WithData("key", $"{programme} {stage} {semester} {feeItem}".Replace("  ", " "));
        }
    }
}
