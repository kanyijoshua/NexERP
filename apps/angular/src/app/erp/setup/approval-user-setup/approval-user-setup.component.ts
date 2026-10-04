import { IdentityUserService } from '@abp/ng.identity/proxy';
import { Component, OnInit } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import {
  ApprovalUserSetupDto,
  ApprovalUserSetupService,
  CreateUpdateApprovalUserSetupDto,
  GetApprovalUserSetupListInput,
} from '@proxy/workflows';
import { Observable, map } from 'rxjs';
import { ErpTableColumn, ErpTableCrudBase, LookupItem } from '../../erp-shared';

/**
 * Who approves whose requests, and up to what amount.
 *
 */
@Component({
    selector: 'app-approval-user-setup',
    templateUrl: './approval-user-setup.component.html',
    standalone: false
})
export class ApprovalUserSetupComponent
  extends ErpTableCrudBase<ApprovalUserSetupDto, CreateUpdateApprovalUserSetupDto>
  implements OnInit
{
  override readonly columns: ErpTableColumn<ApprovalUserSetupDto>[] = [
    { field: 'userName', labelKey: 'Erp::User', width: 190 },
    // Names looked up after the query: shown, not filtered or sorted.
    { field: 'approverUserName', labelKey: 'Erp::Approver', width: 150, sortable: false, filterable: false },
    { field: 'substituteUserName', labelKey: 'Erp::Substitute', width: 150, sortable: false, filterable: false },
    { field: 'salesAmountApprovalLimit', labelKey: 'Erp::SalesAmountApprovalLimit', type: 'currency', width: 180 },
    { field: 'purchaseAmountApprovalLimit', labelKey: 'Erp::PurchaseAmountApprovalLimit', type: 'currency', width: 190 },
    { field: 'isApprovalAdministrator', labelKey: 'Erp::Administrator', type: 'boolean', width: 130, visible: false },
  ];

  protected override permissionPrefix = 'Erp.ApprovalUserSetup';

  /** Approver and substitute are chosen among users that already have a row here. */
  allSetups: ApprovalUserSetupDto[] = [];

  constructor(
    private readonly service: ApprovalUserSetupService,
    private readonly identityUsers: IdentityUserService,
    private readonly fb: FormBuilder,
  ) {
    super();
  }

  protected getList = (query: GetApprovalUserSetupListInput) => this.service.getList(query);
  protected create = (input: CreateUpdateApprovalUserSetupDto) => this.service.create(input);
  protected update = (id: string, input: CreateUpdateApprovalUserSetupDto) =>
    this.service.update(id, input);
  protected delete = (id: string) => this.service.delete(id);

  /** Users of the identity service, for the "User" field of a new row. */
  readonly searchUsers = (term: string): Observable<LookupItem[]> =>
    this.identityUsers
      .getList({ filter: term, maxResultCount: 20, skipCount: 0 } as never)
      .pipe(
        map(result =>
          (result.items ?? []).map(u => ({
            id: u.id,
            code: u.userName ?? '',
            name: u.email ?? undefined,
          })),
        ),
      );

  override ngOnInit(): void {
    super.ngOnInit();
    this.loadAllSetups();
  }

  /** A saved or deleted row changes the approver choices too. */
  protected override refresh(): void {
    super.refresh();
    this.loadAllSetups();
  }

  /** Everyone except the row's own user: nobody approves or substitutes for themselves. */
  othersThan(userId: string | null): ApprovalUserSetupDto[] {
    return this.allSetups.filter(s => s.userId !== userId);
  }

  onUserSelected(user: LookupItem | null): void {
    this.form.patchValue({ userName: user?.code ?? '' });
  }

  protected buildForm(item?: ApprovalUserSetupDto): FormGroup {
    const form = this.fb.group({
      userId: [{ value: item?.userId ?? null, disabled: !!item }, Validators.required],
      userName: [item?.userName ?? '', [Validators.required, Validators.maxLength(256)]],
      approverUserId: [item?.approverUserId ?? null],
      substituteUserId: [item?.substituteUserId ?? null],
      salesAmountApprovalLimit: [
        item?.salesAmountApprovalLimit ?? 0,
        [Validators.required, Validators.min(0)],
      ],
      unlimitedSalesApproval: [item?.unlimitedSalesApproval ?? false],
      purchaseAmountApprovalLimit: [
        item?.purchaseAmountApprovalLimit ?? 0,
        [Validators.required, Validators.min(0)],
      ],
      unlimitedPurchaseApproval: [item?.unlimitedPurchaseApproval ?? false],
      isApprovalAdministrator: [item?.isApprovalAdministrator ?? false],
    });

    // An amount means nothing next to "unlimited", so the field is switched off with it.
    this.linkUnlimited(form, 'unlimitedSalesApproval', 'salesAmountApprovalLimit');
    this.linkUnlimited(form, 'unlimitedPurchaseApproval', 'purchaseAmountApprovalLimit');
    return form;
  }

  private linkUnlimited(form: FormGroup, flag: string, amount: string): void {
    const apply = (unlimited: boolean) =>
      unlimited ? form.get(amount)?.disable() : form.get(amount)?.enable();
    apply(!!form.get(flag)?.value);
    form.get(flag)?.valueChanges.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(apply);
  }

  private loadAllSetups(): void {
    this.service
      .getList({ maxResultCount: 1000, skipCount: 0 } as never)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(result => (this.allSetups = result.items ?? []));
  }
}
