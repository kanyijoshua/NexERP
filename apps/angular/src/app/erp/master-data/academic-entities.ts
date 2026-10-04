import { Injectable, inject } from '@angular/core';
import {
  AcademicDocumentStatus,
  AcademicYearService,
  ApplicationStatus,
  CourseUnitDto,
  CourseUnitService,
  CreateUpdateCourseUnitDto,
  CreateUpdateExamComponentDto,
  CreateUpdateExamResultHeaderDto,
  CreateUpdateExamResultLineDto,
  CreateUpdateFeeStructureLineDto,
  CreateUpdateGradingBandDto,
  CreateUpdateProgrammeStageDto,
  CreateUpdateSemesterRegistrationDto,
  CreateUpdateStudentApplicationDto,
  CreateUpdateStudentBillHeaderDto,
  CreateUpdateStudentBillLineDto,
  CreateUpdateStudentDto,
  CreateUpdateStudentUnitDto,
  ExamCategoryService,
  ExamComponentDto,
  ExamComponentService,
  ExamResultHeaderDto,
  ExamResultLineDto,
  ExamResultLineService,
  ExamResultService,
  ExamType,
  FeeItemService,
  FeeStructureLineDto,
  FeeStructureLineService,
  FeeType,
  GradingBandDto,
  GradingBandService,
  IntakeService,
  ProgrammeService,
  ProgrammeStageDto,
  ProgrammeStageService,
  RegisterFor,
  RegistrationStatus,
  SemesterRegistrationDto,
  SemesterRegistrationService,
  SemesterService,
  StudentApplicationDto,
  StudentApplicationService,
  StudentBillHeaderDto,
  StudentBillLineDto,
  StudentBillLineService,
  StudentBillService,
  StudentDto,
  StudentService,
  StudentStatus,
  StudentUnitDto,
  StudentUnitService,
  StudyMode,
  academicDocumentStatusOptions,
  applicationStatusOptions,
  examTypeOptions,
  feeTypeOptions,
  programmeLevelOptions,
  registerForOptions,
  registrationStatusOptions,
  studentGenderOptions,
  studentSponsorshipOptions,
  studentStatusOptions,
  studyModeOptions,
  unitTypeOptions,
} from '@proxy/academics';
import { forkJoin, map } from 'rxjs';
import { RecordEntity, RecordField } from '../erp-shared';
import { accountField, codeField, codeTableEntity, enumOptions } from './entity-helpers';

const PERMISSION = 'Erp.Academics';
const SETUP_PERMISSION = 'Erp.AcademicSetup';
const today = () => new Date().toISOString().substring(0, 10);
const money = (value: number | undefined) => (value ?? 0).toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 });

/** A read-only figure of a card: a mark, a total or what posting filled in. */
function figure(field: string, labelKey: string, section: string): RecordField {
  return { field, labelKey, type: 'readonly', section, cardOnly: true };
}

/** The fields of a calendar table: its dates and the mark on the period in progress. */
function periodFields(): RecordField[] {
  return [
    { field: 'startDate', labelKey: 'Erp::StartDate', type: 'date' },
    { field: 'endDate', labelKey: 'Erp::EndDate', type: 'date' },
    { field: 'current', labelKey: 'Erp::Current', type: 'checkbox', helpKey: 'Erp::CurrentPeriodHelp' },
  ];
}

/** Who a person is and how to reach them: shared by the application and the student card. */
function personFields(): RecordField[] {
  return [
    { field: 'firstName', labelKey: 'Erp::FirstName', type: 'text', required: true, maxLength: 50 },
    { field: 'otherName', labelKey: 'Erp::OtherName', type: 'text', maxLength: 50 },
    { field: 'lastName', labelKey: 'Erp::LastName', type: 'text', maxLength: 50 },
    { field: 'gender', labelKey: 'Erp::Gender', type: 'select', options: enumOptions(studentGenderOptions, 'StudentGender'), section: 'personal', cardOnly: true },
    { field: 'dateOfBirth', labelKey: 'Erp::DateOfBirth', type: 'date', section: 'personal', cardOnly: true },
    { field: 'nationalId', labelKey: 'Erp::NationalId', type: 'text', maxLength: 40, section: 'personal', cardOnly: true },
    {
      field: 'sponsorship',
      labelKey: 'Erp::Sponsorship',
      type: 'select',
      options: enumOptions(studentSponsorshipOptions, 'StudentSponsorship'),
      section: 'personal',
      cardOnly: true,
    },
    { field: 'guardianName', labelKey: 'Erp::GuardianName', type: 'text', maxLength: 100, section: 'personal', cardOnly: true },
    { field: 'guardianPhoneNo', labelKey: 'Erp::GuardianPhoneNo', type: 'text', maxLength: 30, section: 'personal', cardOnly: true },
    { field: 'phoneNo', labelKey: 'Erp::PhoneNo', type: 'text', maxLength: 30, section: 'contact' },
    { field: 'email', labelKey: 'Erp::Email', type: 'email', maxLength: 80, section: 'contact' },
    { field: 'address', labelKey: 'Erp::Address', type: 'text', maxLength: 100, section: 'contact', cardOnly: true },
    { field: 'city', labelKey: 'Erp::City', type: 'text', maxLength: 50, section: 'contact', cardOnly: true },
  ];
}

/** The programme a person applies to or studies, and when they join it. */
function courseFields(): RecordField[] {
  return [
    codeField('programmeCode', 'Erp::ProgrammeCode', 'programme', undefined, { required: true }),
    codeField('intakeCode', 'Erp::IntakeCode', 'intake', undefined, { helpKey: 'Erp::BlankTakesCurrent' }),
    codeField('academicYearCode', 'Erp::AcademicYearCode', 'academicYear', undefined, { cardOnly: true, helpKey: 'Erp::BlankTakesCurrent' }),
    {
      field: 'studyMode',
      labelKey: 'Erp::StudyMode',
      type: 'select',
      options: enumOptions(studyModeOptions, 'StudyMode').filter(o => o.value !== StudyMode.None),
    },
  ];
}

/**
 * The academic module: the calendar, programmes with their stages and units, grading, the fee
 * structure, applications, students, semester registrations, student bills and exam results.
 * Joined into `MasterDataEntities.all`, so every lookup can find them.
 */
@Injectable({ providedIn: 'root' })
export class AcademicEntities {
  private readonly years = inject(AcademicYearService);
  private readonly semesters = inject(SemesterService);
  private readonly intakes = inject(IntakeService);
  private readonly examCategories = inject(ExamCategoryService);
  private readonly gradingBands = inject(GradingBandService);
  private readonly examComponents = inject(ExamComponentService);
  private readonly programmes = inject(ProgrammeService);
  private readonly stages = inject(ProgrammeStageService);
  private readonly courseUnits = inject(CourseUnitService);
  private readonly feeItems = inject(FeeItemService);
  private readonly feeStructure = inject(FeeStructureLineService);
  private readonly applications = inject(StudentApplicationService);
  private readonly students = inject(StudentService);
  private readonly registrations = inject(SemesterRegistrationService);
  private readonly studentUnits = inject(StudentUnitService);
  private readonly bills = inject(StudentBillService);
  private readonly billLines = inject(StudentBillLineService);
  private readonly examResults = inject(ExamResultService);
  private readonly examResultLines = inject(ExamResultLineService);

