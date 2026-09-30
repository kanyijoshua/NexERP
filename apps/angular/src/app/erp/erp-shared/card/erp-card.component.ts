import {
  Component,
  EventEmitter,
  HostListener,
  Input,
  Output,
} from '@angular/core';
import { ErpCardAction, ErpFactBoxGroup } from './erp-card.models';

@Component({
  selector: 'erp-card',
  templateUrl: './erp-card.component.html',
  styleUrls: ['./erp-card.component.scss'],
  standalone: false,
})
export class ErpCardComponent {
  @Input() title = '';
  @Input() subtitle = '';
  @Input() icon = '';
  @Input() breadcrumb = '';
  @Input() breadcrumbLink: any[] | null = null;
  @Input() status = '';
  @Input() statusClass = 'bg-secondary';
  @Input() busy = false;
  @Input() actions: ErpCardAction[] = [];
  @Input() factBoxGroups: ErpFactBoxGroup[] = [];
  @Input() showFactBox = true;
  @Input() showFactBoxToggle = true;
  @Input() hasFactBoxSlot = false;

  @Output() readonly showFactBoxChange = new EventEmitter<boolean>();
  @Output() readonly actionClick = new EventEmitter<ErpCardAction>();

  /**
   * Universal Business Central FactBox toggle shortcut: Alt + F2
   */
  @HostListener('window:keydown', ['$event'])
  onKeyDown(event: KeyboardEvent): void {
    if (event.altKey && event.key === 'F2') {
      event.preventDefault();
      this.toggleFactBox();
    }
  }

  toggleFactBox(): void {
    this.showFactBox = !this.showFactBox;
    this.showFactBoxChange.emit(this.showFactBox);
  }

  isActionVisible(action: ErpCardAction): boolean {
    if (typeof action.visible === 'function') {
      return action.visible();
    }
    return action.visible !== false;
  }

  isActionDisabled(action: ErpCardAction): boolean {
    if (typeof action.disabled === 'function') {
      return action.disabled();
    }
    return action.disabled === true;
  }

  runAction(action: ErpCardAction, event: MouseEvent): void {
    event.stopPropagation();
    if (!this.isActionDisabled(action)) {
      action.action(event);
      this.actionClick.emit(action);
    }
  }
}
