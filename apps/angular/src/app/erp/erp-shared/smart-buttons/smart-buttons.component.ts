import { Component, EventEmitter, Input, Output } from '@angular/core';
import { SmartButton } from '../models';

/** Stat buttons ("12 Invoices", "3 Shipments") linking to related records. */
@Component({
  selector: 'erp-smart-buttons',
  templateUrl: './smart-buttons.component.html',
  styleUrls: ['./smart-buttons.component.scss'],
  standalone: false,
})
export class SmartButtonsComponent {
  @Input() buttons: SmartButton[] = [];
  /** Emits for buttons without a `routerLink`. */
  @Output() buttonClick = new EventEmitter<SmartButton>();

  hasCount(button: SmartButton): boolean {
    return button.count !== undefined && button.count !== null && button.count !== '';
  }
}