  // ---------------------------------------------------------------- Calendar

  readonly academicYear = codeTableEntity(this.years, {
    key: 'academicYear',
    titleKey: 'Erp::AcademicYear',
    pluralKey: 'Erp::AcademicYears',
    icon: 'fas fa-calendar',
    permission: SETUP_PERMISSION,
    route: '/erp/academic-years',
    columns: [
      { field: 'startDate', labelKey: 'Erp::StartDate', type: 'date' },
      { field: 'endDate', labelKey: 'Erp::EndDate', type: 'date' },
      { field: 'current', labelKey: 'Erp::Current', type: 'boolean' },
    ],
    fields: periodFields(),
    defaults: { current: false },
    quickCreate: false,
  });

  readonly semester = codeTableEntity(this.semesters, {
    key: 'semester',
    titleKey: 'Erp::Semester',
    pluralKey: 'Erp::Semesters',
    icon: 'fas fa-calendar-week',
    permission: SETUP_PERMISSION,
    route: '/erp/semesters',
    columns: [
      { field: 'academicYearCode', labelKey: 'Erp::AcademicYearCode' },
      { field: 'startDate', labelKey: 'Erp::StartDate', type: 'date' },
      { field: 'endDate', labelKey: 'Erp::EndDate', type: 'date' },
      { field: 'current', labelKey: 'Erp::Current', type: 'boolean' },
    ],
    fields: [
      codeField('academicYearCode', 'Erp::AcademicYearCode', 'academicYear'),
      ...periodFields(),
      { field: 'registrationFrom', labelKey: 'Erp::RegistrationFrom', type: 'date', helpKey: 'Erp::RegistrationDatesHelp' },
      { field: 'registrationTo', labelKey: 'Erp::RegistrationTo', type: 'date' },
    ],
    defaults: { current: false },
    quickCreate: false,
  });

  readonly intake = codeTableEntity(this.intakes, {
    key: 'intake',
    titleKey: 'Erp::Intake',
    pluralKey: 'Erp::Intakes',
    icon: 'fas fa-user-plus',
    permission: SETUP_PERMISSION,
    route: '/erp/intakes',
    columns: [
      { field: 'academicYearCode', labelKey: 'Erp::AcademicYearCode' },
      { field: 'startDate', labelKey: 'Erp::StartDate', type: 'date' },
      { field: 'endDate', labelKey: 'Erp::EndDate', type: 'date' },
      { field: 'current', labelKey: 'Erp::Current', type: 'boolean' },
    ],
    fields: [codeField('academicYearCode', 'Erp::AcademicYearCode', 'academicYear'), ...periodFields()],
    defaults: { current: false },
    quickCreate: false,
  });

  // ---------------------------------------------------------------- Grading

  readonly examCategory = codeTableEntity(this.examCategories, {
    key: 'examCategory',
    titleKey: 'Erp::ExamCategory',
    pluralKey: 'Erp::ExamCategories',
    icon: 'fas fa-award',
    permission: SETUP_PERMISSION,
    route: '/erp/exam-categories',
    columns: [{ field: 'blockResultsEntry', labelKey: 'Erp::BlockResultsEntry', type: 'boolean' }],
    fields: [{ field: 'blockResultsEntry', labelKey: 'Erp::BlockResultsEntry', type: 'checkbox' }],
    defaults: { blockResultsEntry: false },
  });

  readonly gradingBand: RecordEntity<GradingBandDto, CreateUpdateGradingBandDto> = {
    key: 'gradingBand',
    titleKey: 'Erp::GradingBand',
    pluralKey: 'Erp::GradingBands',
    icon: 'fas fa-ranking-star',
    permission: SETUP_PERMISSION,
    listRoute: ['/erp/grading-bands'],
    columns: [
      { field: 'examCategoryCode', labelKey: 'Erp::ExamCategoryCode' },
      { field: 'grade', labelKey: 'Erp::Grade' },
      { field: 'fromMark', labelKey: 'Erp::FromMark', type: 'number' },
      { field: 'toMark', labelKey: 'Erp::ToMark', type: 'number' },
      { field: 'points', labelKey: 'Erp::Points', type: 'number' },
      { field: 'remarks', labelKey: 'Erp::Remarks' },
      { field: 'passed', labelKey: 'Erp::Passed', type: 'boolean' },
    ],
    sections: [{ key: 'general', labelKey: 'Erp::General' }],
    fields: [
      codeField('examCategoryCode', 'Erp::ExamCategoryCode', 'examCategory', undefined, { required: true }),
      { field: 'grade', labelKey: 'Erp::Grade', type: 'text', required: true, maxLength: 20 },
      { field: 'fromMark', labelKey: 'Erp::FromMark', type: 'number', min: 0 },
      { field: 'toMark', labelKey: 'Erp::ToMark', type: 'number', min: 0 },
      { field: 'points', labelKey: 'Erp::Points', type: 'number', min: 0 },
      { field: 'passed', labelKey: 'Erp::Passed', type: 'checkbox' },
      { field: 'remarks', labelKey: 'Erp::Remarks', type: 'text', maxLength: 100, helpKey: 'Erp::GradeRemarksHelp' },
      { field: 'description', labelKey: 'Erp::Description', type: 'text', maxLength: 250 },
    ],
    getList: query => this.gradingBands.getList(query),
    get: id => this.gradingBands.get(id),
    create: input => this.gradingBands.create(input),
    update: (id, input) => this.gradingBands.update(id, input),
    delete: id => this.gradingBands.delete(id),
    toItem: dto => ({ id: dto.id, code: `${dto.examCategoryCode ?? ''} ${dto.grade ?? ''}`.trim(), name: dto.remarks ?? undefined }),
    newRecord: () => ({ fromMark: 0, toMark: 100, points: 0, passed: true }),
  };

  readonly examComponent: RecordEntity<ExamComponentDto, CreateUpdateExamComponentDto> = {
    key: 'examComponent',
    titleKey: 'Erp::ExamComponent',
    pluralKey: 'Erp::ExamComponents',
    icon: 'fas fa-chart-pie',
    permission: SETUP_PERMISSION,
    listRoute: ['/erp/exam-components'],
    columns: [
      { field: 'examCategoryCode', labelKey: 'Erp::ExamCategoryCode' },
      { field: 'examType', labelKey: 'Erp::ExamType', type: 'select', options: enumOptions(examTypeOptions, 'ExamType') },
      { field: 'description', labelKey: 'Erp::Description', width: 240 },
      { field: 'maxScore', labelKey: 'Erp::MaxScore', type: 'number' },
      { field: 'contributionPct', labelKey: 'Erp::ContributionPct', type: 'number' },
    ],
    sections: [{ key: 'general', labelKey: 'Erp::General' }],
    fields: [
      codeField('examCategoryCode', 'Erp::ExamCategoryCode', 'examCategory', undefined, { required: true }),
      { field: 'examType', labelKey: 'Erp::ExamType', type: 'select', options: enumOptions(examTypeOptions, 'ExamType') },
      { field: 'description', labelKey: 'Erp::Description', type: 'text', maxLength: 250 },
      { field: 'maxScore', labelKey: 'Erp::MaxScore', type: 'number', min: 0, helpKey: 'Erp::MaxScoreHelp' },
      { field: 'contributionPct', labelKey: 'Erp::ContributionPct', type: 'number', min: 0, helpKey: 'Erp::ContributionPctHelp' },
    ],
    getList: query => this.examComponents.getList(query),
    get: id => this.examComponents.get(id),
    create: input => this.examComponents.create(input),
    update: (id, input) => this.examComponents.update(id, input),
    delete: id => this.examComponents.delete(id),
    toItem: dto => ({ id: dto.id, code: `${dto.examCategoryCode ?? ''} ${ExamType[dto.examType] ?? ''}`.trim(), name: dto.description ?? undefined }),
    newRecord: () => ({ examType: ExamType.FinalExam, maxScore: 100, contributionPct: 0 }),
  };

