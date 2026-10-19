import { CoreTestingModule } from '@abp/ng.core/testing';
import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';
import { WeekGridChange, WeekGridComponent } from './week-grid.component';

describe('WeekGridComponent', () => {
  let fixture: ComponentFixture<WeekGridComponent>;
  let component: WeekGridComponent;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [WeekGridComponent],
      imports: [CoreTestingModule.withConfig()],
    }).compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(WeekGridComponent);
    component = fixture.componentInstance;
    component.startDate = '2026-03-02';
    component.rows = [
      { label: 'Design', values: [8, 8, 4, 0, 0, 0, 0] },
      { label: 'Support', values: [0, 0, 4, 8, 8, 0, 0] },
    ];
    fixture.detectChanges();
  });

  it('shows the seven days from the start date', () => {
    const days = component.days;
    expect(days.length).toBe(7);
    expect(days[0].getDate()).toBe(2);
    expect(days[6].getDate()).toBe(8);
  });

  it('totals rows, days and the week', () => {
    expect(component.rowTotal(component.rows[0])).toBe(20);
    expect(component.dayTotal(2)).toBe(8);
    expect(component.total).toBe(40);
  });

  it('clamps a typed value and emits the change', () => {
    const changes: WeekGridChange[] = [];
    component.valueChange.subscribe(c => changes.push(c));

    component.change(0, 5, '30');
    component.change(1, 6, '-2');

    expect(component.rows[0].values[5]).toBe(24);
    expect(component.rows[1].values[6]).toBe(0);
    expect(changes).toEqual([
      { rowIndex: 0, dayIndex: 5, value: 24 },
      { rowIndex: 1, dayIndex: 6, value: 0 },
    ]);
  });

  it('renders inputs only when editable', () => {
    expect(fixture.nativeElement.querySelectorAll('input').length).toBe(14);
    component.readonly = true;
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelectorAll('input').length).toBe(0);
  });
});
