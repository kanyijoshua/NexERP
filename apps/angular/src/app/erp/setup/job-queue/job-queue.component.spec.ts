import { ListService } from '@abp/ng.core';
import { Confirmation, ConfirmationService, ToasterService } from '@abp/ng.theme.shared';
import { TestBed } from '@angular/core/testing';
import { FormBuilder } from '@angular/forms';
import {
  JobQueueIntervalType,
  JobQueueLogStatus,
  JobQueueService,
  JobQueueStatus,
} from '@proxy/job-queue';
import { of } from 'rxjs';
import { JobQueueComponent } from './job-queue.component';

describe('JobQueueComponent', () => {
  let service: jasmine.SpyObj<JobQueueService>;
  let component: JobQueueComponent;
  let toaster: jasmine.SpyObj<ToasterService>;
  let confirmation: jasmine.SpyObj<ConfirmationService>;

  const mockJobTypes = [
    {
      jobType: 'CleanupJobQueueLogs',
      displayName: 'Job Queue Log Cleanup',
      description: 'Deletes historic logs',
      defaultParametersJson: '{"retentionDays": 30}',
    },
    {
      jobType: 'SalesPostBatch',
      displayName: 'Batch Post Sales Orders',
      description: 'Posts pending sales orders in background',
      defaultParametersJson: '{"batchSize": 50}',
    },
  ];

  const mockCategory = { id: 'c1', code: 'SYSTEM', description: 'System tasks' };

  const mockEntry = {
    id: 'e1',
    description: 'Daily Cleanup',
    categoryCode: 'SYSTEM',
    jobType: 'CleanupJobQueueLogs',
    parameterString: '{"retentionDays": 30}',
    status: JobQueueStatus.Ready,
    recurringJob: true,
    intervalType: JobQueueIntervalType.Days,
    intervalMinutes: 1,
    priority: 5,
    noOfAttemptsToRun: 0,
    maxNoOfAttemptsToRun: 3,
    rerunDelaySeconds: 60,
    timeoutSeconds: 300,
    runOnMondays: true,
    runOnTuesdays: true,
    runOnWednesdays: true,
    runOnThursdays: true,
    runOnFridays: true,
    runOnSaturdays: false,
    runOnSundays: false,
    userId: 'TEST_ADMIN',
    recordIdToProcess: 'Customer: 10000',
    nextRunDateFormula: '1D',
    referenceStartingTime: '2026-10-03T08:00:00Z',
    scheduled: true,
    manualRecurrence: false,
    inactivityTimeoutPeriod: 60,
    notifyOnSuccess: true,
    lastErrorMessage: 'Previous timeout failure',
    lastErrorStackTrace: 'StackTrace details...',
  };

  const mockLog = {
    id: 'l1',
    jobQueueEntryId: 'e1',
    jobDescription: 'Daily Cleanup',
    jobType: 'CleanupJobQueueLogs',
    userId: 'TEST_ADMIN',
    categoryCode: 'SYSTEM',
    parameterString: '{"retentionDays": 30}',
    systemTaskId: '00000000-0000-0000-0000-000000000001',
    startDateTime: '2026-10-03T10:00:00Z',
    endDateTime: '2026-10-03T10:00:05Z',
    durationMs: 5200,
    status: JobQueueLogStatus.Success,
    processedRecords: 42,
    outputDetails: 'Pruned 42 rows',
  };

  beforeEach(() => {
    service = jasmine.createSpyObj<JobQueueService>('JobQueueService', [
      'getCategories',
      'createCategory',
      'updateCategory',
      'deleteCategory',
      'getList',
      'get',
      'create',
      'update',
      'delete',
      'setStatusReady',
      'setStatusOnHold',
      'restart',
      'runOnce',
      'getAvailableJobTypes',
      'getLogs',
      'clearLogs',
    ]);

    service.getAvailableJobTypes.and.returnValue(of({ items: mockJobTypes }) as never);
    service.getCategories.and.returnValue(of({ items: [mockCategory] }) as never);
    service.getList.and.returnValue(of({ items: [mockEntry], totalCount: 1 }) as never);
    service.getLogs.and.returnValue(of({ items: [mockLog], totalCount: 1 }) as never);
    service.create.and.returnValue(of({ id: 'e2' }) as never);
    service.update.and.returnValue(of({ id: 'e1' }) as never);
    service.delete.and.returnValue(of(undefined) as never);
    service.createCategory.and.returnValue(of({ id: 'c2' }) as never);
    service.updateCategory.and.returnValue(of({ id: 'c1' }) as never);
    service.deleteCategory.and.returnValue(of(undefined) as never);
    service.clearLogs.and.returnValue(of(undefined) as never);
    service.setStatusReady.and.returnValue(of({}) as never);
    service.setStatusOnHold.and.returnValue(of({}) as never);
    service.restart.and.returnValue(of({}) as never);
    service.runOnce.and.returnValue(
      of({
        jobQueueEntryId: 'e1',
        logEntryId: 'l1',
        success: true,
        newStatus: JobQueueStatus.Ready,
        processedRecords: 5,
        durationMs: 120,
        outputDetails: 'Cleanup completed',
      }) as never
    );

    toaster = jasmine.createSpyObj('ToasterService', ['success', 'error', 'info', 'warn']);
    confirmation = jasmine.createSpyObj('ConfirmationService', ['warn']);

    TestBed.configureTestingModule({
      providers: [
        JobQueueComponent,
        ListService,
        FormBuilder,
        { provide: JobQueueService, useValue: service },
        { provide: ToasterService, useValue: toaster },
        { provide: ConfirmationService, useValue: confirmation },
      ],
    });

    component = TestBed.inject(JobQueueComponent);
    component.ngOnInit();
  });

  // --- Initial Loading & Tabs ---
  it('loads available job types, categories, and entries on init', () => {
    expect(service.getAvailableJobTypes).toHaveBeenCalled();
    expect(service.getCategories).toHaveBeenCalled();
    expect(service.getList).toHaveBeenCalled();
    expect(component.availableJobTypes.length).toBe(2);
    expect(component.categories.length).toBe(1);
    expect(component.entriesData.items?.length).toBe(1);
    expect(component.activeTab).toBe('entries');
  });

  it('switches tabs and triggers corresponding data loads', () => {
    component.setTab('categories');
    expect(component.activeTab).toBe('categories');
    expect(service.getCategories).toHaveBeenCalledTimes(2);

    component.setTab('logs');
    expect(component.activeTab).toBe('logs');
    expect(service.getLogs).toHaveBeenCalled();

    component.setTab('entries');
    expect(component.activeTab).toBe('entries');
    expect(service.getList).toHaveBeenCalledTimes(2);
  });

  // --- Form Reactions & Presets ---
  it('opens create modal with default values', () => {
    component.openCreateEntry();
    expect(component.isEntryModalOpen).toBeTrue();
    expect(component.selectedEntry).toBeNull();
    expect(component.entryForm.get('priority')?.value).toBe(5);
    expect(component.entryForm.get('recurringJob')?.value).toBeTrue();
    expect(component.entryForm.get('intervalType')?.value).toBe(JobQueueIntervalType.Minutes);
    expect(component.entryForm.get('intervalMinutes')?.value).toBe(60);
    expect(component.entryForm.get('runOnMondays')?.value).toBeTrue();
    expect(component.entryForm.get('runOnSaturdays')?.value).toBeFalse();
  });

  it('opens edit modal and populates form from existing entry', () => {
    component.openEditEntry(mockEntry);
    expect(component.isEntryModalOpen).toBeTrue();
    expect(component.selectedEntry).toBe(mockEntry);
    expect(component.entryForm.get('description')?.value).toBe('Daily Cleanup');
    expect(component.entryForm.get('categoryCode')?.value).toBe('SYSTEM');
    expect(component.entryForm.get('jobType')?.value).toBe('CleanupJobQueueLogs');
    expect(component.entryForm.get('userId')?.value).toBe('TEST_ADMIN');
    expect(component.entryForm.get('recordIdToProcess')?.value).toBe('Customer: 10000');
    expect(component.entryForm.get('nextRunDateFormula')?.value).toBe('1D');
    expect(component.entryForm.get('referenceStartingTime')?.value).toBe('2026-10-03T08:00');
    expect(component.entryForm.get('notifyOnSuccess')?.value).toBeTrue();
    expect(component.entryForm.get('inactivityTimeoutPeriod')?.value).toBe(60);
  });

  it('auto-fills description and parameterString when job type changes on an empty form', () => {
    component.openCreateEntry();
    component.entryForm.patchValue({ description: '', parameterString: '' });

    component.onJobTypeChanged('SalesPostBatch');

    expect(component.entryForm.get('description')?.value).toBe('Batch Post Sales Orders');
    expect(component.entryForm.get('parameterString')?.value).toBe('{"batchSize": 50}');
  });

  it('does not overwrite existing description or parameterString when job type changes', () => {
    component.openCreateEntry();
    component.entryForm.patchValue({
      description: 'Custom Description',
      parameterString: '{"custom": 1}',
    });

    component.onJobTypeChanged('SalesPostBatch');

    expect(component.entryForm.get('description')?.value).toBe('Custom Description');
    expect(component.entryForm.get('parameterString')?.value).toBe('{"custom": 1}');
  });

  it('fillDefaultParameters overwrites parameterString with handler default', () => {
    component.openCreateEntry();
    component.entryForm.patchValue({ jobType: 'CleanupJobQueueLogs', parameterString: '{}' });

    component.fillDefaultParameters();

    expect(component.entryForm.get('parameterString')?.value).toBe('{"retentionDays": 30}');
  });

  it('setWeekdaysOnly turns Mon-Fri on and Sat-Sun off', () => {
    component.openCreateEntry();
    component.entryForm.patchValue({ runOnSaturdays: true, runOnSundays: true });

    component.setWeekdaysOnly();

    expect(component.entryForm.get('runOnMondays')?.value).toBeTrue();
    expect(component.entryForm.get('runOnFridays')?.value).toBeTrue();
    expect(component.entryForm.get('runOnSaturdays')?.value).toBeFalse();
    expect(component.entryForm.get('runOnSundays')?.value).toBeFalse();
  });

  it('setAllDays turns all 7 days on', () => {
    component.openCreateEntry();
    component.entryForm.patchValue({ runOnSaturdays: false, runOnSundays: false });

    component.setAllDays();

    expect(component.entryForm.get('runOnMondays')?.value).toBeTrue();
    expect(component.entryForm.get('runOnSaturdays')?.value).toBeTrue();
    expect(component.entryForm.get('runOnSundays')?.value).toBeTrue();
  });

  // --- Entry CRUD ---
  it('does not submit entry when form is invalid', () => {
    component.openCreateEntry();
    component.entryForm.patchValue({ description: '' });

    component.saveEntry();

    expect(service.create).not.toHaveBeenCalled();
    expect(component.entryForm.get('description')?.touched).toBeTrue();
  });

  it('creates entry when valid and selectedEntry is null', () => {
    component.openCreateEntry();
    component.entryForm.patchValue({
      description: 'New Recurring Task',
      jobType: 'SalesPostBatch',
      priority: 3,
      recurringJob: true,
      intervalType: JobQueueIntervalType.Minutes,
      intervalMinutes: 15,
      maxNoOfAttemptsToRun: 3,
      rerunDelaySeconds: 30,
      timeoutSeconds: 120,
    });

    component.saveEntry();

    expect(service.create).toHaveBeenCalled();
    expect(toaster.success).toHaveBeenCalledWith('Erp::SavedSuccessfully');
    expect(component.isEntryModalOpen).toBeFalse();
  });

  it('updates entry when valid and selectedEntry is present', () => {
    component.openEditEntry(mockEntry);
    component.entryForm.patchValue({ description: 'Renamed Cleanup Task' });

    component.saveEntry();

    expect(service.update).toHaveBeenCalledWith('e1', jasmine.objectContaining({
      description: 'Renamed Cleanup Task',
    }));
    expect(toaster.success).toHaveBeenCalledWith('Erp::SavedSuccessfully');
    expect(component.isEntryModalOpen).toBeFalse();
  });

  it('deletes entry when confirmed', () => {
    confirmation.warn.and.returnValue(of(Confirmation.Status.confirm) as never);

    component.deleteEntry(mockEntry);

    expect(confirmation.warn).toHaveBeenCalled();
    expect(service.delete).toHaveBeenCalledWith('e1');
    expect(toaster.success).toHaveBeenCalledWith('Erp::DeletedSuccessfully');
  });

  it('does not delete entry when dismissed', () => {
    confirmation.warn.and.returnValue(of(Confirmation.Status.dismiss) as never);

    component.deleteEntry(mockEntry);

    expect(confirmation.warn).toHaveBeenCalled();
    expect(service.delete).not.toHaveBeenCalled();
  });

  // --- State Machine & Actions ---
  it('runs an entry on-demand when runOnce is clicked', () => {
    component.runOnce(mockEntry);
    expect(service.runOnce).toHaveBeenCalledWith('e1');
    expect(toaster.success).toHaveBeenCalledWith('Cleanup completed');
  });

  it('shows error toaster when runOnce fails', () => {
    service.runOnce.and.returnValue(
      of({
        jobQueueEntryId: 'e1',
        logEntryId: 'l1',
        success: false,
        newStatus: JobQueueStatus.Error,
        processedRecords: 0,
        durationMs: 45,
        errorMessage: 'Connection lost to server',
      }) as never
    );

    component.runOnce(mockEntry);
    expect(toaster.error).toHaveBeenCalledWith('Connection lost to server');
  });

  it('sets status to OnHold and Ready', () => {
    component.setStatusOnHold(mockEntry);
    expect(service.setStatusOnHold).toHaveBeenCalledWith('e1');

    component.setStatusReady(mockEntry);
    expect(service.setStatusReady).toHaveBeenCalledWith('e1');
  });

  it('restarts a failed entry', () => {
    component.restartEntry(mockEntry);
    expect(service.restart).toHaveBeenCalledWith('e1');
    expect(toaster.success).toHaveBeenCalledWith('Erp::SavedSuccessfully');
  });

  it('opens error modal with error details', () => {
    component.showError(mockEntry);
    expect(component.isErrorModalOpen).toBeTrue();
    expect(component.activeErrorEntry).toBe(mockEntry);
  });

  it('viewEntryLogs switches to logs tab with entry filter', () => {
    component.viewEntryLogs(mockEntry);
    expect(component.selectedLogEntryFilter).toBe('e1');
    expect(component.activeTab).toBe('logs');
    expect(service.getLogs).toHaveBeenCalled();
  });

  // --- Category CRUD ---
  it('opens create category modal with blank form', () => {
    component.openCreateCategory();
    expect(component.isCategoryModalOpen).toBeTrue();
    expect(component.selectedCategory).toBeNull();
    expect(component.categoryForm.get('code')?.value).toBe('');
  });

  it('opens edit category modal with disabled code control', () => {
    component.openEditCategory(mockCategory);
    expect(component.isCategoryModalOpen).toBeTrue();
    expect(component.selectedCategory).toBe(mockCategory);
    expect(component.categoryForm.get('code')?.value).toBe('SYSTEM');
    expect(component.categoryForm.get('code')?.disabled).toBeTrue();
  });

  it('saves new category and uppercases code', () => {
    component.openCreateCategory();
    component.categoryForm.patchValue({ code: 'finance', description: 'Finance jobs' });

    component.saveCategory();

    expect(service.createCategory).toHaveBeenCalledWith({
      code: 'FINANCE',
      description: 'Finance jobs',
    });
    expect(toaster.success).toHaveBeenCalledWith('Erp::SavedSuccessfully');
    expect(component.isCategoryModalOpen).toBeFalse();
  });

  it('updates existing category', () => {
    component.openEditCategory(mockCategory);
    component.categoryForm.patchValue({ description: 'Updated system jobs' });

    component.saveCategory();

    expect(service.updateCategory).toHaveBeenCalledWith('c1', {
      code: 'SYSTEM',
      description: 'Updated system jobs',
    });
    expect(toaster.success).toHaveBeenCalledWith('Erp::SavedSuccessfully');
    expect(component.isCategoryModalOpen).toBeFalse();
  });

  it('deletes category when confirmed', () => {
    confirmation.warn.and.returnValue(of(Confirmation.Status.confirm) as never);

    component.deleteCategory(mockCategory);

    expect(confirmation.warn).toHaveBeenCalled();
    expect(service.deleteCategory).toHaveBeenCalledWith('c1');
    expect(toaster.success).toHaveBeenCalledWith('Erp::DeletedSuccessfully');
  });

  // --- Logs & Details ---
  it('opens log detail modal', () => {
    component.openLogDetail(mockLog);
    expect(component.isLogDetailModalOpen).toBeTrue();
    expect(component.selectedLogDetail).toBe(mockLog);
  });

  it('clears logs when confirmed', () => {
    confirmation.warn.and.returnValue(of(Confirmation.Status.confirm) as never);
    component.selectedLogEntryFilter = 'e1';

    component.clearLogs();

    expect(confirmation.warn).toHaveBeenCalled();
    expect(service.clearLogs).toHaveBeenCalledWith('e1');
    expect(toaster.success).toHaveBeenCalledWith('Erp::LogsClearedSuccessfully');
  });

  // --- UI Helpers ---
  it('returns correct status badge and icon classes', () => {
    expect(component.getStatusBadgeClass(JobQueueStatus.Ready)).toBe('badge bg-primary');
    expect(component.getStatusBadgeClass(JobQueueStatus.InProcess)).toBe('badge bg-warning text-dark');
    expect(component.getStatusBadgeClass(JobQueueStatus.Error)).toBe('badge bg-danger');
    expect(component.getStatusBadgeClass(JobQueueStatus.OnHold)).toBe('badge bg-secondary');
    expect(component.getStatusBadgeClass(JobQueueStatus.Finished)).toBe('badge bg-success');

    expect(component.getStatusIcon(JobQueueStatus.Ready)).toBe('fas fa-clock');
    expect(component.getStatusIcon(JobQueueStatus.InProcess)).toBe('fas fa-spinner fa-spin');
    expect(component.getStatusIcon(JobQueueStatus.Error)).toBe('fas fa-exclamation-triangle');
    expect(component.getStatusIcon(JobQueueStatus.OnHold)).toBe('fas fa-pause');
    expect(component.getStatusIcon(JobQueueStatus.Finished)).toBe('fas fa-check-circle');
  });

  it('returns correct log status badge and icon classes', () => {
    expect(component.getLogStatusBadgeClass(JobQueueLogStatus.Success)).toBe('badge bg-success');
    expect(component.getLogStatusBadgeClass(JobQueueLogStatus.Error)).toBe('badge bg-danger');
    expect(component.getLogStatusBadgeClass(JobQueueLogStatus.InProcess)).toBe('badge bg-warning text-dark');

    expect(component.getLogStatusIcon(JobQueueLogStatus.Success)).toBe('fas fa-check-circle');
    expect(component.getLogStatusIcon(JobQueueLogStatus.Error)).toBe('fas fa-times-circle');
    expect(component.getLogStatusIcon(JobQueueLogStatus.InProcess)).toBe('fas fa-spinner fa-spin');
  });

  it('formats interval strings properly', () => {
    const singleRun = { ...mockEntry, recurringJob: false };
    expect(component.formatInterval(singleRun)).toBe('Run once');

    const minutesEntry = { ...mockEntry, intervalType: JobQueueIntervalType.Minutes, intervalMinutes: 15 };
    expect(component.formatInterval(minutesEntry)).toBe('Every 15 min');

    const hoursEntry = { ...mockEntry, intervalType: JobQueueIntervalType.Hours, intervalMinutes: 4 };
    expect(component.formatInterval(hoursEntry)).toBe('Every 4 hrs');

    const daysEntry = { ...mockEntry, intervalType: JobQueueIntervalType.Days, intervalMinutes: 2 };
    expect(component.formatInterval(daysEntry)).toBe('Every 2 days');

    const weeksEntry = { ...mockEntry, intervalType: JobQueueIntervalType.Weeks, intervalMinutes: 1 };
    expect(component.formatInterval(weeksEntry)).toBe('Every 1 wks');
  });

  it('formats execution duration properly', () => {
    expect(component.formatDuration(250)).toBe('250 ms');
    expect(component.formatDuration(1500)).toBe('1.50 s');
    expect(component.formatDuration(5210)).toBe('5.21 s');
  });
});
