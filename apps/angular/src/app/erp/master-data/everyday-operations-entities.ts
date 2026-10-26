import { mapEnumToOptions } from '@abp/ng.core';
import { Injectable, inject } from '@angular/core';
import {
  ChangeLogEntryDto,
  ChangeLogEntryService,
  ChangeLogEntryType,
  ChangeLogFieldFilterType,
  ChangeLogLoggableTableDto,
  ChangeLogSetupDto,
  ChangeLogSetupService,
  ChangeLogTableDto,
  ChangeLogTableService,
  CreateUpdateChangeLogTableDto,
  PurgeChangeLogEntriesInput,
} from '@proxy/auditing';
import {
  CreateUpdateEmailTemplateDto,
  DocumentEmailService,
  EmailDeliveryStatus,
  EmailDocumentType,
  EmailTemplateDto,
  EmailTemplateService,
  SendDocumentEmailInput,
  SentEmailLogDto,
} from '@proxy/emailing';
import {
  AccrueLeaveInput,
  ApproveLeaveApplicationInput,
  CreateUpdateLeaveApplicationDto,
  LeaveAccrualFrequency,
  LeaveApplicationDto,
  LeaveApplicationService,
  LeaveApplicationStatus,
  LeaveBalanceDto,
  LeaveEntryType,
  LeaveLedgerEntryDto,
  LeaveTypeService,
  RecallLeaveApplicationInput,
  RejectLeaveApplicationInput,
  employeeGenderOptions,
} from '@proxy/human-resources';
import {
  CreateItemChargeAssignmentDto,
  ItemChargeAllocationMethod,
  ItemChargeAssignmentDto,
  ItemChargeAssignmentService,
  ItemChargeService,
  ItemChargeSourceType,
  UpdateItemChargeAssignmentDto,
} from '@proxy/inventory';
import { EMPTY, Observable, map, of, switchMap } from 'rxjs';
import { DocumentLineOption, RecordEntity, RecordField, RecordListAction } from '../erp-shared';
import { RecordDialogService } from '../erp-shared/record/record-dialog.service';
import { codeField, codeTableEntity, enumOptions, runWithInput } from './entity-helpers';
import { SetupEntities } from './setup-entities';

const LEAVE = 'Erp.LeaveManagement';
const HR_SETUP = 'Erp.HumanResourcesSetup';
const CHARGES = 'Erp.ItemCharges';
const EMAILING = 'Erp.DocumentEmailing';
const CHANGE_LOG = 'Erp.ChangeLog';

const today = () => new Date().toISOString().substring(0, 10);

const accrualOptions = enumOptions(mapEnumToOptions(LeaveAccrualFrequency), 'LeaveAccrualFrequency');
const leaveStatusOptions = enumOptions(mapEnumToOptions(LeaveApplicationStatus), 'LeaveApplicationStatus');
const leaveEntryTypeOptions = enumOptions(mapEnumToOptions(LeaveEntryType), 'LeaveEntryType');
const chargeSourceOptions = enumOptions(mapEnumToOptions(ItemChargeSourceType), 'ItemChargeSourceType');
const emailDocumentOptions = enumOptions(mapEnumToOptions(EmailDocumentType), 'EmailDocumentType');
const deliveryStatusOptions = enumOptions(mapEnumToOptions(EmailDeliveryStatus), 'EmailDeliveryStatus');
const fieldFilterOptions = enumOptions(mapEnumToOptions(ChangeLogFieldFilterType), 'ChangeLogFieldFilterType');
const changeTypeOptions = enumOptions(mapEnumToOptions(ChangeLogEntryType), 'ChangeLogEntryType');

function readonly(field: string, labelKey: string, section = 'general'): RecordField {
  return { field, labelKey, type: 'readonly', section, cardOnly: true };
}

/** A table without a card of its own: the rows a server call returns, shown on another card. */
function noRecords<T>(): Pick<RecordEntity<T & { id?: string }>, 'get' | 'create' | 'update' | 'delete'> {
  return { get: () => EMPTY, create: () => EMPTY, update: () => EMPTY, delete: () => EMPTY };
}

/**
 * The receipts or shipments a charge line's cost is assigned to, in a list dialog opened from the
 * line: assignments can be added one by one or suggested over every entry the line can take.
 */
