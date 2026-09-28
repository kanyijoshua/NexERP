import { of } from 'rxjs';
import { RecordEntity } from './record-entity';
import { buildRecordForm, fieldsOf, visibleSections } from './record-form';

const entity: RecordEntity = {
  key: 'thing',
  titleKey: 'Erp::Thing',
  pluralKey: 'Erp::Things',
  icon: '',
  permission: 'Erp.Things',
  columns: [],
  sections: [
    { key: 'general', labelKey: 'Erp::General' },
    { key: 'posting', labelKey: 'Erp::Posting', collapsed: true },
    { key: 'empty', labelKey: 'Erp::Empty' },
  ],
  fields: [
    { field: 'no', labelKey: 'Erp::No', type: 'text', required: true, maxLength: 5, createOnly: true },
    { field: 'email', labelKey: 'Erp::Email', type: 'email' },
    { field: 'price', labelKey: 'Erp::Price', type: 'currency', min: 0 },
    { field: 'balance', labelKey: 'Erp::Balance', type: 'readonly' },
    { field: 'active', labelKey: 'Erp::Active', type: 'checkbox' },
    {
      field: 'categoryId',
      labelKey: 'Erp::Category',
      type: 'lookup',
      lookupEntity: 'category',
      lookupValueField: 'id',
      lookupDisplayField: 'categoryCode',
      section: 'posting',
      cardOnly: true,
    },
  ],
  getList: () => of({ items: [], totalCount: 0 }),
  get: () => of({}),
  create: () => of({}),
  update: () => of({}),
  delete: () => of(undefined),
  toItem: () => ({ code: '' }),
  newRecord: () => ({}),
};

describe('record form', () => {
  it('has a control for every editable field, also the ones the quick dialog hides', () => {
    const form = buildRecordForm(entity, null, { isNew: true });

    expect(Object.keys(form.controls).sort()).toEqual(
      ['active', 'categoryCode', 'categoryId', 'email', 'no', 'price'].sort(),
    );
    expect(form.get('balance')).toBeNull();
  });

  it('starts a new record from type defaults and an existing one from its values', () => {
    expect(buildRecordForm(entity, null, { isNew: true }).getRawValue()).toEqual({
      no: null,
      email: null,
      price: 0,
      active: false,
      categoryId: null,
      categoryCode: null,
    });

    const form = buildRecordForm(
      entity,
      { no: 'A1', price: 5, active: true, categoryId: 'c1', categoryCode: 'CAT' },
      { isNew: false },
    );
    expect(form.getRawValue()).toEqual(
      jasmine.objectContaining({ no: 'A1', price: 5, active: true, categoryId: 'c1', categoryCode: 'CAT' }),
    );
  });

  it('locks create-only fields on an existing record and everything without permission', () => {
    expect(buildRecordForm(entity, null, { isNew: true }).get('no')!.disabled).toBeFalse();
    expect(buildRecordForm(entity, { no: 'A1' }, { isNew: false }).get('no')!.disabled).toBeTrue();

    const readonly = buildRecordForm(entity, { no: 'A1' }, { isNew: false, readonly: true });
    expect(readonly.get('email')!.disabled).toBeTrue();
    expect(readonly.get('price')!.disabled).toBeTrue();
  });

  it('validates required, length, minimum and e-mail', () => {
    const form = buildRecordForm(entity, null, { isNew: true });
    expect(form.get('no')!.hasError('required')).toBeTrue();

    form.patchValue({ no: 'TOO-LONG', email: 'nope', price: -1 });
    expect(form.get('no')!.hasError('maxlength')).toBeTrue();
    expect(form.get('email')!.hasError('email')).toBeTrue();
    expect(form.get('price')!.hasError('min')).toBeTrue();

    form.patchValue({ no: 'A1', email: 'a@b.co', price: 0 });
    expect(form.valid).toBeTrue();
  });

  it('puts fields without a section on the first one and drops empty sections', () => {
    expect(fieldsOf(entity, 'general', false).map(f => f.field)).toEqual([
      'no',
      'email',
      'price',
      'balance',
      'active',
    ]);
    expect(visibleSections(entity, false).map(s => s.key)).toEqual(['general', 'posting']);
  });

  it('the quick form leaves out card-only and read-only fields, and sections left empty', () => {
    expect(fieldsOf(entity, 'general', true).map(f => f.field)).toEqual(['no', 'email', 'price', 'active']);
    expect(visibleSections(entity, true).map(s => s.key)).toEqual(['general']);
  });
});
