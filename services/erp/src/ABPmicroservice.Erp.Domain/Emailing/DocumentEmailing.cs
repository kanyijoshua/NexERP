using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Attachments;
using ABPmicroservice.Erp.Companies;
using ABPmicroservice.Erp.Finance;
using ABPmicroservice.Erp.Purchasing;
using ABPmicroservice.Erp.Sales;
using Microsoft.Extensions.Logging;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Volo.Abp.Emailing;

namespace ABPmicroservice.Erp.Emailing;

/// <summary>
/// Configurable HTML email template for sales and purchase documents.
/// Placeholders such as {{DocumentNo}}, {{PartyName}}, {{TotalAmountIncludingVat}}, {{DueDate}} and
/// {{CompanyName}} are replaced when the email is built; see <see cref="DocumentEmailManager.Placeholders"/>.
/// </summary>
public class EmailTemplate : CompanyAggregateRoot
{
    public const int MaxSubjectLength = ErpDomainConsts.MaxEmailSubjectLength;
    public const int MaxBodyLength = ErpDomainConsts.MaxEmailBodyLength;

    public string Code { get; private set; }
    public string Name { get; private set; }
    public EmailDocumentType DocumentType { get; private set; }
    public string SubjectTemplate { get; private set; }
    public string BodyHtmlTemplate { get; private set; }

    /// <summary>The template a document of this type uses when none is chosen. One per document type.</summary>
    public bool IsDefault { get; private set; }

    public bool Active { get; private set; }

    protected EmailTemplate() { }

    public EmailTemplate(
        Guid id,
        string code,
        string name,
        EmailDocumentType documentType,
        string subjectTemplate,
        string bodyHtmlTemplate,
        bool isDefault = true,
        bool active = true
    )
        : base(id)
    {
        SetCode(code);
        Update(name, documentType, subjectTemplate, bodyHtmlTemplate, isDefault, active);
    }

    public void SetCode(string code)
    {
        Code = CodeTableEntity.NormalizeCode(Check.NotNullOrWhiteSpace(code, nameof(code), ErpDomainConsts.MaxCodeLength));
    }

    public void Update(
        string name,
        EmailDocumentType documentType,
        string subjectTemplate,
        string bodyHtmlTemplate,
        bool isDefault,
        bool active
    )
    {
        Name = Check.NotNullOrWhiteSpace(name, nameof(name), ErpDomainConsts.MaxNameLength);
        DocumentType = documentType;
        SubjectTemplate = Check.NotNullOrWhiteSpace(subjectTemplate, nameof(subjectTemplate), MaxSubjectLength);
        BodyHtmlTemplate = Check.NotNullOrWhiteSpace(bodyHtmlTemplate, nameof(bodyHtmlTemplate), MaxBodyLength);
        IsDefault = isDefault;
        Active = active;
    }

    internal void ClearDefault() => IsDefault = false;

    /// <summary>
    /// Replaces each {{Token}} with its value. <paramref name="htmlEncode"/> escapes the values, so a
    /// customer name cannot add markup to the body; the subject is plain text and is not escaped.
    /// </summary>
    public static string Render(string template, IReadOnlyDictionary<string, string> tokens, bool htmlEncode)
    {
        if (template.IsNullOrWhiteSpace() || tokens == null)
        {
            return template ?? string.Empty;
        }

        var output = template;
        foreach (var kvp in tokens)
        {
            var value = kvp.Value ?? string.Empty;
            output = output.Replace("{{" + kvp.Key + "}}", htmlEncode ? WebUtility.HtmlEncode(value) : value, StringComparison.OrdinalIgnoreCase);
        }
        return output;
    }
}

/// <summary>
/// Log of every document email sent to a customer or vendor, including the ones that failed.
/// </summary>
public class SentEmailLog : CompanyAggregateRoot
{
    public const int MaxEmailLength = ErpDomainConsts.MaxEmailRecipientsLength;
    public const int MaxErrorLength = 500;

    public EmailDocumentType DocumentType { get; private set; }
    public Guid DocumentId { get; private set; }
    public string DocumentNo { get; private set; }
    public string PartyNo { get; private set; }
    public string PartyName { get; private set; }
    public string RecipientEmail { get; private set; }
    public string CcEmail { get; private set; }
    public string Subject { get; private set; }
    public string BodyHtml { get; private set; }
    public string TemplateCode { get; private set; }
    public int AttachmentCount { get; private set; }
    public EmailDeliveryStatus Status { get; private set; }
    public DateTime SentAt { get; private set; }
    public Guid? SentByUserId { get; private set; }
    public string ErrorMessage { get; private set; }