export function itemChargeAssignmentsFor(
  service: ItemChargeAssignmentService,
  dialogs: RecordDialogService,
  sourceType: ItemChargeSourceType,
  sourceLineId: string,
  lineTitle: string,
): Observable<unknown> {
  return service.getAssignableEntries({ sourceType, sourceLineId, maxResultCount: 200 }).pipe(
    switchMap(entries => {
      const entryOptions: DocumentLineOption[] = entries.map(e => ({
        value: e.id ?? null,
        label: `${e.documentNo ?? ''} · ${e.itemNo ?? ''} · ${e.quantity} (${e.postingDate?.substring(0, 10) ?? ''})`,
      }));
      const suggest = (key: string, method: ItemChargeAllocationMethod): RecordListAction => ({
        key,
        labelKey: `Erp::SuggestAllocation${ItemChargeAllocationMethod[method]}`,
        icon: 'fas fa-wand-magic-sparkles',
        permission: `${CHARGES}.Assign`,
        confirmKey: entries.length ? 'Erp::SuggestAllocationConfirmation' : undefined,
        run: () =>
          service.suggestAllocation({ sourceType, sourceLineId, method, targetItemLedgerEntryIds: entries.map(e => e.id!) }),
      });
      const entity: RecordEntity<ItemChargeAssignmentDto, CreateItemChargeAssignmentDto | UpdateItemChargeAssignmentDto> = {
        key: 'itemChargeAssignmentLine',
        titleKey: 'Erp::ItemChargeAssignment',
        pluralKey: 'Erp::ItemChargeAssignments',
        icon: 'fas fa-share-nodes',
        permission: CHARGES,
        editPermission: `${CHARGES}.Assign`,
        listRoute: ['/erp/item-charge-assignments'],
        rowClass: row => (row.posted ? 'text-muted' : ''),
        columns: [
          { field: 'appliesToDocumentNo', labelKey: 'Erp::DocumentNo', width: 140 },
          { field: 'appliesToEntryNo', labelKey: 'Erp::EntryNo', type: 'number', width: 100 },
          { field: 'itemNo', labelKey: 'Erp::ItemNo', width: 120 },
          { field: 'description', labelKey: 'Erp::Description', width: 200 },
          { field: 'qtyToAssign', labelKey: 'Erp::QtyToAssign', type: 'number', width: 110 },
          { field: 'amountToAssign', labelKey: 'Erp::AmountToAssign', type: 'currency', width: 130 },
          { field: 'posted', labelKey: 'Erp::Posted', type: 'boolean', width: 80 },
        ],
        sections: [{ key: 'general', labelKey: 'Erp::General' }],
        fields: [
          {
            field: 'appliesToEntryId',
            labelKey: 'Erp::AppliesToEntry',
            type: 'select',
            options: entryOptions,
            required: true,
            createOnly: true,
            wide: true,
            helpKey: 'Erp::AppliesToEntryHelp',
          },
          { field: 'qtyToAssign', labelKey: 'Erp::QtyToAssign', type: 'number', required: true, min: 0 },
          { field: 'amountToAssign', labelKey: 'Erp::AmountToAssign', type: 'currency', min: 0, helpKey: 'Erp::AmountToAssignHelp' },
        ],
        getList: query => service.getList({ ...query, sourceType, sourceLineId }),
        get: id => service.get(id),
        create: input => {
          const value = input as CreateItemChargeAssignmentDto;
          return service.create({
            sourceType,
            sourceLineId,
            appliesToEntryId: value.appliesToEntryId,
            qtyToAssign: Number(value.qtyToAssign) || 0,
            amountToAssign: value.amountToAssign ? Number(value.amountToAssign) : undefined,
          });
        },
        update: (id, input) =>
          service.update(id, {
            qtyToAssign: Number(input.qtyToAssign) || 0,
            amountToAssign: input.amountToAssign ? Number(input.amountToAssign) : undefined,
          }),
        delete: id => service.delete(id),
        toItem: dto => ({ id: dto.id, code: dto.appliesToDocumentNo ?? '', name: lineTitle }),
        newRecord: () => ({ qtyToAssign: 1 }),
        listActions: [
          suggest('suggestEqually', ItemChargeAllocationMethod.Equally),
          suggest('suggestByAmount', ItemChargeAllocationMethod.ByAmount),
          suggest('suggestByWeight', ItemChargeAllocationMethod.ByWeight),
          suggest('suggestByVolume', ItemChargeAllocationMethod.ByVolume),
        ],
      };
      return dialogs.openList(entity);
    }),
  );
}

/**
 * Emails a document: the message is built from its template first, so the dialog opens with the
 * recipient, subject and body filled in and every file attached to the document ticked. Emits the
 * logged email, which says whether the mail server took it.
 */
export function sendDocumentEmail(
  service: DocumentEmailService,
  dialogs: RecordDialogService,
  documentType: EmailDocumentType,
  documentId: string,
): Observable<SentEmailLogDto> {
  return service.preview({ documentType, documentId }).pipe(
    switchMap(preview => {
      const attachments = preview.attachments ?? [];
      const attachmentField = (index: number) => `attachment${index}`;
      return runWithInput<Record<string, unknown>, SentEmailLogDto>(dialogs, {
        titleKey: 'Erp::SendByEmail',
        icon: 'fas fa-envelope',
        permission: `${EMAILING}.Send`,
        fields: [
          { field: 'recipientEmail', labelKey: 'Erp::RecipientEmail', type: 'email', required: true, maxLength: 256 },
          { field: 'ccEmail', labelKey: 'Erp::CcEmail', type: 'email', maxLength: 256 },
          { field: 'customSubject', labelKey: 'Erp::Subject', type: 'text', required: true, maxLength: 250, wide: true },
          { field: 'customBodyHtml', labelKey: 'Erp::EmailBody', type: 'textarea', maxLength: 8000, wide: true, helpKey: 'Erp::EmailBodyHelp' },
          ...attachments.map((a, i) => ({ field: attachmentField(i), labelKey: a.fileName ?? '', type: 'checkbox' }) as RecordField),
        ],
        values: {
          recipientEmail: preview.recipientEmail ?? '',
          customSubject: preview.subject ?? '',
          customBodyHtml: preview.bodyHtml ?? '',
          ...Object.fromEntries(attachments.map((_, i) => [attachmentField(i), true])),
        },
        submit: value => {
          const input: SendDocumentEmailInput = {
            documentType,
            documentId,
            recipientEmail: String(value['recipientEmail'] ?? ''),
            ccEmail: (value['ccEmail'] as string) || undefined,
            customSubject: (value['customSubject'] as string) || undefined,
            customBodyHtml: (value['customBodyHtml'] as string) || undefined,
            templateCode: preview.templateCode,
            attachmentIds: attachments.filter((a, i) => a.id && value[attachmentField(i)]).map(a => a.id!),
          };
          return service.send(input);
        },
      });
    }),
  );
}

/**
 * Leave management (leave types, applications, the leave ledger and balances), item charges,
 * document emailing (templates and the log of sent emails) and the change log. Joined into
 * `MasterDataEntities.all`, so every lookup can find them.
 */
@Injectable({ providedIn: 'root' })
export class EverydayOperationsEntities {
  private readonly leaveTypes = inject(LeaveTypeService);
  private readonly leaveApplications = inject(LeaveApplicationService);
  private readonly itemCharges = inject(ItemChargeService);
  private readonly chargeAssignments = inject(ItemChargeAssignmentService);
  private readonly emailTemplates = inject(EmailTemplateService);
  private readonly documentEmails = inject(DocumentEmailService);
  private readonly changeLogSetup = inject(ChangeLogSetupService);
  private readonly changeLogTables = inject(ChangeLogTableService);
  private readonly changeLogEntries = inject(ChangeLogEntryService);
  private readonly dialogs = inject(RecordDialogService);
  private readonly setup = inject(SetupEntities);

