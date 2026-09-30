import { Component, EventEmitter, Input, Output } from '@angular/core';

@Component({
  selector: 'erp-fasttab',
  templateUrl: './erp-fasttab.component.html',
  styleUrls: ['./erp-fasttab.component.scss'],
  standalone: false,
})
export class ErpFastTabComponent {
  @Input() titleKey = '';
  @Input() icon = '';
  @Input() collapsed = false;
  @Input() summary = '';
  @Input() badge: string | number | null = null;
  @Input() badgeClass = 'bg-light text-secondary border';

  @Output() readonly collapsedChange = new EventEmitter<boolean>();

  toggle(): void {
    this.collapsed = !this.collapsed;
    this.collapsedChange.emit(this.collapsed);
  }
}
