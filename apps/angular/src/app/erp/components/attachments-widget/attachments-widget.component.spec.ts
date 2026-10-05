import { ConfirmationService, ToasterService } from '@abp/ng.theme.shared';
import { NO_ERRORS_SCHEMA, Pipe, PipeTransform } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { DocumentAttachmentFileType, DocumentAttachmentService } from '@proxy/attachments';
import { of, throwError } from 'rxjs';
import { SpreadsheetDialogService } from '../../erp-shared/spreadsheet/spreadsheet-dialog.service';
import { AttachmentsWidgetComponent } from './attachments-widget.component';

@Pipe({ name: 'abpLocalization', standalone: false })
class LocalizationStubPipe implements PipeTransform {
  transform(value: string): string {
    return value;
  }
}

describe('AttachmentsWidgetComponent', () => {
  let fixture: ComponentFixture<AttachmentsWidgetComponent>;
  let component: AttachmentsWidgetComponent;
  let attachmentServiceSpy: jasmine.SpyObj<DocumentAttachmentService>;
  let spreadsheetServiceSpy: jasmine.SpyObj<SpreadsheetDialogService>;
  let toasterSpy: jasmine.SpyObj<ToasterService>;

  beforeEach(async () => {
    attachmentServiceSpy = jasmine.createSpyObj('DocumentAttachmentService', ['download', 'getList', 'upload', 'update', 'delete']);
    spreadsheetServiceSpy = jasmine.createSpyObj('SpreadsheetDialogService', ['openFile']);
    toasterSpy = jasmine.createSpyObj('ToasterService', ['success', 'warn', 'error']);

    await TestBed.configureTestingModule({
      declarations: [AttachmentsWidgetComponent, LocalizationStubPipe],
      providers: [
        { provide: DocumentAttachmentService, useValue: attachmentServiceSpy },
        { provide: SpreadsheetDialogService, useValue: spreadsheetServiceSpy },
        { provide: ToasterService, useValue: toasterSpy },
        { provide: ConfirmationService, useValue: jasmine.createSpyObj('ConfirmationService', ['warn']) },
      ],
      schemas: [NO_ERRORS_SCHEMA],
    }).compileComponents();

    fixture = TestBed.createComponent(AttachmentsWidgetComponent);
    component = fixture.componentInstance;
  });

  it('identifies spreadsheet attachments properly', () => {
    expect(component.isSpreadsheet({ fileName: 'Report', fileExtension: 'xlsx' } as any)).toBeTrue();
    expect(component.isSpreadsheet({ fileName: 'Model', fileExtension: 'xlsm' } as any)).toBeTrue();
    expect(component.isSpreadsheet({ fileName: 'Rates', fileExtension: 'csv' } as any)).toBeTrue();
    expect(component.isSpreadsheet({ fileName: 'Doc', fileExtension: 'pdf' } as any)).toBeFalse();
    expect(component.isSpreadsheet({ fileName: 'Doc', fileExtension: 'docx' } as any)).toBeFalse();
  });

  it('downloads with blob responseType and opens spreadsheet viewer on openInSpreadsheet', () => {
    const dummyBlob = new Blob(['test content'], { type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet' });
    attachmentServiceSpy.download.and.returnValue(of(dummyBlob));

    const attachment = {
      id: 'att-123',
      fileName: 'SalesData',
      fileExtension: 'xlsx',
      fileType: DocumentAttachmentFileType.Excel,
    } as any;

    component.openInSpreadsheet(attachment);

    expect(attachmentServiceSpy.download).toHaveBeenCalledWith('att-123', { skipHandleError: true });
    expect(spreadsheetServiceSpy.openFile).toHaveBeenCalledWith(dummyBlob, 'SalesData.xlsx');
  });

  it('handles download error gracefully on openInSpreadsheet', () => {
    attachmentServiceSpy.download.and.returnValue(throwError(() => new Error('Network error')));

    const attachment = {
      id: 'att-456',
      fileName: 'Broken',
      fileExtension: 'xlsx',
      fileType: DocumentAttachmentFileType.Excel,
    } as any;

    component.openInSpreadsheet(attachment);

    expect(spreadsheetServiceSpy.openFile).not.toHaveBeenCalled();
    expect(toasterSpy.error).toHaveBeenCalledWith('Erp::SpreadsheetNotReadable');
  });
});
