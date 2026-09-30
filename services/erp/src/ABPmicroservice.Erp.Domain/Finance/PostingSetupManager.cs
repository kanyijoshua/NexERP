using System;
using System.Linq;
using System.Threading.Tasks;
using ABPmicroservice.Erp.Companies;
using ABPmicroservice.Erp.Inventory;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;

namespace ABPmicroservice.Erp.Finance;

/// <summary>
/// The posting groups and setups as the rest of the ledger sees them: checks that a code a record
/// points at exists (BC's TableRelation), and turns a pair of posting groups into the G/L account
/// a posting needs, refusing a missing setup or a blank account the way BC's TestField does.
/// </summary>
public class PostingSetupManager : DomainService
{
    private readonly IRepository<GLAccount, Guid> _glAccountRepository;
    private readonly IRepository<GeneralPostingSetup, Guid> _generalPostingSetupRepository;
    private readonly IRepository<InventoryPostingSetup, Guid> _inventoryPostingSetupRepository;

    public PostingSetupManager(
        IRepository<GLAccount, Guid> glAccountRepository,
        IRepository<GeneralPostingSetup, Guid> generalPostingSetupRepository,
        IRepository<InventoryPostingSetup, Guid> inventoryPostingSetupRepository
    )
    {
        _glAccountRepository = glAccountRepository;
        _generalPostingSetupRepository = generalPostingSetupRepository;
        _inventoryPostingSetupRepository = inventoryPostingSetupRepository;
    }

    /// <summary>Refuses a code of a posting group table that does not exist; blank passes.</summary>
    public Task EnsureGroupExistsAsync<TGroup>(string code)
        where TGroup : PostingGroupBase
    {
        return LazyServiceProvider.LazyGetRequiredService<CodeTableChecker>().EnsureExistsAsync<TGroup>(code);
    }

    /// <summary>Refuses G/L account numbers that do not exist; blanks pass.</summary>
    public async Task EnsureGLAccountsExistAsync(params string[] accountNos)
    {
        var wanted = accountNos.Where(no => !no.IsNullOrWhiteSpace()).Select(no => no.Trim()).Distinct().ToList();
        if (wanted.Count == 0)
        {
            return;
        }

        var existing = await _glAccountRepository.GetListAsync(a => wanted.Contains(a.No));
        var missing = wanted.FirstOrDefault(no => existing.All(a => a.No != no));
        if (missing != null)
        {
            throw new BusinessException(ErpErrorCodes.PostingSetup.GLAccountNotFound).WithData("accountNo", missing);
        }
    }

    public async Task<GeneralPostingSetup> GetGeneralPostingSetupAsync(string genBusPostingGroup, string genProdPostingGroup)
    {
        var bus = PostingGroupBase.NormalizeCode(genBusPostingGroup);
        var prod = PostingGroupBase.NormalizeCode(genProdPostingGroup);

        var setup = await _generalPostingSetupRepository.FirstOrDefaultAsync(s => s.GenBusPostingGroup == bus && s.GenProdPostingGroup == prod);
        if (setup == null)
        {
            throw new BusinessException(ErpErrorCodes.PostingSetup.GeneralPostingSetupMissing)
                .WithData("genBusPostingGroup", bus ?? "")
                .WithData("genProdPostingGroup", prod ?? "");
        }

        return setup;
    }

    /// <summary>The revenue account of a sales line (BC "Sales Account" / "Sales Credit Memo Account").</summary>
    public async Task<string> GetSalesAccountAsync(string genBusPostingGroup, string genProdPostingGroup, bool creditMemo = false)
    {
        var setup = await GetGeneralPostingSetupAsync(genBusPostingGroup, genProdPostingGroup);
        return creditMemo
            ? Required(setup, setup.SalesCreditMemoAccountNo, "Sales Credit Memo Account")
            : Required(setup, setup.SalesAccountNo, "Sales Account");
    }

    /// <summary>The expense account of a purchase line (BC "Purch. Account" / "Purch. Credit Memo Account").</summary>
    public async Task<string> GetPurchaseAccountAsync(string genBusPostingGroup, string genProdPostingGroup, bool creditMemo = false)
    {
        var setup = await GetGeneralPostingSetupAsync(genBusPostingGroup, genProdPostingGroup);
        return creditMemo
            ? Required(setup, setup.PurchCreditMemoAccountNo, "Purch. Credit Memo Account")
            : Required(setup, setup.PurchAccountNo, "Purch. Account");
    }