  // ---------------------------------------------------------------- Programmes

  readonly programme = codeTableEntity(this.programmes, {
    key: 'programme',
    titleKey: 'Erp::Programme',
    pluralKey: 'Erp::Programmes',
    icon: 'fas fa-graduation-cap',
    permission: SETUP_PERMISSION,
    route: '/erp/programmes',
    columns: [
      { field: 'level', labelKey: 'Erp::ProgrammeLevel', type: 'select', options: enumOptions(programmeLevelOptions, 'ProgrammeLevel') },
      { field: 'examCategoryCode', labelKey: 'Erp::ExamCategoryCode' },
      { field: 'durationMonths', labelKey: 'Erp::DurationMonths', type: 'number' },
      { field: 'active', labelKey: 'Erp::Active', type: 'boolean' },
    ],
    sections: [
      { key: 'general', labelKey: 'Erp::General' },
      { key: 'numbering', labelKey: 'Erp::StudentNumbering', collapsed: true },
    ],
    fields: [
      { field: 'level', labelKey: 'Erp::ProgrammeLevel', type: 'select', options: enumOptions(programmeLevelOptions, 'ProgrammeLevel') },
      codeField('examCategoryCode', 'Erp::ExamCategoryCode', 'examCategory', undefined, { helpKey: 'Erp::ProgrammeExamCategoryHelp' }),
      { field: 'durationMonths', labelKey: 'Erp::DurationMonths', type: 'number', min: 0 },
      { field: 'minimumCapacity', labelKey: 'Erp::MinimumCapacity', type: 'number', min: 0, cardOnly: true },
      { field: 'maximumCapacity', labelKey: 'Erp::MaximumCapacity', type: 'number', min: 0, cardOnly: true },
      { field: 'active', labelKey: 'Erp::Active', type: 'checkbox' },
      { field: 'studentNos', labelKey: 'Erp::StudentNos', type: 'text', maxLength: 20, section: 'numbering', cardOnly: true, helpKey: 'Erp::ProgrammeStudentNosHelp' },
      { field: 'studentNoPrefix', labelKey: 'Erp::StudentNoPrefix', type: 'text', maxLength: 5, section: 'numbering', cardOnly: true },
      { field: 'studentNoSuffix', labelKey: 'Erp::StudentNoSuffix', type: 'text', maxLength: 5, section: 'numbering', cardOnly: true },
    ],
    defaults: { level: 0, durationMonths: 0, minimumCapacity: 0, maximumCapacity: 0, active: true },
    quickCreate: false,
  });

  readonly programmeStage: RecordEntity<ProgrammeStageDto, CreateUpdateProgrammeStageDto> = {
    key: 'programmeStage',
    titleKey: 'Erp::ProgrammeStage',
    pluralKey: 'Erp::ProgrammeStages',
    icon: 'fas fa-stairs',
    permission: SETUP_PERMISSION,
    listRoute: ['/erp/programme-stages'],
    columns: [
      { field: 'programmeCode', labelKey: 'Erp::ProgrammeCode' },
      { field: 'code', labelKey: 'Erp::Code' },
      { field: 'description', labelKey: 'Erp::Description', width: 240 },
      { field: 'sequence', labelKey: 'Erp::Sequence', type: 'number' },
      { field: 'finalStage', labelKey: 'Erp::FinalStage', type: 'boolean' },
      { field: 'minimumUnits', labelKey: 'Erp::MinimumUnits', type: 'number' },
      { field: 'maximumUnits', labelKey: 'Erp::MaximumUnits', type: 'number' },
    ],
    sections: [{ key: 'general', labelKey: 'Erp::General' }],
    fields: [
      codeField('programmeCode', 'Erp::ProgrammeCode', 'programme', undefined, { required: true }),
      { field: 'code', labelKey: 'Erp::Code', type: 'text', required: true, maxLength: 20 },
      { field: 'description', labelKey: 'Erp::Description', type: 'text', maxLength: 250 },
      { field: 'sequence', labelKey: 'Erp::Sequence', type: 'number', helpKey: 'Erp::StageSequenceHelp' },
      { field: 'finalStage', labelKey: 'Erp::FinalStage', type: 'checkbox' },
      { field: 'minimumUnits', labelKey: 'Erp::MinimumUnits', type: 'number', min: 0, helpKey: 'Erp::UnitLimitsHelp' },
      { field: 'maximumUnits', labelKey: 'Erp::MaximumUnits', type: 'number', min: 0 },
    ],
    getList: query => this.stages.getList(query),
    get: id => this.stages.get(id),
    create: input => this.stages.create(input),
    update: (id, input) => this.stages.update(id, input),
    delete: id => this.stages.delete(id),
    toItem: dto => ({ id: dto.id, code: dto.code ?? '', name: `${dto.programmeCode ?? ''} ${dto.description ?? ''}`.trim() }),
    newRecord: () => ({ sequence: 1, finalStage: false, minimumUnits: 0, maximumUnits: 0 }),
  };

  readonly courseUnit: RecordEntity<CourseUnitDto, CreateUpdateCourseUnitDto> = {
    key: 'courseUnit',
    titleKey: 'Erp::CourseUnit',
    pluralKey: 'Erp::CourseUnits',
    icon: 'fas fa-book-open',
    permission: SETUP_PERMISSION,
    listRoute: ['/erp/course-units'],
    columns: [
      { field: 'programmeCode', labelKey: 'Erp::ProgrammeCode' },
      { field: 'code', labelKey: 'Erp::Code' },
      { field: 'description', labelKey: 'Erp::Description', width: 260 },
      { field: 'stageCode', labelKey: 'Erp::StageCode' },
      { field: 'semesterCode', labelKey: 'Erp::SemesterCode' },
      { field: 'unitType', labelKey: 'Erp::UnitType', type: 'select', options: enumOptions(unitTypeOptions, 'UnitType') },
      { field: 'creditHours', labelKey: 'Erp::CreditHours', type: 'number' },
      { field: 'blocked', labelKey: 'Erp::Blocked', type: 'boolean' },
    ],
    sections: [{ key: 'general', labelKey: 'Erp::General' }],
    fields: [
      codeField('programmeCode', 'Erp::ProgrammeCode', 'programme', undefined, { required: true }),
      { field: 'code', labelKey: 'Erp::Code', type: 'text', required: true, maxLength: 20 },
      { field: 'description', labelKey: 'Erp::Description', type: 'text', maxLength: 250, wide: true },
      codeField('stageCode', 'Erp::StageCode', 'programmeStage', undefined, { required: true }),
      codeField('semesterCode', 'Erp::SemesterCode', 'semester', undefined, { helpKey: 'Erp::UnitSemesterHelp' }),
      { field: 'unitType', labelKey: 'Erp::UnitType', type: 'select', options: enumOptions(unitTypeOptions, 'UnitType') },
      { field: 'creditHours', labelKey: 'Erp::CreditHours', type: 'number', min: 0 },
      codeField('prerequisiteUnitCode', 'Erp::PrerequisiteUnitCode', 'courseUnit', undefined, { cardOnly: true, helpKey: 'Erp::PrerequisiteHelp' }),
      { field: 'blocked', labelKey: 'Erp::Blocked', type: 'checkbox', cardOnly: true },
    ],
    getList: query => this.courseUnits.getList(query),
    get: id => this.courseUnits.get(id),
    create: input => this.courseUnits.create(input),
    update: (id, input) => this.courseUnits.update(id, input),
    delete: id => this.courseUnits.delete(id),
    toItem: dto => ({ id: dto.id, code: dto.code ?? '', name: `${dto.programmeCode ?? ''} ${dto.description ?? ''}`.trim() }),
    newRecord: () => ({ unitType: 0, creditHours: 0, blocked: false }),
  };

