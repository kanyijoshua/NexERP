import type { FullAuditedEntityDto, PagedAndSortedResultRequestDto } from '@abp/ng.core';
import type { CodeTableDto, CreateUpdateCodeTableDto } from '../companies/models';
import type { AcademicDocumentStatus } from './academic-document-status.enum';
import type { AcademicRequestStatus } from './academic-request-status.enum';
import type { StudentChangeType } from './student-change-type.enum';
import type { ApplicationStatus } from './application-status.enum';
import type { ExamType } from './exam-type.enum';
import type { FeeType } from './fee-type.enum';
import type { ProgrammeLevel } from './programme-level.enum';
import type { RegisterFor } from './register-for.enum';
import type { RegistrationStatus } from './registration-status.enum';
import type { StudentGender } from './student-gender.enum';
import type { StudentSponsorship } from './student-sponsorship.enum';
import type { StudentStatus } from './student-status.enum';
import type { StudyMode } from './study-mode.enum';
import type { UnitType } from './unit-type.enum';
import type { AttendanceMark } from './attendance-mark.enum';
import type { ClinicVisitStatus } from './clinic-visit-status.enum';
import type { HostelAllocationStatus } from './hostel-allocation-status.enum';
import type { LaundryStatus } from './laundry-status.enum';
import type { PatientType } from './patient-type.enum';
import type { RoomType } from './room-type.enum';
import type { ShortCourseApplicationStatus } from './short-course-application-status.enum';
import type { ShortCourseApplicationType } from './short-course-application-type.enum';
import type { TimetableDay } from './timetable-day.enum';
import type { TimetableType } from './timetable-type.enum';
import type { TreatmentType } from './treatment-type.enum';

export interface AcademicSetupDto {
  applicationNos?: string;
  studentNos?: string;
  registrationNos?: string;
  billingNos?: string;
  examResultNos?: string;
  studentPostingGroup?: string;
  studentGenBusPostingGroup?: string;
  checkStudentBalance: boolean;
  maxFeeBalanceToRegister: number;
  billOnRegistration: boolean;
  examRoundingDecimals: number;
  receiptNos?: string;
  refundNos?: string;
  statusChangeNos?: string;
  attendanceNos?: string;
  hostelAllocationNos?: string;
  clinicVisitNos?: string;
  laundryNos?: string;
  shortCourseNos?: string;
  minAttendancePct: number;
  laundryExpressChargePct: number;
  medicalFeeItemCode?: string;
}

export interface AcademicYearDto extends CodeTableDto {
  startDate?: string;
  endDate?: string;
  current: boolean;
}

export interface CreateUpdateAcademicYearDto extends CreateUpdateCodeTableDto {
  startDate?: string;
  endDate?: string;
  current?: boolean;
}

export interface SemesterDto extends CodeTableDto {
  academicYearCode?: string;
  startDate?: string;
  endDate?: string;
  registrationFrom?: string;
  registrationTo?: string;
  current: boolean;
}

export interface CreateUpdateSemesterDto extends CreateUpdateCodeTableDto {
  academicYearCode?: string;
  startDate?: string;
  endDate?: string;
  registrationFrom?: string;
  registrationTo?: string;
  current?: boolean;
}

export interface IntakeDto extends CodeTableDto {
  academicYearCode?: string;
  startDate?: string;
  endDate?: string;
  current: boolean;
}

export interface CreateUpdateIntakeDto extends CreateUpdateCodeTableDto {
  academicYearCode?: string;
  startDate?: string;
  endDate?: string;
  current?: boolean;
}

export interface ExamCategoryDto extends CodeTableDto {
  blockResultsEntry: boolean;
}

export interface CreateUpdateExamCategoryDto extends CreateUpdateCodeTableDto {
  blockResultsEntry?: boolean;
}

export interface GradingBandDto extends FullAuditedEntityDto<string> {
  examCategoryCode?: string;
  grade?: string;
  description?: string;
  fromMark: number;
  toMark: number;
  points: number;
  remarks?: string;
  passed: boolean;
}