    protected SentEmailLog() { }

    public SentEmailLog(
        Guid id,
        EmailDocumentType documentType,
        Guid documentId,
        string documentNo,
        string partyNo,
        string partyName,
        string recipientEmail,
        string ccEmail,
        string subject,
        string bodyHtml,
        string templateCode,
        int attachmentCount,
        DateTime sentAt,
        Guid? sentByUserId = null
    )
        : base(id)
    {
        DocumentType = documentType;
        DocumentId = documentId;
        DocumentNo = Check.NotNullOrWhiteSpace(documentNo, nameof(documentNo), ErpDomainConsts.MaxDocumentNoLength);
        PartyNo = Check.Length(partyNo, nameof(partyNo), ErpDomainConsts.MaxNoLength);
        PartyName = Check.Length(partyName, nameof(partyName), ErpDomainConsts.MaxNameLength);
        RecipientEmail = Check.NotNullOrWhiteSpace(recipientEmail, nameof(recipientEmail), MaxEmailLength);
        CcEmail = Check.Length(ccEmail.IsNullOrWhiteSpace() ? null : ccEmail.Trim(), nameof(ccEmail), MaxEmailLength);
        Subject = Check.NotNullOrWhiteSpace(subject, nameof(subject), EmailTemplate.MaxSubjectLength);
        BodyHtml = Check.NotNullOrWhiteSpace(bodyHtml, nameof(bodyHtml), EmailTemplate.MaxBodyLength);
        TemplateCode = Check.Length(templateCode, nameof(templateCode), ErpDomainConsts.MaxCodeLength);
        AttachmentCount = attachmentCount;
        SentAt = sentAt;
        SentByUserId = sentByUserId;
        Status = EmailDeliveryStatus.Queued;
    }

    public void MarkSent() => Status = EmailDeliveryStatus.Sent;

    public void MarkFailed(string errorMessage)
    {
        Status = EmailDeliveryStatus.Failed;
        var message = errorMessage.IsNullOrWhiteSpace() ? "Unknown error" : errorMessage.Trim();
        ErrorMessage = message.Length > MaxErrorLength ? message[..MaxErrorLength] : message;
    }
}

public class DocumentEmailPreview
{
    public EmailDocumentType DocumentType { get; set; }
    public Guid DocumentId { get; set; }
    public string DocumentNo { get; set; }
    public string PartyNo { get; set; }
    public string PartyName { get; set; }
    public string RecipientEmail { get; set; }
    public string TemplateCode { get; set; }
    public string Subject { get; set; }
    public string BodyHtml { get; set; }

    /// <summary>The files attached to the document that can go with the email.</summary>
    public List<DocumentEmailAttachment> Attachments { get; set; } = [];
}

public class DocumentEmailAttachment
{
    public Guid Id { get; set; }
    public string FileName { get; set; }
}

/// <summary>Everything an email for one document is sent with.</summary>
public class DocumentEmailRequest
{
    public EmailDocumentType DocumentType { get; set; }
    public Guid DocumentId { get; set; }

    /// <summary>Blank means the customer's or vendor's email address.</summary>
    public string RecipientEmail { get; set; }

    public string CcEmail { get; set; }

    /// <summary>Blank means the template's subject.</summary>
    public string Subject { get; set; }

    /// <summary>Blank means the template's body.</summary>
    public string BodyHtml { get; set; }

    public string TemplateCode { get; set; }

    /// <summary>Attachments of the document to send along; each must belong to the document.</summary>
    public List<Guid> AttachmentIds { get; set; } = [];

    public Guid? SentByUserId { get; set; }
}

/// <summary>
/// Builds a document email from a template and sends it with the SMTP settings of the host
/// (Setting Management, "Emailing"). Every attempt is logged, a failed one with its error, so the
/// sender sees what went wrong instead of a rolled-back request.
/// </summary>
public class DocumentEmailManager : DomainService
{
    /// <summary>The placeholders a template can use.</summary>
    public static readonly IReadOnlyList<string> Placeholders =
    [
        "DocumentNo",
        "DocumentType",
        "PartyNo",
        "PartyName",
        "CurrencyCode",
        "TotalAmount",
        "TotalAmountIncludingVat",
        "PostingDate",
        "DueDate",
        "CompanyName",
    ];

