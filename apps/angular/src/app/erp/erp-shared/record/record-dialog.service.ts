import { Injectable, Injector, inject } from '@angular/core';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { Observable, from, of } from 'rxjs';
import { catchError, map } from 'rxjs/operators';
import {
  OpenCardOptions,
  RecordCardResult,
  openRecordCardDialog,
} from './record-card-dialog.component';
import { RecordEntity } from './record-entity';
import { RecordListDialogComponent } from './record-list-dialog.component';

export interface OpenListOptions {
  /** Starting search text. */
  filter?: string;
  /** Shows a "Select" button and closes with the picked record (BC "Select from full list"). */
  selectable?: boolean;
}

/**
 * Opens the generic record dialogs on top of whatever page is showing: the full list ("Search More...",
 * BC's "Select from full list") and the record card (view, edit, create, delete). The dialogs stack, so
 * a lookup inside a card dialog opens another dialog above it.
 */
@Injectable({ providedIn: 'root' })
export class RecordDialogService {
  private readonly modal = inject(NgbModal);
  private readonly injector = inject(Injector);

  /** Emits the picked record, or `null` when the list was closed without a pick. */
  openList<TDto>(entity: RecordEntity, options: OpenListOptions = {}): Observable<TDto | null> {
    const ref = this.modal.open(RecordListDialogComponent, {
      size: 'xl',
      scrollable: true,
      injector: this.injector,
    });
    const list = ref.componentInstance as RecordListDialogComponent;
    list.entity = entity;
    list.selectable = !!options.selectable;
    list.initialFilter = options.filter ?? '';
    return from(ref.result as Promise<TDto>).pipe(
      map(result => result ?? null),
      catchError(() => of(null)),
    );
  }

  /** Emits the saved (or deleted) record, or `null` when the dialog was cancelled. */
  openCard<TDto>(
    entity: RecordEntity,
    options: OpenCardOptions = {},
  ): Observable<RecordCardResult<TDto> | null> {
    return from(openRecordCardDialog<TDto>(this.modal, this.injector, entity, options));
  }
}
