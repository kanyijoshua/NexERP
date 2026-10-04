import { Confirmation, ConfirmationService, ToasterService } from '@abp/ng.theme.shared';
import { Component, DestroyRef, OnInit, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Router } from '@angular/router';
import {
  ConfigPackageDto,
  ConfigPackageFileFormat,
  ConfigPackageService,
} from '@proxy/rapid-start';
import { finalize, switchMap } from 'rxjs/operators';
import { readUploadFile, saveBlob } from '../../erp-shared';
import { CompanyService } from '../../services/company.service';
import { MAX_IMPORT_FILE_BYTES } from '../rapid-start.helpers';

/**
 * RapidStart Configuration Packages list.
 */
@Component({
  selector: 'app-config-packages',
  templateUrl: './config-packages.component.html',
  standalone: false,
})
export class ConfigPackagesComponent implements OnInit {
  private readonly destroyRef = inject(DestroyRef);
  private readonly router = inject(Router);
  private readonly service = inject(ConfigPackageService);
  private readonly companyService = inject(CompanyService);
  private readonly toaster = inject(ToasterService);
  private readonly confirmation = inject(ConfirmationService);

  readonly ConfigPackageFileFormat = ConfigPackageFileFormat;

  packages: ConfigPackageDto[] = [];
  loading = false;
  busy = false;
  searchFilter = '';

  // New package modal state
  isCreateOpen = false;
  isCreateBusy = false;
  newCode = '';
  newPackageName = '';
  newProductVersion = '1.0';

  get canCreate(): boolean {
    return !!this.newCode.trim() && !!this.newPackageName.trim() && !this.isCreateBusy;
  }

  ngOnInit(): void {
    this.load();
    this.companyService.companyChanged$
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(() => this.load());
  }

  load(): void {
    this.loading = true;
    this.service
      .getList({ filter: this.searchFilter.trim() || undefined })
      .pipe(
        finalize(() => (this.loading = false)),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe(result => (this.packages = result.items ?? []));
  }

  openCreate(): void {
    this.newCode = '';
    this.newPackageName = '';
    this.newProductVersion = '1.0';
    this.isCreateOpen = true;
  }

  create(): void {
    if (!this.canCreate) return;

    this.isCreateBusy = true;
    this.service
      .create({
        code: this.newCode.trim().toUpperCase(),
        packageName: this.newPackageName.trim(),
        productVersion: this.newProductVersion.trim() || undefined,
      })
      .pipe(
        finalize(() => (this.isCreateBusy = false)),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe(created => {
        this.isCreateOpen = false;
        this.toaster.success('Erp::SavedSuccessfully');
        this.router.navigate(['/erp/rapid-start/packages', created.id]);
      });
  }

  onImportFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) return;

    if (file.size > MAX_IMPORT_FILE_BYTES) {
      this.toaster.error('Erp::FileTooLarge');
      input.value = '';
      return;
    }

    this.busy = true;
    readUploadFile(file)
      .pipe(
        switchMap(uploadFile => this.service.importPackage(uploadFile)),
        finalize(() => {
          this.busy = false;
          input.value = '';
        }),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe({
        next: () => {
          this.toaster.success('Erp::PackageImported');
          this.load();
        },
        error: () => {
          this.toaster.error('Erp::FileReadFailed');
        },
      });
  }

  exportPackage(pkg: ConfigPackageDto, format: ConfigPackageFileFormat = ConfigPackageFileFormat.Json): void {
    if (!pkg.id) return;
    this.busy = true;
    this.service
      .exportPackage(pkg.id, { format })
      .pipe(
        finalize(() => (this.busy = false)),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe(blob => {
        const ext = format === ConfigPackageFileFormat.Xlsx ? 'xlsx' : 'rapidstart.json';
        saveBlob(blob, `${pkg.code || 'package'}.${ext}`);
      });
  }

  applyPackage(pkg: ConfigPackageDto): void {
    if (!pkg.id) return;
    this.confirmation
      .warn('Erp::ApplyPackageConfirmation', 'Erp::AreYouSure', {
        messageLocalizationParams: [String(pkg.noOfTables || 0)],
      })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(status => {
        if (status !== Confirmation.Status.confirm) return;

        this.busy = true;
        this.service
          .applyPackage(pkg.id!, { tableIds: [] })
          .pipe(
            finalize(() => (this.busy = false)),
            takeUntilDestroyed(this.destroyRef),
          )
          .subscribe(result => {
            this.toaster.success('Erp::PackageAppliedSummary', undefined, {
              messageLocalizationParams: [
                String(result.inserted),
                String(result.modified),
                String(result.errors),
              ],
            });
            this.load();
          });
      });
  }

  deletePackage(pkg: ConfigPackageDto): void {
    if (!pkg.id) return;
    this.confirmation
      .warn('Erp::DeletePackageConfirmation', 'Erp::AreYouSure', {
        messageLocalizationParams: [pkg.code || ''],
      })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(status => {
        if (status !== Confirmation.Status.confirm) return;

        this.busy = true;
        this.service
          .delete(pkg.id!)
          .pipe(
            finalize(() => (this.busy = false)),
            takeUntilDestroyed(this.destroyRef),
          )
          .subscribe(() => {
            this.toaster.success('Erp::DeletedSuccessfully');
            this.load();
          });
      });
  }
}