  readonly leaveType = codeTableEntity(this.leaveTypes, {
    key: 'leaveType',
    titleKey: 'Erp::LeaveType',
    pluralKey: 'Erp::LeaveTypes',
    icon: 'fas fa-plane-departure',
    permission: HR_SETUP,
    route: '/erp/leave-types',
    columns: [
      { field: 'defaultEntitlementDays', labelKey: 'Erp::DefaultEntitlementDays', type: 'number', width: 150 },
      { field: 'maxCarryForwardDays', labelKey: 'Erp::MaxCarryForwardDays', type: 'number', width: 150 },
      { field: 'accrualFrequency', labelKey: 'Erp::AccrualFrequency', type: 'select', options: accrualOptions, width: 130 },
      { field: 'paid', labelKey: 'Erp::Paid', type: 'boolean', width: 80 },
      { field: 'requiresApproval', labelKey: 'Erp::RequiresApproval', type: 'boolean', width: 130 },
    ],
    fields: [
      { field: 'defaultEntitlementDays', labelKey: 'Erp::DefaultEntitlementDays', type: 'number', min: 0, required: true },
      { field: 'maxCarryForwardDays', labelKey: 'Erp::MaxCarryForwardDays', type: 'number', min: 0, helpKey: 'Erp::MaxCarryForwardDaysHelp' },
      { field: 'accrualFrequency', labelKey: 'Erp::AccrualFrequency', type: 'select', options: accrualOptions, required: true },
      codeField('causeOfAbsenceCode', 'Erp::CauseOfAbsenceCode', 'causeOfAbsence', 'general', { helpKey: 'Erp::LeaveCauseOfAbsenceHelp' }),
      { field: 'paid', labelKey: 'Erp::Paid', type: 'checkbox' },
      { field: 'requiresApproval', labelKey: 'Erp::RequiresApproval', type: 'checkbox' },
      { field: 'genderRestriction', labelKey: 'Erp::GenderRestriction', type: 'select', options: enumOptions(employeeGenderOptions, 'EmployeeGender') },
    ],
    defaults: { defaultEntitlementDays: 0, maxCarryForwardDays: 0, accrualFrequency: LeaveAccrualFrequency.Annual, paid: true, requiresApproval: true },
    quickCreate: false,
  });

