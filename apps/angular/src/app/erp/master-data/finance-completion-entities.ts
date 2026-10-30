import { Injectable, inject } from '@angular/core';
import {
  BankFileDto,
  CreateUpdateDirectDebitMandateDto,
  DirectDebitCollectionDto,
  DirectDebitCollectionEntryDto,
  DirectDebitCollectionEntryService,
  DirectDebitCollectionService,
  DirectDebitCollectionStatus,
  DirectDebitMandateDto,
  DirectDebitMandateService,
  DirectDebitMandateType,
  MakeDirectDebitCollectionInput,
  directDebitCollectionStatusOptions,
  directDebitMandateTypeOptions,
  directDebitSequenceTypeOptions,
} from '@proxy/cash-management';
import { CompanyDto, CompanyService } from '@proxy/companies';
import {
  AllocateCostsInput,
  CostAccountingService,
  CostAccountingSetupDto,
  CostAccountingSetupService,
  CostAllocationSourceDto,
  CostAllocationSourceService,
  CostAllocationTargetDto,
  CostAllocationTargetService,
  CostCenterDto,
  CostCenterService,
  CostEntryDto,
  CostEntryService,
  CostObjectDto,
  CostObjectService,
  CostRegisterDto,
  CostRegisterService,
  CostTypeDto,
  CostTypeKind,
  CostTypeService,
  CreateUpdateCostAllocationSourceDto,
  CreateUpdateCostAllocationTargetDto,
  CreateUpdateCostCenterDto,
  CreateUpdateCostObjectDto,
  CreateUpdateCostTypeDto,
  costRegisterSourceOptions,
  costTypeKindOptions,
} from '@proxy/cost-accounting';
import { BusinessUnitDto, BusinessUnitService, CreateUpdateBusinessUnitDto, GeneralJournalService, RunConsolidationInput } from '@proxy/finance';
import {
  AcceptICInboxInput,
  CreateUpdateICPartnerDto,
  ICInboxLineDto,
  ICInboxLineService,
  ICInboxStatus,
  ICInboxTransactionDto,
  ICInboxTransactionService,
  ICOutboxStatus,
  ICOutboxTransactionDto,
  ICOutboxTransactionService,
  ICPartnerDto,
  ICPartnerService,
  icDocumentTypeOptions,
  icInboxStatusOptions,
  icLineTypeOptions,
  icOutboxStatusOptions,
  icSourceTypeOptions,
} from '@proxy/intercompany';
import { documentLineTypeOptions } from '@proxy/documents';
import {
  CreateUpdateStandardVendorPurchaseCodeDto,
  CreateUpdateStandardPurchaseCodeDto,
  CreateUpdateStandardPurchaseLineDto,
  StandardPurchaseCodeDto,
  StandardPurchaseCodeService,
  StandardPurchaseLineDto,
  StandardPurchaseLineService,
  StandardVendorPurchaseCodeDto,
  StandardVendorPurchaseCodeService,
} from '@proxy/purchasing';
import {
  CreateUpdateStandardCustomerSalesCodeDto,
  CreateUpdateStandardSalesCodeDto,
  CreateUpdateStandardSalesLineDto,
  StandardCodeInsertMode,
  StandardCustomerSalesCodeDto,
  StandardCustomerSalesCodeService,
  StandardSalesCodeDto,
  StandardSalesCodeService,
  StandardSalesLineDto,
  StandardSalesLineService,
  standardCodeInsertModeOptions,
} from '@proxy/sales';
import { EMPTY, Observable, map, switchMap, throwError } from 'rxjs';
import { RecordEntity, RecordField } from '../erp-shared';
import { RecordDialogService } from '../erp-shared/record/record-dialog.service';
import { saveBlob } from '../erp-shared/report-page/save-blob';
import { codeField, codeTableEntity, enumOptions, runWithInput } from './entity-helpers';

const DIRECT_DEBITS = 'Erp.DirectDebits';
const CONSOLIDATION = 'Erp.Consolidation';
const INTERCOMPANY = 'Erp.Intercompany';
const COSTING = 'Erp.CostAccounting';
const STANDARD_CODES = 'Erp.StandardCodes';
const today = () => new Date().toISOString().substring(0, 10);
const yearStart = () => `${new Date().getFullYear()}-01-01`;

const lineTypes = enumOptions(documentLineTypeOptions, 'DocumentLineType');
const insertModes = enumOptions(standardCodeInsertModeOptions, 'StandardCodeInsertMode');

function readonly(field: string, labelKey: string, section?: string): RecordField {
  return { field, labelKey, type: 'readonly', section };
}

function download(file: BankFileDto | undefined): void {
  if (file?.content) {
    saveBlob(new Blob([file.content], { type: file.contentType ?? 'text/plain' }), file.fileName ?? 'bank-file.txt');
  }
}

/** A company of the group, for the business unit and IC partner cards. */
function companyEntity(companies: CompanyService): RecordEntity<CompanyDto, never> {
  const list = (filter?: string) =>
    companies.getList().pipe(
      map(result => {
        const term = (filter ?? '').toLowerCase();
        const items = (result.items ?? []).filter(c => !term || (c.name ?? '').toLowerCase().includes(term) || (c.displayName ?? '').toLowerCase().includes(term));
        return { totalCount: items.length, items };
      }),
    );
  return {
    key: 'company',
    titleKey: 'Erp::Company',
    pluralKey: 'Erp::Companies',
    icon: 'fas fa-building',
    permission: 'Erp.Companies',
    listRoute: ['/erp/companies'],
    readOnly: true,
    noCreate: true,
    columns: [
      { field: 'name', labelKey: 'Erp::Name' },
      { field: 'displayName', labelKey: 'Erp::DisplayName' },
    ],
    sections: [{ key: 'general', labelKey: 'Erp::General' }],
    fields: [readonly('name', 'Erp::Name'), readonly('displayName', 'Erp::DisplayName')],
    getList: query => list(query.filter),
    get: id => list().pipe(switchMap(page => {
      const found = page.items.find(c => c.id === id);
      return found ? [found] : throwError(() => new Error('Company not found'));
    })),
    create: () => EMPTY,
    update: () => EMPTY,
    delete: () => EMPTY,
    toItem: dto => ({ id: dto.id, code: dto.name ?? '', name: dto.displayName ?? undefined }),
    newRecord: () => ({}),
  };
}

/**
 * Direct debits, business units and consolidation, intercompany partners and transactions, the
 * cost accounting chart, allocations and entries, and standard sales and purchase codes.
 */
