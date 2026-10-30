using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Companies;
using ABPmicroservice.Erp.Sequences;
using Microsoft.Extensions.Logging;
using Volo.Abp;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using Volo.Abp.Emailing;

namespace ABPmicroservice.Erp.Crm;

/// <summary>Segment: a list of contacts picked by criteria or by hand, used as the audience of a campaign.</summary>
public class Segment : CompanyAggregateRoot, IHasNo
{
    public string No { get; private set; }
    public string Description { get; private set; }

    public Collection<SegmentLine> Lines { get; private set; } = new();

    protected Segment() { }

    public Segment(Guid id, string no, string description)
        : base(id)
    {
        No = Check.NotNullOrWhiteSpace(no, nameof(no), ErpDomainConsts.MaxNoLength).Trim().ToUpperInvariant();
        SetDescription(description);
    }

    public void SetDescription(string description) =>
        Description = Check.Length(description, nameof(description), ErpDomainConsts.MaxDescriptionLength);

    /// <summary>Adds the contact unless it is in the segment already; returns whether it was added.</summary>
    public bool Add(Guid id, Contact contact)
    {
        if (Lines.Any(l => l.ContactNo == contact.No))
        {
            return false;
        }

        Lines.Add(new SegmentLine(id, Id, contact));
        return true;
    }
}

public class SegmentLine : Entity<Guid>
{
    public Guid SegmentId { get; private set; }
    public string ContactNo { get; private set; }
    public string ContactName { get; private set; }
    public string Email { get; private set; }

    protected SegmentLine() { }

    public SegmentLine(Guid id, Guid segmentId, Contact contact)
        : base(id)
    {
        SegmentId = segmentId;
        ContactNo = contact.No;
        ContactName = contact.Name;
        Email = contact.Email;
    }
}

/// <summary>Campaign Mailing Entry: one email of a mass mailing to a campaign's targets, and whether it went.</summary>
public class CampaignMailingEntry : CompanyEntity
{
    public string CampaignNo { get; private set; }
    public string ContactNo { get; private set; }
    public string ContactName { get; private set; }
    public string Email { get; private set; }
    public string Subject { get; private set; }
    public DateTime SentAt { get; private set; }
    public MailingStatus Status { get; private set; }
    public string ErrorMessage { get; private set; }

    protected CampaignMailingEntry() { }

    public CampaignMailingEntry(Guid id, string campaignNo, Contact contact, string subject, DateTime sentAt, MailingStatus status, string errorMessage)
        : base(id)
    {
        CampaignNo = campaignNo;
        ContactNo = contact.No;
        ContactName = contact.Name;
        Email = contact.Email;
        Subject = subject.Truncate(ErpDomainConsts.MaxDescriptionLength);
        SentAt = sentAt;
        Status = status;
        ErrorMessage = errorMessage?.Truncate(500);
    }
}

/// <summary>Who a mass mailing reached.</summary>
public sealed class MailingResult
{
    public int Sent { get; init; }
    public int Failed { get; init; }
    public int WithoutEmail { get; init; }
}

/// <summary>
/// Marketing: builds segments from contacts by criteria, turns a segment into a campaign's targets,
/// and mails the targets one by one. A failed email is logged, not thrown, and every contact
/// reached gets an interaction log entry.
/// </summary>
public class MarketingManager : DomainService
{
    private readonly IRepository<Segment, Guid> _segments;
    private readonly IRepository<Contact, Guid> _contacts;
    private readonly IRepository<Campaign, Guid> _campaigns;
    private readonly IRepository<CampaignTarget, Guid> _targets;

    private IEntryNoGenerator EntryNos => LazyServiceProvider.LazyGetRequiredService<IEntryNoGenerator>();

    public MarketingManager(IRepository<Segment, Guid> segments, IRepository<Contact, Guid> contacts, IRepository<Campaign, Guid> campaigns, IRepository<CampaignTarget, Guid> targets)
    {
        _segments = segments;
        _contacts = contacts;
        _campaigns = campaigns;
        _targets = targets;
    }

    public async Task<string> NextNoAsync() => $"SEG{await EntryNos.NextAsync(ErpSequenceNames.SegmentNo):D5}";

