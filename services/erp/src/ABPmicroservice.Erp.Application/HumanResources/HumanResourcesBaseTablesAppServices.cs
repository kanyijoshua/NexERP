using System;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Companies;
using ABPmicroservice.Erp.Finance;
using ABPmicroservice.Erp.Permissions;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;

namespace ABPmicroservice.Erp.HumanResources;

/// <summary>Relatives.</summary>
public class RelativeAppService : CodeTableAppServiceBase<Relative, CodeTableDto, CreateUpdateCodeTableDto>, IRelativeAppService
{
    public RelativeAppService(IRepository<Relative, Guid> repository)
        : base(repository, ErpPermissions.HumanResourcesSetup.Default) { }

    protected override Relative NewEntity(Guid id, CreateUpdateCodeTableDto input) => new(id, input.Code, input.Description);
}

/// <summary>Misc. Articles.</summary>
public class MiscArticleAppService : CodeTableAppServiceBase<MiscArticle, CodeTableDto, CreateUpdateCodeTableDto>, IMiscArticleAppService
{
    public MiscArticleAppService(IRepository<MiscArticle, Guid> repository)
        : base(repository, ErpPermissions.HumanResourcesSetup.Default) { }

    protected override MiscArticle NewEntity(Guid id, CreateUpdateCodeTableDto input) => new(id, input.Code, input.Description);
}

/// <summary>Confidential.</summary>
public class ConfidentialAppService : CodeTableAppServiceBase<Confidential, CodeTableDto, CreateUpdateCodeTableDto>, IConfidentialAppService
{
    public ConfidentialAppService(IRepository<Confidential, Guid> repository)
        : base(repository, ErpPermissions.HumanResourcesSetup.Default) { }

    protected override Confidential NewEntity(Guid id, CreateUpdateCodeTableDto input) => new(id, input.Code, input.Description);
}

/// <summary>Employee Statistics Groups.</summary>
public class EmployeeStatisticsGroupAppService : CodeTableAppServiceBase<EmployeeStatisticsGroup, CodeTableDto, CreateUpdateCodeTableDto>, IEmployeeStatisticsGroupAppService
{
    public EmployeeStatisticsGroupAppService(IRepository<EmployeeStatisticsGroup, Guid> repository)
        : base(repository, ErpPermissions.HumanResourcesSetup.Default) { }

    protected override EmployeeStatisticsGroup NewEntity(Guid id, CreateUpdateCodeTableDto input) => new(id, input.Code, input.Description);
}

/// <summary>Employee Relatives.</summary>
public class EmployeeRelativeAppService : ErpTableAppService<EmployeeRelative, EmployeeRelativeDto, GetEmployeeRelativeListInput, CreateUpdateEmployeeRelativeDto>, IEmployeeRelativeAppService
{
    public EmployeeRelativeAppService(IRepository<EmployeeRelative, Guid> repository)
        : base(repository, ErpPermissions.Employees.Default) { }

    public override async Task<EmployeeRelativeDto> CreateAsync(CreateUpdateEmployeeRelativeDto input)
    {
        await CheckCreatePolicyAsync();
        await ValidateAsync(input);

        var lineNo = input.LineNo > 0 ? input.LineNo : await NextLineNoAsync(input);
        await EnsureKeyIsUniqueAsync(CodeTableEntity.NormalizeCode(input.EmployeeNo), lineNo, null);

        var entity = new EmployeeRelative(GuidGenerator.Create(), CodeTableEntity.NormalizeCode(input.EmployeeNo), lineNo);
        entity.Set(
            input.RelativeCode,
            input.FirstName,
            input.MiddleName,
            input.LastName,
            input.BirthDate,
            input.PhoneNo,
            input.RelativesEmployeeNo
        );

        await Repository.InsertAsync(entity, autoSave: true);
        return await MapToGetOutputDtoAsync(entity);
    }

