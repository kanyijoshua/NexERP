import { Component, DestroyRef, Input, OnChanges, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ChatterService } from '@proxy/chatter';
import { forkJoin } from 'rxjs';

interface TimelineItem {
  kind: 'note' | 'activity';
  who: string | null;
  text: string;
  time: string;
}

/**
 * Odoo-style chatter: the notes people wrote and what the system did to the record
 * (approval requests, status changes), merged into one timeline, newest first.
 */
@Component({
  selector: 'app-chatter-widget',
  template: `
    <div class="card shadow-sm border-0 chatter-card">
      <div class="card-header bg-light d-flex justify-content-between align-items-center">
        <span class="fw-bold text-secondary"
          ><i class="fas fa-comments text-primary me-2"></i
          >{{ 'Erp::ChatterTitle' | abpLocalization }}</span
        >
        <span class="badge bg-secondary">{{ timeline.length }}</span>
      </div>
      <div class="card-body">
        <div *abpPermission="'Erp.Chatter.Create'" class="input-group mb-3">
          <input
            type="text"
            class="form-control form-control-sm"
            maxlength="2000"
            [placeholder]="'Erp::WriteANote' | abpLocalization"
            [attr.aria-label]="'Erp::WriteANote' | abpLocalization"
            [(ngModel)]="newNoteText"
            (keyup.enter)="postNote()"
          />
          <button
            type="button"
            class="btn btn-sm btn-primary"
            [disabled]="!newNoteText.trim()"
            (click)="postNote()"
          >
            <i class="fas fa-paper-plane me-1"></i>{{ 'Erp::PostNote' | abpLocalization }}
          </button>
        </div>

        <div class="timeline mt-3">
          <div
            *ngFor="let item of timeline"
            class="timeline-item mb-2 p-2 rounded border-start border-3"
            [ngClass]="item.kind === 'note' ? 'bg-light border-primary' : 'border-secondary'"
          >
            <div class="d-flex justify-content-between small text-muted">
              <span class="fw-bold text-dark">
                <i
                  class="fas me-1"
                  [ngClass]="
                    item.kind === 'note'
                      ? 'fa-user-circle text-primary'
                      : 'fa-history text-secondary'
                  "
                ></i>
                {{ item.who || ('Erp::System' | abpLocalization) }}
              </span>
              <span>{{ item.time | date: 'short' }}</span>
            </div>
            <div class="mt-1 small">{{ item.text }}</div>
          </div>
          <div *ngIf="timeline.length === 0" class="text-muted small text-center py-3">
            {{ 'Erp::NoActivityYet' | abpLocalization }}
          </div>
        </div>
      </div>
    </div>
  `,
})
export class ChatterWidgetComponent implements OnChanges {
  private readonly destroyRef = inject(DestroyRef);

  @Input() entityType!: string;
  @Input() entityId!: string;
  @Input() entityNo!: string;

  timeline: TimelineItem[] = [];
  newNoteText = '';

  constructor(private readonly chatterService: ChatterService) {}

  ngOnChanges(): void {
    if (this.entityType && this.entityId) {
      this.load();
    } else {
      this.timeline = [];
    }
  }

  /** Public so a host page can refresh the log after an action that writes to it. */
  load(): void {
    const key = { entityType: this.entityType, entityId: this.entityId };

    forkJoin({
      notes: this.chatterService.getNotes(key),
      activity: this.chatterService.getActivityStream(key),
    })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(({ notes, activity }) => {
        this.timeline = [
          ...(notes.items ?? []).map<TimelineItem>(n => ({
            kind: 'note',
            who: n.authorName ?? null,
            text: n.noteText ?? '',
            time: n.creationTime ?? '',
          })),
          ...(activity.items ?? []).map<TimelineItem>(a => ({
            kind: 'activity',
            who: null,
            text: a.actionDescription ?? '',
            time: a.creationTime ?? '',
          })),
        ].sort((a, b) => b.time.localeCompare(a.time));
      });
  }

  postNote(): void {
    const noteText = this.newNoteText.trim();
    if (!noteText || !this.entityType || !this.entityId) {
      return;
    }

    this.chatterService
      .createNote({
        entityType: this.entityType,
        entityId: this.entityId,
        entityNo: this.entityNo,
        noteText,
      })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(() => {
        this.newNoteText = '';
        this.load();
      });
  }
}