    public const string BuiltInTemplateCode = "BUILT-IN";

    public const string BuiltInSubject = "{{CompanyName}} - {{DocumentType}} {{DocumentNo}}";

    public const string BuiltInBody =
        "<p>Dear {{PartyName}},</p><p>Please find the details of <strong>{{DocumentType}} {{DocumentNo}}</strong> dated {{PostingDate}} for a total of <strong>{{CurrencyCode}} {{TotalAmountIncludingVat}}</strong>, due on {{DueDate}}.</p><p>Best regards,<br/>{{CompanyName}}</p>";

    private readonly IRepository<EmailTemplate, Guid> _templateRepository;
    private readonly IRepository<SentEmailLog, Guid> _logRepository;
    private readonly IRepository<Customer, Guid> _customerRepository;
    private readonly IRepository<Vendor, Guid> _vendorRepository;
    private readonly IRepository<CompanyInformation, Guid> _companyInfoRepository;
    private readonly ICurrentCompany _currentCompany;
    private readonly IEmailSender _emailSender;

    public DocumentEmailManager(
        IRepository<EmailTemplate, Guid> templateRepository,
        IRepository<SentEmailLog, Guid> logRepository,
        IRepository<Customer, Guid> customerRepository,
        IRepository<Vendor, Guid> vendorRepository,
        IRepository<CompanyInformation, Guid> companyInfoRepository,
        ICurrentCompany currentCompany,
        IEmailSender emailSender
    )
    {
        _templateRepository = templateRepository;
        _logRepository = logRepository;
        _customerRepository = customerRepository;
        _vendorRepository = vendorRepository;
        _companyInfoRepository = companyInfoRepository;
        _currentCompany = currentCompany;
        _emailSender = emailSender;
    }

    private DocumentAttachmentManager Attachments => LazyServiceProvider.LazyGetRequiredService<DocumentAttachmentManager>();

    public async Task<DocumentEmailPreview> BuildPreviewAsync(
        EmailDocumentType documentType,
        Guid documentId,
        string templateCode = null
    )
    {
        var context = await ResolveDocumentContextAsync(documentType, documentId);
        var template = await FindTemplateAsync(documentType, templateCode);
        var attachments = await Attachments.GetListAsync(context.AttachmentEntityName, documentId);

        return new DocumentEmailPreview
        {
            DocumentType = documentType,
            DocumentId = documentId,
            DocumentNo = context.DocumentNo,
            PartyNo = context.PartyNo,
            PartyName = context.PartyName,
            RecipientEmail = context.RecipientEmail,
            TemplateCode = template?.Code ?? BuiltInTemplateCode,
            Subject = EmailTemplate.Render(template?.SubjectTemplate ?? BuiltInSubject, context.Tokens, htmlEncode: false),
            BodyHtml = EmailTemplate.Render(template?.BodyHtmlTemplate ?? BuiltInBody, context.Tokens, htmlEncode: true),
            Attachments = attachments.Select(a => new DocumentEmailAttachment { Id = a.Id, FileName = a.FullFileName }).ToList(),
        };
    }

    public async Task<SentEmailLog> SendAsync(DocumentEmailRequest request)
    {
        var preview = await BuildPreviewAsync(request.DocumentType, request.DocumentId, request.TemplateCode);
        var toEmail = !request.RecipientEmail.IsNullOrWhiteSpace() ? request.RecipientEmail.Trim() : preview.RecipientEmail;

        if (toEmail.IsNullOrWhiteSpace())
        {
            throw new BusinessException(ErpErrorCodes.Emailing.RecipientEmailRequired)
                .WithData("documentNo", preview.DocumentNo)
                .WithData("partyNo", preview.PartyNo ?? "");
        }

        var subject = !request.Subject.IsNullOrWhiteSpace() ? request.Subject.Trim() : preview.Subject;
        var bodyHtml = !request.BodyHtml.IsNullOrWhiteSpace() ? request.BodyHtml : preview.BodyHtml;

        // Only files of this document can be sent with it.
        var wanted = (request.AttachmentIds ?? []).Distinct().ToList();
        var files = preview.Attachments.Where(a => wanted.Contains(a.Id)).ToList();

        var log = new SentEmailLog(
            GuidGenerator.Create(),
            request.DocumentType,
            request.DocumentId,
            preview.DocumentNo,
            preview.PartyNo,
            preview.PartyName,
            toEmail,
            request.CcEmail,
            subject,
            bodyHtml,
            preview.TemplateCode,
            files.Count,
            Clock.Now,
            request.SentByUserId
        );

        try
        {
            using var message = new MailMessage { Subject = subject, Body = bodyHtml, IsBodyHtml = true };
            message.To.Add(toEmail);
            if (!log.CcEmail.IsNullOrWhiteSpace())
            {
                message.CC.Add(log.CcEmail);
            }

            foreach (var file in files)
            {
                var content = await Attachments.GetContentAsync(file.Id);
                message.Attachments.Add(new Attachment(new MemoryStream(content), file.FileName));
            }

            await _emailSender.SendAsync(message);
            log.MarkSent();
        }
        // Missing SMTP settings, a refused address or a dropped connection: the attempt is logged as
        // failed with the reason, and the request still succeeds so the log row is kept.
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            Logger.LogWarning(ex, "Sending {DocumentNo} to {Recipient} failed.", preview.DocumentNo, toEmail);
            log.MarkFailed(ex.Message);
        }

