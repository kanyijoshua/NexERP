import { Injectable, inject } from '@angular/core';
import {
  AcademicDocumentStatus,
  AttendanceLineDto,
  AttendanceLineService,
  AttendanceMark,
  AttendanceRegisterDto,
  AttendanceRegisterService,
  ClinicPrescriptionDto,
  ClinicPrescriptionService,
  ClinicVisitDto,
  ClinicVisitService,
  ClinicVisitStatus,
  CreateUpdateAttendanceLineDto,
  CreateUpdateAttendanceRegisterDto,
  CreateUpdateClinicPrescriptionDto,
  CreateUpdateClinicVisitDto,
  CreateUpdateHostelAllocationDto,
  CreateUpdateHostelRoomDto,
  CreateUpdateLaundryOrderDto,
  CreateUpdateLaundryOrderLineDto,
  CreateUpdateShortCourseApplicationDto,
  CreateUpdateShortCourseParticipantDto,
  CreateUpdateTimetableEntryDto,
  HostelAllocationDto,
  HostelAllocationService,
  HostelAllocationStatus,
  HostelRoomDto,
  HostelRoomService,
  HostelService,
  LaundryItemService,
  LaundryOrderDto,
  LaundryOrderLineDto,
  LaundryOrderLineService,
  LaundryOrderService,
  LaundryStatus,
  LectureRoomService,
  PatientType,
  RoomType,
  ShortCourseApplicationDto,
  ShortCourseApplicationService,
  ShortCourseApplicationStatus,
  ShortCourseApplicationType,
  ShortCourseParticipantDto,
  ShortCourseParticipantService,
  ShortCourseService,
  StudentGender,
  TimetableDay,
  TimetableEntryDto,
  TimetableEntryService,
  TimetableType,
  TreatmentType,
  academicDocumentStatusOptions,
  attendanceMarkOptions,
  clinicVisitStatusOptions,
  hostelAllocationStatusOptions,
  laundryStatusOptions,
  patientTypeOptions,
  roomTypeOptions,
  shortCourseApplicationStatusOptions,
  shortCourseApplicationTypeOptions,
  studentGenderOptions,
  timetableDayOptions,
  timetableTypeOptions,
  treatmentTypeOptions,
} from '@proxy/academics';
import { map } from 'rxjs';
import { RecordEntity, RecordField, SmartButton } from '../erp-shared';
import { accountField, codeField, codeTableEntity, enumOptions } from './entity-helpers';

const PERMISSION = 'Erp.Academics';
const SETUP_PERMISSION = 'Erp.AcademicSetup';
const today = () => new Date().toISOString().substring(0, 10);

/** A read-only figure of a card: what posting or processing filled in. */
function figure(field: string, labelKey: string, section = 'general'): RecordField {
  return { field, labelKey, type: 'readonly', section, cardOnly: true };
}

/** The smart button of a document that opens its lines. */
function linesButton(labelKey: string, route: string, documentNo: string | undefined, count: number): SmartButton {
  return { labelKey, icon: 'fas fa-list', count, routerLink: [route], queryParams: { filter: documentNo }, permission: PERMISSION };
}

/**
 * The campus services around the academic records: timetables, class attendance, hostels, the
 * infirmary, the laundry and short courses. Joined into `MasterDataEntities.all`.
 */
@Injectable({ providedIn: 'root' })
export class CampusEntities {
  private readonly lectureRooms = inject(LectureRoomService);
  private readonly timetable = inject(TimetableEntryService);
  private readonly registers = inject(AttendanceRegisterService);
  private readonly attendanceLines = inject(AttendanceLineService);
  private readonly hostels = inject(HostelService);
  private readonly hostelRooms = inject(HostelRoomService);
  private readonly allocations = inject(HostelAllocationService);
  private readonly visits = inject(ClinicVisitService);
  private readonly prescriptions = inject(ClinicPrescriptionService);
  private readonly laundryItems = inject(LaundryItemService);
  private readonly laundryOrders = inject(LaundryOrderService);
  private readonly laundryLines = inject(LaundryOrderLineService);
  private readonly shortCourses = inject(ShortCourseService);
  private readonly applications = inject(ShortCourseApplicationService);
  private readonly participants = inject(ShortCourseParticipantService);

  // ---------------------------------------------------------------- Timetable

  readonly lectureRoom = codeTableEntity(this.lectureRooms, {
    key: 'lectureRoom',
    titleKey: 'Erp::LectureRoom',
    pluralKey: 'Erp::LectureRooms',
    icon: 'fas fa-door-open',
    permission: SETUP_PERMISSION,
    route: '/erp/lecture-rooms',
    columns: [
      { field: 'roomType', labelKey: 'Erp::RoomType', type: 'select', options: enumOptions(roomTypeOptions, 'RoomType') },
      { field: 'buildingCode', labelKey: 'Erp::BuildingCode' },
      { field: 'maximumCapacity', labelKey: 'Erp::MaximumCapacity', type: 'number' },
      { field: 'blocked', labelKey: 'Erp::Blocked', type: 'boolean' },
    ],
    fields: [
      { field: 'roomType', labelKey: 'Erp::RoomType', type: 'select', options: enumOptions(roomTypeOptions, 'RoomType') },
      { field: 'buildingCode', labelKey: 'Erp::BuildingCode', type: 'text', maxLength: 20 },
      { field: 'maximumCapacity', labelKey: 'Erp::MaximumCapacity', type: 'number', min: 0, helpKey: 'Erp::RoomCapacityHelp' },
      { field: 'blocked', labelKey: 'Erp::Blocked', type: 'checkbox' },
    ],
    defaults: { roomType: RoomType.LectureHall, maximumCapacity: 0, blocked: false },
  });

