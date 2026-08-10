using WEB_NAA.Components;
using WEB_NAA.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddScoped<UserSession>();
builder.Services.AddScoped<DdmApiService>();
builder.Services.AddScoped<HistoryApiService>();
builder.Services.AddScoped<HistoryState>();

builder.Services.AddHttpClient("DDM", client =>
{
    client.Timeout = TimeSpan.FromSeconds(20);
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