    public override async Task<EmployeeRelativeDto> UpdateAsync(Guid id, CreateUpdateEmployeeRelativeDto input)
    {
        await CheckUpdatePolicyAsync();

        var entity = await GetEntityByIdAsync(id);
        await ValidateAsync(input);

        var lineNo = input.LineNo > 0 ? input.LineNo : entity.LineNo;
        await EnsureKeyIsUniqueAsync(CodeTableEntity.NormalizeCode(input.EmployeeNo), lineNo, id);

        entity.SetKey(CodeTableEntity.NormalizeCode(input.EmployeeNo), lineNo);
        entity.Set(
            input.RelativeCode,
            input.FirstName,
            input.MiddleName,
            input.LastName,
            input.BirthDate,
            input.PhoneNo,
            input.RelativesEmployeeNo
        );

        await Repository.UpdateAsync(entity, autoSave: true);
        return await MapToGetOutputDtoAsync(entity);
    }

    protected override async Task<IQueryable<EmployeeRelative>> CreateFilteredQueryAsync(GetEmployeeRelativeListInput input)
    {
        var query = await base.CreateFilteredQueryAsync(input);
        var filter = input.Filter?.Trim().ToLower();
        var employeeNo = input.EmployeeNo?.Trim().ToUpperInvariant();

        return query
            .WhereIf(!employeeNo.IsNullOrEmpty(), x => x.EmployeeNo == employeeNo)
            .WhereIf(
                !filter.IsNullOrEmpty(),
                x => x.EmployeeNo.ToLower().Contains(filter)
                    || (x.RelativeCode != null && x.RelativeCode.ToLower().Contains(filter))
                    || (x.FirstName != null && x.FirstName.ToLower().Contains(filter))
                    || (x.MiddleName != null && x.MiddleName.ToLower().Contains(filter))
                    || (x.LastName != null && x.LastName.ToLower().Contains(filter))
                    || (x.PhoneNo != null && x.PhoneNo.ToLower().Contains(filter))
            );
    }

    protected override IQueryable<EmployeeRelative> ApplyDefaultSorting(IQueryable<EmployeeRelative> query) =>
        query.OrderBy(x => x.EmployeeNo).ThenBy(x => x.LineNo);

    private async Task ValidateAsync(CreateUpdateEmployeeRelativeDto input)
    {
        await Relations.EnsureNoExistsAsync<Employee>(input.EmployeeNo);
        await CodeTableChecker.EnsureExistsAsync<Relative>(input.RelativeCode);
        await Relations.EnsureNoExistsAsync<Employee>(input.RelativesEmployeeNo);
    }

    private async Task EnsureKeyIsUniqueAsync(string employeeNo, int lineNo, Guid? exceptId)
    {
        if (await Repository.AnyAsync(x => x.EmployeeNo == employeeNo && x.LineNo == lineNo && x.Id != exceptId))
        {
            throw new BusinessException(ErpErrorCodes.BaseTables.RecordAlreadyExists).WithData("table", "Employee Relative").WithData("key", employeeNo + " " + lineNo.ToString());
        }
    }

    /// <summary>The next Line No. of the parent record: 10000 above the last.</summary>
    private async Task<int> NextLineNoAsync(CreateUpdateEmployeeRelativeDto input)
    {
        var employeeNo = CodeTableEntity.NormalizeCode(input.EmployeeNo);
        var query = await Repository.GetQueryableAsync();
        var last = await AsyncExecuter.FirstOrDefaultAsync(query.Where(x => x.EmployeeNo == employeeNo).OrderByDescending(x => x.LineNo));

        return (last?.LineNo ?? 0) + 10000;
    }
}

/// <summary>Employee Qualifications.</summary>
public class EmployeeQualificationAppService : ErpTableAppService<EmployeeQualification, EmployeeQualificationDto, GetEmployeeQualificationListInput, CreateUpdateEmployeeQualificationDto>, IEmployeeQualificationAppService
{
    public EmployeeQualificationAppService(IRepository<EmployeeQualification, Guid> repository)
        : base(repository, ErpPermissions.Employees.Default) { }

