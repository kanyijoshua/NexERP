using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace ABPmicroservice.Erp.Inventory;

public interface IItemAppService
    : ICrudAppService<ItemDto, Guid, GetItemListInput, CreateUpdateItemDto, CreateUpdateItemDto>
{
    Task<ItemDto> GetByNoAsync(string no);

    Task BlockAsync(Guid id);

    Task UnblockAsync(Guid id);
}

public interface IItemCategoryAppService
    : ICrudAppService<
        ItemCategoryDto,
        Guid,
        PagedAndSortedResultRequestDto,
        CreateUpdateItemCategoryDto,
        CreateUpdateItemCategoryDto
    > { }

public interface IUnitOfMeasureAppService
    : ICrudAppService<
        UnitOfMeasureDto,
        Guid,
        PagedAndSortedResultRequestDto,
        CreateUpdateUnitOfMeasureDto,
        CreateUpdateUnitOfMeasureDto
    > { }

public interface IItemLedgerEntryAppService
    : IReadOnlyAppService<ItemLedgerEntryDto, Guid, GetItemLedgerEntryListInput> { }
