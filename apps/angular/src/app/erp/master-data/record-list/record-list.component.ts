import { ABP, ListService, PagedResultDto } from '@abp/ng.core';
import { Confirmation, ConfirmationService, ToasterService } from '@abp/ng.theme.shared';
import { Component, DestroyRef, OnInit, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ActivatedRoute, Router } from '@angular/router';
import { Subject } from 'rxjs';
import { debounceTime, distinctUntilChanged } from 'rxjs/operators';
import {
  CRUD_FILTER_DEBOUNCE_MS,
  RecordColumn,
  RecordDialogService,
  RecordEntity,
  RecordEntityRegistry,
} from '../../erp-shared';
import { CompanyService } from '../../services/company.service';

/**
 * The list page of a master-data table (BC list page, Odoo list view), driven by the route's
 * `entity`. A row opens the card page; the pencil edits it in a dialog without leaving the list.
 */
@Component({
  selector: 'app-record-list',
  templateUrl: './record-list.component.html',
  providers: [ListService],
  standalone: false,
})
export class RecordListComponent implements OnInit {
  readonly list = inject<ListService<ABP.PageQueryParams>>(ListService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly registry = inject(RecordEntityRegistry);
  private readonly dialogs = inject(RecordDialogService);
  private readonly toaster = inject(ToasterService);
  private readonly confirmation = inject(ConfirmationService);
  private readonly companyService = inject(CompanyService);
  private readonly destroyRef = inject(DestroyRef);

  entity!: RecordEntity;
  data: PagedResultDto<Record<string, any>> = { items: [], totalCount: 0 };
  filter = '';

  private readonly filter$ = new Subject<string>();

  ngOnInit(): void {
    this.entity = this.registry.get(this.route.snapshot.data['entity']);

    this.list
      .hookToQuery(query => this.entity.getList(query as never))
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(result => (this.data = result));

    this.filter$
      .pipe(
        debounceTime(CRUD_FILTER_DEBOUNCE_MS),
        distinctUntilChanged(),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe(value => {
        this.list.page = 0;
        this.list.filter = value;
      });

    this.companyService.companyChanged$.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(() => {
      this.list.page = 0;
      this.list.get();
    });
  }

  onFilterChange(value: string): void {
    this.filter = value ?? '';
    this.filter$.next(this.filter);
  }

  cardLink(row: Record<string, any>): string[] {
    return [...(this.entity.listRoute ?? []), row['id']];
  }

  openNew(): void {
    this.router.navigate([...(this.entity.listRoute ?? []), 'new']);
  }

  onActivate(event: { type: string; row: Record<string, any> }): void {
    if (event.type === 'click') {
      this.router.navigate(this.cardLink(event.row));
    }
  }

  quickEdit(row: Record<string, any>, event: Event): void {
    event.stopPropagation();
    this.dialogs
      .openCard(this.entity, { id: row['id'], quick: false })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(result => result && this.list.get());
  }

  remove(row: Record<string, any>, event: Event): void {
    event.stopPropagation();
    this.confirmation
      .warn('Erp::ItemWillBeDeletedMessage', 'Erp::AreYouSure')
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(status => {
        if (status !== Confirmation.Status.confirm) {
          return;
        }
        this.entity
          .delete(row['id'])
          .pipe(takeUntilDestroyed(this.destroyRef))
          .subscribe(() => {
            this.toaster.success('Erp::DeletedSuccessfully');
            this.list.get();
          });
      });
  }

  optionLabel(row: Record<string, any>, column: RecordColumn): string {
    const value = row[column.field];
    return column.options?.find(o => o.value === value)?.label ?? String(value ?? '');
  }
}