  readonly leaveApplication: RecordEntity<LeaveApplicationDto, CreateUpdateLeaveApplicationDto> = {
    key: 'leaveApplication',
    titleKey: 'Erp::LeaveApplication',
    pluralKey: 'Erp::LeaveApplications',
    icon: 'fas fa-umbrella-beach',
    permission: LEAVE,
    listRoute: ['/erp/leave-applications'],
    chatterEntityType: 'LeaveApplication',
    columns: [
      { field: 'no', labelKey: 'Erp::No', width: 120 },
      { field: 'employeeNo', labelKey: 'Erp::EmployeeNo', width: 120 },
      { field: 'employeeName', labelKey: 'Erp::EmployeeName', width: 200, sortable: false, filterable: false },
      { field: 'leaveTypeCode', labelKey: 'Erp::LeaveType', width: 120 },
      { field: 'startDate', labelKey: 'Erp::StartDate', type: 'date', width: 120 },
      { field: 'endDate', labelKey: 'Erp::EndDate', type: 'date', width: 120 },
      { field: 'daysApplied', labelKey: 'Erp::DaysApplied', type: 'number', width: 110 },
      { field: 'daysApproved', labelKey: 'Erp::DaysApproved', type: 'number', width: 110 },
      { field: 'leaveStatus', labelKey: 'Erp::Status', type: 'badge', options: leaveStatusOptions, width: 140, badgeClass: row => leaveStatusClass(row.leaveStatus) },
    ],
    sections: [
      { key: 'general', labelKey: 'Erp::General' },
      { key: 'approval', labelKey: 'Erp::ApprovalAndPosting', collapsed: true },
    ],
    fields: [
      { field: 'no', labelKey: 'Erp::No', type: 'text', maxLength: 20, placeholderKey: 'Erp::NextFromSeries', createOnly: true },
      {
        field: 'employeeId',
        labelKey: 'Erp::Employee',
        type: 'lookup',
        lookupEntity: 'employee',
        lookupValueField: 'id',
        lookupDisplayField: 'employeeNo',
        required: true,
      },
      codeField('leaveTypeCode', 'Erp::LeaveType', 'leaveType', 'general', { required: true }),
      { field: 'applicationDate', labelKey: 'Erp::ApplicationDate', type: 'date' },
      { field: 'startDate', labelKey: 'Erp::StartDate', type: 'date', required: true },
      { field: 'daysApplied', labelKey: 'Erp::DaysApplied', type: 'number', min: 0, required: true, helpKey: 'Erp::DaysAppliedHelp' },
      { field: 'endDate', labelKey: 'Erp::EndDate', type: 'date', cardOnly: true },
      { field: 'returnDate', labelKey: 'Erp::ReturnDate', type: 'date', cardOnly: true },
      codeField('relieverEmployeeNo', 'Erp::RelieverEmployeeNo', 'employee', 'general', { cardOnly: true }),
      { field: 'reason', labelKey: 'Erp::Reason', type: 'textarea', maxLength: 250, wide: true },
      readonly('balanceBefore', 'Erp::BalanceBefore', 'approval'),
      readonly('daysApproved', 'Erp::DaysApproved', 'approval'),
      readonly('approverComment', 'Erp::ApproverComment', 'approval'),
      readonly('recalledDays', 'Erp::RecalledDays', 'approval'),
      readonly('recallDate', 'Erp::RecallDate', 'approval'),
      readonly('recallReason', 'Erp::RecallReason', 'approval'),
    ],
    getList: query => this.leaveApplications.getList(query),
    get: id => this.leaveApplications.get(id),
    create: input => this.leaveApplications.create(cleanLeaveInput(input)),
    update: (id, input) => this.leaveApplications.update(id, cleanLeaveInput(input)),
    delete: id => this.leaveApplications.delete(id),
    toItem: dto => ({ id: dto.id, code: dto.no ?? '', name: [dto.employeeName, dto.leaveTypeCode].filter(Boolean).join(' · ') || undefined }),
    newRecord: () => ({ applicationDate: today(), startDate: today(), daysApplied: 1 }),
    facts: dto => [
      { labelKey: 'Erp::Status', value: LeaveApplicationStatus[dto.leaveStatus] },
      { labelKey: 'Erp::DaysApplied', value: dto.daysApplied, type: 'number' },
      { labelKey: 'Erp::DaysApproved', value: dto.daysApproved, type: 'number' },
      { labelKey: 'Erp::BalanceBefore', value: dto.balanceBefore, type: 'number' },
    ],
    actions: [
      {
        key: 'sendApproval',
        labelKey: 'Erp::SendApprovalRequest',
        icon: 'fas fa-share',
        permission: `${LEAVE}.Update`,
        visible: dto => dto.leaveStatus === LeaveApplicationStatus.Open,
        run: dto => this.leaveApplications.sendApprovalRequest(dto.id!),
      },
      {
        key: 'cancelApproval',
        labelKey: 'Erp::CancelApprovalRequest',
        icon: 'fas fa-rotate-left',
        permission: `${LEAVE}.Update`,
        visible: dto => dto.leaveStatus === LeaveApplicationStatus.PendingApproval,
        run: dto => this.leaveApplications.cancelApprovalRequest(dto.id!),
      },
      {
        key: 'approve',
        labelKey: 'Erp::Approve',
        icon: 'fas fa-check',
        permission: `${LEAVE}.Approve`,
        visible: dto => dto.leaveStatus === LeaveApplicationStatus.Open || dto.leaveStatus === LeaveApplicationStatus.PendingApproval,
        run: dto =>
          runWithInput<ApproveLeaveApplicationInput>(this.dialogs, {
            titleKey: 'Erp::ApproveLeaveApplication',
            icon: 'fas fa-check',
            permission: `${LEAVE}.Approve`,
            fields: [
              { field: 'daysApproved', labelKey: 'Erp::DaysApproved', type: 'number', min: 0, required: true },
              { field: 'comment', labelKey: 'Erp::Comment', type: 'textarea', maxLength: 250, wide: true },
            ],
            values: { daysApproved: dto.daysApplied },
            submit: input => this.leaveApplications.approve(dto.id!, input),
          }),
      },
      {
        key: 'reject',
        labelKey: 'Erp::Reject',
        icon: 'fas fa-xmark',
        permission: `${LEAVE}.Approve`,
        visible: dto => dto.leaveStatus === LeaveApplicationStatus.Open || dto.leaveStatus === LeaveApplicationStatus.PendingApproval,
        run: dto =>
          runWithInput<RejectLeaveApplicationInput>(this.dialogs, {
            titleKey: 'Erp::RejectLeaveApplication',
            icon: 'fas fa-xmark',
            permission: `${LEAVE}.Approve`,
            fields: [{ field: 'comment', labelKey: 'Erp::Comment', type: 'textarea', maxLength: 250, wide: true }],
            submit: input => this.leaveApplications.reject(dto.id!, input),
          }),
      },
      {
        key: 'reopen',
        labelKey: 'Erp::Reopen',
        icon: 'fas fa-lock-open',
        permission: `${LEAVE}.Update`,
        visible: dto => dto.leaveStatus === LeaveApplicationStatus.Released || dto.leaveStatus === LeaveApplicationStatus.Rejected,
        run: dto => this.leaveApplications.reopen(dto.id!),
      },
      {
        key: 'post',
        labelKey: 'Erp::Post',
        icon: 'fas fa-paper-plane',
        permission: `${LEAVE}.Post`,
        // A leave type that needs no approval is released by posting it.
        visible: dto => dto.leaveStatus === LeaveApplicationStatus.Released || dto.leaveStatus === LeaveApplicationStatus.Open,
        confirmKey: 'Erp::PostLeaveApplicationConfirmation',
        run: dto => this.leaveApplications.runPosting(dto.id!),
      },
      {
        key: 'recall',
        labelKey: 'Erp::RecallFromLeave',
        icon: 'fas fa-person-walking-arrow-loop-left',
        permission: `${LEAVE}.Post`,
        visible: dto =>
          (dto.leaveStatus === LeaveApplicationStatus.Posted || dto.leaveStatus === LeaveApplicationStatus.Recalled) &&
          dto.daysApproved > dto.recalledDays,
        run: dto =>
          runWithInput<RecallLeaveApplicationInput>(this.dialogs, {
            titleKey: 'Erp::RecallFromLeave',
            icon: 'fas fa-person-walking-arrow-loop-left',
            permission: `${LEAVE}.Post`,
            fields: [
              { field: 'recallDays', labelKey: 'Erp::RecallDays', type: 'number', min: 0, required: true, helpKey: 'Erp::RecallDaysHelp' },
              { field: 'recallDate', labelKey: 'Erp::RecallDate', type: 'date', required: true },
              { field: 'reason', labelKey: 'Erp::Reason', type: 'textarea', maxLength: 250, wide: true },
            ],
            values: { recallDays: dto.daysApproved - dto.recalledDays, recallDate: today() },
            submit: input => this.leaveApplications.recall(dto.id!, { ...input, recallDays: Number(input.recallDays) || 0 }),
          }),
      },
    ],
    listActions: [
      {
        key: 'accrue',
        labelKey: 'Erp::AccrueLeave',
        icon: 'fas fa-calendar-plus',
        permission: `${LEAVE}.Post`,
        run: () =>
          runWithInput<AccrueLeaveInput>(this.dialogs, {
            titleKey: 'Erp::AccrueLeave',
            icon: 'fas fa-calendar-plus',
            permission: `${LEAVE}.Post`,
            fields: [
              codeField('leaveTypeCode', 'Erp::LeaveType', 'leaveType', 'general', { required: true }),
              { field: 'leavePeriodYear', labelKey: 'Erp::LeavePeriodYear', type: 'number', min: 2000, required: true },
              { field: 'postingDate', labelKey: 'Erp::PostingDate', type: 'date' },
              { field: 'employeeId', labelKey: 'Erp::Employee', type: 'lookup', lookupEntity: 'employee', lookupValueField: 'id', helpKey: 'Erp::AccrueLeaveEmployeeHelp' },
              { field: 'daysOverride', labelKey: 'Erp::DaysOverride', type: 'number', min: 0, helpKey: 'Erp::DaysOverrideHelp' },
              { field: 'carryForwardFromPreviousYear', labelKey: 'Erp::CarryForwardFromPreviousYear', type: 'checkbox' },
            ],
            values: { leavePeriodYear: new Date().getFullYear(), postingDate: today(), carryForwardFromPreviousYear: true },
            submit: input =>
              this.leaveApplications.accrue({
                ...input,
                leavePeriodYear: Number(input.leavePeriodYear),
                employeeId: input.employeeId || undefined,
                daysOverride: input.daysOverride === null || (input.daysOverride as unknown) === '' ? undefined : Number(input.daysOverride),
              }),
          }),
      },
    ],
  };

