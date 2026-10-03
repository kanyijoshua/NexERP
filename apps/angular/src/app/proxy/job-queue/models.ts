import type { PagedAndSortedResultRequestDto } from '@abp/ng.core';
import type { JobQueueStatus } from './job-queue-status.enum';
import type { JobQueueLogStatus } from './job-queue-log-status.enum';
import type { JobQueueIntervalType } from './job-queue-interval-type.enum';

export interface JobQueueCategoryDto {
  id: string;
  code: string;
  description?: string;
  creationTime?: string;
}

export interface CreateUpdateJobQueueCategoryDto {
  code: string;
  description?: string;
}

export interface JobQueueEntryDto {
  id: string;
  description: string;
  categoryCode?: string;
  jobType: string;
  parameterString?: string;
  status: JobQueueStatus;
  priority: number;
  earliestStartDateTime?: string;
  expirationDateTime?: string;
  recurringJob: boolean;
  runOnMondays: boolean;
  runOnTuesdays: boolean;
  runOnWednesdays: boolean;
  runOnThursdays: boolean;
  runOnFridays: boolean;
  runOnSaturdays: boolean;
  runOnSundays: boolean;
  dailyStartingTime?: string;
  dailyEndingTime?: string;
  intervalType: JobQueueIntervalType;
  intervalMinutes: number;
  nextRunTime?: string;
  lastRunTime?: string;
  lastHeartbeat?: string;
  noOfAttemptsToRun: number;
  maxNoOfAttemptsToRun: number;
  rerunDelaySeconds: number;
  timeoutSeconds: number;
  lastErrorMessage?: string;
  lastErrorStackTrace?: string;
  userId?: string;
  recordIdToProcess?: string;
  nextRunDateFormula?: string;
  referenceStartingTime?: string;
  lastReadyState?: string;
  notifyOnSuccess: boolean;
  scheduled: boolean;
  manualRecurrence: boolean;
  inactivityTimeoutPeriod?: number;
  systemTaskId?: string;
}

export interface CreateJobQueueEntryDto {
  description: string;
  categoryCode?: string;
  jobType: string;
  parameterString?: string;
  priority: number;
  earliestStartDateTime?: string;
  expirationDateTime?: string;
  recurringJob: boolean;
  intervalType: JobQueueIntervalType;
  intervalMinutes: number;
  runOnMondays: boolean;
  runOnTuesdays: boolean;
  runOnWednesdays: boolean;
  runOnThursdays: boolean;
  runOnFridays: boolean;
  runOnSaturdays: boolean;
  runOnSundays: boolean;
  dailyStartingTime?: string;
  dailyEndingTime?: string;
  maxNoOfAttemptsToRun: number;
  rerunDelaySeconds: number;
  timeoutSeconds: number;
  userId?: string;
  recordIdToProcess?: string;
  nextRunDateFormula?: string;
  referenceStartingTime?: string;
  notifyOnSuccess?: boolean;
  manualRecurrence?: boolean;
  inactivityTimeoutPeriod?: number;
}

export interface UpdateJobQueueEntryDto {
  description: string;
  categoryCode?: string;
  jobType: string;
  parameterString?: string;
  priority: number;
  earliestStartDateTime?: string;
  expirationDateTime?: string;
  recurringJob: boolean;
  intervalType: JobQueueIntervalType;
  intervalMinutes: number;
  runOnMondays: boolean;
  runOnTuesdays: boolean;
  runOnWednesdays: boolean;
  runOnThursdays: boolean;
  runOnFridays: boolean;
  runOnSaturdays: boolean;
  runOnSundays: boolean;
  dailyStartingTime?: string;
  dailyEndingTime?: string;
  maxNoOfAttemptsToRun: number;
  rerunDelaySeconds: number;
  timeoutSeconds: number;
  userId?: string;
  recordIdToProcess?: string;
  nextRunDateFormula?: string;
  referenceStartingTime?: string;
  notifyOnSuccess?: boolean;
  manualRecurrence?: boolean;
  inactivityTimeoutPeriod?: number;
}

export interface GetJobQueueEntriesInput extends PagedAndSortedResultRequestDto {
  categoryCode?: string;
  jobType?: string;
  status?: JobQueueStatus;
  filter?: string;
}

export interface JobQueueLogEntryDto {
  id: string;
  jobQueueEntryId: string;
  jobType: string;
  description?: string;
  startDateTime: string;
  endDateTime?: string;
  durationMs: number;
  status: JobQueueLogStatus;
  processedRecords: number;
  outputDetails?: string;
  errorMessage?: string;
  errorStackTrace?: string;
  userId?: string;
  categoryCode?: string;
  parameterString?: string;
  systemTaskId?: string;
}

export interface GetJobQueueLogsInput extends PagedAndSortedResultRequestDto {
  jobQueueEntryId?: string;
  status?: JobQueueLogStatus;
}

export interface JobTypeInfoDto {
  jobType: string;
  displayName: string;
  description?: string;
  defaultParametersJson?: string;
}

export interface JobQueueRunResultDto {
  jobQueueEntryId: string;
  logEntryId: string;
  success: boolean;
  newStatus: JobQueueStatus;
  processedRecords: number;
  outputDetails?: string;
  errorMessage?: string;
  durationMs: number;
}
