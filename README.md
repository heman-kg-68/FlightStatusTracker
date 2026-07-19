# Flight Status Tracker

A local, offline flight-status lookup application built with a .NET 8 Minimal API backend and a React + Vite frontend. The solution queries two deterministic stub providers, normalizes their response values into one unified model, merges their results using a clear business rule, and presents the outcome in a simple support-console UI.

## Overview

This repository implements the challenge requirements from the approved specification. It is designed to run completely locally without external APIs, credentials, persistence, or network dependencies.

The backend exposes a single flight-status endpoint that accepts a flight number and a travel date, queries two offline providers, normalizes the provider-specific status values, merges the outcomes, and returns a unified response payload.

## Features

- Backend endpoint for flight status lookup
- Deterministic stub providers for AeroTrack and QuickFlight
- Status normalization for OnTime, Delayed, Cancelled, Diverted, and Unknown
- Merge logic that prefers the newest provider result when both respond
- Time-based inference for unknown statuses when schedule/actual times are available
- Simple React UI for entering flight details and viewing the merged result
- Swagger/OpenAPI documentation for the API
- xUnit unit tests covering the core business logic

## Architecture Overview

The solution is organized into the following layers:

- API layer: Minimal API host and HTTP endpoint wiring
- Application layer: service orchestration and business logic
- Domain layer: domain models, enums, and shared contracts
- Infrastructure layer: deterministic provider implementations
- Tests: xUnit suite for the core service behavior

## Technology Stack

- Backend: .NET 8 Minimal API, C#
- Frontend: React 18 + Vite + JavaScript
- API docs: Swagger / OpenAPI
- Testing: xUnit

## Project Structure

```text
src/
  FlightStatus.Api/
    Program.cs
  FlightStatus.Application/
    Interfaces/
    Services/
  FlightStatus.Domain/
    Enums/
    Interfaces/
    Models/
  FlightStatus.Infrastructure/
    Providers/

tests/
  FlightStatus.Tests/
    Services/

flightstatus-ui/
  src/
  index.html
  package.json
  vite.config.js
```

## Setup

### Prerequisites

- .NET 8 SDK
- Node.js 18+ and npm

### Restore dependencies

From the repository root:

```bash
dotnet restore FlightStatusTracker.sln
cd flightstatus-ui
npm install
```

## Running the Backend

Start the API from the repository root:

```bash
cd src/FlightStatus.Api

dotnet run
```

The backend will be available at:

- http://localhost:5001/flights/status

Swagger UI is also available at:

- http://localhost:5001/swagger

## Running the Frontend

In a second terminal, start the React development server:

```bash
cd flightstatus-ui
npm run dev
```

The frontend will be available at:

- http://localhost:5174

The Vite dev server is configured to proxy requests to the backend at http://localhost:5001.

## API Endpoint

### GET /flights/status

Returns a normalized and merged flight status result for the supplied flight number and date.

#### Query parameters

- flightNumber: required flight identifier string
- date: required date in yyyy-MM-dd format

#### Example request

```bash
curl "http://localhost:5001/flights/status?flightNumber=A100&date=2026-07-19"
```

#### Example response

```json
{
  "flightNumber": "A100",
  "date": "2026-07-19",
  "status": "OnTime",
  "message": "AeroTrack reported the flight is on time.",
  "scheduledDepartureUtc": "2026-07-19T10:00:00+00:00",
  "actualDepartureUtc": "2026-07-19T09:55:00+00:00",
  "scheduledArrivalUtc": "2026-07-19T14:00:00+00:00",
  "actualArrivalUtc": "2026-07-19T14:00:00+00:00",
  "terminal": "A",
  "gate": "12",
  "delayReason": null,
  "lastUpdatedUtc": "2026-07-19T09:30:00+00:00",
  "sourceProvider": "AeroTrack"
}
```

### Validation behavior

If flightNumber or date is missing, empty, or invalid, the API returns HTTP 400.

## Testing

Run the full solution test suite:

```bash
dotnet test FlightStatusTracker.sln
```

The current implementation has 16 passing xUnit tests.

## Assumptions Made

The implementation follows the challenge requirements and makes these assumptions:

- The date parameter represents the travel date being searched.
- flightNumber is treated as a simple string and does not require airline-specific validation.
- Status values are normalized case-insensitively.
- Unknown values are treated as Unknown unless schedule/actual timing data can infer OnTime or Delayed.
- The 15-minute threshold is treated inclusively: within 15 minutes is OnTime, beyond 15 minutes is Delayed.
- The backend returns a clear message for Unknown outcomes rather than an empty response.

## Design Decisions

- The backend keeps the API layer thin and moves the business logic into the application layer.
- Providers are injected through interfaces so the merge and normalization logic can remain isolated and testable.
- The frontend is intentionally simple and focused on the core support-agent workflow.
- The providers are deterministic stub implementations so the application works offline and is easy to demo.

## Stub Data Scenarios

Stub data is configured based on flight number, so scenarios are
reproducible on any date you query. Use `date=2026-07-19` for the demo data below,
or any other date — the scenario still resolves correctly by flight number.

| Flight Number       | AeroTrack Response               | QuickFlight Response           | Unified Result | What it demonstrates                                                                  |
| ------------------- | -------------------------------- | ------------------------------ | -------------- | ------------------------------------------------------------------------------------- |
| `A100`              | ON_TIME                          | SCHEDULED_ON_TIME              | **OnTime**     | Both providers agree                                                                  |
| `A200`              | DELAY                            | Late                           | **Delayed**    | Both providers agree                                                                  |
| `A300`              | CANCELLED                        | CANCELED                       | **Cancelled**  | Both providers agree                                                                  |
| `A400`              | DIVERTED                         | Divert                         | **Diverted**   | Both providers agree                                                                  |
| `A500`              | ON_TIME                          | _(no data)_                    | **OnTime**     | Single-provider fallback — AeroTrack only                                             |
| `A600`              | _(no data)_                      | CANCELED                       | **Cancelled**  | Single-provider fallback — QuickFlight only                                           |
| `A700`              | ON_TIME (older `lastUpdatedUtc`) | Late (newer `lastUpdatedUtc`)  | **Delayed**    | Merge rule — later `lastUpdatedUtc` wins, even overriding a fuller AeroTrack response |
| `A800`              | unmapped/garbage status string   | unmapped/garbage status string | **Unknown**    | Both providers return unrecognised status codes                                       |
| _(any other value)_ | _(no data)_                      | _(no data)_                    | **Unknown**    | Neither provider responds                                                             |

**Note:** since QuickFlight is the "minimal" provider per spec, its response never
includes `terminal`, `gate`, `delayReason`, or actual departure/arrival times — even when
it's the only provider that responded. The frontend correctly omits those fields in this case.