@Injectable({ providedIn: 'root' })
export class FinanceCompletionEntities {
  private readonly dialogs = inject(RecordDialogService);
  private readonly companies = inject(CompanyService);
  private readonly journals = inject(GeneralJournalService);
  private readonly mandates = inject(DirectDebitMandateService);
  private readonly collections = inject(DirectDebitCollectionService);
  private readonly collectionEntries = inject(DirectDebitCollectionEntryService);
  private readonly businessUnits = inject(BusinessUnitService);
  private readonly icPartners = inject(ICPartnerService);
  private readonly outbox = inject(ICOutboxTransactionService);
  private readonly inbox = inject(ICInboxTransactionService);
  private readonly inboxLines = inject(ICInboxLineService);
  private readonly costSetup = inject(CostAccountingSetupService);
  private readonly costTypes = inject(CostTypeService);
  private readonly costCenters = inject(CostCenterService);
  private readonly costObjects = inject(CostObjectService);
  private readonly costEntries = inject(CostEntryService);
  private readonly costRegisters = inject(CostRegisterService);
  private readonly allocationSources = inject(CostAllocationSourceService);
  private readonly allocationTargets = inject(CostAllocationTargetService);
  private readonly costing = inject(CostAccountingService);
  private readonly salesCodes = inject(StandardSalesCodeService);
  private readonly salesLines = inject(StandardSalesLineService);
  private readonly customerCodes = inject(StandardCustomerSalesCodeService);
  private readonly purchaseCodes = inject(StandardPurchaseCodeService);
  private readonly purchaseLines = inject(StandardPurchaseLineService);
  private readonly vendorCodes = inject(StandardVendorPurchaseCodeService);

  readonly company = companyEntity(this.companies);

  readonly directDebitMandate: RecordEntity<DirectDebitMandateDto, CreateUpdateDirectDebitMandateDto> = {
    key: 'directDebitMandate',
    titleKey: 'Erp::DirectDebitMandate',
    pluralKey: 'Erp::DirectDebitMandates',
    icon: 'fas fa-file-signature',
    permission: DIRECT_DEBITS,
    listRoute: ['/erp/direct-debit-mandates'],
    columns: [
      { field: 'code', labelKey: 'Erp::Code', width: 160 },
      { field: 'customerNo', labelKey: 'Erp::CustomerNo', width: 120 },
      { field: 'customerBankAccountCode', labelKey: 'Erp::CustomerBankAccount', width: 150 },
      { field: 'type', labelKey: 'Erp::Type', type: 'select', options: enumOptions(directDebitMandateTypeOptions, 'DirectDebitMandateType'), width: 110 },
      { field: 'validFrom', labelKey: 'Erp::ValidFrom', type: 'date', width: 120 },
      { field: 'validTo', labelKey: 'Erp::ValidTo', type: 'date', width: 120 },
      { field: 'usedCount', labelKey: 'Erp::UsedCount', type: 'number', width: 100 },
      { field: 'blocked', labelKey: 'Erp::Blocked', type: 'boolean', width: 90 },
    ],
    sections: [{ key: 'general', labelKey: 'Erp::General' }],
    fields: [
      { field: 'code', labelKey: 'Erp::MandateId', type: 'text', required: true, maxLength: 35, createOnly: true },
      codeField('customerNo', 'Erp::CustomerNo', 'customer', 'general', { required: true, createOnly: true }),
      { field: 'customerBankAccountCode', labelKey: 'Erp::CustomerBankAccount', type: 'text', required: true, maxLength: 20 },
      { field: 'type', labelKey: 'Erp::Type', type: 'select', options: enumOptions(directDebitMandateTypeOptions, 'DirectDebitMandateType') },
      { field: 'validFrom', labelKey: 'Erp::ValidFrom', type: 'date', required: true },
      { field: 'validTo', labelKey: 'Erp::ValidTo', type: 'date' },
      { field: 'expectedUses', labelKey: 'Erp::ExpectedUses', type: 'number', min: 0, helpKey: 'Erp::ExpectedUsesHelp' },
      readonly('usedCount', 'Erp::UsedCount'),
      { field: 'blocked', labelKey: 'Erp::Blocked', type: 'checkbox' },
    ],
    getList: query => this.mandates.getList(query),
    get: id => this.mandates.get(id),
    create: input => this.mandates.create(input),
    update: (id, input) => this.mandates.update(id, input),
    delete: id => this.mandates.delete(id),
    toItem: dto => ({ id: dto.id, code: dto.code ?? '', name: dto.customerNo ?? undefined }),
    newRecord: () => ({ type: DirectDebitMandateType.Recurrent, validFrom: today(), expectedUses: 0, blocked: false }),
  };

  readonly directDebitCollection: RecordEntity<DirectDebitCollectionDto, never> = {
    key: 'directDebitCollection',
    titleKey: 'Erp::DirectDebitCollection',
    pluralKey: 'Erp::DirectDebitCollections',
    icon: 'fas fa-money-bill-transfer',
    permission: DIRECT_DEBITS,
    listRoute: ['/erp/direct-debit-collections'],
    readOnly: true,
    columns: [
      { field: 'no', labelKey: 'Erp::No', width: 150 },
      { field: 'createdDate', labelKey: 'Erp::CreatedDate', type: 'date', width: 120 },
      { field: 'bankAccountNo', labelKey: 'Erp::BankAccountNo', width: 140 },
      { field: 'entryCount', labelKey: 'Erp::Entries', type: 'number', width: 90, sortable: false },
      { field: 'totalAmount', labelKey: 'Erp::Amount', type: 'currency', width: 130, sortable: false },
      { field: 'status', labelKey: 'Erp::Status', type: 'select', options: enumOptions(directDebitCollectionStatusOptions, 'DirectDebitCollectionStatus'), width: 150 },
    ],
    sections: [{ key: 'general', labelKey: 'Erp::General' }],
    fields: [readonly('no', 'Erp::No'), readonly('bankAccountNo', 'Erp::BankAccountNo'), readonly('creditorIdentifier', 'Erp::CreditorIdentifier'), readonly('totalAmount', 'Erp::Amount')],
    getList: query => this.collections.getList(query),
    get: id => this.collections.get(id),
    create: () => EMPTY,
    update: () => EMPTY,
    delete: id => this.collections.delete(id),
    toItem: dto => ({ id: dto.id, code: dto.no ?? '', name: dto.bankAccountNo ?? undefined }),
    newRecord: () => ({}),
    actions: [
      {
        key: 'export',
        labelKey: 'Erp::ExportDirectDebitFile',
        icon: 'fas fa-file-export',
        permission: `${DIRECT_DEBITS}.Update`,
        visible: dto => dto.status === DirectDebitCollectionStatus.New || dto.status === DirectDebitCollectionStatus.FileCreated,
        run: dto => this.collections.export(dto.id!).pipe(map(file => download(file))),
      },
      {
        key: 'receiptLines',
        labelKey: 'Erp::MakeReceiptLines',
        icon: 'fas fa-receipt',
        permission: `${DIRECT_DEBITS}.Update`,
        visible: dto => dto.status === DirectDebitCollectionStatus.New || dto.status === DirectDebitCollectionStatus.FileCreated,
        run: dto =>
          runWithInput<{ batchName: string; postingDate: string }>(this.dialogs, {
            titleKey: 'Erp::MakeReceiptLines',
            icon: 'fas fa-receipt',
            permission: `${DIRECT_DEBITS}.Update`,
            fields: [
              { field: 'batchName', labelKey: 'Erp::GeneralJournalBatch', type: 'text', required: true, maxLength: 10, helpKey: 'Erp::GeneralJournalBatchHelp' },
              { field: 'postingDate', labelKey: 'Erp::PostingDate', type: 'date', required: true },
            ],
            values: { postingDate: today() },
            submit: input => this.batchId(input.batchName).pipe(switchMap(id => this.collections.makeReceiptLines(dto.id!, { genJournalBatchId: id, postingDate: input.postingDate }))),
          }),
      },
      {
        key: 'close',
        labelKey: 'Erp::Close',
        icon: 'fas fa-box-archive',
        permission: `${DIRECT_DEBITS}.Update`,
        visible: dto => dto.status !== DirectDebitCollectionStatus.Closed,
        run: dto => this.collections.close(dto.id!),
      },
    ],
    listActions: [
      {
        key: 'make',
        labelKey: 'Erp::CreateDirectDebitCollection',
        icon: 'fas fa-plus',
        permission: `${DIRECT_DEBITS}.Create`,
        run: () =>
          runWithInput<MakeDirectDebitCollectionInput>(this.dialogs, {
            titleKey: 'Erp::CreateDirectDebitCollection',
            icon: 'fas fa-money-bill-transfer',
            permission: `${DIRECT_DEBITS}.Create`,
            fields: [
              { field: 'fromDueDate', labelKey: 'Erp::FromDueDate', type: 'date', required: true },
              { field: 'toDueDate', labelKey: 'Erp::ToDueDate', type: 'date', required: true },
              { field: 'bankAccountNo', labelKey: 'Erp::BankAccountNo', type: 'lookup', lookupEntity: 'bankAccount', required: true },
              { field: 'creditorIdentifier', labelKey: 'Erp::CreditorIdentifier', type: 'text', maxLength: 35 },
            ],
            values: { fromDueDate: yearStart(), toDueDate: today() },
            submit: input => this.collections.make(input),
          }),
      },
    ],
    parts: [
      {
        entity: 'directDebitCollectionEntry',
        lines: dto => this.collectionEntries.getList({ directDebitCollectionId: dto.id, maxResultCount: 1000, skipCount: 0 }).pipe(map(r => r.items ?? [])),
        columns: ['customerNo', 'customerName', 'documentNo', 'dueDate', 'amount', 'mandateCode', 'sequenceType'],
        totals: ['amount'],
        editable: () => false,
      },
    ],
  };

