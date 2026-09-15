import { Component, Input, OnChanges } from '@angular/core';
import { ChatterService, DocumentNoteDto } from '../../services/chatter.service';

@Component({
  selector: 'app-chatter-widget',
  template: `
    <div class="card shadow-sm border-0 chatter-card">
      <div class="card-header bg-light d-flex justify-content-between align-items-center">
        <span class="fw-bold text-secondary"><i class="fas fa-comments text-primary me-2"></i>Odoo Chatter & Activity Log</span>
        <span class="badge bg-secondary">{{ notes.length }} Notes</span>
      </div>
      <div class="card-body">
        <div class="input-group mb-3">
          <input
            type="text"
            class="form-control form-control-sm"
            placeholder="Send a message or internal note..."
            [(ngModel)]="newNoteText"
            (keyup.enter)="postNote()"
          />
          <button class="btn btn-sm btn-primary" (click)="postNote()">
            <i class="fas fa-paper-plane me-1"></i> Post Note
          </button>
        </div>

        <div class="timeline mt-3">
          <div *ngFor="let note of notes" class="timeline-item mb-3 p-2 rounded bg-light border-start border-3 border-primary">
            <div class="d-flex justify-content-between small text-muted">
              <span class="fw-bold text-dark"><i class="fas fa-user-circle text-primary me-1"></i> {{ note.authorName }}</span>
              <span>{{ note.creationTime | date: 'short' }}</span>
            </div>
            <div class="mt-1 small">{{ note.noteText }}</div>
          </div>
          <div *if="notes.length === 0" class="text-muted small text-center py-3">No activity logs or internal notes yet.</div>
        </div>
      </div>
    </div>
  `,
  styles: [
    `
      .chatter-card { border-radius: 8px; }
      .timeline-item { background-color: #f8f9fa; }
    `,
  ],
})
export class ChatterWidgetComponent implements OnChanges {
  @Input() entityType!: string;
  @Input() entityId!: string;
  @Input() entityNo!: string;

  notes: DocumentNoteDto[] = [];
  newNoteText = '';

  constructor(private chatterService: ChatterService) {}

  ngOnChanges(): void {
    if (this.entityType && this.entityId) {
      this.loadNotes();
    }
  }

  loadNotes(): void {
    this.chatterService.getNotes(this.entityType, this.entityId).subscribe(data => {
      this.notes = data;
    });
  }

  postNote(): void {
    if (!this.newNoteText.trim() || !this.entityType || !this.entityId) return;
    this.chatterService.addNote(this.entityType, this.entityId, this.entityNo, this.newNoteText).subscribe(() => {
      this.newNoteText = '';
      this.loadNotes();
    });
  }
}
