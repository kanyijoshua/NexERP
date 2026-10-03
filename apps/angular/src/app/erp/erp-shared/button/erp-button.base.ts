import { Directive, EventEmitter, inject, Input, Output } from '@angular/core';
import { PermissionService } from '@abp/ng.core';
import { Confirmation, ConfirmationService } from '@abp/ng.theme.shared';
import { ErpButtonSize, ErpButtonVariant } from './erp-button.models';

/**
 * Base abstract directive for dynamic ERP buttons.
 * Provides loading spinner state, confirmation dialog handling,
 * ABP permission validation, and styling variant resolution.
 *
 * Can be inherited by specialized button components (e.g. PostButton,
 * ApprovalButton, ExportButton, WorkflowActionButton).
 */
@Directive()
export abstract class ErpButtonBase {
  protected readonly permissions = inject(PermissionService, { optional: true });
  protected readonly confirmation = inject(ConfirmationService, { optional: true });

  @Input() variant: ErpButtonVariant = 'primary';
  @Input() size: ErpButtonSize = 'md';
  @Input() icon?: string;
  @Input() iconPosition: 'left' | 'right' = 'left';
  @Input() loading = false;
  @Input() loadingText?: string;
  @Input() disabled = false;
  @Input() confirm = false;
  @Input() confirmTitle = 'Erp::AreYouSure';
  @Input() confirmMessage = 'Erp::AreYouSureMessage';
  @Input() permission?: string;
  @Input() type: 'button' | 'submit' | 'reset' = 'button';
  @Input() btnClass = '';
  @Input() badge?: string;
  @Input() badgeClass = 'bg-secondary';

  @Output() btnClick = new EventEmitter<MouseEvent>();

  /**
   * Evaluates if the current user has the required permission policy.
   */
  get isGranted(): boolean {
    if (!this.permission || !this.permissions) return true;
    return this.permissions.getGrantedPolicy(this.permission);
  }

  /**
   * Computes the Bootstrap button CSS class string.
   */
  get buttonCssClass(): string {
    const classes = ['btn'];

    // Variant
    if (this.variant.startsWith('outline-')) {
      classes.push(`btn-${this.variant}`);
    } else if (this.variant === 'link') {
      classes.push('btn-link');
    } else {
      classes.push(`btn-${this.variant}`);
    }

    // Size
    if (this.size === 'sm') classes.push('btn-sm');
    if (this.size === 'lg') classes.push('btn-lg');

    // Custom
    if (this.btnClass) classes.push(this.btnClass);

    return classes.join(' ');
  }

  /**
   * Main click handler executing permission check, disabled check,
   * confirmation workflow, or direct emission.
   */
  handleClick(event: MouseEvent): void {
    if (this.disabled || this.loading) {
      event.preventDefault();
      event.stopPropagation();
      return;
    }

    if (!this.isGranted) {
      event.preventDefault();
      event.stopPropagation();
      return;
    }

    if (this.confirm && this.confirmation) {
      event.preventDefault();
      event.stopPropagation();
      this.confirmation
        .warn(this.confirmMessage, this.confirmTitle)
        .subscribe(status => {
          if (status === Confirmation.Status.confirm) {
            this.btnClick.emit(event);
          }
        });
      return;
    }

    this.btnClick.emit(event);
  }
}
