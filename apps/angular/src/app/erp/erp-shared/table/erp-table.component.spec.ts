import { LocalizationService, PagedResultDto } from '@abp/ng.core';
import { ConfirmationService, ToasterService } from '@abp/ng.theme.shared';
import { ElementRef, SimpleChange } from '@angular/core';
import { fakeAsync, TestBed, tick } from '@angular/core/testing';
import { FormGroup } from '@angular/forms';
import { ListService } from '@abp/ng.core';
import { Observable, Subject, of } from 'rxjs';
import { CompanyService } from '../../services/company.service';
import { ErpTableCrudBase } from './erp-table.base';
import { ErpTableComponent } from './erp-table.component';
import { expression, toDynamicFilter } from './erp-table.filter';
import { ErpTableColumn, ErpTableQuery } from './erp-table.models';

interface TestItem {
  id: string;
  code: string;
  name: string;
  amount: number;
  isActive: boolean;
  date?: string;
}

const sampleItems: TestItem[] = [
  { id: '1', code: 'C01', name: 'Alpha Item', amount: 100, isActive: true, date: '2026-01-15' },
  { id: '2', code: 'C02', name: 'Beta Item', amount: 250, isActive: false, date: '2026-02-20' },
  { id: '3', code: 'C03', name: 'Gamma Item', amount: 50, isActive: true, date: '2026-03-10' },
  { id: '4', code: 'C04', name: 'Delta Item', amount: 500, isActive: false, date: '2026-04-05' },
];

const sampleColumns: ErpTableColumn<TestItem>[] = [
  { field: 'code', labelKey: 'Code', type: 'code' },
  { field: 'name', labelKey: 'Name', type: 'text' },
  { field: 'amount', labelKey: 'Amount', type: 'currency' },
  { field: 'isActive', labelKey: 'Active', type: 'switch' },
  { field: 'date', labelKey: 'Date', type: 'date' },
];

class DummyCrudComponent extends ErpTableCrudBase<TestItem, any> {
  protected override permissionPrefix = 'Test';

  override readonly columns = sampleColumns;

  protected getList(): Observable<PagedResultDto<TestItem>> {
    return of({ items: [], totalCount: 0 });
  }
  protected buildForm(): FormGroup {
    return new FormGroup({});
  }
  protected create(input: any): Observable<unknown> {
    return of(input);
  }
  protected update(_id: string, input: any): Observable<unknown> {
    return of(input);
  }
  protected delete(): Observable<unknown> {
    return of(null);
  }
}

function configure(): void {
  TestBed.configureTestingModule({
    providers: [
      { provide: ElementRef, useValue: new ElementRef(document.createElement('div')) },
      { provide: LocalizationService, useValue: { instant: (key: string) => key } },
      ListService,
      { provide: ToasterService, useValue: {} },
      { provide: ConfirmationService, useValue: {} },
      { provide: CompanyService, useValue: { companyChanged$: of() } },
    ],
  });
}

function createTable(tableId = 'spec-' + Math.random()): ErpTableComponent<TestItem> {
  const table = TestBed.runInInjectionContext(() => new ErpTableComponent<TestItem>());
  table.tableId = tableId;
  table.columns = [...sampleColumns.map(c => ({ ...c }))];
  return table;
}

function init(table: ErpTableComponent<TestItem>, items?: TestItem[]): void {
  table.items = items;
  table.ngOnInit();
  table.ngOnChanges({
    columns: new SimpleChange(null, table.columns, true),
    ...(items ? { items: new SimpleChange(null, items, true) } : {}),
  });
}

