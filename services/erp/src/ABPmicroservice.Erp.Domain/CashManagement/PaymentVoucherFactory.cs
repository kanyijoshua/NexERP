using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Finance;
using ABPmicroservice.Erp.Numbering;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace ABPmicroservice.Erp.CashManagement;

/// <summary>One amount of a voucher raised by another document.</summary>
public record PaymentVoucherRequestLine(GenJournalAccountType AccountType, string AccountNo, string Description, decimal Amount, string AppliesToDocNo = null);

/// <summary>
/// Raises payment vouchers for other documents: a student refund, a member's exit benefit, a
/// pension payroll. The voucher is left open, numbered from the Cash Management Setup, for the
/// cashier to choose the bank account and take it through approval like any other.
/// </summary>
public class PaymentVoucherFactory : DomainService
{
    private readonly IRepository<PaymentVoucherHeader, Guid> _headers;
    private readonly IRepository<PaymentVoucherLine, Guid> _lines;
    private readonly CashManagementSetupManager _setupManager;
    private readonly PaymentVoucherEngine _engine;
    private readonly NoSeriesManager _noSeriesManager;

    public PaymentVoucherFactory(
        IRepository<PaymentVoucherHeader, Guid> headers,
        IRepository<PaymentVoucherLine, Guid> lines,
        CashManagementSetupManager setupManager,
        PaymentVoucherEngine engine,
        NoSeriesManager noSeriesManager
    )
    {
        _headers = headers;
        _lines = lines;
        _setupManager = setupManager;
        _engine = engine;
        _noSeriesManager = noSeriesManager;
    }

    /// <summary>The next voucher number; the setup must name the series, since nobody is there to type one.</summary>
    public async Task<string> NextNoAsync(DateTime date)
    {
        var setup = await _setupManager.GetAsync();
        if (setup.PaymentVoucherNos.IsNullOrWhiteSpace())
        {
            throw new BusinessException(ErpErrorCodes.CashManagement.VoucherNosMissing);
        }

        return (await _noSeriesManager.GetNextNoAsync(setup.PaymentVoucherNos, date)).ToUpperInvariant();
    }

    public async Task<PaymentVoucherHeader> CreateAsync(
        string sourceType,
        string sourceNo,
        DateTime date,
        string payee,
        string narration,
        IReadOnlyList<PaymentVoucherRequestLine> lines
    )
    {
        var header = new PaymentVoucherHeader(GuidGenerator.Create(), await NextNoAsync(date), date, date);
        header.SetPayee(payee.Truncate(PaymentVoucherHeader.MaxPayeeLength), null, narration.Truncate(PaymentVoucherHeader.MaxNarrationLength));
        header.SetSource(sourceType, sourceNo);
        await _headers.InsertAsync(header, autoSave: true);

        var lineNo = 0;
        foreach (var request in lines)
        {
            lineNo += 10000;
            var line = new PaymentVoucherLine(GuidGenerator.Create(), header.No, lineNo);
            line.SetAccount(null, request.AccountType, request.AccountNo, await _engine.GetAccountNameAsync(request.AccountType, request.AccountNo));
            line.SetDetails(request.Description.Truncate(ErpDomainConsts.MaxDescriptionLength), request.AppliesToDocNo);
            line.SetAmounts(request.Amount, 0m, null, null, null);
            await _lines.InsertAsync(line, autoSave: true);
        }

        await _engine.UpdateTotalsAsync(header);
        return header;
    }
}