export interface CreateUpdateGradingBandDto {
  examCategoryCode: string;
  grade: string;
  description?: string;
  fromMark?: number;
  toMark?: number;
  points?: number;
  remarks?: string;
  passed?: boolean;
}

export interface GetExamCategoryTableListInput extends PagedAndSortedResultRequestDto {
  /** Filter pane conditions as JSON (see erp-table). */
  dynamicFilter?: string;
  filter?: string;
  examCategoryCode?: string;
}

export interface ExamComponentDto extends FullAuditedEntityDto<string> {
  examCategoryCode?: string;
  examType: ExamType;
  description?: string;
  maxScore: number;
  contributionPct: number;
}

export interface CreateUpdateExamComponentDto {
  examCategoryCode: string;
  examType?: ExamType;
  description?: string;
  maxScore?: number;
  contributionPct?: number;
}

export interface ProgrammeDto extends CodeTableDto {
  level: ProgrammeLevel;
  examCategoryCode?: string;
  durationMonths: number;
  minimumCapacity: number;
  maximumCapacity: number;
  studentNos?: string;
  studentNoPrefix?: string;
  studentNoSuffix?: string;
  active: boolean;
}

export interface CreateUpdateProgrammeDto extends CreateUpdateCodeTableDto {
  level?: ProgrammeLevel;
  examCategoryCode?: string;
  durationMonths?: number;
  minimumCapacity?: number;
  maximumCapacity?: number;
  studentNos?: string;
  studentNoPrefix?: string;
  studentNoSuffix?: string;
  active?: boolean;
}

export interface ProgrammeStageDto extends FullAuditedEntityDto<string> {
  programmeCode?: string;
  code?: string;
  description?: string;
  sequence: number;
  finalStage: boolean;
  minimumUnits: number;
  maximumUnits: number;
}

export interface CreateUpdateProgrammeStageDto {
  programmeCode: string;
  code: string;
  description?: string;
  sequence?: number;
  finalStage?: boolean;
  minimumUnits?: number;
  maximumUnits?: number;
}

export interface GetProgrammeTableListInput extends PagedAndSortedResultRequestDto {
  /** Filter pane conditions as JSON (see erp-table). */
  dynamicFilter?: string;
  filter?: string;
  programmeCode?: string;
  stageCode?: string;
}

export interface CourseUnitDto extends FullAuditedEntityDto<string> {
  programmeCode?: string;
  code?: string;
  description?: string;
  stageCode?: string;
  semesterCode?: string;
  unitType: UnitType;
  creditHours: number;
  prerequisiteUnitCode?: string;
  blocked: boolean;
}

export interface CreateUpdateCourseUnitDto {
  programmeCode: string;
  code: string;
  description?: string;
  stageCode: string;
  semesterCode?: string;
  unitType?: UnitType;
  creditHours?: number;
  prerequisiteUnitCode?: string;
  blocked?: boolean;
}

export interface FeeItemDto extends CodeTableDto {
  feeType: FeeType;
  glAccountNo?: string;
  defaultAmount: number;
  tuitionFee: boolean;
}

export interface CreateUpdateFeeItemDto extends CreateUpdateCodeTableDto {
  feeType?: FeeType;
  glAccountNo?: string;
  defaultAmount?: number;
  tuitionFee?: boolean;
}

export interface FeeStructureLineDto extends FullAuditedEntityDto<string> {
  programmeCode?: string;
  stageCode?: string;
  semesterCode?: string;
  studyMode: StudyMode;
  feeItemCode?: string;
  amount: number;
}

export interface CreateUpdateFeeStructureLineDto {
  programmeCode: string;
  stageCode: string;
  semesterCode?: string;
  studyMode?: StudyMode;
  feeItemCode: string;
  amount?: number;
}

