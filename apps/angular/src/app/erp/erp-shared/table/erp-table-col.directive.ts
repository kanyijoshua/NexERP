import { Directive, Input, TemplateRef } from '@angular/core';

/**
 * Declares a custom cell template for a column in `<erp-table>`.
 *
 * Example:
 * ```html
 * <ng-template erpTableCol="nextNo" let-row="row" let-value="value">
 *   <span class="badge bg-primary">{{ value }}</span>
 * </ng-template>
 * ```
 */
@Directive({
  selector: '[erpTableCol]',
  standalone: false,
})
export class ErpTableColDirective {
  @Input('erpTableCol') colName!: string;

  constructor(public readonly templateRef: TemplateRef<any>) {}
}

/**
 * Declares custom actions column content in `<erp-table>`.
 *
 * Example:
 * ```html
 * <ng-template erpTableActions let-row="row">
 *   <button class="btn btn-sm btn-outline-primary erp-action-btn" (click)="edit(row)">
 *     <i class="fas fa-pen"></i>
 *   </button>
 * </ng-template>
 * ```
 */
@Directive({
  selector: '[erpTableActions]',
  standalone: false,
})
export class ErpTableActionsDirective {
  constructor(public readonly templateRef: TemplateRef<any>) {}
}

/**
 * Declares how one row looks when `<erp-table>` shows cards (`layout="cards"`, kanban /
 * tiles). The toolbar, filters and infinite scrolling are the grid's.
 *
 * ```html
 * <ng-template erpTableCard let-row>
 *   <div class="card h-100">{{ row.name }}</div>
 * </ng-template>
 * ```
 */
@Directive({
  selector: '[erpTableCard]',
  standalone: false,
})
export class ErpTableCardDirective {
  constructor(public readonly templateRef: TemplateRef<any>) {}
}
