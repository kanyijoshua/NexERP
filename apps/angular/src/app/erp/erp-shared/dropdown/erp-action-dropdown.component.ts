import { Component, EventEmitter, inject, Input, Output } from '@angular/core';
import { PermissionService } from '@abp/ng.core';

export interface ErpActionDropdownItem {
  key: string;
  label: string;
  icon?: string;
  badge?: string;
  badgeClass?: string;
  permission?: string;
  disabled?: boolean;
  divider?: boolean;
  danger?: boolean;
  action?: () => void;
}

@Component({
  selector: 'erp-action-dropdown',
  template: `
    <div ngbDropdown class="d-inline-block erp-action-dropdown" [placement]="placement">
      <button
        type="button"
        class="btn d-inline-flex align-items-center gap-1 shadow-none"
        [ngClass]="[variantClass, sizeClass, btnClass]"
        [disabled]="disabled"
        ngbDropdownToggle
      >
        <i *ngIf="icon" [class]="icon"></i>
        <span>{{ label }}</span>
      </button>

      <div ngbDropdownMenu class="dropdown-menu shadow-sm py-1 border rounded-2">
        <ng-container *ngFor="let item of actions">
          <div *ngIf="item.divider" class="dropdown-divider my-1"></div>

          <button
            *ngIf="!item.divider && isGranted(item.permission)"
            type="button"
            class="dropdown-item d-flex align-items-center justify-content-between px-3 py-2 text-start small"
            [class.text-danger]="item.danger"
            [disabled]="item.disabled"
            (click)="handleAction(item)"
          >
            <div class="d-flex align-items-center gap-2">
              <i *ngIf="item.icon" [class]="item.icon + (item.danger ? ' text-danger' : ' text-secondary')" style="width: 16px;"></i>
              <span [class.fw-semibold]="item.danger">{{ item.label }}</span>
            </div>
            <span *ngIf="item.badge" class="badge rounded-pill ms-2" [ngClass]="item.badgeClass || 'bg-secondary'">
              {{ item.badge }}
            </span>
          </button>
        </ng-container>
      </div>
    </div>
  `,
  styles: [
    `
      .dropdown-item {
        font-size: 0.8125rem;
        cursor: pointer;
        transition: background-color 0.1s ease;
        &:hover:not([disabled]) {
          background-color: #f1f5f9;
        }
        &:active:not([disabled]) {
          background-color: #e2e8f0;
        }
      }
    `,
  ],
  standalone: false,
})
export class ErpActionDropdownComponent {
  private readonly permissions = inject(PermissionService, { optional: true });

  @Input() label = 'Actions';
  @Input() icon = 'fas fa-ellipsis-vertical';
  @Input() variant: 'primary' | 'secondary' | 'outline-primary' | 'outline-secondary' | 'outline-dark' = 'outline-secondary';
  @Input() size: 'sm' | 'md' | 'lg' = 'sm';
  @Input() btnClass = '';
  @Input() disabled = false;
  @Input() placement = 'bottom-end';
  @Input() actions: ErpActionDropdownItem[] = [];

  @Output() actionClick = new EventEmitter<ErpActionDropdownItem>();

  get variantClass(): string {
    return `btn-${this.variant}`;
  }

  get sizeClass(): string {
    return this.size === 'sm' ? 'btn-sm' : this.size === 'lg' ? 'btn-lg' : '';
  }

  isGranted(permission?: string): boolean {
    if (!permission || !this.permissions) return true;
    return this.permissions.getGrantedPolicy(permission);
  }

  handleAction(item: ErpActionDropdownItem): void {
    if (item.disabled) return;
    if (item.action) item.action();
    this.actionClick.emit(item);
  }
}
