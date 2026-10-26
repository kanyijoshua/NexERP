using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Companies;
using ABPmicroservice.Erp.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;

namespace ABPmicroservice.Erp.Emailing;

[Authorize(ErpPermissions.DocumentEmailing.Default)]
public class EmailTemplateAppService
    : ErpCrudAppService<
        EmailTemplate,
        EmailTemplateDto,
        Guid,
        GetEmailTemplateListInput,
        CreateUpdateEmailTemplateDto,
        CreateUpdateEmailTemplateDto
    >,
        IEmailTemplateAppService
{
    private readonly DocumentEmailManager _emailManager;

    public EmailTemplateAppService(IRepository<EmailTemplate, Guid> repository, DocumentEmailManager emailManager)
        : base(repository)
    {
        _emailManager = emailManager;
        GetPolicyName = ErpPermissions.DocumentEmailing.Default;
        GetListPolicyName = ErpPermissions.DocumentEmailing.Default;
        CreatePolicyName = ErpPermissions.DocumentEmailing.ManageTemplates;
        UpdatePolicyName = ErpPermissions.DocumentEmailing.ManageTemplates;
        DeletePolicyName = ErpPermissions.DocumentEmailing.ManageTemplates;
    }

    public override async Task<EmailTemplateDto> CreateAsync(CreateUpdateEmailTemplateDto input)
    {
        await CheckCreatePolicyAsync();
        var code = CodeTableEntity.NormalizeCode(input.Code);
        if (await Repository.AnyAsync(x => x.Code == code))
        {
            throw new BusinessException(ErpErrorCodes.Emailing.TemplateCodeAlreadyExists).WithData("code", code);
        }

        var entity = new EmailTemplate(
            GuidGenerator.Create(),
            code,
            input.Name,
            input.DocumentType,
            input.SubjectTemplate,
            input.BodyHtmlTemplate,
            input.IsDefault,
            input.Active
        );

        await Repository.InsertAsync(entity, autoSave: true);
        await _emailManager.ClearOtherDefaultsAsync(entity);
        return await MapToGetOutputDtoAsync(entity);
    }

    public override async Task<EmailTemplateDto> UpdateAsync(Guid id, CreateUpdateEmailTemplateDto input)
    {
        await CheckUpdatePolicyAsync();
        var entity = await GetEntityByIdAsync(id);
        var code = CodeTableEntity.NormalizeCode(input.Code);
        if (await Repository.AnyAsync(x => x.Code == code && x.Id != id))
        {
            throw new BusinessException(ErpErrorCodes.Emailing.TemplateCodeAlreadyExists).WithData("code", code);
        }

        entity.SetCode(code);
        entity.Update(
            input.Name,
            input.DocumentType,
            input.SubjectTemplate,
            input.BodyHtmlTemplate,
            input.IsDefault,
            input.Active
        );

        await Repository.UpdateAsync(entity, autoSave: true);
        await _emailManager.ClearOtherDefaultsAsync(entity);
        return await MapToGetOutputDtoAsync(entity);
    }

    /// <summary>The placeholders a subject or body can use. Routed as GET /api/erp/email-template/placeholders.</summary>
    public Task<List<string>> GetPlaceholdersAsync() => Task.FromResult(DocumentEmailManager.Placeholders.ToList());

    protected override async Task<IQueryable<EmailTemplate>> CreateFilteredQueryAsync(GetEmailTemplateListInput input)
    {
        var query = await base.CreateFilteredQueryAsync(input);
        var filter = input.Filter?.Trim().ToLower();

        return query
            .WhereIf(!filter.IsNullOrEmpty(), x => x.Code.ToLower().Contains(filter) || x.Name.ToLower().Contains(filter))
            .WhereIf(input.DocumentType.HasValue, x => x.DocumentType == input.DocumentType.Value)
            .WhereIf(input.Active.HasValue, x => x.Active == input.Active.Value);
    }

    protected override IQueryable<EmailTemplate> ApplyDefaultSorting(IQueryable<EmailTemplate> query)
    {
        return query.OrderBy(x => x.DocumentType).ThenBy(x => x.Code);
    }
}

[Authorize(ErpPermissions.DocumentEmailing.Default)]
public class DocumentEmailAppService
    : ErpReadOnlyAppService<SentEmailLog, SentEmailLogDto, Guid, GetSentEmailLogListInput>,
        IDocumentEmailAppService
{
    private readonly DocumentEmailManager _emailManager;

    public DocumentEmailAppService(
        IRepository<SentEmailLog, Guid> repository,
        DocumentEmailManager emailManager
    )
        : base(repository)
    {
        _emailManager = emailManager;
        GetPolicyName = ErpPermissions.DocumentEmailing.Default;
        GetListPolicyName = ErpPermissions.DocumentEmailing.Default;
    }

    public async Task<DocumentEmailPreviewDto> PreviewAsync(PreviewDocumentEmailInput input)
    {
        var preview = await _emailManager.BuildPreviewAsync(
            input.DocumentType,
            input.DocumentId,
            input.TemplateCode
        );
        return ObjectMapper.Map<DocumentEmailPreview, DocumentEmailPreviewDto>(preview);
    }

    [Authorize(ErpPermissions.DocumentEmailing.Send)]
    public async Task<SentEmailLogDto> SendAsync(SendDocumentEmailInput input)
    {
        var log = await _emailManager.SendAsync(
            new DocumentEmailRequest
            {
                DocumentType = input.DocumentType,
                DocumentId = input.DocumentId,
                RecipientEmail = input.RecipientEmail,
                CcEmail = input.CcEmail,
                Subject = input.CustomSubject,
                BodyHtml = input.CustomBodyHtml,
                TemplateCode = input.TemplateCode,
                AttachmentIds = input.AttachmentIds ?? [],
                SentByUserId = CurrentUser.Id,
            }
        );
        return ObjectMapper.Map<SentEmailLog, SentEmailLogDto>(log);
    }

    protected override async Task<IQueryable<SentEmailLog>> CreateFilteredQueryAsync(GetSentEmailLogListInput input)
    {
        var query = await base.CreateFilteredQueryAsync(input);
        var filter = input.Filter?.Trim().ToLower();

        return query
            .WhereIf(
                !filter.IsNullOrEmpty(),
                x => x.DocumentNo.ToLower().Contains(filter)
                     || x.RecipientEmail.ToLower().Contains(filter)
                     || (x.PartyName != null && x.PartyName.ToLower().Contains(filter))
                     || x.Subject.ToLower().Contains(filter)
            )
            .WhereIf(input.DocumentType.HasValue, x => x.DocumentType == input.DocumentType.Value)
            .WhereIf(input.DocumentId.HasValue, x => x.DocumentId == input.DocumentId.Value)
            .WhereIf(!input.DocumentNo.IsNullOrWhiteSpace(), x => x.DocumentNo == input.DocumentNo)
            .WhereIf(input.Status.HasValue, x => x.Status == input.Status.Value);
    }

    protected override IQueryable<SentEmailLog> ApplyDefaultSorting(IQueryable<SentEmailLog> query)
    {
        return query.OrderByDescending(x => x.SentAt);
    }
}
