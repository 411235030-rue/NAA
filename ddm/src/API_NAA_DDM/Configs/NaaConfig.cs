namespace API_NAA_DDM.Configs;

public static class NaaConfig
{
    public static string NaaServiceDomain { get; set; } =
        Environment.GetEnvironmentVariable("NAA_SERVICE_DOMAIN") ?? "http://localhost:5210";
}
