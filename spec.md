# Specification: Flight Status Tracker

## 1. Overview

This project implements a flight status lookup feature for the SkyRoute platform. A support agent can enter a flight number and a travel date, submit the request, and receive a normalized flight status result derived from two offline stub providers.

The solution will be implemented as a .NET 8 Minimal API backend with a simple React + JavaScript frontend. The implementation must run locally without external services, credentials, persistence, or real flight APIs.

---

## 2. Goals

- Provide a working end-to-end flight status lookup experience.
- Query two deterministic stub providers.
- Normalize provider-specific status values into a single unified model.
- Merge provider results using a clearly defined business rule.
- Deliver a simple, maintainable, testable solution suitable for the assignment.

---

## 3. Functional Requirements

### 3.1 API Endpoint

The backend must expose the following endpoint:

- GET /flights/status?flightNumber={code}&date={yyyy-MM-dd}

### 3.2 Input Validation

The API must return HTTP 400 when either of the following is missing or empty:

- flightNumber
- date

### 3.3 Provider Integration

The backend must query two stub providers:

- AeroTrack
- QuickFlight

Both providers will be implemented behind the interface `IFlightStatusProvider` and injected via dependency injection.

### 3.4 Status Normalization

Both providers use different status vocabularies and must be normalized into a single unified status model.

#### Status mapping contract

- OnTime
  - AeroTrack: `ON_TIME`
  - QuickFlight: `SCHEDULED_ON_TIME`

- Delayed
  - AeroTrack: `DELAY`
  - QuickFlight: `Late`

- Cancelled
  - AeroTrack: `CANCELLED`
  - QuickFlight: `CANCELED`

- Diverted
  - AeroTrack: `DIVERTED`
  - QuickFlight: `Divert`

- Unknown
  - Any value not listed above
  - Missing, null, or empty values

Normalization must be case-insensitive and should treat unrecognized values as `Unknown`.

### 3.5 Merge Rules

The system must merge results from both providers using the following rules:

1. If both providers return a result, prefer the provider with the later `lastUpdatedUtc`.
2. If only one provider returns a result, use that result.
3. If neither provider returns a result, return `Unknown` with a clear message.

### 3.6 Unified Response Model

The API must return a unified `FlightStatusResult` object containing:

- flightNumber
- date
- status
- message
- scheduledDepartureUtc
- actualDepartureUtc
- scheduledArrivalUtc
- actualArrivalUtc
- terminal
- gate
- delayReason
- lastUpdatedUtc
- sourceProvider

If a field is not available from the selected provider, it should be omitted or set to `null` as appropriate.

### 3.7 Frontend Requirements

The frontend must provide:

- A search form with flight number and date inputs
- A submit action that calls the backend API
- A result card showing the final status with color coding
- AeroTrack-only fields (gate, terminal, delayReason) shown only when present
- A clear error state when the API fails

### 3.8 UI Status Visuals

The frontend must render status with the following colors:

- Green: `OnTime`
- Amber: `Delayed`
- Red: `Cancelled` or `Diverted`
- Grey: `Unknown`

---

## 4. Non-Functional Requirements

### 4.1 Technology Stack

- Backend: .NET 8 Minimal API in C#
- Frontend: React with JavaScript
- Testing: xUnit
- Offline execution: fully local, no real network calls

### 4.2 Maintainability

The solution should be structured so that domain logic is separated from infrastructure and API concerns.

### 4.3 Determinism

The stub providers must be deterministic and should return predefined scenarios for testing and demo purposes.

### 4.4 Testability

Business rules must be isolated so they can be tested without requiring HTTP or frontend components.

### 4.5 Documentation

The repository must include:

- README with setup instructions
- spec.md describing the design and contracts
- prompts.md documenting AI usage and decisions
- reflection.md summarizing improvements that could be made later

---

## 5. Assumptions and Clarifications

The following assumptions are part of this specification and should be implemented consistently:

1. The `date` parameter represents the travel date being searched.
2. `flightNumber` is treated as a simple code string and does not require airline-specific validation.
3. A time-based status should be inferred from available departure/arrival schedule and actual values when a provider does not provide a recognized status value.
4. The 15-minute threshold is interpreted as inclusive of the threshold:
   - within 15 minutes of schedule => `OnTime`
   - beyond 15 minutes => `Delayed`
5. If both providers return conflicting statuses and the same `lastUpdatedUtc`, the provider with the later timestamp should still be preferred; if timestamps are equal, the implementation should use a deterministic tie-break rule.
6. The API should return a clear message for `Unknown` results rather than an empty response.

---

## 6. Domain Model