  readonly leaveLedgerEntry: RecordEntity<LeaveLedgerEntryDto, never> = {
    key: 'leaveLedgerEntry',
    titleKey: 'Erp::LeaveLedgerEntry',
    pluralKey: 'Erp::LeaveLedgerEntries',
    icon: 'fas fa-book',
    permission: LEAVE,
    listRoute: ['/erp/leave-ledger-entries'],
    readOnly: true,
    columns: [
      { field: 'entryNo', labelKey: 'Erp::EntryNo', type: 'number', width: 100 },
      { field: 'postingDate', labelKey: 'Erp::PostingDate', type: 'date', width: 120 },
      { field: 'employeeNo', labelKey: 'Erp::EmployeeNo', width: 120 },
      { field: 'employeeName', labelKey: 'Erp::EmployeeName', width: 200, sortable: false, filterable: false },
      { field: 'leaveTypeCode', labelKey: 'Erp::LeaveType', width: 120 },
      { field: 'leavePeriodYear', labelKey: 'Erp::LeavePeriodYear', type: 'number', width: 110 },
      { field: 'entryType', labelKey: 'Erp::EntryType', type: 'select', options: leaveEntryTypeOptions, width: 130 },
      { field: 'documentNo', labelKey: 'Erp::DocumentNo', width: 120 },
      { field: 'days', labelKey: 'Erp::Days', type: 'number', width: 90 },
      { field: 'description', labelKey: 'Erp::Description', width: 220 },
    ],
    sections: [{ key: 'general', labelKey: 'Erp::General' }],
    fields: [
      readonly('entryNo', 'Erp::EntryNo'),
      readonly('employeeNo', 'Erp::EmployeeNo'),
      readonly('leaveTypeCode', 'Erp::LeaveType'),
      readonly('postingDate', 'Erp::PostingDate'),
      readonly('documentNo', 'Erp::DocumentNo'),
      readonly('days', 'Erp::Days'),
      readonly('description', 'Erp::Description'),
    ],
    getList: query => this.leaveApplications.getLedgerEntries(query),
    // The ledger has no endpoint per entry; the card shows the row the list loaded.
    get: () => EMPTY,
    create: () => EMPTY,
    update: () => EMPTY,
    delete: () => EMPTY,
    toItem: dto => ({ id: dto.id, code: String(dto.entryNo), name: dto.description ?? undefined }),
    newRecord: () => ({}),
  };

  readonly leaveBalance: RecordEntity<LeaveBalanceDto & { id?: string }, never> = {
    key: 'leaveBalance',
    titleKey: 'Erp::LeaveBalance',
    pluralKey: 'Erp::LeaveBalances',
    icon: 'fas fa-scale-balanced',
    permission: LEAVE,
    listRoute: ['/erp/employees'],
    readOnly: true,
    columns: [
      { field: 'leaveTypeCode', labelKey: 'Erp::LeaveType', width: 120 },
      { field: 'leaveTypeDescription', labelKey: 'Erp::Description', width: 180 },
      { field: 'leavePeriodYear', labelKey: 'Erp::LeavePeriodYear', type: 'number', width: 100 },
      { field: 'entitledDays', labelKey: 'Erp::EntitledDays', type: 'number', width: 100 },
      { field: 'carriedForwardDays', labelKey: 'Erp::CarriedForwardDays', type: 'number', width: 120 },
      { field: 'accruedDays', labelKey: 'Erp::AccruedDays', type: 'number', width: 100 },
      { field: 'takenDays', labelKey: 'Erp::TakenDays', type: 'number', width: 100 },
      { field: 'recalledDays', labelKey: 'Erp::RecalledDays', type: 'number', width: 100 },
      { field: 'balanceDays', labelKey: 'Erp::BalanceDays', type: 'number', width: 100 },
    ],
    sections: [{ key: 'general', labelKey: 'Erp::General' }],
    fields: [readonly('leaveTypeCode', 'Erp::LeaveType'), readonly('balanceDays', 'Erp::BalanceDays')],
    getList: () => of({ items: [], totalCount: 0 }),
    ...noRecords<LeaveBalanceDto>(),
    toItem: dto => ({ id: dto.leaveTypeCode, code: dto.leaveTypeCode ?? '', name: String(dto.balanceDays) }),
    newRecord: () => ({}),
  };

  readonly itemCharge = codeTableEntity(this.itemCharges, {
    key: 'itemCharge',
    titleKey: 'Erp::ItemCharge',
    pluralKey: 'Erp::ItemCharges',
    icon: 'fas fa-truck-fast',
    permission: CHARGES,
    route: '/erp/item-charges',
    columns: [
      { field: 'genProdPostingGroup', labelKey: 'Erp::GenProdPostingGroup', width: 160 },
      { field: 'vatProdPostingGroup', labelKey: 'Erp::VatProdPostingGroup', width: 160 },
    ],
    fields: [
      codeField('genProdPostingGroup', 'Erp::GenProdPostingGroup', 'genProdPostingGroup', 'general', { required: true }),
      codeField('vatProdPostingGroup', 'Erp::VatProdPostingGroup', 'vatProdPostingGroup'),
    ],
    quickCreate: false,
  });