    public override async Task<EmployeeQualificationDto> CreateAsync(CreateUpdateEmployeeQualificationDto input)
    {
        await CheckCreatePolicyAsync();
        await ValidateAsync(input);

        var lineNo = input.LineNo > 0 ? input.LineNo : await NextLineNoAsync(input);
        await EnsureKeyIsUniqueAsync(CodeTableEntity.NormalizeCode(input.EmployeeNo), lineNo, null);

        var entity = new EmployeeQualification(GuidGenerator.Create(), CodeTableEntity.NormalizeCode(input.EmployeeNo), lineNo);
        entity.Set(
            input.QualificationCode,
            input.FromDate,
            input.ToDate,
            input.Type,
            input.Description,
            input.InstitutionCompany,
            input.Cost,
            input.CourseGrade,
            input.EmployeeStatus,
            input.ExpirationDate
        );

        await Repository.InsertAsync(entity, autoSave: true);
        return await MapToGetOutputDtoAsync(entity);
    }

    public override async Task<EmployeeQualificationDto> UpdateAsync(Guid id, CreateUpdateEmployeeQualificationDto input)
    {
        await CheckUpdatePolicyAsync();

        var entity = await GetEntityByIdAsync(id);
        await ValidateAsync(input);

        var lineNo = input.LineNo > 0 ? input.LineNo : entity.LineNo;
        await EnsureKeyIsUniqueAsync(CodeTableEntity.NormalizeCode(input.EmployeeNo), lineNo, id);

        entity.SetKey(CodeTableEntity.NormalizeCode(input.EmployeeNo), lineNo);
        entity.Set(
            input.QualificationCode,
            input.FromDate,
            input.ToDate,
            input.Type,
            input.Description,
            input.InstitutionCompany,
            input.Cost,
            input.CourseGrade,
            input.EmployeeStatus,
            input.ExpirationDate
        );

        await Repository.UpdateAsync(entity, autoSave: true);
        return await MapToGetOutputDtoAsync(entity);
    }

    protected override async Task<IQueryable<EmployeeQualification>> CreateFilteredQueryAsync(GetEmployeeQualificationListInput input)
    {
        var query = await base.CreateFilteredQueryAsync(input);
        var filter = input.Filter?.Trim().ToLower();
        var employeeNo = input.EmployeeNo?.Trim().ToUpperInvariant();

        return query
            .WhereIf(!employeeNo.IsNullOrEmpty(), x => x.EmployeeNo == employeeNo)
            .WhereIf(
                !filter.IsNullOrEmpty(),
                x => x.EmployeeNo.ToLower().Contains(filter)
                    || (x.QualificationCode != null && x.QualificationCode.ToLower().Contains(filter))
                    || (x.Description != null && x.Description.ToLower().Contains(filter))
                    || (x.InstitutionCompany != null && x.InstitutionCompany.ToLower().Contains(filter))
                    || (x.CourseGrade != null && x.CourseGrade.ToLower().Contains(filter))
            );
    }

    protected override IQueryable<EmployeeQualification> ApplyDefaultSorting(IQueryable<EmployeeQualification> query) =>
        query.OrderBy(x => x.EmployeeNo).ThenBy(x => x.LineNo);

    private async Task ValidateAsync(CreateUpdateEmployeeQualificationDto input)
    {
        await Relations.EnsureNoExistsAsync<Employee>(input.EmployeeNo);
        await CodeTableChecker.EnsureExistsAsync<Qualification>(input.QualificationCode);
    }

    private async Task EnsureKeyIsUniqueAsync(string employeeNo, int lineNo, Guid? exceptId)
    {
        if (await Repository.AnyAsync(x => x.EmployeeNo == employeeNo && x.LineNo == lineNo && x.Id != exceptId))
        {
            throw new BusinessException(ErpErrorCodes.BaseTables.RecordAlreadyExists).WithData("table", "Employee Qualification").WithData("key", employeeNo + " " + lineNo.ToString());
        }
    }

