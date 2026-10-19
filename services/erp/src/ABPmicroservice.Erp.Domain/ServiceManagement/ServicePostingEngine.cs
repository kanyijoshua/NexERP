using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Documents;
using ABPmicroservice.Erp.Finance;
using ABPmicroservice.Erp.Inventory;
using ABPmicroservice.Erp.Numbering;
using ABPmicroservice.Erp.Projects;
using ABPmicroservice.Erp.Sales;
using ABPmicroservice.Erp.Sequences;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace ABPmicroservice.Erp.ServiceManagement;

/// <summary>What posting a service order did.</summary>
public sealed class ServicePostingResult
{
    /// <summary>The posted sales invoice; null when every line was free labour or cost under warranty.</summary>
    public string InvoiceNo { get; init; }

    public int LineCount { get; init; }
    public decimal Amount { get; init; }
    public decimal WarrantyAmount { get; init; }
}

/// <summary>
/// Posts service orders and invoices service contracts. The customer side goes through a sales invoice
/// posted by the sales engine, so parts leave stock at cost and the customer, revenue and VAT post exactly
/// as for any sale. Labour is sold on the service revenue account and written to the resource ledger.
/// Warranty lines are free: parts still leave stock, and the value goes to the warranty ledger.
/// </summary>
public class ServicePostingEngine : DomainService
{
    private T Repo<T>() where T : class => LazyServiceProvider.LazyGetRequiredService<T>();

    private IEntryNoGenerator EntryNos => Repo<IEntryNoGenerator>();

    public async Task<ServiceMgtSetup> GetSetupAsync() =>
        await Repo<IRepository<ServiceMgtSetup, Guid>>().FirstOrDefaultAsync() ?? throw new BusinessException(ErpErrorCodes.Service.SetupMissing);

