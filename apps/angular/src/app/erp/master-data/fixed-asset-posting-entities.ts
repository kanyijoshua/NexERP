import { Injectable, inject } from '@angular/core';
import { GenJournalAccountType, genJournalAccountTypeOptions } from '@proxy/finance';
import {
  CreateUpdateFAJournalBatchDto,
  CreateUpdateFAJournalLineDto,
  FAJournalBatchDto,
  FAJournalBatchService,
  FAJournalLineDto,
  FAJournalLineService,
  FAJournalPostingType,
  FixedAssetValueService,
} from '@proxy/fixed-assets';
import { mapEnumToOptions } from '@abp/ng.core';
import { EMPTY, map, of } from 'rxjs';
import { RecordEntity, RecordField } from '../erp-shared';
import { BaseTableEntities } from './base-entities';
import { codeField, enumOptions } from './entity-helpers';

const JOURNALS = 'Erp.FAJournals';
const today = () => new Date().toISOString().substring(0, 10);
const postingTypeOptions = enumOptions(mapEnumToOptions(FAJournalPostingType), 'FAJournalPostingType');

function readonly(field: string, labelKey: string): RecordField {
  return { field, labelKey, type: 'readonly' };
}

/**
 * FA journals (acquisitions, depreciation, write-downs, disposals) and the book value of an asset
 * per depreciation book, which the fixed asset card shows under its depreciation books.
 */
@Injectable({ providedIn: 'root' })
export class FixedAssetPostingEntities {
  private readonly batches = inject(FAJournalBatchService);
  private readonly lines = inject(FAJournalLineService);
  private readonly values = inject(FixedAssetValueService);
  private readonly baseTables = inject(BaseTableEntities);

  readonly faJournalBatch: RecordEntity<FAJournalBatchDto, CreateUpdateFAJournalBatchDto> = {
    key: 'faJournalBatch',
    titleKey: 'Erp::FAJournalBatch',
    pluralKey: 'Erp::FAJournals',
    icon: 'fas fa-building-circle-arrow-right',
    permission: JOURNALS,
    listRoute: ['/erp/fa-journals'],
    columns: [
      { field: 'name', labelKey: 'Erp::Name', width: 150 },
      { field: 'description', labelKey: 'Erp::Description' },
      { field: 'depreciationBookCode', labelKey: 'Erp::DepreciationBookCode', width: 160 },
    ],
    sections: [{ key: 'general', labelKey: 'Erp::General' }],
    fields: [
      { field: 'name', labelKey: 'Erp::Name', type: 'text', required: true, maxLength: 10 },
      { field: 'description', labelKey: 'Erp::Description', type: 'text', maxLength: 250 },
      codeField('depreciationBookCode', 'Erp::DepreciationBookCode', 'depreciationBook'),
    ],
    getList: query => this.batches.getList(query),
    get: id => this.batches.get(id),
    create: input => this.batches.create(input),
    update: (id, input) => this.batches.update(id, input),
    delete: id => this.batches.delete(id),
    toItem: dto => ({ id: dto.id, code: dto.name ?? '', name: dto.description ?? undefined }),
    newRecord: term => ({ name: (term ?? '').toUpperCase() }),
    actions: [
      {
        key: 'calculateDepreciation',
        labelKey: 'Erp::CalculateDepreciation',
        icon: 'fas fa-calculator',
        permission: JOURNALS,
        run: dto => this.batches.calculateDepreciation(dto.id!, { untilDate: today() }),
      },
      {
        key: 'post',
        labelKey: 'Erp::Post',
        icon: 'fas fa-paper-plane',
        permission: `${JOURNALS}.Post`,
        confirmKey: 'Erp::PostDocumentConfirmation',
        run: dto => this.batches.runPosting(dto.id!),
      },
    ],
    parts: [
      {
        entity: 'faJournalLine',
        lines: dto => this.lines.getList({ faJournalBatchId: dto.id, maxResultCount: 1000, skipCount: 0 }).pipe(map(r => r.items ?? [])),
        newLine: dto => ({ faJournalBatchId: dto.id, postingDate: today(), documentNo: `${dto.name ?? 'FA'}-${today()}`, faPostingType: FAJournalPostingType.AcquisitionCost }),
        columns: ['postingDate', 'faPostingType', 'documentNo', 'faNo', 'description', 'amount', 'balAccountType', 'balAccountNo'],
        totals: ['amount'],
      },
    ],
  };