  // ---------------------------------------------------------------- Fees

  readonly feeItem = codeTableEntity(this.feeItems, {
    key: 'feeItem',
    titleKey: 'Erp::FeeItem',
    pluralKey: 'Erp::FeeItems',
    icon: 'fas fa-tags',
    permission: SETUP_PERMISSION,
    route: '/erp/fee-items',
    columns: [
      { field: 'feeType', labelKey: 'Erp::FeeType', type: 'select', options: enumOptions(feeTypeOptions, 'FeeType') },
      { field: 'glAccountNo', labelKey: 'Erp::GLAccountNo' },
      { field: 'defaultAmount', labelKey: 'Erp::DefaultAmount', type: 'currency' },
      { field: 'tuitionFee', labelKey: 'Erp::TuitionFee', type: 'boolean' },
    ],
    fields: [
      { field: 'feeType', labelKey: 'Erp::FeeType', type: 'select', options: enumOptions(feeTypeOptions, 'FeeType') },
      accountField('glAccountNo', 'Erp::GLAccountNo', 'general', true),
      { field: 'defaultAmount', labelKey: 'Erp::DefaultAmount', type: 'currency', min: 0, helpKey: 'Erp::FeeDefaultAmountHelp' },
      { field: 'tuitionFee', labelKey: 'Erp::TuitionFee', type: 'checkbox' },
    ],
    defaults: { feeType: FeeType.NormalCharge, defaultAmount: 0, tuitionFee: false },
    quickCreate: false,
  });

  readonly feeStructureLine: RecordEntity<FeeStructureLineDto, CreateUpdateFeeStructureLineDto> = {
    key: 'feeStructureLine',
    titleKey: 'Erp::FeeStructureLine',
    pluralKey: 'Erp::FeeStructure',
    icon: 'fas fa-table-list',
    permission: SETUP_PERMISSION,
    listRoute: ['/erp/fee-structure'],
    columns: [
      { field: 'programmeCode', labelKey: 'Erp::ProgrammeCode' },
      { field: 'stageCode', labelKey: 'Erp::StageCode' },
      { field: 'semesterCode', labelKey: 'Erp::SemesterCode' },
      { field: 'studyMode', labelKey: 'Erp::StudyMode', type: 'select', options: enumOptions(studyModeOptions, 'StudyMode') },
      { field: 'feeItemCode', labelKey: 'Erp::FeeItemCode' },
      { field: 'amount', labelKey: 'Erp::Amount', type: 'currency' },
    ],
    sections: [{ key: 'general', labelKey: 'Erp::General' }],
    fields: [
      codeField('programmeCode', 'Erp::ProgrammeCode', 'programme', undefined, { required: true }),
      codeField('stageCode', 'Erp::StageCode', 'programmeStage', undefined, { required: true }),
      codeField('semesterCode', 'Erp::SemesterCode', 'semester', undefined, { helpKey: 'Erp::FeeSemesterHelp' }),
      { field: 'studyMode', labelKey: 'Erp::StudyMode', type: 'select', options: enumOptions(studyModeOptions, 'StudyMode'), helpKey: 'Erp::FeeStudyModeHelp' },
      codeField('feeItemCode', 'Erp::FeeItemCode', 'feeItem', undefined, { required: true }),
      { field: 'amount', labelKey: 'Erp::Amount', type: 'currency', min: 0 },
    ],
    getList: query => this.feeStructure.getList(query),
    get: id => this.feeStructure.get(id),
    create: input => this.feeStructure.create(input),
    update: (id, input) => this.feeStructure.update(id, input),
    delete: id => this.feeStructure.delete(id),
    toItem: dto => ({ id: dto.id, code: `${dto.programmeCode ?? ''} ${dto.stageCode ?? ''} ${dto.feeItemCode ?? ''}`.trim() }),
    newRecord: () => ({ studyMode: StudyMode.None, amount: 0 }),
  };

  // ---------------------------------------------------------------- Applications and students

