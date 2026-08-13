# API_NAA

Local NAA API for saving and querying chat history in SQL Server.

## Database migration

Before running this version, apply
`database/migrations/20260813_conversation_id_soft_delete.sql` to `MCH_NAA`.
The migration keeps existing conversation grouping values, renames `THREAD_ID` to
`CONVERSATION_ID`, adds soft-delete metadata, and adds nullable `AGENT_THREAD_ID` for
the server-side Agent conversation context. It does not delete question or answer rows.

Development uses Windows authentication. Keep the machine-specific connection string
outside source control with User Secrets:

```powershell
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "<connection-string>" --project .\src\API_NAA\API_NAA.csproj
```

The local connection string for this checkout has already been configured in the
current Windows user's secret store.

## Run

```powershell
dotnet run --project .\src\API_NAA\API_NAA.csproj --launch-profile https
```

Default URL: `https://localhost:5210`
