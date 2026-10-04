import { Injectable, inject } from '@angular/core';
import {
  CreateUpdatePaymentVoucherHeaderDto,
  CreateUpdatePaymentVoucherLineDto,
  PaymentDeductionCodeService,
  PaymentTypeService,
  PaymentVoucherHeaderDto,
  PaymentVoucherLineDto,
  PaymentVoucherLineService,
  PaymentVoucherService,
  paymentDeductionTypeOptions,
} from '@proxy/cash-management';
import { DocumentStatus, documentStatusOptions } from '@proxy/documents';
import { GenJournalAccountType, genJournalAccountTypeOptions } from '@proxy/finance';
import { map } from 'rxjs';
import { RecordEntity, RecordField } from '../erp-shared';
import { accountField, codeField, codeTableEntity, enumOptions } from './entity-helpers';

const PERMISSION = 'Erp.PaymentVouchers';
const SETUP_PERMISSION = 'Erp.PaymentVoucherSetup';
const today = () => new Date().toISOString().substring(0, 10);

/** A read-only figure of a card: a total or what posting filled in. */
function figure(field: string, labelKey: string, section: string): RecordField {
  return { field, labelKey, type: 'readonly', section, cardOnly: true };
}

/** The accounts a voucher line can pay: anything but a bank account, which is what a voucher pays from. */
const lineAccountTypes = enumOptions(genJournalAccountTypeOptions, 'GenJournalAccountType').filter(o => o.value !== GenJournalAccountType.BankAccount);

/**
 * Payment vouchers: the request to pay one payee out of one bank account, with what is withheld
 * from the payment, and the payment types and deduction codes its lines are built from. Joined
 * into `MasterDataEntities.all`, so every lookup can find them.
 */
@Injectable({ providedIn: 'root' })
export class PaymentVoucherEntities {
  private readonly deductionCodes = inject(PaymentDeductionCodeService);
  private readonly paymentTypes = inject(PaymentTypeService);
  private readonly vouchers = inject(PaymentVoucherService);
  private readonly voucherLines = inject(PaymentVoucherLineService);

  readonly paymentDeductionCode = codeTableEntity(this.deductionCodes, {
    key: 'paymentDeductionCode',
    titleKey: 'Erp::PaymentDeductionCode',
    pluralKey: 'Erp::PaymentDeductionCodes',
    icon: 'fas fa-scissors',
    permission: SETUP_PERMISSION,
    route: '/erp/payment-deduction-codes',
    columns: [
      { field: 'deductionType', labelKey: 'Erp::DeductionType', type: 'select', options: enumOptions(paymentDeductionTypeOptions, 'PaymentDeductionType') },
      { field: 'ratePct', labelKey: 'Erp::RatePct', type: 'number' },
      { field: 'payableAccountNo', labelKey: 'Erp::PayableAccountNo' },
    ],
    fields: [
      { field: 'deductionType', labelKey: 'Erp::DeductionType', type: 'select', options: enumOptions(paymentDeductionTypeOptions, 'PaymentDeductionType') },
      { field: 'ratePct', labelKey: 'Erp::RatePct', type: 'number', min: 0 },
      { ...accountField('payableAccountNo', 'Erp::PayableAccountNo', 'general', true), helpKey: 'Erp::PayableAccountHelp' },
    ],
    defaults: { deductionType: 0, ratePct: 0 },
    quickCreate: false,
  });

