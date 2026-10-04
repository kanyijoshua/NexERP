namespace ABPmicroservice.Erp.Academics;

/// <summary>The level of the award a programme leads to.</summary>
public enum ProgrammeLevel
{
    None = 0,
    Certificate = 1,
    Diploma = 2,
    AdvancedDiploma = 3,
    Degree = 4,
    Masters = 5,
    Doctorate = 6,
    ShortCourse = 7,
}

/// <summary>How a student attends; fees can differ by it.</summary>
public enum StudyMode
{
    /// <summary>On a fee structure line: the fee applies whatever the mode of study.</summary>
    None = 0,
    FullTime = 1,
    PartTime = 2,
    Evening = 3,
    Weekend = 4,
    DistanceLearning = 5,
}

public enum StudentGender
{
    None = 0,
    Male = 1,
    Female = 2,
}

/// <summary>Who pays the student's fees.</summary>
public enum StudentSponsorship
{
    None = 0,
    Self = 1,
    Corporate = 2,
    Government = 3,
}

public enum ApplicationStatus
{
    Open = 0,
    Submitted = 1,
    Approved = 2,
    Rejected = 3,

    /// <summary>The applicant has become a student.</summary>
    Admitted = 4,
}

public enum StudentStatus
{
    /// <summary>Admitted, not yet registered for a semester.</summary>
    Registration = 0,
    Current = 1,
    Alumni = 2,
    Deferred = 3,
    Suspended = 4,
    Discontinued = 5,
    Dropped = 6,
    Expelled = 7,
    Deceased = 8,
}

public enum UnitType
{
    Core = 0,
    Elective = 1,
    Required = 2,
}

/// <summary>What a semester registration is for.</summary>
public enum RegisterFor
{
    /// <summary>Every unit of the stage and semester.</summary>
    Stage = 0,

    /// <summary>Units picked one by one.</summary>
    Units = 1,
    Supplementary = 2,
    Retake = 3,
}

public enum RegistrationStatus
{
    Open = 0,
    Submitted = 1,
}

public enum FeeType
{
    NormalCharge = 0,
    OptionalCharge = 1,
}

/// <summary>The status of a student bill or an exam results document.</summary>
public enum AcademicDocumentStatus
{
    Open = 0,
    Posted = 1,
}

/// <summary>The status of a request that is approved rather than posted: a refund, a change of a student's standing.</summary>
public enum AcademicRequestStatus
{
    Open = 0,
    Approved = 1,
}

/// <summary>A change of a student's standing, and the status it leaves the student in.</summary>
public enum StudentChangeType
{
    /// <summary>The student steps out for a while and keeps their place: Deferred.</summary>
    Deferment = 0,

    /// <summary>A deferred, suspended or discontinued student comes back: Current.</summary>
    Readmission = 1,
    Suspension = 2,
    Discontinuation = 3,

    /// <summary>The student has passed every unit and owes nothing: Alumni.</summary>
    Graduation = 4,
}

/// <summary>A part of a unit's assessment; each contributes a share of the final score.</summary>
public enum ExamType
{
    Assignment = 0,
    Cat = 1,
    Cat2 = 2,
    FinalExam = 3,
}

public enum RoomType
{
    LectureHall = 0,
    Laboratory = 1,
}

/// <summary>A teaching timetable repeats weekly; an exam timetable is held on dates.</summary>
public enum TimetableType
{
    Teaching = 0,
    Exam = 1,
}

/// <summary>The day of the week a timetable entry is held on; Monday first, as timetables are read.</summary>
public enum TimetableDay
{
    Monday = 1,
    Tuesday = 2,
    Wednesday = 3,
    Thursday = 4,
    Friday = 5,
    Saturday = 6,
    Sunday = 7,
}

/// <summary>A student's attendance at one class session.</summary>
public enum AttendanceMark
{
    Present = 0,
    Absent = 1,

    /// <summary>Counts as attended.</summary>
    Late = 2,

    /// <summary>Not held against the student: the session is left out of the student's count.</summary>
    Excused = 3,
}

/// <summary>A student's place in a hostel: booked, allocated (and billed), then cleared on leaving.</summary>
public enum HostelAllocationStatus
{
    Booking = 0,
    Allocated = 1,
    Cleared = 2,
}

/// <summary>Who an infirmary visit is for.</summary>
public enum PatientType
{
    Student = 0,
    Employee = 1,
    Other = 2,
}

public enum TreatmentType
{
    Outpatient = 0,
    Inpatient = 1,
}

public enum ClinicVisitStatus
{
    Open = 0,
    Completed = 1,
    Referred = 2,
}

/// <summary>A laundry order: received, invoiced and sent for washing, ready, and collected.</summary>
public enum LaundryStatus
{
    Received = 0,
    Invoiced = 1,
    Ready = 2,
    Collected = 3,
}

/// <summary>An individual applies for themselves; a corporate application is one sponsor for many participants.</summary>
public enum ShortCourseApplicationType
{
    Individual = 0,
    Corporate = 1,
}

public enum ShortCourseApplicationStatus
{
    Open = 0,
    Submitted = 1,
    Approved = 2,
    Rejected = 3,

    /// <summary>The participants are registered and the course fee is billed.</summary>
    Registered = 4,
}
