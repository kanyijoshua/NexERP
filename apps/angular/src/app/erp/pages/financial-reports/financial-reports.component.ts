import { Component, OnInit } from '@angular/core';
import { ErpApiService, FinancialReportDto, ReportLayoutDto } from '../../services/erp-api.service';

@Component({
  selector: 'app-financial-reports',
  template: `
    <div class="container-fluid py-3">
      <app-company-switcher></app-company-switcher>

      <div class="card border-0 shadow-sm">
        <div class="card-header bg-white d-flex justify-content-between align-items-center py-3">
          <div>
            <h5 class="mb-0 fw-bold"><i class="fas fa-chart-line text-primary me-2"></i>Financial Reports (BC Account Schedules)</h5>
            <small class="text-muted">Trial Balance / Balance Sheet / Income Statement Report Engine</small>
          </div>
          <div class="d-flex gap-2 align-items-center">
            <button class="btn btn-sm btn-outline-primary" (click)="openLayoutModal()">
              <i class="fas fa-layer-group me-1"></i> Report Layout Selection (BC T9651)
            </button>
            <button class="btn btn-sm btn-success"><i class="fas fa-file-excel me-1"></i> Export to Excel (.xlsx)</button>
          </div>
        </div>

        <div class="card-body">
          <div class="d-flex justify-content-between align-items-center mb-3 p-2 bg-light rounded">
            <div class="small">
              <span class="fw-bold text-dark">Active Default Layout:</span>
              <span class="badge bg-primary ms-2">{{ activeLayout?.layoutName || 'Standard Grid (RDLC)' }}</span>
              <span class="badge bg-secondary ms-1">{{ activeLayout?.layoutType || 'RDLC' }}</span>
            </div>
            <div class="small text-muted">
              <i class="fas fa-info-circle me-1"></i> Layout type determines report renderer (RDLC, Word, Excel, HTML)
            </div>
          </div>

          <div *if="report" class="table-responsive">
            <h6 class="fw-bold text-uppercase border-bottom pb-2">{{ report.reportTitle }}</h6>
            <table class="table table-sm table-striped align-middle mb-0">
              <thead class="table-light">
                <tr>
                  <th>Account No.</th>
                  <th>Description</th>
                  <th class="text-end">Amount</th>
                </tr>
              </thead>
              <tbody>
                <tr *ngFor="let row of report.rows">
                  <td class="fw-bold">{{ row.rowNo }}</td>
                  <td>{{ row.description }}</td>
                  <td class="text-end fw-bold" [class.text-danger]="row.amount < 0" [class.text-success]="row.amount > 0">
                    {{ row.amount | currency }}
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>
      </div>

      <!-- Report Layout Selection Modal (BC Table 9651) -->
      <div *if="showLayoutModal" class="modal d-block bg-dark bg-opacity-50" tabindex="-1">
        <div class="modal-dialog modal-lg">
          <div class="modal-content">
            <div class="modal-header">
              <h5 class="modal-title"><i class="fas fa-layer-group text-primary me-2"></i>Report Layout Selection & Layouts</h5>
              <button type="button" class="btn-close" (click)="showLayoutModal = false"></button>
            </div>
            <div class="modal-body">
              <p class="text-muted small">Select the default layout view to be used when generating and printing Trial Balance reports.</p>
              <div class="table-responsive">
                <table class="table table-hover align-middle mb-0">
                  <thead class="table-light">
                    <tr>
                      <th>Layout Name</th>
                      <th>Type</th>
                      <th>Description</th>
                      <th>Status</th>
                      <th class="text-end">Action</th>
                    </tr>
                  </thead>
                  <tbody>
                    <tr *ngFor="let l of layouts">
                      <td class="fw-bold">{{ l.layoutName }}</td>
                      <td><span class="badge bg-secondary">{{ l.layoutType }}</span></td>
                      <td class="small">{{ l.description }}</td>
                      <td>
                        <span class="badge" [class.bg-success]="l.isDefault" [class.bg-light]="!l.isDefault" [class.text-dark]="!l.isDefault">
                          {{ l.isDefault ? 'Active Default' : 'Available' }}
                        </span>
                      </td>
                      <td class="text-end">
                        <button *if="!l.isDefault" class="btn btn-xs btn-outline-primary" (click)="setDefaultLayout(l)">
                          Set Default
                        </button>
                      </td>
                    </tr>
                  </tbody>
                </table>
              </div>
            </div>
            <div class="modal-footer">
              <button class="btn btn-secondary" (click)="showLayoutModal = false">Close</button>
            </div>
          </div>
        </div>
      </div>
    </div>
  `,
})
export class FinancialReportsComponent implements OnInit {
  report: FinancialReportDto | null = null;
  layouts: ReportLayoutDto[] = [];
  activeLayout: ReportLayoutDto | null = null;
  showLayoutModal = false;

  constructor(private erpApi: ErpApiService) {}

  ngOnInit(): void {
    const today = new Date().toISOString().split('T')[0];
    this.erpApi.getTrialBalance('2026-01-01', today).subscribe(data => {
      this.report = data;
    });
    this.loadLayouts();
  }

  loadLayouts(): void {
    this.erpApi.getReportLayouts('Trial Balance').subscribe(list => {
      this.layouts = list;
      this.activeLayout = list.find(l => l.isDefault) || (list.length > 0 ? list[0] : null);
    });
  }

  openLayoutModal(): void {
    this.showLayoutModal = true;
  }

  setDefaultLayout(layout: ReportLayoutDto): void {
    this.erpApi.setDefaultReportLayout('Trial Balance', layout.id).subscribe(() => {
      this.loadLayouts();
    });
  }
}
