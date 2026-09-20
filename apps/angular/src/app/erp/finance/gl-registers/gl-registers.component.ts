import { ABP, ListService, LocalizationService, PagedResultDto } from '@abp/ng.core';
import { Confirmation, ConfirmationService, ToasterService } from '@abp/ng.theme.shared';
import { Component, DestroyRef, OnInit, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { GLRegisterDto, GlRegisterService, PostingPreviewLineDto } from '@proxy/finance';
import { Observable } from 'rxjs';
import { CompanyService } from '../../services/company.service';

/**
 * G/L registers: what each posting run wrote, and the action that undoes it.
 * Mirrors Business Central page 116 together with its Reverse Transaction action.
 */
@Component({
  selector: 'app-gl-registers',
  templateUrl: './gl-registers.component.html',
  providers: [ListService],
})
export class GLRegistersComponent implements OnInit {
  readonly list = inject<ListService<ABP.PageQueryParams>>(ListService);

  private readonly destroyRef = inject(DestroyRef);
  private readonly toaster = inject(ToasterService);
  private readonly confirmation = inject(ConfirmationService);
  private readonly companyService = inject(CompanyService);
  private readonly localization = inject(LocalizationService);
  private readonly registers = inject(GlRegisterService);

  data: PagedResultDto<GLRegisterDto> = { items: [], totalCount: 0 };
  onlyReversible = false;
  busy = false;

  /** The register whose entries are on screen, and the entries themselves. */
  openRegisterNo: number | null = null;
  entries: PostingPreviewLineDto[] = [];

  ngOnInit(): void {
    this.list
      .hookToQuery(query => this.getList(query))
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(result => (this.data = result));

    this.companyService.companyChanged$.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(() => {
      this.closeEntries();
      this.list.get();
    });
  }

  toggleOnlyReversible(value: boolean): void {
    this.onlyReversible = value;
    this.list.page = 0;
    this.list.get();
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
              this.list.get();
            },
            error: () => (this.busy = false),
          });
      });
  }

  private getList(query: ABP.PageQueryParams): Observable<PagedResultDto<GLRegisterDto>> {
    return this.registers.getList({ ...query, onlyReversible: this.onlyReversible } as never);
  }
}
