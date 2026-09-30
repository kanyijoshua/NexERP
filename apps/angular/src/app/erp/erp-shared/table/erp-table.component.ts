import { LocalizationService, PagedResultDto } from '@abp/ng.core';
import { CdkDragDrop, moveItemInArray } from '@angular/cdk/drag-drop';
import {
  AfterContentInit,
  AfterViewInit,
  Component,
  ContentChild,
  ContentChildren,
  DestroyRef,
  ElementRef,
  EventEmitter,
  Input,
  OnChanges,
  OnInit,
  Output,
  QueryList,
  SimpleChanges,
  TemplateRef,
  computed,
  inject,
  signal,
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Subject, Subscription } from 'rxjs';
import { debounceTime } from 'rxjs/operators';
import { ErpTableActionsDirective, ErpTableCardDirective, ErpTableColDirective } from './erp-table-col.directive';
import {
  defaultOperatorFor,
  isComplete,
  isNoValueOperator,
  matchesState,
  operatorsFor,
  toDynamicFilter,
} from './erp-table.filter';
import {
  ErpColumnPin,
  ErpFilterCriterion,
  ErpFilterLogic,
  ErpFilterOperator,
  ErpFilterState,
  ErpSavedColumnConfig,
  ErpSavedView,
  ErpTableAction,
  ErpTableColumn,
  ErpTableQuery,
  ErpTableSource,
  ErpTableSwitchEvent,
} from './erp-table.models';

const SEARCH_DEBOUNCE_MS = 300;

/**
 * The ERP data grid (Business Central list page, Odoo list view).
 *
 * - **Data**: give it a `source` and it loads one page at a time from the server as the user
 *   scrolls (infinite scrolling, no pager), sending the search, the sort and the filter pane's
 *   conditions with every request. Give it `items` instead and it does all of that in memory.
 * - **Filtering**: a quick search, and a filter pane of conditions on any column (typed
 *   operators, Business Central filter expressions such as `1000..2000|3000`), matched all or any,
 *   shown as removable facets and savable as named views.
 * - **Columns**: users choose which columns show, their order and width, and which are fixed
 *   on horizontal scrolling. The first column is fixed left and the actions column fixed right
 *   by default. Choices are remembered per `tableId`.
 */
@Component({
  selector: 'erp-table',
  templateUrl: './erp-table.component.html',
  styleUrls: ['./erp-table.component.scss'],
  standalone: false,
})
export class ErpTableComponent<T = any> implements OnInit, OnChanges, AfterContentInit, AfterViewInit {
  private readonly destroyRef = inject(DestroyRef);
  private readonly host = inject<ElementRef<HTMLElement>>(ElementRef);
  private readonly localization = inject(LocalizationService, { optional: true });

  // --- Data ---
  /** Server mode: loads a page of rows. Takes precedence over `items`. */
  @Input() source?: ErpTableSource<T>;
  /** Client mode: every row, filtered, sorted and scrolled in memory. */
  @Input() items: T[] | null | undefined = [];
  /** Client mode only: shows the loading bar while the host fetches `items`. */
  @Input() loading = false;
  /** Rows fetched per request in server mode. */
  @Input() pageSize = 50;

  // --- Columns & actions ---
  @Input() columns: ErpTableColumn<T>[] = [];
  @Input() actions: ErpTableAction<T>[] = [];
  @Input() showActions = true;
  @Input() actionsTitleKey = 'Erp::Actions';
  @Input() actionsWidth = 110;
  /** Keeps the first visible column in view on horizontal scrolling unless the user unpins it. */
  @Input() firstColumnFixed = true;
  /** Keeps the actions column in view on horizontal scrolling. */
  @Input() actionsColumnFixed = true;

  // --- Layout ---
  /** Remembers the user's columns and views under this key; defaults to the page's path. */
  @Input() tableId?: string;
  /** Height of the grid; its body scrolls inside it. */
  @Input() height = 'calc(100vh - 290px)';
  @Input() rowHeight = 42;
  @Input() headerHeight = 42;
  /** 'cards' shows the rows through the projected `erpTableCard` template instead of the grid. */
  @Input() layout: 'table' | 'cards' = 'table';
  @Input() wrapInCard = true;
  @Input() clickableRows = true;
  @Input() emptyMessageKey = 'Erp::NoDataAvailable';

