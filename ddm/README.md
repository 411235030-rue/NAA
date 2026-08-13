# API_NAA_DDM

Local DDM proxy for the NAA demo.

## Local Environment

| Environment | Required | Default | Comment |
| --- | --- | --- | --- |
| `NAA_SERVICE_DOMAIN` | No | `https://localhost:5210` | Local API_NAA service URL |

No hospital logging database, mail plugin, OAuth plugin, or project-level NuGet package reference is required for local development.

## Routes

| Route | Method | Comment |
| --- | --- | --- |
| `/SaveHistory` | POST | Forward one turn using the existing ConversationId |
| `/GetConversationSummaries` | POST | Query active or deleted conversation summaries |
| `/GetConversationById` | POST | Load all turns for one owned conversation |
| `/SoftDeleteConversation` | POST | Soft-delete one owned conversation |
| `/RestoreConversation` | POST | Restore one owned conversation |
| `/ReviseText` | POST | Ask AgentBuilder and save the turn |

## Agent configuration

`IAgentService` isolates AgentBuilder from the DDM-to-API history flow. NAA keeps its
own `ConversationId`; the provider's conversation identifier is stored separately as
`AgentThreadId` on the server and is never supplied by Front.

Keep the future key on the DDM server only. For local development, use User Secrets:

```powershell
dotnet user-secrets set "Agent:ApiKey" "<key>" --project .\src\API_NAA_DDM\API_NAA_DDM.csproj
dotnet user-secrets set "Agent:BaseUrl" "https://your-agent-server/v1" --project .\src\API_NAA_DDM\API_NAA_DDM.csproj
```

For a deployed DDM, use the `Agent__ApiKey` environment variable or a server-side
secret provider. Do not put the key in appsettings files, Front, browser requests,
source control, or logs.

The Agent endpoint must use HTTPS. `Agent:ChatPath` defaults to `chat-messages` and can
be overridden without changing code if the application's API Access page specifies a
different path. The DDM never returns the configured key to Front.