  /** Every assignment of every charge line, posted or not; they are made from the charge line. */
  readonly itemChargeAssignment: RecordEntity<ItemChargeAssignmentDto, UpdateItemChargeAssignmentDto> = {
    key: 'itemChargeAssignment',
    titleKey: 'Erp::ItemChargeAssignment',
    pluralKey: 'Erp::ItemChargeAssignments',
    icon: 'fas fa-share-nodes',
    permission: CHARGES,
    editPermission: `${CHARGES}.Assign`,
    listRoute: ['/erp/item-charge-assignments'],
    noCreate: true,
    columns: [
      { field: 'sourceDocumentNo', labelKey: 'Erp::SourceDocumentNo', width: 140 },
      { field: 'sourceType', labelKey: 'Erp::SourceType', type: 'select', options: chargeSourceOptions, width: 130 },
      { field: 'sourceLineNo', labelKey: 'Erp::LineNo', type: 'number', width: 80 },
      { field: 'itemChargeNo', labelKey: 'Erp::ItemCharge', width: 120 },
      { field: 'appliesToDocumentNo', labelKey: 'Erp::AppliesToDocumentNo', width: 150 },
      { field: 'appliesToEntryNo', labelKey: 'Erp::EntryNo', type: 'number', width: 100 },
      { field: 'itemNo', labelKey: 'Erp::ItemNo', width: 120 },
      { field: 'qtyToAssign', labelKey: 'Erp::QtyToAssign', type: 'number', width: 110 },
      { field: 'amountToAssign', labelKey: 'Erp::AmountToAssign', type: 'currency', width: 130 },
      { field: 'posted', labelKey: 'Erp::Posted', type: 'boolean', width: 80 },
      { field: 'amountPostedLcy', labelKey: 'Erp::AmountPostedLcy', type: 'currency', width: 140 },
    ],
    sections: [{ key: 'general', labelKey: 'Erp::General' }],
    fields: [
      readonly('sourceDocumentNo', 'Erp::SourceDocumentNo'),
      readonly('itemChargeNo', 'Erp::ItemCharge'),
      readonly('appliesToDocumentNo', 'Erp::AppliesToDocumentNo'),
      readonly('itemNo', 'Erp::ItemNo'),
      { field: 'qtyToAssign', labelKey: 'Erp::QtyToAssign', type: 'number', required: true, min: 0 },
      { field: 'amountToAssign', labelKey: 'Erp::AmountToAssign', type: 'currency', min: 0, helpKey: 'Erp::AmountToAssignHelp' },
    ],
    getList: query => this.chargeAssignments.getList(query),
    get: id => this.chargeAssignments.get(id),
    create: () => EMPTY,
    update: (id, input) =>
      this.chargeAssignments.update(id, {
        qtyToAssign: Number(input.qtyToAssign) || 0,
        amountToAssign: input.amountToAssign ? Number(input.amountToAssign) : undefined,
      }),
    delete: id => this.chargeAssignments.delete(id),
    toItem: dto => ({ id: dto.id, code: `${dto.sourceDocumentNo ?? ''} ${dto.sourceLineNo}`.trim(), name: dto.appliesToDocumentNo ?? undefined }),
    newRecord: () => ({}),
  };

  readonly emailTemplate: RecordEntity<EmailTemplateDto, CreateUpdateEmailTemplateDto> = {
    key: 'emailTemplate',
    titleKey: 'Erp::EmailTemplate',
    pluralKey: 'Erp::EmailTemplates',
    icon: 'fas fa-envelope-open-text',
    permission: EMAILING,
    editPermission: `${EMAILING}.ManageTemplates`,
    listRoute: ['/erp/email-templates'],
    columns: [
      { field: 'code', labelKey: 'Erp::Code', width: 140 },
      { field: 'name', labelKey: 'Erp::Name', width: 220 },
      { field: 'documentType', labelKey: 'Erp::DocumentType', type: 'select', options: emailDocumentOptions, width: 170 },
      { field: 'subjectTemplate', labelKey: 'Erp::Subject', width: 280 },
      { field: 'isDefault', labelKey: 'Erp::IsDefault', type: 'boolean', width: 90 },
      { field: 'active', labelKey: 'Erp::Active', type: 'boolean', width: 80 },
    ],
    sections: [
      { key: 'general', labelKey: 'Erp::General' },
      { key: 'message', labelKey: 'Erp::Message' },
    ],
    fields: [
      { field: 'code', labelKey: 'Erp::Code', type: 'text', required: true, maxLength: 20, createOnly: true },
      { field: 'name', labelKey: 'Erp::Name', type: 'text', required: true, maxLength: 100 },
      { field: 'documentType', labelKey: 'Erp::DocumentType', type: 'select', options: emailDocumentOptions, required: true },
      { field: 'isDefault', labelKey: 'Erp::IsDefault', type: 'checkbox', helpKey: 'Erp::EmailTemplateDefaultHelp' },
      { field: 'active', labelKey: 'Erp::Active', type: 'checkbox' },
      { field: 'subjectTemplate', labelKey: 'Erp::Subject', type: 'text', required: true, maxLength: 250, wide: true, section: 'message' },
      {
        field: 'bodyHtmlTemplate',
        labelKey: 'Erp::EmailBody',
        type: 'textarea',
        required: true,
        maxLength: 8000,
        wide: true,
        section: 'message',
        helpKey: 'Erp::EmailTemplatePlaceholdersHelp',
      },
    ],
    getList: query => this.emailTemplates.getList(query),
    get: id => this.emailTemplates.get(id),
    create: input => this.emailTemplates.create(input),
    update: (id, input) => this.emailTemplates.update(id, input),
    delete: id => this.emailTemplates.delete(id),
    toItem: dto => ({ id: dto.id, code: dto.code ?? '', name: dto.name ?? undefined }),
    newRecord: term => ({
      code: (term ?? '').trim().toUpperCase().substring(0, 20),
      documentType: EmailDocumentType.SalesInvoice,
      subjectTemplate: '{{CompanyName}} - {{DocumentType}} {{DocumentNo}}',
      bodyHtmlTemplate: '<p>Dear {{PartyName}},</p><p>Please find {{DocumentType}} {{DocumentNo}} for {{CurrencyCode}} {{TotalAmountIncludingVat}}, due on {{DueDate}}.</p><p>Best regards,<br/>{{CompanyName}}</p>',
      isDefault: false,
      active: true,
    }),
  };