  // --- Toolbar ---
  @Input() showToolbar = true;
  @Input() showFilterBar = true;
  @Input() showColumnChooser = true;
  @Input() enableQuickSearch = true;
  /** Search the list opens with, e.g. from a smart button's `?filter=`. */
  @Input() initialSearch = '';

  // --- Selection ---
  @Input() selectable = false;
  @Input() selectionType: 'single' | 'multi' | 'checkbox' = 'single';
  @Input() selected: T[] = [];

  // --- Outputs ---
  @Output() readonly rowClick = new EventEmitter<T>();
  @Output() readonly rowDblClick = new EventEmitter<T>();
  @Output() readonly codeClick = new EventEmitter<T>();
  @Output() readonly selectedChange = new EventEmitter<T[]>();
  @Output() readonly switchChange = new EventEmitter<ErpTableSwitchEvent<T>>();
  @Output() readonly filterChange = new EventEmitter<ErpFilterState>();
  @Output() readonly columnsChange = new EventEmitter<ErpTableColumn<T>[]>();
  /** The rows loaded so far and the total the server reported; e.g. for exporting. */
  @Output() readonly rowsChange = new EventEmitter<{ rows: T[]; totalCount: number }>();

  // --- Projected templates ---
  @ContentChildren(ErpTableColDirective) private readonly colDirectives!: QueryList<ErpTableColDirective>;
  @ContentChild(ErpTableActionsDirective) readonly customActionsDirective?: ErpTableActionsDirective;
  @ContentChild(ErpTableCardDirective) readonly cardDirective?: ErpTableCardDirective;
  private templateMap = new Map<string, TemplateRef<any>>();

  // --- Rows ---
  readonly rows = signal<T[]>([]);
  readonly totalCount = signal(0);
  readonly isLoading = signal(false);
  readonly hasMore = computed(() => this.rows().length < this.totalCount());
  private generation = 0;
  private request?: Subscription;
  private sorting?: string;

  // --- Columns ---
  internalColumns: ErpTableColumn<T>[] = [];
  readonly visibleColumns = signal<ErpTableColumn<T>[]>([]);
  columnSearchQuery = '';

  // --- Filtering ---
  searchTerm = '';
  filterLogic: ErpFilterLogic = 'and';
  filterCriteria: ErpFilterCriterion[] = [];
  isFilterPaneOpen = false;
  savedViews: ErpSavedView[] = [];
  newViewName = '';
  private readonly search$ = new Subject<void>();
  private readonly criteria$ = new Subject<void>();

  readonly isNoValueOperator = isNoValueOperator;

  get isServerMode(): boolean {
    return !!this.source;
  }

  get showActionsColumn(): boolean {
    return this.showActions && (this.actions.length > 0 || !!this.customActionsDirective);
  }

  /** Conditions with their value filled in: the ones actually filtering. */
  get activeCriteriaCount(): number {
    return this.filterCriteria.filter(isComplete).length;
  }