  readonly timetableEntry: RecordEntity<TimetableEntryDto, CreateUpdateTimetableEntryDto> = {
    key: 'timetableEntry',
    titleKey: 'Erp::TimetableEntry',
    pluralKey: 'Erp::Timetable',
    icon: 'fas fa-calendar-week',
    permission: PERMISSION,
    listRoute: ['/erp/timetable'],
    columns: [
      { field: 'unitCode', labelKey: 'Erp::UnitCode' },
      { field: 'unitDescription', labelKey: 'Erp::Description', width: 200 },
      { field: 'programmeCode', labelKey: 'Erp::ProgrammeCode' },
      { field: 'semesterCode', labelKey: 'Erp::SemesterCode' },
      { field: 'timetableType', labelKey: 'Erp::TimetableType', type: 'select', options: enumOptions(timetableTypeOptions, 'TimetableType') },
      { field: 'day', labelKey: 'Erp::Day', type: 'select', options: enumOptions(timetableDayOptions, 'TimetableDay') },
      { field: 'examDate', labelKey: 'Erp::ExamDate', type: 'date' },
      { field: 'startTime', labelKey: 'Erp::StartTime' },
      { field: 'endTime', labelKey: 'Erp::EndTime' },
      { field: 'roomCode', labelKey: 'Erp::RoomCode' },
      { field: 'lecturerNo', labelKey: 'Erp::LecturerNo' },
    ],
    sections: [{ key: 'general', labelKey: 'Erp::General' }],
    fields: [
      codeField('programmeCode', 'Erp::ProgrammeCode', 'programme', undefined, { required: true }),
      codeField('unitCode', 'Erp::UnitCode', 'courseUnit', undefined, { required: true }),
      codeField('semesterCode', 'Erp::SemesterCode', 'semester', undefined, { helpKey: 'Erp::BlankTakesCurrent' }),
      { field: 'timetableType', labelKey: 'Erp::TimetableType', type: 'select', options: enumOptions(timetableTypeOptions, 'TimetableType') },
      { field: 'day', labelKey: 'Erp::Day', type: 'select', options: enumOptions(timetableDayOptions, 'TimetableDay'), helpKey: 'Erp::TimetableDayHelp' },
      { field: 'examDate', labelKey: 'Erp::ExamDate', type: 'date', helpKey: 'Erp::ExamDateHelp' },
      { field: 'startTime', labelKey: 'Erp::StartTime', type: 'text', required: true, maxLength: 5, placeholderKey: 'Erp::TimePlaceholder' },
      { field: 'endTime', labelKey: 'Erp::EndTime', type: 'text', required: true, maxLength: 5, placeholderKey: 'Erp::TimePlaceholder' },
      codeField('roomCode', 'Erp::RoomCode', 'lectureRoom'),
      codeField('lecturerNo', 'Erp::LecturerNo', 'employee'),
      { field: 'remarks', labelKey: 'Erp::Remarks', type: 'text', maxLength: 250, wide: true, cardOnly: true },
    ],
    getList: query => this.timetable.getList(query),
    get: id => this.timetable.get(id),
    create: input => this.timetable.create(input),
    update: (id, input) => this.timetable.update(id, input),
    delete: id => this.timetable.delete(id),
    toItem: dto => ({ id: dto.id, code: `${dto.unitCode ?? ''} ${dto.startTime ?? ''}`.trim(), name: dto.unitDescription ?? undefined }),
    newRecord: () => ({ timetableType: TimetableType.Teaching, day: TimetableDay.Monday, startTime: '08:00', endTime: '10:00' }),
  };

  // ---------------------------------------------------------------- Class attendance

  readonly attendanceRegister: RecordEntity<AttendanceRegisterDto, CreateUpdateAttendanceRegisterDto> = {
    key: 'attendanceRegister',
    titleKey: 'Erp::AttendanceRegister',
    pluralKey: 'Erp::AttendanceRegisters',
    icon: 'fas fa-clipboard-user',
    permission: PERMISSION,
    listRoute: ['/erp/attendance-registers'],
    parts: [
      {
        entity: 'attendanceLine',
        lines: dto => this.attendanceLines.getList({ documentNo: dto.no, maxResultCount: 1000, skipCount: 0 }).pipe(map(result => result.items ?? [])),
        newLine: dto => ({ documentNo: dto.no }),
        editable: dto => dto.status === AcademicDocumentStatus.Open,
      },
    ],
    attachmentEntityType: 'AttendanceRegister',
    columns: [
      { field: 'no', labelKey: 'Erp::No', width: 120 },
      { field: 'programmeCode', labelKey: 'Erp::ProgrammeCode' },
      { field: 'unitCode', labelKey: 'Erp::UnitCode' },
      { field: 'unitDescription', labelKey: 'Erp::Description', width: 200 },
      { field: 'lessonDate', labelKey: 'Erp::LessonDate', type: 'date' },
      { field: 'status', labelKey: 'Erp::Status', type: 'select', options: enumOptions(academicDocumentStatusOptions, 'AcademicDocumentStatus') },
      { field: 'noOfStudents', labelKey: 'Erp::NoOfStudents', type: 'number' },
      { field: 'noPresent', labelKey: 'Erp::NoPresent', type: 'number' },
    ],
    sections: [{ key: 'general', labelKey: 'Erp::General' }],
    fields: [
      { field: 'no', labelKey: 'Erp::No', type: 'text', maxLength: 20, placeholderKey: 'Erp::NextFromSeries', createOnly: true },
      codeField('programmeCode', 'Erp::ProgrammeCode', 'programme', undefined, { required: true }),
      codeField('unitCode', 'Erp::UnitCode', 'courseUnit', undefined, { required: true }),
      codeField('semesterCode', 'Erp::SemesterCode', 'semester', undefined, { helpKey: 'Erp::BlankTakesCurrent' }),
      { field: 'lessonDate', labelKey: 'Erp::LessonDate', type: 'date', required: true },
      { field: 'startTime', labelKey: 'Erp::StartTime', type: 'text', maxLength: 5, placeholderKey: 'Erp::TimePlaceholder' },
      { field: 'hours', labelKey: 'Erp::Hours', type: 'number', min: 0 },
      codeField('lecturerNo', 'Erp::LecturerNo', 'employee'),
      { field: 'remarks', labelKey: 'Erp::Remarks', type: 'text', maxLength: 250, wide: true, cardOnly: true },
      figure('postedBy', 'Erp::PostedBy'),
    ],
    getList: query => this.registers.getList(query),
    get: id => this.registers.get(id),
    create: input => this.registers.create(input),
    update: (id, input) => this.registers.update(id, input),
    delete: id => this.registers.delete(id),
    toItem: dto => ({ id: dto.id, code: dto.no ?? '', name: `${dto.unitCode ?? ''} ${dto.lessonDate?.substring(0, 10) ?? ''}`.trim() }),
    newRecord: () => ({ lessonDate: today(), hours: 1 }),
    facts: dto => [
      { labelKey: 'Erp::Status', value: AcademicDocumentStatus[dto.status] },
      { labelKey: 'Erp::NoOfStudents', value: dto.noOfStudents, type: 'number' },
      { labelKey: 'Erp::NoPresent', value: dto.noPresent, type: 'number' },
    ],
    actions: [
      {
        key: 'suggest',
        labelKey: 'Erp::SuggestStudents',
        icon: 'fas fa-wand-magic-sparkles',
        permission: `${PERMISSION}.Update`,
        visible: dto => dto.status === AcademicDocumentStatus.Open,
        run: dto => this.registers.suggestLines(dto.id!),
      },
      {
        key: 'post',
        labelKey: 'Erp::Post',
        icon: 'fas fa-paper-plane',
        permission: `${PERMISSION}.Post`,
        visible: dto => dto.status === AcademicDocumentStatus.Open,
        confirmKey: 'Erp::PostAttendanceConfirmation',
        run: dto => this.registers.runPosting(dto.id!),
      },
    ],
    related: dto =>
      this.attendanceLines
        .getList({ documentNo: dto.no, maxResultCount: 1, skipCount: 0 })
        .pipe(map(lines => [linesButton('Erp::AttendanceLines', '/erp/attendance-lines', dto.no, lines.totalCount ?? 0)])),
  };

