import { Component, Input } from '@angular/core';

export type ErpBadgeTone =
  | 'success'
  | 'warning'
  | 'danger'
  | 'info'
  | 'primary'
  | 'secondary'
  | 'dark'
  | 'neutral';

@Component({
  selector: 'erp-badge',
  templateUrl: './erp-badge.component.html',
  styleUrls: ['./erp-badge.component.scss'],
  standalone: false,
})
export class ErpBadgeComponent {
  @Input() status?: string | number | boolean;
  @Input() text?: string;
  @Input() tone?: ErpBadgeTone;
  @Input() pill = true;
  @Input() dot = true;
  @Input() icon?: string;
  @Input() size: 'sm' | 'md' | 'lg' = 'md';
  @Input() badgeClass = '';

  get displayLabel(): string {
    if (this.text !== undefined && this.text !== null) {
      return this.text;
    }
    if (this.status !== undefined && this.status !== null) {
      if (typeof this.status === 'boolean') {
        return this.status ? 'Yes' : 'No';
      }
      return String(this.status);
    }
    return '';
  }

  get computedTone(): ErpBadgeTone {
    if (this.tone) {
      return this.tone;
    }
    if (this.status === undefined || this.status === null) {
      return 'neutral';
    }

    const val = String(this.status).toLowerCase().trim();

    // Success tones
    if (
      val === 'active' ||
      val === 'posted' ||
      val === 'approved' ||
      val === 'completed' ||
      val === 'released' ||
      val === 'true' ||
      val === 'paid' ||
      val === 'success' ||
      val === 'finalized'
    ) {
      return 'success';
    }

    // Warning tones
    if (
      val === 'pending' ||
      val === 'open' ||
      val === 'draft' ||
      val === 'in progress' ||
      val === 'processing' ||
      val === 'waiting'
    ) {
      return 'warning';
    }

    // Danger tones
    if (
      val === 'inactive' ||
      val === 'cancelled' ||
      val === 'rejected' ||
      val === 'failed' ||
      val === 'error' ||
      val === 'closed' ||
      val === 'blocked' ||
      val === 'false'
    ) {
      return 'danger';
    }

    // Info tones
    if (val === 'info' || val === 'scheduled' || val === 'queued' || val === 'new') {
      return 'info';
    }

    return 'neutral';
  }

  get containerClasses(): string {
    const classes = ['erp-badge', `tone-${this.computedTone}`, `size-${this.size}`];
    if (this.pill) classes.push('is-pill');
    if (this.badgeClass) classes.push(this.badgeClass);
    return classes.join(' ');
  }
}