export interface PersonInputDto {
  firstName: string;
  otherName?: string;
  lastName?: string;
  gender?: StudentGender;
  dateOfBirth?: string;
  nationalId?: string;
  phoneNo?: string;
  email?: string;
  address?: string;
  city?: string;
  programmeCode: string;
  intakeCode?: string;
  academicYearCode?: string;
  studyMode?: StudyMode;
  sponsorship?: StudentSponsorship;
  guardianName?: string;
  guardianPhoneNo?: string;
}

export interface StudentApplicationDto extends FullAuditedEntityDto<string> {
  no?: string;
  applicationDate?: string;
  firstName?: string;
  otherName?: string;
  lastName?: string;
  fullName?: string;
  gender: StudentGender;
  dateOfBirth?: string;
  nationalId?: string;
  phoneNo?: string;
  email?: string;
  address?: string;
  city?: string;
  programmeCode?: string;
  intakeCode?: string;
  academicYearCode?: string;
  studyMode: StudyMode;
  formerSchool?: string;
  indexNumber?: string;
  meanGrade?: string;
  sponsorship: StudentSponsorship;
  guardianName?: string;
  guardianPhoneNo?: string;
  status: ApplicationStatus;
  rejectionReason?: string;
  studentNo?: string;
  admissionDate?: string;
}

export interface CreateUpdateStudentApplicationDto extends PersonInputDto {
  no?: string;
  applicationDate?: string;
  formerSchool?: string;
  indexNumber?: string;
  meanGrade?: string;
}

export interface GetStudentApplicationListInput extends PagedAndSortedResultRequestDto {
  /** Filter pane conditions as JSON (see erp-table). */
  dynamicFilter?: string;
  filter?: string;
  programmeCode?: string;
  intakeCode?: string;
  status?: ApplicationStatus;
}

export interface RejectApplicationInput {
  reason?: string;
}

export interface AdmitApplicationInput {
  admissionDate?: string;
}

export interface StudentDto extends FullAuditedEntityDto<string> {
  no?: string;
  firstName?: string;
  otherName?: string;
  lastName?: string;
  fullName?: string;
  gender: StudentGender;
  dateOfBirth?: string;
  nationalId?: string;
  phoneNo?: string;
  email?: string;
  address?: string;
  city?: string;
  programmeCode?: string;
  currentStageCode?: string;
  currentSemesterCode?: string;
  intakeCode?: string;
  academicYearCode?: string;
  studyMode: StudyMode;
  status: StudentStatus;
  admissionDate?: string;
  customerNo?: string;
  applicationNo?: string;
  sponsorship: StudentSponsorship;
  guardianName?: string;
  guardianPhoneNo?: string;
}

export interface CreateUpdateStudentDto extends PersonInputDto {
  no?: string;
  status?: StudentStatus;
  admissionDate?: string;
}

export interface GetStudentListInput extends PagedAndSortedResultRequestDto {
  /** Filter pane conditions as JSON (see erp-table). */
  dynamicFilter?: string;
  filter?: string;
  programmeCode?: string;
  stageCode?: string;
  intakeCode?: string;
  status?: StudentStatus;
}

export interface StudentBalanceDto {
  studentNo?: string;
  customerNo?: string;
  totalBilled: number;
  balance: number;
  prepayment: number;
}

export interface TranscriptLineDto {
  academicYearCode?: string;
  semesterCode?: string;
  stageCode?: string;
  unitCode?: string;
  unitDescription?: string;
  creditHours: number;
  finalScore: number;
  grade?: string;
  points: number;
  resultRemarks?: string;
  passed: boolean;
}

export interface StudentTranscriptDto {
  studentNo?: string;
  studentName?: string;
  programmeCode?: string;
  unitsTaken: number;
  unitsPassed: number;
  meanScore: number;
  meanPoints: number;
  lines: TranscriptLineDto[];
}

export interface SemesterRegistrationDto extends FullAuditedEntityDto<string> {
  no?: string;
  studentNo?: string;
  studentName?: string;
  programmeCode?: string;
  stageCode?: string;
  semesterCode?: string;
  academicYearCode?: string;
  registrationDate?: string;
  registerFor: RegisterFor;
  remarks?: string;
  status: RegistrationStatus;
  noOfUnits: number;
  billNo?: string;
  billedAmount: number;
}