### 6.1 Unified Status Enum

```csharp
public enum FlightStatus
{
    OnTime,
    Delayed,
    Cancelled,
    Diverted,
    Unknown
}
```

### 6.2 FlightStatusResult

```csharp
public class FlightStatusResult
{
    public string FlightNumber { get; set; } = string.Empty;
    public string Date { get; set; } = string.Empty;
    public FlightStatus Status { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTimeOffset? ScheduledDepartureUtc { get; set; }
    public DateTimeOffset? ActualDepartureUtc { get; set; }
    public DateTimeOffset? ScheduledArrivalUtc { get; set; }
    public DateTimeOffset? ActualArrivalUtc { get; set; }
    public string? Terminal { get; set; }
    public string? Gate { get; set; }
    public string? DelayReason { get; set; }
    public DateTimeOffset? LastUpdatedUtc { get; set; }
    public string? SourceProvider { get; set; }
}
```

---

## 7. Provider Contract

The provider abstraction must be defined as:

```csharp
public interface IFlightStatusProvider
{
    string Name { get; }
    Task<FlightStatusResult?> GetStatusAsync(string flightNumber, string date, CancellationToken cancellationToken);
}
```

Each provider implementation must:

- Accept the same input parameters
- Return either a normalized result or `null` if no usable data is available
- Be deterministic for the assignment

---

## 8. API Response Contract

### 8.1 Success Response

HTTP 200 with a JSON body representing `FlightStatusResult`.

Example:

```json
{
  "flightNumber": "SKY123",
  "date": "2026-07-19",
  "status": "Delayed",
  "message": "Flight is delayed based on provider data.",
  "scheduledDepartureUtc": "2026-07-19T10:00:00Z",
  "actualDepartureUtc": "2026-07-19T10:25:00Z",
  "scheduledArrivalUtc": "2026-07-19T14:00:00Z",
  "actualArrivalUtc": "2026-07-19T14:40:00Z",
  "terminal": "A",
  "gate": "12",
  "delayReason": "Weather",
  "lastUpdatedUtc": "2026-07-19T09:35:00Z",
  "sourceProvider": "AeroTrack"
}
```

### 8.2 Validation Error Response

HTTP 400 with a simple error payload such as:

```json
{
  "error": "flightNumber and date are required"
}
```

### 8.3 Unknown Result Response

HTTP 200 with a `FlightStatusResult` where:

- `status` is `Unknown`
- `message` clearly explains that neither provider returned usable data

---

## 9. Frontend Specification

The frontend will be a simple React + JavaScript single-page application.

### 9.1 UI Elements

- Flight number input
- Date input
- Search button
- Result card
- Error banner
- Empty state when no search has been performed yet

### 9.2 Behavior

- On submit, the frontend calls the backend endpoint.
- On success, it renders the returned result.
- On validation or network failure, it displays a clear message.
- AeroTrack-only fields are rendered only if they are present.

---

## 10. Testing Scope

The solution should include automated unit tests that cover the core business logic and validate meaningful domain behavior.

### 10.1 Unit Tests

Tests should verify:

- normalization of all provider status values
- status inference from schedule/actual time differences
- merge precedence when both providers respond
- fallback behavior when only one provider responds
- unknown-result behavior when no provider data is usable
- deterministic handling of invalid, missing, or unrecognized values

The tests should focus on business rules and decision-making rather than superficial coverage or mock-only validation.

---

## 11. Implementation Structure

A simple maintainable structure is:

```text
src/
  FlightStatus.Api/
    Program.cs
    Endpoints/
    Models/
  FlightStatus.Application/
    Services/
  FlightStatus.Domain/
    Models/
    Enums/
    Interfaces/
  FlightStatus.Infrastructure/
    Providers/
    StubData/

tests/
  FlightStatus.Tests/
    Domain/
    Application/
    Api/

flightstatus-ui/
  src/
```

The API project should remain thin. Domain/business rules should live outside the API layer.

---

## 12. Open Implementation Notes

The following are implementation decisions that should be made during development but are already scoped for the solution:

- The exact tie-break rule when both providers have equal timestamps should be deterministic and documented in code.
- The message text for `Unknown` and delayed results should be consistent and user-friendly.
- The frontend should remain intentionally simple and avoid unnecessary complexity.

---

## 13. Definition of Done

The implementation is complete when:

- The API endpoint works end-to-end locally.
- Both stub providers are queried and merged correctly.
- Status normalization follows the documented mapping contract.
- The frontend displays success, empty, and error states correctly.
- Unit tests cover the core business rules.
- Documentation is present and sufficient for a clean-clone setup.
