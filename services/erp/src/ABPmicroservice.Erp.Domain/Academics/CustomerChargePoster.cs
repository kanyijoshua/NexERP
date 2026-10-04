using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Finance;
using Volo.Abp;
using Volo.Abp.Domain.Services;

namespace ABPmicroservice.Erp.Academics;

/// <summary>One income line of a customer charge: the account credited and the amount.</summary>
public record CustomerChargeLine(string GLAccountNo, string Description, decimal Amount);

/// <summary>
/// Posts a charge to a customer: the total debited to the customer as an invoice, each line
/// credited to its income account. Used by the campus services that bill anyone, not only
/// students (laundry, short courses).
/// </summary>
public class CustomerChargePoster : DomainService
{
    private readonly GenJnlPostLine _genJnlPostLine;
    private readonly GLRegisterManager _registerManager;
    private readonly GeneralLedgerSetupManager _glSetupManager;

    public CustomerChargePoster(GenJnlPostLine genJnlPostLine, GLRegisterManager registerManager, GeneralLedgerSetupManager glSetupManager)
    {
        _genJnlPostLine = genJnlPostLine;
        _registerManager = registerManager;
        _glSetupManager = glSetupManager;
    }

    /// <returns>The total charged.</returns>
    public async Task<decimal> PostAsync(
        string sourceCode,
        string documentNo,
        DateTime postingDate,
        string customerNo,
        string description,
        IReadOnlyList<CustomerChargeLine> lines
    )
    {
        await _glSetupManager.CheckPostingDateAsync(postingDate);

        var charged = lines.Where(l => l.Amount != 0m).ToList();
        if (charged.Count == 0)
        {
            throw new BusinessException(ErpErrorCodes.Academics.NothingToPost).WithData("documentNo", documentNo);
        }

        // Checked before anything is written: the charge is posted whole or not at all.
        var missing = charged.FirstOrDefault(l => l.GLAccountNo.IsNullOrWhiteSpace());
        if (missing != null)
        {
            throw new BusinessException(ErpErrorCodes.PostingSetup.AccountMissing).WithData("field", "G/L Account No.").WithData("setup", missing.Description ?? documentNo);
        }

        var register = await _registerManager.OpenAsync(postingDate, sourceCode, documentNo);
        var context = new GLPostingContext(register, sourceCode);

        foreach (var line in charged)
        {
            await _genJnlPostLine.PostGLDirectAsync(
                line.GLAccountNo,
                postingDate,
                GLEntryDocumentType.None,
                documentNo,
                (line.Description.IsNullOrWhiteSpace() ? description : line.Description).Truncate(ErpDomainConsts.MaxDescriptionLength),
                -line.Amount,
                customerNo,
                context: context
            );
        }

        var total = charged.Sum(l => l.Amount);
        await _genJnlPostLine.PostLineAsync(
            new GenJournalLine(
                GuidGenerator.Create(),
                Guid.Empty,
                1,
                postingDate,
                GLEntryDocumentType.Invoice,
                documentNo,
                GenJournalAccountType.Customer,
                customerNo,
                description.Truncate(ErpDomainConsts.MaxDescriptionLength),
                total
            ),
            context
        );

        await _registerManager.CloseAsync(register);
        return total;
    }
}
