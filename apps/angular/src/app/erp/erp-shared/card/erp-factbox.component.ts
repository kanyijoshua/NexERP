import { Component, EventEmitter, Input, Output } from '@angular/core';
import { ErpFactBoxItem, ErpFactBoxTile } from './erp-card.models';

@Component({
  selector: 'erp-factbox',
  templateUrl: './erp-factbox.component.html',
  styleUrls: ['./erp-factbox.component.scss'],
  standalone: false,
})
export class ErpFactBoxComponent {
  @Input() titleKey = '';
  @Input() icon = '';
  @Input() collapsed = false;
  @Input() tiles: ErpFactBoxTile[] = [];
  @Input() facts: ErpFactBoxItem[] = [];

  @Output() readonly collapsedChange = new EventEmitter<boolean>();
  @Output() readonly tileClick = new EventEmitter<ErpFactBoxTile>();
  @Output() readonly factClick = new EventEmitter<ErpFactBoxItem>();

  toggle(): void {
    this.collapsed = !this.collapsed;
    this.collapsedChange.emit(this.collapsed);
  }

  onTileClick(tile: ErpFactBoxTile): void {
    if (tile.action) {
      tile.action();
    }
    this.tileClick.emit(tile);
  }

  onFactClick(fact: ErpFactBoxItem): void {
    if (fact.action) {
      fact.action();
    }
    this.factClick.emit(fact);
  }
}