  readonly paymentType = codeTableEntity(this.paymentTypes, {
    key: 'paymentType',
    titleKey: 'Erp::PaymentType',
    pluralKey: 'Erp::PaymentTypes',
    icon: 'fas fa-money-check',
    permission: SETUP_PERMISSION,
    route: '/erp/payment-types',
    columns: [
      { field: 'accountType', labelKey: 'Erp::AccountType', type: 'select', options: lineAccountTypes },
      { field: 'accountNo', labelKey: 'Erp::AccountNo' },
      { field: 'vatRatePct', labelKey: 'Erp::VatRatePct', type: 'number' },
      { field: 'withholdingTaxCode', labelKey: 'Erp::WithholdingTaxCode' },
      { field: 'blocked', labelKey: 'Erp::Blocked', type: 'boolean' },
    ],
    sections: [
      { key: 'general', labelKey: 'Erp::General' },
      { key: 'deductions', labelKey: 'Erp::Deductions' },
    ],
    fields: [
      { field: 'accountType', labelKey: 'Erp::AccountType', type: 'select', options: lineAccountTypes },
      { field: 'accountNo', labelKey: 'Erp::AccountNo', type: 'text', maxLength: 20 },
      { field: 'blocked', labelKey: 'Erp::Blocked', type: 'checkbox' },
      { field: 'vatRatePct', labelKey: 'Erp::VatRatePct', type: 'number', min: 0, section: 'deductions' },
      codeField('withholdingTaxCode', 'Erp::WithholdingTaxCode', 'paymentDeductionCode', 'deductions'),
      codeField('withholdingVatCode', 'Erp::WithholdingVatCode', 'paymentDeductionCode', 'deductions'),
      codeField('retentionCode', 'Erp::RetentionCode', 'paymentDeductionCode', 'deductions'),
    ],
    defaults: { accountType: GenJournalAccountType.GLAccount, vatRatePct: 0, blocked: false },
    quickCreate: false,
  });

