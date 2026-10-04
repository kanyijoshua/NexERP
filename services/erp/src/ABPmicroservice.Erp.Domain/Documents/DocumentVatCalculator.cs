using System;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Finance;
using ABPmicroservice.Erp.Inventory;
using ABPmicroservice.Erp.Purchasing;
using ABPmicroservice.Erp.Sales;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace ABPmicroservice.Erp.Documents;

/// <summary>
/// Gives every line of a sales or purchase document its VAT when a line
/// is validated: the party's VAT business group and the item's or G/L account's VAT product group
/// pick a VAT Posting Setup. A line whose product has no VAT group carries no VAT.
/// </summary>
public class DocumentVatCalculator : DomainService
{
    private readonly IRepository<Item, Guid> _itemRepository;
    private readonly IRepository<GLAccount, Guid> _glAccountRepository;
    private readonly PostingSetupManager _postingSetupManager;

    public DocumentVatCalculator(
        IRepository<Item, Guid> itemRepository,
        IRepository<GLAccount, Guid> glAccountRepository,
        PostingSetupManager postingSetupManager
    )
    {
        _itemRepository = itemRepository;
        _glAccountRepository = glAccountRepository;
        _postingSetupManager = postingSetupManager;
    }

    public async Task ApplyAsync(SalesHeader header, Customer customer)
    {
        foreach (var line in header.Lines)
        {
            line.SetVat(await FindSetupAsync(customer.VatBusPostingGroup, line.Type, line.No));
        }

        header.RecalculateTotals();
    }

    public async Task ApplyAsync(PurchaseHeader header, Vendor vendor)
    {
        foreach (var line in header.Lines)
        {
            line.SetVat(await FindSetupAsync(vendor.VatBusPostingGroup, line.Type, line.No));
        }

        header.RecalculateTotals();
    }

    private async Task<VatPostingSetup> FindSetupAsync(string vatBusPostingGroup, DocumentLineType type, string no)
    {
        var vatProdPostingGroup = await VatProdPostingGroupOfAsync(type, no);
        return vatProdPostingGroup == null
            ? null
            : await _postingSetupManager.GetVatPostingSetupAsync(vatBusPostingGroup, vatProdPostingGroup);
    }

    private async Task<string> VatProdPostingGroupOfAsync(DocumentLineType type, string no)
    {
        if (no.IsNullOrWhiteSpace())
        {
            return null;
        }

        return type switch
        {
            DocumentLineType.Item => (await _itemRepository.FirstOrDefaultAsync(i => i.No == no))?.VatProdPostingGroup,
            DocumentLineType.GLAccount => (await _glAccountRepository.FirstOrDefaultAsync(a => a.No == no))?.VatProdPostingGroup,
            _ => null,
        };
    }
}
