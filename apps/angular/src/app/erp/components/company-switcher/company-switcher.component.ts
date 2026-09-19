import { Component, OnInit } from '@angular/core';
import { CompanyDto, CompanyService } from '../../services/company.service';

@Component({
  selector: 'app-company-switcher',
  template: `
    <div class="company-switcher-container">
      <div class="input-group input-group-sm">
        <span class="input-group-text bg-primary text-white"><i class="fas fa-building me-1"></i> Legal Company</span>
        <select class="form-select" [value]="activeCompanyId" (change)="onCompanyChange($event)">
          <option *ngFor="let company of companies" [value]="company.id">
            {{ company.displayName }} {{ company.evaluationCompany ? '(Evaluation)' : '' }}
          </option>
        </select>
        <button class="btn btn-outline-secondary" (click)="openCopyModal()" title="Copy Company">
          <i class="fas fa-copy"></i> Copy Company
        </button>
      </div>

      <div *ngIf="showModal" class="modal d-block bg-dark bg-opacity-50" tabindex="-1">
        <div class="modal-dialog">
          <div class="modal-content">
            <div class="modal-header">
              <h5 class="modal-title"><i class="fas fa-copy text-primary me-2"></i>Copy Company (Business Central CU 357)</h5>
              <button type="button" class="btn-close" (click)="showModal = false"></button>
            </div>
            <div class="modal-body">
              <p class="text-muted small">Clones standard Chart of Accounts, Dimensions, and Posting Setup into a new company.</p>
              <div class="mb-3">
                <label class="form-label">New Company Code</label>
                <input class="form-control" [(ngModel)]="newCompanyCode" placeholder="CRONUS_DE" />
              </div>
              <div class="mb-3">
                <label class="form-label">Display Name</label>
                <input class="form-control" [(ngModel)]="newDisplayName" placeholder="CRONUS Germany GmbH" />
              </div>
            </div>
            <div class="modal-footer">
              <button class="btn btn-secondary" (click)="showModal = false">Cancel</button>
              <button class="btn btn-primary" (click)="executeCopyCompany()">Copy Company</button>
            </div>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [
    `
      .company-switcher-container { margin-bottom: 1rem; }
    `,
  ],
})
export class CompanySwitcherComponent implements OnInit {
  companies: CompanyDto[] = [];
  activeCompanyId: string | null = null;
  showModal = false;
  newCompanyCode = '';
  newDisplayName = '';

  constructor(private companyService: CompanyService) {}

  ngOnInit(): void {
    this.activeCompanyId = this.companyService.getActiveCompanyId();
    this.companyService.getCompanies().subscribe(list => {
      this.companies = list;
      this.activeCompanyId = this.companyService.ensureValidActiveCompany(list);
    });
  }

  onCompanyChange(event: any): void {
    const selectedId = event.target.value;
    this.activeCompanyId = selectedId;
    // No browser reload: pages react to CompanyService.companyChanged$ and re-query.
    this.companyService.setActiveCompany(selectedId);
  }

  openCopyModal(): void {
    this.showModal = true;
  }

  executeCopyCompany(): void {
    if (!this.activeCompanyId || !this.newCompanyCode) return;
    this.companyService.copyCompany(this.activeCompanyId, this.newCompanyCode, this.newDisplayName).subscribe(() => {
      this.showModal = false;
      this.ngOnInit();
    });
  }
}
