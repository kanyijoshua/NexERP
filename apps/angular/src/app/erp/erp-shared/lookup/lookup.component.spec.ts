import { CoreTestingModule } from '@abp/ng.core/testing';
import { Component, ViewChild } from '@angular/core';
import { ComponentFixture, TestBed, fakeAsync, tick, waitForAsync } from '@angular/core/testing';
import { FormArray, FormControl, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { NgbTypeaheadModule } from '@ng-bootstrap/ng-bootstrap';
import { Observable, of } from 'rxjs';
import { LookupItem } from '../models';
import { LOOKUP_DEBOUNCE_MS, LookupComponent } from './lookup.component';

const ITEMS: LookupItem[] = [
  { id: 'id-10000', code: '10000', name: 'Adatum Corporation' },
  { id: 'id-20000', code: '20000', name: 'Trey Research' },
];

@Component({
  selector: 'erp-test-lookup-host',
  template: `
    <table [formGroup]="form">
      <tbody formArrayName="lines">
        <tr *ngFor="let row of lines.controls; let i = index" [formGroupName]="i">
          <td>
            <erp-lookup
              formControlName="customerNo"
              [source]="source"
              [valueField]="valueField"
              [allowFreeText]="allowFreeText"
              (selected)="selected = $event"
            ></erp-lookup>
          </td>
        </tr>
      </tbody>
    </table>
  `,
})
class TestLookupHostComponent {
  @ViewChild(LookupComponent, { static: false }) lookup!: LookupComponent;
  valueField: 'code' | 'id' = 'code';
  allowFreeText = false;
  selected: LookupItem | null | undefined;
  sourceSpy = jasmine.createSpy('source');

  lines = new FormArray([new FormGroup({ customerNo: new FormControl<string | null>(null) })]);
  form = new FormGroup({ lines: this.lines });

  source = (term: string): Observable<LookupItem[]> => {
    this.sourceSpy(term);
    return of(ITEMS.filter(item => item.code.startsWith(term)));
  };

  get control(): FormControl<string | null> {
    return this.lines.at(0).controls.customerNo;
  }
}

describe('LookupComponent', () => {
  let fixture: ComponentFixture<TestLookupHostComponent>;
  let host: TestLookupHostComponent;
  let input: HTMLInputElement;

  const type = (text: string) => {
    input.value = text;
    input.dispatchEvent(new Event('input'));
  };
  const blur = () => input.dispatchEvent(new Event('blur'));

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [LookupComponent, TestLookupHostComponent],
      imports: [CoreTestingModule.withConfig(), ReactiveFormsModule, NgbTypeaheadModule],
    }).compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(TestLookupHostComponent);
    host = fixture.componentInstance;
    fixture.detectChanges();
    input = fixture.nativeElement.querySelector('input');
  });

  it('writeValue shows the code written by the form (inside a FormArray row)', () => {
    host.control.setValue('20000');
    expect(input.value).toBe('20000');
    expect(host.lookup.value).toBe('20000');

    host.control.setValue(null);
    expect(input.value).toBe('');
    expect(host.lookup.value).toBeNull();
  });

  it('writeValue does not echo a change back to the form', () => {
    const changes: (string | null)[] = [];
    host.control.valueChanges.subscribe(v => changes.push(v));
    host.control.setValue('10000');
    expect(changes).toEqual(['10000']);
    expect(host.control.dirty).toBeFalse();
  });

  it('selecting an item writes its code, shows the code and emits the full item', () => {
    host.lookup.onSelect({ item: ITEMS[0], preventDefault: () => undefined });

    expect(host.control.value).toBe('10000');
    expect(input.value).toBe('10000');
    expect(host.selected).toEqual(ITEMS[0]);
  });

  it('writes the id when valueField is "id" but still shows the code', () => {
    host.valueField = 'id';
    fixture.detectChanges();
    host.lookup.onSelect({ item: ITEMS[1], preventDefault: () => undefined });

    expect(host.control.value).toBe('id-20000');
    expect(input.value).toBe('20000');
  });

  it('formats the dropdown as "CODE — Name" and the input as the code', () => {
    expect(host.lookup.resultFormatter(ITEMS[0])).toBe('10000 — Adatum Corporation');
    expect(host.lookup.resultFormatter({ code: 'X' })).toBe('X');
    expect(host.lookup.inputFormatter(ITEMS[0])).toBe('10000');
  });

  it('debounces typing by 250 ms before calling the source', fakeAsync(() => {
    const results: (readonly LookupItem[])[] = [];
    const text$ = new Observable<string>(subscriber => {
      subscriber.next('1');
      subscriber.next('10');
    });
    const subscription = host.lookup.search(text$).subscribe(r => results.push(r));

    tick(LOOKUP_DEBOUNCE_MS - 1);
    expect(host.sourceSpy).not.toHaveBeenCalled();
    tick(1);
    expect(host.sourceSpy).toHaveBeenCalledOnceWith('10');
    expect(results).toEqual([[ITEMS[0]]]);
    subscription.unsubscribe();
  }));

  it('writes null on blur when the typed text matches no item', () => {
    host.control.setValue('10000');
    type('nonsense');
    blur();

    expect(host.control.value).toBeNull();
    expect(input.value).toBe('');
    expect(host.selected).toBeNull();
    expect(host.control.touched).toBeTrue();
  });

  it('writes the raw text on blur when allowFreeText is true', () => {
    host.allowFreeText = true;
    fixture.detectChanges();
    type('ONE-OFF');
    blur();

    expect(host.control.value).toBe('ONE-OFF');
    expect(input.value).toBe('ONE-OFF');
  });

  it('selects the exact code match on blur (type a code and tab away)', () => {
    type('20000');
    blur();

    expect(host.control.value).toBe('20000');
    expect(host.selected).toEqual(ITEMS[1]);
  });

  it('writes null when the text is cleared', () => {
    host.control.setValue('10000');
    type('');
    blur();
    expect(host.control.value).toBeNull();
  });

  it('keeps an untouched value on blur', () => {
    host.control.setValue('99999');
    blur();
    expect(host.control.value).toBe('99999');
    expect(host.sourceSpy).not.toHaveBeenCalled();
  });

  it('supports disabling through the form control', () => {
    host.control.disable();
    fixture.detectChanges();
    expect(input.disabled).toBeTrue();

    host.control.enable();
    fixture.detectChanges();
    expect(input.disabled).toBeFalse();
  });
});