    public async Task<ServicePostingResult> PostOrderAsync(Guid orderId, DateTime postingDate)
    {
        var orders = Repo<IRepository<ServiceOrder, Guid>>();
        var order = await orders.GetAsync(orderId, includeDetails: true);
        order.EnsureNotFinished();
        if (order.Status == ServiceOrderStatus.OnHold)
        {
            throw new BusinessException(ErpErrorCodes.Service.OrderOnHold).WithData("no", order.No);
        }

        var lines = order.GetOpenLines().OrderBy(l => l.LineNo).ToList();
        if (lines.Count == 0)
        {
            throw new BusinessException(ErpErrorCodes.Service.NothingToPost).WithData("no", order.No);
        }

        var setup = await GetSetupAsync();
        if (lines.Any(l => l.Type == ServiceLineType.Resource && !l.Warranty && l.LineAmount != 0m) && setup.ServiceRevenueAccountNo.IsNullOrWhiteSpace())
        {
            throw new BusinessException(ErpErrorCodes.Service.SetupAccountMissing).WithData("account", nameof(ServiceMgtSetup.ServiceRevenueAccountNo));
        }

        var customer = await Repo<IRepository<Customer, Guid>>().GetAsync(order.CustomerId);
        var header = await NewInvoiceAsync(customer, postingDate, order.No);

        // Parts always go on the invoice (the stock has to move); labour and costs only when charged.
        foreach (var line in lines)
        {
            var quantity = line.RemainingQuantity;
            var price = line.Warranty ? 0m : line.UnitPrice;
            var discount = line.Warranty ? 0m : line.LineDiscountPct;
            switch (line.Type)
            {
                case ServiceLineType.Item:
                    var itemLine = header.AddLine(GuidGenerator.Create(), DocumentLineType.Item, line.No, line.Description, quantity, price, discount);
                    itemLine.SetLocation(line.LocationCode);
                    itemLine.SetQtyPerUnitOfMeasure(1m);
                    break;
                case ServiceLineType.Resource when price != 0m:
                    header.AddLine(GuidGenerator.Create(), DocumentLineType.GLAccount, setup.ServiceRevenueAccountNo, line.Description ?? line.No, quantity, price, discount).SetQtyPerUnitOfMeasure(1m);
                    break;
                case ServiceLineType.Cost when price != 0m:
                    header.AddLine(GuidGenerator.Create(), DocumentLineType.GLAccount, line.No, line.Description ?? line.No, quantity, price, discount).SetQtyPerUnitOfMeasure(1m);
                    break;
            }
        }

        var invoiceNo = await PostInvoiceAsync(header, customer);
        var documentNo = invoiceNo ?? order.No;

        decimal amount = 0m, warrantyAmount = 0m;
        var items = Repo<IRepository<Item, Guid>>();
        var serviceItems = order.ItemLines.ToDictionary(l => l.ServiceItemNo, l => l);
        foreach (var line in lines)
        {
            var quantity = line.RemainingQuantity;
            var unitCost = line.UnitCost;
            if (unitCost == 0m && line.Type == ServiceLineType.Item)
            {
                unitCost = (await items.FirstOrDefaultAsync(i => i.No == line.No))?.UnitCost ?? 0m;
            }

            var cost = Math.Round(quantity * unitCost, 2, MidpointRounding.AwayFromZero);
            var lineAmount = Math.Round(quantity * line.UnitPrice * (1m - line.LineDiscountPct / 100m), 2, MidpointRounding.AwayFromZero);
            var charged = line.Warranty ? 0m : lineAmount;
            amount += charged;

            await Repo<IRepository<ServiceLedgerEntry, Guid>>().InsertAsync(new ServiceLedgerEntry(GuidGenerator.Create())
            {
                EntryNo = await EntryNos.NextAsync(ErpSequenceNames.ServiceLedgerEntry),
                EntryType = ServiceLedgerEntryType.Sale,
                PostingDate = postingDate.Date,
                DocumentNo = documentNo,
                ServiceOrderNo = order.No,
                ServiceContractNo = order.ContractNo,
                ServiceItemNo = line.ServiceItemNo,
                CustomerNo = order.CustomerNo,
                Type = line.Type,
                No = line.No,
                Description = line.Description,
                Quantity = quantity,
                CostAmount = cost,
                Amount = charged,
                Warranty = line.Warranty,
            });

            if (line.Warranty)
            {
                warrantyAmount += lineAmount;
                var itemLine = line.ServiceItemNo == null ? null : serviceItems.GetValueOrDefault(line.ServiceItemNo);
                await Repo<IRepository<WarrantyLedgerEntry, Guid>>().InsertAsync(new WarrantyLedgerEntry(GuidGenerator.Create())
                {
                    EntryNo = await EntryNos.NextAsync(ErpSequenceNames.WarrantyLedgerEntry),
                    PostingDate = postingDate.Date,
                    DocumentNo = documentNo,
                    ServiceOrderNo = order.No,
                    ServiceItemNo = line.ServiceItemNo,
                    ItemNo = itemLine?.ItemNo,
                    SerialNo = itemLine?.SerialNo,
                    CustomerNo = order.CustomerNo,
                    Type = line.Type,
                    No = line.No,
                    Description = line.Description,
                    Quantity = quantity,
                    Amount = lineAmount,
                    CostAmount = cost,
                });
            }

            if (line.Type == ServiceLineType.Resource)
            {
                // Labour is a usage of the technician's time, like time posted to a project.
                await Repo<IRepository<ResourceLedgerEntry, Guid>>().InsertAsync(new ResourceLedgerEntry(GuidGenerator.Create())
                {
                    EntryNo = await EntryNos.NextAsync(ErpSequenceNames.ResourceLedgerEntry),
                    ResourceNo = line.No,
                    PostingDate = postingDate.Date,
                    DocumentNo = documentNo,
                    Description = line.Description,
                    Quantity = quantity,
                    UnitCost = unitCost,
                    TotalCost = cost,
                    UnitPrice = line.Warranty ? 0m : line.UnitPrice,
                    TotalPrice = charged,
                    Chargeable = !line.Warranty,
                });
            }

            line.RecordPosted(quantity);
        }

        order.RecordPosting(invoiceNo, postingDate);
        await orders.UpdateAsync(order, autoSave: true);

        return new ServicePostingResult { InvoiceNo = invoiceNo, LineCount = lines.Count, Amount = amount, WarrantyAmount = warrantyAmount };
    }