  readonly sentEmail: RecordEntity<SentEmailLogDto, never> = {
    key: 'sentEmail',
    titleKey: 'Erp::SentEmail',
    pluralKey: 'Erp::SentEmails',
    icon: 'fas fa-paper-plane',
    permission: EMAILING,
    listRoute: ['/erp/sent-emails'],
    readOnly: true,
    rowClass: row => (row.status === EmailDeliveryStatus.Failed ? 'text-danger' : ''),
    columns: [
      { field: 'documentNo', labelKey: 'Erp::DocumentNo', width: 130 },
      { field: 'documentType', labelKey: 'Erp::DocumentType', type: 'select', options: emailDocumentOptions, width: 170 },
      { field: 'partyName', labelKey: 'Erp::Name', width: 200 },
      { field: 'recipientEmail', labelKey: 'Erp::RecipientEmail', width: 220 },
      { field: 'subject', labelKey: 'Erp::Subject', width: 260 },
      { field: 'attachmentCount', labelKey: 'Erp::Attachments', type: 'number', width: 100 },
      { field: 'status', labelKey: 'Erp::Status', type: 'select', options: deliveryStatusOptions, width: 100 },
      { field: 'sentAt', labelKey: 'Erp::SentAt', type: 'datetime', width: 160 },
      { field: 'errorMessage', labelKey: 'Erp::ErrorMessage', width: 260 },
    ],
    sections: [
      { key: 'general', labelKey: 'Erp::General' },
      { key: 'message', labelKey: 'Erp::Message' },
    ],
    fields: [
      readonly('documentNo', 'Erp::DocumentNo'),
      readonly('partyName', 'Erp::Name'),
      readonly('recipientEmail', 'Erp::RecipientEmail'),
      readonly('ccEmail', 'Erp::CcEmail'),
      readonly('templateCode', 'Erp::EmailTemplate'),
      readonly('sentAt', 'Erp::SentAt'),
      readonly('errorMessage', 'Erp::ErrorMessage'),
      readonly('subject', 'Erp::Subject', 'message'),
      readonly('bodyHtml', 'Erp::EmailBody', 'message'),
    ],
    getList: query => this.documentEmails.getList(query),
    get: id => this.documentEmails.get(id),
    create: () => EMPTY,
    update: () => EMPTY,
    delete: () => EMPTY,
    toItem: dto => ({ id: dto.id, code: dto.documentNo ?? '', name: dto.recipientEmail ?? undefined }),
    newRecord: () => ({}),
    facts: dto => [
      { labelKey: 'Erp::Status', value: EmailDeliveryStatus[dto.status] },
      { labelKey: 'Erp::Attachments', value: dto.attachmentCount, type: 'number' },
    ],
  };

  /** The tables the change log can watch, with their fields: what a change log table is looked up in. */
  readonly loggableTable: RecordEntity<ChangeLogLoggableTableDto & { id?: string }, never> = {
    key: 'loggableTable',
    titleKey: 'Erp::LoggableTable',
    pluralKey: 'Erp::LoggableTables',
    icon: 'fas fa-table',
    permission: CHANGE_LOG,
    listRoute: ['/erp/change-log-tables'],
    readOnly: true,
    columns: [
      { field: 'entityName', labelKey: 'Erp::TableName', width: 220 },
      { field: 'fieldList', labelKey: 'Erp::Fields', sortable: false, filterable: false },
    ],
    sections: [{ key: 'general', labelKey: 'Erp::General' }],
    fields: [readonly('entityName', 'Erp::TableName')],
    getList: query =>
      this.changeLogSetup.getLoggableTables().pipe(
        map(tables => {
          const term = (query.filter ?? '').trim().toLowerCase();
          const rows = tables
            .filter(t => !term || (t.entityName ?? '').toLowerCase().includes(term))
            .map(t => ({ ...t, id: t.entityName, fieldList: (t.fields ?? []).join(', ') }));
          return { items: rows.slice(query.skipCount, query.skipCount + query.maxResultCount), totalCount: rows.length };
        }),
      ),
    ...noRecords<ChangeLogLoggableTableDto>(),
    toItem: dto => ({ id: dto.entityName, code: dto.entityName ?? '', name: `${dto.fields?.length ?? 0}` }),
    newRecord: () => ({}),
  };

  readonly changeLogTable: RecordEntity<ChangeLogTableDto, CreateUpdateChangeLogTableDto> = {
    key: 'changeLogTable',
    titleKey: 'Erp::ChangeLogTable',
    pluralKey: 'Erp::ChangeLogTables',
    icon: 'fas fa-clock-rotate-left',
    permission: CHANGE_LOG,
    editPermission: `${CHANGE_LOG}.Manage`,
    listRoute: ['/erp/change-log-tables'],
    columns: [
      { field: 'entityName', labelKey: 'Erp::TableName', width: 220 },
      { field: 'logInsertion', labelKey: 'Erp::LogInsertion', type: 'select', options: fieldFilterOptions, width: 140 },
      { field: 'logModification', labelKey: 'Erp::LogModification', type: 'select', options: fieldFilterOptions, width: 140 },
      { field: 'logDeletion', labelKey: 'Erp::LogDeletion', type: 'select', options: fieldFilterOptions, width: 140 },
      { field: 'trackedFields', labelKey: 'Erp::TrackedFields', sortable: false, filterable: false, format: v => (v ?? []).join(', ') },
    ],
    sections: [{ key: 'general', labelKey: 'Erp::General' }],
    fields: [
      codeField('entityName', 'Erp::TableName', 'loggableTable', 'general', { required: true, createOnly: true }),
      { field: 'logInsertion', labelKey: 'Erp::LogInsertion', type: 'select', options: fieldFilterOptions, required: true },
      { field: 'logModification', labelKey: 'Erp::LogModification', type: 'select', options: fieldFilterOptions, required: true },
      { field: 'logDeletion', labelKey: 'Erp::LogDeletion', type: 'select', options: fieldFilterOptions, required: true },
      { field: 'trackedFields', labelKey: 'Erp::TrackedFields', type: 'text', wide: true, helpKey: 'Erp::TrackedFieldsHelp' },
    ],
    getList: query => this.changeLogTables.getList(query),
    get: id => this.changeLogTables.get(id),
    create: input => this.changeLogTables.create(input),
    update: (id, input) => this.changeLogTables.update(id, input),
    delete: id => this.changeLogTables.delete(id),
    // The tracked fields are typed as a comma-separated list.
    toInput: value =>
      ({
        ...value,
        trackedFields: String(value['trackedFields'] ?? '')
          .split(',')
          .map(f => f.trim())
          .filter(Boolean),
      }) as unknown as CreateUpdateChangeLogTableDto,
    toItem: dto => ({ id: dto.id, code: dto.entityName ?? '' }),
    newRecord: term => ({
      entityName: term ?? '',
      logInsertion: ChangeLogFieldFilterType.AllFields,
      logModification: ChangeLogFieldFilterType.AllFields,
      logDeletion: ChangeLogFieldFilterType.AllFields,
      trackedFields: [],
    }),
    listActions: [
      {
        key: 'changeLogSetup',
        labelKey: 'Erp::ChangeLogSetup',
        icon: 'fas fa-toggle-on',
        permission: `${CHANGE_LOG}.Manage`,
        run: () =>
          this.changeLogSetup.get().pipe(
            switchMap(setup =>
              runWithInput<ChangeLogSetupDto>(this.dialogs, {
                titleKey: 'Erp::ChangeLogSetup',
                icon: 'fas fa-toggle-on',
                permission: `${CHANGE_LOG}.Manage`,
                fields: [{ field: 'changeLogActivated', labelKey: 'Erp::ChangeLogActivated', type: 'checkbox', helpKey: 'Erp::ChangeLogActivatedHelp' }],
                values: setup,
                submit: input => this.changeLogSetup.update({ changeLogActivated: !!input.changeLogActivated }),
              }),
            ),
          ),
      },
    ],
  };

