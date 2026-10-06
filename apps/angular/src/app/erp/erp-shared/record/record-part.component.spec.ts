import { NO_ERRORS_SCHEMA } from '@angular/core';
import { TestBed, fakeAsync, tick } from '@angular/core/testing';
import { PermissionService } from '@abp/ng.core';
import { ConfirmationService, ToasterService } from '@abp/ng.theme.shared';
import { of } from 'rxjs';
import { RecordDialogService } from './record-dialog.service';
import { RecordEntity, RecordEntityRegistry, RecordPart } from './record-entity';
import { RecordPartComponent } from './record-part.component';

describe('RecordPartComponent', () => {
  let component: RecordPartComponent;
  let lineEntity: RecordEntity;

  const header = { id: 'h1', no: 'PC-00001', status: 0 };

  beforeEach(() => {
    lineEntity = {
      key: 'testLine',
      titleKey: 'Erp::TestLine',
      pluralKey: 'Erp::TestLines',
      icon: 'fas fa-list',
      permission: 'Erp.Tests',
      columns: [
        { field: 'documentNo', labelKey: 'Erp::DocumentNo' },
        { field: 'memberName', labelKey: 'Erp::MemberName' },
        { field: 'amount', labelKey: 'Erp::Amount', type: 'currency' },
        { field: 'totalAmount', labelKey: 'Erp::TotalAmount', type: 'currency' },
      ],
      sections: [{ key: 'general', labelKey: 'Erp::General' }],
      fields: [
        { field: 'documentNo', labelKey: 'Erp::DocumentNo', type: 'lookup', lookupEntity: 'testHeader', required: true, createOnly: true },
        { field: 'memberNo', labelKey: 'Erp::MemberNo', type: 'lookup', lookupEntity: 'pensionMember', required: true },
        { field: 'amount', labelKey: 'Erp::Amount', type: 'currency', min: 0 },
      ],
      getList: () => of({ items: [], totalCount: 0 }),
      get: () => of({}),
      create: jasmine.createSpy('create').and.callFake((input: any) => of({ id: 'l1', ...input, memberName: 'Wanjiku', totalAmount: input.amount })),
      update: jasmine.createSpy('update').and.callFake((id: string, input: any) => of({ id, ...input, memberName: 'Wanjiku', totalAmount: input.amount })),
      delete: () => of(undefined),
      toItem: (dto: any) => ({ code: dto.memberNo ?? '' }),
      newRecord: () => ({ amount: 0 }),
    };

    const registry = new RecordEntityRegistry();
    registry.register(lineEntity);

    TestBed.configureTestingModule({
      declarations: [RecordPartComponent],
      providers: [
        { provide: RecordEntityRegistry, useValue: registry },
        { provide: RecordDialogService, useValue: { openCard: () => of(null) } },
        { provide: PermissionService, useValue: { getGrantedPolicy: () => true } },
        { provide: ConfirmationService, useValue: { warn: () => of('confirm') } },
        { provide: ToasterService, useValue: { success: () => undefined } },
      ],
      schemas: [NO_ERRORS_SCHEMA],
    });
    // Only the component's behaviour is under test here, not the grid it renders.
    TestBed.overrideTemplate(RecordPartComponent, '');

    component = TestBed.createComponent(RecordPartComponent).componentInstance;
    component.part = {
      entity: 'testLine',
      lines: () => of([]),
      newLine: dto => ({ documentNo: dto['no'] }),
      columns: ['memberName', 'amount', 'totalAmount'],
      totals: ['totalAmount'],
      editable: dto => dto['status'] === 0,
    } as RecordPart<any>;
    component.record = header;
    component.ngOnChanges();
  });

  it('hides the header number and shows the required fields the part did not list', () => {
    const fields = component.gridColumns.map(c => c.field);

    expect(fields).toEqual(['memberNo', 'memberName', 'amount', 'totalAmount']);
    expect(component.gridColumns.find(c => c.field === 'memberName')?.readonly).toBeTrue();
    expect(component.gridColumns.find(c => c.field === 'amount')?.readonly).toBeFalse();
  });

  it('saves a new line with the header number once its required fields are filled in', fakeAsync(() => {
    const changed = jasmine.createSpy('changed');
    component.changed.subscribe(changed);

    component.addLine();
    const row = component.rows.at(0);
    expect(row.getRawValue()['documentNo']).toBe('PC-00001');

    // Not yet: the member is missing.
    row.get('amount')!.setValue(500);
    component.onLineChange({ index: 0, field: 'amount' });
    tick();
    expect(lineEntity.create).not.toHaveBeenCalled();

    row.get('memberNo')!.setValue('M000010');
    component.onLineChange({ index: 0, field: 'memberNo' });
    tick();

    expect(lineEntity.create).toHaveBeenCalledWith(jasmine.objectContaining({ documentNo: 'PC-00001', memberNo: 'M000010', amount: 500 }));
    expect(row.get('id')!.value).toBe('l1');
    expect(row.get('memberName')!.value).toBe('Wanjiku');
    expect(component.totals[0].value).toBe(500);
    expect(changed).toHaveBeenCalled();

    // A saved line saves its changes as an update.
    row.get('amount')!.setValue(750);
    component.onLineChange({ index: 0, field: 'amount' });
    tick();
    expect(lineEntity.update).toHaveBeenCalledWith('l1', jasmine.objectContaining({ amount: 750, documentNo: 'PC-00001' }));
  }));

  it('shows the lines of a closed document read-only', () => {
    component.record = { ...header, status: 1 };
    component.ngOnChanges();

    expect(component.canCreate).toBeFalse();
    expect(component.canUpdate).toBeFalse();
    component.addLine();
    expect(component.rows.length).toBe(0);
  });
});