  readonly directDebitCollectionEntry: RecordEntity<DirectDebitCollectionEntryDto, never> = {
    key: 'directDebitCollectionEntry',
    titleKey: 'Erp::DirectDebitCollectionEntry',
    pluralKey: 'Erp::DirectDebitCollectionEntries',
    icon: 'fas fa-list',
    permission: DIRECT_DEBITS,
    listRoute: ['/erp/direct-debit-collections'],
    readOnly: true,
    columns: [
      { field: 'customerNo', labelKey: 'Erp::CustomerNo', width: 120 },
      { field: 'customerName', labelKey: 'Erp::CustomerName' },
      { field: 'documentNo', labelKey: 'Erp::DocumentNo', width: 130 },
      { field: 'dueDate', labelKey: 'Erp::DueDate', type: 'date', width: 120 },
      { field: 'amount', labelKey: 'Erp::Amount', type: 'currency', width: 120 },
      { field: 'mandateCode', labelKey: 'Erp::MandateId', width: 140 },
      { field: 'sequenceType', labelKey: 'Erp::SequenceType', type: 'select', options: enumOptions(directDebitSequenceTypeOptions, 'DirectDebitSequenceType'), width: 120 },
    ],
    sections: [{ key: 'general', labelKey: 'Erp::General' }],
    fields: [readonly('customerNo', 'Erp::CustomerNo'), readonly('documentNo', 'Erp::DocumentNo'), readonly('amount', 'Erp::Amount')],
    getList: query => this.collectionEntries.getList(query),
    get: id => this.collectionEntries.get(id),
    create: () => EMPTY,
    update: () => EMPTY,
    delete: () => EMPTY,
    toItem: dto => ({ id: dto.id, code: dto.documentNo ?? '', name: dto.customerName ?? undefined }),
    newRecord: () => ({}),
  };

  readonly businessUnit: RecordEntity<BusinessUnitDto, CreateUpdateBusinessUnitDto> = {
    ...codeTableEntity<BusinessUnitDto, CreateUpdateBusinessUnitDto>(this.businessUnits, {
    key: 'businessUnit',
    titleKey: 'Erp::BusinessUnit',
    pluralKey: 'Erp::BusinessUnits',
    icon: 'fas fa-sitemap',
    permission: CONSOLIDATION,
    route: '/erp/business-units',
    columns: [
      { field: 'consolidate', labelKey: 'Erp::Consolidate', type: 'boolean', width: 110 },
      { field: 'consolidationPct', labelKey: 'Erp::ConsolidationPct', type: 'number', width: 130 },
      { field: 'currencyCode', labelKey: 'Erp::CurrencyCode', width: 100 },
      { field: 'lastRunEndingDate', labelKey: 'Erp::LastRunEndingDate', type: 'date', width: 150 },
    ],
    fields: [
      { field: 'subsidiaryCompanyId', labelKey: 'Erp::SubsidiaryCompany', type: 'lookup', lookupEntity: 'company', lookupValueField: 'id', required: true },
      { field: 'consolidate', labelKey: 'Erp::Consolidate', type: 'checkbox' },
      { field: 'consolidationPct', labelKey: 'Erp::ConsolidationPct', type: 'number', min: 0 },
      codeField('currencyCode', 'Erp::CurrencyCode', 'currency'),
      { field: 'residualAccountNo', labelKey: 'Erp::ResidualAccountNo', type: 'lookup', lookupEntity: 'glAccount' },
      readonly('lastRunStartingDate', 'Erp::LastRunStartingDate'),
      readonly('lastRunEndingDate', 'Erp::LastRunEndingDate'),
    ],
    defaults: { consolidate: true, consolidationPct: 100 },
    }),
    listActions: [
      {
        key: 'run',
        labelKey: 'Erp::RunConsolidation',
        icon: 'fas fa-sitemap',
        permission: `${CONSOLIDATION}.Run`,
        run: () => this.runConsolidation(),
      },
    ],
  };

