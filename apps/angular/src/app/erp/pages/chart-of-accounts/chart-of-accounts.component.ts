import { Component, OnInit } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { CompanyService } from '../../services/company.service';
import { ErpApiService, GLAccountDto } from '../../services/erp-api.service';

@Component({
  selector: 'app-chart-of-accounts',
  template: `
    <div class="container-fluid py-3">
      <app-company-switcher></app-company-switcher>

      <div class="card border-0 shadow-sm">
        <div class="card-header bg-white d-flex justify-content-between align-items-center py-3">
          <h5 class="mb-0 fw-bold"><i class="fas fa-list-ol text-primary me-2"></i>Chart of Accounts (Business Central Table 15)</h5>
          <button class="btn btn-sm btn-outline-primary"><i class="fas fa-file-excel me-1"></i> Export to Excel</button>
        </div>
        <div class="table-responsive">
          <table class="table table-hover align-middle mb-0">
            <thead class="table-light">
              <tr>
                <th>No.</th>
                <th>Name</th>
                <th>Account Type</th>
                <th>Category</th>
                <th class="text-end">Net Change</th>
                <th class="text-end">Balance</th>
              </tr>
            </thead>
            <tbody>
              <tr *ngFor="let acc of accounts">
                <td class="fw-bold">{{ acc.no }}</td>
                <td>{{ acc.name }}</td>
                <td><span class="badge bg-secondary">Posting</span></td>
                <td>Asset / Revenue</td>
                <td class="text-end fw-bold" [class.text-success]="acc.netChange > 0">{{ acc.netChange | currency }}</td>
                <td class="text-end fw-bold" [class.text-primary]="acc.balance > 0">{{ acc.balance | currency }}</td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </div>
  `,
})
export class ChartOfAccountsComponent implements OnInit {
  accounts: GLAccountDto[] = [];

  constructor(private erpApi: ErpApiService, companyService: CompanyService) {
    // Re-query when the active company changes (replaces the old full page reload).
    companyService.companyChanged$.pipe(takeUntilDestroyed()).subscribe(() => this.ngOnInit());
  }

  ngOnInit(): void {
    this.erpApi.getGLAccounts().subscribe(data => {
      this.accounts = data;
    });
  }
}
