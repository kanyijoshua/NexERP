import { Component, EventEmitter, Input, Output } from '@angular/core';
import { ErpButtonBase } from './erp-button.base';
import { ErpButtonAction } from './erp-button.models';

@Component({
  selector: 'erp-split-button',
  template: `
    <div *ngIf="isGranted" class="btn-group erp-split-btn-group" role="group">
      <!-- Main Action Button -->
      <button
        [type]="type"
        class="btn d-inline-flex align-items-center gap-2"
        [ngClass]="buttonCssClass"
        [disabled]="disabled || loading"
        (click)="handleClick($event)"
      >
        <span
          *ngIf="loading"
          class="spinner-border spinner-border-sm"
          role="status"
          aria-hidden="true"
        ></span>
        <i *ngIf="!loading && icon && iconPosition === 'left'" [class]="icon"></i>
        <span>
          <ng-container *ngIf="loading && loadingText; else defaultContent">
            {{ loadingText }}
          </ng-container>
          <ng-template #defaultContent>
            <ng-content></ng-content>
          </ng-template>
        </span>
        <i *ngIf="!loading && icon && iconPosition === 'right'" [class]="icon"></i>
      </button>

      <!-- Dropdown Toggle Button -->
      <div ngbDropdown class="btn-group" [placement]="placement" role="group">
        <button
          type="button"
          class="btn dropdown-toggle dropdown-toggle-split shadow-none"
          [ngClass]="buttonCssClass"
          [disabled]="disabled || loading"
          ngbDropdownToggle
          aria-expanded="false"
        >
          <span class="visually-hidden">Toggle Dropdown</span>
        </button>

        <div ngbDropdownMenu class="dropdown-menu shadow-sm py-1 border rounded-2">
          <ng-container *ngFor="let item of actions">
            <div *ngIf="item.divider" class="dropdown-divider my-1"></div>

            <button
              *ngIf="!item.divider && isActionGranted(item.permission)"
              type="button"
              class="dropdown-item d-flex align-items-center gap-2 px-3 py-2 small"
              [class.text-danger]="item.danger"
              [disabled]="item.disabled"
              (click)="onActionItemClick(item, $event)"
            >
              <i *ngIf="item.icon" [class]="item.icon + (item.danger ? ' text-danger' : ' text-secondary')" style="width: 16px;"></i>
              <span>{{ item.label }}</span>
            </button>
          </ng-container>
        </div>
      </div>
    </div>
  `,
  styles: [
    `
      :host {
        display: inline-block;
      }
      .dropdown-item {
        font-size: 0.8125rem;
        cursor: pointer;
        transition: background-color 0.1s ease;
        &:hover:not([disabled]) {
          background-color: #f1f5f9;
        }
      }
    `,
  ],
  standalone: false,
})
export class ErpSplitButtonComponent extends ErpButtonBase {
  @Input() actions: ErpButtonAction[] = [];
  @Input() placement = 'bottom-end';

  @Output() actionClick = new EventEmitter<ErpButtonAction>();

  isActionGranted(permission?: string): boolean {
    if (!permission || !this.permissions) return true;
    return this.permissions.getGrantedPolicy(permission);
  }

  onActionItemClick(item: ErpButtonAction, event: MouseEvent): void {
    if (item.disabled) return;
    item.action(event);
    this.actionClick.emit(item);
  }
}