  readonly icPartner = codeTableEntity<ICPartnerDto, CreateUpdateICPartnerDto>(this.icPartners, {
    key: 'icPartner',
    titleKey: 'Erp::ICPartner',
    pluralKey: 'Erp::ICPartners',
    icon: 'fas fa-arrows-left-right',
    permission: INTERCOMPANY,
    route: '/erp/ic-partners',
    columns: [
      { field: 'customerNo', labelKey: 'Erp::CustomerNo', width: 120 },
      { field: 'vendorNo', labelKey: 'Erp::VendorNo', width: 120 },
      { field: 'blocked', labelKey: 'Erp::Blocked', type: 'boolean', width: 90 },
    ],
    fields: [
      { field: 'partnerCompanyId', labelKey: 'Erp::PartnerCompany', type: 'lookup', lookupEntity: 'company', lookupValueField: 'id', required: true },
      codeField('customerNo', 'Erp::CustomerNo', 'customer'),
      codeField('vendorNo', 'Erp::VendorNo', 'vendor'),
      { field: 'defaultGLAccountNo', labelKey: 'Erp::DefaultGLAccountNo', type: 'lookup', lookupEntity: 'glAccount', helpKey: 'Erp::ICDefaultAccountHelp' },
      { field: 'blocked', labelKey: 'Erp::Blocked', type: 'checkbox' },
    ],
    defaults: { blocked: false },
  });

  readonly icOutboxTransaction: RecordEntity<ICOutboxTransactionDto, never> = {
    key: 'icOutboxTransaction',
    titleKey: 'Erp::ICOutboxTransaction',
    pluralKey: 'Erp::ICOutboxTransactions',
    icon: 'fas fa-paper-plane',
    permission: INTERCOMPANY,
    listRoute: ['/erp/ic-outbox'],
    readOnly: true,
    columns: [
      { field: 'transactionNo', labelKey: 'Erp::TransactionNo', type: 'number', width: 110 },
      { field: 'partnerCode', labelKey: 'Erp::ICPartner', width: 120 },
      { field: 'sourceType', labelKey: 'Erp::SourceType', type: 'select', options: enumOptions(icSourceTypeOptions, 'ICSourceType'), width: 160 },
      { field: 'documentNo', labelKey: 'Erp::DocumentNo', width: 130 },
      { field: 'postingDate', labelKey: 'Erp::PostingDate', type: 'date', width: 120 },
      { field: 'amount', labelKey: 'Erp::Amount', type: 'currency', width: 130 },
      { field: 'status', labelKey: 'Erp::Status', type: 'select', options: enumOptions(icOutboxStatusOptions, 'ICOutboxStatus'), width: 110 },
    ],
    sections: [{ key: 'general', labelKey: 'Erp::General' }],
    fields: [readonly('documentNo', 'Erp::DocumentNo'), readonly('partnerCode', 'Erp::ICPartner'), readonly('amount', 'Erp::Amount')],
    getList: query => this.outbox.getList(query),
    get: id => this.outbox.get(id),
    create: () => EMPTY,
    update: () => EMPTY,
    delete: () => EMPTY,
    toItem: dto => ({ id: dto.id, code: String(dto.transactionNo), name: dto.documentNo ?? undefined }),
    newRecord: () => ({}),
    actions: [
      {
        key: 'send',
        labelKey: 'Erp::SendToPartner',
        icon: 'fas fa-paper-plane',
        permission: `${INTERCOMPANY}.Process`,
        visible: dto => dto.status === ICOutboxStatus.New,
        run: dto => this.outbox.send(dto.id!),
      },
      {
        key: 'cancel',
        labelKey: 'Erp::Cancel',
        icon: 'fas fa-ban',
        permission: `${INTERCOMPANY}.Process`,
        confirmKey: 'Erp::AreYouSure',
        visible: dto => dto.status === ICOutboxStatus.New,
        run: dto => this.outbox.cancel(dto.id!),
      },
    ],
    listActions: [
      {
        key: 'sendAll',
        labelKey: 'Erp::SendAllToPartners',
        icon: 'fas fa-paper-plane',
        permission: `${INTERCOMPANY}.Process`,
        run: () => this.outbox.sendAll(),
      },
    ],
  };

  readonly icInboxTransaction: RecordEntity<ICInboxTransactionDto, never> = {
    key: 'icInboxTransaction',
    titleKey: 'Erp::ICInboxTransaction',
    pluralKey: 'Erp::ICInboxTransactions',
    icon: 'fas fa-inbox',
    permission: INTERCOMPANY,
    listRoute: ['/erp/ic-inbox'],
    readOnly: true,
    columns: [
      { field: 'transactionNo', labelKey: 'Erp::TransactionNo', type: 'number', width: 110 },
      { field: 'partnerCode', labelKey: 'Erp::ICPartner', width: 120 },
      { field: 'documentType', labelKey: 'Erp::DocumentType', type: 'select', options: enumOptions(icDocumentTypeOptions, 'ICDocumentType'), width: 160 },
      { field: 'partnerDocumentNo', labelKey: 'Erp::PartnerDocumentNo', width: 140 },
      { field: 'postingDate', labelKey: 'Erp::PostingDate', type: 'date', width: 120 },
      { field: 'amount', labelKey: 'Erp::Amount', type: 'currency', width: 130 },
      { field: 'status', labelKey: 'Erp::Status', type: 'select', options: enumOptions(icInboxStatusOptions, 'ICInboxStatus'), width: 110 },
      { field: 'createdDocumentNo', labelKey: 'Erp::CreatedDocumentNo', width: 140 },
    ],
    sections: [{ key: 'general', labelKey: 'Erp::General' }],
    fields: [readonly('partnerDocumentNo', 'Erp::PartnerDocumentNo'), readonly('partnerCode', 'Erp::ICPartner'), readonly('createdDocumentNo', 'Erp::CreatedDocumentNo')],
    getList: query => this.inbox.getList(query),
    get: id => this.inbox.get(id),
    create: () => EMPTY,
    update: () => EMPTY,
    delete: () => EMPTY,
    toItem: dto => ({ id: dto.id, code: String(dto.transactionNo), name: dto.partnerDocumentNo ?? undefined }),
    newRecord: () => ({}),
    actions: [
      {
        key: 'accept',
        labelKey: 'Erp::Accept',
        icon: 'fas fa-check',
        permission: `${INTERCOMPANY}.Process`,
        visible: dto => dto.status === ICInboxStatus.Pending,
        run: dto =>
          runWithInput<AcceptICInboxInput>(this.dialogs, {
            titleKey: 'Erp::Accept',
            icon: 'fas fa-check',
            permission: `${INTERCOMPANY}.Process`,
            fields: [{ field: 'postingDate', labelKey: 'Erp::PostingDate', type: 'date', required: true }],
            values: { postingDate: dto.postingDate?.substring(0, 10) ?? today() },
            submit: input => this.inbox.accept(dto.id!, input),
          }),
      },
      {
        key: 'reject',
        labelKey: 'Erp::Reject',
        icon: 'fas fa-xmark',
        permission: `${INTERCOMPANY}.Process`,
        confirmKey: 'Erp::AreYouSure',
        visible: dto => dto.status === ICInboxStatus.Pending,
        run: dto => this.inbox.reject(dto.id!),
      },
    ],
    parts: [
      {
        entity: 'icInboxLine',
        lines: dto => this.inboxLines.getList({ icInboxTransactionId: dto.id, maxResultCount: 1000, skipCount: 0 }).pipe(map(r => r.items ?? [])),
        columns: ['type', 'no', 'description', 'quantity', 'unitAmount', 'lineDiscountPct'],
        editable: () => false,
      },
    ],
  };

