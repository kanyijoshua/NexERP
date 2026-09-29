import { CoreModule } from '@abp/ng.core';
import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { CompanyDto, CompanyService } from '../../services/company.service';

/** Copy Company (Business Central CU 357): a new company set up like the source one. */
@Component({
  selector: 'app-copy-company-dialog',
  template: `
    <h2 mat-dialog-title>{{ 'Erp::CopyCompany' | abpLocalization }}</h2>
    <mat-dialog-content>
      <p class="text-muted small">
        {{ 'Erp::CopyCompanyDescription' | abpLocalization: source.displayName }}
      </p>
      <mat-form-field appearance="outline" class="w-100">
        <mat-label>{{ 'Erp::NewCompanyName' | abpLocalization }}</mat-label>
        <input matInput required [(ngModel)]="name" placeholder="CRONUS_DE" />
      </mat-form-field>
      <mat-form-field appearance="outline" class="w-100">
        <mat-label>{{ 'Erp::DisplayName' | abpLocalization }}</mat-label>
        <input matInput [(ngModel)]="displayName" placeholder="CRONUS Germany GmbH" />
      </mat-form-field>
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button mat-dialog-close type="button">{{ 'AbpUi::Cancel' | abpLocalization }}</button>
      <button mat-flat-button type="button" [disabled]="!name.trim() || busy" (click)="copy()">
        {{ 'Erp::CopyCompany' | abpLocalization }}
      </button>
    </mat-dialog-actions>
  `,
  imports: [CoreModule, FormsModule, MatButtonModule, MatDialogModule, MatFormFieldModule, MatInputModule],
})
export class CopyCompanyDialogComponent {
  private readonly companies = inject(CompanyService);
  private readonly ref = inject(MatDialogRef<CopyCompanyDialogComponent, CompanyDto>);

  readonly source: CompanyDto = inject(MAT_DIALOG_DATA);

  name = '';
  displayName = '';
  busy = false;

  copy(): void {
    this.busy = true;
    this.companies.copyCompany(this.source.id, this.name.trim(), this.displayName.trim()).subscribe({
      next: company => this.ref.close(company),
      error: () => (this.busy = false),
    });
  }
}
