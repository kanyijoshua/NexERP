import { PagedResultDto } from '@abp/ng.core';
import { Component, DestroyRef, OnInit, ViewChild, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ActivatedRoute } from '@angular/router';
import {
  CustomerLedgerEntryService,
  GLEntryDocumentType,
  GetPartyLedgerEntryListInput,
  VendorLedgerEntryService,
  glEntryDocumentTypeOptions,
} from '@proxy/finance';
import { EmployeeLedgerEntryService } from '@proxy/human-resources';
import { Observable, map } from 'rxjs';
import { ErpTableColumn, ErpTableComponent, ErpTableSource } from '../../erp-shared';
import { CompanyService } from '../../services/company.service';

export type PartyLedgerKind = 'customer' | 'vendor' | 'employee';

/** One row of any of the three party ledgers, as the page shows it. */
export interface PartyLedgerRow {
  entryNo: number;
  partyNo?: string;
  postingDate?: string;
  documentType?: string;
  documentNo?: string;
  description?: string;
  currencyCode?: string;
  amount: number;
  amountLcy: number;
  remainingAmount: number;
  remainingAmountLcy: number;
  dueDate?: string;
  open: boolean;
  closedByEntryNo: number;
  reversed: boolean;
}

/** Each ledger's wording, and the entry field its party number is filtered and sorted by. */
const TITLES: Record<PartyLedgerKind, { title: string; party: string; icon: string; partyField: string }> = {
  customer: { title: 'Erp::CustomerLedgerEntries', party: 'Erp::Customer', icon: 'fas fa-user-tie', partyField: 'customerNo' },
  vendor: { title: 'Erp::VendorLedgerEntries', party: 'Erp::Vendor', icon: 'fas fa-truck', partyField: 'vendorNo' },
  employee: { title: 'Erp::EmployeeLedgerEntries', party: 'Erp::Employee', icon: 'fas fa-id-badge', partyField: 'employeeNo' },
};

/**
 * Customer, vendor and employee ledger entries. Mirrors Business Central pages 25, 29 and 5237:
 * each entry in its own currency and in LCY, what is still open of it, and the entry that closed
 * it. Which ledger is shown comes from the route's `kind`; `?partyNo=` narrows it to one party.
 */
@Component({
  selector: 'app-party-ledger-entries',
  templateUrl: './party-ledger-entries.component.html',
  standalone: false,
})
export class PartyLedgerEntriesComponent implements OnInit {
  @ViewChild(ErpTableComponent) table?: ErpTableComponent<PartyLedgerRow>;

  private readonly customers = inject(CustomerLedgerEntryService);
  private readonly vendors = inject(VendorLedgerEntryService);
  private readonly employees = inject(EmployeeLedgerEntryService);
  private readonly companyService = inject(CompanyService);
  private readonly route = inject(ActivatedRoute);
  private readonly destroyRef = inject(DestroyRef);

  readonly onlyOpen = signal(false);

  kind: PartyLedgerKind = 'customer';
  partyNo: string | null = null;
  columns: ErpTableColumn<PartyLedgerRow>[] = [];

  readonly source: ErpTableSource<PartyLedgerRow> = query =>
    this.load({ ...query, partyNo: this.partyNo ?? undefined, onlyOpen: this.onlyOpen() });

  get labels() {
    return TITLES[this.kind];
  }

  /** Employees are paid in LCY only, so their ledger has no currency columns. */
  get showCurrency(): boolean {
    return this.kind !== 'employee';
  }

  ngOnInit(): void {
    this.kind = (this.route.snapshot.data['kind'] as PartyLedgerKind) ?? 'customer';
    this.partyNo = this.route.snapshot.queryParamMap.get('partyNo');
    this.columns = this.buildColumns();

    this.companyService.companyChanged$.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(() => this.table?.reload());
  }

  toggleOpen(value: boolean): void {
    this.onlyOpen.set(value);
    this.table?.reload();
  }

  showAll(): void {
    this.partyNo = null;
    this.table?.reload();
  }

  private buildColumns(): ErpTableColumn<PartyLedgerRow>[] {
    const currency = this.showCurrency;
    const partyField = this.labels.partyField;
    const columns: (ErpTableColumn<PartyLedgerRow> | false)[] = [
      { field: 'entryNo', labelKey: 'Erp::EntryNo', type: 'number', width: 90 },
      { field: 'postingDate', labelKey: 'Erp::PostingDate', type: 'date', width: 110 },
      { field: 'partyNo', labelKey: this.labels.party, width: 110, filterField: partyField, sortField: partyField },
      {
        field: 'documentType',
        labelKey: 'Erp::DocumentType',
        type: 'select',
        width: 120,
        options: glEntryDocumentTypeOptions.map(o => ({ value: o.key, label: o.key })),
      },
      { field: 'documentNo', labelKey: 'Erp::DocumentNo', width: 130 },
      { field: 'description', labelKey: 'Erp::Description', width: 220 },
      currency && { field: 'currencyCode', labelKey: 'Erp::CurrencyCode', width: 80 },
      { field: 'amount', labelKey: 'Erp::Amount', type: 'currency', width: 120 },
      currency && { field: 'amountLcy', labelKey: 'Erp::AmountLcy', type: 'currency', width: 130 },
      { field: 'remainingAmount', labelKey: 'Erp::RemainingAmount', type: 'currency', width: 130 },
      currency && { field: 'remainingAmountLcy', labelKey: 'Erp::RemainingAmountLcy', type: 'currency', width: 140 },
      currency && { field: 'dueDate', labelKey: 'Erp::DueDate', type: 'date', width: 110 },
      { field: 'open', labelKey: 'Erp::Open', type: 'boolean', width: 70 },
      { field: 'closedByEntryNo', labelKey: 'Erp::ClosedByEntryNo', type: 'number', width: 110 },
      { field: 'reversed', labelKey: 'Erp::Reversed', type: 'boolean', width: 90 },
    ];
    return columns.filter((c): c is ErpTableColumn<PartyLedgerRow> => !!c);
  }

  private load(query: GetPartyLedgerEntryListInput): Observable<PagedResultDto<PartyLedgerRow>> {
    if (this.kind === 'employee') {
      return this.employees.getList({ ...query, employeeNo: query.partyNo }).pipe(
        map(result => ({
          totalCount: result.totalCount,
          items: (result.items ?? []).map(e => ({
            entryNo: e.entryNo,
            partyNo: e.employeeNo,
            postingDate: e.postingDate,
            documentType: GLEntryDocumentType[e.documentType],
            documentNo: e.documentNo,
            description: e.description,
            amount: e.amount,
            amountLcy: e.amount,
            remainingAmount: e.remainingAmount,
            remainingAmountLcy: e.remainingAmount,
            open: e.open,
            closedByEntryNo: e.closedByEntryNo,
            reversed: e.reversed,
          })),
        })),
      );
    }

    const service = this.kind === 'vendor' ? this.vendors : this.customers;
    return service.getList(query);
  }
}
