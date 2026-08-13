# WEB_NAA

Local Blazor front end for the NAA demo.

This project uses local fake authentication and does not require external SSO settings, OAuth client secrets, or project-level NuGet package references.

The front end communicates only with DDM. DDM is responsible for all communication with API_NAA; the browser-facing project never calls API_NAA directly.

## Run

```powershell
dotnet run --project .\src\WEB_NAA\WEB_NAA.csproj --launch-profile https
```

Default URL: `https://localhost:5124`