  readonly attendanceLine: RecordEntity<AttendanceLineDto, CreateUpdateAttendanceLineDto> = {
    key: 'attendanceLine',
    titleKey: 'Erp::AttendanceLine',
    pluralKey: 'Erp::AttendanceLines',
    icon: 'fas fa-list',
    permission: PERMISSION,
    listRoute: ['/erp/attendance-lines'],
    columns: [
      { field: 'documentNo', labelKey: 'Erp::DocumentNo', width: 120 },
      { field: 'studentNo', labelKey: 'Erp::StudentNo' },
      { field: 'studentName', labelKey: 'Erp::StudentName', width: 240 },
      { field: 'mark', labelKey: 'Erp::AttendanceMark', type: 'select', options: enumOptions(attendanceMarkOptions, 'AttendanceMark') },
    ],
    sections: [{ key: 'general', labelKey: 'Erp::General' }],
    fields: [
      codeField('documentNo', 'Erp::DocumentNo', 'attendanceRegister', undefined, { required: true, createOnly: true }),
      codeField('studentNo', 'Erp::StudentNo', 'student', undefined, { required: true }),
      { field: 'mark', labelKey: 'Erp::AttendanceMark', type: 'select', options: enumOptions(attendanceMarkOptions, 'AttendanceMark'), helpKey: 'Erp::AttendanceMarkHelp' },
    ],
    getList: query => this.attendanceLines.getList(query),
    get: id => this.attendanceLines.get(id),
    create: input => this.attendanceLines.create(input),
    update: (id, input) => this.attendanceLines.update(id, input),
    delete: id => this.attendanceLines.delete(id),
    toItem: dto => ({ id: dto.id, code: `${dto.documentNo ?? ''} ${dto.studentNo ?? ''}`.trim(), name: dto.studentName ?? undefined }),
    newRecord: () => ({ mark: AttendanceMark.Present }),
  };

  // ---------------------------------------------------------------- Hostels

  readonly hostel = codeTableEntity(this.hostels, {
    key: 'hostel',
    titleKey: 'Erp::Hostel',
    pluralKey: 'Erp::Hostels',
    icon: 'fas fa-bed',
    permission: SETUP_PERMISSION,
    route: '/erp/hostels',
    columns: [
      { field: 'gender', labelKey: 'Erp::Gender', type: 'select', options: enumOptions(studentGenderOptions, 'StudentGender') },
      { field: 'costPerOccupant', labelKey: 'Erp::CostPerOccupant', type: 'currency' },
      { field: 'feeItemCode', labelKey: 'Erp::FeeItemCode' },
      { field: 'blocked', labelKey: 'Erp::Blocked', type: 'boolean' },
    ],
    fields: [
      { field: 'gender', labelKey: 'Erp::Gender', type: 'select', options: enumOptions(studentGenderOptions, 'StudentGender'), helpKey: 'Erp::HostelGenderHelp' },
      { field: 'costPerOccupant', labelKey: 'Erp::CostPerOccupant', type: 'currency', min: 0, helpKey: 'Erp::CostPerOccupantHelp' },
      codeField('feeItemCode', 'Erp::FeeItemCode', 'feeItem', undefined, { required: true }),
      { field: 'blocked', labelKey: 'Erp::Blocked', type: 'checkbox' },
    ],
    defaults: { gender: StudentGender.None, costPerOccupant: 0, feeItemCode: 'HOSTEL', blocked: false },
    quickCreate: false,
  });

