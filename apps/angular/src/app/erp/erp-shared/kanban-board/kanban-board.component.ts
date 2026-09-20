import { CdkDragDrop } from '@angular/cdk/drag-drop';
import {
  Component,
  EventEmitter,
  Input,
  Output,
  TemplateRef,
  ViewEncapsulation,
} from '@angular/core';
import { KanbanColumn, KanbanMoveEvent } from '../models';

/**
 * Odoo style kanban board (CRM pipeline, service tickets ...), built on `@angular/cdk/drag-drop`.
 *
 * A drop is applied optimistically to the `cards` arrays of the columns (in place, so the
 * parent keeps its references) and reported through `(moved)`; call `event.revert()` when the
 * server rejects the move.
 */
@Component({
  selector: 'erp-kanban-board',
  templateUrl: './kanban-board.component.html',
  styleUrls: ['./kanban-board.component.scss'],
  // The CDK drag preview lives in <body>; all styles are scoped by the erp-kanban BEM block.
  encapsulation: ViewEncapsulation.None,
})
export class KanbanBoardComponent<T = unknown> {
  @Input() columns: KanbanColumn<T>[] = [];
  /** Context: `$implicit` = card, `column` = its column. */
  @Input() cardTemplate?: TemplateRef<{ $implicit: T; column: KanbanColumn<T> }>;
  @Input() cardId: (card: T) => string = card => String((card as { id?: unknown })?.id ?? '');
  @Input() canMove = true;
  @Input() emptyKey = 'Erp::NoCards';

  @Output() moved = new EventEmitter<KanbanMoveEvent>();

  onDrop(event: CdkDragDrop<KanbanColumn<T>>): void {
    this.moveCard(
      event.previousContainer.data.id,
      event.container.data.id,
      event.previousIndex,
      event.currentIndex,
    );
  }

  /**
   * Moves a card optimistically and emits `(moved)`. Returns the emitted event, or `null` when
   * nothing moved (moving disabled, unknown column / index, or dropped where it already was).
   */
  moveCard(
    fromColumnId: string,
    toColumnId: string,
    previousIndex: number,
    index: number,
  ): KanbanMoveEvent | null {
    if (!this.canMove) {
      return null;
    }
    const from = this.columns.find(c => c.id === fromColumnId);
    const to = this.columns.find(c => c.id === toColumnId);
    if (!from || !to || previousIndex < 0 || previousIndex >= from.cards.length) {
      return null;
    }
    const targetIndex = Math.max(
      0,
      Math.min(index, from === to ? to.cards.length - 1 : to.cards.length),
    );
    if (from === to && targetIndex === previousIndex) {
      return null;
    }

    const fromSnapshot = [...from.cards];
    const toSnapshot = [...to.cards];

    const [card] = from.cards.splice(previousIndex, 1);
    to.cards.splice(targetIndex, 0, card);

    let reverted = false;
    const moveEvent: KanbanMoveEvent = {
      cardId: this.cardId(card),
      fromColumnId,
      toColumnId,
      index: targetIndex,
      revert: () => {
        if (reverted) {
          return;
        }
        reverted = true;
        from.cards.splice(0, from.cards.length, ...fromSnapshot);
        if (to !== from) {
          to.cards.splice(0, to.cards.length, ...toSnapshot);
        }
      },
    };
    this.moved.emit(moveEvent);
    return moveEvent;
  }

  toggleFold(column: KanbanColumn<T>): void {
    column.folded = !column.folded;
  }

  trackByColumn = (_index: number, column: KanbanColumn<T>): string => column.id;

  trackByCard = (_index: number, card: T): string => this.cardId(card);
}
