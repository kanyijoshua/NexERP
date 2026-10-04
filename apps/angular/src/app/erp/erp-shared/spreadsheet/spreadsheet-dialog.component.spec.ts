import { ToasterService } from '@abp/ng.theme.shared';
import { NO_ERRORS_SCHEMA, Pipe, PipeTransform } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap';
import { SpreadsheetDialogComponent } from './spreadsheet-dialog.component';

@Pipe({ name: 'abpLocalization', standalone: false })
class LocalizationStubPipe implements PipeTransform {
  transform(value: string): string {
    return value;
  }
}

describe('SpreadsheetDialogComponent', () => {
  let fixture: ComponentFixture<SpreadsheetDialogComponent>;
  let component: SpreadsheetDialogComponent;

  /** The grid's own view of the first worksheet. */
  const sheet = () => (component as any).worksheets[0];

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [SpreadsheetDialogComponent, LocalizationStubPipe],
      providers: [
        { provide: NgbActiveModal, useValue: { dismiss: () => undefined } },
        { provide: ToasterService, useValue: { warn: () => undefined, error: () => undefined } },
      ],
      schemas: [NO_ERRORS_SCHEMA],
    }).compileComponents();

    fixture = TestBed.createComponent(SpreadsheetDialogComponent);
    component = fixture.componentInstance;
    component.title = 'Budget';
    component.sheets = [
      {
        name: 'Budget',
        rows: [
          ['Account', 'Amount'],
          ['Rent', 1000],
          ['Power', 250],
          ['Total', '=SUM(B2:B3)'],
        ],
        boldRows: [0],
      },
    ];

    fixture.detectChanges();
    await component.ngAfterViewInit();
    fixture.detectChanges();
  });

  it('shows the workbook and calculates its formulas', () => {
    expect(component.failed()).toBeFalse();
    expect(component.loading()).toBeFalse();
    expect(sheet().getValueFromCoords(1, 3, false)).toBe('=SUM(B2:B3)');
    expect(String(sheet().getValueFromCoords(1, 3, true))).toBe('1250');
  });

  it('recalculates when a cell a formula reads is changed', () => {
    sheet().setValueFromCoords(1, 1, 2000);

    expect(String(sheet().getValueFromCoords(1, 3, true))).toBe('2250');
  });

  it('writes what is typed in the formula bar into the selected cell', () => {
    (component as any).selection = { x: 2, y: 1, x2: 2, y2: 1 };

    component.applyContent('=B2*2');

    expect(sheet().getValueFromCoords(2, 1, false)).toBe('=B2*2');
    expect(String(sheet().getValueFromCoords(2, 1, true))).toBe('2000');
  });

  it('sums the selected cells into the first empty cell below them', () => {
    (component as any).selection = { x: 1, y: 1, x2: 1, y2: 2 };

    component.autoSum();

    // Row 4 already holds the total, so the sum lands on the first empty row after it.
    expect(sheet().getValueFromCoords(1, 4, false)).toBe('=SUM(B2:B3)');
  });

  it('keeps formulas and bold rows in what it saves', () => {
    const snapshot = (component as any).snapshot();

    expect(snapshot[0].name).toBe('Budget');
    expect(snapshot[0].rows[3].slice(0, 2)).toEqual(['Total', '=SUM(B2:B3)']);
    expect(snapshot[0].boldRows).toContain(0);
  });
});
