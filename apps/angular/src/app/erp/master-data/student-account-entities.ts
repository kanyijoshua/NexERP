import { Injectable, inject } from '@angular/core';
import {
  AcademicDocumentStatus,
  AcademicRequestStatus,
  CreateUpdateStudentReceiptDto,
  CreateUpdateStudentRefundDto,
  CreateUpdateStudentStatusChangeDto,
  StudentChangeType,
  StudentReceiptDto,
  StudentReceiptService,
  StudentRefundDto,
  StudentRefundService,
  StudentStatus,
  StudentStatusChangeDto,
  StudentStatusChangeService,
  academicDocumentStatusOptions,
  academicRequestStatusOptions,
  studentChangeTypeOptions,
  studentStatusOptions,
} from '@proxy/academics';
import { RecordEntity, RecordField } from '../erp-shared';
import { codeField, enumOptions } from './entity-helpers';

const PERMISSION = 'Erp.Academics';
const today = () => new Date().toISOString().substring(0, 10);

/** A read-only figure of a card: what posting or approval filled in. */
function figure(field: string, labelKey: string, section: string): RecordField {
  return { field, labelKey, type: 'readonly', section, cardOnly: true };
}

/**
 * The documents around a student's account and standing: fee receipts (which may be paid ahead
 * of a bill), refunds of what was paid ahead, and changes of status. Joined into
 * `MasterDataEntities.all`, so every lookup can find them.
 */
@Injectable({ providedIn: 'root' })
export class StudentAccountEntities {
  private readonly receipts = inject(StudentReceiptService);
  private readonly refunds = inject(StudentRefundService);
  private readonly statusChanges = inject(StudentStatusChangeService);

  readonly studentReceipt: RecordEntity<StudentReceiptDto, CreateUpdateStudentReceiptDto> = {
    key: 'studentReceipt',
    titleKey: 'Erp::StudentReceipt',
    pluralKey: 'Erp::StudentReceipts',
    icon: 'fas fa-receipt',
    permission: PERMISSION,
    listRoute: ['/erp/student-receipts'],
    attachmentEntityType: 'StudentReceipt',
    columns: [
      { field: 'no', labelKey: 'Erp::No', width: 120 },
      { field: 'studentNo', labelKey: 'Erp::StudentNo' },
      { field: 'studentName', labelKey: 'Erp::StudentName', width: 220 },
      { field: 'postingDate', labelKey: 'Erp::PostingDate', type: 'date' },
      { field: 'bankAccountNo', labelKey: 'Erp::BankAccountNo' },
      { field: 'externalDocumentNo', labelKey: 'Erp::ExternalDocumentNo' },
      { field: 'amount', labelKey: 'Erp::Amount', type: 'currency' },
      { field: 'appliesToBillNo', labelKey: 'Erp::AppliesToBillNo' },
      { field: 'status', labelKey: 'Erp::Status', type: 'select', options: enumOptions(academicDocumentStatusOptions, 'AcademicDocumentStatus') },
    ],
    sections: [{ key: 'general', labelKey: 'Erp::General' }],
    fields: [
      { field: 'no', labelKey: 'Erp::No', type: 'text', maxLength: 20, placeholderKey: 'Erp::NextFromSeries', createOnly: true },
      codeField('studentNo', 'Erp::StudentNo', 'student', undefined, { required: true }),
      { field: 'postingDate', labelKey: 'Erp::PostingDate', type: 'date', required: true },
      { field: 'amount', labelKey: 'Erp::Amount', type: 'currency', min: 0, required: true },
      codeField('bankAccountNo', 'Erp::BankAccountNo', 'bankAccount', undefined, { required: true }),
      codeField('payMode', 'Erp::PayMode', 'paymentMethod'),
      { field: 'externalDocumentNo', labelKey: 'Erp::ExternalDocumentNo', type: 'text', maxLength: 35, helpKey: 'Erp::ReceiptReferenceHelp' },
      codeField('appliesToBillNo', 'Erp::AppliesToBillNo', 'studentBill', undefined, { helpKey: 'Erp::AppliesToBillHelp' }),
      { field: 'description', labelKey: 'Erp::Description', type: 'text', maxLength: 250, wide: true, cardOnly: true },
      figure('postedBy', 'Erp::PostedBy', 'general'),
    ],
    getList: query => this.receipts.getList(query),
    get: id => this.receipts.get(id),
    create: input => this.receipts.create(input),
    update: (id, input) => this.receipts.update(id, input),
    delete: id => this.receipts.delete(id),
    toItem: dto => ({ id: dto.id, code: dto.no ?? '', name: dto.studentName ?? undefined }),
    newRecord: () => ({ postingDate: today() }),
    facts: dto => [
      { labelKey: 'Erp::Status', value: AcademicDocumentStatus[dto.status] },
      { labelKey: 'Erp::Amount', value: dto.amount, type: 'currency' },
    ],
    actions: [
      {
        key: 'post',
        labelKey: 'Erp::Post',
        icon: 'fas fa-paper-plane',
        permission: `${PERMISSION}.Post`,
        visible: dto => dto.status === AcademicDocumentStatus.Open,
        confirmKey: 'Erp::PostStudentReceiptConfirmation',
        run: dto => this.receipts.runPosting(dto.id!),
      },
    ],
  };