describe('ErpTableComponent', () => {
  beforeEach(() => {
    configure();
    localStorage.clear();
  });

  describe('columns', () => {
    it('fixes the first column on the left by default and keeps the actions column fixed', () => {
      const table = createTable();
      init(table, sampleItems);

      expect(table.isFrozenLeft(table.visibleColumns()[0])).toBeTrue();
      expect(table.isFrozenLeft(table.visibleColumns()[1])).toBeFalse();
      expect(table.actionsColumnFixed).toBeTrue();
    });

    it('lets the user fix, hide and reorder columns, and remembers it per table', () => {
      const table = createTable('remembered');
      init(table, sampleItems);

      const name = table.getColumnByField('name')!;
      table.setColumnPin(name, 'left');
      table.setColumnPin(table.getColumnByField('code')!, 'left'); // toggles off
      table.toggleColumnVisibility(table.getColumnByField('amount')!);
      table.moveColumn(table.getColumnByField('date')!, -1);

      expect(table.visibleColumns().map(c => c.field)).toEqual(['code', 'name', 'date', 'isActive']);

      const again = createTable('remembered');
      init(again, sampleItems);
      expect(again.getColumnByField('name')!.pinned).toBe('left');
      expect(again.getColumnByField('code')!.pinned).toBe('none');
      expect(again.getColumnByField('amount')!.visible).toBeFalse();
      expect(again.internalColumns.map(c => c.field)).toEqual(['code', 'name', 'amount', 'date', 'isActive']);

      again.resetColumnsToDefault();
      expect(again.visibleColumns().length).toBe(5);
    });

    it('never hides the last visible column', () => {
      const table = createTable();
      init(table, sampleItems);
      table.internalColumns.slice(1).forEach(c => table.toggleColumnVisibility(c));
      table.toggleColumnVisibility(table.internalColumns[0]);

      expect(table.visibleColumns().map(c => c.field)).toEqual(['code']);
    });
  });

  describe('filtering rows in memory', () => {
    it('searches across columns', fakeAsync(() => {
      const table = createTable();
      init(table, sampleItems);

      table.onSearchInput('gamma');
      tick(300);

      expect(table.rows().map(r => r.id)).toEqual(['3']);
    }));

    it('filters by typed conditions matched all or any, shown as facets', fakeAsync(() => {
      const table = createTable();
      init(table, sampleItems);

      table.addFilterCriterion('amount');
      table.filterCriteria[0].operator = 'gte';
      table.filterCriteria[0].value = 200;
      table.addFilterCriterion('isActive'); // isTrue
      table.onCriteriaChanged();
      tick(300);
      expect(table.rows().length).toBe(0);

      table.setFilterLogic('or');
      tick(300);
      // Amount ≥ 200 (2, 4) or active (1, 3).
      expect(table.rows().map(r => r.id)).toEqual(['1', '2', '3', '4']);
      expect(table.activeFilterPills.length).toBe(2);

      table.removeFilterPill(table.filterCriteria[1].id);
      expect(table.rows().map(r => r.id)).toEqual(['2', '4']);
    }));

    it('accepts filter expressions with ranges and alternatives', fakeAsync(() => {
      const table = createTable();
      init(table, sampleItems);

      table.addFilterCriterion('code');
      table.onOperatorChange(table.filterCriteria[0], 'expression');
      table.filterCriteria[0].value = 'C01..C02|C04';
      table.onCriteriaChanged();
      tick(300);

      expect(table.rows().map(r => r.id)).toEqual(['1', '2', '4']);
    }));

    it('ignores a condition until its value is filled in', fakeAsync(() => {
      const table = createTable();
      init(table, sampleItems);

      table.addFilterCriterion('name');
      tick(300);

      expect(table.rows().length).toBe(4);
      expect(table.activeCriteriaCount).toBe(0);
    }));

    it('saves the filter as a view and applies it again', fakeAsync(() => {
      const table = createTable('views');
      init(table, sampleItems);
      table.addFilterCriterion('name');
      table.filterCriteria[0].value = 'beta';
      table.newViewName = 'Betas';
      table.saveCurrentView();

      const again = createTable('views');
      init(again, sampleItems);
      expect(again.savedViews.map(v => v.name)).toEqual(['Betas']);

      again.applyView(again.savedViews[0]);
      expect(again.rows().map(r => r.id)).toEqual(['2']);
      tick(300);
    }));
  });

  describe('loading from the server with infinite scrolling', () => {
    it('sends search, conditions and sort, and appends the next page', fakeAsync(() => {
      const table = createTable();
      const queries: ErpTableQuery[] = [];
      table.pageSize = 2;
      table.source = query => {
        queries.push(query);
        return of({ items: sampleItems.slice(query.skipCount, query.skipCount + query.maxResultCount), totalCount: 4 });
      };
      init(table);
      table.ngAfterViewInit();
      tick();

      expect(table.rows().length).toBe(2);
      expect(table.hasMore()).toBeTrue();

      table.loadMore();
      tick();
      expect(table.rows().map(r => r.id)).toEqual(['1', '2', '3', '4']);
      expect(queries[1].skipCount).toBe(2);
      expect(table.hasMore()).toBeFalse();

      table.onSort({ sorts: [{ prop: 'name', dir: 'desc' }] });
      tick();
      expect(queries[2]).toEqual(jasmine.objectContaining({ skipCount: 0, sorting: 'name desc' }));

      table.addFilterCriterion('code');
      table.onOperatorChange(table.filterCriteria[0], 'expression');
      table.filterCriteria[0].value = 'C01..C03';
      table.onCriteriaChanged();
      tick(300);
      const filter = JSON.parse(queries[queries.length - 1].dynamicFilter!);
      expect(filter).toEqual({ logic: 'and', conditions: [{ field: 'code', operator: 'expression', value: 'C01..C03', valueTo: null }] });
    }));

    it('drops an answer that a newer search has overtaken', fakeAsync(() => {
      const table = createTable();
      const pending: Subject<PagedResultDto<TestItem>>[] = [];
      table.source = () => {
        const answer = new Subject<PagedResultDto<TestItem>>();
        pending.push(answer);
        return answer;
      };
      init(table);
      table.ngAfterViewInit();
      table.reload();

      pending[1].next({ items: [sampleItems[1]], totalCount: 1 });
      pending[0].next({ items: sampleItems, totalCount: 4 });
      tick();

      expect(table.rows().map(r => r.id)).toEqual(['2']);
    }));
  });
});

