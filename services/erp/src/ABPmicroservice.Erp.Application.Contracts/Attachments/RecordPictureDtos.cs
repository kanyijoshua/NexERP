using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace ABPmicroservice.Erp.Attachments;

public class GetRecordPictureInput
{
    /// <summary>The table the record belongs to, e.g. "Customer" or "Employee".</summary>
    [Required]
    [StringLength(ErpDomainConsts.MaxEntityNameLength)]
    public string EntityName { get; set; }

    public Guid RecordId { get; set; }
}

public class UploadRecordPictureInput : GetRecordPictureInput
{
    /// <summary>The image, base64 encoded: PNG, JPEG, GIF or WebP.</summary>
    [Required]
    public string ContentBase64 { get; set; }
}

public class RecordPictureDto
{
    /// <summary>False when the record has no picture; the other fields are then empty.</summary>
    public bool HasPicture { get; set; }

    public string ContentType { get; set; }
    public string ContentBase64 { get; set; }
    public long Size { get; set; }
    public DateTime? ChangedTime { get; set; }
}

/// <summary>
/// The picture shown as a record's avatar on its card. Same permissions as attachments: the
/// attachments permission plus the permission to read the record's table.
/// </summary>
public interface IRecordPictureAppService : IApplicationService
{
    /// <summary>Routed as GET /api/erp/record-picture.</summary>
    Task<RecordPictureDto> GetAsync(GetRecordPictureInput input);

    /// <summary>Routed as POST /api/erp/record-picture/upload.</summary>
    Task<RecordPictureDto> UploadAsync(UploadRecordPictureInput input);

    /// <summary>Routed as DELETE /api/erp/record-picture.</summary>
    Task DeleteAsync(GetRecordPictureInput input);
}
