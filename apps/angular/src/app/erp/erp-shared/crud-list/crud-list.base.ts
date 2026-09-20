import { ABP, ListService, PagedResultDto } from '@abp/ng.core';
import { Confirmation, ConfirmationService, ToasterService } from '@abp/ng.theme.shared';
import { DestroyRef, Directive, OnInit, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormGroup } from '@angular/forms';
import { Observable, Subject, of } from 'rxjs';
import { debounceTime, distinctUntilChanged, finalize } from 'rxjs/operators';
import { CompanyService } from '../../services/company.service';

export const CRUD_FILTER_DEBOUNCE_MS = 300;

/**
 * Base class of the standard ABP list page (paged table + create/edit modal + delete).
 *
 * Usage:
 * ```ts
 * @Component({ ..., providers: [ListService] })
 * export class CustomersComponent extends CrudListBase<CustomerDto, CreateUpdateCustomerDto> {
 *   constructor(private service: CustomerService, private fb: FormBuilder) { super(); }
 *   protected getList = (query: ABP.PageQueryParams) => this.service.getList(query);
 *   ...
 * }
 * ```
 * Subclasses that implement `ngOnInit` must call `super.ngOnInit()`.
 * HTTP errors are NOT swallowed: ABP's default error handler shows them.
 */
@Directive()
export abstract class CrudListBase<TDto extends { id?: string }, TCreateUpdate> implements OnInit {
  /** Component level instance: subclasses must add `providers: [ListService]`. */
  readonly list = inject<ListService<ABP.PageQueryParams>>(ListService);
  protected readonly toaster = inject(ToasterService);
  protected readonly confirmation = inject(ConfirmationService);
  protected readonly companyService = inject(CompanyService);
  protected readonly destroyRef = inject(DestroyRef);

  data: PagedResultDto<TDto> = { items: [], totalCount: 0 };

  isModalOpen = false;
  isBusy = false;
  selected: TDto | null = null;
  form!: FormGroup;

  /** Localization keys, override to customise the messages. */
  protected savedMessageKey = 'Erp::SavedSuccessfully';
  protected deletedMessageKey = 'Erp::DeletedSuccessfully';
  protected deleteConfirmationMessageKey = 'Erp::ItemWillBeDeletedMessage';
  protected deleteConfirmationTitleKey = 'Erp::AreYouSure';

  private _filter = '';
  private readonly filter$ = new Subject<string>();

  get filter(): string {
    return this._filter;
  }

  /** Debounced: resets to the first page and re-queries 300 ms after the last change. */
  set filter(value: string) {
    this._filter = value ?? '';
    this.filter$.next(this._filter);
  }

  protected abstract getList(query: ABP.PageQueryParams): Observable<PagedResultDto<TDto>>;
  protected abstract buildForm(item?: TDto): FormGroup;
  protected abstract create(input: TCreateUpdate): Observable<unknown>;
  protected abstract update(id: string, input: TCreateUpdate): Observable<unknown>;
  protected abstract delete(id: string): Observable<unknown>;

  ngOnInit(): void {
    this.list
      .hookToQuery(query => this.getList(query))
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(result => (this.data = result));

    this.filter$
      .pipe(
        debounceTime(CRUD_FILTER_DEBOUNCE_MS),
        distinctUntilChanged(),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe(value => {
        if (this.list.page !== 0) {
          this.list.page = 0;
        }
        this.list.filter = value;
      });

    this.companyService.companyChanged$.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(() => {
      if (this.list.page !== 0) {
        this.list.page = 0;
      }
      this.list.get();
    });
  }

  /** Loads the full record before editing. Defaults to the row itself. */
  protected load(_id: string, row: TDto): Observable<TDto> {
    return of(row);
  }

  /** Maps the form value to the create/update DTO. Defaults to `form.getRawValue()`. */
  protected toInput(form: FormGroup): TCreateUpdate {
    return form.getRawValue() as TCreateUpdate;
  }

  openCreate(): void {
    this.selected = null;
    this.form = this.buildForm();
    this.isModalOpen = true;
  }

  openEdit(item: TDto): void {
    if (!item.id) {
      this.showEdit(item);
      return;
    }
    this.load(item.id, item)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(full => this.showEdit(full));
  }

  save(): void {
    if (!this.form || this.isBusy) {
      return;
    }
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const input = this.toInput(this.form);
    const id = this.selected?.id;
    const request$ = id ? this.update(id, input) : this.create(input);

    this.isBusy = true;
    request$
      .pipe(
        finalize(() => (this.isBusy = false)),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe(() => {
        this.isModalOpen = false;
        this.toaster.success(this.savedMessageKey);
        this.list.get();
      });
  }

  remove(item: TDto): void {
    const id = item.id;
    if (!id) {
      return;
    }
    this.confirmation
      .warn(this.deleteConfirmationMessageKey, this.deleteConfirmationTitleKey)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(status => {
        if (status !== Confirmation.Status.confirm) {
          return;
        }
        this.delete(id)
          .pipe(takeUntilDestroyed(this.destroyRef))
          .subscribe(() => {
            this.toaster.success(this.deletedMessageKey);
            this.list.get();
          });
      });
  }

  private showEdit(item: TDto): void {
    this.selected = item;
    this.form = this.buildForm(item);
    this.isModalOpen = true;
  }
}
