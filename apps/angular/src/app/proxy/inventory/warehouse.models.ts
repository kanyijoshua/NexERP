import type { EntityDto, FullAuditedEntityDto, PagedAndSortedResultRequestDto } from '@abp/ng.core';
import type { CodeTableDto, CreateUpdateCodeTableDto } from '../companies/models';
import type { ItemLedgerEntryType } from './item-ledger-entry-type.enum';
import type { ItemJournalTemplateType, ItemTrackingSourceType, TransferStatus } from './warehouse.enums';

export interface GetItemChildListInput extends PagedAndSortedResultRequestDto {
  dynamicFilter?: string;
  itemNo?: string;
  filter?: string;
}

export interface ItemVariantDto extends FullAuditedEntityDto<string> {
  itemNo?: string;
  code?: string;
  description?: string;
  blocked: boolean;
}

export interface CreateUpdateItemVariantDto {
  itemNo: string;
  code: string;
  description?: string;
  blocked: boolean;
}

export interface ItemUnitOfMeasureDto extends FullAuditedEntityDto<string> {
  itemNo?: string;
  code?: string;
  qtyPerUnitOfMeasure: number;
}

export interface CreateUpdateItemUnitOfMeasureDto {
  itemNo: string;
  code: string;
  qtyPerUnitOfMeasure: number;
}

export interface ItemTrackingCodeDto extends CodeTableDto {
  lotSpecific: boolean;
  serialSpecific: boolean;
  expirationDateRequired: boolean;
}

export interface CreateUpdateItemTrackingCodeDto extends CreateUpdateCodeTableDto {
  lotSpecific: boolean;
  serialSpecific: boolean;
  expirationDateRequired: boolean;
}

export interface LotNoInformationDto extends FullAuditedEntityDto<string> {
  itemNo?: string;
  variantCode?: string;
  lotNo?: string;
  description?: string;
  expirationDate?: string;
  blocked: boolean;
}

export interface CreateUpdateLotNoInformationDto {
  itemNo: string;
  variantCode?: string;
  lotNo: string;
  description?: string;
  expirationDate?: string;
  blocked: boolean;
}

export interface SerialNoInformationDto extends FullAuditedEntityDto<string> {
  itemNo?: string;
  variantCode?: string;
  serialNo?: string;
  description?: string;
  blocked: boolean;
}

export interface CreateUpdateSerialNoInformationDto {
  itemNo: string;
  variantCode?: string;
  serialNo: string;
  description?: string;
  blocked: boolean;
}

export interface ItemTrackingLineDto extends FullAuditedEntityDto<string> {
  sourceType: ItemTrackingSourceType;
  sourceId?: string;
  itemNo?: string;
  lotNo?: string;
  serialNo?: string;
  quantity: number;
  quantityHandled: number;
  expirationDate?: string;
}

export interface CreateUpdateItemTrackingLineDto {
  sourceType: ItemTrackingSourceType;
  sourceId: string;
  itemNo: string;
  lotNo?: string;
  serialNo?: string;
  quantity: number;
  expirationDate?: string;
}

export interface GetItemTrackingLineListInput extends PagedAndSortedResultRequestDto {
  dynamicFilter?: string;
  sourceType?: ItemTrackingSourceType;
  sourceId?: string;
  itemNo?: string;
}

export interface ItemJournalTemplateDto extends FullAuditedEntityDto<string> {
  name?: string;
  description?: string;
  type: ItemJournalTemplateType;
}

export interface CreateUpdateItemJournalTemplateDto {
  name: string;
  description?: string;
  type: ItemJournalTemplateType;
}

export interface ItemJournalBatchDto extends FullAuditedEntityDto<string> {
  journalTemplateName?: string;
  name?: string;
  description?: string;
  templateType: ItemJournalTemplateType;
}

export interface CreateUpdateItemJournalBatchDto {
  journalTemplateName: string;
  name: string;
  description?: string;
}

export interface GetItemJournalBatchListInput extends PagedAndSortedResultRequestDto {
  dynamicFilter?: string;
  filter?: string;
  templateType?: ItemJournalTemplateType;
}

