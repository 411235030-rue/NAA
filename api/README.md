# API_NAA

Local NAA API for saving and querying chat history in SQL Server.

Development uses Windows authentication to connect to `TUTA0204\SQLEXP2022`, database `MCH_NAA`. The connection string is stored in `src/API_NAA/appsettings.Development.json`.

## Run

```powershell
dotnet run --project .\src\API_NAA\API_NAA.csproj --launch-profile http
```

Default URL: `http://localhost:5210`
