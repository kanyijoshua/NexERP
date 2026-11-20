using System;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Exporting;
using ABPmicroservice.Erp.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Authorization;

namespace ABPmicroservice.Erp.Attachments;

[Authorize(ErpPermissions.Attachments.Default)]
public class RecordPictureAppService : ErpAppService, IRecordPictureAppService
{
    private readonly RecordPictureManager _manager;

    public RecordPictureAppService(RecordPictureManager manager)
    {
        _manager = manager;
    }

    public async Task<RecordPictureDto> GetAsync(GetRecordPictureInput input)
    {
        await CheckRecordAccessAsync(input.EntityName);
        return Map(await _manager.FindAsync(input.EntityName, input.RecordId));
    }

    [Authorize(ErpPermissions.Attachments.Create)]
    public async Task<RecordPictureDto> UploadAsync(UploadRecordPictureInput input)
    {
        await CheckRecordAccessAsync(input.EntityName);

        // Refused on the encoded length, before the image is decoded into memory.
        if ((input.ContentBase64?.Length ?? 0) / 4L * 3 > ErpDomainConsts.MaxRecordPictureBytes + 3)
        {
            throw RecordPictureManager.TooLarge();
        }

        byte[] content;
        try
        {
            content = Convert.FromBase64String(input.ContentBase64 ?? string.Empty);
        }
        catch (FormatException)
        {
            throw new BusinessException(ErpErrorCodes.Attachments.FileEmpty);
        }

        return Map(await _manager.SetAsync(input.EntityName, input.RecordId, content));
    }

    [Authorize(ErpPermissions.Attachments.Delete)]
    public async Task DeleteAsync(GetRecordPictureInput input)
    {
        await CheckRecordAccessAsync(input.EntityName);
        await _manager.RemoveAsync(input.EntityName, input.RecordId);
    }

    /// <summary>The caller may read the record's table; a table with no permission entry is closed.</summary>
    private async Task CheckRecordAccessAsync(string entityName)
    {
        var permission = ErpEntityPermissions.Find(entityName)
            ?? throw new AbpAuthorizationException(code: AbpAuthorizationErrorCodes.GivenPolicyHasNotGranted);

        await AuthorizationService.CheckAsync(permission);
    }

    private static RecordPictureDto Map(RecordPicture picture)
    {
        if (picture == null)
        {
            return new RecordPictureDto();
        }

        return new RecordPictureDto
        {
            HasPicture = true,
            ContentType = picture.ContentType,
            ContentBase64 = Convert.ToBase64String(picture.Content),
            Size = picture.Size,
            ChangedTime = picture.LastModificationTime ?? picture.CreationTime,
        };
    }
}
