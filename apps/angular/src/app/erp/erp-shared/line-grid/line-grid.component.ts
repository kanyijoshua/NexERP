import {
  AfterViewInit,
  Component,
  DestroyRef,
  ElementRef,
  EventEmitter,
  Input,
  Output,
  QueryList,
  ViewChildren,
  inject,
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormArray, FormGroup } from '@angular/forms';
import { DocumentLineChange, DocumentLineColumn, LookupItem } from '../models';
import { ErpAmountPipe } from '../pipes/erp-amount.pipe';

/**
 * Editable table over a `FormArray` of `FormGroup`s, driven by `DocumentLineColumn`s.
 * Shared by `erp-document-page` and `erp-journal-page`, usable on its own too.
 *
 * Keyboard: Enter on the last row asks for a new line (and focuses it once the parent
 * pushed it into the FormArray); Ctrl+Delete removes the current line.
 */
@Component({
  selector: 'erp-line-grid',
  templateUrl: './line-grid.component.html',
  styleUrls: ['./line-grid.component.scss'],
})
export class LineGridComponent implements AfterViewInit {
  @Input() lines: FormArray = new FormArray<FormGroup>([]);
  @Input() columns: DocumentLineColumn[] = [];
  @Input() readonly = false;
  @Input() emptyKey = 'Erp::NoLines';

  @Output() addLine = new EventEmitter<void>();
  @Output() removeLine = new EventEmitter<number>();
  @Output() lineChange = new EventEmitter<DocumentLineChange>();

  @ViewChildren('rowEl') rowElements!: QueryList<ElementRef<HTMLTableRowElement>>;

  private focusNewRow = false;
  private readonly amountPipe = new ErpAmountPipe();
  private readonly destroyRef = inject(DestroyRef);

  get rows(): FormGroup[] {
    return (this.lines?.controls ?? []) as FormGroup[];
  }

  ngAfterViewInit(): void {
    this.rowElements.changes.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(() => {
      if (!this.focusNewRow) {
        return;
      }
      this.focusNewRow = false;
      const last = this.rowElements.last?.nativeElement;
      const target = last?.querySelector<HTMLElement>(
        'input:not([disabled]), select:not([disabled])',
      );
      // After the view settled, otherwise the typeahead of a lookup cell opens mid change detection.
      setTimeout(() => target?.focus());
    });
  }

  requestAddLine(): void {
    if (this.readonly) {
      return;
    }
    this.focusNewRow = true;
    this.addLine.emit();
  }

  onRowKeydown(event: KeyboardEvent, index: number): void {
    if (this.readonly) {
      return;
    }
    if (event.key === 'Enter' && !event.defaultPrevented && index === this.rows.length - 1) {
      // defaultPrevented: the typeahead popup consumed Enter to pick an item.
      event.preventDefault();
      this.requestAddLine();
    } else if (event.key === 'Delete' && event.ctrlKey) {
      event.preventDefault();
      this.removeLine.emit(index);
    }
  }

  onCellChange(index: number, field: string): void {
    this.lineChange.emit({ index, field });
  }

  onLookupSelected(index: number, field: string, item: LookupItem | null): void {
    this.lineChange.emit({ index, field, item });
  }

  isNumeric(column: DocumentLineColumn): boolean {
    return column.type === 'number' || column.type === 'currency';
  }

  /** Text of a cell in read-only rendering (select cells return their option label). */
  displayText(row: FormGroup, column: DocumentLineColumn): string {
    const value: unknown = row.get(column.field)?.value;
    if (value === null || value === undefined) {
      return '';
    }
    if (column.type === 'select') {
      const option = column.options?.find(o => o.value === value);
      return option ? option.label : String(value);
    }
    if (typeof value === 'number' && column.type !== 'number') {
      return this.amountPipe.transform(value);
    }
    return String(value);
  }

  trackByField(_index: number, column: DocumentLineColumn): string {
    return column.field;
  }
}
