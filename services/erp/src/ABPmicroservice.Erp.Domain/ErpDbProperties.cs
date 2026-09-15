namespace ABPmicroservice.Erp;

public static class ErpDbProperties
{
    public const string ConnectionStringName = "Erp";
    public static string DbTablePrefix { get; set; } = "Erp";

    public static string DbSchema { get; set; } = null;
}
