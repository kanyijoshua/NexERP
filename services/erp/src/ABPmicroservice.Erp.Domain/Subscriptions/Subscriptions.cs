using System;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Companies;
using ABPmicroservice.Erp.Documents;
using ABPmicroservice.Erp.JobQueue;
using ABPmicroservice.Erp.Numbering;
using ABPmicroservice.Erp.Sales;
using ABPmicroservice.Erp.Sequences;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace ABPmicroservice.Erp.Subscriptions;

/// <summary>Subscription Plan: what a subscription invoices, at what price and how often.</summary>
public class SubscriptionPlan : CodeTableEntity
{
    /// <summary>Months between invoices: 1 is monthly, 12 yearly.</summary>
    public int BillingPeriodMonths { get; private set; } = 1;

    public decimal Price { get; private set; }
    public SubscriptionLineType LineType { get; private set; }

    /// <summary>The G/L account or item the invoice line is for.</summary>
    public string LineNo { get; private set; }

    /// <summary>Days before the first invoice; the first period starts after them.</summary>
    public int TrialDays { get; private set; }

    protected SubscriptionPlan() { }

    public SubscriptionPlan(Guid id, string code, string description)
        : base(id, code, description) { }

    public void Set(int billingPeriodMonths, decimal price, SubscriptionLineType lineType, string lineNo, int trialDays)
    {
        BillingPeriodMonths = Math.Clamp(billingPeriodMonths, 1, 60);
        Price = Math.Max(0m, price);
        LineType = lineType;
        LineNo = lineNo.IsNullOrWhiteSpace() ? null : Check.Length(lineNo.Trim(), nameof(lineNo), ErpDomainConsts.MaxNoLength);
        TrialDays = Math.Max(0, trialDays);
    }
}

/// <summary>
/// Customer Subscription: a customer on a plan, invoiced every billing period from its next billing
/// date until it is cancelled or its end date passes.
/// </summary>
public class CustomerSubscription : CompanyAggregateRoot, IHasNo
{
    public string No { get; private set; }
    public Guid CustomerId { get; private set; }
    public string CustomerNo { get; private set; }
    public string CustomerName { get; private set; }
    public string PlanCode { get; private set; }
    public decimal Quantity { get; private set; } = 1m;

    /// <summary>The price for this customer; zero takes the plan's.</summary>
    public decimal UnitPrice { get; private set; }

    public DateTime StartDate { get; private set; }
    public DateTime? EndDate { get; private set; }
    public DateTime NextBillingDate { get; private set; }
    public SubscriptionStatus Status { get; private set; }
    public string LastInvoiceNo { get; private set; }
    public DateTime? LastInvoiceDate { get; private set; }

    protected CustomerSubscription() { }

    public CustomerSubscription(Guid id, string no, Customer customer)
        : base(id)
    {
        No = Check.NotNullOrWhiteSpace(no, nameof(no), ErpDomainConsts.MaxNoLength).Trim().ToUpperInvariant();
        CustomerId = customer.Id;
        CustomerNo = customer.No;
        CustomerName = customer.Name;
        Status = SubscriptionStatus.Draft;
    }

    public void Set(SubscriptionPlan plan, decimal quantity, decimal unitPrice, DateTime startDate, DateTime? endDate)
    {
        EnsureStatus(SubscriptionStatus.Draft, SubscriptionStatus.Active, SubscriptionStatus.Paused);
        PlanCode = plan.Code;
        Quantity = quantity > 0m ? quantity : 1m;
        UnitPrice = Math.Max(0m, unitPrice);
        if (endDate.HasValue && endDate.Value.Date < startDate.Date)
        {
            throw new BusinessException(ErpErrorCodes.GeneralLedger.InvalidDateRange);
        }

        if (Status == SubscriptionStatus.Draft)
        {
            StartDate = startDate.Date;
            NextBillingDate = StartDate.AddDays(plan.TrialDays);
        }

        EndDate = endDate?.Date;
    }

    public void Activate() => Move(SubscriptionStatus.Active, SubscriptionStatus.Draft, SubscriptionStatus.Paused);

    public void Pause() => Move(SubscriptionStatus.Paused, SubscriptionStatus.Active);

    public void Cancel() => Move(SubscriptionStatus.Cancelled, SubscriptionStatus.Draft, SubscriptionStatus.Active, SubscriptionStatus.Paused);

    public decimal PriceOf(SubscriptionPlan plan) => UnitPrice != 0m ? UnitPrice : plan.Price;

    public bool IsDue(DateTime asOf) => Status == SubscriptionStatus.Active && NextBillingDate <= asOf.Date && (!EndDate.HasValue || NextBillingDate <= EndDate.Value);

    /// <summary>The period from the next billing date was invoiced: the next one starts a billing period later.</summary>
    internal void RecordInvoice(string invoiceNo, DateTime invoiceDate, int billingPeriodMonths)
    {
        LastInvoiceNo = invoiceNo;
        LastInvoiceDate = invoiceDate.Date;
        NextBillingDate = NextBillingDate.AddMonths(billingPeriodMonths);
        if (EndDate.HasValue && NextBillingDate > EndDate.Value)
        {
            Status = SubscriptionStatus.Expired;
        }
    }

    private void Move(SubscriptionStatus to, params SubscriptionStatus[] from)
    {
        EnsureStatus(from);
        Status = to;
    }

    private void EnsureStatus(params SubscriptionStatus[] allowed)
    {
        if (!allowed.Contains(Status))
        {
            throw new BusinessException(ErpErrorCodes.Subscriptions.SubscriptionStatusInvalid).WithData("no", No).WithData("status", Status.ToString());
        }
    }
}

