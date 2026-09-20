import { PermissionService } from '@abp/ng.core';
import { Confirmation, ConfirmationService, ToasterService } from '@abp/ng.theme.shared';
import { Component, DestroyRef, OnInit, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import {
  ApprovalDocumentKind,
  ApproverLimitType,
  CreateUpdateWorkflowDto,
  WorkflowDto,
  WorkflowService,
  approvalDocumentKindOptions,
  approverLimitTypeOptions,
} from '@proxy/workflows';
import { finalize } from 'rxjs';
import { CompanyService } from '../../services/company.service';

/** Approval workflows. Mirrors Business Central page 1500 "Workflows", limited to approval templates. */
@Component({
  selector: 'app-workflows',
  templateUrl: './workflows.component.html',
})
export class WorkflowsComponent implements OnInit {
  private readonly destroyRef = inject(DestroyRef);

  readonly documentKinds = approvalDocumentKindOptions;
  readonly limitTypes = approverLimitTypeOptions;

  readonly canManage = inject(PermissionService).getGrantedPolicy('Erp.Workflows.Manage');

  workflows: WorkflowDto[] = [];
  expandedId: string | null = null;
  loading = false;

  isModalOpen = false;
  isBusy = false;
  selected: WorkflowDto | null = null;
  form!: FormGroup;

  constructor(
    private readonly service: WorkflowService,
    private readonly fb: FormBuilder,
    private readonly companyService: CompanyService,
    private readonly toaster: ToasterService,
    private readonly confirmation: ConfirmationService,
  ) {}

  ngOnInit(): void {
    this.load();
    this.companyService.companyChanged$
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(() => this.load());
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

  limitTypeName(workflow: WorkflowDto): string {
    return ApproverLimitType[workflow.approverLimitType ?? ApproverLimitType.ApproverChain];
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

  openCreate(): void {
    this.selected = null;
    this.form = this.buildForm();
    this.isModalOpen = true;
  }

  openEdit(workflow: WorkflowDto): void {
    this.selected = workflow;
    this.form = this.buildForm(workflow);
    this.isModalOpen = true;
  }

  save(): void {
    if (this.form.invalid || this.isBusy) {
      this.form.markAllAsTouched();
      return;
    }

    const input = this.form.getRawValue() as CreateUpdateWorkflowDto;
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

  private buildForm(workflow?: WorkflowDto): FormGroup {
    return this.fb.group({
      code: [
        { value: workflow?.code ?? '', disabled: !!workflow },
        [Validators.required, Validators.maxLength(30)],
      ],
      description: [workflow?.description ?? '', Validators.maxLength(250)],
      documentKind: [
        workflow?.documentKind ?? ApprovalDocumentKind.SalesDocument,
        Validators.required,
      ],
      minimumAmount: [workflow?.minimumAmount ?? 0, [Validators.required, Validators.min(0)]],
      approverLimitType: [
        workflow?.approverLimitType ?? ApproverLimitType.ApproverChain,
        Validators.required,
      ],
      dueDays: [
        workflow?.dueDays ?? 0,
        [Validators.required, Validators.min(0), Validators.max(3650)],
      ],
    });
  }
}
