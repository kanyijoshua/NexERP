import { Component, OnInit } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { CompanyService } from '../../services/company.service';
import { ErpApiService, PurchaseHeaderDto } from '../../services/erp-api.service';

@Component({
  selector: 'app-purchase-invoices',
  template: `
    <div class="container-fluid py-3">
      <app-company-switcher></app-company-switcher>

      <div class="row">
        <div class="col-md-8">
          <div class="card border-0 shadow-sm">
            <div class="card-header bg-white d-flex justify-content-between align-items-center py-3">
              <h5 class="mb-0 fw-bold"><i class="fas fa-shopping-cart text-primary me-2"></i>Purchase Invoices (BC Table 38/122)</h5>
              <button class="btn btn-sm btn-primary"><i class="fas fa-plus me-1"></i> New Purchase Invoice</button>
            </div>
            <div class="table-responsive">
              <table class="table table-hover align-middle mb-0">
                <thead class="table-light">
                  <tr>
                    <th>Invoice No</th>
                    <th>Vendor</th>
                    <th>Posting Date</th>
                    <th class="text-end">Total Amount</th>
                    <th>Status</th>
                  </tr>
                </thead>
                <tbody>
                  <tr *ngFor="let inv of invoices" [class.table-active]="selectedInvoice?.id === inv.id" (click)="selectInvoice(inv)">
                    <td class="fw-bold">{{ inv.no }}</td>
                    <td>{{ inv.buyFromVendorName }}</td>
                    <td>{{ inv.postingDate | date: 'mediumDate' }}</td>
                    <td class="text-end fw-bold">{{ inv.totalAmountIncludingVat | currency }}</td>
                    <td>
                      <span class="badge" [class.bg-success]="inv.posted" [class.bg-warning]="!inv.posted">
                        {{ inv.posted ? 'Posted' : 'Open Draft' }}
                      </span>
                    </td>
                  </tr>
                </tbody>
              </table>
            </div>
          </div>
        </div>

        <div class="col-md-4">
          <app-chatter-widget
            *ngIf="selectedInvoice"
            entityType="PurchaseHeader"
            [entityId]="selectedInvoice.id"
            [entityNo]="selectedInvoice.no"
          ></app-chatter-widget>
        </div>
      </div>
    </div>
  `,
})
export class PurchaseInvoicesComponent implements OnInit {
  invoices: PurchaseHeaderDto[] = [];
  selectedInvoice: PurchaseHeaderDto | null = null;

  constructor(private erpApi: ErpApiService, companyService: CompanyService) {
    // Re-query when the active company changes (replaces the old full page reload).
    companyService.companyChanged$.pipe(takeUntilDestroyed()).subscribe(() => this.ngOnInit());
  }

  ngOnInit(): void {
    this.erpApi.getPurchaseInvoices().subscribe(data => {
      this.invoices = data;
      if (data.length > 0) this.selectedInvoice = data[0];
    });
  }

  selectInvoice(inv: PurchaseHeaderDto): void {
    this.selectedInvoice = inv;
  }
}