  readonly faJournalLine: RecordEntity<FAJournalLineDto, CreateUpdateFAJournalLineDto> = {
    key: 'faJournalLine',
    titleKey: 'Erp::FAJournalLine',
    pluralKey: 'Erp::FAJournalLines',
    icon: 'fas fa-list',
    permission: JOURNALS,
    listRoute: ['/erp/fa-journals'],
    columns: [
      { field: 'postingDate', labelKey: 'Erp::PostingDate', type: 'date', width: 120 },
      { field: 'faPostingType', labelKey: 'Erp::FAPostingType', type: 'select', options: postingTypeOptions, width: 150 },
      { field: 'documentNo', labelKey: 'Erp::DocumentNo', width: 130 },
      { field: 'faNo', labelKey: 'Erp::FANo', width: 120 },
      { field: 'description', labelKey: 'Erp::Description' },
      { field: 'amount', labelKey: 'Erp::Amount', type: 'currency', width: 130 },
      { field: 'balAccountType', labelKey: 'Erp::BalAccountType', type: 'select', options: enumOptions(genJournalAccountTypeOptions, 'GenJournalAccountType'), width: 140 },
      { field: 'balAccountNo', labelKey: 'Erp::BalAccountNo', width: 120 },
    ],
    sections: [{ key: 'general', labelKey: 'Erp::General' }],
    fields: [
      { field: 'postingDate', labelKey: 'Erp::PostingDate', type: 'date', required: true },
      { field: 'faPostingType', labelKey: 'Erp::FAPostingType', type: 'select', options: postingTypeOptions },
      { field: 'documentNo', labelKey: 'Erp::DocumentNo', type: 'text', required: true, maxLength: 20 },
      codeField('faNo', 'Erp::FANo', 'fixedAsset', 'general', { required: true }),
      { field: 'description', labelKey: 'Erp::Description', type: 'text', maxLength: 250 },
      { field: 'amount', labelKey: 'Erp::Amount', type: 'currency', helpKey: 'Erp::FAJournalAmountHelp' },
      codeField('depreciationBookCode', 'Erp::DepreciationBookCode', 'depreciationBook'),
      { field: 'balAccountType', labelKey: 'Erp::BalAccountType', type: 'select', options: enumOptions(genJournalAccountTypeOptions, 'GenJournalAccountType') },
      { field: 'balAccountNo', labelKey: 'Erp::BalAccountNo', type: 'text', maxLength: 20 },
    ],
    getList: query => this.lines.getList(query),
    get: id => this.lines.get(id),
    create: input => this.lines.create(input),
    update: (id, input) => this.lines.update(id, input),
    delete: id => this.lines.delete(id),
    toItem: dto => ({ id: dto.id, code: dto.faNo ?? '', name: dto.description ?? undefined }),
    newRecord: () => ({ postingDate: today(), faPostingType: FAJournalPostingType.AcquisitionCost, amount: 0, balAccountType: GenJournalAccountType.GLAccount }),
  };

  readonly faBookValue: RecordEntity = {
    key: 'faBookValue',
    titleKey: 'Erp::BookValue',
    pluralKey: 'Erp::BookValues',
    icon: 'fas fa-scale-balanced',
    permission: 'Erp.FixedAssets',
    listRoute: ['/erp/fixed-assets'],
    readOnly: true,
    columns: [
      { field: 'depreciationBookCode', labelKey: 'Erp::DepreciationBookCode', width: 140 },
      { field: 'acquisitionDate', labelKey: 'Erp::AcquisitionDate', type: 'date', width: 120 },
      { field: 'lastDepreciationDate', labelKey: 'Erp::LastDepreciationDate', type: 'date', width: 140 },
      { field: 'disposalDate', labelKey: 'Erp::DisposalDate', type: 'date', width: 120 },
      { field: 'acquisitionCost', labelKey: 'Erp::AcquisitionCost', type: 'currency', width: 130 },
      { field: 'depreciation', labelKey: 'Erp::Depreciation', type: 'currency', width: 130 },
      { field: 'bookValue', labelKey: 'Erp::BookValue', type: 'currency', width: 130 },
    ],
    sections: [{ key: 'general', labelKey: 'Erp::General' }],
    fields: [readonly('depreciationBookCode', 'Erp::DepreciationBookCode'), readonly('bookValue', 'Erp::BookValue')],
    getList: () => of({ items: [], totalCount: 0 }),
    get: () => EMPTY,
    create: () => EMPTY,
    update: () => EMPTY,
    delete: () => EMPTY,
    toItem: dto => ({ id: dto.depreciationBookCode, code: dto.depreciationBookCode ?? '', name: String(dto.bookValue) }),
    newRecord: () => ({}),
  };

  constructor() {
    // The asset card shows what the asset is carried at in each of its books.
    const asset = this.baseTables.fixedAsset;
    asset.parts = [
      ...(asset.parts ?? []),
      { entity: 'faBookValue', titleKey: 'Erp::BookValues', lines: dto => this.values.getBookValues({ faNo: dto.no! }), editable: () => false },
    ];
  }

  get all(): RecordEntity[] {
    return [this.faJournalBatch, this.faJournalLine, this.faBookValue];
  }
}