export interface ItemJournalLineDto extends FullAuditedEntityDto<string> {
  itemJournalBatchId?: string;
  lineNo: number;
  postingDate?: string;
  entryType: ItemLedgerEntryType;
  documentNo?: string;
  itemId?: string;
  itemNo?: string;
  description?: string;
  locationCode?: string;
  newLocationCode?: string;
  binCode?: string;
  variantCode?: string;
  unitOfMeasureCode?: string;
  quantity: number;
  unitCost: number;
  amount: number;
  lotNo?: string;
  serialNo?: string;
  expirationDate?: string;
  qtyCalculated: number;
  qtyPhysInventory: number;
  unitCostCalculated: number;
  unitCostRevalued: number;
}

export interface CreateUpdateItemJournalLineDto {
  itemJournalBatchId: string;
  postingDate: string;
  entryType: ItemLedgerEntryType;
  documentNo: string;
  itemNo: string;
  description?: string;
  locationCode?: string;
  newLocationCode?: string;
  binCode?: string;
  variantCode?: string;
  unitOfMeasureCode?: string;
  quantity: number;
  unitCost: number;
  lotNo?: string;
  serialNo?: string;
  expirationDate?: string;
  qtyPhysInventory: number;
  unitCostRevalued: number;
}

export interface GetItemJournalLineListInput extends PagedAndSortedResultRequestDto {
  dynamicFilter?: string;
  itemJournalBatchId?: string;
  filter?: string;
}

export interface CalculateInventoryInput {
  postingDate: string;
  documentNo: string;
  locationCode?: string;
}

export interface ItemJournalPostingResultDto {
  lineCount: number;
  registerNo?: number;
}

export interface TransferHeaderDto extends FullAuditedEntityDto<string> {
  no?: string;
  fromLocationCode?: string;
  toLocationCode?: string;
  inTransitCode?: string;
  postingDate?: string;
  shipmentDate?: string;
  receiptDate?: string;
  status: TransferStatus;
  completed: boolean;
  lastShipmentNo?: string;
  lastReceiptNo?: string;
}

export interface CreateUpdateTransferHeaderDto {
  no?: string;
  fromLocationCode: string;
  toLocationCode: string;
  inTransitCode: string;
  postingDate: string;
  shipmentDate?: string;
  receiptDate?: string;
}

export interface GetTransferListInput extends PagedAndSortedResultRequestDto {
  dynamicFilter?: string;
  filter?: string;
  completed?: boolean;
}

export interface TransferLineDto extends EntityDto<string> {
  transferHeaderId?: string;
  lineNo: number;
  itemNo?: string;
  variantCode?: string;
  description?: string;
  unitOfMeasureCode?: string;
  quantity: number;
  qtyToShip: number;
  quantityShipped: number;
  qtyToReceive: number;
  quantityReceived: number;
}

export interface CreateUpdateTransferLineDto {
  transferHeaderId: string;
  itemNo: string;
  variantCode?: string;
  description?: string;
  unitOfMeasureCode?: string;
  quantity: number;
  qtyToShip?: number;
  qtyToReceive?: number;
}

export interface GetTransferLineListInput extends PagedAndSortedResultRequestDto {
  dynamicFilter?: string;
  transferHeaderId?: string;
}

export interface PostedTransferDto extends EntityDto<string> {
  no?: string;
  receipt: boolean;
  transferOrderNo?: string;
  fromLocationCode?: string;
  toLocationCode?: string;
  inTransitCode?: string;
  postingDate?: string;
  lines: PostedTransferLineDto[];
}

export interface PostedTransferLineDto extends EntityDto<string> {
  postedTransferId?: string;
  lineNo: number;
  itemNo?: string;
  variantCode?: string;
  description?: string;
  unitOfMeasureCode?: string;
  quantity: number;
}

export interface GetPostedTransferListInput extends PagedAndSortedResultRequestDto {
  dynamicFilter?: string;
  filter?: string;
  receipt?: boolean;
}