  readonly studentApplication: RecordEntity<StudentApplicationDto, CreateUpdateStudentApplicationDto> = {
    key: 'studentApplication',
    titleKey: 'Erp::StudentApplication',
    pluralKey: 'Erp::StudentApplications',
    icon: 'fas fa-file-signature',
    permission: PERMISSION,
    listRoute: ['/erp/student-applications'],
    attachmentEntityType: 'StudentApplication',
    columns: [
      { field: 'no', labelKey: 'Erp::No', width: 120 },
      { field: 'fullName', labelKey: 'Erp::FullName', width: 220 },
      { field: 'programmeCode', labelKey: 'Erp::ProgrammeCode' },
      { field: 'intakeCode', labelKey: 'Erp::IntakeCode' },
      { field: 'applicationDate', labelKey: 'Erp::ApplicationDate', type: 'date' },
      { field: 'status', labelKey: 'Erp::Status', type: 'select', options: enumOptions(applicationStatusOptions, 'ApplicationStatus') },
      { field: 'studentNo', labelKey: 'Erp::StudentNo' },
    ],
    sections: [
      { key: 'general', labelKey: 'Erp::General' },
      { key: 'contact', labelKey: 'Erp::AddressAndContact' },
      { key: 'personal', labelKey: 'Erp::Personal', collapsed: true },
      { key: 'background', labelKey: 'Erp::AcademicBackground', collapsed: true },
      { key: 'admission', labelKey: 'Erp::Admission', collapsed: true },
    ],
    fields: [
      { field: 'no', labelKey: 'Erp::No', type: 'text', maxLength: 20, placeholderKey: 'Erp::NextFromSeries', createOnly: true },
      { field: 'applicationDate', labelKey: 'Erp::ApplicationDate', type: 'date', required: true },
      ...personFields(),
      ...courseFields(),
      { field: 'formerSchool', labelKey: 'Erp::FormerSchool', type: 'text', maxLength: 100, section: 'background', cardOnly: true },
      { field: 'indexNumber', labelKey: 'Erp::IndexNumber', type: 'text', maxLength: 35, section: 'background', cardOnly: true },
      { field: 'meanGrade', labelKey: 'Erp::MeanGrade', type: 'text', maxLength: 20, section: 'background', cardOnly: true },
      figure('studentNo', 'Erp::StudentNo', 'admission'),
      figure('admissionDate', 'Erp::AdmissionDate', 'admission'),
      figure('rejectionReason', 'Erp::RejectionReason', 'admission'),
    ],
    getList: query => this.applications.getList(query),
    get: id => this.applications.get(id),
    create: input => this.applications.create(input),
    update: (id, input) => this.applications.update(id, input),
    delete: id => this.applications.delete(id),
    toItem: dto => ({ id: dto.id, code: dto.no ?? '', name: dto.fullName ?? undefined }),
    newRecord: term => ({ firstName: term ?? '', applicationDate: today(), gender: 0, sponsorship: 0, studyMode: StudyMode.FullTime }),
    facts: dto => [
      { labelKey: 'Erp::Status', value: ApplicationStatus[dto.status] },
      { labelKey: 'Erp::StudentNo', value: dto.studentNo },
    ],
    actions: [
      {
        key: 'submit',
        labelKey: 'Erp::Submit',
        icon: 'fas fa-share',
        permission: `${PERMISSION}.Update`,
        visible: dto => dto.status === ApplicationStatus.Open,
        run: dto => this.applications.submit(dto.id!),
      },
      {
        key: 'approve',
        labelKey: 'Erp::Approve',
        icon: 'fas fa-check',
        permission: `${PERMISSION}.Post`,
        visible: dto => dto.status === ApplicationStatus.Submitted,
        run: dto => this.applications.approve(dto.id!),
      },
      {
        key: 'reject',
        labelKey: 'Erp::Reject',
        icon: 'fas fa-xmark',
        permission: `${PERMISSION}.Post`,
        visible: dto => dto.status === ApplicationStatus.Submitted,
        confirmKey: 'Erp::RejectApplicationConfirmation',
        run: dto => this.applications.reject(dto.id!, {}),
      },
      {
        key: 'reopen',
        labelKey: 'Erp::Reopen',
        icon: 'fas fa-lock-open',
        permission: `${PERMISSION}.Update`,
        visible: dto => dto.status !== ApplicationStatus.Open && dto.status !== ApplicationStatus.Admitted,
        run: dto => this.applications.reopen(dto.id!),
      },
      {
        key: 'admit',
        labelKey: 'Erp::AdmitStudent',
        icon: 'fas fa-user-graduate',
        permission: `${PERMISSION}.Post`,
        visible: dto => dto.status === ApplicationStatus.Approved,
        confirmKey: 'Erp::AdmitStudentConfirmation',
        run: dto => this.applications.admit(dto.id!, {}),
      },
    ],
    related: dto =>
      this.students.getList({ filter: dto.studentNo || '-', maxResultCount: 1, skipCount: 0 }).pipe(
        map(students =>
          dto.studentNo
            ? [
                {
                  labelKey: 'Erp::Student',
                  icon: 'fas fa-user-graduate',
                  count: students.totalCount ?? 0,
                  routerLink: ['/erp/students'],
                  queryParams: { filter: dto.studentNo },
                  permission: PERMISSION,
                },
              ]
            : [],
        ),
      ),
  };

  readonly student: RecordEntity<StudentDto, CreateUpdateStudentDto> = {
    key: 'student',
    titleKey: 'Erp::Student',
    pluralKey: 'Erp::Students',
    icon: 'fas fa-user-graduate',
    permission: PERMISSION,
    listRoute: ['/erp/students'],
    attachmentEntityType: 'Student',
    columns: [
      { field: 'no', labelKey: 'Erp::No', width: 120 },
      { field: 'fullName', labelKey: 'Erp::FullName', width: 220 },
      { field: 'programmeCode', labelKey: 'Erp::ProgrammeCode' },
      { field: 'currentStageCode', labelKey: 'Erp::CurrentStageCode' },
      { field: 'currentSemesterCode', labelKey: 'Erp::CurrentSemesterCode' },
      { field: 'intakeCode', labelKey: 'Erp::IntakeCode' },
      { field: 'studyMode', labelKey: 'Erp::StudyMode', type: 'select', options: enumOptions(studyModeOptions, 'StudyMode') },
      { field: 'status', labelKey: 'Erp::Status', type: 'select', options: enumOptions(studentStatusOptions, 'StudentStatus') },
    ],
    sections: [
      { key: 'general', labelKey: 'Erp::General' },
      { key: 'contact', labelKey: 'Erp::AddressAndContact' },
      { key: 'personal', labelKey: 'Erp::Personal', collapsed: true },
      { key: 'progress', labelKey: 'Erp::AcademicProgress' },
    ],
    fields: [
      { field: 'no', labelKey: 'Erp::No', type: 'text', maxLength: 20, placeholderKey: 'Erp::NextFromSeries', createOnly: true },
      ...personFields(),
      ...courseFields(),
      { field: 'status', labelKey: 'Erp::Status', type: 'select', options: enumOptions(studentStatusOptions, 'StudentStatus'), cardOnly: true },
      { field: 'admissionDate', labelKey: 'Erp::AdmissionDate', type: 'date', cardOnly: true },
      figure('currentStageCode', 'Erp::CurrentStageCode', 'progress'),
      figure('currentSemesterCode', 'Erp::CurrentSemesterCode', 'progress'),
      figure('customerNo', 'Erp::CustomerNo', 'progress'),
      figure('applicationNo', 'Erp::ApplicationNo', 'progress'),
    ],
    getList: query => this.students.getList(query),
    get: id => this.students.get(id),
    create: input => this.students.create(input),
    update: (id, input) => this.students.update(id, input),
    delete: id => this.students.delete(id),
    toItem: dto => ({ id: dto.id, code: dto.no ?? '', name: dto.fullName ?? undefined }),
    newRecord: term => ({
      firstName: term ?? '',
      gender: 0,
      sponsorship: 0,
      studyMode: StudyMode.FullTime,
      status: StudentStatus.Registration,
      admissionDate: today(),
    }),
    facts: dto => [
      { labelKey: 'Erp::Status', value: StudentStatus[dto.status] },
      { labelKey: 'Erp::ProgrammeCode', value: dto.programmeCode },
      { labelKey: 'Erp::CurrentStageCode', value: dto.currentStageCode },
    ],
    // The balance is the customer account's and the mean is worked out from the graded units,
    // so the card shows what a fee statement and a transcript would.
    related: dto =>
      forkJoin({
        balance: this.students.getBalance(dto.id!),
        transcript: this.students.getTranscript(dto.id!),
        registrations: this.registrations.getList({ studentNo: dto.no, maxResultCount: 1, skipCount: 0 }),
        bills: this.bills.getList({ studentNo: dto.no, maxResultCount: 1, skipCount: 0 }),
      }).pipe(
        map(({ balance, transcript, registrations, bills }) => [
          {
            labelKey: 'Erp::FeeBalance',
            icon: 'fas fa-scale-balanced',
            count: money(balance.balance),
            routerLink: ['/erp/finance/customer-ledger-entries'],
            queryParams: { filter: balance.customerNo },
            permission: 'Erp.Customers',
          },
          {
            labelKey: 'Erp::Prepayment',
            icon: 'fas fa-receipt',
            count: money(balance.prepayment),
            routerLink: ['/erp/student-receipts'],
            queryParams: { filter: dto.no },
            permission: PERMISSION,
          },
          {
            labelKey: 'Erp::StudentBills',
            icon: 'fas fa-file-invoice-dollar',
            count: bills.totalCount ?? 0,
            routerLink: ['/erp/student-bills'],
            queryParams: { filter: dto.no },
            permission: PERMISSION,
          },
          {
            labelKey: 'Erp::SemesterRegistrations',
            icon: 'fas fa-clipboard-list',
            count: registrations.totalCount ?? 0,
            routerLink: ['/erp/semester-registrations'],
            queryParams: { filter: dto.no },
            permission: PERMISSION,
          },
          {
            labelKey: 'Erp::MeanScore',
            icon: 'fas fa-square-poll-vertical',
            count: money(transcript.meanScore),
            routerLink: ['/erp/student-units'],
            queryParams: { filter: dto.no },
            permission: PERMISSION,
          },
        ]),
      ),
  };

