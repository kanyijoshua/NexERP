namespace ABPmicroservice.Erp.HumanResources;

/// <summary>How a calendar change repeats.</summary>
public enum CalendarRecurringSystem
{
    /// <summary>One date only.</summary>
    None = 0,

    /// <summary>The same day and month every year, e.g. a public holiday.</summary>
    AnnualRecurring = 1,

    /// <summary>The same weekday every week, e.g. Saturday and Sunday.</summary>
    WeeklyRecurring = 2,
}

/// <summary>A weekday, Monday first.</summary>
public enum CalendarWeekday
{
    None = 0,
    Monday = 1,
    Tuesday = 2,
    Wednesday = 3,
    Thursday = 4,
    Friday = 5,
    Saturday = 6,
    Sunday = 7,
}
