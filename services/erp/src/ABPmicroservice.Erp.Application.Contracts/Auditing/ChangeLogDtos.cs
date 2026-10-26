using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace ABPmicroservice.Erp.Auditing;

public class ChangeLogSetupDto
{
    public bool ChangeLogActivated { get; set; }
}

public class ChangeLogTableDto : FullAuditedEntityDto<Guid>
{
    public string EntityName { get; set; }
    public ChangeLogFieldFilterType LogInsertion { get; set; }
    public ChangeLogFieldFilterType LogModification { get; set; }
    public ChangeLogFieldFilterType LogDeletion { get; set; }
    public List<string> TrackedFields { get; set; } = [];
}

public class CreateUpdateChangeLogTableDto
{
    [Required]
    [StringLength(ErpDomainConsts.MaxNameLength)]
    public string EntityName { get; set; }

    public ChangeLogFieldFilterType LogInsertion { get; set; }
    public ChangeLogFieldFilterType LogModification { get; set; } = ChangeLogFieldFilterType.AllFields;
    public ChangeLogFieldFilterType LogDeletion { get; set; }

    /// <summary>The fields logged where a type of change is set to Some Fields.</summary>
    public List<string> TrackedFields { get; set; } = [];
}

public class GetChangeLogTableListInput : ErpPagedListInput
{
    public string Filter { get; set; }
}

/// <summary>A table that can be logged, with the fields that can be picked for Some Fields.</summary>
public class ChangeLogLoggableTableDto
{
    public string EntityName { get; set; }
    public List<string> Fields { get; set; } = [];
}

public class ChangeLogEntryDto : EntityDto<Guid>
{
    public DateTime ChangedAt { get; set; }
    public Guid? UserId { get; set; }
    public string UserName { get; set; }
    public string EntityName { get; set; }
    public Guid RecordId { get; set; }
    public string PrimaryKey { get; set; }
    public string FieldName { get; set; }
    public ChangeLogEntryType TypeOfChange { get; set; }
    public string OldValue { get; set; }
    public string NewValue { get; set; }
}

public class GetChangeLogEntryListInput : ErpPagedListInput
{
    public string Filter { get; set; }
    public string EntityName { get; set; }
    public Guid? RecordId { get; set; }
    public string FieldName { get; set; }
    public ChangeLogEntryType? TypeOfChange { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}

/// <summary>The clean-up: deletes the entries logged before a date.</summary>
public class PurgeChangeLogEntriesInput
{
    [Required]
    public DateTime Before { get; set; }

    /// <summary>Blank deletes the entries of every table.</summary>
    [StringLength(ErpDomainConsts.MaxNameLength)]
    public string EntityName { get; set; }
}

public class PurgeChangeLogEntriesResultDto
{
    public int Deleted { get; set; }
}

public interface IChangeLogSetupAppService : IApplicationService
{
    Task<ChangeLogSetupDto> GetAsync();

    Task<ChangeLogSetupDto> UpdateAsync(ChangeLogSetupDto input);

    /// <summary>The tables that can be logged. Routed as GET /api/erp/change-log-setup/loggable-tables.</summary>
    Task<List<ChangeLogLoggableTableDto>> GetLoggableTablesAsync();
}

public interface IChangeLogTableAppService
    : ICrudAppService<ChangeLogTableDto, Guid, GetChangeLogTableListInput, CreateUpdateChangeLogTableDto, CreateUpdateChangeLogTableDto> { }

public interface IChangeLogEntryAppService : IReadOnlyAppService<ChangeLogEntryDto, Guid, GetChangeLogEntryListInput>
{
    /// <summary>Routed as POST /api/erp/change-log-entry/purge-entries.</summary>
    Task<PurgeChangeLogEntriesResultDto> PurgeEntriesAsync(PurgeChangeLogEntriesInput input);
}
