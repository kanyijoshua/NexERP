import { Component, OnInit } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { CompanyService } from '../../services/company.service';
import { ErpApiService, SalesHeaderDto } from '../../services/erp-api.service';

@Component({
  selector: 'app-sales-invoices',
  template: `
    <div class="container-fluid py-3">
      <app-company-switcher></app-company-switcher>

      <div class="row">
        <div class="col-md-8">
          <div class="card border-0 shadow-sm">
            <div class="card-header bg-white d-flex justify-content-between align-items-center py-3">
              <h5 class="mb-0 fw-bold"><i class="fas fa-file-invoice-dollar text-primary me-2"></i>Sales Invoices (BC Table 36/112)</h5>
              <button class="btn btn-sm btn-primary"><i class="fas fa-plus me-1"></i> New Sales Invoice</button>
            </div>
            <div class="table-responsive">
              <table class="table table-hover align-middle mb-0">
                <thead class="table-light">
                  <tr>
                    <th>Invoice No</th>
                    <th>Customer</th>
                    <th>Posting Date</th>
                    <th class="text-end">Total Amount</th>
                    <th>Status</th>
                    <th class="text-end">Actions</th>
                  </tr>
                </thead>
                <tbody>
                  <tr *ngFor="let inv of invoices" [class.table-active]="selectedInvoice?.id === inv.id" (click)="selectInvoice(inv)">
                    <td class="fw-bold">{{ inv.no }}</td>
                    <td>{{ inv.sellToCustomerName }}</td>
                    <td>{{ inv.postingDate | date: 'mediumDate' }}</td>
                    <td class="text-end fw-bold">{{ inv.totalAmountIncludingVat | currency }}</td>
                    <td>
                      <span class="badge" [class.bg-success]="inv.posted" [class.bg-warning]="!inv.posted">
                        {{ inv.posted ? 'Posted' : 'Open Draft' }}
                      </span>
                    </td>
                    <td class="text-end">
                      <button *ngIf="!inv.posted" class="btn btn-sm btn-success me-1" (click)="postInvoice(inv.id)">
                        <i class="fas fa-check-circle me-1"></i> Post (CU 80)
                      </button>
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
            entityType="SalesHeader"
            [entityId]="selectedInvoice.id"
            [entityNo]="selectedInvoice.no"
          ></app-chatter-widget>
          <div *ngIf="!selectedInvoice" class="card border-0 shadow-sm p-4 text-center text-muted">
            <i class="fas fa-mouse-pointer fa-2x mb-2 text-secondary"></i>
            <p class="mb-0 small">Select a sales invoice to view Odoo Chatter activity feed & internal notes.</p>
          </div>
        </div>
      </div>
    </div>
  `,
})
export class SalesInvoicesComponent implements OnInit {
  invoices: SalesHeaderDto[] = [];
  selectedInvoice: SalesHeaderDto | null = null;

  constructor(private erpApi: ErpApiService, companyService: CompanyService) {
    // Re-query when the active company changes (replaces the old full page reload).
    companyService.companyChanged$.pipe(takeUntilDestroyed()).subscribe(() => this.ngOnInit());
  }

  ngOnInit(): void {
    this.loadInvoices();
  }

  loadInvoices(): void {
    this.erpApi.getSalesInvoices().subscribe(data => {
      this.invoices = data;
      if (data.length > 0 && !this.selectedInvoice) {
        this.selectedInvoice = data[0];
      }
    });
  }

  selectInvoice(inv: SalesHeaderDto): void {
    this.selectedInvoice = inv;
  }

  postInvoice(id: string): void {
    this.erpApi.postSalesInvoice(id).subscribe(() => {
      this.loadInvoices();
    });
  }
}
