import { CoreTestingModule } from '@abp/ng.core/testing';
import { TestBed } from '@angular/core/testing';
import { RecordEntity } from '../erp-shared';
import { MasterDataEntities, codeOf } from './master-data-entities';

describe('MasterDataEntities', () => {
  let entities: RecordEntity[];

  beforeEach(() => {
    TestBed.configureTestingModule({ imports: [CoreTestingModule.withConfig()] });
    entities = TestBed.inject(MasterDataEntities).all;
  });

  it('has unique keys', () => {
    const keys = entities.map(e => e.key);
    expect(new Set(keys).size).toBe(keys.length);
  });

  it('only looks up tables that are registered', () => {
    const keys = new Set(entities.map(e => e.key));
    const lookups = entities
      .map(e => e.fields.filter(f => f.type === 'lookup'))
      .reduce((all, fields) => all.concat(fields), []);

    expect(lookups.length).toBeGreaterThan(0);
    lookups.forEach(f => expect(keys.has(f.lookupEntity!)).withContext(f.field).toBeTrue());
  });

  it('puts every field on a section of its own table', () => {
    entities.forEach(e => {
      const sections = new Set(e.sections.map(s => s.key));
      e.fields
        .filter(f => f.section)
        .forEach(f => expect(sections.has(f.section!)).withContext(`${e.key}.${f.field}`).toBeTrue());
    });
  });

  it('uses Erp localization keys everywhere', () => {
    entities.forEach(e => {
      const keys = [
        e.titleKey,
        e.pluralKey,
        ...e.sections.map(s => s.labelKey),
        ...e.fields.map(f => f.labelKey),
        ...e.columns.map(c => c.labelKey),
      ];
      keys.forEach(k => expect(k.startsWith('Erp::')).withContext(`${e.key}: ${k}`).toBeTrue());
    });
  });

  it('opens every table on its own list route', () => {
    entities.forEach(e => expect(e.listRoute?.[0]).withContext(e.key).toMatch(/^\/erp\//));
  });

  it('a quick create fills every required field the quick form shows', () => {
    entities
      .filter(e => e.quickCreate)
      .forEach(e => {
        const input = e.quickCreate!('abc') as Record<string, unknown>;
        e.fields
          .filter(f => f.required && !f.cardOnly)
          .forEach(f => expect(input[f.field]).withContext(`${e.key}.${f.field}`).toBeTruthy());
      });
  });

  it('turns typed text into a code', () => {
    expect(codeOf('  pcs ', 10)).toBe('PCS');
    expect(codeOf('a-very-long-code', 5)).toBe('A-VER');
    expect(codeOf(undefined, 5)).toBe('');
  });
});