  readonly paymentVoucher: RecordEntity<PaymentVoucherHeaderDto, CreateUpdatePaymentVoucherHeaderDto> = {
    key: 'paymentVoucher',
    titleKey: 'Erp::PaymentVoucher',
    pluralKey: 'Erp::PaymentVouchers',
    icon: 'fas fa-money-check-dollar',
    permission: PERMISSION,
    listRoute: ['/erp/payment-vouchers'],
    attachmentEntityType: 'PaymentVoucherHeader',
    columns: [
      { field: 'no', labelKey: 'Erp::No', width: 120 },
      { field: 'postingDate', labelKey: 'Erp::PostingDate', type: 'date' },
      { field: 'payee', labelKey: 'Erp::Payee', width: 220 },
      { field: 'payingBankAccountNo', labelKey: 'Erp::PayingBankAccountNo' },
      { field: 'chequeNo', labelKey: 'Erp::ChequeNo' },
      { field: 'status', labelKey: 'Erp::Status', type: 'select', options: enumOptions(documentStatusOptions, 'DocumentStatus') },
      { field: 'totalAmount', labelKey: 'Erp::TotalAmount', type: 'currency' },
      { field: 'totalNetAmount', labelKey: 'Erp::TotalNetAmount', type: 'currency' },
      { field: 'sourceNo', labelKey: 'Erp::SourceNo' },
    ],
    sections: [
      { key: 'general', labelKey: 'Erp::General' },
      { key: 'payment', labelKey: 'Erp::Payment' },
      { key: 'totals', labelKey: 'Erp::Totals' },
    ],
    fields: [
      { field: 'no', labelKey: 'Erp::No', type: 'text', maxLength: 20, placeholderKey: 'Erp::NextFromSeries', createOnly: true },
      { field: 'documentDate', labelKey: 'Erp::DocumentDate', type: 'date', required: true },
      { field: 'postingDate', labelKey: 'Erp::PostingDate', type: 'date', required: true },
      { field: 'payee', labelKey: 'Erp::Payee', type: 'text', maxLength: 100, required: true },
      { field: 'onBehalfOf', labelKey: 'Erp::OnBehalfOf', type: 'text', maxLength: 100, cardOnly: true },
      { field: 'paymentNarration', labelKey: 'Erp::PaymentNarration', type: 'text', maxLength: 100, wide: true },
      codeField('payMode', 'Erp::PayMode', 'paymentMethod', 'payment'),
      codeField('payingBankAccountNo', 'Erp::PayingBankAccountNo', 'bankAccount', 'payment'),
      { field: 'chequeNo', labelKey: 'Erp::ChequeNo', type: 'text', maxLength: 35, section: 'payment', helpKey: 'Erp::ChequeHelp' },
      { field: 'chequeDate', labelKey: 'Erp::ChequeDate', type: 'date', section: 'payment', cardOnly: true },
      figure('currencyCode', 'Erp::CurrencyCode', 'payment'),
      figure('totalAmount', 'Erp::TotalAmount', 'totals'),
      figure('totalWithholdingTaxAmount', 'Erp::TotalWithholdingTaxAmount', 'totals'),
      figure('totalWithholdingVatAmount', 'Erp::TotalWithholdingVatAmount', 'totals'),
      figure('totalRetentionAmount', 'Erp::TotalRetentionAmount', 'totals'),
      figure('totalNetAmount', 'Erp::TotalNetAmount', 'totals'),
      figure('sourceType', 'Erp::SourceType', 'totals'),
      figure('sourceNo', 'Erp::SourceNo', 'totals'),
      figure('postedBy', 'Erp::PostedBy', 'totals'),
    ],
    getList: query => this.vouchers.getList(query),
    get: id => this.vouchers.get(id),
    create: input => this.vouchers.create(input),
    update: (id, input) => this.vouchers.update(id, input),
    delete: id => this.vouchers.delete(id),
    toItem: dto => ({ id: dto.id, code: dto.no ?? '', name: dto.payee ?? undefined }),
    newRecord: () => ({ documentDate: today(), postingDate: today() }),
    facts: dto => [
      { labelKey: 'Erp::Status', value: DocumentStatus[dto.status] },
      { labelKey: 'Erp::TotalAmount', value: dto.totalAmount, type: 'currency' },
      { labelKey: 'Erp::Deductions', value: dto.totalAmount - dto.totalNetAmount, type: 'currency' },
      { labelKey: 'Erp::TotalNetAmount', value: dto.totalNetAmount, type: 'currency' },
    ],
    actions: [
      {
        key: 'sendApproval',
        labelKey: 'Erp::SendApprovalRequest',
        icon: 'fas fa-share',
        permission: `${PERMISSION}.Update`,
        visible: dto => dto.status === DocumentStatus.Open,
        run: dto => this.vouchers.sendApprovalRequest(dto.id!),
      },
      {
        key: 'cancelApproval',
        labelKey: 'Erp::CancelApprovalRequest',
        icon: 'fas fa-rotate-left',
        permission: `${PERMISSION}.Update`,
        visible: dto => dto.status === DocumentStatus.PendingApproval,
        run: dto => this.vouchers.cancelApprovalRequest(dto.id!),
      },
      {
        key: 'release',
        labelKey: 'Erp::Release',
        icon: 'fas fa-lock',
        permission: `${PERMISSION}.Update`,
        visible: dto => dto.status === DocumentStatus.Open,
        run: dto => this.vouchers.release(dto.id!),
      },
      {
        key: 'reopen',
        labelKey: 'Erp::Reopen',
        icon: 'fas fa-lock-open',
        permission: `${PERMISSION}.Update`,
        visible: dto => dto.status === DocumentStatus.Released,
        run: dto => this.vouchers.reopen(dto.id!),
      },
      {
        key: 'post',
        labelKey: 'Erp::Post',
        icon: 'fas fa-paper-plane',
        permission: `${PERMISSION}.Post`,
        visible: dto => dto.status === DocumentStatus.Released,
        confirmKey: 'Erp::PostPaymentVoucherConfirmation',
        run: dto => this.vouchers.runPosting(dto.id!),
      },
    ],
    // The lines are on the card itself; only an open voucher's lines can change.
    parts: [
      {
        entity: 'paymentVoucherLine',
        lines: dto =>
          this.voucherLines
            .getList({ documentNo: dto.no, sorting: 'lineNo', maxResultCount: 1000, skipCount: 0 })
            .pipe(map(result => result.items ?? [])),
        newLine: dto => ({ documentNo: dto.no }),
        columns: [
          'lineNo',
          'paymentTypeCode',
          'accountType',
          'accountNo',
          'accountName',
          'description',
          'amount',
          'withholdingTaxAmount',
          'withholdingVatAmount',
          'retentionAmount',
          'netAmount',
        ],
        totals: ['amount', 'withholdingTaxAmount', 'withholdingVatAmount', 'retentionAmount', 'netAmount'],
        editable: dto => dto.status === DocumentStatus.Open,
      },
    ],
  };