export interface CreateUpdateSemesterRegistrationDto {
  no?: string;
  studentNo: string;
  stageCode: string;
  semesterCode?: string;
  academicYearCode?: string;
  registrationDate?: string;
  registerFor?: RegisterFor;
  remarks?: string;
}

export interface GetSemesterRegistrationListInput extends PagedAndSortedResultRequestDto {
  /** Filter pane conditions as JSON (see erp-table). */
  dynamicFilter?: string;
  filter?: string;
  studentNo?: string;
  semesterCode?: string;
  status?: RegistrationStatus;
}

export interface StudentUnitDto extends FullAuditedEntityDto<string> {
  registrationNo?: string;
  studentNo?: string;
  studentName?: string;
  programmeCode?: string;
  stageCode?: string;
  semesterCode?: string;
  academicYearCode?: string;
  unitCode?: string;
  unitDescription?: string;
  unitType: UnitType;
  creditHours: number;
  assignmentMark?: number;
  catMark?: number;
  cat2Mark?: number;
  examMark?: number;
  finalScore: number;
  grade?: string;
  points: number;
  resultRemarks?: string;
  passed: boolean;
  sessionsHeld: number;
  sessionsAttended: number;
  attendancePct: number;
}

export interface CreateUpdateStudentUnitDto {
  registrationNo: string;
  unitCode: string;
}

export interface GetStudentUnitListInput extends PagedAndSortedResultRequestDto {
  /** Filter pane conditions as JSON (see erp-table). */
  dynamicFilter?: string;
  filter?: string;
  registrationNo?: string;
  studentNo?: string;
  unitCode?: string;
  semesterCode?: string;
}

export interface StudentBillHeaderDto extends FullAuditedEntityDto<string> {
  no?: string;
  studentNo?: string;
  studentName?: string;
  postingDate?: string;
  programmeCode?: string;
  stageCode?: string;
  semesterCode?: string;
  academicYearCode?: string;
  description?: string;
  registrationNo?: string;
  status: AcademicDocumentStatus;
  postedDate?: string;
  postedBy?: string;
  totalAmount: number;
}

export interface CreateUpdateStudentBillHeaderDto {
  no?: string;
  studentNo: string;
  postingDate?: string;
  stageCode?: string;
  semesterCode?: string;
  academicYearCode?: string;
  description?: string;
}

export interface GetStudentBillListInput extends PagedAndSortedResultRequestDto {
  /** Filter pane conditions as JSON (see erp-table). */
  dynamicFilter?: string;
  filter?: string;
  studentNo?: string;
  status?: AcademicDocumentStatus;
}

export interface StudentBillLineDto extends FullAuditedEntityDto<string> {
  documentNo?: string;
  lineNo: number;
  feeItemCode?: string;
  description?: string;
  amount: number;
}

export interface CreateUpdateStudentBillLineDto {
  documentNo: string;
  lineNo?: number;
  feeItemCode: string;
  description?: string;
  amount?: number;
}

export interface GetDocumentLineListInput extends PagedAndSortedResultRequestDto {
  /** Filter pane conditions as JSON (see erp-table). */
  dynamicFilter?: string;
  filter?: string;
  documentNo?: string;
}

export interface ExamResultHeaderDto extends FullAuditedEntityDto<string> {
  no?: string;
  programmeCode?: string;
  stageCode?: string;
  semesterCode?: string;
  academicYearCode?: string;
  unitCode?: string;
  unitDescription?: string;
  examType: ExamType;
  lecturerNo?: string;
  documentDate?: string;
  status: AcademicDocumentStatus;
  postedDate?: string;
  postedBy?: string;
  noOfStudents: number;
}

export interface CreateUpdateExamResultHeaderDto {
  no?: string;
  programmeCode: string;
  unitCode: string;
  semesterCode?: string;
  academicYearCode?: string;
  examType?: ExamType;
  lecturerNo?: string;
  documentDate?: string;
}

