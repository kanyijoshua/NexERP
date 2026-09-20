import { ToasterService } from '@abp/ng.theme.shared';
import { Component, DestroyRef, OnInit, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder, FormGroup } from '@angular/forms';
import { NoSeriesDto, NoSeriesService } from '@proxy/numbering';
import { PurchaseSetupService } from '@proxy/purchasing';
import { SalesSetupService } from '@proxy/sales';
import { finalize, forkJoin } from 'rxjs';
import { CompanyService } from '../../services/company.service';

interface SeriesField {
  control: string;
  labelKey: string;
}

/**
 * Number series per kind of record. Mirrors the "Number Series" tabs of Business Central's
 * Sales &amp; Receivables Setup (page 459) and Purchases &amp; Payables Setup (page 460).
 */
@Component({
  selector: 'app-document-setup',
  templateUrl: './document-setup.component.html',
})
export class DocumentSetupComponent implements OnInit {
  private readonly destroyRef = inject(DestroyRef);

  readonly salesFields: SeriesField[] = [
    { control: 'customerNos', labelKey: 'Erp::CustomerNos' },
    { control: 'quoteNos', labelKey: 'Erp::QuoteNos' },
    { control: 'orderNos', labelKey: 'Erp::OrderNos' },
    { control: 'invoiceNos', labelKey: 'Erp::InvoiceNos' },
    { control: 'creditMemoNos', labelKey: 'Erp::CreditMemoNos' },
    { control: 'postedInvoiceNos', labelKey: 'Erp::PostedInvoiceNos' },
    { control: 'postedCreditMemoNos', labelKey: 'Erp::PostedCreditMemoNos' },
  ];

  readonly purchaseFields: SeriesField[] = [
    { control: 'vendorNos', labelKey: 'Erp::VendorNos' },
    ...this.salesFields.slice(1),
  ];

  series: NoSeriesDto[] = [];
  salesForm: FormGroup = this.buildForm(this.salesFields);
  purchaseForm: FormGroup = this.buildForm(this.purchaseFields);
  loading = false;
  savingSales = false;
  savingPurchase = false;

  constructor(
    private readonly fb: FormBuilder,
    private readonly noSeriesService: NoSeriesService,
    private readonly salesSetupService: SalesSetupService,
    private readonly purchaseSetupService: PurchaseSetupService,
    private readonly companyService: CompanyService,
    private readonly toaster: ToasterService,
  ) {}

  ngOnInit(): void {
    this.load();
    this.companyService.companyChanged$
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(() => this.load());
  }

  /** "SI-00012" next to the chosen series, so the effect of a choice is visible at once. */
  nextNo(code: string | null): string | null {
    return this.series.find(s => s.code === code)?.nextNo ?? null;
  }

  saveSales(): void {
    this.savingSales = true;
    this.salesSetupService
      .update(this.blankToNull(this.salesForm))
      .pipe(
        finalize(() => (this.savingSales = false)),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe(setup => {
        this.salesForm.reset(setup);
        this.toaster.success('Erp::SavedSuccessfully');
      });
  }

  savePurchase(): void {
    this.savingPurchase = true;
    this.purchaseSetupService
      .update(this.blankToNull(this.purchaseForm))
      .pipe(
        finalize(() => (this.savingPurchase = false)),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe(setup => {
        this.purchaseForm.reset(setup);
        this.toaster.success('Erp::SavedSuccessfully');
      });
  }

  private load(): void {
    this.loading = true;
    forkJoin({
      series: this.noSeriesService.getList({ maxResultCount: 1000, skipCount: 0 } as never),
      sales: this.salesSetupService.get(),
      purchase: this.purchaseSetupService.get(),
    })
      .pipe(
        finalize(() => (this.loading = false)),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe(({ series, sales, purchase }) => {
        this.series = series.items ?? [];
        this.salesForm.reset(sales);
        this.purchaseForm.reset(purchase);
      });
  }

  private buildForm(fields: SeriesField[]): FormGroup {
    const group = this.fb.group({});
    fields.forEach(field => group.addControl(field.control, this.fb.control<string | null>(null)));
    return group;
  }

  // The "(none)" option is an empty string; the API expects null for "numbered by hand".
  private blankToNull<T>(form: FormGroup): T {
    const value = form.getRawValue() as Record<string, unknown>;
    const result: Record<string, unknown> = {};
    Object.keys(value).forEach(key => (result[key] = value[key] === '' ? null : value[key]));
    return result as T;
  }
}
