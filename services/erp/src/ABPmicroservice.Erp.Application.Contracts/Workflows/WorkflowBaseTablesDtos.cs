using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Companies;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace ABPmicroservice.Erp.Workflows;

/// <summary>Workflow User Groups.</summary>
public interface IWorkflowUserGroupAppService : ICrudAppService<CodeTableDto, Guid, GetCodeTableListInput, CreateUpdateCodeTableDto, CreateUpdateCodeTableDto> { }

public class WorkflowUserGroupMemberDto : FullAuditedEntityDto<Guid>
{
    public string WorkflowUserGroupCode { get; set; }
    public string UserName { get; set; }
    public int SequenceNo { get; set; }
}

public class CreateUpdateWorkflowUserGroupMemberDto
{
    [Required]
    [StringLength(20)]
    public string WorkflowUserGroupCode { get; set; }

    [Required]
    [StringLength(50)]
    public string UserName { get; set; }

    public int SequenceNo { get; set; }
}

public class GetWorkflowUserGroupMemberListInput : ErpPagedListInput
{
    /// <summary>Matches any of the text columns, ignoring case.</summary>
    public string Filter { get; set; }
    public string WorkflowUserGroupCode { get; set; }
}

/// <summary>Workflow User Group Members.</summary>
public interface IWorkflowUserGroupMemberAppService : ICrudAppService<WorkflowUserGroupMemberDto, Guid, GetWorkflowUserGroupMemberListInput, CreateUpdateWorkflowUserGroupMemberDto, CreateUpdateWorkflowUserGroupMemberDto> { }

public class ApprovalCommentLineDto : FullAuditedEntityDto<Guid>
{
    public long EntryNo { get; set; }
    public int TableId { get; set; }
    public ApprovalDocumentType DocumentType { get; set; }
    public string DocumentNo { get; set; }
    public string UserId { get; set; }
    public DateTime? DateAndTime { get; set; }
    public string Comment { get; set; }
}

public class CreateUpdateApprovalCommentLineDto
{
    /// <summary>Zero takes the next free number.</summary>
    public long EntryNo { get; set; }

    public int TableId { get; set; }

    public ApprovalDocumentType DocumentType { get; set; }

    [StringLength(20)]
    public string DocumentNo { get; set; }

    [StringLength(50)]
    public string UserId { get; set; }

    public DateTime? DateAndTime { get; set; }

    [StringLength(80)]
    public string Comment { get; set; }
}

public class GetApprovalCommentLineListInput : ErpPagedListInput
{
    /// <summary>Matches any of the text columns, ignoring case.</summary>
    public string Filter { get; set; }
    public string DocumentNo { get; set; }
}

/// <summary>Approval Comment Lines.</summary>
public interface IApprovalCommentLineAppService : ICrudAppService<ApprovalCommentLineDto, Guid, GetApprovalCommentLineListInput, CreateUpdateApprovalCommentLineDto, CreateUpdateApprovalCommentLineDto> { }