export interface GetExamResultListInput extends PagedAndSortedResultRequestDto {
  /** Filter pane conditions as JSON (see erp-table). */
  dynamicFilter?: string;
  filter?: string;
  programmeCode?: string;
  unitCode?: string;
  status?: AcademicDocumentStatus;
}

export interface ExamResultLineDto extends FullAuditedEntityDto<string> {
  documentNo?: string;
  lineNo: number;
  studentNo?: string;
  studentName?: string;
  mark: number;
  notDone: boolean;
}

export interface CreateUpdateExamResultLineDto {
  documentNo: string;
  lineNo?: number;
  studentNo: string;
  mark?: number;
  notDone?: boolean;
}

export interface StudentReceiptDto extends FullAuditedEntityDto<string> {
  no?: string;
  studentNo?: string;
  studentName?: string;
  postingDate?: string;
  bankAccountNo?: string;
  payMode?: string;
  externalDocumentNo?: string;
  amount: number;
  appliesToBillNo?: string;
  description?: string;
  status: AcademicDocumentStatus;
  postedDate?: string;
  postedBy?: string;
}

export interface CreateUpdateStudentReceiptDto {
  no?: string;
  studentNo: string;
  postingDate?: string;
  bankAccountNo?: string;
  payMode?: string;
  externalDocumentNo?: string;
  amount: number;
  appliesToBillNo?: string;
  description?: string;
}

export interface GetStudentDocumentListInput extends PagedAndSortedResultRequestDto {
  /** Filter pane conditions as JSON (see erp-table). */
  dynamicFilter?: string;
  filter?: string;
  studentNo?: string;
}

export interface StudentRefundDto extends FullAuditedEntityDto<string> {
  no?: string;
  studentNo?: string;
  studentName?: string;
  documentDate?: string;
  amount: number;
  reason?: string;
  status: AcademicRequestStatus;
  approvedDate?: string;
  approvedBy?: string;
  paymentVoucherNo?: string;
}

export interface CreateUpdateStudentRefundDto {
  no?: string;
  studentNo: string;
  documentDate?: string;
  amount: number;
  reason?: string;
}

export interface StudentStatusChangeDto extends FullAuditedEntityDto<string> {
  no?: string;
  studentNo?: string;
  studentName?: string;
  changeType: StudentChangeType;
  effectiveDate?: string;
  resumeDate?: string;
  reason?: string;
  status: AcademicRequestStatus;
  previousStatus: StudentStatus;
  newStatus: StudentStatus;
  approvedDate?: string;
  approvedBy?: string;
}

export interface CreateUpdateStudentStatusChangeDto {
  no?: string;
  studentNo: string;
  changeType?: StudentChangeType;
  effectiveDate?: string;
  resumeDate?: string;
  reason?: string;
}

// ---------------------------------------------------------------- Campus

export interface GetCampusDocumentListInput extends PagedAndSortedResultRequestDto {
  /** Filter pane conditions as JSON (see erp-table). */
  dynamicFilter?: string;
  filter?: string;
}

export interface LectureRoomDto extends CodeTableDto {
  roomType: RoomType;
  buildingCode?: string;
  maximumCapacity: number;
  blocked: boolean;
}

export interface CreateUpdateLectureRoomDto extends CreateUpdateCodeTableDto {
  roomType: RoomType;
  buildingCode?: string;
  maximumCapacity: number;
  blocked: boolean;
}

export interface TimetableEntryDto extends FullAuditedEntityDto<string> {
  semesterCode?: string;
  academicYearCode?: string;
  timetableType: TimetableType;
  programmeCode?: string;
  stageCode?: string;
  unitCode?: string;
  unitDescription?: string;
  day: TimetableDay;
  examDate?: string;
  startTime?: string;
  endTime?: string;
  roomCode?: string;
  lecturerNo?: string;
  remarks?: string;
}

export interface CreateUpdateTimetableEntryDto {
  semesterCode?: string;
  academicYearCode?: string;
  timetableType: TimetableType;
  programmeCode: string;
  unitCode: string;
  day: TimetableDay;
  examDate?: string;
  startTime: string;
  endTime: string;
  roomCode?: string;
  lecturerNo?: string;
  remarks?: string;
}

