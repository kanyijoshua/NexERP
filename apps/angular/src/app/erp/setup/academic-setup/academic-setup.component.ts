import { ToasterService } from '@abp/ng.theme.shared';
import { Component, DestroyRef, OnInit, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder } from '@angular/forms';
import { AcademicSetupDto, AcademicSetupService } from '@proxy/academics';
import { NoSeriesDto, NoSeriesService } from '@proxy/numbering';
import { finalize, forkJoin } from 'rxjs';
import { CompanyService } from '../../services/company.service';

/**
 * Academic Setup: the number series of the academic documents, the posting groups a student's
 * customer account is opened with, and the rules registration and grading follow.
 */
@Component({
  selector: 'app-academic-setup',
  templateUrl: './academic-setup.component.html',
  standalone: false,
})
export class AcademicSetupComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly setupService = inject(AcademicSetupService);
  private readonly noSeriesService = inject(NoSeriesService);
  private readonly companyService = inject(CompanyService);
  private readonly toaster = inject(ToasterService);
  private readonly destroyRef = inject(DestroyRef);

  readonly series = signal<NoSeriesDto[]>([]);
  readonly loading = signal(false);
  readonly saving = signal(false);

  /** The number series fields, in the order the page lists them. */
  readonly seriesFields = [
    { control: 'applicationNos', labelKey: 'Erp::ApplicationNos' },
    { control: 'studentNos', labelKey: 'Erp::StudentNos' },
    { control: 'registrationNos', labelKey: 'Erp::RegistrationNos' },
    { control: 'billingNos', labelKey: 'Erp::BillingNos' },
    { control: 'examResultNos', labelKey: 'Erp::ExamResultNos' },
    { control: 'receiptNos', labelKey: 'Erp::ReceiptNos' },
    { control: 'refundNos', labelKey: 'Erp::RefundNos' },
    { control: 'statusChangeNos', labelKey: 'Erp::StatusChangeNos' },
    { control: 'attendanceNos', labelKey: 'Erp::AttendanceNos' },
    { control: 'hostelAllocationNos', labelKey: 'Erp::HostelAllocationNos' },
    { control: 'clinicVisitNos', labelKey: 'Erp::ClinicVisitNos' },
    { control: 'laundryNos', labelKey: 'Erp::LaundryNos' },
    { control: 'shortCourseNos', labelKey: 'Erp::ShortCourseNos' },
  ] as const;

  readonly form = this.fb.group({
    applicationNos: this.fb.control<string | null>(null),
    studentNos: this.fb.control<string | null>(null),
    registrationNos: this.fb.control<string | null>(null),
    billingNos: this.fb.control<string | null>(null),
    examResultNos: this.fb.control<string | null>(null),
    receiptNos: this.fb.control<string | null>(null),
    refundNos: this.fb.control<string | null>(null),
    statusChangeNos: this.fb.control<string | null>(null),
    studentPostingGroup: this.fb.control<string | null>(null),
    studentGenBusPostingGroup: this.fb.control<string | null>(null),
    checkStudentBalance: this.fb.nonNullable.control(false),
    maxFeeBalanceToRegister: this.fb.nonNullable.control(0),
    billOnRegistration: this.fb.nonNullable.control(true),
    examRoundingDecimals: this.fb.nonNullable.control(0),
    attendanceNos: this.fb.control<string | null>(null),
    hostelAllocationNos: this.fb.control<string | null>(null),
    clinicVisitNos: this.fb.control<string | null>(null),
    laundryNos: this.fb.control<string | null>(null),
    shortCourseNos: this.fb.control<string | null>(null),
    minAttendancePct: this.fb.nonNullable.control(0),
    laundryExpressChargePct: this.fb.nonNullable.control(0),
    medicalFeeItemCode: this.fb.control<string | null>(null),
  });

  ngOnInit(): void {
    this.load();
    this.companyService.companyChanged$.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(() => this.load());
  }

  save(): void {
    const value = this.form.getRawValue();
    const input: AcademicSetupDto = {
      applicationNos: value.applicationNos || undefined,
      studentNos: value.studentNos || undefined,
      registrationNos: value.registrationNos || undefined,
      billingNos: value.billingNos || undefined,
      examResultNos: value.examResultNos || undefined,
      receiptNos: value.receiptNos || undefined,
      refundNos: value.refundNos || undefined,
      statusChangeNos: value.statusChangeNos || undefined,
      studentPostingGroup: value.studentPostingGroup || undefined,
      studentGenBusPostingGroup: value.studentGenBusPostingGroup || undefined,
      checkStudentBalance: value.checkStudentBalance,
      maxFeeBalanceToRegister: Number(value.maxFeeBalanceToRegister) || 0,
      billOnRegistration: value.billOnRegistration,
      examRoundingDecimals: Number(value.examRoundingDecimals) || 0,
      attendanceNos: value.attendanceNos || undefined,
      hostelAllocationNos: value.hostelAllocationNos || undefined,
      clinicVisitNos: value.clinicVisitNos || undefined,
      laundryNos: value.laundryNos || undefined,
      shortCourseNos: value.shortCourseNos || undefined,
      minAttendancePct: Number(value.minAttendancePct) || 0,
      laundryExpressChargePct: Number(value.laundryExpressChargePct) || 0,
      medicalFeeItemCode: value.medicalFeeItemCode || undefined,
    };

    this.saving.set(true);
    this.setupService
      .update(input)
      .pipe(
        finalize(() => this.saving.set(false)),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe(setup => {
        this.show(setup);
        this.toaster.success('Erp::SavedSuccessfully');
      });
  }

  private load(): void {
    this.loading.set(true);
    forkJoin({
      setup: this.setupService.get(),
      series: this.noSeriesService.getList({ maxResultCount: 1000, skipCount: 0 } as never),
    })
      .pipe(
        finalize(() => this.loading.set(false)),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe(({ setup, series }) => {
        this.series.set(series.items ?? []);
        this.show(setup);
      });
  }

  private show(setup: AcademicSetupDto): void {
    this.form.reset({
      applicationNos: setup.applicationNos ?? null,
      studentNos: setup.studentNos ?? null,
      registrationNos: setup.registrationNos ?? null,
      billingNos: setup.billingNos ?? null,
      examResultNos: setup.examResultNos ?? null,
      receiptNos: setup.receiptNos ?? null,
      refundNos: setup.refundNos ?? null,
      statusChangeNos: setup.statusChangeNos ?? null,
      studentPostingGroup: setup.studentPostingGroup ?? null,
      studentGenBusPostingGroup: setup.studentGenBusPostingGroup ?? null,
      checkStudentBalance: setup.checkStudentBalance,
      maxFeeBalanceToRegister: setup.maxFeeBalanceToRegister ?? 0,
      billOnRegistration: setup.billOnRegistration,
      examRoundingDecimals: setup.examRoundingDecimals ?? 0,
      attendanceNos: setup.attendanceNos ?? null,
      hostelAllocationNos: setup.hostelAllocationNos ?? null,
      clinicVisitNos: setup.clinicVisitNos ?? null,
      laundryNos: setup.laundryNos ?? null,
      shortCourseNos: setup.shortCourseNos ?? null,
      minAttendancePct: setup.minAttendancePct ?? 0,
      laundryExpressChargePct: setup.laundryExpressChargePct ?? 0,
      medicalFeeItemCode: setup.medicalFeeItemCode ?? null,
    });
  }
}