  ngOnInit(): void {
    this.searchTerm = this.initialSearch ?? '';
    this.savedViews = this.loadViews();

    this.search$
      .pipe(debounceTime(SEARCH_DEBOUNCE_MS), takeUntilDestroyed(this.destroyRef))
      .subscribe(() => this.applyFilters());
    this.criteria$
      .pipe(debounceTime(SEARCH_DEBOUNCE_MS), takeUntilDestroyed(this.destroyRef))
      .subscribe(() => this.applyFilters());
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['columns']) {
      this.initializeColumns();
    }
    if (changes['items'] && !this.isServerMode) {
      this.applyClientRows();
    }
    if (changes['source'] && !changes['source'].firstChange) {
      this.reload();
    }
    if (changes['layout'] && !changes['layout'].firstChange) {
      setTimeout(() => this.fillViewport());
    }
  }

  ngAfterContentInit(): void {
    this.rebuildTemplateMap();
    this.colDirectives.changes.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(() => this.rebuildTemplateMap());
  }

  ngAfterViewInit(): void {
    if (this.isServerMode) {
      this.fetch(true);
    }
  }

  // ---------------------------------------------------------------------------------------
  // Data
  // ---------------------------------------------------------------------------------------

  /** Loads the list again from the top, keeping search, filters and sort. */
  reload(): void {
    if (this.isServerMode) {
      this.fetch(true);
    } else {
      this.applyClientRows();
    }
  }

  /** Loads the next page, if there is one (server mode). */
  loadMore(): void {
    if (this.isServerMode) {
      this.fetch(false);
    }
  }

  private fetch(reset: boolean): void {
    if (!this.source || (!reset && (this.isLoading() || !this.hasMore()))) {
      return;
    }

    if (reset) {
      this.generation++;
      this.request?.unsubscribe();
    }

    const generation = this.generation;
    const query: ErpTableQuery = {
      filter: this.searchTerm.trim() || undefined,
      sorting: this.sorting,
      skipCount: reset ? 0 : this.rows().length,
      maxResultCount: this.pageSize,
      dynamicFilter: toDynamicFilter(this.filterState, this.internalColumns),
    };

    this.isLoading.set(true);
    this.request = this.source(query)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (result: PagedResultDto<T>) => {
          // A newer search or filter has been sent since; this answer is stale.
          if (generation !== this.generation) {
            return;
          }
          const items = result.items ?? [];
          this.rows.set(reset ? [...items] : [...this.rows(), ...items]);
          this.totalCount.set(result.totalCount ?? this.rows().length);
          this.isLoading.set(false);
          this.rowsChange.emit({ rows: this.rows(), totalCount: this.totalCount() });
          if (reset) {
            this.scrollToTop();
          }
          setTimeout(() => this.fillViewport());
        },
        error: () => {
          if (generation === this.generation) {
            this.isLoading.set(false);
          }
        },
      });
  }

  /** Client mode: every item that passes the search and the conditions. */
  private applyClientRows(): void {
    const state = this.filterState;
    const items = this.items ?? [];
    const rows = state.searchTerm.trim() || state.criteria.some(isComplete)
      ? items.filter(row => matchesState(row, state, this.internalColumns))
      : [...items];

    this.rows.set(rows);
    this.totalCount.set(rows.length);
    this.rowsChange.emit({ rows, totalCount: rows.length });
  }

  /** Infinite scrolling: near the bottom of the loaded rows, fetch the next page. */
  onTableScroll(event: { offsetY?: number }): void {
    if (!this.isServerMode || event?.offsetY === undefined) {
      return;
    }

    const body = this.host.nativeElement.querySelector<HTMLElement>('.datatable-body');
    const viewHeight = body?.clientHeight ?? 0;
    if (event.offsetY + viewHeight >= this.rows().length * this.rowHeight - this.rowHeight * 5) {
      this.fetch(false);
    }
  }

  onCardsScroll(event: Event): void {
    const element = event.target as HTMLElement;
    if (element.scrollTop + element.clientHeight >= element.scrollHeight - 200) {
      this.loadMore();
    }
  }

  /** A first page shorter than the viewport leaves nothing to scroll; keep loading until it fills. */
  private fillViewport(): void {
    if (!this.isServerMode || this.isLoading() || !this.hasMore()) {
      return;
    }

    const scroller = this.host.nativeElement.querySelector<HTMLElement>(
      this.layout === 'cards' ? '.erp-cards-scroller' : '.datatable-body',
    );
    if (scroller && scroller.scrollHeight <= scroller.clientHeight + this.rowHeight) {
      this.fetch(false);
    }
  }

  private scrollToTop(): void {
    const scroller = this.host.nativeElement.querySelector<HTMLElement>('.datatable-body, .erp-cards-scroller');
    if (scroller) {
      scroller.scrollTop = 0;
    }
  }

  /** Server mode sorts on the server; client mode lets the grid sort the rows it holds. */
  onSort(event: { sorts?: { prop: string; dir: string }[] }): void {
    if (!this.isServerMode) {
      return;
    }

    const sort = event.sorts?.[0];
    const column = sort ? this.getColumnByField(sort.prop) : undefined;
    this.sorting = sort ? `${column?.sortField ?? sort.prop} ${sort.dir}` : undefined;
    this.fetch(true);
  }

  // ---------------------------------------------------------------------------------------
  // Columns: visibility, order, width and pinning
  // ---------------------------------------------------------------------------------------

  private initializeColumns(): void {
    const saved = this.loadSavedColumnConfig();
    const byField = new Map(saved.map((c, index) => [c.field, { ...c, index }]));

    const columns = (this.columns ?? []).map((col, index) => {
      const config = byField.get(col.field);
      let pin: ErpColumnPin = col.pinned ?? (col.frozenLeft ? 'left' : col.frozenRight ? 'right' : 'none');
      if (!col.pinned && !col.frozenLeft && !col.frozenRight && index === 0 && this.firstColumnFixed) {
        pin = 'left';
      }

      return {
        ...col,
        pinned: config?.pinned ?? pin,
        visible: config ? config.visible : col.visible !== false,
        width: config?.width ?? col.width,
      };
    });

    // Saved order first; columns added to the page since then go at the end in their own order.
    this.internalColumns = columns.sort((a, b) => (byField.get(a.field)?.index ?? 1000) - (byField.get(b.field)?.index ?? 1000));
    this.updateVisibleColumns(false);
  }

  private updateVisibleColumns(save = true): void {
    if (save) {
      this.saveColumnConfig();
    }
    this.visibleColumns.set(this.internalColumns.filter(c => c.visible !== false));
    this.columnsChange.emit(this.internalColumns);
  }

  isFrozenLeft(col: ErpTableColumn<T>): boolean {
    return col.pinned === 'left';
  }

  isFrozenRight(col: ErpTableColumn<T>): boolean {
    return col.pinned === 'right';
  }

  toggleColumnVisibility(col: ErpTableColumn<T>, event?: Event): void {
    event?.stopPropagation();
    if (col.hideable === false) {
      return;
    }
    // At least one column always stays.
    if (col.visible !== false && this.visibleColumns().length <= 1) {
      return;
    }
    col.visible = col.visible === false;
    this.updateVisibleColumns();
  }

  setColumnPin(col: ErpTableColumn<T>, pin: ErpColumnPin, event?: Event): void {
    event?.stopPropagation();
    col.pinned = col.pinned === pin ? 'none' : pin;
    this.updateVisibleColumns();
  }

  moveColumn(col: ErpTableColumn<T>, step: -1 | 1, event?: Event): void {
    event?.stopPropagation();
    const from = this.internalColumns.indexOf(col);
    const to = from + step;
    if (from < 0 || to < 0 || to >= this.internalColumns.length) {
      return;
    }
    const columns = [...this.internalColumns];
    [columns[from], columns[to]] = [columns[to], columns[from]];
    this.internalColumns = columns;
    this.updateVisibleColumns();
  }

  /** Dragging a column in the chooser sets its place in the grid. */
  dropColumn(event: CdkDragDrop<unknown>): void {
    if (event.previousIndex === event.currentIndex) {
      return;
    }
    const columns = [...this.internalColumns];
    moveItemInArray(columns, event.previousIndex, event.currentIndex);
    this.internalColumns = columns;
    this.updateVisibleColumns();
  }

  showAllColumns(): void {
    this.internalColumns.forEach(c => (c.visible = true));
    this.updateVisibleColumns();
  }

  resetColumnsToDefault(): void {
    try {
      localStorage.removeItem(this.columnsKey);
    } catch {
      // Storage may be unavailable (private mode); the defaults apply either way.
    }
    this.initializeColumns();
  }

  /** Dragging a header to another place (the grid reports positions among the visible columns). */
  onReorder(event: { column?: { prop?: string }; newValue?: number }): void {
    const moved = this.getColumnByField(String(event.column?.prop ?? ''));
    const target = this.visibleColumns()[event.newValue ?? -1];
    if (!moved || !target || moved === target) {
      return;
    }
    const columns = this.internalColumns.filter(c => c !== moved);
    columns.splice(columns.indexOf(target) + (this.internalColumns.indexOf(moved) < this.internalColumns.indexOf(target) ? 1 : 0), 0, moved);
    this.internalColumns = columns;
    this.updateVisibleColumns();
  }

  onResize(event: { column?: { prop?: string }; newValue?: number }): void {
    const col = this.getColumnByField(String(event.column?.prop ?? ''));
    if (col && event.newValue) {
      col.width = Math.round(event.newValue);
      this.saveColumnConfig();
    }
  }

  get filteredColumnsForChooser(): ErpTableColumn<T>[] {
    const q = this.columnSearchQuery.trim().toLowerCase();
    return q
      ? this.internalColumns.filter(col => this.getColumnLabel(col).toLowerCase().includes(q) || col.field.toLowerCase().includes(q))
      : this.internalColumns;
  }

  // ---------------------------------------------------------------------------------------
  // Filtering (BC filter pane, Odoo facets)
  // ---------------------------------------------------------------------------------------

  get filterState(): ErpFilterState {
    return { searchTerm: this.searchTerm, criteria: this.filterCriteria, logic: this.filterLogic };
  }

  get filterableColumns(): ErpTableColumn<T>[] {
    return this.internalColumns.filter(c => c.filterable !== false && c.type !== 'custom');
  }

  onSearchInput(value: string): void {
    this.searchTerm = value ?? '';
    this.search$.next();
  }

  toggleFilterPane(): void {
    this.isFilterPaneOpen = !this.isFilterPaneOpen;
    if (this.isFilterPaneOpen && this.filterCriteria.length === 0) {
      this.addFilterCriterion();
    }
  }

  /** Adds a condition, on the given column or the first filterable one; also the header's filter button. */
  addFilterCriterion(field?: string): void {
    const column = this.getColumnByField(field ?? '') ?? this.filterableColumns[0];
    if (!column) {
      return;
    }
    const operator = defaultOperatorFor(column);
    this.filterCriteria = [
      ...this.filterCriteria,
      { id: 'c' + Math.random().toString(36).substring(2, 9), field: column.field, operator, value: '' },
    ];
    this.isFilterPaneOpen = true;
    this.onCriteriaChanged();
  }

  removeFilterCriterion(id: string): void {
    this.filterCriteria = this.filterCriteria.filter(c => c.id !== id);
    this.applyFilters();
  }

  clearAllFilters(): void {
    this.filterCriteria = [];
    this.searchTerm = '';
    this.applyFilters();
  }

  onFieldChange(criterion: ErpFilterCriterion, field: string): void {
    criterion.field = field;
    criterion.operator = defaultOperatorFor(this.getColumnByField(field));
    criterion.value = '';
    criterion.valueTo = '';
    this.onCriteriaChanged();
  }

  onOperatorChange(criterion: ErpFilterCriterion, operator: ErpFilterOperator): void {
    criterion.operator = operator;
    if (isNoValueOperator(operator)) {
      criterion.value = undefined;
      criterion.valueTo = undefined;
    }
    this.onCriteriaChanged();
  }

  setFilterLogic(logic: ErpFilterLogic): void {
    this.filterLogic = logic;
    this.onCriteriaChanged();
  }

  /** Typing into a condition waits a moment, so the list is not reloaded on every key. */
  onCriteriaChanged(): void {
    this.criteria$.next();
  }

  applyFilters(): void {
    this.filterChange.emit(this.filterState);
    this.reload();
  }

  operatorsFor(field: string): { value: ErpFilterOperator; label: string }[] {
    return operatorsFor(this.getColumnByField(field));
  }

  inputTypeFor(field: string): 'select' | 'number' | 'date' | 'text' {
    const column = this.getColumnByField(field);
    if (column?.options?.length) {
      return 'select';
    }
    switch (column?.type) {
      case 'number':
      case 'currency':
        return 'number';
      case 'date':
      case 'datetime':
        return 'date';
      default:
        return 'text';
    }
  }

  /** The Odoo-style facets above the grid: one per active condition, plus the search. */
  get activeFilterPills(): { id: string; label: string; isSearch?: boolean }[] {
    const pills: { id: string; label: string; isSearch?: boolean }[] = [];
    if (this.searchTerm.trim()) {
      pills.push({ id: '__search__', label: `"${this.searchTerm.trim()}"`, isSearch: true });
    }

    for (const c of this.filterCriteria.filter(isComplete)) {
      const column = this.getColumnByField(c.field);
      const name = column ? this.getColumnLabel(column) : c.field;
      const op = this.l(`Erp::Table:Op:${c.operator}`);
      const value = c.operator === 'between'
        ? `${this.valueLabel(column, c.value) || '…'} .. ${this.valueLabel(column, c.valueTo) || '…'}`
        : isNoValueOperator(c.operator) ? '' : this.valueLabel(column, c.value);
      pills.push({ id: c.id, label: value ? `${name} ${op} ${value}` : `${name} ${op}` });
    }

    return pills;
  }

  removeFilterPill(id: string): void {
    if (id === '__search__') {
      this.searchTerm = '';
      this.applyFilters();
    } else {
      this.removeFilterCriterion(id);
    }
  }

  private valueLabel(column: ErpTableColumn<T> | undefined, value: any): string {
    if (value === undefined || value === null || value === '') {
      return '';
    }
    const option = column?.options?.find(o => String(o.value) === String(value));
    return option ? this.l(option.label) : String(value);
  }

  // --- Saved views (BC views, Odoo favorites) ---

  saveCurrentView(): void {
    const name = this.newViewName.trim();
    if (!name) {
      return;
    }
    const view: ErpSavedView = {
      name,
      state: {
        searchTerm: this.searchTerm,
        logic: this.filterLogic,
        criteria: this.filterCriteria.filter(isComplete).map(c => ({ ...c })),
      },
    };
    this.savedViews = [...this.savedViews.filter(v => v.name !== name), view];
    this.newViewName = '';
    this.saveViews();
  }

  applyView(view: ErpSavedView): void {
    this.searchTerm = view.state.searchTerm ?? '';
    this.filterLogic = view.state.logic ?? 'and';
    this.filterCriteria = (view.state.criteria ?? []).map(c => ({ ...c }));
    this.applyFilters();
  }

  deleteView(view: ErpSavedView, event?: Event): void {
    event?.stopPropagation();
    this.savedViews = this.savedViews.filter(v => v !== view);
    this.saveViews();
  }

  // ---------------------------------------------------------------------------------------
  // Persistence (per table, per browser)
  // ---------------------------------------------------------------------------------------

  private get storageId(): string {
    return this.tableId || window.location.pathname;
  }

  private get columnsKey(): string {
    return `erp_table_cols_${this.storageId}`;
  }

  private get viewsKey(): string {
    return `erp_table_views_${this.storageId}`;
  }

  private saveColumnConfig(): void {
    const config: ErpSavedColumnConfig[] = this.internalColumns.map(c => ({
      field: c.field,
      visible: c.visible !== false,
      pinned: c.pinned ?? 'none',
      width: c.width,
    }));
    this.write(this.columnsKey, config);
  }

  private loadSavedColumnConfig(): ErpSavedColumnConfig[] {
    return this.read<ErpSavedColumnConfig[]>(this.columnsKey) ?? [];
  }

  private saveViews(): void {
    this.write(this.viewsKey, this.savedViews);
  }

  private loadViews(): ErpSavedView[] {
    return this.read<ErpSavedView[]>(this.viewsKey) ?? [];
  }

  private read<V>(key: string): V | null {
    try {
      const raw = localStorage.getItem(key);
      return raw ? (JSON.parse(raw) as V) : null;
    } catch {
      return null;
    }
  }

  private write(key: string, value: unknown): void {
    try {
      localStorage.setItem(key, JSON.stringify(value));
    } catch {
      // Storage may be full or blocked; the table works without remembering.
    }
  }

  // ---------------------------------------------------------------------------------------
  // Cells
  // ---------------------------------------------------------------------------------------

  private rebuildTemplateMap(): void {
    const directives = this.colDirectives?.toArray() ?? [];
    this.templateMap = new Map(directives.filter(d => !!d.colName).map(d => [d.colName, d.templateRef] as const));
  }

  getCustomTemplate(field: string): TemplateRef<any> | null {
    return this.getColumnByField(field)?.template ?? this.templateMap.get(field) ?? null;
  }

  getColumnByField(field: string): ErpTableColumn<T> | undefined {
    return this.internalColumns.find(c => c.field === field);
  }

  getColumnLabel(col: ErpTableColumn<T>): string {
    return col.labelKey ? this.l(col.labelKey) : col.field;
  }

  optionLabel(col: ErpTableColumn<T>, value: any): string {
    const option = col.options?.find(o => o.value === value);
    return option ? this.l(option.label) : String(value ?? '');
  }

  getHeaderClass(col: ErpTableColumn<T>): string {
    return [col.headerClass, this.alignClass(col)].filter(Boolean).join(' ');
  }

  getCellClass(col: ErpTableColumn<T>): string {
    return [col.cellClass, this.alignClass(col)].filter(Boolean).join(' ');
  }

  private alignClass(col: ErpTableColumn<T>): string {
    if (col.type === 'currency' || col.type === 'number') {
      return 'text-end';
    }
    return col.type === 'boolean' || col.type === 'switch' ? 'text-center' : '';
  }

  getBadgeClass(col: ErpTableColumn<T>, row: T): string {
    return typeof col.badgeClass === 'function' ? col.badgeClass(row) : col.badgeClass || 'bg-light text-secondary border';
  }

  getIcon(col: ErpTableColumn<T>, row: T): string {
    return typeof col.icon === 'function' ? col.icon(row) : col.icon || '';
  }

  isClickable(col: ErpTableColumn<T>, row: T): boolean {
    if (col.type !== 'code' && col.type !== 'link') {
      return false;
    }
    return typeof col.clickable === 'function' ? col.clickable(row) : col.clickable !== false;
  }

  onCodeColumnClick(row: T, event: MouseEvent): void {
    event.stopPropagation();
    this.codeClick.emit(row);
  }

  onSwitchToggle(col: ErpTableColumn<T>, row: T, event: Event): void {
    event.stopPropagation();
    const checked = (event.target as HTMLInputElement).checked;
    (row as any)[col.field] = checked;
    this.switchChange.emit({ row, field: col.field, checked });
  }

  onActivate(event: { type: string; row: T }): void {
    if (!this.clickableRows || !event.row) {
      return;
    }
    if (event.type === 'click') {
      this.rowClick.emit(event.row);
    } else if (event.type === 'dblclick') {
      this.rowDblClick.emit(event.row);
    }
  }

  onSelect(event: { selected?: T[] }): void {
    if (this.selectable && event.selected) {
      this.selectedChange.emit(event.selected);
    }
  }

  isActionVisible(action: ErpTableAction<T>, row: T): boolean {
    return typeof action.visible === 'function' ? action.visible(row) : action.visible !== false;
  }

  isActionDisabled(action: ErpTableAction<T>, row: T): boolean {
    return typeof action.disabled === 'function' ? action.disabled(row) : action.disabled === true;
  }

  executeAction(action: ErpTableAction<T>, row: T, event: MouseEvent): void {
    event.stopPropagation();
    if (!this.isActionDisabled(action, row)) {
      action.action(row, event);
    }
  }

  trackRow = (_index: number, row: any) => row?.id ?? row;

  private l(key: string): string {
    return this.localization ? this.localization.instant(key) : key;
  }
}