  readonly icInboxLine: RecordEntity<ICInboxLineDto, never> = {
    key: 'icInboxLine',
    titleKey: 'Erp::ICInboxLine',
    pluralKey: 'Erp::ICInboxLines',
    icon: 'fas fa-list',
    permission: INTERCOMPANY,
    listRoute: ['/erp/ic-inbox'],
    readOnly: true,
    columns: [
      { field: 'type', labelKey: 'Erp::Type', type: 'select', options: enumOptions(icLineTypeOptions, 'ICLineType'), width: 110 },
      { field: 'no', labelKey: 'Erp::No', width: 120 },
      { field: 'description', labelKey: 'Erp::Description' },
      { field: 'quantity', labelKey: 'Erp::Quantity', type: 'number', width: 100 },
      { field: 'unitAmount', labelKey: 'Erp::UnitAmount', type: 'currency', width: 120 },
      { field: 'lineDiscountPct', labelKey: 'Erp::LineDiscountPct', type: 'number', width: 100 },
    ],
    sections: [{ key: 'general', labelKey: 'Erp::General' }],
    fields: [readonly('no', 'Erp::No'), readonly('description', 'Erp::Description'), readonly('quantity', 'Erp::Quantity')],
    getList: query => this.inboxLines.getList(query),
    get: id => this.inboxLines.get(id),
    create: () => EMPTY,
    update: () => EMPTY,
    delete: () => EMPTY,
    toItem: dto => ({ id: dto.id, code: dto.no ?? '', name: dto.description ?? undefined }),
    newRecord: () => ({}),
  };

  readonly costType: RecordEntity<CostTypeDto, CreateUpdateCostTypeDto> = {
    key: 'costType',
    titleKey: 'Erp::CostType',
    pluralKey: 'Erp::CostTypes',
    icon: 'fas fa-chart-pie',
    permission: COSTING,
    listRoute: ['/erp/cost-types'],
    columns: [
      { field: 'no', labelKey: 'Erp::No', width: 120 },
      { field: 'name', labelKey: 'Erp::Name' },
      { field: 'type', labelKey: 'Erp::Type', type: 'select', options: enumOptions(costTypeKindOptions, 'CostTypeKind'), width: 120 },
      { field: 'glAccountRange', labelKey: 'Erp::GLAccountRange', width: 170 },
      { field: 'costCenterCode', labelKey: 'Erp::CostCenterCode', width: 130 },
      { field: 'blocked', labelKey: 'Erp::Blocked', type: 'boolean', width: 90 },
    ],
    sections: [{ key: 'general', labelKey: 'Erp::General' }],
    fields: [
      { field: 'no', labelKey: 'Erp::No', type: 'text', required: true, maxLength: 20, createOnly: true },
      { field: 'name', labelKey: 'Erp::Name', type: 'text', maxLength: 100 },
      { field: 'type', labelKey: 'Erp::Type', type: 'select', options: enumOptions(costTypeKindOptions, 'CostTypeKind') },
      { field: 'glAccountRange', labelKey: 'Erp::GLAccountRange', type: 'text', maxLength: 250, helpKey: 'Erp::GLAccountRangeHelp' },
      { field: 'totaling', labelKey: 'Erp::Totaling', type: 'text', maxLength: 250 },
      codeField('costCenterCode', 'Erp::CostCenterCode', 'costCenter'),
      codeField('costObjectCode', 'Erp::CostObjectCode', 'costObject'),
      { field: 'blocked', labelKey: 'Erp::Blocked', type: 'checkbox' },
    ],
    getList: query => this.costTypes.getList(query),
    get: id => this.costTypes.get(id),
    create: input => this.costTypes.create(input),
    update: (id, input) => this.costTypes.update(id, input),
    delete: id => this.costTypes.delete(id),
    toItem: dto => ({ id: dto.id, code: dto.no ?? '', name: dto.name ?? undefined }),
    newRecord: term => ({ no: (term ?? '').trim().toUpperCase(), type: CostTypeKind.CostType, blocked: false }),
    listActions: [
      {
        key: 'transfer',
        labelKey: 'Erp::TransferGLEntriesToCost',
        icon: 'fas fa-right-left',
        permission: `${COSTING}.Post`,
        confirmKey: 'Erp::AreYouSure',
        run: () => this.costing.transferFromGeneralLedger(),
      },
      {
        key: 'allocate',
        labelKey: 'Erp::RunCostAllocations',
        icon: 'fas fa-share-nodes',
        permission: `${COSTING}.Post`,
        run: () =>
          runWithInput<AllocateCostsInput>(this.dialogs, {
            titleKey: 'Erp::RunCostAllocations',
            icon: 'fas fa-share-nodes',
            permission: `${COSTING}.Post`,
            fields: [
              { field: 'fromLevel', labelKey: 'Erp::FromLevel', type: 'number', min: 1, required: true },
              { field: 'toLevel', labelKey: 'Erp::ToLevel', type: 'number', min: 1, required: true },
              { field: 'postingDate', labelKey: 'Erp::PostingDate', type: 'date', required: true },
              { field: 'documentNo', labelKey: 'Erp::DocumentNo', type: 'text', required: true, maxLength: 20 },
            ],
            values: { fromLevel: 1, toLevel: 99, postingDate: today() },
            submit: input => this.costing.allocate(input),
          }),
      },
      {
        key: 'setup',
        labelKey: 'Erp::CostAccountingSetup',
        icon: 'fas fa-gears',
        permission: `${COSTING}.Update`,
        run: () =>
          this.costSetup.get().pipe(
            switchMap(setup =>
              runWithInput<CostAccountingSetupDto>(this.dialogs, {
                titleKey: 'Erp::CostAccountingSetup',
                icon: 'fas fa-gears',
                permission: `${COSTING}.Update`,
                fields: [
                  { field: 'costCenterDimension', labelKey: 'Erp::CostCenterDimension', type: 'text', maxLength: 20 },
                  { field: 'costObjectDimension', labelKey: 'Erp::CostObjectDimension', type: 'text', maxLength: 20 },
                ],
                values: setup,
                submit: input => this.costSetup.update({ ...setup, ...input }),
              }),
            ),
          ),
      },
    ],
  };

  readonly costCenter = codeTableEntity<CostCenterDto, CreateUpdateCostCenterDto>(this.costCenters, {
    key: 'costCenter',
    titleKey: 'Erp::CostCenter',
    pluralKey: 'Erp::CostCenters',
    icon: 'fas fa-people-group',
    permission: COSTING,
    route: '/erp/cost-centers',
    columns: [{ field: 'blocked', labelKey: 'Erp::Blocked', type: 'boolean', width: 90 }],
    fields: [{ field: 'blocked', labelKey: 'Erp::Blocked', type: 'checkbox' }],
    defaults: { blocked: false },
  });

