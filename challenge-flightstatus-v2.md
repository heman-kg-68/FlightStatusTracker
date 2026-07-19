# Challenge: Flight Status Tracker

## Context

Build a Flight Status lookup feature for the SkyRoute platform. A support agent enters a flight number and date; the system queries two stub providers, normalises their responses into a unified status model, and displays the result.

No starter code is provided. Take this from brief to a running application — analysis, design, implementation, tests, and documentation.

## Functional Requirements

### Providers

| | AeroTrack | QuickFlight |
|---|---|---|
| Detail level | Full — status, scheduled & actual times, terminal, gate, delay reason | Minimal — status and scheduled times only |
| lastUpdatedUtc | Present | Present |

Both providers use different status vocabularies. Normalise both into the unified enum below.

### Unified Status Model

| Status | Rule |
|---|---|
| OnTime | Departure or arrival within 15 minutes of schedule |
| Delayed | Departure or arrival pushed beyond 15 minutes |
| Cancelled | Flight will not operate |
| Diverted | Flight landed at a different airport |
| Unknown | No usable status returned by either provider |

### Merge Rules

- If both providers return a result, prefer the one with the later `lastUpdatedUtc`
- If only one provider responds, use that result
- If neither responds, return `Unknown` with a clear message

### API

- `GET /flights/status?flightNumber={code}&date={yyyy-MM-dd}`
- Queries both stubs, normalises, merges, returns unified `FlightStatusResult`
- Return `400` if `flightNumber` or `date` is missing
- Use `IFlightStatusProvider` with two DI-injected stub implementations
- Stubs must be deterministic and cover multiple status scenarios

### Frontend

- Search form: flight number + date
- Result card: status with colour coding — green (OnTime), amber (Delayed), red (Cancelled / Diverted), grey (Unknown)
- AeroTrack-only fields (gate, terminal, delay reason) shown only when present
- Clear error state when the API fails

## Scope

- No real flight APIs, credentials, auth, or persistence
- Must run fully offline on a local machine

## Tech Stack

- **Backend:** .NET 8+ Minimal API (C#)
- **Frontend:** Angular, React, Blazor, or plain HTML/JS
- **Tests:** xUnit or NUnit
- **AI tooling:** Any AI tool is permitted for general use; for coding, an IDE-integrated tool (GitHub Copilot, Cursor, Claude Code) is mandatory

## AI Tooling Guidelines

- Use AI across the SDLC — analysis, modelling, tests, and documentation, not just code generation
- An IDE-integrated tool must be actively used during implementation
- Capture significant prompts and key judgement calls in `prompts.md`

## Submission

Submit via a public GitHub repository only — no zip files.

```
flight-status/
├── README.md          # setup, run steps, assumptions
├── spec.md            # data models and interface contracts — commit before implementation
├── FlightStatus.Api/
├── FlightStatus.Tests/
├── <case-name>-ui/
├── prompts.md          # AI prompts used, with notes on decisions
└── reflection.md       # what you would improve with more time
```

## Evaluation Criteria

- **Design & Architecture** — did you think before you coded? Is the solution structured, extensible, and properly abstracted?
- **Code, Tests & AI usage** — is the code clean and working? Are tests meaningful? Is AI tooling used thoughtfully and honestly reflected upon?
- **Operability & Delivery** — does it run from a clean clone using only your README? Is the submission complete and professional?

You will be expected to run and demo the application on your own device during the interview, and complete live change tasks on your running solution.

## Definition of Done

- Application runs end-to-end from a clean clone using only the README
- All specified API endpoints respond correctly, including error and edge cases
- Frontend reflects all defined states — results, empty, error
- Unit tests cover core business logic and are meaningful, not cosmetic
- AI tooling usage is evident, documented, and critically reflected upon
- `spec.md` is committed before any implementation files
- No secrets or credentials committed to the repository
