# API_NAA_DDM

Local DDM proxy for the NAA demo.

## Local Environment

| Environment | Required | Default | Comment |
| --- | --- | --- | --- |
| `NAA_SERVICE_DOMAIN` | No | `http://localhost:5210` | Local API_NAA service URL |

No hospital logging database, mail plugin, OAuth plugin, external AI service key, or project-level NuGet package reference is required for local development.

## Routes

| Route | Method | Comment |
| --- | --- | --- |
| `/SaveHistory` | POST | Forward history to local API |
| `/GetHistoryByAccount` | POST | Query history from local API |
| `/ReviseText` | POST | Return a local demo text revision |
