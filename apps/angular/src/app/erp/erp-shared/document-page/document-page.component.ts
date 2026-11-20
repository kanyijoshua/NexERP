import { Confirmation, ConfirmationService } from '@abp/ng.theme.shared';
import {
  Component,
  DestroyRef,
  EventEmitter,
  HostBinding,
  HostListener,
  Input,
  Output,
  inject,
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormArray, FormGroup } from '@angular/forms';
import { ErpBarAction } from '../action-bar/action-bar';
import { FactBoxVisibilityService } from '../card/factbox-visibility.service';
import { DocumentAction, DocumentLineChange, DocumentLineColumn, DocumentTotal } from '../models';

export const DEFAULT_STATUS_COLORS: Record<string, string> = {
  Draft: 'secondary',
  Open: 'secondary',
  'Pending Approval': 'warning',
  PendingApproval: 'warning',
  'Pending Prepayment': 'warning',
  Released: 'primary',
  Approved: 'primary',
  Shipped: 'info',
  Received: 'info',
  Posted: 'success',
  Paid: 'success',
  Closed: 'dark',
  Rejected: 'danger',
  Cancelled: 'danger',
  Canceled: 'danger',
};

/**
 * Layout and behaviour shell of header + lines documents (sales / purchase invoices,
 * transfer orders ...).
 *
 * Content slots: `[header]`, `[side]` (needs `[showSide]="true"`), `[smartButtons]`.
 *
 * NOTE: controls projected into `[header]` resolve their parent form from the template that
 * declares them, not from this shell. Put `[formGroup]="headerForm"` on the projected element:
 * `<div header [formGroup]="headerForm" class="row g-3"> ... </div>`.
 */
@Component({
  selector: 'erp-document-page',
  templateUrl: './document-page.component.html',
  standalone: false,
})
export class DocumentPageComponent {
  @HostBinding('attr.title') readonly hostTitle = null;

  /** Localization key or plain text (e.g. the document number). */
  @Input() title = '';
  @Input() subtitle?: string;
  @Input() status?: string | null;
  /** Status -> Bootstrap contextual colour, merged over `DEFAULT_STATUS_COLORS`. */
  @Input() statusColors: Record<string, string> = {};
  @Input() headerForm?: FormGroup;
  @Input() lines: FormArray = new FormArray<FormGroup>([]);
  @Input() lineColumns: DocumentLineColumn[] = [];
  @Input() totals: DocumentTotal[] = [];
  @Input() actions: DocumentAction[] = [];
  @Input() readonly = false;
  @Input() busy = false;
  /** Renders the `[side]` slot as a right hand column. */
  @Input() showSide = false;
  @Input() linesTitleKey = 'Erp::Lines';
  @Input() currencyCode?: string;
  /** Shows an open button per line (e.g. its item tracking). */
  @Input() openable = false;

  @Output() addLine = new EventEmitter<void>();
  @Output() removeLine = new EventEmitter<number>();
  @Output() lineChange = new EventEmitter<DocumentLineChange>();
  @Output() openLine = new EventEmitter<number>();
  /** Emits the action key, after the confirmation when `confirmKey` is set. */
  @Output() action = new EventEmitter<string>();

  private readonly confirmation = inject(ConfirmationService);
  private readonly destroyRef = inject(DestroyRef);
  private readonly factBoxVisibility = inject(FactBoxVisibilityService);

  /** The user's remembered choice to show the `[side]` FactBox pane (button or Alt+F2). */
  readonly sideVisible = this.factBoxVisibility.visible;

  /** The `[side]` pane is there and the user has not hidden it. */
  get sideShown(): boolean {
    return this.showSide && this.sideVisible();
  }

  @HostListener('document:keydown', ['$event'])
  onFactBoxShortcut(event: KeyboardEvent): void {
    if (this.showSide && FactBoxVisibilityService.isToggleShortcut(event)) {
      event.preventDefault();
      this.toggleSide();
    }
  }

  toggleSide(): void {
    this.factBoxVisibility.toggle();
  }

  get statusClass(): string {
    const status = this.status ?? '';
    const colour = this.statusColors?.[status] ?? DEFAULT_STATUS_COLORS[status] ?? 'secondary';
    return `bg-${colour}`;
  }

  get isInvalid(): boolean {
    return !!this.headerForm?.invalid || !!this.lines?.invalid;
  }

  isActionDisabled(item: DocumentAction): boolean {
    return this.busy || !!item.disabled || (!!item.requiresValid && this.isInvalid);
  }

  private barCache: { signature: string; actions: ErpBarAction[] } | null = null;

  /**
   * The actions as the action bar takes them. Rebuilt only when an action or its enabled state
   * changes, so open menus survive change detection.
   */
  get barActions(): ErpBarAction[] {
    const actions = this.actions.map(item => {
      const isSave = item.key === 'save';
      const isDelete = item.key === 'delete';
      return {
        key: item.key,
        labelKey: item.labelKey,
        icon: item.icon,
        permission: item.permission,
        cssClass: item.cssClass,
        disabled: this.isActionDisabled(item),
        category: item.category ?? (isSave || isDelete ? 'manage' : undefined),
        promoted: item.promoted ?? (isSave ? true : isDelete ? false : undefined),
        danger: isDelete,
      } as ErpBarAction;
    });
    const signature = actions.map(a => `${a.key}:${a.disabled ? 1 : 0}:${a.labelKey}`).join('|');
    if (this.barCache?.signature !== signature) {
      this.barCache = { signature, actions };
    }
    return this.barCache.actions;
  }

  runBarAction(action: ErpBarAction): void {
    const item = this.actions.find(a => a.key === action.key);
    if (item) {
      this.runAction(item);
    }
  }

  runAction(item: DocumentAction): void {
    if (this.isActionDisabled(item)) {
      return;
    }
    if (!item.confirmKey) {
      this.action.emit(item.key);
      return;
    }
    this.confirmation
      .warn(item.confirmKey, 'Erp::AreYouSure')
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(status => {
        if (status === Confirmation.Status.confirm) {
          this.action.emit(item.key);
        }
      });
  }

  trackByKey(_index: number, item: DocumentAction): string {
    return item.key;
  }
}