  readonly studentRefund: RecordEntity<StudentRefundDto, CreateUpdateStudentRefundDto> = {
    key: 'studentRefund',
    titleKey: 'Erp::StudentRefund',
    pluralKey: 'Erp::StudentRefunds',
    icon: 'fas fa-hand-holding-dollar',
    permission: PERMISSION,
    listRoute: ['/erp/student-refunds'],
    attachmentEntityType: 'StudentRefund',
    columns: [
      { field: 'no', labelKey: 'Erp::No', width: 120 },
      { field: 'studentNo', labelKey: 'Erp::StudentNo' },
      { field: 'studentName', labelKey: 'Erp::StudentName', width: 220 },
      { field: 'documentDate', labelKey: 'Erp::DocumentDate', type: 'date' },
      { field: 'amount', labelKey: 'Erp::Amount', type: 'currency' },
      { field: 'status', labelKey: 'Erp::Status', type: 'select', options: enumOptions(academicRequestStatusOptions, 'AcademicRequestStatus') },
      { field: 'paymentVoucherNo', labelKey: 'Erp::PaymentVoucherNo' },
    ],
    sections: [{ key: 'general', labelKey: 'Erp::General' }],
    fields: [
      { field: 'no', labelKey: 'Erp::No', type: 'text', maxLength: 20, placeholderKey: 'Erp::NextFromSeries', createOnly: true },
      codeField('studentNo', 'Erp::StudentNo', 'student', undefined, { required: true }),
      { field: 'documentDate', labelKey: 'Erp::DocumentDate', type: 'date', required: true },
      { field: 'amount', labelKey: 'Erp::Amount', type: 'currency', min: 0, required: true },
      { field: 'reason', labelKey: 'Erp::Reason', type: 'text', maxLength: 250, wide: true },
      figure('paymentVoucherNo', 'Erp::PaymentVoucherNo', 'general'),
      figure('approvedBy', 'Erp::ApprovedBy', 'general'),
    ],
    getList: query => this.refunds.getList(query),
    get: id => this.refunds.get(id),
    create: input => this.refunds.create(input),
    update: (id, input) => this.refunds.update(id, input),
    delete: id => this.refunds.delete(id),
    toItem: dto => ({ id: dto.id, code: dto.no ?? '', name: dto.studentName ?? undefined }),
    newRecord: () => ({ documentDate: today() }),
    facts: dto => [
      { labelKey: 'Erp::Status', value: AcademicRequestStatus[dto.status] },
      { labelKey: 'Erp::Amount', value: dto.amount, type: 'currency' },
      { labelKey: 'Erp::PaymentVoucherNo', value: dto.paymentVoucherNo },
    ],
    actions: [
      {
        key: 'approve',
        labelKey: 'Erp::Approve',
        icon: 'fas fa-check',
        permission: `${PERMISSION}.Post`,
        visible: dto => dto.status === AcademicRequestStatus.Open,
        confirmKey: 'Erp::ApproveRefundConfirmation',
        run: dto => this.refunds.approve(dto.id!),
      },
    ],
  };

