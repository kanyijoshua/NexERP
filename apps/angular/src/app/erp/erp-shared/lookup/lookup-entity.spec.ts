import { PermissionService } from '@abp/ng.core';
import { CoreTestingModule } from '@abp/ng.core/testing';
import { Component, ViewChild } from '@angular/core';
import { ComponentFixture, TestBed, fakeAsync, flush, tick, waitForAsync } from '@angular/core/testing';
import { FormControl, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { NgbTypeaheadModule } from '@ng-bootstrap/ng-bootstrap';
import { NgxValidateCoreModule } from '@ngx-validate/core';
import { Observable, of } from 'rxjs';
import { LookupItem } from '../models';
import { RecordDialogService } from '../record/record-dialog.service';
import { RecordEntity, RecordEntityRegistry } from '../record/record-entity';
import { LOOKUP_DEBOUNCE_MS, LookupComponent, LookupOption } from './lookup.component';

interface Uom {
  id: string;
  code: string;
  description: string;
}

const UNITS: Uom[] = Array.from({ length: 12 }, (_, i) => ({
  id: `id-${i}`,
  code: `U${i}`,
  description: `Unit ${i}`,
}));

function unitEntity(overrides: Partial<RecordEntity<Uom>> = {}): RecordEntity<Uom> {
  return {
    key: 'uom',
    titleKey: 'Erp::UnitOfMeasure',
    pluralKey: 'Erp::UnitsOfMeasure',
    icon: 'fas fa-ruler',
    permission: 'Erp.UnitsOfMeasure',
    columns: [],
    fields: [],
    sections: [],
    getList: query => {
      const items = UNITS.filter(u => u.code.startsWith((query.filter ?? '').toUpperCase()));
      return of({ items: items.slice(0, query.maxResultCount), totalCount: items.length });
    },
    get: id => of(UNITS.find(u => u.id === id)!),
    create: jasmine
      .createSpy('create')
      .and.callFake((input: Partial<Uom>) => of({ id: 'new-id', code: input.code!, description: '' })),
    update: (_id, input) => of(input),
    delete: () => of(undefined),
    toItem: dto => ({ id: dto.id, code: dto.code, name: dto.description }),
    newRecord: term => ({ code: term ?? '' }),
    quickCreate: term => ({ code: term.toUpperCase() }),
    ...overrides,
  };
}

@Component({
    selector: 'erp-test-entity-lookup-host',
    template: `<div [formGroup]="form">
    <erp-lookup
      [formControl]="control"
      [entity]="entityKey"
      [valueField]="valueField"
      (selected)="selected = $event"
    ></erp-lookup>
  </div>`,
    standalone: false
})
class HostComponent {
  @ViewChild(LookupComponent, { static: true }) lookup!: LookupComponent;
  control = new FormControl<string | null>(null);
  // ngx-validate (exported by the ABP core module) needs a parent form group.
  form = new FormGroup({ control: this.control });
  entityKey: string | null = 'uom';
  valueField: 'code' | 'id' = 'code';
  selected: LookupItem | null | undefined;
}

describe('LookupComponent with a record entity', () => {
  let fixture: ComponentFixture<HostComponent>;
  let host: HostComponent;
  let entity: RecordEntity<Uom>;
  let dialogs: jasmine.SpyObj<RecordDialogService>;
  let canCreate: boolean;

  const results = (term: string): LookupOption[] => {
    let options: readonly LookupOption[] = [];
    const subscription = host.lookup
      .search(new Observable<string>(s => s.next(term)))
      .subscribe(r => (options = r));
    tick(LOOKUP_DEBOUNCE_MS);
    subscription.unsubscribe();
    return [...options];
  };

  beforeEach(waitForAsync(() => {
    dialogs = jasmine.createSpyObj<RecordDialogService>('RecordDialogService', [
      'openList',
      'openCard',
    ]);
    canCreate = true;

    TestBed.configureTestingModule({
      declarations: [LookupComponent, HostComponent],
      imports: [
        CoreTestingModule.withConfig(),
        ReactiveFormsModule,
        NgbTypeaheadModule,
        NgxValidateCoreModule.forRoot(),
      ],
      providers: [{ provide: RecordDialogService, useValue: dialogs }],
    }).compileComponents();
  }));

  beforeEach(() => {
    entity = unitEntity();
    TestBed.inject(RecordEntityRegistry).register(entity);
    spyOn(TestBed.inject(PermissionService), 'getGrantedPolicy').and.callFake(() => canCreate);

    fixture = TestBed.createComponent(HostComponent);
    host = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('cuts the dropdown to the limit and ends it with Search More, Create and Create and edit', fakeAsync(() => {
    const options = results('u');

    expect(options.filter(o => !o.action).length).toBe(8);
    expect(options.slice(8).map(o => o.action)).toEqual(['searchMore', 'create', 'createEdit']);
    expect(options.slice(8).every(o => o.term === 'u')).toBeTrue();
    // The action gives the input its focus back, which leaves a timer of the typeahead behind.
    flush();
  }));

  it('offers no one-click create for a code that already exists, nor without the permission', fakeAsync(() => {
    expect(results('U3').map(o => o.action)).toEqual([undefined, 'searchMore', 'createEdit']);

    canCreate = false;
    expect(results('X').map(o => o.action)).toEqual(['searchMore']);
    // The action gives the input its focus back, which leaves a timer of the typeahead behind.
    flush();
  }));

  it('offers "Create and edit" only when the entity has no quick create', fakeAsync(() => {
    entity.quickCreate = undefined;
    expect(results('X').map(o => o.action)).toEqual(['searchMore', 'createEdit']);
    // The action gives the input its focus back, which leaves a timer of the typeahead behind.
    flush();
  }));

  it('a plain lookup without an entity keeps its source results unchanged', fakeAsync(() => {
    host.entityKey = null;
    fixture.detectChanges();
    expect(results('u')).toEqual([]);
    // The action gives the input its focus back, which leaves a timer of the typeahead behind.
    flush();
  }));

  it('"Create \'term\'" creates the record and selects it', fakeAsync(() => {
    const prevent = jasmine.createSpy('preventDefault');
    host.lookup.onSelect({ item: { code: '', action: 'create', term: 'box' }, preventDefault: prevent });

    expect(prevent).toHaveBeenCalled();
    expect(entity.create).toHaveBeenCalledOnceWith({ code: 'BOX' });
    expect(host.control.value).toBe('BOX');
    expect(host.selected?.data).toEqual({ id: 'new-id', code: 'BOX', description: '' });
    // The action gives the input its focus back, which leaves a timer of the typeahead behind.
    flush();
  }));

  it('"Search More" opens the full list on the typed term and selects the pick', fakeAsync(() => {
    dialogs.openList.and.returnValue(of(UNITS[5]));
    host.lookup.onSelect({ item: { code: '', action: 'searchMore', term: 'U' }, preventDefault: () => undefined });

    expect(dialogs.openList).toHaveBeenCalledOnceWith(entity, { filter: 'U', selectable: true });
    expect(host.control.value).toBe('U5');
    expect(host.selected?.name).toBe('Unit 5');
    // The action gives the input its focus back, which leaves a timer of the typeahead behind.
    flush();
  }));

  it('a cancelled list leaves the value alone', fakeAsync(() => {
    host.control.setValue('U1');
    dialogs.openList.and.returnValue(of(null));
    host.lookup.openList();
    expect(host.control.value).toBe('U1');
    // The action gives the input its focus back, which leaves a timer of the typeahead behind.
    flush();
  }));

  it('"Create and edit" opens a quick card with the term and selects the saved record', fakeAsync(() => {
    dialogs.openCard.and.returnValue(of({ record: { id: 'x', code: 'PALLET', description: 'Pallet' } }));
    host.lookup.onSelect({ item: { code: '', action: 'createEdit', term: 'pallet' }, preventDefault: () => undefined });

    expect(dialogs.openCard).toHaveBeenCalledOnceWith(entity, { term: 'pallet', quick: true });
    expect(host.control.value).toBe('PALLET');
    // The action gives the input its focus back, which leaves a timer of the typeahead behind.
    flush();
  }));

  it('opens the card of a code value by looking the code up', fakeAsync(() => {
    host.control.setValue('U7');
    dialogs.openCard.and.returnValue(of(null));
    host.lookup.openCard();

    expect(dialogs.openCard).toHaveBeenCalledOnceWith(entity, { id: 'id-7', quick: false });
    // The action gives the input its focus back, which leaves a timer of the typeahead behind.
    flush();
  }));

  it('an edited record refreshes the input without re-selecting; a renamed code is written back', fakeAsync(() => {
    host.control.setValue('U7');
    const changes: (string | null)[] = [];
    host.control.valueChanges.subscribe(v => changes.push(v));

    dialogs.openCard.and.returnValue(of({ record: { id: 'id-7', code: 'U7', description: 'Renamed' } }));
    host.lookup.openCard();
    expect(changes).toEqual([]);
    expect(host.selected).toBeUndefined();

    dialogs.openCard.and.returnValue(of({ record: { id: 'id-7', code: 'U77', description: 'x' } }));
    host.lookup.openCard();
    expect(changes).toEqual(['U77']);
    // The action gives the input its focus back, which leaves a timer of the typeahead behind.
    flush();
  }));

  it('clears the value when the record is deleted from its card', fakeAsync(() => {
    host.control.setValue('U7');
    dialogs.openCard.and.returnValue(of({ record: UNITS[7], deleted: true }));
    host.lookup.openCard();

    expect(host.control.value).toBeNull();
    expect(host.selected).toBeNull();
    // The action gives the input its focus back, which leaves a timer of the typeahead behind.
    flush();
  }));

  it('shows the code of an id value by reading the record', () => {
    host.valueField = 'id';
    fixture.detectChanges();
    host.control.setValue('id-3');

    expect(fixture.nativeElement.querySelector('input').value).toBe('U3');
  });

  it('renders the full-list button always and the open-card button once there is a value', () => {
    const buttons = () => fixture.nativeElement.querySelectorAll('button').length;
    expect(buttons()).toBe(1);

    host.control.setValue('U1');
    fixture.detectChanges();
    expect(buttons()).toBe(2);
  });
});
