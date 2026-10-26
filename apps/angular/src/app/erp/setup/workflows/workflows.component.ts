import { PermissionService } from '@abp/ng.core';
import { Confirmation, ConfirmationService, ToasterService } from '@abp/ng.theme.shared';
import { Component, DestroyRef, OnInit, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import {
  ApprovalDocumentKind,
  ApprovalUserSetupDto,
  ApprovalUserSetupService,
  ApproverLimitType,
  CreateUpdateWorkflowDto,
  CreateUpdateWorkflowStepDto,
  WorkflowApproverType,
  WorkflowDto,
  WorkflowService,
  WorkflowTemplateDto,
  WorkflowUserGroupService,
  approvalDocumentKindOptions,
  approverLimitTypeOptions,
  workflowApproverTypeOptions,
} from '@proxy/workflows';
import { CodeTableDto } from '@proxy/companies';
import { finalize } from 'rxjs';
import { CompanyService } from '../../services/company.service';

export interface WorkflowCondition {
  field: string;
  operator: string;
  value: string;
}

export interface FormWorkflowStep {
  sequenceNo: number;
  eventName: string;
  conditionRule: string;
  responseAction: string;
}

export interface ConditionColumnOption {
  field: string;
  label: string;
  type: 'number' | 'text' | 'select';
  options?: string[];
}

const PURCHASE_COLUMNS: ConditionColumnOption[] = [
  { field: 'Amount', label: 'Amount (Excl. VAT)', type: 'number' },
  { field: 'TotalAmountIncludingVat', label: 'Amount Incl. VAT', type: 'number' },
  { field: 'DocumentType', label: 'Document Type', type: 'select', options: ['Order', 'Invoice', 'CreditMemo', 'Quote'] },
  { field: 'BuyFromVendorNo', label: 'Buy-From Vendor No.', type: 'text' },
  { field: 'PayToVendorNo', label: 'Pay-To Vendor No.', type: 'text' },
  { field: 'CurrencyCode', label: 'Currency Code', type: 'text' },
  { field: 'PaymentTermsCode', label: 'Payment Terms Code', type: 'text' },
  { field: 'LocationCode', label: 'Location Code', type: 'text' },
];

const SALES_COLUMNS: ConditionColumnOption[] = [
  { field: 'Amount', label: 'Amount (Excl. VAT)', type: 'number' },
  { field: 'TotalAmountIncludingVat', label: 'Amount Incl. VAT', type: 'number' },
  { field: 'DocumentType', label: 'Document Type', type: 'select', options: ['Order', 'Invoice', 'CreditMemo', 'Quote'] },
  { field: 'SellToCustomerNo', label: 'Sell-To Customer No.', type: 'text' },
  { field: 'BillToCustomerNo', label: 'Bill-To Customer No.', type: 'text' },
  { field: 'CurrencyCode', label: 'Currency Code', type: 'text' },
  { field: 'PaymentTermsCode', label: 'Payment Terms Code', type: 'text' },
  { field: 'LocationCode', label: 'Location Code', type: 'text' },
];

/** Voucher, requisition and tender conditions are evaluated on the amount only. */
const AMOUNT_COLUMNS: ConditionColumnOption[] = [{ field: 'Amount', label: 'Amount (Excl. VAT)', type: 'number' }];

const DOCUMENT_LABELS: Record<ApprovalDocumentKind, string> = {
  [ApprovalDocumentKind.SalesDocument]: 'sales document',
  [ApprovalDocumentKind.PurchaseDocument]: 'purchase document',
  [ApprovalDocumentKind.PaymentVoucher]: 'payment voucher',
  [ApprovalDocumentKind.PurchaseRequisition]: 'purchase requisition',
  [ApprovalDocumentKind.Tender]: 'tender',
  [ApprovalDocumentKind.LeaveApplication]: 'leave application',
};

/** Same wording as the server: the trigger event of a document kind. */
export function requestedEventName(kind: ApprovalDocumentKind): string {
  return `Approval of a ${DOCUMENT_LABELS[kind] ?? 'purchase document'} is requested.`;
}

const NOT_ACTED_ON_EVENT = 'An approval request is not acted on.';
const AUTO_DELEGATE_RESPONSE = 'Delegate the request to the substitute, or to the approver\'s approver.';

const WORKFLOW_EVENTS: string[] = [
  ...Object.values(DOCUMENT_LABELS).map(label => `Approval of a ${label} is requested.`),
  'An approval request is approved.',
  'An approval request is rejected.',
  'An approval request is canceled.',
  'An approval request is delegated.',
  NOT_ACTED_ON_EVENT,
  'A document status is changed to Pending Approval.',
];

const WORKFLOW_RESPONSES: string[] = [
  'Set the document status to Pending Approval. Create an approval request. Send the first request.',
  'Send the next approval request.',
  'Release the document.',
  'Reject all approval requests. Reopen the document.',
  'Cancel all approval requests. Reopen the document.',
  'Send the request to the substitute, or to the approver\'s approver.',
  AUTO_DELEGATE_RESPONSE,
];

/**
 * Approval workflows: who approves (approver chain, specific approver or workflow user group),
 * dynamic conditions, the approval sequence, automatic delegation, templates and copies.
 */
@Component({
  selector: 'app-workflows',
  templateUrl: './workflows.component.html',
  standalone: false,
})
export class WorkflowsComponent implements OnInit {
  private readonly destroyRef = inject(DestroyRef);
  private readonly userSetupService = inject(ApprovalUserSetupService);
  private readonly userGroupService = inject(WorkflowUserGroupService);

  readonly documentKinds = approvalDocumentKindOptions;
  readonly limitTypes = approverLimitTypeOptions;
  readonly approverTypes = workflowApproverTypeOptions;
  readonly ApproverLimitType = ApproverLimitType;
  readonly ApprovalDocumentKind = ApprovalDocumentKind;
  readonly WorkflowApproverType = WorkflowApproverType;
  readonly workflowEvents = WORKFLOW_EVENTS;
  readonly workflowResponses = WORKFLOW_RESPONSES;
  readonly operators = ['>=', '>', '<=', '<', '=', '<>', 'contains'];

  readonly canManage = inject(PermissionService).getGrantedPolicy('Erp.Workflows.Manage');

  workflows: WorkflowDto[] = [];
  expandedId: string | null = null;
  loading = false;

  isModalOpen = false;
  isBusy = false;
  selected: WorkflowDto | null = null;
  form!: FormGroup;

  activeModalTab: 'general' | 'conditions' | 'sequence' = 'general';
  conditions: WorkflowCondition[] = [];
  formSteps: FormWorkflowStep[] = [];
  approvalUsers: ApprovalUserSetupDto[] = [];
  userGroups: CodeTableDto[] = [];

  // New from template
  isTemplateModalOpen = false;
  templates: WorkflowTemplateDto[] = [];
  templateForm!: FormGroup;

  // Copy
  isCopyModalOpen = false;
  copySource: WorkflowDto | null = null;
  copyForm!: FormGroup;

  // Step editor state
  editingStepIndex: number | null = null;
  stepForm!: FormGroup;

  constructor(
    private readonly service: WorkflowService,
    private readonly fb: FormBuilder,
    private readonly companyService: CompanyService,
    private readonly toaster: ToasterService,
    private readonly confirmation: ConfirmationService,
  ) {}

  ngOnInit(): void {
    this.load();
    this.loadApprovalUsers();
    this.loadUserGroups();
    this.companyService.companyChanged$
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(() => {
        this.load();
        this.loadApprovalUsers();
        this.loadUserGroups();
      });
  }

  load(): void {
    this.loading = true;
    this.service
      .getList()
      .pipe(
        finalize(() => (this.loading = false)),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe(result => (this.workflows = result.items ?? []));
  }

  loadApprovalUsers(): void {
    this.userSetupService
      .getList({ maxResultCount: 100, skipCount: 0 })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(res => {
        this.approvalUsers = res.items ?? [];
      });
  }

  loadUserGroups(): void {
    this.userGroupService
      .getList({ maxResultCount: 1000, skipCount: 0 })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(res => (this.userGroups = res.items ?? []));
  }

  limitTypeName(workflow: WorkflowDto): string {
    return ApproverLimitType[workflow.approverLimitType ?? ApproverLimitType.ApproverChain];
  }

  /** What the Approvers column shows: the group, the specific approver or the limit type. */
  approversLabel(workflow: WorkflowDto): string {
    if (workflow.approverType === WorkflowApproverType.WorkflowUserGroup) {
      return workflow.workflowUserGroupCode ?? '';
    }
    if (workflow.approverLimitType === ApproverLimitType.SpecificApprover) {
      return workflow.specificApproverUserName ?? '';
    }
    return '';
  }

  approverTypeName(type: WorkflowApproverType | undefined): string {
    return WorkflowApproverType[type ?? WorkflowApproverType.Approver];
  }

  get isGroupApproval(): boolean {
    return this.form?.get('approverType')?.value === WorkflowApproverType.WorkflowUserGroup;
  }

  get needsSpecificApprover(): boolean {
    return !this.isGroupApproval && this.form?.get('approverLimitType')?.value === ApproverLimitType.SpecificApprover;
  }

  // --- New from template / copy ---

  openTemplates(): void {
    this.templateForm = this.fb.group({
      templateCode: ['', Validators.required],
      code: ['', [Validators.required, Validators.maxLength(30)]],
      description: ['', Validators.maxLength(250)],
      workflowUserGroupCode: [''],
    });
    this.service
      .getTemplates()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(res => {
        this.templates = res.items ?? [];
        this.isTemplateModalOpen = true;
      });
  }

  get selectedTemplate(): WorkflowTemplateDto | undefined {
    const code = this.templateForm?.get('templateCode')?.value;
    return this.templates.find(t => t.code === code);
  }

  pickTemplate(template: WorkflowTemplateDto): void {
    this.templateForm.patchValue({ templateCode: template.code, description: template.description });
  }

  createFromTemplate(): void {
    const template = this.selectedTemplate;
    const raw = this.templateForm.getRawValue();
    const needsGroup = template?.approverType === WorkflowApproverType.WorkflowUserGroup;
    if (this.templateForm.invalid || this.isBusy || (needsGroup && !raw.workflowUserGroupCode)) {
      this.templateForm.markAllAsTouched();
      return;
    }

    this.isBusy = true;
    this.service
      .newFromTemplate({
        templateCode: raw.templateCode,
        code: raw.code,
        description: raw.description,
        workflowUserGroupCode: needsGroup ? raw.workflowUserGroupCode : undefined,
      })
      .pipe(
        finalize(() => (this.isBusy = false)),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe(created => {
        this.isTemplateModalOpen = false;
        this.toaster.success('Erp::SavedSuccessfully');
        this.load();
        this.openEdit(created);
      });
  }

  openCopy(workflow: WorkflowDto): void {
    this.copySource = workflow;
    this.copyForm = this.fb.group({
      code: [`${workflow.code}-COPY`.slice(0, 30), [Validators.required, Validators.maxLength(30)]],
      description: [workflow.description ?? '', Validators.maxLength(250)],
    });
    this.isCopyModalOpen = true;
  }

  copy(): void {
    if (!this.copySource?.id || this.copyForm.invalid || this.isBusy) {
      this.copyForm?.markAllAsTouched();
      return;
    }

    this.isBusy = true;
    this.service
      .copy(this.copySource.id, this.copyForm.getRawValue())
      .pipe(
        finalize(() => (this.isBusy = false)),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe(() => {
        this.isCopyModalOpen = false;
        this.toaster.success('Erp::SavedSuccessfully');
        this.load();
      });
  }

  kindName(workflow: WorkflowDto): string {
    return ApprovalDocumentKind[workflow.documentKind ?? ApprovalDocumentKind.SalesDocument];
  }

  toggleSteps(workflow: WorkflowDto): void {
    this.expandedId = this.expandedId === workflow.id ? null : (workflow.id ?? null);
  }

  /** Enabling is what makes documents need approval, so it is confirmed; disabling is not. */
  setEnabled(workflow: WorkflowDto, enabled: boolean): void {
    if (!workflow.id) {
      return;
    }
    const id = workflow.id;

    if (!enabled) {
      this.service
        .disable(id)
        .pipe(takeUntilDestroyed(this.destroyRef))
        .subscribe(() => this.load());
      return;
    }

    this.confirmation
      .warn('Erp::EnableWorkflowConfirmation', 'Erp::AreYouSure')
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(status => {
        if (status === Confirmation.Status.confirm) {
          this.service
            .enable(id)
            .pipe(takeUntilDestroyed(this.destroyRef))
            .subscribe(() => this.load());
        } else {
          this.load(); // put the switch back
        }
      });
  }

  get availableColumns(): ConditionColumnOption[] {
    const kind = this.form?.get('documentKind')?.value;
    if (kind === ApprovalDocumentKind.PurchaseDocument) {
      return PURCHASE_COLUMNS;
    }
    return kind === ApprovalDocumentKind.SalesDocument ? SALES_COLUMNS : AMOUNT_COLUMNS;
  }

  openCreate(): void {
    this.selected = null;
    this.activeModalTab = 'general';
    this.editingStepIndex = null;
    this.form = this.buildForm();
    this.initConditions([]);
    this.initDefaultSteps(ApprovalDocumentKind.PurchaseDocument);
    this.isModalOpen = true;
  }

  openEdit(workflow: WorkflowDto): void {
    this.selected = workflow;
    this.activeModalTab = 'general';
    this.editingStepIndex = null;
    this.form = this.buildForm(workflow);
    this.initConditionsFromWorkflow(workflow);
    this.initStepsFromWorkflow(workflow);
    this.isModalOpen = true;
  }

  onDocumentKindChange(kind: ApprovalDocumentKind): void {
    // The trigger step follows the document kind.
    if (this.formSteps.length > 0 && /^Approval of an? .* is requested\.$/.test(this.formSteps[0].eventName)) {
      this.formSteps[0].eventName = requestedEventName(kind);
    }
    // Filter conditions whose field is not in available columns
    const allowed = new Set(this.availableColumns.map(c => c.field));
    this.conditions = this.conditions.filter(c => allowed.has(c.field));
  }

  // --- Dynamic Conditions ---

  addCondition(field?: string, operator?: string, value?: string): void {
    const defaultField = field ?? (this.availableColumns[0]?.field || 'Amount');
    this.conditions.push({
      field: defaultField,
      operator: operator ?? '>=',
      value: value ?? (defaultField.includes('Amount') ? '0' : ''),
    });
    this.syncConditionsToForm();
  }

  removeCondition(index: number): void {
    this.conditions.splice(index, 1);
    this.syncConditionsToForm();
  }

  onConditionFieldChange(condition: WorkflowCondition): void {
    const col = this.availableColumns.find(c => c.field === condition.field);
    if (col?.type === 'select' && col.options && col.options.length > 0) {
      condition.operator = '=';
      condition.value = col.options[0];
    } else if (col?.type === 'number') {
      condition.operator = '>=';
      condition.value = '0';
    } else {
      condition.operator = '=';
      condition.value = '';
    }
    this.syncConditionsToForm();
  }

  onConditionValueChange(): void {
    this.syncConditionsToForm();
  }

  getColumnOption(field: string): ConditionColumnOption | undefined {
    return this.availableColumns.find(c => c.field === field);
  }

  syncConditionsToForm(): void {
    const amountCondition = this.conditions.find(c => c.field === 'Amount');
    if (amountCondition) {
      const num = parseFloat(amountCondition.value);
      if (!isNaN(num) && num >= 0) {
        this.form.get('minimumAmount')?.setValue(num, { emitEvent: false });
      }
    }
    // Update first step condition rule with dynamic condition summary
    if (this.formSteps.length > 0) {
      this.formSteps[0].conditionRule = this.getConditionRuleSummary();
    }
  }

  getConditionRuleSummary(): string {
    if (this.conditions.length === 0) {
      return 'Always';
    }
    return this.conditions
      .map(c => `${c.field} ${c.operator} ${c.value}`)
      .join('; ');
  }

  private initConditions(items: WorkflowCondition[]): void {
    this.conditions = items.length > 0 ? items : [];
    if (this.conditions.length === 0) {
      // Add default Amount >= 0 condition
      this.conditions = [{ field: 'Amount', operator: '>=', value: '0' }];
    }
  }

  private initConditionsFromWorkflow(workflow: WorkflowDto): void {
    const parsed: WorkflowCondition[] = [];
    const triggerStep = workflow.steps?.find(s => s.eventName?.includes('requested')) ?? workflow.steps?.[0];
    const ruleStr = triggerStep?.conditionRule;

    if (ruleStr && ruleStr !== 'Always') {
      const parts = ruleStr.split(';').map(p => p.trim()).filter(Boolean);
      for (const part of parts) {
        for (const op of ['>=', '<=', '<>', '!=', '>', '<', '=', 'contains']) {
          const idx = part.indexOf(op);
          if (idx > 0) {
            const rawField = part.substring(0, idx).trim();
            const rawVal = part.substring(idx + op.length).trim().replace(/^['"]|['"]$/g, '');
            // Match against available columns
            const matchedCol = this.availableColumns.find(
              c => c.field.toLowerCase() === rawField.toLowerCase().replace(/\s+/g, '')
            );
            if (matchedCol) {
              parsed.push({
                field: matchedCol.field,
                operator: op === '!=' ? '<>' : op,
                value: rawVal,
              });
            }
            break;
          }
        }
      }
    }

    if (parsed.length === 0 && workflow.minimumAmount > 0) {
      parsed.push({
        field: 'Amount',
        operator: '>=',
        value: String(workflow.minimumAmount),
      });
    }

    this.initConditions(parsed);
  }

  // --- Approval Sequence / Steps ---

  /** What the first response says it creates, matching the server text. */
  private describeApprovers(): string {
    const raw = this.form?.getRawValue();
    if (raw?.approverType === WorkflowApproverType.WorkflowUserGroup) {
      return `workflow user group ${(raw.workflowUserGroupCode ?? '').toUpperCase()}`;
    }
    if (raw?.approverLimitType === ApproverLimitType.SpecificApprover) {
      const user = this.approvalUsers.find(u => u.userId === raw.specificApproverUserId);
      return `specific approver ${user?.userName ?? ''}`;
    }
    return ApproverLimitType[raw?.approverLimitType ?? ApproverLimitType.DirectApprover];
  }

  initDefaultSteps(kind: ApprovalDocumentKind): void {
    const condition = this.getConditionRuleSummary();
    const delegateAfterDays = this.form?.get('delegateAfterDays')?.value ?? 0;

    this.formSteps = [
      {
        sequenceNo: 1,
        eventName: requestedEventName(kind),
        conditionRule: condition,
        responseAction: `Set the document status to Pending Approval. Create an approval request (${this.describeApprovers()}). Send the first request.`,
      },
      {
        sequenceNo: 2,
        eventName: 'An approval request is approved.',
        conditionRule: 'Pending approvals: > 0',
        responseAction: 'Send the next approval request.',
      },
      {
        sequenceNo: 3,
        eventName: 'An approval request is approved.',
        conditionRule: 'Pending approvals: 0',
        responseAction: 'Release the document.',
      },
      {
        sequenceNo: 4,
        eventName: 'An approval request is rejected.',
        conditionRule: 'Always',
        responseAction: 'Reject all approval requests. Reopen the document.',
      },
      {
        sequenceNo: 5,
        eventName: 'An approval request is canceled.',
        conditionRule: 'Always',
        responseAction: 'Cancel all approval requests. Reopen the document.',
      },
      {
        sequenceNo: 6,
        eventName: 'An approval request is delegated.',
        conditionRule: 'Always',
        responseAction: 'Send the request to the substitute, or to the approver\'s approver.',
      },
    ];

    if (delegateAfterDays > 0) {
      this.formSteps.push({
        sequenceNo: 7,
        eventName: NOT_ACTED_ON_EVENT,
        conditionRule: `Open for ${delegateAfterDays} day(s)`,
        responseAction: AUTO_DELEGATE_RESPONSE,
      });
    }
  }

  initStepsFromWorkflow(workflow: WorkflowDto): void {
    if (workflow.steps && workflow.steps.length > 0) {
      this.formSteps = workflow.steps
        .slice()
        .sort((a, b) => a.sequenceNo - b.sequenceNo)
        .map(s => ({
          sequenceNo: s.sequenceNo,
          eventName: s.eventName ?? '',
          conditionRule: s.conditionRule ?? 'Always',
          responseAction: s.responseAction ?? '',
        }));
    } else {
      this.initDefaultSteps(workflow.documentKind);
    }
  }

  resetToDefaults(): void {
    const kind = this.form?.get('documentKind')?.value ?? ApprovalDocumentKind.PurchaseDocument;
    this.initDefaultSteps(kind);
    this.editingStepIndex = null;
    this.toaster.info('Erp::WorkflowDefaultsRestored');
  }

  addStep(): void {
    const newSeq = this.formSteps.length + 1;
    const step: FormWorkflowStep = {
      sequenceNo: newSeq,
      eventName: this.workflowEvents[0],
      conditionRule: 'Always',
      responseAction: this.workflowResponses[0],
    };
    this.formSteps.push(step);
    this.startEditingStep(this.formSteps.length - 1);
  }

  startEditingStep(index: number): void {
    this.editingStepIndex = index;
    const step = this.formSteps[index];
    this.stepForm = this.fb.group({
      eventName: [step.eventName, Validators.required],
      conditionPreset: ['custom'],
      conditionRule: [step.conditionRule, Validators.required],
      responseAction: [step.responseAction, Validators.required],
    });
  }

  cancelEditingStep(): void {
    this.editingStepIndex = null;
  }

  saveEditingStep(): void {
    if (this.stepForm.invalid || this.editingStepIndex === null) {
      return;
    }
    const val = this.stepForm.value;
    const step = this.formSteps[this.editingStepIndex];
    step.eventName = val.eventName;
    step.conditionRule = val.conditionRule;
    step.responseAction = val.responseAction;
    this.editingStepIndex = null;
  }

  onConditionPresetChange(preset: string): void {
    if (!this.stepForm) return;
    if (preset === 'dynamic') {
      this.stepForm.get('conditionRule')?.setValue(this.getConditionRuleSummary());
    } else if (preset === 'always') {
      this.stepForm.get('conditionRule')?.setValue('Always');
    } else if (preset === 'pending_gt_0') {
      this.stepForm.get('conditionRule')?.setValue('Pending approvals: > 0');
    } else if (preset === 'pending_eq_0') {
      this.stepForm.get('conditionRule')?.setValue('Pending approvals: 0');
    }
  }

  removeStep(index: number): void {
    this.formSteps.splice(index, 1);
    this.formSteps.forEach((s, idx) => (s.sequenceNo = idx + 1));
    if (this.editingStepIndex === index) {
      this.editingStepIndex = null;
    } else if (this.editingStepIndex !== null && this.editingStepIndex > index) {
      this.editingStepIndex--;
    }
  }

  moveStepUp(index: number): void {
    if (index <= 0) return;
    const temp = this.formSteps[index];
    this.formSteps[index] = this.formSteps[index - 1];
    this.formSteps[index - 1] = temp;
    this.formSteps.forEach((s, idx) => (s.sequenceNo = idx + 1));
  }

  moveStepDown(index: number): void {
    if (index >= this.formSteps.length - 1) return;
    const temp = this.formSteps[index];
    this.formSteps[index] = this.formSteps[index + 1];
    this.formSteps[index + 1] = temp;
    this.formSteps.forEach((s, idx) => (s.sequenceNo = idx + 1));
  }

  // --- Save / Submit ---

  save(): void {
    if (this.form.invalid || this.isBusy) {
      this.form.markAllAsTouched();
      return;
    }

    if (this.editingStepIndex !== null) {
      this.saveEditingStep();
    }

    // Ensure first step condition rule reflects dynamic conditions
    if (this.formSteps.length > 0) {
      this.formSteps[0].conditionRule = this.getConditionRuleSummary();
    }

    const raw = this.form.getRawValue();
    if (this.isGroupApproval && !raw.workflowUserGroupCode) {
      this.activeModalTab = 'general';
      this.form.get('workflowUserGroupCode')?.markAsTouched();
      this.toaster.warn('Erp::Erp:Approvals:00016');
      return;
    }
    if (this.needsSpecificApprover && !raw.specificApproverUserId) {
      this.activeModalTab = 'general';
      this.form.get('specificApproverUserId')?.markAsTouched();
      this.toaster.warn('Erp::Erp:Approvals:00018');
      return;
    }

    const stepsPayload: CreateUpdateWorkflowStepDto[] = this.formSteps.map(s => ({
      sequenceNo: s.sequenceNo,
      eventName: s.eventName,
      conditionRule: s.conditionRule,
      responseAction: s.responseAction,
    }));

    const input: CreateUpdateWorkflowDto = {
      code: raw.code,
      description: raw.description,
      documentKind: raw.documentKind,
      minimumAmount: raw.minimumAmount,
      approverLimitType: raw.approverLimitType,
      dueDays: raw.dueDays,
      approverType: raw.approverType,
      workflowUserGroupCode: this.isGroupApproval ? raw.workflowUserGroupCode : undefined,
      specificApproverUserId: this.needsSpecificApprover ? raw.specificApproverUserId : undefined,
      delegateAfterDays: raw.delegateAfterDays ?? 0,
      steps: stepsPayload,
    };

    const request$ = this.selected?.id
      ? this.service.update(this.selected.id, input)
      : this.service.create(input);

    this.isBusy = true;
    request$
      .pipe(
        finalize(() => (this.isBusy = false)),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe(() => {
        this.isModalOpen = false;
        this.toaster.success('Erp::SavedSuccessfully');
        this.load();
      });
  }

  remove(workflow: WorkflowDto): void {
    if (!workflow.id) {
      return;
    }
    const id = workflow.id;

    this.confirmation
      .warn('Erp::ItemWillBeDeletedMessage', 'Erp::AreYouSure')
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(status => {
        if (status === Confirmation.Status.confirm) {
          this.service
            .delete(id)
            .pipe(takeUntilDestroyed(this.destroyRef))
            .subscribe(() => {
              this.toaster.success('Erp::DeletedSuccessfully');
              this.load();
            });
        }
      });
  }

  private refreshFirstResponse(): void {
    const first = this.formSteps[0];
    if (first?.responseAction.startsWith('Set the document status to Pending Approval. Create an approval request')) {
      first.responseAction = `Set the document status to Pending Approval. Create an approval request (${this.describeApprovers()}). Send the first request.`;
    }
  }

  /** Keeps the "not acted on" step in line with the delegate-after setting. */
  private syncDelegateStep(days: number): void {
    const index = this.formSteps.findIndex(s => s.eventName === NOT_ACTED_ON_EVENT);
    if (days > 0 && index < 0) {
      this.formSteps.push({
        sequenceNo: this.formSteps.length + 1,
        eventName: NOT_ACTED_ON_EVENT,
        conditionRule: `Open for ${days} day(s)`,
        responseAction: AUTO_DELEGATE_RESPONSE,
      });
    } else if (days > 0) {
      this.formSteps[index].conditionRule = `Open for ${days} day(s)`;
    } else if (index >= 0) {
      this.removeStep(index);
    }
  }

  private buildForm(workflow?: WorkflowDto): FormGroup {
    const grp = this.fb.group({
      code: [
        { value: workflow?.code ?? '', disabled: !!workflow },
        [Validators.required, Validators.maxLength(30)],
      ],
      description: [workflow?.description ?? '', Validators.maxLength(250)],
      documentKind: [
        workflow?.documentKind ?? ApprovalDocumentKind.PurchaseDocument,
        Validators.required,
      ],
      minimumAmount: [workflow?.minimumAmount ?? 0, [Validators.required, Validators.min(0)]],
      approverLimitType: [
        workflow?.approverLimitType ?? ApproverLimitType.DirectApprover,
        Validators.required,
      ],
      dueDays: [
        workflow?.dueDays ?? 3,
        [Validators.required, Validators.min(0), Validators.max(3650)],
      ],
      approverType: [workflow?.approverType ?? WorkflowApproverType.Approver, Validators.required],
      workflowUserGroupCode: [workflow?.workflowUserGroupCode ?? ''],
      specificApproverUserId: [workflow?.specificApproverUserId ?? null],
      delegateAfterDays: [
        workflow?.delegateAfterDays ?? 0,
        [Validators.required, Validators.min(0), Validators.max(365)],
      ],
    });

    // The first response names the approvers, so it follows these settings.
    for (const name of ['approverType', 'approverLimitType', 'workflowUserGroupCode', 'specificApproverUserId']) {
      grp.get(name)?.valueChanges
        .pipe(takeUntilDestroyed(this.destroyRef))
        .subscribe(() => this.refreshFirstResponse());
    }
    grp.get('delegateAfterDays')?.valueChanges
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(days => this.syncDelegateStep(days ?? 0));

    grp.get('documentKind')?.valueChanges
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(val => this.onDocumentKindChange(val));

    grp.get('minimumAmount')?.valueChanges
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(minVal => {
        const amtCondition = this.conditions.find(c => c.field === 'Amount');
        if (amtCondition) {
          amtCondition.value = String(minVal ?? 0);
          if (this.formSteps.length > 0) {
            this.formSteps[0].conditionRule = this.getConditionRuleSummary();
          }
        }
      });

    return grp;
  }
}