    /// <summary>The next Line No. of the parent record: 10000 above the last.</summary>
    private async Task<int> NextLineNoAsync(CreateUpdateEmployeeQualificationDto input)
    {
        var employeeNo = CodeTableEntity.NormalizeCode(input.EmployeeNo);
        var query = await Repository.GetQueryableAsync();
        var last = await AsyncExecuter.FirstOrDefaultAsync(query.Where(x => x.EmployeeNo == employeeNo).OrderByDescending(x => x.LineNo));

        return (last?.LineNo ?? 0) + 10000;
    }
}

/// <summary>Misc. Article Information.</summary>
public class MiscArticleInformationAppService : ErpTableAppService<MiscArticleInformation, MiscArticleInformationDto, GetMiscArticleInformationListInput, CreateUpdateMiscArticleInformationDto>, IMiscArticleInformationAppService
{
    public MiscArticleInformationAppService(IRepository<MiscArticleInformation, Guid> repository)
        : base(repository, ErpPermissions.Employees.Default) { }

    public override async Task<MiscArticleInformationDto> CreateAsync(CreateUpdateMiscArticleInformationDto input)
    {
        await CheckCreatePolicyAsync();
        await ValidateAsync(input);

        var lineNo = input.LineNo > 0 ? input.LineNo : await NextLineNoAsync(input);
        await EnsureKeyIsUniqueAsync(CodeTableEntity.NormalizeCode(input.EmployeeNo), CodeTableEntity.NormalizeCode(input.MiscArticleCode), lineNo, null);

        var entity = new MiscArticleInformation(GuidGenerator.Create(), CodeTableEntity.NormalizeCode(input.EmployeeNo), CodeTableEntity.NormalizeCode(input.MiscArticleCode), lineNo);
        entity.Set(
            input.Description,
            input.FromDate,
            input.ToDate,
            input.InUse,
            input.SerialNo
        );

        await Repository.InsertAsync(entity, autoSave: true);
        return await MapToGetOutputDtoAsync(entity);
    }

    public override async Task<MiscArticleInformationDto> UpdateAsync(Guid id, CreateUpdateMiscArticleInformationDto input)
    {
        await CheckUpdatePolicyAsync();

        var entity = await GetEntityByIdAsync(id);
        await ValidateAsync(input);

        var lineNo = input.LineNo > 0 ? input.LineNo : entity.LineNo;
        await EnsureKeyIsUniqueAsync(CodeTableEntity.NormalizeCode(input.EmployeeNo), CodeTableEntity.NormalizeCode(input.MiscArticleCode), lineNo, id);

        entity.SetKey(CodeTableEntity.NormalizeCode(input.EmployeeNo), CodeTableEntity.NormalizeCode(input.MiscArticleCode), lineNo);
        entity.Set(
            input.Description,
            input.FromDate,
            input.ToDate,
            input.InUse,
            input.SerialNo
        );

        await Repository.UpdateAsync(entity, autoSave: true);
        return await MapToGetOutputDtoAsync(entity);
    }

    protected override async Task<IQueryable<MiscArticleInformation>> CreateFilteredQueryAsync(GetMiscArticleInformationListInput input)
    {
        var query = await base.CreateFilteredQueryAsync(input);
        var filter = input.Filter?.Trim().ToLower();
        var employeeNo = input.EmployeeNo?.Trim().ToUpperInvariant();

        return query
            .WhereIf(!employeeNo.IsNullOrEmpty(), x => x.EmployeeNo == employeeNo)
            .WhereIf(
                !filter.IsNullOrEmpty(),
                x => x.EmployeeNo.ToLower().Contains(filter)
                    || x.MiscArticleCode.ToLower().Contains(filter)
                    || (x.Description != null && x.Description.ToLower().Contains(filter))
                    || (x.SerialNo != null && x.SerialNo.ToLower().Contains(filter))
            );
    }