  // ---------------------------------------------------------------- Registration

  readonly semesterRegistration: RecordEntity<SemesterRegistrationDto, CreateUpdateSemesterRegistrationDto> = {
    key: 'semesterRegistration',
    titleKey: 'Erp::SemesterRegistration',
    pluralKey: 'Erp::SemesterRegistrations',
    icon: 'fas fa-clipboard-list',
    permission: PERMISSION,
    listRoute: ['/erp/semester-registrations'],
    attachmentEntityType: 'SemesterRegistration',
    columns: [
      { field: 'no', labelKey: 'Erp::No', width: 120 },
      { field: 'studentNo', labelKey: 'Erp::StudentNo' },
      { field: 'studentName', labelKey: 'Erp::StudentName', width: 220 },
      { field: 'programmeCode', labelKey: 'Erp::ProgrammeCode' },
      { field: 'stageCode', labelKey: 'Erp::StageCode' },
      { field: 'semesterCode', labelKey: 'Erp::SemesterCode' },
      { field: 'registrationDate', labelKey: 'Erp::RegistrationDate', type: 'date' },
      { field: 'status', labelKey: 'Erp::Status', type: 'select', options: enumOptions(registrationStatusOptions, 'RegistrationStatus') },
      { field: 'noOfUnits', labelKey: 'Erp::NoOfUnits', type: 'number' },
      { field: 'billedAmount', labelKey: 'Erp::BilledAmount', type: 'currency' },
    ],
    sections: [{ key: 'general', labelKey: 'Erp::General' }],
    fields: [
      { field: 'no', labelKey: 'Erp::No', type: 'text', maxLength: 20, placeholderKey: 'Erp::NextFromSeries', createOnly: true },
      codeField('studentNo', 'Erp::StudentNo', 'student', undefined, { required: true }),
      codeField('stageCode', 'Erp::StageCode', 'programmeStage', undefined, { required: true }),
      codeField('semesterCode', 'Erp::SemesterCode', 'semester', undefined, { helpKey: 'Erp::BlankTakesCurrent' }),
      codeField('academicYearCode', 'Erp::AcademicYearCode', 'academicYear', undefined, { cardOnly: true }),
      { field: 'registrationDate', labelKey: 'Erp::RegistrationDate', type: 'date', required: true },
      { field: 'registerFor', labelKey: 'Erp::RegisterFor', type: 'select', options: enumOptions(registerForOptions, 'RegisterFor') },
      { field: 'remarks', labelKey: 'Erp::Remarks', type: 'text', maxLength: 250, wide: true, cardOnly: true },
      figure('programmeCode', 'Erp::ProgrammeCode', 'general'),
      figure('billNo', 'Erp::BillNo', 'general'),
    ],
    getList: query => this.registrations.getList(query),
    get: id => this.registrations.get(id),
    create: input => this.registrations.create(input),
    update: (id, input) => this.registrations.update(id, input),
    delete: id => this.registrations.delete(id),
    toItem: dto => ({ id: dto.id, code: dto.no ?? '', name: dto.studentName ?? undefined }),
    newRecord: () => ({ registrationDate: today(), registerFor: RegisterFor.Stage }),
    facts: dto => [
      { labelKey: 'Erp::Status', value: RegistrationStatus[dto.status] },
      { labelKey: 'Erp::NoOfUnits', value: dto.noOfUnits, type: 'number' },
      { labelKey: 'Erp::BilledAmount', value: dto.billedAmount, type: 'currency' },
    ],
    actions: [
      {
        key: 'fillUnits',
        labelKey: 'Erp::FillUnits',
        icon: 'fas fa-wand-magic-sparkles',
        permission: `${PERMISSION}.Update`,
        visible: dto => dto.status === RegistrationStatus.Open,
        run: dto => this.registrations.fillUnits(dto.id!),
      },
      {
        key: 'submit',
        labelKey: 'Erp::SubmitRegistration',
        icon: 'fas fa-paper-plane',
        permission: `${PERMISSION}.Post`,
        visible: dto => dto.status === RegistrationStatus.Open,
        confirmKey: 'Erp::SubmitRegistrationConfirmation',
        run: dto => this.registrations.submit(dto.id!),
      },
    ],
    related: dto =>
      this.studentUnits.getList({ registrationNo: dto.no, maxResultCount: 1, skipCount: 0 }).pipe(
        map(units => [
          {
            labelKey: 'Erp::StudentUnits',
            icon: 'fas fa-book-open',
            count: units.totalCount ?? 0,
            routerLink: ['/erp/student-units'],
            queryParams: { filter: dto.no },
            permission: PERMISSION,
          },
          ...(dto.billNo
            ? [
                {
                  labelKey: 'Erp::StudentBill',
                  icon: 'fas fa-file-invoice-dollar',
                  routerLink: ['/erp/student-bills'],
                  queryParams: { filter: dto.billNo },
                  permission: PERMISSION,
                },
              ]
            : []),
        ]),
      ),
  };

