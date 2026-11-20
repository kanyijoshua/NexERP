import { PermissionService } from '@abp/ng.core';
import { Component, EventEmitter, Input, OnChanges, Output, inject } from '@angular/core';
import { ErpActionGroup, ErpBarAction, groupActions, isPromoted } from './action-bar';

/**
 * A card's action bar: the key actions as buttons up front, then one menu per group (Process,
 * Release, Posting, Request Approval, Prepare, New Document, Print/Send, Actions).
 *
 * Content slots: `[leading]` before the promoted buttons (Save, Discard, New) and
 * `[trailing]` after the menus (Report menu, side panel toggle).
 */
@Component({
  selector: 'erp-action-bar',
  standalone: false,
  template: `
    <div class="d-flex flex-wrap align-items-center gap-2 erp-action-bar" role="toolbar">
      <ng-content select="[leading]"></ng-content>

      @for (action of promoted; track action.key) {
        <button
          type="button"
          class="btn btn-sm"
          [ngClass]="action.cssClass || 'btn-outline-primary'"
          [disabled]="busy || action.disabled"
          (click)="run.emit(action)"
        >
          <i [class]="action.icon + ' me-1'"></i>{{ action.labelKey | abpLocalization }}
        </button>
      }

      @for (group of groups; track group.category) {
        <div ngbDropdown class="d-inline-block" placement="bottom-start" container="body">
          <button type="button" class="btn btn-sm btn-outline-secondary" ngbDropdownToggle [disabled]="busy">
            <i [class]="group.icon + ' me-1'"></i>{{ group.labelKey | abpLocalization }}
          </button>
          <div ngbDropdownMenu class="shadow-sm small">
            @for (action of group.actions; track action.key) {
              @if (action.danger && !$first) {
                <div class="dropdown-divider"></div>
              }
              <button
                ngbDropdownItem
                type="button"
                [class.text-danger]="action.danger"
                [disabled]="action.disabled"
                (click)="run.emit(action)"
              >
                <i [class]="action.icon + ' fa-fw me-2'" [class.text-muted]="!action.danger"></i>{{ action.labelKey | abpLocalization }}
              </button>
            }
          </div>
        </div>
      }

      <ng-content select="[trailing]"></ng-content>
    </div>
  `,
})
export class ErpActionBarComponent implements OnChanges {
  private readonly permissions = inject(PermissionService);

  @Input() actions: readonly ErpBarAction[] = [];
  /** Most promoted buttons shown up front; the rest stay in their menus. */
  @Input() maxPromoted = 4;
  @Input() busy = false;

  @Output() readonly run = new EventEmitter<ErpBarAction>();

  promoted: ErpBarAction[] = [];
  groups: ErpActionGroup[] = [];

  ngOnChanges(): void {
    const allowed = this.actions.filter(a => !a.permission || this.permissions.getGrantedPolicy(a.permission));
    this.promoted = allowed.filter(isPromoted).slice(0, this.maxPromoted);
    this.groups = groupActions(allowed);
  }
}