export interface GetTimetableEntryListInput extends PagedAndSortedResultRequestDto {
  dynamicFilter?: string;
  filter?: string;
  semesterCode?: string;
  programmeCode?: string;
  timetableType?: TimetableType;
}

export interface AttendanceRegisterDto extends FullAuditedEntityDto<string> {
  no?: string;
  programmeCode?: string;
  stageCode?: string;
  semesterCode?: string;
  academicYearCode?: string;
  unitCode?: string;
  unitDescription?: string;
  lecturerNo?: string;
  lessonDate?: string;
  startTime?: string;
  hours: number;
  remarks?: string;
  status: AcademicDocumentStatus;
  postedDate?: string;
  postedBy?: string;
  noOfStudents: number;
  noPresent: number;
}

export interface CreateUpdateAttendanceRegisterDto {
  no?: string;
  programmeCode: string;
  unitCode: string;
  semesterCode?: string;
  academicYearCode?: string;
  lecturerNo?: string;
  lessonDate?: string;
  startTime?: string;
  hours: number;
  remarks?: string;
}

export interface AttendanceLineDto extends FullAuditedEntityDto<string> {
  documentNo?: string;
  lineNo: number;
  studentNo?: string;
  studentName?: string;
  mark: AttendanceMark;
}

export interface CreateUpdateAttendanceLineDto {
  documentNo: string;
  lineNo?: number;
  studentNo: string;
  mark: AttendanceMark;
}

export interface HostelDto extends CodeTableDto {
  gender: StudentGender;
  costPerOccupant: number;
  feeItemCode?: string;
  blocked: boolean;
}

export interface CreateUpdateHostelDto extends CreateUpdateCodeTableDto {
  gender: StudentGender;
  costPerOccupant: number;
  feeItemCode?: string;
  blocked: boolean;
}

export interface HostelRoomDto extends FullAuditedEntityDto<string> {
  hostelCode?: string;
  roomNo?: string;
  bedSpaces: number;
  roomCost: number;
  outOfOrder: boolean;
  occupiedSpaces: number;
  vacantSpaces: number;
}

export interface CreateUpdateHostelRoomDto {
  hostelCode: string;
  roomNo: string;
  bedSpaces: number;
  roomCost: number;
  outOfOrder: boolean;
}

export interface GetHostelRoomListInput extends PagedAndSortedResultRequestDto {
  dynamicFilter?: string;
  filter?: string;
  hostelCode?: string;
  vacantOnly?: boolean;
}

export interface HostelAllocationDto extends FullAuditedEntityDto<string> {
  no?: string;
  studentNo?: string;
  studentName?: string;
  hostelCode?: string;
  roomNo?: string;
  semesterCode?: string;
  allocationDate?: string;
  remarks?: string;
  status: HostelAllocationStatus;
  charges: number;
  billNo?: string;
  clearanceDate?: string;
  processedBy?: string;
}

export interface CreateUpdateHostelAllocationDto {
  no?: string;
  studentNo: string;
  hostelCode: string;
  roomNo: string;
  semesterCode?: string;
  allocationDate?: string;
  remarks?: string;
}

export interface ClinicVisitDto extends FullAuditedEntityDto<string> {
  no?: string;
  patientType: PatientType;
  patientNo?: string;
  patientName?: string;
  visitDate?: string;
  treatmentType: TreatmentType;
  complaint?: string;
  diagnosis?: string;
  treatment?: string;
  attendedBy?: string;
  referredTo?: string;
  offDutyFrom?: string;
  offDutyTo?: string;
  offDutyDays: number;
  lightDutyDays: number;
  offDutyComments?: string;
  charge: number;
  status: ClinicVisitStatus;
  billNo?: string;
  completedDate?: string;
  completedBy?: string;
}

