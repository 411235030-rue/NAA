using API_NAA.EndPoints;
using API_NAA.Extensions;
using API_NAA.Interfaces;
using API_NAA.Services;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IDbServices, DbServices>();
builder.Services.AddTransient<IEndpoint, NaaEndPoints>();
builder.Services.AddHealthChecks();
builder.Services.AddEndpointsApiExplorer();
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
});

var app = builder.Build();

app.UseForwardedHeaders();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseHealthChecks("/healthz");
app.MapEndpoints();

app.Run();
