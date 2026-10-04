import { Directive, ViewChild } from '@angular/core';
import { CrudListBase } from '../crud-list/crud-list.base';
import { ErpTableComponent } from './erp-table.component';
import { ErpTableAction, ErpTableColumn, ErpTableSource, ErpTableSwitchEvent } from './erp-table.models';

/**
 * Base contract for components that render an `<erp-table>`.
 */
export interface ErpTableContract<T> {
  columns: ErpTableColumn<T>[];
  actions?: ErpTableAction<T>[];
  onRowClick?(row: T): void;
  onCodeClick?(row: T): void;
  onSwitchChange?(event: ErpTableSwitchEvent<T>): void;
}

/**
 * Base class for standard ERP CRUD list components with table column configuration.
 *
 * The page's `<erp-table [source]="source">` loads, searches, filters and scrolls the rows itself;
 * this base keeps the modal form, the company switcher hook and standard Edit & Delete actions,
 * and reloads the table after a save or a delete.
 *
 * Usage:
 * ```ts
 * @Component({ ... })
 * export class MyListComponent extends ErpTableCrudBase<MyDto, CreateUpdateMyDto> {
 *   override readonly columns: ErpTableColumn<MyDto>[] = [
 *     { field: 'code', labelKey: 'Erp::Code', type: 'code' },
 *     { field: 'description', labelKey: 'Erp::Description' },
 *   ];
 *   ...
 * }
 * ```
 */
@Directive()
export abstract class ErpTableCrudBase<TDto extends { id?: string }, TCreateUpdate = any>
  extends CrudListBase<TDto, TCreateUpdate>
  implements ErpTableContract<TDto>
{
  @ViewChild(ErpTableComponent) protected table?: ErpTableComponent<TDto>;

  protected override usesListService = false;

  /** Hand this to `<erp-table [source]>`: the table's query is a superset of the page query. */
  readonly source: ErpTableSource<TDto> = query => this.getList(query as never);

  /** Column definitions for the table. Subclasses must define this. */
  abstract readonly columns: ErpTableColumn<TDto>[];

  /** Optional permission prefix for standard actions (e.g. 'Erp.NoSeries'). */
  protected permissionPrefix = '';

  /** Table actions. Initialized with default Edit and Delete actions. */
  actions: ErpTableAction<TDto>[] = [];

  override ngOnInit(): void {
    super.ngOnInit();
    if (!this.actions.length) {
      this.actions = this.buildDefaultActions();
    }
  }

  /**
   * Builds the standard Edit and Delete actions.
   * Can be overridden or extended by subclasses.
   */
  protected buildDefaultActions(): ErpTableAction<TDto>[] {
    const list: ErpTableAction<TDto>[] = [
      {
        key: 'edit',
        title: 'Erp::Edit',
        icon: 'fas fa-pen',
        btnClass: 'btn-outline-secondary',
        permission: this.permissionPrefix ? `${this.permissionPrefix}.Update` : undefined,
        action: (row, event) => {
          event?.stopPropagation();
          this.openEdit(row);
        },
      },
      {
        key: 'delete',
        title: 'Erp::Delete',
        icon: 'fas fa-trash',
        btnClass: 'btn-outline-danger',
        permission: this.permissionPrefix ? `${this.permissionPrefix}.Delete` : undefined,
        action: (row, event) => {
          event?.stopPropagation();
          this.remove(row);
        },
      },
    ];

    return list;
  }

  protected override refresh(): void {
    this.table?.reload();
  }

  /**
   * Invoked when a row is clicked. Default behavior opens the edit modal.
   */
  onRowClick(row: TDto): void {
    this.openEdit(row);
  }

  /**
   * Invoked when a code / link column is clicked. Default behavior opens the edit modal.
   */
  onCodeClick(row: TDto): void {
    this.openEdit(row);
  }

  /**
   * Invoked when a switch toggle is changed in the table.
   */
  onSwitchChange(event: ErpTableSwitchEvent<TDto>): void {
    // Subclasses can implement live updates
  }
}