    protected override IQueryable<MiscArticleInformation> ApplyDefaultSorting(IQueryable<MiscArticleInformation> query) =>
        query.OrderBy(x => x.EmployeeNo).ThenBy(x => x.MiscArticleCode).ThenBy(x => x.LineNo);

    private async Task ValidateAsync(CreateUpdateMiscArticleInformationDto input)
    {
        await Relations.EnsureNoExistsAsync<Employee>(input.EmployeeNo);
        await CodeTableChecker.EnsureExistsAsync<MiscArticle>(input.MiscArticleCode);
    }

    private async Task EnsureKeyIsUniqueAsync(string employeeNo, string miscArticleCode, int lineNo, Guid? exceptId)
    {
        if (await Repository.AnyAsync(x => x.EmployeeNo == employeeNo && x.MiscArticleCode == miscArticleCode && x.LineNo == lineNo && x.Id != exceptId))
        {
            throw new BusinessException(ErpErrorCodes.BaseTables.RecordAlreadyExists).WithData("table", "Misc. Article Information").WithData("key", employeeNo + " " + miscArticleCode + " " + lineNo.ToString());
        }
    }

    /// <summary>The next Line No. of the parent record: 10000 above the last.</summary>
    private async Task<int> NextLineNoAsync(CreateUpdateMiscArticleInformationDto input)
    {
        var employeeNo = CodeTableEntity.NormalizeCode(input.EmployeeNo);
        var miscArticleCode = CodeTableEntity.NormalizeCode(input.MiscArticleCode);
        var query = await Repository.GetQueryableAsync();
        var last = await AsyncExecuter.FirstOrDefaultAsync(query.Where(x => x.EmployeeNo == employeeNo && x.MiscArticleCode == miscArticleCode).OrderByDescending(x => x.LineNo));

        return (last?.LineNo ?? 0) + 10000;
    }
}

/// <summary>Confidential Information.</summary>
public class ConfidentialInformationAppService : ErpTableAppService<ConfidentialInformation, ConfidentialInformationDto, GetConfidentialInformationListInput, CreateUpdateConfidentialInformationDto>, IConfidentialInformationAppService
{
    public ConfidentialInformationAppService(IRepository<ConfidentialInformation, Guid> repository)
        : base(repository, ErpPermissions.Employees.Default) { }

    public override async Task<ConfidentialInformationDto> CreateAsync(CreateUpdateConfidentialInformationDto input)
    {
        await CheckCreatePolicyAsync();
        await ValidateAsync(input);

        var lineNo = input.LineNo > 0 ? input.LineNo : await NextLineNoAsync(input);
        await EnsureKeyIsUniqueAsync(CodeTableEntity.NormalizeCode(input.EmployeeNo), CodeTableEntity.NormalizeCode(input.ConfidentialCode), lineNo, null);

        var entity = new ConfidentialInformation(GuidGenerator.Create(), CodeTableEntity.NormalizeCode(input.EmployeeNo), CodeTableEntity.NormalizeCode(input.ConfidentialCode), lineNo);
        entity.Set(input.Description);

        await Repository.InsertAsync(entity, autoSave: true);
        return await MapToGetOutputDtoAsync(entity);
    }

    public override async Task<ConfidentialInformationDto> UpdateAsync(Guid id, CreateUpdateConfidentialInformationDto input)
    {
        await CheckUpdatePolicyAsync();

        var entity = await GetEntityByIdAsync(id);
        await ValidateAsync(input);

        var lineNo = input.LineNo > 0 ? input.LineNo : entity.LineNo;
        await EnsureKeyIsUniqueAsync(CodeTableEntity.NormalizeCode(input.EmployeeNo), CodeTableEntity.NormalizeCode(input.ConfidentialCode), lineNo, id);

        entity.SetKey(CodeTableEntity.NormalizeCode(input.EmployeeNo), CodeTableEntity.NormalizeCode(input.ConfidentialCode), lineNo);
        entity.Set(input.Description);

        await Repository.UpdateAsync(entity, autoSave: true);
        return await MapToGetOutputDtoAsync(entity);
    }

