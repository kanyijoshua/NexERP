import { Component, Input } from '@angular/core';

@Component({
  selector: 'erp-button-group',
  template: `
    <div
      class="btn-group erp-btn-group"
      [class.btn-group-sm]="size === 'sm'"
      [class.btn-group-lg]="size === 'lg'"
      [class.btn-group-vertical]="vertical"
      role="group"
    >
      <ng-content></ng-content>
    </div>
  `,
  styles: [
    `
      :host {
        display: inline-block;
      }
    `,
  ],
  standalone: false,
})
export class ErpButtonGroupComponent {
  @Input() size: 'sm' | 'md' | 'lg' = 'md';
  @Input() vertical = false;
}