    /// <summary>Adds the contacts matching every criterion given (blank criteria match all); returns the contacts added.</summary>
    public async Task<int> AddContactsAsync(Guid segmentId, string city, string salespersonCode, ContactType? type, bool onlyWithEmail)
    {
        var segment = await _segments.GetAsync(segmentId, includeDetails: true);
        var cityFilter = city?.Trim();
        var salesperson = CodeTableEntity.NormalizeCode(salespersonCode);
        var contacts = (await _contacts.GetListAsync())
            .Where(c => cityFilter.IsNullOrEmpty() || string.Equals(c.City, cityFilter, StringComparison.OrdinalIgnoreCase))
            .Where(c => salesperson == null || c.SalespersonCode == salesperson)
            .Where(c => !type.HasValue || c.Type == type.Value)
            .Where(c => !onlyWithEmail || !c.Email.IsNullOrWhiteSpace())
            .OrderBy(c => c.No)
            .ToList();

        var added = contacts.Count(c => segment.Add(GuidGenerator.Create(), c));
        await _segments.UpdateAsync(segment, autoSave: true);
        return added;
    }

    /// <summary>Makes every contact of the segment a target of the campaign; returns the targets added.</summary>
    public async Task<int> ApplyToCampaignAsync(Guid segmentId, string campaignNo)
    {
        var segment = await _segments.GetAsync(segmentId, includeDetails: true);
        if (segment.Lines.Count == 0)
        {
            throw new BusinessException(ErpErrorCodes.Crm.SegmentHasNoLines).WithData("no", segment.No);
        }

        var campaign = await GetCampaignAsync(campaignNo);
        var existing = (await _targets.GetListAsync(t => t.CampaignNo == campaign.No)).Select(t => t.ContactNo).ToHashSet();
        var added = 0;
        foreach (var line in segment.Lines.Where(l => !existing.Contains(l.ContactNo)))
        {
            var contactNo = line.ContactNo;
            var contact = await _contacts.FirstOrDefaultAsync(c => c.No == contactNo);
            if (contact != null)
            {
                await _targets.InsertAsync(new CampaignTarget(GuidGenerator.Create(), campaign.No, contact), autoSave: true);
                added++;
            }
        }

        return added;
    }

    /// <summary>Emails every target of the campaign that has an address, logging each attempt.</summary>
    public async Task<MailingResult> SendMailingAsync(string campaignNo, string subject, string bodyHtml)
    {
        var campaign = await GetCampaignAsync(campaignNo);
        var targets = (await _targets.GetListAsync(t => t.CampaignNo == campaign.No)).OrderBy(t => t.ContactNo).ToList();
        var sender = LazyServiceProvider.LazyGetRequiredService<IEmailSender>();
        var entries = LazyServiceProvider.LazyGetRequiredService<IRepository<CampaignMailingEntry, Guid>>();
        var interactions = LazyServiceProvider.LazyGetRequiredService<IRepository<InteractionLogEntry, Guid>>();
        int sent = 0, failed = 0, withoutEmail = 0;

        foreach (var target in targets)
        {
            var contactNo = target.ContactNo;
            var contact = await _contacts.FirstOrDefaultAsync(c => c.No == contactNo);
            if (contact == null)
            {
                continue;
            }

            if (contact.Email.IsNullOrWhiteSpace())
            {
                withoutEmail++;
                await entries.InsertAsync(new CampaignMailingEntry(GuidGenerator.Create(), campaign.No, contact, subject, Clock.Now, MailingStatus.NoEmail, null), autoSave: true);
                continue;
            }

            try
            {
                using var message = new MailMessage { Subject = subject, Body = bodyHtml.Replace("{{Name}}", System.Net.WebUtility.HtmlEncode(contact.Name ?? "")), IsBodyHtml = true };
                message.To.Add(contact.Email);
                await sender.SendAsync(message);
                sent++;
                await entries.InsertAsync(new CampaignMailingEntry(GuidGenerator.Create(), campaign.No, contact, subject, Clock.Now, MailingStatus.Sent, null), autoSave: true);
                var interaction = new InteractionLogEntry(GuidGenerator.Create(), Clock.Now, contact, null, $"Email: {subject}".Truncate(ErpDomainConsts.MaxDescriptionLength));
                interaction.SetDetails(null, campaign.No, 0, 0m, campaign.SalespersonCode);
                await interactions.InsertAsync(interaction, autoSave: true);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                Logger.LogWarning(ex, "Mailing {Campaign} to {Email} failed.", campaign.No, contact.Email);
                failed++;
                await entries.InsertAsync(new CampaignMailingEntry(GuidGenerator.Create(), campaign.No, contact, subject, Clock.Now, MailingStatus.Failed, ex.Message), autoSave: true);
            }
        }

        return new MailingResult { Sent = sent, Failed = failed, WithoutEmail = withoutEmail };
    }

    private async Task<Campaign> GetCampaignAsync(string campaignNo)
    {
        var no = campaignNo?.Trim().ToUpperInvariant();
        return await _campaigns.FirstOrDefaultAsync(c => c.No == no)
            ?? throw new BusinessException(ErpErrorCodes.Crm.CampaignNotFound).WithData("no", no ?? "");
    }
}