    protected override async Task<IQueryable<ConfidentialInformation>> CreateFilteredQueryAsync(GetConfidentialInformationListInput input)
    {
        var query = await base.CreateFilteredQueryAsync(input);
        var filter = input.Filter?.Trim().ToLower();
        var employeeNo = input.EmployeeNo?.Trim().ToUpperInvariant();

        return query
            .WhereIf(!employeeNo.IsNullOrEmpty(), x => x.EmployeeNo == employeeNo)
            .WhereIf(
                !filter.IsNullOrEmpty(),
                x => x.EmployeeNo.ToLower().Contains(filter)
                    || x.ConfidentialCode.ToLower().Contains(filter)
                    || (x.Description != null && x.Description.ToLower().Contains(filter))
            );
    }

    protected override IQueryable<ConfidentialInformation> ApplyDefaultSorting(IQueryable<ConfidentialInformation> query) =>
        query.OrderBy(x => x.EmployeeNo).ThenBy(x => x.ConfidentialCode).ThenBy(x => x.LineNo);

    private async Task ValidateAsync(CreateUpdateConfidentialInformationDto input)
    {
        await Relations.EnsureNoExistsAsync<Employee>(input.EmployeeNo);
        await CodeTableChecker.EnsureExistsAsync<Confidential>(input.ConfidentialCode);
    }

    private async Task EnsureKeyIsUniqueAsync(string employeeNo, string confidentialCode, int lineNo, Guid? exceptId)
    {
        if (await Repository.AnyAsync(x => x.EmployeeNo == employeeNo && x.ConfidentialCode == confidentialCode && x.LineNo == lineNo && x.Id != exceptId))
        {
            throw new BusinessException(ErpErrorCodes.BaseTables.RecordAlreadyExists).WithData("table", "Confidential Information").WithData("key", employeeNo + " " + confidentialCode + " " + lineNo.ToString());
        }
    }

    /// <summary>The next Line No. of the parent record: 10000 above the last.</summary>
    private async Task<int> NextLineNoAsync(CreateUpdateConfidentialInformationDto input)
    {
        var employeeNo = CodeTableEntity.NormalizeCode(input.EmployeeNo);
        var confidentialCode = CodeTableEntity.NormalizeCode(input.ConfidentialCode);
        var query = await Repository.GetQueryableAsync();
        var last = await AsyncExecuter.FirstOrDefaultAsync(query.Where(x => x.EmployeeNo == employeeNo && x.ConfidentialCode == confidentialCode).OrderByDescending(x => x.LineNo));

        return (last?.LineNo ?? 0) + 10000;
    }
}

/// <summary>Alternative Addresses.</summary>
public class AlternativeAddressAppService : ErpTableAppService<AlternativeAddress, AlternativeAddressDto, GetAlternativeAddressListInput, CreateUpdateAlternativeAddressDto>, IAlternativeAddressAppService
{
    public AlternativeAddressAppService(IRepository<AlternativeAddress, Guid> repository)
        : base(repository, ErpPermissions.Employees.Default) { }

    public override async Task<AlternativeAddressDto> CreateAsync(CreateUpdateAlternativeAddressDto input)
    {
        await CheckCreatePolicyAsync();
        await ValidateAsync(input);

        await EnsureKeyIsUniqueAsync(CodeTableEntity.NormalizeCode(input.EmployeeNo), CodeTableEntity.NormalizeCode(input.Code), null);

        var entity = new AlternativeAddress(GuidGenerator.Create(), CodeTableEntity.NormalizeCode(input.EmployeeNo), CodeTableEntity.NormalizeCode(input.Code));
        entity.Set(
            input.Name,
            input.Name2,
            input.Address,
            input.Address2,
            input.City,
            input.PostCode,
            input.County,
            input.PhoneNo,
            input.FaxNo,
            input.Email,
            input.CountryRegionCode
        );

        await Repository.InsertAsync(entity, autoSave: true);
        return await MapToGetOutputDtoAsync(entity);
    }

