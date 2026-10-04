import { LocalizationService } from '@abp/ng.core';
import { Confirmation, ConfirmationService, ToasterService } from '@abp/ng.theme.shared';
import { Component, DestroyRef, OnInit, ViewChild, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { GLRegisterDto, GetGLRegistersInput, GlRegisterService, PostingPreviewLineDto } from '@proxy/finance';
import { ErpTableAction, ErpTableColumn, ErpTableComponent, ErpTableSource } from '../../erp-shared';
import { CompanyService } from '../../services/company.service';

/**
 * G/L registers: what each posting run wrote, and the action that undoes it.
 *together with its Reverse Transaction action.
 */
@Component({
  selector: 'app-gl-registers',
  templateUrl: './gl-registers.component.html',
  standalone: false,
})
export class GLRegistersComponent implements OnInit {
  @ViewChild(ErpTableComponent) table?: ErpTableComponent<GLRegisterDto>;

  private readonly destroyRef = inject(DestroyRef);
  private readonly toaster = inject(ToasterService);
  private readonly confirmation = inject(ConfirmationService);
  private readonly companyService = inject(CompanyService);
  private readonly localization = inject(LocalizationService);
  private readonly registers = inject(GlRegisterService);

  readonly columns: ErpTableColumn<GLRegisterDto>[] = [
    { field: 'no', labelKey: 'Erp::RegisterNo', type: 'code', width: 110 },
    { field: 'postingDate', labelKey: 'Erp::PostingDate', type: 'date', width: 140 },
    { field: 'journalBatchName', labelKey: 'Erp::Batch', width: 200 },
    { field: 'sourceCode', labelKey: 'Erp::SourceCode', width: 120 },
    { field: 'userName', labelKey: 'Erp::PostedBy', width: 150 },
    { field: 'transactionNo', labelKey: 'Erp::TransactionNo', type: 'number', width: 130 },
    { field: 'reversed', labelKey: 'Erp::Reversed', type: 'boolean', width: 160 },
  ];

  readonly actions: ErpTableAction<GLRegisterDto>[] = [
    { key: 'entries', title: 'Erp::ViewEntries', icon: 'fas fa-list', action: row => this.showEntries(row) },
    {
      key: 'reverse',
      title: 'Erp::Reverse',
      icon: 'fas fa-undo',
      btnClass: 'btn-outline-danger',
      permission: 'Erp.GLRegisters.Reverse',
      disabled: row => this.busy || !row.isReversible,
      action: row => this.reverse(row),
    },
  ];

  readonly source: ErpTableSource<GLRegisterDto> = query =>
    this.registers.getList({ ...query, onlyReversible: this.onlyReversible } as GetGLRegistersInput);

  onlyReversible = false;
  busy = false;

  /** The register whose entries are on screen, and the entries themselves. */
  openRegisterNo: number | null = null;
  entries: PostingPreviewLineDto[] = [];

  ngOnInit(): void {
    this.companyService.companyChanged$.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(() => {
      this.closeEntries();
      this.table?.reload();
    });
  }

  toggleOnlyReversible(value: boolean): void {
    this.onlyReversible = value;
    this.table?.reload();
  }

  showEntries(register: GLRegisterDto): void {
    if (this.openRegisterNo === register.no) {
      this.closeEntries();
      return;
    }

    this.registers
      .getEntries(register.no)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(result => {
        this.openRegisterNo = register.no;
        this.entries = result.items ?? [];
      });
  }

  closeEntries(): void {
    this.openRegisterNo = null;
    this.entries = [];
  }

  /**
   * Reversal writes a mirror image rather than deleting anything, so the confirmation says so
   * plainly: the original stays on record next to its correction.
   */
  reverse(register: GLRegisterDto): void {
    this.confirmation
      .warn(
        this.localization.instant('Erp::ReverseRegisterConfirmation', String(register.no)),
        'Erp::AreYouSure',
      )
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(status => {
        if (status !== Confirmation.Status.confirm) {
          return;
        }

        this.busy = true;
        this.registers
          .runReversal({ registerNo: register.no })
          .pipe(takeUntilDestroyed(this.destroyRef))
          .subscribe({
            next: result => {
              this.busy = false;
              this.toaster.success(
                this.localization.instant(
                  'Erp::ReversedSuccessfully',
                  String(result.reversalRegisterNo),
                ),
              );
              this.closeEntries();
              this.table?.reload();
            },
            error: () => (this.busy = false),
          });
      });
  }
}