  readonly hostelRoom: RecordEntity<HostelRoomDto, CreateUpdateHostelRoomDto> = {
    key: 'hostelRoom',
    titleKey: 'Erp::HostelRoom',
    pluralKey: 'Erp::HostelRooms',
    icon: 'fas fa-door-closed',
    permission: SETUP_PERMISSION,
    listRoute: ['/erp/hostel-rooms'],
    columns: [
      { field: 'roomNo', labelKey: 'Erp::RoomNo' },
      { field: 'hostelCode', labelKey: 'Erp::HostelCode' },
      { field: 'bedSpaces', labelKey: 'Erp::BedSpaces', type: 'number' },
      { field: 'occupiedSpaces', labelKey: 'Erp::OccupiedSpaces', type: 'number' },
      { field: 'vacantSpaces', labelKey: 'Erp::VacantSpaces', type: 'number', sortable: false, filterable: false },
      { field: 'roomCost', labelKey: 'Erp::RoomCost', type: 'currency' },
      { field: 'outOfOrder', labelKey: 'Erp::OutOfOrder', type: 'boolean' },
    ],
    sections: [{ key: 'general', labelKey: 'Erp::General' }],
    fields: [
      codeField('hostelCode', 'Erp::HostelCode', 'hostel', undefined, { required: true }),
      { field: 'roomNo', labelKey: 'Erp::RoomNo', type: 'text', required: true, maxLength: 20 },
      { field: 'bedSpaces', labelKey: 'Erp::BedSpaces', type: 'number', min: 0 },
      { field: 'roomCost', labelKey: 'Erp::RoomCost', type: 'currency', min: 0, helpKey: 'Erp::RoomCostHelp' },
      { field: 'outOfOrder', labelKey: 'Erp::OutOfOrder', type: 'checkbox' },
      figure('occupiedSpaces', 'Erp::OccupiedSpaces'),
    ],
    getList: query => this.hostelRooms.getList(query),
    get: id => this.hostelRooms.get(id),
    create: input => this.hostelRooms.create(input),
    update: (id, input) => this.hostelRooms.update(id, input),
    delete: id => this.hostelRooms.delete(id),
    toItem: dto => ({ id: dto.id, code: dto.roomNo ?? '', name: `${dto.hostelCode ?? ''} (${dto.vacantSpaces} free)` }),
    newRecord: () => ({ bedSpaces: 2, roomCost: 0, outOfOrder: false }),
    facts: dto => [
      { labelKey: 'Erp::BedSpaces', value: dto.bedSpaces, type: 'number' },
      { labelKey: 'Erp::VacantSpaces', value: dto.vacantSpaces, type: 'number' },
    ],
  };

  readonly hostelAllocation: RecordEntity<HostelAllocationDto, CreateUpdateHostelAllocationDto> = {
    key: 'hostelAllocation',
    titleKey: 'Erp::HostelAllocation',
    pluralKey: 'Erp::HostelAllocations',
    icon: 'fas fa-key',
    permission: PERMISSION,
    listRoute: ['/erp/hostel-allocations'],
    attachmentEntityType: 'HostelAllocation',
    columns: [
      { field: 'no', labelKey: 'Erp::No', width: 120 },
      { field: 'studentNo', labelKey: 'Erp::StudentNo' },
      { field: 'studentName', labelKey: 'Erp::StudentName', width: 200 },
      { field: 'hostelCode', labelKey: 'Erp::HostelCode' },
      { field: 'roomNo', labelKey: 'Erp::RoomNo' },
      { field: 'semesterCode', labelKey: 'Erp::SemesterCode' },
      { field: 'status', labelKey: 'Erp::Status', type: 'select', options: enumOptions(hostelAllocationStatusOptions, 'HostelAllocationStatus') },
      { field: 'charges', labelKey: 'Erp::Charges', type: 'currency' },
    ],
    sections: [{ key: 'general', labelKey: 'Erp::General' }],
    fields: [
      { field: 'no', labelKey: 'Erp::No', type: 'text', maxLength: 20, placeholderKey: 'Erp::NextFromSeries', createOnly: true },
      codeField('studentNo', 'Erp::StudentNo', 'student', undefined, { required: true }),
      codeField('hostelCode', 'Erp::HostelCode', 'hostel', undefined, { required: true }),
      { field: 'roomNo', labelKey: 'Erp::RoomNo', type: 'text', required: true, maxLength: 20 },
      codeField('semesterCode', 'Erp::SemesterCode', 'semester', undefined, { helpKey: 'Erp::BlankTakesCurrent' }),
      { field: 'allocationDate', labelKey: 'Erp::AllocationDate', type: 'date', required: true },
      { field: 'remarks', labelKey: 'Erp::Remarks', type: 'text', maxLength: 250, wide: true, cardOnly: true },
      figure('billNo', 'Erp::BillNo'),
      figure('clearanceDate', 'Erp::ClearanceDate'),
      figure('processedBy', 'Erp::ProcessedBy'),
    ],
    getList: query => this.allocations.getList(query),
    get: id => this.allocations.get(id),
    create: input => this.allocations.create(input),
    update: (id, input) => this.allocations.update(id, input),
    delete: id => this.allocations.delete(id),
    toItem: dto => ({ id: dto.id, code: dto.no ?? '', name: dto.studentName ?? undefined }),
    newRecord: () => ({ allocationDate: today() }),
    facts: dto => [
      { labelKey: 'Erp::Status', value: HostelAllocationStatus[dto.status] },
      { labelKey: 'Erp::Charges', value: dto.charges, type: 'currency' },
      { labelKey: 'Erp::BillNo', value: dto.billNo },
    ],
    actions: [
      {
        key: 'allocate',
        labelKey: 'Erp::Allocate',
        icon: 'fas fa-key',
        permission: `${PERMISSION}.Post`,
        visible: dto => dto.status === HostelAllocationStatus.Booking,
        confirmKey: 'Erp::AllocateHostelConfirmation',
        run: dto => this.allocations.allocate(dto.id!),
      },
      {
        key: 'clear',
        labelKey: 'Erp::ClearRoom',
        icon: 'fas fa-door-open',
        permission: `${PERMISSION}.Post`,
        visible: dto => dto.status === HostelAllocationStatus.Allocated,
        confirmKey: 'Erp::ClearRoomConfirmation',
        run: dto => this.allocations.clear(dto.id!),
      },
    ],
  };

  // ---------------------------------------------------------------- Infirmary

