using API_NAA_DDM.Configs;
using API_NAA_DDM.EndPoints;
using API_NAA_DDM.Extensions;
using API_NAA_DDM.Interfaces;
using API_NAA_DDM.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddHealthChecks();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddHttpClient<INaaHttpServices, NaaHttpServices>(client =>
{
    client.BaseAddress = new Uri(NaaConfig.NaaServiceDomain);
});

builder.Services.AddTransient<INaaEndPoint, NaaEndPoints>();

var app = builder.Build();

app.UseHealthChecks("/healthz");
app.MapEndpoints();

app.Run();
