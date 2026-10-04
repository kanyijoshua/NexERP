import { ABP, ListService, PagedResultDto } from '@abp/ng.core';
import { Confirmation, ConfirmationService, ToasterService } from '@abp/ng.theme.shared';
import { Component, DestroyRef, OnInit, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import {
  CreateJobQueueEntryDto,
  CreateUpdateJobQueueCategoryDto,
  JobQueueCategoryDto,
  JobQueueEntryDto,
  JobQueueIntervalType,
  JobQueueLogEntryDto,
  JobQueueLogStatus,
  JobQueueService,
  JobQueueStatus,
  JobTypeInfoDto,
  UpdateJobQueueEntryDto,
} from '@proxy/job-queue';
import { ErpTableColumn } from '../../erp-shared';

type ActiveTab = 'entries' | 'categories' | 'logs';

@Component({
  selector: 'app-job-queue',
  templateUrl: './job-queue.component.html',
  styleUrls: ['./job-queue.component.scss'],
  providers: [ListService],
  standalone: false,
})
export class JobQueueComponent implements OnInit {
  private readonly jobQueueService = inject(JobQueueService);
  private readonly fb = inject(FormBuilder);
  private readonly toaster = inject(ToasterService);
  private readonly confirmation = inject(ConfirmationService);
  private readonly destroyRef = inject(DestroyRef);
  readonly list = inject(ListService);

  readonly JobQueueStatus = JobQueueStatus;
  readonly JobQueueLogStatus = JobQueueLogStatus;
  readonly JobQueueIntervalType = JobQueueIntervalType;

  activeTab: ActiveTab = 'entries';

  // Entries
  entriesData: PagedResultDto<JobQueueEntryDto> = { items: [], totalCount: 0 };
  selectedEntry: JobQueueEntryDto | null = null;
  isEntryModalOpen = false;
  entryForm!: FormGroup;
  selectedCategoryFilter: string | null = null;
  selectedStatusFilter: JobQueueStatus | null = null;
  entrySearchFilter = '';

  // Categories
  categories: JobQueueCategoryDto[] = [];
  selectedCategory: JobQueueCategoryDto | null = null;
  isCategoryModalOpen = false;
  categoryForm!: FormGroup;

  // Logs
  logsData: PagedResultDto<JobQueueLogEntryDto> = { items: [], totalCount: 0 };
  selectedLogEntryFilter: string | null = null;
  selectedLogStatusFilter: JobQueueLogStatus | null = null;
  selectedLogDetail: JobQueueLogEntryDto | null = null;
  isLogDetailModalOpen = false;

  // Error dialog
  activeErrorEntry: JobQueueEntryDto | null = null;
  isErrorModalOpen = false;

  // Metadata
  availableJobTypes: JobTypeInfoDto[] = [];
  isBusy = false;

  readonly entryColumns: ErpTableColumn<JobQueueEntryDto>[] = [
    { field: 'status', labelKey: 'Erp::Status', width: 130 },
    { field: 'description', labelKey: 'Erp::Description', width: 220 },
    { field: 'jobType', labelKey: 'Erp::JobType', width: 170 },
    { field: 'categoryCode', labelKey: 'Erp::CategoryCode', width: 120 },
    { field: 'userId', labelKey: 'Erp::UserId', width: 120 },
    { field: 'recordIdToProcess', labelKey: 'Erp::RecordIdToProcess', width: 150 },
    { field: 'nextRunDateFormula', labelKey: 'Erp::NextRunDateFormula', width: 140 },
    { field: 'scheduled', labelKey: 'Erp::Scheduled', width: 100, type: 'boolean' },
    { field: 'notifyOnSuccess', labelKey: 'Erp::NotifyOnSuccess', width: 120, type: 'boolean' },
    { field: 'priority', labelKey: 'Erp::Priority', width: 90, type: 'number' },
    { field: 'nextRunTime', labelKey: 'Erp::NextRunTime', width: 160 },
    { field: 'lastRunTime', labelKey: 'Erp::LastRunTime', width: 160 },
    { field: 'noOfAttemptsToRun', labelKey: 'Erp::Attempts', width: 100 },
  ];

  readonly logColumns: ErpTableColumn<JobQueueLogEntryDto>[] = [
    { field: 'status', labelKey: 'Erp::Status', width: 120 },
    { field: 'jobType', labelKey: 'Erp::JobType', width: 160 },
    { field: 'description', labelKey: 'Erp::Description', width: 220 },
    { field: 'categoryCode', labelKey: 'Erp::CategoryCode', width: 120 },
    { field: 'userId', labelKey: 'Erp::UserId', width: 120 },
    { field: 'startDateTime', labelKey: 'Erp::StartTime', width: 160 },
    { field: 'durationMs', labelKey: 'Erp::Duration', width: 110, type: 'number' },
    { field: 'processedRecords', labelKey: 'Erp::ProcessedRecords', width: 130, type: 'number' },
  ];

  ngOnInit(): void {
    this.loadAvailableJobTypes();
    this.loadCategories();
    this.loadEntries();
  }

  setTab(tab: ActiveTab): void {
    this.activeTab = tab;
    if (tab === 'entries') {
      this.loadEntries();
    } else if (tab === 'categories') {
      this.loadCategories();
    } else if (tab === 'logs') {
      this.loadLogs();
    }
  }

  // --- Handlers & Categories Loading ---
  loadAvailableJobTypes(): void {
    this.jobQueueService
      .getAvailableJobTypes()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(result => {
        this.availableJobTypes = result.items ?? [];
      });
  }

  loadCategories(): void {
    this.jobQueueService
      .getCategories()
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(result => {
        this.categories = result.items ?? [];
      });
  }

  // --- Entries Management ---
  loadEntries(): void {
    this.isBusy = true;
    this.jobQueueService
      .getList({
        maxResultCount: 50,
        skipCount: 0,
        categoryCode: this.selectedCategoryFilter || undefined,
        status: this.selectedStatusFilter !== null ? this.selectedStatusFilter : undefined,
        filter: this.entrySearchFilter || undefined,
        sorting: 'NextRunTime asc',
      })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: result => {
          this.entriesData = result;
          this.isBusy = false;
        },
        error: () => (this.isBusy = false),
      });
  }

  openCreateEntry(): void {
    this.selectedEntry = null;
    this.buildEntryForm();
    this.isEntryModalOpen = true;
  }

  openEditEntry(entry: JobQueueEntryDto): void {
    this.selectedEntry = entry;
    this.buildEntryForm(entry);
    this.isEntryModalOpen = true;
  }

  buildEntryForm(item?: JobQueueEntryDto): void {
    this.entryForm = this.fb.group({
      description: [item?.description ?? '', [Validators.required, Validators.maxLength(250)]],
      categoryCode: [item?.categoryCode ?? ''],
      jobType: [item?.jobType ?? (this.availableJobTypes[0]?.jobType || ''), Validators.required],
      parameterString: [item?.parameterString ?? ''],
      userId: [item?.userId ?? ''],
      recordIdToProcess: [item?.recordIdToProcess ?? ''],
      priority: [item?.priority ?? 5, [Validators.required, Validators.min(1), Validators.max(10)]],
      recurringJob: [item?.recurringJob ?? true],
      nextRunDateFormula: [item?.nextRunDateFormula ?? ''],
      referenceStartingTime: [item?.referenceStartingTime ? item.referenceStartingTime.substring(0, 16) : ''],
      intervalType: [item?.intervalType ?? JobQueueIntervalType.Minutes, Validators.required],
      intervalMinutes: [item?.intervalMinutes ?? 60, [Validators.required, Validators.min(1)]],
      runOnMondays: [item?.runOnMondays ?? true],
      runOnTuesdays: [item?.runOnTuesdays ?? true],
      runOnWednesdays: [item?.runOnWednesdays ?? true],
      runOnThursdays: [item?.runOnThursdays ?? true],
      runOnFridays: [item?.runOnFridays ?? true],
      runOnSaturdays: [item?.runOnSaturdays ?? false],
      runOnSundays: [item?.runOnSundays ?? false],
      dailyStartingTime: [item?.dailyStartingTime ?? ''],
      dailyEndingTime: [item?.dailyEndingTime ?? ''],
      maxNoOfAttemptsToRun: [item?.maxNoOfAttemptsToRun ?? 3, [Validators.required, Validators.min(0)]],
      rerunDelaySeconds: [item?.rerunDelaySeconds ?? 60, [Validators.required, Validators.min(0)]],
      timeoutSeconds: [item?.timeoutSeconds ?? 300, [Validators.required, Validators.min(10)]],
      notifyOnSuccess: [item?.notifyOnSuccess ?? false],
      manualRecurrence: [item?.manualRecurrence ?? false],
      inactivityTimeoutPeriod: [item?.inactivityTimeoutPeriod ?? null],
    });
  }

  onJobTypeChanged(jobType: string): void {
    const handler = this.availableJobTypes.find(h => h.jobType === jobType);
    if (handler && !this.entryForm.get('parameterString')?.value && handler.defaultParametersJson) {
      this.entryForm.patchValue({ parameterString: handler.defaultParametersJson });
    }
    if (handler && !this.selectedEntry && !this.entryForm.get('description')?.value) {
      this.entryForm.patchValue({ description: handler.displayName });
    }
  }

  fillDefaultParameters(): void {
    const jobType = this.entryForm.get('jobType')?.value;
    const handler = this.availableJobTypes.find(h => h.jobType === jobType);
    if (handler && handler.defaultParametersJson) {
      this.entryForm.patchValue({ parameterString: handler.defaultParametersJson });
    }
  }

  setWeekdaysOnly(): void {
    this.entryForm.patchValue({
      runOnMondays: true,
      runOnTuesdays: true,
      runOnWednesdays: true,
      runOnThursdays: true,
      runOnFridays: true,
      runOnSaturdays: false,
      runOnSundays: false,
    });
  }

  setAllDays(): void {
    this.entryForm.patchValue({
      runOnMondays: true,
      runOnTuesdays: true,
      runOnWednesdays: true,
      runOnThursdays: true,
      runOnFridays: true,
      runOnSaturdays: true,
      runOnSundays: true,
    });
  }

  saveEntry(): void {
    if (this.entryForm.invalid) {
      this.entryForm.markAllAsTouched();
      return;
    }

    this.isBusy = true;
    const formValue = this.entryForm.value;

    const dto: CreateJobQueueEntryDto | UpdateJobQueueEntryDto = {
      description: formValue.description,
      categoryCode: formValue.categoryCode || undefined,
      jobType: formValue.jobType,
      parameterString: formValue.parameterString || undefined,
      userId: formValue.userId || undefined,
      recordIdToProcess: formValue.recordIdToProcess || undefined,
      priority: Number(formValue.priority),
      recurringJob: Boolean(formValue.recurringJob),
      nextRunDateFormula: formValue.nextRunDateFormula || undefined,
      referenceStartingTime: this.safeToIso(formValue.referenceStartingTime),
      intervalType: Number(formValue.intervalType),
      intervalMinutes: Number(formValue.intervalMinutes),
      runOnMondays: Boolean(formValue.runOnMondays),
      runOnTuesdays: Boolean(formValue.runOnTuesdays),
      runOnWednesdays: Boolean(formValue.runOnWednesdays),
      runOnThursdays: Boolean(formValue.runOnThursdays),
      runOnFridays: Boolean(formValue.runOnFridays),
      runOnSaturdays: Boolean(formValue.runOnSaturdays),
      runOnSundays: Boolean(formValue.runOnSundays),
      dailyStartingTime: formValue.dailyStartingTime || undefined,
      dailyEndingTime: formValue.dailyEndingTime || undefined,
      maxNoOfAttemptsToRun: Number(formValue.maxNoOfAttemptsToRun),
      rerunDelaySeconds: Number(formValue.rerunDelaySeconds),
      timeoutSeconds: Number(formValue.timeoutSeconds),
      notifyOnSuccess: Boolean(formValue.notifyOnSuccess),
      manualRecurrence: Boolean(formValue.manualRecurrence),
      inactivityTimeoutPeriod: formValue.inactivityTimeoutPeriod ? Number(formValue.inactivityTimeoutPeriod) : undefined,
    };

    const action$ = this.selectedEntry
      ? this.jobQueueService.update(this.selectedEntry.id, dto)
      : this.jobQueueService.create(dto);

    action$.pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: () => {
        this.isBusy = false;
        this.isEntryModalOpen = false;
        this.toaster.success('Erp::SavedSuccessfully');
        this.loadEntries();
      },
      error: () => (this.isBusy = false),
    });
  }

  private safeToIso(val?: string | null): string | undefined {
    if (!val) return undefined;
    const d = new Date(val);
    return isNaN(d.getTime()) ? undefined : d.toISOString();
  }

  deleteEntry(entry: JobQueueEntryDto): void {
    this.confirmation
      .warn('Erp::ItemWillBeDeletedMessage', 'Erp::AreYouSure')
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(status => {
        if (status === Confirmation.Status.confirm) {
          this.isBusy = true;
          this.jobQueueService
            .delete(entry.id)
            .pipe(takeUntilDestroyed(this.destroyRef))
            .subscribe({
              next: () => {
                this.isBusy = false;
                this.toaster.success('Erp::DeletedSuccessfully');
                this.loadEntries();
              },
              error: () => (this.isBusy = false),
            });
        }
      });
  }

  // --- Entry State Machine Actions ---
  runOnce(entry: JobQueueEntryDto): void {
    this.isBusy = true;
    this.jobQueueService
      .runOnce(entry.id)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: result => {
          this.isBusy = false;
          if (result.success) {
            this.toaster.success(
              result.outputDetails || `Job completed successfully (${result.durationMs}ms, ${result.processedRecords} records)`
            );
          } else {
            this.toaster.error(result.errorMessage || 'Job execution failed');
          }
          this.loadEntries();
        },
        error: () => (this.isBusy = false),
      });
  }

  setStatusReady(entry: JobQueueEntryDto): void {
    this.isBusy = true;
    this.jobQueueService
      .setStatusReady(entry.id)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.isBusy = false;
          this.toaster.success('Erp::SavedSuccessfully');
          this.loadEntries();
        },
        error: () => (this.isBusy = false),
      });
  }

  setStatusOnHold(entry: JobQueueEntryDto): void {
    this.isBusy = true;
    this.jobQueueService
      .setStatusOnHold(entry.id)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.isBusy = false;
          this.toaster.success('Erp::SavedSuccessfully');
          this.loadEntries();
        },
        error: () => (this.isBusy = false),
      });
  }

  restartEntry(entry: JobQueueEntryDto): void {
    this.isBusy = true;
    this.jobQueueService
      .restart(entry.id)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.isBusy = false;
          this.toaster.success('Erp::SavedSuccessfully');
          this.loadEntries();
        },
        error: () => (this.isBusy = false),
      });
  }

  showError(entry: JobQueueEntryDto): void {
    this.activeErrorEntry = entry;
    this.isErrorModalOpen = true;
  }

  viewEntryLogs(entry: JobQueueEntryDto): void {
    this.selectedLogEntryFilter = entry.id;
    this.setTab('logs');
  }

  // --- Category CRUD ---
  openCreateCategory(): void {
    this.selectedCategory = null;
    this.categoryForm = this.fb.group({
      code: ['', [Validators.required, Validators.maxLength(20)]],
      description: ['', [Validators.maxLength(100)]],
    });
    this.isCategoryModalOpen = true;
  }

  openEditCategory(category: JobQueueCategoryDto): void {
    this.selectedCategory = category;
    this.categoryForm = this.fb.group({
      code: [{ value: category.code, disabled: true }, [Validators.required, Validators.maxLength(20)]],
      description: [category.description ?? '', [Validators.maxLength(100)]],
    });
    this.isCategoryModalOpen = true;
  }

  saveCategory(): void {
    if (this.categoryForm.invalid) {
      this.categoryForm.markAllAsTouched();
      return;
    }

    const value = this.categoryForm.getRawValue();
    const dto: CreateUpdateJobQueueCategoryDto = {
      code: value.code.toUpperCase(),
      description: value.description,
    };

    const action$ = this.selectedCategory
      ? this.jobQueueService.updateCategory(this.selectedCategory.id, dto)
      : this.jobQueueService.createCategory(dto);

    this.isBusy = true;
    action$.pipe(takeUntilDestroyed(this.destroyRef)).subscribe({
      next: () => {
        this.isBusy = false;
        this.isCategoryModalOpen = false;
        this.toaster.success('Erp::SavedSuccessfully');
        this.loadCategories();
      },
      error: () => (this.isBusy = false),
    });
  }

  deleteCategory(category: JobQueueCategoryDto): void {
    this.confirmation
      .warn('Erp::ItemWillBeDeletedMessage', 'Erp::AreYouSure')
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(status => {
        if (status === Confirmation.Status.confirm) {
          this.isBusy = true;
          this.jobQueueService
            .deleteCategory(category.id)
            .pipe(takeUntilDestroyed(this.destroyRef))
            .subscribe({
              next: () => {
                this.isBusy = false;
                this.toaster.success('Erp::DeletedSuccessfully');
                this.loadCategories();
              },
              error: () => (this.isBusy = false),
            });
        }
      });
  }

  // --- Logs Management ---
  loadLogs(): void {
    this.isBusy = true;
    this.jobQueueService
      .getLogs({
        maxResultCount: 50,
        skipCount: 0,
        jobQueueEntryId: this.selectedLogEntryFilter || undefined,
        status: this.selectedLogStatusFilter !== null ? this.selectedLogStatusFilter : undefined,
        sorting: 'StartDateTime desc',
      })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: result => {
          this.logsData = result;
          this.isBusy = false;
        },
        error: () => (this.isBusy = false),
      });
  }

  openLogDetail(log: JobQueueLogEntryDto): void {
    this.selectedLogDetail = log;
    this.isLogDetailModalOpen = true;
  }

  clearLogs(): void {
    this.confirmation
      .warn('Erp::AreYouSureToClearLogs', 'Erp::AreYouSure')
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(status => {
        if (status === Confirmation.Status.confirm) {
          this.isBusy = true;
          this.jobQueueService
            .clearLogs(this.selectedLogEntryFilter || undefined)
            .pipe(takeUntilDestroyed(this.destroyRef))
            .subscribe({
              next: () => {
                this.isBusy = false;
                this.toaster.success('Erp::LogsClearedSuccessfully');
                this.loadLogs();
              },
              error: () => (this.isBusy = false),
            });
        }
      });
  }

  // --- UI Helpers ---
  getStatusBadgeClass(status: JobQueueStatus): string {
    switch (status) {
      case JobQueueStatus.Ready:
        return 'badge bg-primary';
      case JobQueueStatus.InProcess:
        return 'badge bg-warning text-dark';
      case JobQueueStatus.Error:
        return 'badge bg-danger';
      case JobQueueStatus.OnHold:
        return 'badge bg-secondary';
      case JobQueueStatus.Finished:
        return 'badge bg-success';
      default:
        return 'badge bg-light text-dark';
    }
  }

  getStatusIcon(status: JobQueueStatus): string {
    switch (status) {
      case JobQueueStatus.Ready:
        return 'fas fa-clock';
      case JobQueueStatus.InProcess:
        return 'fas fa-spinner fa-spin';
      case JobQueueStatus.Error:
        return 'fas fa-exclamation-triangle';
      case JobQueueStatus.OnHold:
        return 'fas fa-pause';
      case JobQueueStatus.Finished:
        return 'fas fa-check-circle';
      default:
        return 'fas fa-question';
    }
  }

  getLogStatusBadgeClass(status: JobQueueLogStatus): string {
    switch (status) {
      case JobQueueLogStatus.Success:
        return 'badge bg-success';
      case JobQueueLogStatus.Error:
        return 'badge bg-danger';
      case JobQueueLogStatus.InProcess:
        return 'badge bg-warning text-dark';
      default:
        return 'badge bg-secondary';
    }
  }

  getLogStatusIcon(status: JobQueueLogStatus): string {
    switch (status) {
      case JobQueueLogStatus.Success:
        return 'fas fa-check-circle';
      case JobQueueLogStatus.Error:
        return 'fas fa-times-circle';
      case JobQueueLogStatus.InProcess:
        return 'fas fa-spinner fa-spin';
      default:
        return 'fas fa-info-circle';
    }
  }

  formatInterval(entry: JobQueueEntryDto): string {
    if (!entry.recurringJob) {
      return 'Run once';
    }
    const typeStr =
      entry.intervalType === JobQueueIntervalType.Minutes
        ? 'min'
        : entry.intervalType === JobQueueIntervalType.Hours
        ? 'hrs'
        : entry.intervalType === JobQueueIntervalType.Days
        ? 'days'
        : 'wks';
    return `Every ${entry.intervalMinutes} ${typeStr}`;
  }

  formatDuration(durationMs: number): string {
    if (durationMs < 1000) {
      return `${durationMs} ms`;
    }
    return `${(durationMs / 1000).toFixed(2)} s`;
  }
}