  readonly clinicVisit: RecordEntity<ClinicVisitDto, CreateUpdateClinicVisitDto> = {
    key: 'clinicVisit',
    titleKey: 'Erp::ClinicVisit',
    pluralKey: 'Erp::ClinicVisits',
    icon: 'fas fa-house-medical',
    permission: PERMISSION,
    listRoute: ['/erp/clinic-visits'],
    parts: [
      {
        entity: 'clinicPrescription',
        lines: dto => this.prescriptions.getList({ documentNo: dto.no, maxResultCount: 1000, skipCount: 0 }).pipe(map(result => result.items ?? [])),
        newLine: dto => ({ documentNo: dto.no }),
        editable: dto => dto.status === ClinicVisitStatus.Open,
      },
    ],
    attachmentEntityType: 'ClinicVisit',
    columns: [
      { field: 'no', labelKey: 'Erp::No', width: 120 },
      { field: 'visitDate', labelKey: 'Erp::VisitDate', type: 'date' },
      { field: 'patientType', labelKey: 'Erp::PatientType', type: 'select', options: enumOptions(patientTypeOptions, 'PatientType') },
      { field: 'patientNo', labelKey: 'Erp::PatientNo' },
      { field: 'patientName', labelKey: 'Erp::PatientName', width: 200 },
      { field: 'diagnosis', labelKey: 'Erp::Diagnosis', width: 200 },
      { field: 'offDutyDays', labelKey: 'Erp::OffDutyDays', type: 'number' },
      { field: 'status', labelKey: 'Erp::Status', type: 'select', options: enumOptions(clinicVisitStatusOptions, 'ClinicVisitStatus') },
    ],
    sections: [
      { key: 'general', labelKey: 'Erp::General' },
      { key: 'clinical', labelKey: 'Erp::Clinical' },
      { key: 'sickSheet', labelKey: 'Erp::SickSheet', collapsed: true },
    ],
    fields: [
      { field: 'no', labelKey: 'Erp::No', type: 'text', maxLength: 20, placeholderKey: 'Erp::NextFromSeries', createOnly: true },
      { field: 'patientType', labelKey: 'Erp::PatientType', type: 'select', options: enumOptions(patientTypeOptions, 'PatientType') },
      { field: 'patientNo', labelKey: 'Erp::PatientNo', type: 'text', maxLength: 20, helpKey: 'Erp::PatientNoHelp' },
      { field: 'patientName', labelKey: 'Erp::PatientName', type: 'text', maxLength: 200, helpKey: 'Erp::PatientNameHelp' },
      { field: 'visitDate', labelKey: 'Erp::VisitDate', type: 'date', required: true },
      { field: 'treatmentType', labelKey: 'Erp::TreatmentType', type: 'select', options: enumOptions(treatmentTypeOptions, 'TreatmentType'), section: 'clinical' },
      { field: 'complaint', labelKey: 'Erp::Complaint', type: 'text', maxLength: 250, wide: true, section: 'clinical' },
      { field: 'diagnosis', labelKey: 'Erp::Diagnosis', type: 'text', maxLength: 250, wide: true, section: 'clinical' },
      { field: 'treatment', labelKey: 'Erp::Treatment', type: 'text', maxLength: 250, wide: true, section: 'clinical', cardOnly: true },
      codeField('attendedBy', 'Erp::AttendedBy', 'employee', 'clinical', { cardOnly: true }),
      { field: 'referredTo', labelKey: 'Erp::ReferredTo', type: 'text', maxLength: 100, section: 'clinical', cardOnly: true, helpKey: 'Erp::ReferredToHelp' },
      { field: 'charge', labelKey: 'Erp::Charge', type: 'currency', min: 0, section: 'clinical', helpKey: 'Erp::ClinicChargeHelp' },
      { field: 'offDutyFrom', labelKey: 'Erp::OffDutyFrom', type: 'date', section: 'sickSheet', cardOnly: true },
      { field: 'offDutyTo', labelKey: 'Erp::OffDutyTo', type: 'date', section: 'sickSheet', cardOnly: true },
      { field: 'lightDutyDays', labelKey: 'Erp::LightDutyDays', type: 'number', min: 0, section: 'sickSheet', cardOnly: true },
      { field: 'offDutyComments', labelKey: 'Erp::OffDutyComments', type: 'text', maxLength: 250, wide: true, section: 'sickSheet', cardOnly: true },
      figure('offDutyDays', 'Erp::OffDutyDays', 'sickSheet'),
      figure('billNo', 'Erp::BillNo'),
      figure('completedBy', 'Erp::CompletedBy'),
    ],
    getList: query => this.visits.getList(query),
    get: id => this.visits.get(id),
    create: input => this.visits.create(input),
    update: (id, input) => this.visits.update(id, input),
    delete: id => this.visits.delete(id),
    toItem: dto => ({ id: dto.id, code: dto.no ?? '', name: dto.patientName ?? undefined }),
    newRecord: () => ({ patientType: PatientType.Student, treatmentType: TreatmentType.Outpatient, visitDate: today(), lightDutyDays: 0, charge: 0 }),
    facts: dto => [
      { labelKey: 'Erp::Status', value: ClinicVisitStatus[dto.status] },
      { labelKey: 'Erp::OffDutyDays', value: dto.offDutyDays, type: 'number' },
      { labelKey: 'Erp::Charge', value: dto.charge, type: 'currency' },
    ],
    actions: [
      {
        key: 'complete',
        labelKey: 'Erp::CompleteVisit',
        icon: 'fas fa-check',
        permission: `${PERMISSION}.Post`,
        visible: dto => dto.status === ClinicVisitStatus.Open,
        confirmKey: 'Erp::CompleteVisitConfirmation',
        run: dto => this.visits.complete(dto.id!),
      },
    ],
    related: dto =>
      this.prescriptions
        .getList({ documentNo: dto.no, maxResultCount: 1, skipCount: 0 })
        .pipe(map(lines => [linesButton('Erp::ClinicPrescriptions', '/erp/clinic-prescriptions', dto.no, lines.totalCount ?? 0)])),
  };

  readonly clinicPrescription: RecordEntity<ClinicPrescriptionDto, CreateUpdateClinicPrescriptionDto> = {
    key: 'clinicPrescription',
    titleKey: 'Erp::ClinicPrescription',
    pluralKey: 'Erp::ClinicPrescriptions',
    icon: 'fas fa-prescription-bottle-medical',
    permission: PERMISSION,
    listRoute: ['/erp/clinic-prescriptions'],
    columns: [
      { field: 'documentNo', labelKey: 'Erp::DocumentNo', width: 120 },
      { field: 'itemNo', labelKey: 'Erp::ItemNo' },
      { field: 'description', labelKey: 'Erp::Description', width: 220 },
      { field: 'quantity', labelKey: 'Erp::Quantity', type: 'number' },
      { field: 'dosage', labelKey: 'Erp::Dosage', width: 200 },
      { field: 'issued', labelKey: 'Erp::Issued', type: 'boolean' },
    ],
    sections: [{ key: 'general', labelKey: 'Erp::General' }],
    fields: [
      codeField('documentNo', 'Erp::DocumentNo', 'clinicVisit', undefined, { required: true, createOnly: true }),
      codeField('itemNo', 'Erp::ItemNo', 'item', undefined, { helpKey: 'Erp::PrescriptionItemHelp' }),
      { field: 'description', labelKey: 'Erp::Description', type: 'text', maxLength: 250 },
      { field: 'quantity', labelKey: 'Erp::Quantity', type: 'number', min: 0 },
      { field: 'dosage', labelKey: 'Erp::Dosage', type: 'text', maxLength: 250 },
      codeField('locationCode', 'Erp::LocationCode', 'location'),
    ],
    getList: query => this.prescriptions.getList(query),
    get: id => this.prescriptions.get(id),
    create: input => this.prescriptions.create(input),
    update: (id, input) => this.prescriptions.update(id, input),
    delete: id => this.prescriptions.delete(id),
    toItem: dto => ({ id: dto.id, code: `${dto.documentNo ?? ''} ${dto.itemNo ?? ''}`.trim(), name: dto.description ?? undefined }),
    newRecord: () => ({ quantity: 1 }),
  };