export interface CreateUpdateClinicVisitDto {
  no?: string;
  patientType: PatientType;
  patientNo?: string;
  patientName?: string;
  visitDate?: string;
  treatmentType: TreatmentType;
  complaint?: string;
  diagnosis?: string;
  treatment?: string;
  attendedBy?: string;
  referredTo?: string;
  offDutyFrom?: string;
  offDutyTo?: string;
  lightDutyDays: number;
  offDutyComments?: string;
  charge: number;
}

export interface ClinicPrescriptionDto extends FullAuditedEntityDto<string> {
  documentNo?: string;
  lineNo: number;
  itemNo?: string;
  description?: string;
  quantity: number;
  dosage?: string;
  locationCode?: string;
  issued: boolean;
}

export interface CreateUpdateClinicPrescriptionDto {
  documentNo: string;
  lineNo?: number;
  itemNo?: string;
  description?: string;
  quantity: number;
  dosage?: string;
  locationCode?: string;
}

export interface LaundryItemDto extends CodeTableDto {
  ratePerItem: number;
  glAccountNo?: string;
}

export interface CreateUpdateLaundryItemDto extends CreateUpdateCodeTableDto {
  ratePerItem: number;
  glAccountNo?: string;
}

export interface LaundryOrderDto extends FullAuditedEntityDto<string> {
  no?: string;
  customerNo?: string;
  customerName?: string;
  receivedDate?: string;
  promisedDate?: string;
  express: boolean;
  discountPct: number;
  remarks?: string;
  status: LaundryStatus;
  totalAmount: number;
  invoicedDate?: string;
  collectedDate?: string;
  processedBy?: string;
}

export interface CreateUpdateLaundryOrderDto {
  no?: string;
  customerNo: string;
  receivedDate?: string;
  promisedDate?: string;
  express: boolean;
  discountPct: number;
  remarks?: string;
}

export interface LaundryOrderLineDto extends FullAuditedEntityDto<string> {
  documentNo?: string;
  lineNo: number;
  laundryItemCode?: string;
  description?: string;
  quantity: number;
  unitPrice: number;
  amount: number;
}

export interface CreateUpdateLaundryOrderLineDto {
  documentNo: string;
  lineNo?: number;
  laundryItemCode: string;
  description?: string;
  quantity: number;
  unitPrice?: number;
}

export interface ShortCourseDto extends CodeTableDto {
  durationDays: number;
  feePerParticipant: number;
  glAccountNo?: string;
  maxParticipants: number;
  active: boolean;
}

export interface CreateUpdateShortCourseDto extends CreateUpdateCodeTableDto {
  durationDays: number;
  feePerParticipant: number;
  glAccountNo?: string;
  maxParticipants: number;
  active: boolean;
}

export interface ShortCourseApplicationDto extends FullAuditedEntityDto<string> {
  no?: string;
  applicationType: ShortCourseApplicationType;
  courseCode?: string;
  courseDescription?: string;
  applicationDate?: string;
  startDate?: string;
  endDate?: string;
  customerNo?: string;
  customerName?: string;
  feePerParticipant: number;
  remarks?: string;
  rejectionReason?: string;
  status: ShortCourseApplicationStatus;
  noOfParticipants: number;
  billedAmount: number;
  registeredDate?: string;
  processedBy?: string;
}

export interface CreateUpdateShortCourseApplicationDto {
  no?: string;
  applicationType: ShortCourseApplicationType;
  courseCode: string;
  applicationDate?: string;
  startDate?: string;
  endDate?: string;
  customerNo?: string;
  feePerParticipant: number;
  remarks?: string;
}

export interface RejectShortCourseApplicationInput {
  reason: string;
}

export interface ShortCourseParticipantDto extends FullAuditedEntityDto<string> {
  documentNo?: string;
  lineNo: number;
  name?: string;
  nationalId?: string;
  phoneNo?: string;
  email?: string;
  certificateNo?: string;
}

export interface CreateUpdateShortCourseParticipantDto {
  documentNo: string;
  lineNo?: number;
  name: string;
  nationalId?: string;
  phoneNo?: string;
  email?: string;
}