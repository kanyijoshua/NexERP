import { Confirmation, ConfirmationService, ToasterService } from '@abp/ng.theme.shared';
import { Component, DestroyRef, Input, OnChanges, SimpleChanges, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { RecordPictureDto, RecordPictureService } from '@proxy/attachments';
import { filter, finalize, switchMap } from 'rxjs';
import { readAsBase64 } from '../../components/attachments-widget/attachments-widget.component';

/** The server refuses anything larger; checking here spares the upload. */
export const MAX_PICTURE_BYTES = 2 * 1024 * 1024;

/** Raster formats only: SVG can carry script and the picture is shown inline. */
export const PICTURE_ACCEPT = 'image/png,image/jpeg,image/gif,image/webp';

const AVATAR_COLOURS = ['teal', 'blue', 'purple', 'emerald', 'amber', 'rose', 'indigo'];

/** Up to two initials of a name: "Lakeside Stationers" becomes "LS". */
export function initialsOf(name: string | null | undefined): string {
  const words = (name ?? '').trim().split(/\s+/).filter(Boolean);
  if (!words.length) {
    return '?';
  }
  const letters = words.length === 1 ? words[0].slice(0, 2) : words[0][0] + words[words.length - 1][0];
  return letters.toUpperCase();
}

/** A stable colour per name, so a record keeps its colour between visits. */
export function avatarColourOf(name: string | null | undefined): string {
  let hash = 0;
  for (const ch of name ?? '') {
    hash = (hash * 31 + ch.charCodeAt(0)) | 0;
  }
  return AVATAR_COLOURS[Math.abs(hash) % AVATAR_COLOURS.length];
}

/**
 * Side FactBox with a record's picture shown as an avatar: the uploaded image, or the record's
 * initials on a colour when it has none. Users with the attachments permission can upload,
 * replace or remove it.
 */
@Component({
  selector: 'erp-picture-factbox',
  standalone: false,
  styles: [
    `
      .erp-picture-avatar {
        width: 112px;
        height: 112px;
        border-radius: 50%;
        overflow: hidden;
        display: flex;
        align-items: center;
        justify-content: center;
        font-size: 2.5rem;
        font-weight: 600;
        color: #fff;
        box-shadow: 0 0 0 4px var(--bs-body-bg, #fff), 0 0 0 5px var(--bs-border-color, #dee2e6);
      }
      .erp-picture-avatar img {
        width: 100%;
        height: 100%;
        object-fit: cover;
      }
      .erp-picture-avatar.drop-target {
        outline: 2px dashed var(--bs-primary);
        outline-offset: 4px;
      }
      .avatar-teal { background: #0d9488; }
      .avatar-blue { background: #2563eb; }
      .avatar-purple { background: #7c3aed; }
      .avatar-emerald { background: #059669; }
      .avatar-amber { background: #d97706; }
      .avatar-rose { background: #e11d48; }
      .avatar-indigo { background: #4f46e5; }
    `,
  ],
  template: `
    <div class="card border-0 shadow-sm mb-3">
      <div class="card-header bg-light fw-bold text-secondary small text-uppercase d-flex align-items-center">
        <i class="fas fa-image me-2"></i>{{ titleKey | abpLocalization }}
        @if (busy()) {
          <i class="fas fa-spinner fa-spin ms-auto text-muted"></i>
        }
      </div>
      <div class="card-body d-flex flex-column align-items-center gap-3">
        <div
          class="erp-picture-avatar"
          [ngClass]="src() ? '' : 'avatar-' + colour()"
          [class.drop-target]="dragging()"
          (dragover)="onDragOver($event)"
          (dragleave)="dragging.set(false)"
          (drop)="onDrop($event)"
        >
          @if (src(); as url) {
            <img [src]="url" [alt]="name" />
          } @else {
            <span aria-hidden="true">{{ initials() }}</span>
          }
        </div>
        @if (name) {
          <div class="fw-semibold text-center text-truncate w-100" [title]="name">{{ name }}</div>
        }
        @if (editable) {
          <div class="d-flex gap-2" *abpPermission="'Erp.Attachments.Create'">
            <label class="btn btn-sm btn-outline-primary mb-0" [class.disabled]="busy()">
              <i class="fas fa-upload me-1"></i>{{ (src() ? 'Erp::ChangePicture' : 'Erp::UploadPicture') | abpLocalization }}
              <input type="file" class="d-none" [accept]="accept" [disabled]="busy()" (change)="onFileSelected($event)" />
            </label>
            @if (src()) {
              <button
                *abpPermission="'Erp.Attachments.Delete'"
                type="button"
                class="btn btn-sm btn-outline-danger"
                [disabled]="busy()"
                [title]="'Erp::RemovePicture' | abpLocalization"
                [attr.aria-label]="'Erp::RemovePicture' | abpLocalization"
                (click)="remove()"
              >
                <i class="fas fa-trash"></i>
              </button>
            }
          </div>
        }
      </div>
    </div>
  `,
})
export class ErpPictureFactBoxComponent implements OnChanges {
  private readonly service = inject(RecordPictureService);
  private readonly toaster = inject(ToasterService);
  private readonly confirmation = inject(ConfirmationService);
  private readonly destroyRef = inject(DestroyRef);

  /** The table's name in the server's entity registry, e.g. `Customer`. */
  @Input() entityType!: string;
  @Input() entityId!: string;
  /** The record's name, for the initials when there is no picture. */
  @Input() name = '';
  @Input() titleKey = 'Erp::Picture';
  /** Shows upload and remove (still subject to the attachments permissions). */
  @Input() editable = true;

  readonly accept = PICTURE_ACCEPT;
  readonly picture = signal<RecordPictureDto | null>(null);
  readonly busy = signal(false);
  readonly dragging = signal(false);
  private readonly nameSignal = signal('');

  readonly src = computed(() => {
    const p = this.picture();
    return p?.hasPicture && p.contentBase64 ? `data:${p.contentType};base64,${p.contentBase64}` : null;
  });
  readonly initials = computed(() => initialsOf(this.nameSignal()));
  readonly colour = computed(() => avatarColourOf(this.nameSignal()));

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['name']) {
      this.nameSignal.set(this.name ?? '');
    }
    // The card hands in a fresh record after every save; only another record needs a reload.
    if (!changes['entityType'] && !changes['entityId']) {
      return;
    }
    this.picture.set(null);
    if (this.entityType && this.entityId) {
      this.load();
    }
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    // Cleared so that choosing the same file again raises the change event again.
    input.value = '';
    if (file) {
      this.upload(file);
    }
  }

  onDragOver(event: DragEvent): void {
    if (!this.editable || this.busy()) {
      return;
    }
    event.preventDefault();
    this.dragging.set(true);
  }

  onDrop(event: DragEvent): void {
    event.preventDefault();
    this.dragging.set(false);
    const file = event.dataTransfer?.files?.[0];
    if (file && this.editable && !this.busy()) {
      this.upload(file);
    }
  }

  upload(file: File): void {
    if (!PICTURE_ACCEPT.split(',').includes(file.type)) {
      this.toaster.error('Erp::PictureNotAnImage');
      return;
    }
    if (file.size > MAX_PICTURE_BYTES) {
      this.toaster.error('Erp::PictureTooLarge');
      return;
    }

    this.busy.set(true);
    readAsBase64(file)
      .pipe(
        switchMap(contentBase64 =>
          this.service.upload({ entityName: this.entityType, recordId: this.entityId, contentBase64 }),
        ),
        finalize(() => this.busy.set(false)),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe(picture => {
        this.picture.set(picture);
        this.toaster.success('Erp::SavedSuccessfully');
      });
  }

  remove(): void {
    this.confirmation
      .warn('Erp::RemovePictureConfirm', 'Erp::AreYouSure')
      .pipe(
        filter(status => status === Confirmation.Status.confirm),
        switchMap(() => {
          this.busy.set(true);
          return this.service
            .delete({ entityName: this.entityType, recordId: this.entityId })
            .pipe(finalize(() => this.busy.set(false)));
        }),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe(() => this.picture.set(null));
  }

  private load(): void {
    const key = `${this.entityType}/${this.entityId}`;
    this.service
      .get({ entityName: this.entityType, recordId: this.entityId }, { skipHandleError: true })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        // A reply for a record the card has moved away from is dropped.
        next: picture => {
          if (`${this.entityType}/${this.entityId}` === key) {
            this.picture.set(picture);
          }
        },
        // No permission or no picture: the initials stand in.
        error: () => this.picture.set(null),
      });
  }
}