  readonly paymentVoucherLine: RecordEntity<PaymentVoucherLineDto, CreateUpdatePaymentVoucherLineDto> = {
    key: 'paymentVoucherLine',
    titleKey: 'Erp::PaymentVoucherLine',
    pluralKey: 'Erp::PaymentVoucherLines',
    icon: 'fas fa-list',
    permission: PERMISSION,
    listRoute: ['/erp/payment-voucher-lines'],
    columns: [
      { field: 'documentNo', labelKey: 'Erp::DocumentNo', width: 120 },
      { field: 'lineNo', labelKey: 'Erp::LineNo', type: 'number', width: 80 },
      { field: 'paymentTypeCode', labelKey: 'Erp::PaymentTypeCode' },
      { field: 'accountType', labelKey: 'Erp::AccountType', type: 'select', options: lineAccountTypes },
      { field: 'accountNo', labelKey: 'Erp::AccountNo' },
      { field: 'accountName', labelKey: 'Erp::AccountName', width: 200 },
      { field: 'description', labelKey: 'Erp::Description', width: 220 },
      { field: 'amount', labelKey: 'Erp::Amount', type: 'currency' },
      { field: 'withholdingTaxAmount', labelKey: 'Erp::WithholdingTaxAmount', type: 'currency' },
      { field: 'withholdingVatAmount', labelKey: 'Erp::WithholdingVatAmount', type: 'currency' },
      { field: 'retentionAmount', labelKey: 'Erp::RetentionAmount', type: 'currency' },
      { field: 'netAmount', labelKey: 'Erp::NetAmount', type: 'currency' },
    ],
    sections: [
      { key: 'general', labelKey: 'Erp::General' },
      { key: 'deductions', labelKey: 'Erp::Deductions' },
    ],
    fields: [
      codeField('documentNo', 'Erp::DocumentNo', 'paymentVoucher', undefined, { required: true, createOnly: true }),
      codeField('paymentTypeCode', 'Erp::PaymentTypeCode', 'paymentType', undefined, { helpKey: 'Erp::PaymentTypeHelp' }),
      { field: 'accountType', labelKey: 'Erp::AccountType', type: 'select', options: lineAccountTypes },
      { field: 'accountNo', labelKey: 'Erp::AccountNo', type: 'text', maxLength: 20 },
      { field: 'amount', labelKey: 'Erp::Amount', type: 'currency', min: 0, required: true, helpKey: 'Erp::GrossAmountHelp' },
      { field: 'appliesToDocNo', labelKey: 'Erp::AppliesToDocNo', type: 'text', maxLength: 20, helpKey: 'Erp::AppliesToDocNoHelp' },
      { field: 'description', labelKey: 'Erp::Description', type: 'text', maxLength: 250, wide: true },
      figure('accountName', 'Erp::AccountName', 'general'),
      { field: 'vatRatePct', labelKey: 'Erp::VatRatePct', type: 'number', min: 0, section: 'deductions' },
      codeField('withholdingTaxCode', 'Erp::WithholdingTaxCode', 'paymentDeductionCode', 'deductions'),
      codeField('withholdingVatCode', 'Erp::WithholdingVatCode', 'paymentDeductionCode', 'deductions'),
      codeField('retentionCode', 'Erp::RetentionCode', 'paymentDeductionCode', 'deductions'),
      figure('withholdingTaxAmount', 'Erp::WithholdingTaxAmount', 'deductions'),
      figure('withholdingVatAmount', 'Erp::WithholdingVatAmount', 'deductions'),
      figure('retentionAmount', 'Erp::RetentionAmount', 'deductions'),
      figure('netAmount', 'Erp::NetAmount', 'deductions'),
    ],
    getList: query => this.voucherLines.getList(query),
    get: id => this.voucherLines.get(id),
    create: input => this.voucherLines.create(input),
    update: (id, input) => this.voucherLines.update(id, input),
    delete: id => this.voucherLines.delete(id),
    // Blank on the form means "take it from the payment type", which the server reads as a missing value.
    toInput: value =>
      Object.fromEntries(Object.entries(value).map(([key, v]) => [key, v === '' || v === null ? undefined : v])) as unknown as CreateUpdatePaymentVoucherLineDto,
    toItem: dto => ({ id: dto.id, code: `${dto.documentNo ?? ''} ${dto.lineNo}`.trim(), name: dto.accountName ?? undefined }),
    newRecord: () => ({}),
  };

  get all(): RecordEntity[] {
    return [this.paymentDeductionCode, this.paymentType, this.paymentVoucher, this.paymentVoucherLine];
  }
}