  // ---------------------------------------------------------------- Laundry

  readonly laundryItem = codeTableEntity(this.laundryItems, {
    key: 'laundryItem',
    titleKey: 'Erp::LaundryItem',
    pluralKey: 'Erp::LaundryItems',
    icon: 'fas fa-shirt',
    permission: SETUP_PERMISSION,
    route: '/erp/laundry-items',
    columns: [
      { field: 'ratePerItem', labelKey: 'Erp::RatePerItem', type: 'currency' },
      { field: 'glAccountNo', labelKey: 'Erp::GLAccountNo' },
    ],
    fields: [
      { field: 'ratePerItem', labelKey: 'Erp::RatePerItem', type: 'currency', min: 0 },
      accountField('glAccountNo', 'Erp::GLAccountNo', 'general', true),
    ],
    defaults: { ratePerItem: 0 },
    quickCreate: false,
  });

  readonly laundryOrder: RecordEntity<LaundryOrderDto, CreateUpdateLaundryOrderDto> = {
    key: 'laundryOrder',
    titleKey: 'Erp::LaundryOrder',
    pluralKey: 'Erp::LaundryOrders',
    icon: 'fas fa-soap',
    permission: PERMISSION,
    listRoute: ['/erp/laundry-orders'],
    parts: [
      {
        entity: 'laundryOrderLine',
        lines: dto => this.laundryLines.getList({ documentNo: dto.no, maxResultCount: 1000, skipCount: 0 }).pipe(map(result => result.items ?? [])),
        newLine: dto => ({ documentNo: dto.no }),
        editable: dto => dto.status === LaundryStatus.Received,
      },
    ],
    attachmentEntityType: 'LaundryOrder',
    columns: [
      { field: 'no', labelKey: 'Erp::No', width: 120 },
      { field: 'customerNo', labelKey: 'Erp::CustomerNo' },
      { field: 'customerName', labelKey: 'Erp::CustomerName', width: 200 },
      { field: 'receivedDate', labelKey: 'Erp::ReceivedDate', type: 'date' },
      { field: 'express', labelKey: 'Erp::Express', type: 'boolean' },
      { field: 'status', labelKey: 'Erp::Status', type: 'select', options: enumOptions(laundryStatusOptions, 'LaundryStatus') },
      { field: 'totalAmount', labelKey: 'Erp::TotalAmount', type: 'currency' },
    ],
    sections: [{ key: 'general', labelKey: 'Erp::General' }],
    fields: [
      { field: 'no', labelKey: 'Erp::No', type: 'text', maxLength: 20, placeholderKey: 'Erp::NextFromSeries', createOnly: true },
      codeField('customerNo', 'Erp::CustomerNo', 'customer', undefined, { required: true, helpKey: 'Erp::LaundryCustomerHelp' }),
      { field: 'receivedDate', labelKey: 'Erp::ReceivedDate', type: 'date', required: true },
      { field: 'promisedDate', labelKey: 'Erp::PromisedDate', type: 'date' },
      { field: 'express', labelKey: 'Erp::Express', type: 'checkbox', helpKey: 'Erp::LaundryExpressHelp' },
      { field: 'discountPct', labelKey: 'Erp::DiscountPct', type: 'number', min: 0 },
      { field: 'remarks', labelKey: 'Erp::Remarks', type: 'text', maxLength: 250, wide: true, cardOnly: true },
      figure('collectedDate', 'Erp::CollectedDate'),
      figure('processedBy', 'Erp::ProcessedBy'),
    ],
    getList: query => this.laundryOrders.getList(query),
    get: id => this.laundryOrders.get(id),
    create: input => this.laundryOrders.create(input),
    update: (id, input) => this.laundryOrders.update(id, input),
    delete: id => this.laundryOrders.delete(id),
    toItem: dto => ({ id: dto.id, code: dto.no ?? '', name: dto.customerName ?? undefined }),
    newRecord: () => ({ receivedDate: today(), express: false, discountPct: 0 }),
    facts: dto => [
      { labelKey: 'Erp::Status', value: LaundryStatus[dto.status] },
      { labelKey: 'Erp::TotalAmount', value: dto.totalAmount, type: 'currency' },
    ],
    actions: [
      {
        key: 'invoice',
        labelKey: 'Erp::InvoiceOrder',
        icon: 'fas fa-file-invoice-dollar',
        permission: `${PERMISSION}.Post`,
        visible: dto => dto.status === LaundryStatus.Received,
        confirmKey: 'Erp::InvoiceLaundryConfirmation',
        run: dto => this.laundryOrders.invoice(dto.id!),
      },
      {
        key: 'ready',
        labelKey: 'Erp::MarkReady',
        icon: 'fas fa-check',
        permission: `${PERMISSION}.Update`,
        visible: dto => dto.status === LaundryStatus.Invoiced,
        run: dto => this.laundryOrders.markReady(dto.id!),
      },
      {
        key: 'collected',
        labelKey: 'Erp::MarkCollected',
        icon: 'fas fa-hand-holding',
        permission: `${PERMISSION}.Update`,
        visible: dto => dto.status === LaundryStatus.Ready,
        run: dto => this.laundryOrders.markCollected(dto.id!),
      },
    ],
    related: dto =>
      this.laundryLines
        .getList({ documentNo: dto.no, maxResultCount: 1, skipCount: 0 })
        .pipe(map(lines => [linesButton('Erp::LaundryOrderLines', '/erp/laundry-order-lines', dto.no, lines.totalCount ?? 0)])),
  };