  readonly studentStatusChange: RecordEntity<StudentStatusChangeDto, CreateUpdateStudentStatusChangeDto> = {
    key: 'studentStatusChange',
    titleKey: 'Erp::StudentStatusChange',
    pluralKey: 'Erp::StudentStatusChanges',
    icon: 'fas fa-user-clock',
    permission: PERMISSION,
    listRoute: ['/erp/student-status-changes'],
    attachmentEntityType: 'StudentStatusChange',
    columns: [
      { field: 'no', labelKey: 'Erp::No', width: 120 },
      { field: 'studentNo', labelKey: 'Erp::StudentNo' },
      { field: 'studentName', labelKey: 'Erp::StudentName', width: 220 },
      { field: 'changeType', labelKey: 'Erp::ChangeType', type: 'select', options: enumOptions(studentChangeTypeOptions, 'StudentChangeType') },
      { field: 'effectiveDate', labelKey: 'Erp::EffectiveDate', type: 'date' },
      { field: 'newStatus', labelKey: 'Erp::NewStatus', type: 'select', options: enumOptions(studentStatusOptions, 'StudentStatus') },
      { field: 'status', labelKey: 'Erp::Status', type: 'select', options: enumOptions(academicRequestStatusOptions, 'AcademicRequestStatus') },
    ],
    sections: [{ key: 'general', labelKey: 'Erp::General' }],
    fields: [
      { field: 'no', labelKey: 'Erp::No', type: 'text', maxLength: 20, placeholderKey: 'Erp::NextFromSeries', createOnly: true },
      codeField('studentNo', 'Erp::StudentNo', 'student', undefined, { required: true }),
      { field: 'changeType', labelKey: 'Erp::ChangeType', type: 'select', options: enumOptions(studentChangeTypeOptions, 'StudentChangeType') },
      { field: 'effectiveDate', labelKey: 'Erp::EffectiveDate', type: 'date', required: true },
      { field: 'resumeDate', labelKey: 'Erp::ResumeDate', type: 'date', helpKey: 'Erp::ResumeDateHelp' },
      { field: 'reason', labelKey: 'Erp::Reason', type: 'text', maxLength: 250, wide: true },
      figure('approvedBy', 'Erp::ApprovedBy', 'general'),
    ],
    getList: query => this.statusChanges.getList(query),
    get: id => this.statusChanges.get(id),
    create: input => this.statusChanges.create(input),
    update: (id, input) => this.statusChanges.update(id, input),
    delete: id => this.statusChanges.delete(id),
    toItem: dto => ({ id: dto.id, code: dto.no ?? '', name: dto.studentName ?? undefined }),
    newRecord: () => ({ changeType: StudentChangeType.Deferment, effectiveDate: today() }),
    facts: dto => [
      { labelKey: 'Erp::Status', value: AcademicRequestStatus[dto.status] },
      { labelKey: 'Erp::PreviousStatus', value: StudentStatus[dto.previousStatus] },
      { labelKey: 'Erp::NewStatus', value: StudentStatus[dto.newStatus] },
    ],
    actions: [
      {
        key: 'approve',
        labelKey: 'Erp::Approve',
        icon: 'fas fa-check',
        permission: `${PERMISSION}.Post`,
        visible: dto => dto.status === AcademicRequestStatus.Open,
        confirmKey: 'Erp::ApproveStatusChangeConfirmation',
        run: dto => this.statusChanges.approve(dto.id!),
      },
    ],
  };

  get all(): RecordEntity[] {
    return [this.studentReceipt, this.studentRefund, this.studentStatusChange];
  }
}