    /// <summary>The VAT Posting Setup of a business and product VAT group; either may be blank.</summary>
    public async Task<VatPostingSetup> GetVatPostingSetupAsync(string vatBusPostingGroup, string vatProdPostingGroup)
    {
        var bus = CodeTableEntity.NormalizeCode(vatBusPostingGroup);
        var prod = CodeTableEntity.NormalizeCode(vatProdPostingGroup);

        var repository = LazyServiceProvider.LazyGetRequiredService<IRepository<VatPostingSetup, Guid>>();
        var setup = await repository.FirstOrDefaultAsync(s => s.VatBusPostingGroup == bus && s.VatProdPostingGroup == prod);
        if (setup == null || setup.Blocked)
        {
            throw new BusinessException(ErpErrorCodes.PostingSetup.VatPostingSetupMissing)
                .WithData("vatBusPostingGroup", bus ?? "")
                .WithData("vatProdPostingGroup", prod ?? "");
        }

        return setup;
    }

    /// <summary>
    /// The account a VAT amount posts to: sales VAT for sales, purchase VAT for purchases, and the
    /// reverse charge account for the self-assessed side of a reverse charge purchase.
    /// </summary>
    public static string GetVatAccount(VatPostingSetup setup, VatEntryType type, bool reverseChargeSide = false)
    {
        var (accountNo, field) = reverseChargeSide
            ? (setup.ReverseChrgVatAccountNo, "Reverse Chrg. VAT Acc.")
            : type == VatEntryType.Sale
                ? (setup.SalesVatAccountNo, "Sales VAT Account")
                : (setup.PurchaseVatAccountNo, "Purchase VAT Account");

        if (accountNo.IsNullOrWhiteSpace())
        {
            throw new BusinessException(ErpErrorCodes.PostingSetup.AccountMissing)
                .WithData("field", field)
                .WithData("setup", "VAT Posting Setup " + VatPostingSetup.DescribeKey(setup.VatBusPostingGroup, setup.VatProdPostingGroup));
        }

        return accountNo;
    }

    /// <summary>
    /// The balance-sheet account stock of this inventory posting group is carried on at this
    /// location, falling back to the row with a blank location.
    /// </summary>
    public async Task<string> GetInventoryAccountAsync(string inventoryPostingGroup, string locationCode = null)
    {
        var group = PostingGroupBase.NormalizeCode(inventoryPostingGroup);
        var location = CodeTableEntity.NormalizeCode(locationCode);

        InventoryPostingSetup setup = null;
        if (group != null)
        {
            setup = await _inventoryPostingSetupRepository.FirstOrDefaultAsync(s => s.InventoryPostingGroup == group && s.LocationCode == location);
            if (setup == null && location != null)
            {
                setup = await _inventoryPostingSetupRepository.FirstOrDefaultAsync(s => s.InventoryPostingGroup == group && s.LocationCode == null);
            }
        }

        if (setup == null)
        {
            throw new BusinessException(ErpErrorCodes.PostingSetup.InventoryPostingSetupMissing)
                .WithData("inventoryPostingGroup", group ?? "");
        }

        if (setup.InventoryAccountNo.IsNullOrWhiteSpace())
        {
            throw new BusinessException(ErpErrorCodes.PostingSetup.AccountMissing)
                .WithData("field", "Inventory Account")
                .WithData("setup", $"Inventory Posting Setup {location} {group}".Replace("  ", " "));
        }

        return setup.InventoryAccountNo;
    }

    /// <summary>The COGS account of an item sold to a party of this business group (BC "COGS Account").</summary>
    public async Task<string> GetCogsAccountAsync(string genBusPostingGroup, string genProdPostingGroup)
    {
        var setup = await GetGeneralPostingSetupAsync(genBusPostingGroup, genProdPostingGroup);
        return Required(setup, setup.COGSAccountNo, "COGS Account");
    }

    private static string Required(GeneralPostingSetup setup, string accountNo, string field)
    {
        if (accountNo.IsNullOrWhiteSpace())
        {
            throw new BusinessException(ErpErrorCodes.PostingSetup.AccountMissing)
                .WithData("field", field)
                .WithData("setup", "General Posting Setup " + GeneralPostingSetup.DescribeKey(setup.GenBusPostingGroup, setup.GenProdPostingGroup));
        }

        return accountNo;
    }
}
