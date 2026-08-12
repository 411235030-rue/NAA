using API_NAA.EndPoints;
using API_NAA.Extensions;
using API_NAA.Interfaces;
using API_NAA.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IDbServices, DbServices>();
builder.Services.AddTransient<IEndpoint, NaaEndPoints>();
builder.Services.AddHealthChecks();
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

app.UseHealthChecks("/healthz");
app.MapEndpoints();

app.Run();