/// <summary>
/// Recurring invoicing: a sales invoice for every period of an active subscription that has
/// started, posted at once when asked. A subscription catches up period by period.
/// </summary>
public class SubscriptionManager : DomainService
{
    private readonly IRepository<CustomerSubscription, Guid> _subscriptions;
    private readonly IRepository<SubscriptionPlan, Guid> _plans;

    private IRepository<T, Guid> Repo<T>() where T : class, Volo.Abp.Domain.Entities.IEntity<Guid> => LazyServiceProvider.LazyGetRequiredService<IRepository<T, Guid>>();
    private IEntryNoGenerator EntryNos => LazyServiceProvider.LazyGetRequiredService<IEntryNoGenerator>();

    public SubscriptionManager(IRepository<CustomerSubscription, Guid> subscriptions, IRepository<SubscriptionPlan, Guid> plans)
    {
        _subscriptions = subscriptions;
        _plans = plans;
    }

    public async Task<string> NextNoAsync() => $"SUB{await EntryNos.NextAsync(ErpSequenceNames.SubscriptionNo):D5}";

    public async Task<SubscriptionPlan> GetPlanAsync(string code)
    {
        var normalized = CodeTableEntity.NormalizeCode(code);
        return await _plans.FirstOrDefaultAsync(p => p.Code == normalized)
            ?? throw new BusinessException(ErpErrorCodes.Subscriptions.PlanNotFound).WithData("code", normalized ?? "");
    }

    /// <summary>Invoices every due period of active subscriptions (or just one); returns the invoices made.</summary>
    public async Task<int> InvoiceDueAsync(DateTime asOf, bool post, Guid? subscriptionId = null)
    {
        var due = subscriptionId.HasValue
            ? [await _subscriptions.GetAsync(subscriptionId.Value)]
            : await _subscriptions.GetListAsync(s => s.Status == SubscriptionStatus.Active && s.NextBillingDate <= asOf.Date);

        var invoices = 0;
        foreach (var subscription in due.OrderBy(s => s.No))
        {
            var plan = await GetPlanAsync(subscription.PlanCode);
            while (subscription.IsDue(asOf))
            {
                await InvoiceAsync(subscription, plan, post);
                invoices++;
            }

            await _subscriptions.UpdateAsync(subscription, autoSave: true);
        }

        return invoices;
    }

    private async Task InvoiceAsync(CustomerSubscription subscription, SubscriptionPlan plan, bool post)
    {
        if (plan.LineNo == null)
        {
            throw new BusinessException(ErpErrorCodes.Subscriptions.PlanLineMissing).WithData("code", plan.Code);
        }

        var customer = await Repo<Customer>().GetAsync(subscription.CustomerId);
        var date = subscription.NextBillingDate;
        var series = (await LazyServiceProvider.LazyGetRequiredService<SalesReceivablesSetupManager>().GetAsync()).GetDocumentNos(SalesDocumentType.Invoice);
        var no = series.IsNullOrWhiteSpace()
            ? $"{subscription.No}-{date:yyyyMMdd}"
            : await LazyServiceProvider.LazyGetRequiredService<NoSeriesManager>().GetNextNoAsync(series, date);

        var invoice = new SalesHeader(GuidGenerator.Create(), SalesDocumentType.Invoice, no, customer.Id, customer.No, customer.Name, date);
        invoice.SetDates(date, null);
        invoice.SetPaymentTerms(customer.PaymentTermsCode);
        invoice.SetExternalDocumentNo(subscription.No);
        var periodEnd = date.AddMonths(plan.BillingPeriodMonths).AddDays(-1);
        var description = $"{plan.Description ?? plan.Code} {date:yyyy-MM-dd}..{periodEnd:yyyy-MM-dd}".Truncate(ErpDomainConsts.MaxDescriptionLength);
        invoice.AddLine(
            GuidGenerator.Create(),
            plan.LineType == SubscriptionLineType.Item ? DocumentLineType.Item : DocumentLineType.GLAccount,
            plan.LineNo,
            description,
            subscription.Quantity,
            subscription.PriceOf(plan)
        );
        await LazyServiceProvider.LazyGetRequiredService<DocumentVatCalculator>().ApplyAsync(invoice, customer);
        await Repo<SalesHeader>().InsertAsync(invoice, autoSave: true);

        var invoiceNo = invoice.No;
        if (post)
        {
            var result = await LazyServiceProvider.LazyGetRequiredService<SalesPostingEngine>().PostAsync(invoice.Id);
            invoiceNo = result.Invoice?.No ?? invoiceNo;
        }

        subscription.RecordInvoice(invoiceNo, date, plan.BillingPeriodMonths);
    }
}

/// <summary>Invoices every subscription period that has started: run it daily.</summary>
public class SubscriptionInvoicingJobHandler : IJobHandler, ITransientDependency
{
    public string JobType => "InvoiceSubscriptions";
    public string DisplayName => "Invoice Subscriptions";
    public string Description => "Creates and posts a sales invoice for every active customer subscription period that has started.";
    public string DefaultParametersJson => "{}";

    private readonly SubscriptionManager _manager;

    public SubscriptionInvoicingJobHandler(SubscriptionManager manager)
    {
        _manager = manager;
    }

    public async Task<JobExecutionResult> ExecuteAsync(JobExecutionContext context)
    {
        var invoiced = await _manager.InvoiceDueAsync(DateTime.Today, post: true);
        return JobExecutionResult.Ok(invoiced, $"Invoiced {invoiced} subscription period(s).");
    }
}