  readonly studentUnit: RecordEntity<StudentUnitDto, CreateUpdateStudentUnitDto> = {
    key: 'studentUnit',
    titleKey: 'Erp::StudentUnit',
    pluralKey: 'Erp::StudentUnits',
    icon: 'fas fa-book-open',
    permission: PERMISSION,
    listRoute: ['/erp/student-units'],
    columns: [
      { field: 'registrationNo', labelKey: 'Erp::RegistrationNo', width: 120 },
      { field: 'studentNo', labelKey: 'Erp::StudentNo' },
      { field: 'studentName', labelKey: 'Erp::StudentName', width: 200 },
      { field: 'unitCode', labelKey: 'Erp::UnitCode' },
      { field: 'unitDescription', labelKey: 'Erp::Description', width: 220 },
      { field: 'semesterCode', labelKey: 'Erp::SemesterCode' },
      { field: 'catMark', labelKey: 'Erp::CatMark', type: 'number' },
      { field: 'examMark', labelKey: 'Erp::ExamMark', type: 'number' },
      { field: 'finalScore', labelKey: 'Erp::FinalScore', type: 'number' },
      { field: 'grade', labelKey: 'Erp::Grade' },
      { field: 'passed', labelKey: 'Erp::Passed', type: 'boolean' },
    ],
    sections: [
      { key: 'general', labelKey: 'Erp::General' },
      { key: 'results', labelKey: 'Erp::Results' },
    ],
    fields: [
      codeField('registrationNo', 'Erp::RegistrationNo', 'semesterRegistration', undefined, { required: true, createOnly: true }),
      codeField('unitCode', 'Erp::UnitCode', 'courseUnit', undefined, { required: true }),
      figure('studentNo', 'Erp::StudentNo', 'general'),
      figure('studentName', 'Erp::StudentName', 'general'),
      figure('unitDescription', 'Erp::Description', 'general'),
      figure('stageCode', 'Erp::StageCode', 'general'),
      figure('semesterCode', 'Erp::SemesterCode', 'general'),
      figure('academicYearCode', 'Erp::AcademicYearCode', 'general'),
      figure('assignmentMark', 'Erp::AssignmentMark', 'results'),
      figure('catMark', 'Erp::CatMark', 'results'),
      figure('cat2Mark', 'Erp::Cat2Mark', 'results'),
      figure('examMark', 'Erp::ExamMark', 'results'),
      figure('finalScore', 'Erp::FinalScore', 'results'),
      figure('grade', 'Erp::Grade', 'results'),
      figure('resultRemarks', 'Erp::Remarks', 'results'),
    ],
    getList: query => this.studentUnits.getList(query),
    get: id => this.studentUnits.get(id),
    create: input => this.studentUnits.create(input),
    update: (id, input) => this.studentUnits.update(id, input),
    delete: id => this.studentUnits.delete(id),
    toItem: dto => ({ id: dto.id, code: `${dto.registrationNo ?? ''} ${dto.unitCode ?? ''}`.trim(), name: dto.unitDescription ?? undefined }),
    newRecord: () => ({}),
    facts: dto => [
      { labelKey: 'Erp::FinalScore', value: dto.finalScore, type: 'number' },
      { labelKey: 'Erp::Grade', value: dto.grade },
      { labelKey: 'Erp::Passed', value: dto.passed, type: 'boolean' },
    ],
  };

  // ---------------------------------------------------------------- Billing

  readonly studentBill: RecordEntity<StudentBillHeaderDto, CreateUpdateStudentBillHeaderDto> = {
    key: 'studentBill',
    titleKey: 'Erp::StudentBill',
    pluralKey: 'Erp::StudentBills',
    icon: 'fas fa-file-invoice-dollar',
    permission: PERMISSION,
    listRoute: ['/erp/student-bills'],
    attachmentEntityType: 'StudentBillHeader',
    columns: [
      { field: 'no', labelKey: 'Erp::No', width: 120 },
      { field: 'studentNo', labelKey: 'Erp::StudentNo' },
      { field: 'studentName', labelKey: 'Erp::StudentName', width: 220 },
      { field: 'postingDate', labelKey: 'Erp::PostingDate', type: 'date' },
      { field: 'stageCode', labelKey: 'Erp::StageCode' },
      { field: 'semesterCode', labelKey: 'Erp::SemesterCode' },
      { field: 'status', labelKey: 'Erp::Status', type: 'select', options: enumOptions(academicDocumentStatusOptions, 'AcademicDocumentStatus') },
      { field: 'totalAmount', labelKey: 'Erp::TotalAmount', type: 'currency' },
    ],
    sections: [{ key: 'general', labelKey: 'Erp::General' }],
    fields: [
      { field: 'no', labelKey: 'Erp::No', type: 'text', maxLength: 20, placeholderKey: 'Erp::NextFromSeries', createOnly: true },
      codeField('studentNo', 'Erp::StudentNo', 'student', undefined, { required: true }),
      { field: 'postingDate', labelKey: 'Erp::PostingDate', type: 'date', required: true },
      codeField('stageCode', 'Erp::StageCode', 'programmeStage', undefined, { helpKey: 'Erp::BillStageHelp' }),
      codeField('semesterCode', 'Erp::SemesterCode', 'semester'),
      codeField('academicYearCode', 'Erp::AcademicYearCode', 'academicYear', undefined, { cardOnly: true }),
      { field: 'description', labelKey: 'Erp::Description', type: 'text', maxLength: 250, wide: true },
      figure('programmeCode', 'Erp::ProgrammeCode', 'general'),
      figure('registrationNo', 'Erp::RegistrationNo', 'general'),
      figure('postedBy', 'Erp::PostedBy', 'general'),
    ],
    getList: query => this.bills.getList(query),
    get: id => this.bills.get(id),
    create: input => this.bills.create(input),
    update: (id, input) => this.bills.update(id, input),
    delete: id => this.bills.delete(id),
    toItem: dto => ({ id: dto.id, code: dto.no ?? '', name: dto.studentName ?? undefined }),
    newRecord: () => ({ postingDate: today() }),
    facts: dto => [
      { labelKey: 'Erp::Status', value: AcademicDocumentStatus[dto.status] },
      { labelKey: 'Erp::TotalAmount', value: dto.totalAmount, type: 'currency' },
    ],
    actions: [
      {
        key: 'suggest',
        labelKey: 'Erp::SuggestFees',
        icon: 'fas fa-wand-magic-sparkles',
        permission: `${PERMISSION}.Update`,
        visible: dto => dto.status === AcademicDocumentStatus.Open,
        run: dto => this.bills.suggestLines(dto.id!),
      },
      {
        key: 'post',
        labelKey: 'Erp::Post',
        icon: 'fas fa-paper-plane',
        permission: `${PERMISSION}.Post`,
        visible: dto => dto.status === AcademicDocumentStatus.Open,
        confirmKey: 'Erp::PostStudentBillConfirmation',
        run: dto => this.bills.runPosting(dto.id!),
      },
    ],
    related: dto =>
      this.billLines.getList({ documentNo: dto.no, maxResultCount: 1, skipCount: 0 }).pipe(
        map(lines => [
          {
            labelKey: 'Erp::StudentBillLines',
            icon: 'fas fa-list',
            count: lines.totalCount ?? 0,
            routerLink: ['/erp/student-bill-lines'],
            queryParams: { filter: dto.no },
            permission: PERMISSION,
          },
        ]),
      ),
  };

  readonly studentBillLine: RecordEntity<StudentBillLineDto, CreateUpdateStudentBillLineDto> = {
    key: 'studentBillLine',
    titleKey: 'Erp::StudentBillLine',
    pluralKey: 'Erp::StudentBillLines',
    icon: 'fas fa-list',
    permission: PERMISSION,
    listRoute: ['/erp/student-bill-lines'],
    columns: [
      { field: 'documentNo', labelKey: 'Erp::DocumentNo', width: 120 },
      { field: 'feeItemCode', labelKey: 'Erp::FeeItemCode' },
      { field: 'description', labelKey: 'Erp::Description', width: 260 },
      { field: 'amount', labelKey: 'Erp::Amount', type: 'currency' },
    ],
    sections: [{ key: 'general', labelKey: 'Erp::General' }],
    fields: [
      codeField('documentNo', 'Erp::DocumentNo', 'studentBill', undefined, { required: true, createOnly: true }),
      codeField('feeItemCode', 'Erp::FeeItemCode', 'feeItem', undefined, { required: true }),
      { field: 'description', labelKey: 'Erp::Description', type: 'text', maxLength: 250, helpKey: 'Erp::BillLineDescriptionHelp' },
      { field: 'amount', labelKey: 'Erp::Amount', type: 'currency', min: 0 },
    ],
    getList: query => this.billLines.getList(query),
    get: id => this.billLines.get(id),
    create: input => this.billLines.create(input),
    update: (id, input) => this.billLines.update(id, input),
    delete: id => this.billLines.delete(id),
    toItem: dto => ({ id: dto.id, code: `${dto.documentNo ?? ''} ${dto.feeItemCode ?? ''}`.trim(), name: dto.description ?? undefined }),
    newRecord: () => ({}),
  };

