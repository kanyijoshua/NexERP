import { Component, DestroyRef, OnInit, ViewChild, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ActivatedRoute } from '@angular/router';
import { BankAccountLedgerEntryDto, BankAccountLedgerEntryService } from '@proxy/cash-management';
import { ErpTableColumn, ErpTableComponent, ErpTableSource } from '../../erp-shared';
import { CompanyService } from '../../services/company.service';

/**
 * Bank Account Ledger Entries. Mirrors Business Central page 372; opened from a bank account card
 * with `?bankAccountId=`, or unfiltered from the menu.
 */
@Component({
  selector: 'app-bank-ledger-entries',
  templateUrl: './bank-ledger-entries.component.html',
  standalone: false,
})
export class BankLedgerEntriesComponent implements OnInit {
  @ViewChild(ErpTableComponent) table?: ErpTableComponent<BankAccountLedgerEntryDto>;

  private readonly entries = inject(BankAccountLedgerEntryService);
  private readonly companyService = inject(CompanyService);
  private readonly route = inject(ActivatedRoute);
  private readonly destroyRef = inject(DestroyRef);

  readonly columns: ErpTableColumn<BankAccountLedgerEntryDto>[] = [
    { field: 'entryNo', labelKey: 'Erp::EntryNo', type: 'number', width: 100 },
    { field: 'postingDate', labelKey: 'Erp::PostingDate', type: 'date', width: 120 },
    { field: 'bankAccountNo', labelKey: 'Erp::BankAccount', width: 150 },
    { field: 'documentNo', labelKey: 'Erp::DocumentNo', width: 140 },
    { field: 'description', labelKey: 'Erp::Description', width: 260 },
    { field: 'amount', labelKey: 'Erp::Amount', type: 'currency', width: 140 },
    { field: 'open', labelKey: 'Erp::Open', type: 'boolean', width: 80 },
    { field: 'reversed', labelKey: 'Erp::Reversed', type: 'boolean', width: 100 },
  ];

  readonly source: ErpTableSource<BankAccountLedgerEntryDto> = query =>
    this.entries.getList({ ...query, bankAccountId: this.bankAccountId ?? undefined });

  bankAccountId: string | null = null;
  bankAccountNo: string | null = null;

  ngOnInit(): void {
    this.bankAccountId = this.route.snapshot.queryParamMap.get('bankAccountId');
    this.bankAccountNo = this.route.snapshot.queryParamMap.get('bankAccountNo');

    this.companyService.companyChanged$.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(() => this.table?.reload());
  }

  showAll(): void {
    this.bankAccountId = null;
    this.bankAccountNo = null;
    this.table?.reload();
  }
}