  readonly laundryOrderLine: RecordEntity<LaundryOrderLineDto, CreateUpdateLaundryOrderLineDto> = {
    key: 'laundryOrderLine',
    titleKey: 'Erp::LaundryOrderLine',
    pluralKey: 'Erp::LaundryOrderLines',
    icon: 'fas fa-list',
    permission: PERMISSION,
    listRoute: ['/erp/laundry-order-lines'],
    columns: [
      { field: 'documentNo', labelKey: 'Erp::DocumentNo', width: 120 },
      { field: 'laundryItemCode', labelKey: 'Erp::LaundryItemCode' },
      { field: 'description', labelKey: 'Erp::Description', width: 220 },
      { field: 'quantity', labelKey: 'Erp::Quantity', type: 'number' },
      { field: 'unitPrice', labelKey: 'Erp::UnitPrice', type: 'currency' },
      { field: 'amount', labelKey: 'Erp::Amount', type: 'currency' },
    ],
    sections: [{ key: 'general', labelKey: 'Erp::General' }],
    fields: [
      codeField('documentNo', 'Erp::DocumentNo', 'laundryOrder', undefined, { required: true, createOnly: true }),
      codeField('laundryItemCode', 'Erp::LaundryItemCode', 'laundryItem', undefined, { required: true }),
      { field: 'description', labelKey: 'Erp::Description', type: 'text', maxLength: 250 },
      { field: 'quantity', labelKey: 'Erp::Quantity', type: 'number', min: 0 },
      { field: 'unitPrice', labelKey: 'Erp::UnitPrice', type: 'currency', min: 0, helpKey: 'Erp::LaundryUnitPriceHelp' },
      figure('amount', 'Erp::Amount'),
    ],
    getList: query => this.laundryLines.getList(query),
    get: id => this.laundryLines.get(id),
    create: input => this.laundryLines.create(input),
    update: (id, input) => this.laundryLines.update(id, input),
    delete: id => this.laundryLines.delete(id),
    toItem: dto => ({ id: dto.id, code: `${dto.documentNo ?? ''} ${dto.laundryItemCode ?? ''}`.trim(), name: dto.description ?? undefined }),
    newRecord: () => ({ quantity: 1 }),
  };

  // ---------------------------------------------------------------- Short courses

  readonly shortCourse = codeTableEntity(this.shortCourses, {
    key: 'shortCourse',
    titleKey: 'Erp::ShortCourse',
    pluralKey: 'Erp::ShortCourses',
    icon: 'fas fa-chalkboard',
    permission: SETUP_PERMISSION,
    route: '/erp/short-courses',
    columns: [
      { field: 'durationDays', labelKey: 'Erp::DurationDays', type: 'number' },
      { field: 'feePerParticipant', labelKey: 'Erp::FeePerParticipant', type: 'currency' },
      { field: 'maxParticipants', labelKey: 'Erp::MaxParticipants', type: 'number' },
      { field: 'active', labelKey: 'Erp::Active', type: 'boolean' },
    ],
    fields: [
      { field: 'durationDays', labelKey: 'Erp::DurationDays', type: 'number', min: 0 },
      { field: 'feePerParticipant', labelKey: 'Erp::FeePerParticipant', type: 'currency', min: 0 },
      accountField('glAccountNo', 'Erp::GLAccountNo', 'general', true),
      { field: 'maxParticipants', labelKey: 'Erp::MaxParticipants', type: 'number', min: 0, helpKey: 'Erp::MaxParticipantsHelp' },
      { field: 'active', labelKey: 'Erp::Active', type: 'checkbox' },
    ],
    defaults: { durationDays: 1, feePerParticipant: 0, maxParticipants: 0, active: true },
    quickCreate: false,
  });