  readonly costObject = codeTableEntity<CostObjectDto, CreateUpdateCostObjectDto>(this.costObjects, {
    key: 'costObject',
    titleKey: 'Erp::CostObject',
    pluralKey: 'Erp::CostObjects',
    icon: 'fas fa-cube',
    permission: COSTING,
    route: '/erp/cost-objects',
    columns: [{ field: 'blocked', labelKey: 'Erp::Blocked', type: 'boolean', width: 90 }],
    fields: [{ field: 'blocked', labelKey: 'Erp::Blocked', type: 'checkbox' }],
    defaults: { blocked: false },
  });

  readonly costEntry: RecordEntity<CostEntryDto, never> = {
    key: 'costEntry',
    titleKey: 'Erp::CostEntry',
    pluralKey: 'Erp::CostEntries',
    icon: 'fas fa-receipt',
    permission: COSTING,
    listRoute: ['/erp/cost-entries'],
    readOnly: true,
    columns: [
      { field: 'entryNo', labelKey: 'Erp::EntryNo', type: 'number', width: 90 },
      { field: 'postingDate', labelKey: 'Erp::PostingDate', type: 'date', width: 120 },
      { field: 'costTypeNo', labelKey: 'Erp::CostTypeNo', width: 120 },
      { field: 'costCenterCode', labelKey: 'Erp::CostCenterCode', width: 120 },
      { field: 'costObjectCode', labelKey: 'Erp::CostObjectCode', width: 120 },
      { field: 'documentNo', labelKey: 'Erp::DocumentNo', width: 130 },
      { field: 'description', labelKey: 'Erp::Description' },
      { field: 'amount', labelKey: 'Erp::Amount', type: 'currency', width: 120 },
      { field: 'glAccountNo', labelKey: 'Erp::GLAccountNo', width: 110 },
      { field: 'allocationCode', labelKey: 'Erp::AllocationCode', width: 130 },
    ],
    sections: [{ key: 'general', labelKey: 'Erp::General' }],
    fields: [readonly('costTypeNo', 'Erp::CostTypeNo'), readonly('documentNo', 'Erp::DocumentNo'), readonly('amount', 'Erp::Amount')],
    getList: query => this.costEntries.getList(query),
    get: id => this.costEntries.get(id),
    create: () => EMPTY,
    update: () => EMPTY,
    delete: () => EMPTY,
    toItem: dto => ({ id: dto.id, code: String(dto.entryNo), name: dto.description ?? undefined }),
    newRecord: () => ({}),
  };

  readonly costRegister: RecordEntity<CostRegisterDto, never> = {
    key: 'costRegister',
    titleKey: 'Erp::CostRegister',
    pluralKey: 'Erp::CostRegisters',
    icon: 'fas fa-book',
    permission: COSTING,
    listRoute: ['/erp/cost-registers'],
    readOnly: true,
    columns: [
      { field: 'no', labelKey: 'Erp::No', type: 'number', width: 90 },
      { field: 'source', labelKey: 'Erp::Source', type: 'select', options: enumOptions(costRegisterSourceOptions, 'CostRegisterSource'), width: 160 },
      { field: 'postingDate', labelKey: 'Erp::PostingDate', type: 'date', width: 120 },
      { field: 'fromEntryNo', labelKey: 'Erp::FromEntryNo', type: 'number', width: 110 },
      { field: 'toEntryNo', labelKey: 'Erp::ToEntryNo', type: 'number', width: 110 },
      { field: 'entryCount', labelKey: 'Erp::Entries', type: 'number', width: 90 },
      { field: 'debitAmount', labelKey: 'Erp::DebitAmount', type: 'currency', width: 130 },
    ],
    sections: [{ key: 'general', labelKey: 'Erp::General' }],
    fields: [readonly('no', 'Erp::No'), readonly('entryCount', 'Erp::Entries'), readonly('debitAmount', 'Erp::DebitAmount')],
    getList: query => this.costRegisters.getList(query),
    get: id => this.costRegisters.get(id),
    create: () => EMPTY,
    update: () => EMPTY,
    delete: () => EMPTY,
    toItem: dto => ({ id: dto.id, code: String(dto.no), name: undefined }),
    newRecord: () => ({}),
  };

  readonly costAllocationSource: RecordEntity<CostAllocationSourceDto, CreateUpdateCostAllocationSourceDto> = {
    key: 'costAllocationSource',
    titleKey: 'Erp::CostAllocation',
    pluralKey: 'Erp::CostAllocations',
    icon: 'fas fa-share-nodes',
    permission: COSTING,
    listRoute: ['/erp/cost-allocations'],
    columns: [
      { field: 'code', labelKey: 'Erp::Code', width: 130 },
      { field: 'description', labelKey: 'Erp::Description' },
      { field: 'level', labelKey: 'Erp::Level', type: 'number', width: 80 },
      { field: 'costTypeRange', labelKey: 'Erp::CostTypeRange', width: 150 },
      { field: 'costCenterCode', labelKey: 'Erp::CostCenterCode', width: 130 },
      { field: 'totalShare', labelKey: 'Erp::TotalShare', type: 'number', width: 110, sortable: false },
      { field: 'blocked', labelKey: 'Erp::Blocked', type: 'boolean', width: 90 },
    ],
    sections: [{ key: 'general', labelKey: 'Erp::General' }],
    fields: [
      { field: 'code', labelKey: 'Erp::Code', type: 'text', required: true, maxLength: 20, createOnly: true },
      { field: 'description', labelKey: 'Erp::Description', type: 'text', maxLength: 250, wide: true },
      { field: 'level', labelKey: 'Erp::Level', type: 'number', min: 1, required: true },
      { field: 'costTypeRange', labelKey: 'Erp::CostTypeRange', type: 'text', required: true, maxLength: 250 },
      codeField('costCenterCode', 'Erp::CostCenterCode', 'costCenter'),
      codeField('costObjectCode', 'Erp::CostObjectCode', 'costObject'),
      codeField('creditToCostTypeNo', 'Erp::CreditToCostType', 'costType', 'general', { required: true }),
      { field: 'blocked', labelKey: 'Erp::Blocked', type: 'checkbox' },
    ],
    getList: query => this.allocationSources.getList(query),
    get: id => this.allocationSources.get(id),
    create: input => this.allocationSources.create(input),
    update: (id, input) => this.allocationSources.update(id, input),
    delete: id => this.allocationSources.delete(id),
    toItem: dto => ({ id: dto.id, code: dto.code ?? '', name: dto.description ?? undefined }),
    newRecord: () => ({ level: 1, blocked: false }),
    parts: [
      {
        entity: 'costAllocationTarget',
        lines: dto => this.allocationTargets.getList({ costAllocationSourceId: dto.id, maxResultCount: 1000, skipCount: 0 }).pipe(map(r => r.items ?? [])),
        newLine: dto => ({ costAllocationSourceId: dto.id, share: 0 }),
        columns: ['costTypeNo', 'costCenterCode', 'costObjectCode', 'share'],
        totals: ['share'],
      },
    ],
  };