    /// <summary>
    /// Invoices every period of signed contracts that has started by a date (one contract, or all of
    /// them), each on its own sales invoice. Returns how many periods were invoiced.
    /// </summary>
    public async Task<int> InvoiceContractsAsync(DateTime asOfDate, Guid? contractId = null)
    {
        var contracts = Repo<IRepository<ServiceContract, Guid>>();
        var query = await contracts.WithDetailsAsync(c => c.Lines);
        var due = query.Where(c => c.Status == ServiceContractStatus.Signed && c.NextInvoiceDate != null && c.NextInvoiceDate <= asOfDate.Date);
        if (contractId.HasValue)
        {
            due = due.Where(c => c.Id == contractId.Value);
        }

        var list = await AsyncExecuter.ToListAsync(due.OrderBy(c => c.No));
        if (list.Count == 0)
        {
            return 0;
        }

        var setup = await GetSetupAsync();
        if (setup.ContractRevenueAccountNo.IsNullOrWhiteSpace())
        {
            throw new BusinessException(ErpErrorCodes.Service.SetupAccountMissing).WithData("account", nameof(ServiceMgtSetup.ContractRevenueAccountNo));
        }

        var count = 0;
        foreach (var contract in list)
        {
            var customer = await Repo<IRepository<Customer, Guid>>().GetAsync(contract.CustomerId);

            // A contract behind by several periods catches up, one invoice per period.
            while (contract.IsDue(asOfDate))
            {
                var from = contract.NextInvoiceDate!.Value;
                var to = from.AddMonths((int)contract.InvoicePeriod).AddDays(-1);
                var amount = contract.PeriodAmount;
                var header = await NewInvoiceAsync(customer, from, contract.No);
                header.AddLine(GuidGenerator.Create(), DocumentLineType.GLAccount, setup.ContractRevenueAccountNo, $"{contract.No} {from:yyyy-MM-dd}..{to:yyyy-MM-dd}", 1m, amount).SetQtyPerUnitOfMeasure(1m);
                var invoiceNo = await PostInvoiceAsync(header, customer);

                await Repo<IRepository<ServiceLedgerEntry, Guid>>().InsertAsync(new ServiceLedgerEntry(GuidGenerator.Create())
                {
                    EntryNo = await EntryNos.NextAsync(ErpSequenceNames.ServiceLedgerEntry),
                    EntryType = ServiceLedgerEntryType.Contract,
                    PostingDate = from,
                    DocumentNo = invoiceNo,
                    ServiceContractNo = contract.No,
                    CustomerNo = contract.CustomerNo,
                    Type = ServiceLineType.Cost,
                    No = setup.ContractRevenueAccountNo,
                    Description = contract.Description,
                    Quantity = 1m,
                    Amount = amount,
                });

                contract.RecordInvoiced(invoiceNo);
                count++;
            }

            await contracts.UpdateAsync(contract, autoSave: true);
        }

        return count;
    }

    private async Task<SalesHeader> NewInvoiceAsync(Customer customer, DateTime postingDate, string sourceNo)
    {
        if (customer.Blocked)
        {
            throw new BusinessException(ErpErrorCodes.Service.CustomerBlocked).WithData("no", customer.No);
        }

        var salesSetup = await Repo<SalesReceivablesSetupManager>().GetAsync();
        var series = salesSetup.GetDocumentNos(SalesDocumentType.Invoice);
        string no;
        if (series.IsNullOrWhiteSpace())
        {
            var headers = Repo<IRepository<SalesHeader, Guid>>();
            var taken = await headers.CountAsync(h => h.DocumentType == SalesDocumentType.Invoice && h.No.StartsWith(sourceNo));
            no = $"{sourceNo}-I{taken + 1}";
        }
        else
        {
            no = await Repo<NoSeriesManager>().GetNextNoAsync(series, postingDate);
        }

        var header = new SalesHeader(GuidGenerator.Create(), SalesDocumentType.Invoice, no, customer.Id, customer.No, customer.Name, postingDate.Date);
        header.SetDates(postingDate.Date, await Repo<PaymentTermsManager>().CalculateDueDateAsync(customer.PaymentTermsCode, postingDate.Date));
        header.SetCurrency(customer.CurrencyCode);
        header.SetPaymentTerms(customer.PaymentTermsCode);
        header.SetExternalDocumentNo(sourceNo);
        return header;
    }

    /// <summary>Posts the invoice through the sales engine; null when it has no lines to post.</summary>
    private async Task<string> PostInvoiceAsync(SalesHeader header, Customer customer)
    {
        if (header.Lines.Count == 0)
        {
            return null;
        }

        await Repo<DocumentVatCalculator>().ApplyAsync(header, customer);
        await Repo<IRepository<SalesHeader, Guid>>().InsertAsync(header, autoSave: true);
        var result = await Repo<SalesPostingEngine>().PostAsync(header.Id);
        return result.Invoice?.No ?? header.No;
    }
}