  readonly shortCourseApplication: RecordEntity<ShortCourseApplicationDto, CreateUpdateShortCourseApplicationDto> = {
    key: 'shortCourseApplication',
    titleKey: 'Erp::ShortCourseApplication',
    pluralKey: 'Erp::ShortCourseApplications',
    icon: 'fas fa-user-graduate',
    permission: PERMISSION,
    listRoute: ['/erp/short-course-applications'],
    parts: [
      {
        entity: 'shortCourseParticipant',
        lines: dto => this.participants.getList({ documentNo: dto.no, maxResultCount: 1000, skipCount: 0 }).pipe(map(result => result.items ?? [])),
        newLine: dto => ({ documentNo: dto.no }),
        editable: dto => dto.status === ShortCourseApplicationStatus.Open,
      },
    ],
    attachmentEntityType: 'ShortCourseApplication',
    columns: [
      { field: 'no', labelKey: 'Erp::No', width: 120 },
      { field: 'applicationType', labelKey: 'Erp::ApplicationType', type: 'select', options: enumOptions(shortCourseApplicationTypeOptions, 'ShortCourseApplicationType') },
      { field: 'courseCode', labelKey: 'Erp::CourseCode' },
      { field: 'startDate', labelKey: 'Erp::StartDate', type: 'date' },
      { field: 'customerName', labelKey: 'Erp::CustomerName', width: 200 },
      { field: 'noOfParticipants', labelKey: 'Erp::NoOfParticipants', type: 'number' },
      { field: 'status', labelKey: 'Erp::Status', type: 'select', options: enumOptions(shortCourseApplicationStatusOptions, 'ShortCourseApplicationStatus') },
      { field: 'billedAmount', labelKey: 'Erp::BilledAmount', type: 'currency' },
    ],
    sections: [{ key: 'general', labelKey: 'Erp::General' }],
    fields: [
      { field: 'no', labelKey: 'Erp::No', type: 'text', maxLength: 20, placeholderKey: 'Erp::NextFromSeries', createOnly: true },
      {
        field: 'applicationType',
        labelKey: 'Erp::ApplicationType',
        type: 'select',
        options: enumOptions(shortCourseApplicationTypeOptions, 'ShortCourseApplicationType'),
        helpKey: 'Erp::ShortCourseApplicationTypeHelp',
      },
      codeField('courseCode', 'Erp::CourseCode', 'shortCourse', undefined, { required: true }),
      { field: 'applicationDate', labelKey: 'Erp::ApplicationDate', type: 'date', required: true },
      { field: 'startDate', labelKey: 'Erp::StartDate', type: 'date', required: true },
      { field: 'endDate', labelKey: 'Erp::EndDate', type: 'date', helpKey: 'Erp::ShortCourseEndDateHelp' },
      codeField('customerNo', 'Erp::SponsorCustomerNo', 'customer', undefined, { helpKey: 'Erp::ShortCourseSponsorHelp' }),
      { field: 'feePerParticipant', labelKey: 'Erp::FeePerParticipant', type: 'currency', min: 0, helpKey: 'Erp::ShortCourseFeeHelp' },
      { field: 'remarks', labelKey: 'Erp::Remarks', type: 'text', maxLength: 250, wide: true, cardOnly: true },
      figure('rejectionReason', 'Erp::RejectionReason'),
      figure('processedBy', 'Erp::ProcessedBy'),
    ],
    getList: query => this.applications.getList(query),
    get: id => this.applications.get(id),
    create: input => this.applications.create(input),
    update: (id, input) => this.applications.update(id, input),
    delete: id => this.applications.delete(id),
    toItem: dto => ({ id: dto.id, code: dto.no ?? '', name: `${dto.courseCode ?? ''} ${dto.customerName ?? ''}`.trim() }),
    newRecord: () => ({ applicationType: ShortCourseApplicationType.Individual, applicationDate: today(), startDate: today(), feePerParticipant: 0 }),
    facts: dto => [
      { labelKey: 'Erp::Status', value: ShortCourseApplicationStatus[dto.status] },
      { labelKey: 'Erp::NoOfParticipants', value: dto.noOfParticipants, type: 'number' },
      { labelKey: 'Erp::BilledAmount', value: dto.billedAmount, type: 'currency' },
    ],
    actions: [
      {
        key: 'submit',
        labelKey: 'Erp::Submit',
        icon: 'fas fa-paper-plane',
        permission: `${PERMISSION}.Update`,
        visible: dto => dto.status === ShortCourseApplicationStatus.Open,
        run: dto => this.applications.submit(dto.id!),
      },
      {
        key: 'approve',
        labelKey: 'Erp::Approve',
        icon: 'fas fa-check',
        permission: `${PERMISSION}.Post`,
        visible: dto => dto.status === ShortCourseApplicationStatus.Submitted,
        run: dto => this.applications.approve(dto.id!),
      },
      {
        key: 'reopen',
        labelKey: 'Erp::Reopen',
        icon: 'fas fa-rotate-left',
        permission: `${PERMISSION}.Update`,
        visible: dto =>
          [ShortCourseApplicationStatus.Submitted, ShortCourseApplicationStatus.Approved, ShortCourseApplicationStatus.Rejected].includes(dto.status),
        run: dto => this.applications.reopen(dto.id!),
      },
      {
        key: 'register',
        labelKey: 'Erp::RegisterParticipants',
        icon: 'fas fa-id-card',
        permission: `${PERMISSION}.Post`,
        visible: dto => dto.status === ShortCourseApplicationStatus.Approved,
        confirmKey: 'Erp::RegisterParticipantsConfirmation',
        run: dto => this.applications.register(dto.id!),
      },
    ],
    related: dto =>
      this.participants
        .getList({ documentNo: dto.no, maxResultCount: 1, skipCount: 0 })
        .pipe(map(lines => [linesButton('Erp::ShortCourseParticipants', '/erp/short-course-participants', dto.no, lines.totalCount ?? 0)])),
  };

  readonly shortCourseParticipant: RecordEntity<ShortCourseParticipantDto, CreateUpdateShortCourseParticipantDto> = {
    key: 'shortCourseParticipant',
    titleKey: 'Erp::ShortCourseParticipant',
    pluralKey: 'Erp::ShortCourseParticipants',
    icon: 'fas fa-users',
    permission: PERMISSION,
    listRoute: ['/erp/short-course-participants'],
    columns: [
      { field: 'documentNo', labelKey: 'Erp::DocumentNo', width: 120 },
      { field: 'name', labelKey: 'Erp::Name', width: 220 },
      { field: 'nationalId', labelKey: 'Erp::NationalId' },
      { field: 'phoneNo', labelKey: 'Erp::PhoneNo' },
      { field: 'email', labelKey: 'Erp::Email' },
      { field: 'certificateNo', labelKey: 'Erp::CertificateNo' },
    ],
    sections: [{ key: 'general', labelKey: 'Erp::General' }],
    fields: [
      codeField('documentNo', 'Erp::DocumentNo', 'shortCourseApplication', undefined, { required: true, createOnly: true }),
      { field: 'name', labelKey: 'Erp::Name', type: 'text', required: true, maxLength: 200 },
      { field: 'nationalId', labelKey: 'Erp::NationalId', type: 'text', maxLength: 40 },
      { field: 'phoneNo', labelKey: 'Erp::PhoneNo', type: 'text', maxLength: 30 },
      { field: 'email', labelKey: 'Erp::Email', type: 'email', maxLength: 80 },
      figure('certificateNo', 'Erp::CertificateNo'),
    ],
    getList: query => this.participants.getList(query),
    get: id => this.participants.get(id),
    create: input => this.participants.create(input),
    update: (id, input) => this.participants.update(id, input),
    delete: id => this.participants.delete(id),
    toItem: dto => ({ id: dto.id, code: `${dto.documentNo ?? ''} ${dto.lineNo}`.trim(), name: dto.name ?? undefined }),
    newRecord: () => ({}),
  };

  get all(): RecordEntity[] {
    return [
      this.lectureRoom,
      this.timetableEntry,
      this.attendanceRegister,
      this.attendanceLine,
      this.hostel,
      this.hostelRoom,
      this.hostelAllocation,
      this.clinicVisit,
      this.clinicPrescription,
      this.laundryItem,
      this.laundryOrder,
      this.laundryOrderLine,
      this.shortCourse,
      this.shortCourseApplication,
      this.shortCourseParticipant,
    ];
  }
}