  readonly costAllocationTarget: RecordEntity<CostAllocationTargetDto, CreateUpdateCostAllocationTargetDto> = {
    key: 'costAllocationTarget',
    titleKey: 'Erp::CostAllocationTarget',
    pluralKey: 'Erp::CostAllocationTargets',
    icon: 'fas fa-bullseye',
    permission: COSTING,
    listRoute: ['/erp/cost-allocations'],
    editPermission: `${COSTING}.Update`,
    columns: [
      { field: 'costTypeNo', labelKey: 'Erp::CostTypeNo', width: 130 },
      { field: 'costCenterCode', labelKey: 'Erp::CostCenterCode', width: 130 },
      { field: 'costObjectCode', labelKey: 'Erp::CostObjectCode', width: 130 },
      { field: 'share', labelKey: 'Erp::Share', type: 'number', width: 100 },
    ],
    sections: [{ key: 'general', labelKey: 'Erp::General' }],
    fields: [
      codeField('costTypeNo', 'Erp::CostTypeNo', 'costType', 'general', { required: true }),
      codeField('costCenterCode', 'Erp::CostCenterCode', 'costCenter'),
      codeField('costObjectCode', 'Erp::CostObjectCode', 'costObject'),
      { field: 'share', labelKey: 'Erp::Share', type: 'number', min: 0, required: true },
    ],
    getList: query => this.allocationTargets.getList(query),
    get: id => this.allocationTargets.get(id),
    create: input => this.allocationTargets.create(input),
    update: (id, input) => this.allocationTargets.update(id, input),
    delete: id => this.allocationTargets.delete(id),
    toItem: dto => ({ id: dto.id, code: dto.costCenterCode ?? dto.costObjectCode ?? '', name: dto.costTypeNo ?? undefined }),
    newRecord: () => ({ share: 0 }),
  };

  readonly standardSalesCode: RecordEntity<StandardSalesCodeDto, CreateUpdateStandardSalesCodeDto> = {
    ...codeTableEntity<StandardSalesCodeDto, CreateUpdateStandardSalesCodeDto>(this.salesCodes, {
      key: 'standardSalesCode',
      titleKey: 'Erp::StandardSalesCode',
      pluralKey: 'Erp::StandardSalesCodes',
      icon: 'fas fa-repeat',
      permission: STANDARD_CODES,
      route: '/erp/standard-sales-codes',
      columns: [{ field: 'lineCount', labelKey: 'Erp::Lines', type: 'number', width: 90, sortable: false }],
      fields: [codeField('currencyCode', 'Erp::CurrencyCode', 'currency')],
    }),
    parts: [
      {
        entity: 'standardSalesLine',
        lines: dto => this.salesLines.getList({ standardSalesCodeId: dto.id, maxResultCount: 1000, skipCount: 0 }).pipe(map(r => r.items ?? [])),
        newLine: dto => ({ standardSalesCodeId: dto.id, quantity: 1, unitAmount: 0 }),
        columns: ['type', 'no', 'description', 'quantity', 'unitAmount', 'unitOfMeasureCode'],
      },
    ],
  };

  readonly standardSalesLine: RecordEntity<StandardSalesLineDto, CreateUpdateStandardSalesLineDto> = this.standardLine<StandardSalesLineDto, CreateUpdateStandardSalesLineDto>(
    'standardSalesLine',
    'Erp::StandardSalesLine',
    'Erp::StandardSalesLines',
    '/erp/standard-sales-codes',
    this.salesLines,
  );

  readonly standardCustomerSalesCode: RecordEntity<StandardCustomerSalesCodeDto, CreateUpdateStandardCustomerSalesCodeDto> = {
    key: 'standardCustomerSalesCode',
    titleKey: 'Erp::StandardCustomerSalesCode',
    pluralKey: 'Erp::StandardCustomerSalesCodes',
    icon: 'fas fa-user-tag',
    permission: STANDARD_CODES,
    listRoute: ['/erp/standard-customer-sales-codes'],
    columns: [
      { field: 'customerNo', labelKey: 'Erp::CustomerNo', width: 120 },
      { field: 'code', labelKey: 'Erp::Code', width: 130 },
      { field: 'insertOnQuotes', labelKey: 'Erp::InsertOnQuotes', type: 'select', options: insertModes, width: 130 },
      { field: 'insertOnOrders', labelKey: 'Erp::InsertOnOrders', type: 'select', options: insertModes, width: 130 },
      { field: 'insertOnInvoices', labelKey: 'Erp::InsertOnInvoices', type: 'select', options: insertModes, width: 130 },
    ],
    sections: [{ key: 'general', labelKey: 'Erp::General' }],
    fields: [
      codeField('customerNo', 'Erp::CustomerNo', 'customer', 'general', { required: true, createOnly: true }),
      codeField('code', 'Erp::StandardSalesCode', 'standardSalesCode', 'general', { required: true, createOnly: true }),
      { field: 'insertOnQuotes', labelKey: 'Erp::InsertOnQuotes', type: 'select', options: insertModes },
      { field: 'insertOnOrders', labelKey: 'Erp::InsertOnOrders', type: 'select', options: insertModes },
      { field: 'insertOnInvoices', labelKey: 'Erp::InsertOnInvoices', type: 'select', options: insertModes },
    ],
    getList: query => this.customerCodes.getList(query),
    get: id => this.customerCodes.get(id),
    create: input => this.customerCodes.create(input),
    update: (id, input) => this.customerCodes.update(id, input),
    delete: id => this.customerCodes.delete(id),
    toItem: dto => ({ id: dto.id, code: dto.code ?? '', name: dto.customerNo ?? undefined }),
    newRecord: () => ({ insertOnQuotes: StandardCodeInsertMode.Manual, insertOnOrders: StandardCodeInsertMode.Manual, insertOnInvoices: StandardCodeInsertMode.Manual }),
  };

  readonly standardPurchaseCode: RecordEntity<StandardPurchaseCodeDto, CreateUpdateStandardPurchaseCodeDto> = {
    ...codeTableEntity<StandardPurchaseCodeDto, CreateUpdateStandardPurchaseCodeDto>(this.purchaseCodes, {
      key: 'standardPurchaseCode',
      titleKey: 'Erp::StandardPurchaseCode',
      pluralKey: 'Erp::StandardPurchaseCodes',
      icon: 'fas fa-repeat',
      permission: STANDARD_CODES,
      route: '/erp/standard-purchase-codes',
      columns: [{ field: 'lineCount', labelKey: 'Erp::Lines', type: 'number', width: 90, sortable: false }],
      fields: [codeField('currencyCode', 'Erp::CurrencyCode', 'currency')],
    }),
    parts: [
      {
        entity: 'standardPurchaseLine',
        lines: dto => this.purchaseLines.getList({ standardPurchaseCodeId: dto.id, maxResultCount: 1000, skipCount: 0 }).pipe(map(r => r.items ?? [])),
        newLine: dto => ({ standardPurchaseCodeId: dto.id, quantity: 1, unitAmount: 0 }),
        columns: ['type', 'no', 'description', 'quantity', 'unitAmount', 'unitOfMeasureCode'],
      },
    ],
  };