    public override async Task<AlternativeAddressDto> UpdateAsync(Guid id, CreateUpdateAlternativeAddressDto input)
    {
        await CheckUpdatePolicyAsync();

        var entity = await GetEntityByIdAsync(id);
        await ValidateAsync(input);

        await EnsureKeyIsUniqueAsync(CodeTableEntity.NormalizeCode(input.EmployeeNo), CodeTableEntity.NormalizeCode(input.Code), id);

        entity.SetKey(CodeTableEntity.NormalizeCode(input.EmployeeNo), CodeTableEntity.NormalizeCode(input.Code));
        entity.Set(
            input.Name,
            input.Name2,
            input.Address,
            input.Address2,
            input.City,
            input.PostCode,
            input.County,
            input.PhoneNo,
            input.FaxNo,
            input.Email,
            input.CountryRegionCode
        );

        await Repository.UpdateAsync(entity, autoSave: true);
        return await MapToGetOutputDtoAsync(entity);
    }

    protected override async Task<IQueryable<AlternativeAddress>> CreateFilteredQueryAsync(GetAlternativeAddressListInput input)
    {
        var query = await base.CreateFilteredQueryAsync(input);
        var filter = input.Filter?.Trim().ToLower();
        var employeeNo = input.EmployeeNo?.Trim().ToUpperInvariant();

        return query
            .WhereIf(!employeeNo.IsNullOrEmpty(), x => x.EmployeeNo == employeeNo)
            .WhereIf(
                !filter.IsNullOrEmpty(),
                x => x.EmployeeNo.ToLower().Contains(filter)
                    || x.Code.ToLower().Contains(filter)
                    || (x.Name != null && x.Name.ToLower().Contains(filter))
                    || (x.Name2 != null && x.Name2.ToLower().Contains(filter))
                    || (x.Address != null && x.Address.ToLower().Contains(filter))
                    || (x.Address2 != null && x.Address2.ToLower().Contains(filter))
            );
    }

    protected override IQueryable<AlternativeAddress> ApplyDefaultSorting(IQueryable<AlternativeAddress> query) =>
        query.OrderBy(x => x.EmployeeNo).ThenBy(x => x.Code);

    private async Task ValidateAsync(CreateUpdateAlternativeAddressDto input)
    {
        await Relations.EnsureNoExistsAsync<Employee>(input.EmployeeNo);
        await CodeTableChecker.EnsureExistsAsync<CountryRegion>(input.CountryRegionCode);
    }

    private async Task EnsureKeyIsUniqueAsync(string employeeNo, string code, Guid? exceptId)
    {
        if (await Repository.AnyAsync(x => x.EmployeeNo == employeeNo && x.Code == code && x.Id != exceptId))
        {
            throw new BusinessException(ErpErrorCodes.BaseTables.RecordAlreadyExists).WithData("table", "Alternative Address").WithData("key", employeeNo + " " + code);
        }
    }
}

/// <summary>Human Resource Comment Lines.</summary>
public class HumanResourceCommentLineAppService : ErpTableAppService<HumanResourceCommentLine, HumanResourceCommentLineDto, GetHumanResourceCommentLineListInput, CreateUpdateHumanResourceCommentLineDto>, IHumanResourceCommentLineAppService
{
    public HumanResourceCommentLineAppService(IRepository<HumanResourceCommentLine, Guid> repository)
        : base(repository, ErpPermissions.Employees.Default) { }

