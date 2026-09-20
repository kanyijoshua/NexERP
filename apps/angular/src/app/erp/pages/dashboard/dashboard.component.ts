import { Component } from '@angular/core';

@Component({
  selector: 'app-erp-dashboard',
  template: `
    <div class="container-fluid py-3">
      <app-company-switcher></app-company-switcher>

      <div class="row g-3 mb-4">
        <div class="col-md-3">
          <div class="card border-0 shadow-sm bg-gradient text-white bg-primary p-3 rounded-3">
            <div class="d-flex justify-content-between align-items-center">
              <div>
                <h6 class="text-white-50 text-uppercase mb-1 small fw-bold">Open Sales Invoices</h6>
                <h3 class="mb-0 fw-bold">12</h3>
              </div>
              <i class="fas fa-file-invoice-dollar fa-2x opacity-75"></i>
            </div>
          </div>
        </div>

        <div class="col-md-3">
          <div class="card border-0 shadow-sm bg-gradient text-white bg-success p-3 rounded-3">
            <div class="d-flex justify-content-between align-items-center">
              <div>
                <h6 class="text-white-50 text-uppercase mb-1 small fw-bold">Overdue Receivables</h6>
                <h3 class="mb-0 fw-bold">$42,500.00</h3>
              </div>
              <i class="fas fa-hand-holding-usd fa-2x opacity-75"></i>
            </div>
          </div>
        </div>

        <div class="col-md-3">
          <div class="card border-0 shadow-sm bg-gradient text-white bg-warning p-3 rounded-3">
            <div class="d-flex justify-content-between align-items-center">
              <div>
                <h6 class="text-white-50 text-uppercase mb-1 small fw-bold">Pending Approvals</h6>
                <h3 class="mb-0 fw-bold">3</h3>
              </div>
              <i class="fas fa-user-clock fa-2x opacity-75"></i>
            </div>
          </div>
        </div>

        <div class="col-md-3">
          <div class="card border-0 shadow-sm bg-gradient text-white bg-dark p-3 rounded-3">
            <div class="d-flex justify-content-between align-items-center">
              <div>
                <h6 class="text-white-50 text-uppercase mb-1 small fw-bold">Inventory Valuation</h6>
                <h3 class="mb-0 fw-bold">$184,200.00</h3>
              </div>
              <i class="fas fa-boxes fa-2x opacity-75"></i>
            </div>
          </div>
        </div>
      </div>

      <div class="row g-3">
        <div class="col-md-8">
          <div class="card border-0 shadow-sm p-3">
            <h5 class="fw-bold mb-3">
              <i class="fas fa-chart-area text-primary me-2"></i>Financial Overview & Cash Flow
            </h5>
            <div class="alert alert-info small mb-0">
              <i class="fas fa-info-circle me-1"></i> Running standard Business Central Chart of
              Accounts posting engine with double-entry balance verification.
            </div>
          </div>
        </div>
        <div class="col-md-4">
          <app-chatter-widget
            entityType="System"
            entityId="00000000-0000-0000-0000-000000000000"
            entityNo="SYS-001"
          ></app-chatter-widget>
        </div>
      </div>
    </div>
  `,
})
export class DashboardComponent {}