  readonly standardPurchaseLine: RecordEntity<StandardPurchaseLineDto, CreateUpdateStandardPurchaseLineDto> = this.standardLine<StandardPurchaseLineDto, CreateUpdateStandardPurchaseLineDto>(
    'standardPurchaseLine',
    'Erp::StandardPurchaseLine',
    'Erp::StandardPurchaseLines',
    '/erp/standard-purchase-codes',
    this.purchaseLines,
  );

  readonly standardVendorPurchaseCode: RecordEntity<StandardVendorPurchaseCodeDto, CreateUpdateStandardVendorPurchaseCodeDto> = {
    ...this.standardCustomerSalesCode,
    key: 'standardVendorPurchaseCode',
    titleKey: 'Erp::StandardVendorPurchaseCode',
    pluralKey: 'Erp::StandardVendorPurchaseCodes',
    listRoute: ['/erp/standard-vendor-purchase-codes'],
    columns: [
      { field: 'vendorNo', labelKey: 'Erp::VendorNo', width: 120 },
      ...this.standardCustomerSalesCode.columns.slice(1),
    ],
    fields: [
      codeField('vendorNo', 'Erp::VendorNo', 'vendor', 'general', { required: true, createOnly: true }),
      codeField('code', 'Erp::StandardPurchaseCode', 'standardPurchaseCode', 'general', { required: true, createOnly: true }),
      ...this.standardCustomerSalesCode.fields.slice(2),
    ],
    getList: query => this.vendorCodes.getList(query),
    get: id => this.vendorCodes.get(id),
    create: input => this.vendorCodes.create(input),
    update: (id, input) => this.vendorCodes.update(id, input),
    delete: id => this.vendorCodes.delete(id),
    toItem: dto => ({ id: dto.id, code: dto.code ?? '', name: dto.vendorNo ?? undefined }),
  } as unknown as RecordEntity<StandardVendorPurchaseCodeDto, CreateUpdateStandardVendorPurchaseCodeDto>;

  get all(): RecordEntity[] {
    return [
      this.company,
      this.directDebitMandate,
      this.directDebitCollection,
      this.directDebitCollectionEntry,
      this.businessUnit,
      this.icPartner,
      this.icOutboxTransaction,
      this.icInboxTransaction,
      this.icInboxLine,
      this.costType,
      this.costCenter,
      this.costObject,
      this.costEntry,
      this.costRegister,
      this.costAllocationSource,
      this.costAllocationTarget,
      this.standardSalesCode,
      this.standardSalesLine,
      this.standardCustomerSalesCode,
      this.standardPurchaseCode,
      this.standardPurchaseLine,
      this.standardVendorPurchaseCode,
    ];
  }

  /** Consolidates the business units set to consolidate over a period. */
  runConsolidation(): Observable<unknown> {
    return runWithInput<RunConsolidationInput>(this.dialogs, {
      titleKey: 'Erp::RunConsolidation',
      icon: 'fas fa-sitemap',
      permission: `${CONSOLIDATION}.Run`,
      fields: [
        { field: 'startingDate', labelKey: 'Erp::StartingDate', type: 'date', required: true },
        { field: 'endingDate', labelKey: 'Erp::EndingDate', type: 'date', required: true },
        { field: 'documentNo', labelKey: 'Erp::DocumentNo', type: 'text', required: true, maxLength: 20 },
      ],
      values: { startingDate: yearStart(), endingDate: today() },
      submit: input => this.businessUnits.runConsolidation(input),
    });
  }

  /** The id of a general journal batch, found by its name. */
  private batchId(name: string): Observable<string> {
    const wanted = (name ?? '').trim().toUpperCase();
    return this.journals.getBatches({ journalTemplateName: 'GENERAL' } as never).pipe(
      switchMap(result => {
        const batch = (result.items ?? []).find(b => (b.name ?? '').toUpperCase() === wanted);
        return batch?.id ? [batch.id] : throwError(() => new Error(`Journal batch ${wanted} not found`));
      }),
    );
  }

  private standardLine<TDto extends { id?: string; no?: string; description?: string }, TInput>(
    key: string,
    titleKey: string,
    pluralKey: string,
    route: string,
    service: {
      getList(query: never): Observable<{ items?: TDto[]; totalCount?: number }>;
      get(id: string): Observable<TDto>;
      create(input: TInput): Observable<TDto>;
      update(id: string, input: TInput): Observable<TDto>;
      delete(id: string): Observable<void>;
    },
  ): RecordEntity<TDto, TInput> {
    return {
      key,
      titleKey,
      pluralKey,
      icon: 'fas fa-list',
      permission: STANDARD_CODES,
      listRoute: [route],
      editPermission: `${STANDARD_CODES}.Update`,
      columns: [
        { field: 'type', labelKey: 'Erp::Type', type: 'select', options: lineTypes, width: 120 },
        { field: 'no', labelKey: 'Erp::No', width: 120 },
        { field: 'description', labelKey: 'Erp::Description' },
        { field: 'quantity', labelKey: 'Erp::Quantity', type: 'number', width: 100 },
        { field: 'unitAmount', labelKey: 'Erp::UnitAmount', type: 'currency', width: 120 },
        { field: 'unitOfMeasureCode', labelKey: 'Erp::UnitOfMeasureCode', width: 110 },
      ],
      sections: [{ key: 'general', labelKey: 'Erp::General' }],
      fields: [
        { field: 'type', labelKey: 'Erp::Type', type: 'select', options: lineTypes },
        { field: 'no', labelKey: 'Erp::No', type: 'text', maxLength: 20 },
        { field: 'description', labelKey: 'Erp::Description', type: 'text', maxLength: 250 },
        { field: 'quantity', labelKey: 'Erp::Quantity', type: 'number', min: 0 },
        { field: 'unitAmount', labelKey: 'Erp::UnitAmount', type: 'currency', min: 0, helpKey: 'Erp::StandardLineAmountHelp' },
        codeField('unitOfMeasureCode', 'Erp::UnitOfMeasureCode', 'unitOfMeasure'),
      ],
      getList: query => service.getList(query as never) as never,
      get: id => service.get(id),
      create: input => service.create(input),
      update: (id, input) => service.update(id, input),
      delete: id => service.delete(id),
      toItem: dto => ({ id: dto.id, code: dto.no ?? '', name: dto.description ?? undefined }),
      newRecord: () => ({ quantity: 1, unitAmount: 0 }) as unknown as Partial<TDto>,
    };
  }
}