  // ---------------------------------------------------------------- Exam results

  readonly examResult: RecordEntity<ExamResultHeaderDto, CreateUpdateExamResultHeaderDto> = {
    key: 'examResult',
    titleKey: 'Erp::ExamResult',
    pluralKey: 'Erp::ExamResults',
    icon: 'fas fa-square-poll-vertical',
    permission: PERMISSION,
    listRoute: ['/erp/exam-results'],
    attachmentEntityType: 'ExamResultHeader',
    columns: [
      { field: 'no', labelKey: 'Erp::No', width: 120 },
      { field: 'programmeCode', labelKey: 'Erp::ProgrammeCode' },
      { field: 'unitCode', labelKey: 'Erp::UnitCode' },
      { field: 'unitDescription', labelKey: 'Erp::Description', width: 220 },
      { field: 'semesterCode', labelKey: 'Erp::SemesterCode' },
      { field: 'examType', labelKey: 'Erp::ExamType', type: 'select', options: enumOptions(examTypeOptions, 'ExamType') },
      { field: 'documentDate', labelKey: 'Erp::DocumentDate', type: 'date' },
      { field: 'status', labelKey: 'Erp::Status', type: 'select', options: enumOptions(academicDocumentStatusOptions, 'AcademicDocumentStatus') },
      { field: 'noOfStudents', labelKey: 'Erp::NoOfStudents', type: 'number' },
    ],
    sections: [{ key: 'general', labelKey: 'Erp::General' }],
    fields: [
      { field: 'no', labelKey: 'Erp::No', type: 'text', maxLength: 20, placeholderKey: 'Erp::NextFromSeries', createOnly: true },
      codeField('programmeCode', 'Erp::ProgrammeCode', 'programme', undefined, { required: true }),
      codeField('unitCode', 'Erp::UnitCode', 'courseUnit', undefined, { required: true }),
      codeField('semesterCode', 'Erp::SemesterCode', 'semester', undefined, { helpKey: 'Erp::BlankTakesCurrent' }),
      codeField('academicYearCode', 'Erp::AcademicYearCode', 'academicYear', undefined, { cardOnly: true }),
      { field: 'examType', labelKey: 'Erp::ExamType', type: 'select', options: enumOptions(examTypeOptions, 'ExamType') },
      { field: 'documentDate', labelKey: 'Erp::DocumentDate', type: 'date', required: true },
      codeField('lecturerNo', 'Erp::LecturerNo', 'employee', undefined, { cardOnly: true }),
      figure('unitDescription', 'Erp::Description', 'general'),
      figure('stageCode', 'Erp::StageCode', 'general'),
      figure('postedBy', 'Erp::PostedBy', 'general'),
    ],
    getList: query => this.examResults.getList(query),
    get: id => this.examResults.get(id),
    create: input => this.examResults.create(input),
    update: (id, input) => this.examResults.update(id, input),
    delete: id => this.examResults.delete(id),
    toItem: dto => ({ id: dto.id, code: dto.no ?? '', name: `${dto.unitCode ?? ''} ${ExamType[dto.examType] ?? ''}`.trim() }),
    newRecord: () => ({ documentDate: today(), examType: ExamType.FinalExam }),
    facts: dto => [
      { labelKey: 'Erp::Status', value: AcademicDocumentStatus[dto.status] },
      { labelKey: 'Erp::NoOfStudents', value: dto.noOfStudents, type: 'number' },
    ],
    actions: [
      {
        key: 'suggest',
        labelKey: 'Erp::SuggestStudents',
        icon: 'fas fa-wand-magic-sparkles',
        permission: `${PERMISSION}.Update`,
        visible: dto => dto.status === AcademicDocumentStatus.Open,
        run: dto => this.examResults.suggestLines(dto.id!),
      },
      {
        key: 'post',
        labelKey: 'Erp::Post',
        icon: 'fas fa-paper-plane',
        permission: `${PERMISSION}.Post`,
        visible: dto => dto.status === AcademicDocumentStatus.Open,
        confirmKey: 'Erp::PostExamResultConfirmation',
        run: dto => this.examResults.runPosting(dto.id!),
      },
    ],
    related: dto =>
      this.examResultLines.getList({ documentNo: dto.no, maxResultCount: 1, skipCount: 0 }).pipe(
        map(lines => [
          {
            labelKey: 'Erp::ExamResultLines',
            icon: 'fas fa-list',
            count: lines.totalCount ?? 0,
            routerLink: ['/erp/exam-result-lines'],
            queryParams: { filter: dto.no },
            permission: PERMISSION,
          },
        ]),
      ),
  };

  readonly examResultLine: RecordEntity<ExamResultLineDto, CreateUpdateExamResultLineDto> = {
    key: 'examResultLine',
    titleKey: 'Erp::ExamResultLine',
    pluralKey: 'Erp::ExamResultLines',
    icon: 'fas fa-list',
    permission: PERMISSION,
    listRoute: ['/erp/exam-result-lines'],
    columns: [
      { field: 'documentNo', labelKey: 'Erp::DocumentNo', width: 120 },
      { field: 'studentNo', labelKey: 'Erp::StudentNo' },
      { field: 'studentName', labelKey: 'Erp::StudentName', width: 240 },
      { field: 'mark', labelKey: 'Erp::Mark', type: 'number' },
      { field: 'notDone', labelKey: 'Erp::NotDone', type: 'boolean' },
    ],
    sections: [{ key: 'general', labelKey: 'Erp::General' }],
    fields: [
      codeField('documentNo', 'Erp::DocumentNo', 'examResult', undefined, { required: true, createOnly: true }),
      codeField('studentNo', 'Erp::StudentNo', 'student', undefined, { required: true }),
      { field: 'mark', labelKey: 'Erp::Mark', type: 'number', min: 0, helpKey: 'Erp::MarkHelp' },
      { field: 'notDone', labelKey: 'Erp::NotDone', type: 'checkbox', helpKey: 'Erp::NotDoneHelp' },
    ],
    getList: query => this.examResultLines.getList(query),
    get: id => this.examResultLines.get(id),
    create: input => this.examResultLines.create(input),
    update: (id, input) => this.examResultLines.update(id, input),
    delete: id => this.examResultLines.delete(id),
    toItem: dto => ({ id: dto.id, code: `${dto.documentNo ?? ''} ${dto.studentNo ?? ''}`.trim(), name: dto.studentName ?? undefined }),
    newRecord: () => ({ mark: 0, notDone: false }),
  };

  get all(): RecordEntity[] {
    return [
      this.academicYear,
      this.semester,
      this.intake,
      this.examCategory,
      this.gradingBand,
      this.examComponent,
      this.programme,
      this.programmeStage,
      this.courseUnit,
      this.feeItem,
      this.feeStructureLine,
      this.studentApplication,
      this.student,
      this.semesterRegistration,
      this.studentUnit,
      this.studentBill,
      this.studentBillLine,
      this.examResult,
      this.examResultLine,
    ];
  }
}