        await _logRepository.InsertAsync(log, autoSave: true);
        return log;
    }

    /// <summary>Makes <paramref name="template"/> the only default of its document type.</summary>
    public async Task ClearOtherDefaultsAsync(EmailTemplate template)
    {
        if (!template.IsDefault)
        {
            return;
        }

        var others = await _templateRepository.GetListAsync(x =>
            x.DocumentType == template.DocumentType && x.IsDefault && x.Id != template.Id);
        foreach (var other in others)
        {
            other.ClearDefault();
            await _templateRepository.UpdateAsync(other, autoSave: true);
        }
    }

    private async Task<EmailTemplate> FindTemplateAsync(EmailDocumentType documentType, string templateCode)
    {
        if (!templateCode.IsNullOrWhiteSpace() && !string.Equals(templateCode.Trim(), BuiltInTemplateCode, StringComparison.OrdinalIgnoreCase))
        {
            var code = CodeTableEntity.NormalizeCode(templateCode);
            return await _templateRepository.FirstOrDefaultAsync(x => x.Code == code && x.Active)
                ?? throw new BusinessException(ErpErrorCodes.Emailing.TemplateNotFound).WithData("code", code);
        }

        return await _templateRepository.FirstOrDefaultAsync(x => x.DocumentType == documentType && x.IsDefault && x.Active)
            ?? await _templateRepository.FirstOrDefaultAsync(x => x.DocumentType == documentType && x.Active);
    }

    private sealed class DocumentContext
    {
        public string DocumentNo { get; init; }
        public string PartyNo { get; init; }
        public string PartyName { get; init; }
        public string RecipientEmail { get; init; }
        public string AttachmentEntityName { get; init; }
        public Dictionary<string, string> Tokens { get; init; }
    }

    private sealed record DocumentFacts(
        string DocumentNo,
        string PartyNo,
        string PartyName,
        string RecipientEmail,
        string CurrencyCode,
        decimal TotalAmount,
        decimal TotalAmountIncludingVat,
        DateTime PostingDate,
        DateTime? DueDate,
        string AttachmentEntityName
    );

    private async Task<DocumentContext> ResolveDocumentContextAsync(EmailDocumentType documentType, Guid documentId)
    {
        var facts = documentType switch
        {
            EmailDocumentType.SalesQuote
                or EmailDocumentType.SalesOrder
                or EmailDocumentType.SalesInvoice
                or EmailDocumentType.SalesCreditMemo => await SalesFactsAsync(documentId),
            EmailDocumentType.PostedSalesInvoice or EmailDocumentType.PostedSalesCreditMemo => await PostedSalesFactsAsync(documentId),
            EmailDocumentType.PurchaseQuote
                or EmailDocumentType.PurchaseOrder
                or EmailDocumentType.PurchaseInvoice => await PurchaseFactsAsync(documentId),
            EmailDocumentType.PostedPurchaseInvoice => await PostedPurchaseFactsAsync(documentId),
            _ => null,
        } ?? throw new BusinessException(ErpErrorCodes.Emailing.DocumentNotFound).WithData("id", documentId);

        var companyId = _currentCompany.Id;
        var companyInfo = companyId == null ? null : await _companyInfoRepository.FirstOrDefaultAsync(x => x.CompanyId == companyId.Value);
        var companyName = companyInfo?.Name ?? _currentCompany.Name ?? string.Empty;

        var currencyCode = facts.CurrencyCode;
        if (currencyCode.IsNullOrWhiteSpace())
        {
            var glSetup = await LazyServiceProvider.LazyGetRequiredService<GeneralLedgerSetupManager>().GetAsync();
            currencyCode = glSetup.LcyCode ?? string.Empty;
        }

        string Date(DateTime value) => value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

        var tokens = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["DocumentNo"] = facts.DocumentNo,
            ["DocumentType"] = DescribeDocumentType(documentType),
            ["PartyNo"] = facts.PartyNo,
            ["PartyName"] = facts.PartyName,
            ["CurrencyCode"] = currencyCode,
            ["TotalAmount"] = facts.TotalAmount.ToString("N2", CultureInfo.InvariantCulture),
            ["TotalAmountIncludingVat"] = facts.TotalAmountIncludingVat.ToString("N2", CultureInfo.InvariantCulture),
            ["PostingDate"] = Date(facts.PostingDate),
            ["DueDate"] = Date(facts.DueDate ?? facts.PostingDate),
            ["CompanyName"] = companyName,
        };

        return new DocumentContext
        {
            DocumentNo = facts.DocumentNo,
            PartyNo = facts.PartyNo,
            PartyName = facts.PartyName,
            RecipientEmail = facts.RecipientEmail,
            AttachmentEntityName = facts.AttachmentEntityName,
            Tokens = tokens,
        };
    }

    private async Task<DocumentFacts> SalesFactsAsync(Guid id)
    {
        var doc = await LazyServiceProvider.LazyGetRequiredService<IRepository<SalesHeader, Guid>>().FindAsync(id, includeDetails: false);
        if (doc == null)
        {
            return null;
        }

        var customer = await _customerRepository.FindAsync(doc.CustomerId);
        return new DocumentFacts(doc.No, doc.SellToCustomerNo, doc.SellToCustomerName, customer?.Email, doc.CurrencyCode,
            doc.TotalAmount, doc.TotalAmountIncludingVat, doc.PostingDate, doc.DueDate, nameof(SalesHeader));
    }

    private async Task<DocumentFacts> PostedSalesFactsAsync(Guid id)
    {
        var doc = await LazyServiceProvider.LazyGetRequiredService<IRepository<PostedSalesHeader, Guid>>().FindAsync(id, includeDetails: false);
        if (doc == null)
        {
            return null;
        }

        var customer = await _customerRepository.FindAsync(doc.CustomerId);
        return new DocumentFacts(doc.No, doc.SellToCustomerNo, doc.SellToCustomerName, customer?.Email, doc.CurrencyCode,
            doc.TotalAmount, doc.TotalAmountIncludingVat, doc.PostingDate, doc.DueDate, nameof(PostedSalesHeader));
    }

    private async Task<DocumentFacts> PurchaseFactsAsync(Guid id)
    {
        var doc = await LazyServiceProvider.LazyGetRequiredService<IRepository<PurchaseHeader, Guid>>().FindAsync(id, includeDetails: false);
        if (doc == null)
        {
            return null;
        }

        var vendor = await _vendorRepository.FindAsync(doc.VendorId);
        return new DocumentFacts(doc.No, doc.BuyFromVendorNo, doc.BuyFromVendorName, vendor?.Email, doc.CurrencyCode,
            doc.TotalAmount, doc.TotalAmountIncludingVat, doc.PostingDate, doc.DueDate, nameof(PurchaseHeader));
    }

    private async Task<DocumentFacts> PostedPurchaseFactsAsync(Guid id)
    {
        var doc = await LazyServiceProvider.LazyGetRequiredService<IRepository<PostedPurchaseHeader, Guid>>().FindAsync(id, includeDetails: false);
        if (doc == null)
        {
            return null;
        }

        var vendor = await _vendorRepository.FindAsync(doc.VendorId);
        return new DocumentFacts(doc.No, doc.BuyFromVendorNo, doc.BuyFromVendorName, vendor?.Email, doc.CurrencyCode,
            doc.TotalAmount, doc.TotalAmountIncludingVat, doc.PostingDate, doc.DueDate, nameof(PostedPurchaseHeader));
    }

    /// <summary>"Sales Invoice", "Posted Purchase Invoice": how the type reads in a subject line.</summary>
    public static string DescribeDocumentType(EmailDocumentType type) =>
        System.Text.RegularExpressions.Regex.Replace(type.ToString(), "(?<=[a-z])(?=[A-Z])", " ");
}