describe('erp-table filter helpers', () => {
  it('sends only complete conditions, under their server field name', () => {
    const json = toDynamicFilter(
      {
        searchTerm: '',
        logic: 'or',
        criteria: [
          { id: 'a', field: 'partyNo', operator: 'equals', value: 'C1' },
          { id: 'b', field: 'name', operator: 'contains', value: '' },
          { id: 'c', field: 'blocked', operator: 'isTrue' },
        ],
      },
      [{ field: 'partyNo', filterField: 'customerNo' }],
    );

    expect(JSON.parse(json!)).toEqual({
      logic: 'or',
      conditions: [
        { field: 'customerNo', operator: 'equals', value: 'C1', valueTo: null },
        { field: 'blocked', operator: 'isTrue', value: null, valueTo: null },
      ],
    });
  });

  it('reads filter expressions', () => {
    const today = new Date(2026, 8, 30);
    expect(expression('1500', '1000..2000', 'text', today)).toBeTrue();
    expect(expression('2500', '1000..2000|3000', 'text', today)).toBeFalse();
    expect(expression(5, '<>0&<10', 'number', today)).toBeTrue();
    expect(expression('Adatum Corp', 'ada*', 'text', today)).toBeTrue();
    expect(expression('', "''", 'text', today)).toBeTrue();
    expect(expression('2026-09-30', 't', 'date', today)).toBeTrue();
    expect(expression('2026-09-01', '..2026-08-31', 'date', today)).toBeFalse();
  });
});

describe('ErpTableCrudBase', () => {
  beforeEach(() => configure());

  it('builds the standard actions and opens the card on a row click', () => {
    TestBed.runInInjectionContext(() => {
      const dummy = new DummyCrudComponent();
      dummy.ngOnInit();

      expect(dummy.actions.map(a => a.key)).toEqual(['edit', 'delete']);

      spyOn(dummy, 'openEdit');
      dummy.onRowClick(sampleItems[0]);
      expect(dummy.openEdit).toHaveBeenCalledWith(sampleItems[0]);
    });
  });
});
