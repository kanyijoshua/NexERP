import { Confirmation, ConfirmationService, ToasterService } from '@abp/ng.theme.shared';
import { Component, DestroyRef, Input, OnChanges, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { DocumentAttachmentDto, DocumentAttachmentFileType, DocumentAttachmentService } from '@proxy/attachments';
import { Observable, concatMap, finalize, from, toArray } from 'rxjs';
import { saveBlob } from '../../erp-shared/report-page/save-blob';
import { SpreadsheetDialogService } from '../../erp-shared/spreadsheet/spreadsheet-dialog.service';
import { isSpreadsheetFile } from '../../erp-shared/spreadsheet/spreadsheet-io';

/** The server refuses anything larger; checking here spares the upload. */
export const MAX_ATTACHMENT_BYTES = 10 * 1024 * 1024;

const FILE_TYPE_ICONS: Record<number, string> = {
  [DocumentAttachmentFileType.Image]: 'fa-file-image text-info',
  [DocumentAttachmentFileType.Pdf]: 'fa-file-pdf text-danger',
  [DocumentAttachmentFileType.Word]: 'fa-file-word text-primary',
  [DocumentAttachmentFileType.Excel]: 'fa-file-excel text-success',
  [DocumentAttachmentFileType.PowerPoint]: 'fa-file-powerpoint text-warning',
  [DocumentAttachmentFileType.Email]: 'fa-envelope text-secondary',
  [DocumentAttachmentFileType.Xml]: 'fa-file-code text-secondary',
};

/** Reads a file as base64, without the `data:...;base64,` prefix a data URL carries. */
export function readAsBase64(file: File): Observable<string> {
  return new Observable<string>(subscriber => {
    const reader = new FileReader();
    reader.onload = () => {
      const result = String(reader.result ?? '');
      subscriber.next(result.substring(result.indexOf(',') + 1));
      subscriber.complete();
    };
    reader.onerror = () => subscriber.error(reader.error);
    reader.readAsDataURL(file);
    return () => reader.abort();
  });
}

/**
 * The files attached to a record: attach, download, delete, and on a
 * document mark a file to follow it onto the posted document.
 */
@Component({
  selector: 'app-attachments-widget',
  templateUrl: './attachments-widget.component.html',
  standalone: false,
})
export class AttachmentsWidgetComponent implements OnChanges {
  private readonly destroyRef = inject(DestroyRef);
  private readonly service = inject(DocumentAttachmentService);
  private readonly toaster = inject(ToasterService);
  private readonly confirmation = inject(ConfirmationService);
  private readonly spreadsheets = inject(SpreadsheetDialogService);

  /** The table the record belongs to, as the server's entity registry names it, e.g. `Vendor`. */
  @Input() entityType!: string;
  @Input() entityId!: string;
  /** Shows the switch that carries a file onto the posted document: `purchase` or `sales`. */
  @Input() documentFlow: 'purchase' | 'sales' | null = null;

  readonly attachments = signal<DocumentAttachmentDto[]>([]);
  readonly busy = signal(false);

  ngOnChanges(): void {
    if (this.entityType && this.entityId) {
      this.load();
    } else {
      this.attachments.set([]);
    }
  }

  onFilesSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const files = Array.from(input.files ?? []);
    // Cleared so that choosing the same file again raises the change event again.
    input.value = '';

    const tooLarge = files.find(file => file.size > MAX_ATTACHMENT_BYTES);
    if (tooLarge) {
      this.toaster.error('Erp::AttachmentTooLarge');
      return;
    }

    if (files.length === 0) {
      return;
    }

    this.busy.set(true);
    // One at a time, so the attachment numbers follow the order the files were chosen in.
    from(files)
      .pipe(
        concatMap(file =>
          readAsBase64(file).pipe(
            concatMap(contentBase64 =>
              this.service.upload({
                entityName: this.entityType,
                recordId: this.entityId,
                fileName: file.name,
                contentType: file.type || undefined,
                contentBase64,
                documentFlowPurchase: this.documentFlow === 'purchase',
                documentFlowSales: this.documentFlow === 'sales',
              }),
            ),
          ),
        ),
        toArray(),
        finalize(() => {
          this.busy.set(false);
          this.load();
        }),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe(() => this.toaster.success('Erp::SavedSuccessfully'));
  }

  download(attachment: DocumentAttachmentDto): void {
    this.service
      .download(attachment.id!)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(blob => saveBlob(blob, this.fullName(attachment)));
  }

  isSpreadsheet(attachment: DocumentAttachmentDto): boolean {
    return isSpreadsheetFile(this.fullName(attachment));
  }

  /** Opens a workbook in the spreadsheet view instead of saving it to disk. */
  openInSpreadsheet(attachment: DocumentAttachmentDto): void {
    this.service
      .download(attachment.id!)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(blob => this.spreadsheets.openFile(blob, this.fullName(attachment)));
  }

  toggleFlow(attachment: DocumentAttachmentDto): void {
    const purchase = this.documentFlow === 'purchase' ? !attachment.documentFlowPurchase : attachment.documentFlowPurchase;
    const sales = this.documentFlow === 'sales' ? !attachment.documentFlowSales : attachment.documentFlowSales;

    this.service
      .update(attachment.id!, { fileName: attachment.fileName ?? '', documentFlowPurchase: purchase, documentFlowSales: sales })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(updated => this.attachments.update(list => list.map(a => (a.id === updated.id ? updated : a))));
  }

  remove(attachment: DocumentAttachmentDto): void {
    this.confirmation
      .warn('Erp::AttachmentWillBeDeleted', 'Erp::AreYouSure')
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(status => {
        if (status !== Confirmation.Status.confirm) {
          return;
        }

        this.service
          .delete(attachment.id!)
          .pipe(takeUntilDestroyed(this.destroyRef))
          .subscribe(() => this.attachments.update(list => list.filter(a => a.id !== attachment.id)));
      });
  }

  flows(attachment: DocumentAttachmentDto): boolean {
    return this.documentFlow === 'purchase' ? attachment.documentFlowPurchase : attachment.documentFlowSales;
  }

  fullName(attachment: DocumentAttachmentDto): string {
    return attachment.fileExtension ? `${attachment.fileName}.${attachment.fileExtension}` : (attachment.fileName ?? '');
  }

  icon(attachment: DocumentAttachmentDto): string {
    return FILE_TYPE_ICONS[attachment.fileType] ?? 'fa-file text-secondary';
  }

  size(bytes: number): string {
    if (bytes < 1024) {
      return `${bytes} B`;
    }

    return bytes < 1024 * 1024 ? `${(bytes / 1024).toFixed(1)} KB` : `${(bytes / (1024 * 1024)).toFixed(1)} MB`;
  }

  private load(): void {
    this.service
      .getList({ entityName: this.entityType, recordId: this.entityId })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(result => this.attachments.set(result.items ?? []));
  }
}
