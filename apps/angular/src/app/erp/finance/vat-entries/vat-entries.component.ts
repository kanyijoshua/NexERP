import { Component, DestroyRef, OnInit, ViewChild, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { GetVatEntryListInput, VatEntryDto, VatEntryService, VatEntryType, vatEntryTypeOptions } from '@proxy/finance';
import { ErpTableColumn, ErpTableComponent, ErpTableSource } from '../../erp-shared';
import { CompanyService } from '../../services/company.service';

/** VAT Entries. what the VAT return is built from. */
@Component({
  selector: 'app-vat-entries',
  templateUrl: './vat-entries.component.html',
  standalone: false,
})
export class VatEntriesComponent implements OnInit {
  @ViewChild(ErpTableComponent) table?: ErpTableComponent<VatEntryDto>;

  private readonly entries = inject(VatEntryService);
  private readonly companyService = inject(CompanyService);
  private readonly destroyRef = inject(DestroyRef);

  readonly typeOptions = vatEntryTypeOptions;
  readonly VatEntryType = VatEntryType;
  readonly loadedRows = signal<VatEntryDto[]>([]);

  readonly columns: ErpTableColumn<VatEntryDto>[] = [
    { field: 'entryNo', labelKey: 'Erp::EntryNo', type: 'number', width: 100 },
    { field: 'postingDate', labelKey: 'Erp::PostingDate', type: 'date', width: 120 },
    { field: 'documentNo', labelKey: 'Erp::DocumentNo', width: 140 },
    {
      field: 'type',
      labelKey: 'Erp::Type',
      type: 'select',
      width: 110,
      options: vatEntryTypeOptions.map(o => ({ value: o.value, label: 'Erp::Enum:VatEntryType.' + o.key })),
    },
    { field: 'billToPayToNo', labelKey: 'Erp::BillToPayToNo', width: 140 },
    { field: 'vatBusPostingGroup', labelKey: 'Erp::VatBusPostingGroup', width: 150 },
    { field: 'vatProdPostingGroup', labelKey: 'Erp::VatProdPostingGroup', width: 150 },
    { field: 'vatPercent', labelKey: 'Erp::VatPercent', type: 'number', width: 90 },
    { field: 'base', labelKey: 'Erp::Base', type: 'currency', width: 130 },
    { field: 'amount', labelKey: 'Erp::Amount', type: 'currency', width: 130 },
    { field: 'reversed', labelKey: 'Erp::Reversed', type: 'boolean', width: 100 },
  ];

  type: VatEntryType | null = null;
  fromDate = '';
  toDate = '';

  readonly source: ErpTableSource<VatEntryDto> = query =>
    this.entries.getList({
      ...query,
      type: this.type ?? undefined,
      fromDate: this.fromDate || undefined,
      toDate: this.toDate || undefined,
    } as GetVatEntryListInput);

  ngOnInit(): void {
    this.companyService.companyChanged$.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(() => this.refresh());
  }

  refresh(): void {
    this.table?.reload();
  }

  /** Output VAT (sales) is shown as owed, input VAT (purchases) as reclaimable. */
  totalOf(type: VatEntryType): number {
    return this.loadedRows()
      .filter(e => e.type === type)
      .reduce((sum, e) => sum + e.amount, 0);
  }
}