  readonly changeLogEntry: RecordEntity<ChangeLogEntryDto, never> = {
    key: 'changeLogEntry',
    titleKey: 'Erp::ChangeLogEntry',
    pluralKey: 'Erp::ChangeLogEntries',
    icon: 'fas fa-list-check',
    permission: CHANGE_LOG,
    listRoute: ['/erp/change-log-entries'],
    readOnly: true,
    columns: [
      { field: 'changedAt', labelKey: 'Erp::ChangedAt', type: 'datetime', width: 170 },
      { field: 'userName', labelKey: 'Erp::UserName', width: 140 },
      { field: 'entityName', labelKey: 'Erp::TableName', width: 180 },
      { field: 'primaryKey', labelKey: 'Erp::PrimaryKey', width: 160 },
      { field: 'fieldName', labelKey: 'Erp::FieldName', width: 160 },
      { field: 'typeOfChange', labelKey: 'Erp::TypeOfChange', type: 'select', options: changeTypeOptions, width: 130 },
      { field: 'oldValue', labelKey: 'Erp::OldValue', width: 200 },
      { field: 'newValue', labelKey: 'Erp::NewValue', width: 200 },
    ],
    sections: [{ key: 'general', labelKey: 'Erp::General' }],
    fields: [
      readonly('changedAt', 'Erp::ChangedAt'),
      readonly('userName', 'Erp::UserName'),
      readonly('entityName', 'Erp::TableName'),
      readonly('primaryKey', 'Erp::PrimaryKey'),
      readonly('fieldName', 'Erp::FieldName'),
      readonly('oldValue', 'Erp::OldValue'),
      readonly('newValue', 'Erp::NewValue'),
    ],
    getList: query => this.changeLogEntries.getList(query),
    get: id => this.changeLogEntries.get(id),
    create: () => EMPTY,
    update: () => EMPTY,
    delete: () => EMPTY,
    toItem: dto => ({ id: dto.id, code: `${dto.entityName ?? ''} ${dto.fieldName ?? ''}`.trim(), name: dto.newValue ?? undefined }),
    newRecord: () => ({}),
    listActions: [
      {
        key: 'purge',
        labelKey: 'Erp::DeleteOldEntries',
        icon: 'fas fa-broom',
        permission: `${CHANGE_LOG}.Manage`,
        run: () =>
          runWithInput<PurgeChangeLogEntriesInput>(this.dialogs, {
            titleKey: 'Erp::DeleteOldEntries',
            icon: 'fas fa-broom',
            permission: `${CHANGE_LOG}.Manage`,
            fields: [
              { field: 'before', labelKey: 'Erp::DeleteEntriesBefore', type: 'date', required: true },
              codeField('entityName', 'Erp::TableName', 'loggableTable', 'general', { helpKey: 'Erp::DeleteEntriesTableHelp' }),
            ],
            values: { before: oneYearAgo() },
            submit: input => this.changeLogEntries.purgeEntries({ before: input.before, entityName: input.entityName || undefined }),
          }),
      },
    ],
  };

  constructor() {
    // The employee card shows what is left of each kind of leave this year.
    const employee = this.setup.employee;
    employee.parts = [
      ...(employee.parts ?? []),
      {
        entity: 'leaveBalance',
        titleKey: 'Erp::LeaveBalances',
        lines: dto => this.leaveApplications.getBalances({ employeeId: dto.id, leavePeriodYear: new Date().getFullYear() }),
        editable: () => false,
      },
    ];
  }

  get all(): RecordEntity[] {
    return [
      this.leaveType,
      this.leaveApplication,
      this.leaveLedgerEntry,
      this.leaveBalance,
      this.itemCharge,
      this.itemChargeAssignment,
      this.emailTemplate,
      this.sentEmail,
      this.loggableTable,
      this.changeLogTable,
      this.changeLogEntry,
    ];
  }
}

/** Blank dates and numbers on the form are missing values to the server, not empty strings. */
function cleanLeaveInput(input: CreateUpdateLeaveApplicationDto): CreateUpdateLeaveApplicationDto {
  return {
    ...input,
    no: input.no || undefined,
    applicationDate: input.applicationDate || undefined,
    endDate: input.endDate || undefined,
    returnDate: input.returnDate || undefined,
    relieverEmployeeNo: input.relieverEmployeeNo || undefined,
    daysApplied: Number(input.daysApplied) || 0,
  };
}

function leaveStatusClass(status: LeaveApplicationStatus): string {
  switch (status) {
    case LeaveApplicationStatus.PendingApproval:
      return 'bg-warning text-dark';
    case LeaveApplicationStatus.Released:
      return 'bg-primary';
    case LeaveApplicationStatus.Rejected:
      return 'bg-danger';
    case LeaveApplicationStatus.Posted:
      return 'bg-success';
    case LeaveApplicationStatus.Recalled:
      return 'bg-info text-dark';
    default:
      return 'bg-secondary';
  }
}

function oneYearAgo(): string {
  const date = new Date();
  date.setFullYear(date.getFullYear() - 1);
  return date.toISOString().substring(0, 10);
}