    public override async Task<HumanResourceCommentLineDto> CreateAsync(CreateUpdateHumanResourceCommentLineDto input)
    {
        await CheckCreatePolicyAsync();
        await ValidateAsync(input);

        var lineNo = input.LineNo > 0 ? input.LineNo : await NextLineNoAsync(input);
        await EnsureKeyIsUniqueAsync(input.TableName, CodeTableEntity.NormalizeCode(input.No), input.TableLineNo, lineNo, null);

        var entity = new HumanResourceCommentLine(GuidGenerator.Create(), input.TableName, CodeTableEntity.NormalizeCode(input.No), input.TableLineNo, lineNo);
        entity.Set(
            input.AlternativeAddressCode,
            input.Date,
            input.Code,
            input.Comment
        );

        await Repository.InsertAsync(entity, autoSave: true);
        return await MapToGetOutputDtoAsync(entity);
    }

    public override async Task<HumanResourceCommentLineDto> UpdateAsync(Guid id, CreateUpdateHumanResourceCommentLineDto input)
    {
        await CheckUpdatePolicyAsync();

        var entity = await GetEntityByIdAsync(id);
        await ValidateAsync(input);

        var lineNo = input.LineNo > 0 ? input.LineNo : entity.LineNo;
        await EnsureKeyIsUniqueAsync(input.TableName, CodeTableEntity.NormalizeCode(input.No), input.TableLineNo, lineNo, id);

        entity.SetKey(input.TableName, CodeTableEntity.NormalizeCode(input.No), input.TableLineNo, lineNo);
        entity.Set(
            input.AlternativeAddressCode,
            input.Date,
            input.Code,
            input.Comment
        );

        await Repository.UpdateAsync(entity, autoSave: true);
        return await MapToGetOutputDtoAsync(entity);
    }

    protected override async Task<IQueryable<HumanResourceCommentLine>> CreateFilteredQueryAsync(GetHumanResourceCommentLineListInput input)
    {
        var query = await base.CreateFilteredQueryAsync(input);
        var filter = input.Filter?.Trim().ToLower();
        var no = input.No?.Trim().ToUpperInvariant();

        return query
            .WhereIf(!no.IsNullOrEmpty(), x => x.No == no)
            .WhereIf(
                !filter.IsNullOrEmpty(),
                x => x.No.ToLower().Contains(filter)
                    || (x.AlternativeAddressCode != null && x.AlternativeAddressCode.ToLower().Contains(filter))
                    || (x.Code != null && x.Code.ToLower().Contains(filter))
                    || (x.Comment != null && x.Comment.ToLower().Contains(filter))
            );
    }

    protected override IQueryable<HumanResourceCommentLine> ApplyDefaultSorting(IQueryable<HumanResourceCommentLine> query) =>
        query.OrderBy(x => x.TableName).ThenBy(x => x.No).ThenBy(x => x.TableLineNo).ThenBy(x => x.LineNo);

    private static Task ValidateAsync(CreateUpdateHumanResourceCommentLineDto input) => Task.CompletedTask;

    private async Task EnsureKeyIsUniqueAsync(HumanResourcesCommentTableName tableName, string no, int tableLineNo, int lineNo, Guid? exceptId)
    {
        if (await Repository.AnyAsync(x => x.TableName == tableName && x.No == no && x.TableLineNo == tableLineNo && x.LineNo == lineNo && x.Id != exceptId))
        {
            throw new BusinessException(ErpErrorCodes.BaseTables.RecordAlreadyExists).WithData("table", "Human Resource Comment Line").WithData("key", tableName.ToString() + " " + no + " " + tableLineNo.ToString() + " " + lineNo.ToString());
        }
    }

    /// <summary>The next Line No. of the parent record: 10000 above the last.</summary>
    private async Task<int> NextLineNoAsync(CreateUpdateHumanResourceCommentLineDto input)
    {
        var tableName = input.TableName;
        var no = CodeTableEntity.NormalizeCode(input.No);
        var tableLineNo = input.TableLineNo;
        var query = await Repository.GetQueryableAsync();
        var last = await AsyncExecuter.FirstOrDefaultAsync(query.Where(x => x.TableName == tableName && x.No == no && x.TableLineNo == tableLineNo).OrderByDescending(x => x.LineNo));

        return (last?.LineNo ?? 0) + 10000;
    }
}
