# prompts.md

This file documents the significant AI prompts used across the SDLC for the Flight
Status Tracker challenge, in the order they were used, along with the purpose of
each and the key judgement calls that came out of them.

---

## 1. Requirements & architecture analysis

**Prompt:**

> You are a senior .NET Solution Architect. Read the attached case study carefully
> before making any recommendations. Your tasks are:
>
> 1. Summarize all functional requirements.
> 2. Summarize all non-functional requirements.
> 3. Identify any assumptions that are not explicitly stated.
> 4. Highlight any ambiguous requirements or areas where implementation decisions
>    are needed before development.
> 5. List potential edge cases that should be considered.
> 6. Identify the core business rules that should be isolated from infrastructure code.
> 7. Recommend a simple, maintainable architecture for a .NET 8 Minimal API solution
>    that is appropriate for this assignment.
> 8. Suggest a logical project and folder structure.
> 9. Explain why your proposed design is easy to test and extend.
>    Do not generate implementation code yet. Focus only on analysis and design
>    decisions that should be completed before writing spec.md.

**Purpose:** Force an explicit analysis-before-code step rather than jumping
straight to implementation. Used to surface ambiguities in the brief early —
before any commitment was made in spec.md or code.

**Key decisions / notes:** This is where the status-vocabulary ambiguity
(AeroTrack vs QuickFlight using different raw values) and the merge tie-break
gap (what happens if both providers share the same `lastUpdatedUtc`) were first
flagged as open questions needing an explicit decision.

---

## 2. Locking in the status-mapping assumption

**Prompt:**

> Locking in the status-mapping assumption for spec.md:
>
> - OnTime → AeroTrack: `ON_TIME`, QuickFlight: `SCHEDULED_ON_TIME`
> - Delayed → AeroTrack: `DELAY`, QuickFlight: `Late`
> - Cancelled → AeroTrack: `CANCELLED`, QuickFlight: `CANCELED`
> - Diverted → AeroTrack: `DIVERTED`, QuickFlight: `Divert`
> - Unknown → any value not listed above, or missing/null/empty

**Purpose:** The challenge brief never specifies the exact raw string vocabulary
each provider uses — only that "both providers use different status vocabularies."
This prompt made that assumption explicit and locked it in before any code
depended on it, so it could be committed to spec.md as a documented decision
rather than an implicit one buried in a switch statement.

**Key decisions / notes:** Matching is case-insensitive and exact-match only —
no fuzzy matching. Any unmapped, missing, null, or empty value normalises to
Unknown. This was implemented as a lookup rather than hardcoded if/else chains
so it's easy to extend if new providers or status codes are added later.

---

## 3. Generating spec.md

**Prompt:**

> Generate the spec.md file for this project based on key our decisions and
> assumptions.

**Purpose:** Produce the committed spec.md deliverable required by the
challenge's Definition of Done ("spec.md is committed before any implementation
files"). Consolidated the architecture analysis and the status-mapping
assumption into a single reference document before writing any code.

---

## 4. Generating the solution structure

**Prompt:**

> Generate the .NET 8 solution structure and folder organization based on
> agreed spec.md.

**Purpose:** Scaffold the actual project layout (FlightStatus.Api,
FlightStatus.Tests, layered folders for Domain/Application/Infrastructure)
to match the architecture agreed in spec.md, rather than letting structure
emerge ad hoc during implementation.

---

## 5. Implementing domain models, interfaces, and stub providers

**Prompt:**

> Implement the next phase of the backend.
>
> - Generate the domain models, enums, and DTOs.
> - Create the IFlightStatusProvider interface.
> - Implement deterministic AeroTrackProvider and QuickFlightProvider stub
>   implementations.
>   Once the implementation is complete, build the solution, fix any compilation
>   issues if they exist.

**Purpose:** First real implementation phase — the domain layer and the
provider abstraction (`IFlightStatusProvider`) that both stub providers
implement, satisfying the spec's requirement to use DI-injected stub
implementations rather than concrete provider references.

---

## 6. Implementing status normalization and merge logic

**Prompt:**

> Implement the status normalization, merge logic, in FlightStatusService.
> Keep the service logic simple and more readable.
>
> - If both providers return a result, prefer the one with the later lastUpdatedUtc
> - If only one provider responds, use that result
> - If neither responds, return Unknown with a clear message

**Purpose:** Core business-rule implementation — this is the heart of the
challenge (normalise two vocabularies into one, then merge per the documented
rules). Explicitly asked for simplicity/readability up front rather than
optimizing prematurely.

**Key decisions / notes:** This service was later revisited twice more: once
to parallelize the provider calls (see below) and once to split normalization
and merging out into their own classes for single-responsibility.

---

## 7. Making stub providers scenario-complete and switch-free

**Prompt:**

> Update the AeroTrackProvider and QuickFlightProvider to use deterministic
> stub data. Use the flight number as the primary lookup key and return
> predefined responses for different scenarios. Each provider should maintain
> its own stub dataset so they can return different results for the same
> flight when needed.
> Cover these scenarios:
>
> - Both providers return data (same or different status)
> - Latest LastUpdatedUtc wins
> - Only AeroTrack returns data
> - Only QuickFlight returns data
> - Neither provider returns data
>   Keep the implementation simple, maintainable, and easy to extend without
>   hardcoded switch statements. Build the solution after the changes and verify
>   it compiles successfully.

**Purpose:** Directly satisfies the spec's requirement that "stubs must be
deterministic and cover multiple status scenarios." Deliberately chose flight
number (not date) as the scenario key so the same input always produces the
same output regardless of when the demo is run.

**Key decisions / notes:** Landed on a flight-number scheme (A100–A800, plus
any unmatched number) covering: both-agree for each of the four known
statuses, single-provider-only for each provider, a `lastUpdatedUtc` tie-break
case, and a both-unmapped-status case. Each provider's dataset is a
`Dictionary<string, ProviderFlightStatusData>` rather than if/else or switch
logic, so adding a new scenario is a one-line addition. QuickFlight's stub
data was later corrected to strictly enforce the "minimal" detail level from
the spec (no actual times, terminal, gate, or delay reason — even internally
in its own stub dictionary, not just in the returned object).

---

## 8. Generating unit tests

**Prompt:**

> Based on the approved spec.md, generate meaningful xUnit unit tests covering
> the business logic, including status normalization, merge logic,
> FlightStatusService, and all important success and edge case scenarios.
> Build the solution, run the tests, fix any failing tests, and verify that
> all tests pass successfully.

**Purpose:** Satisfies the Definition of Done's requirement that "unit tests
cover core business logic and are meaningful, not cosmetic." Explicitly scoped
to business logic (normalization, merging) rather than testing infrastructure
or framework wiring.

---

## 9. Introducing IFlightStatusService and refactoring DI

**Prompt:**

> Introduce an IFlightStatusService interface and update the implementation to
> depend on it. Refactor dependency injection and update the unit tests to use
> the interface where appropriate.

**Purpose:** Improves testability and follows the same dependency-inversion
pattern already used for `IFlightStatusProvider` — the API layer and tests now
depend on an abstraction rather than the concrete `FlightStatusService`.

---

## 10. First pass: React UI

**Prompt:**

> Based on the approved spec.md, implement a simple React UI for the Flight
> Status Tracker in flightstatus-ui folder, integrate it with the backend API,
> build the application, fix any issues, and verify it runs successfully.

**Purpose:** Initial frontend implementation — search form, result card with
status colour coding, and conditional AeroTrack-only fields, per the spec's
frontend requirements.

---

## 11. Second pass: React UI on a separate port

**Prompt:**

> Based on the approved spec.md, implement a simple React UI in the
> flightstatus-ui folder, integrate it with the backend Flight Status API,
> configure it to run on a different port than the backend, build the
> application, fix any compilation or runtime issues, and verify both the
> frontend and backend run successfully together.

**Purpose:** Refinement pass on the frontend to ensure the dev server and API
run on distinct ports with a working proxy configuration (see `vite.config.js`),
so the app runs correctly from a clean clone without manual port juggling.

---

## 12. Generating README.md

**Prompt:**

> Based on the approved spec.md and the implemented solution, generate a
> professional README.md for the GitHub repository.
> Include: project overview, features, architecture overview, technology
> stack, project structure, setup and run instructions for backend and
> frontend, API endpoint details with a sample request and response,
> assumptions made based on the challenge requirements, design decisions, and
> known limitations.
> Ensure the setup steps, run instructions, and assumptions accurately reflect
> the challenge requirements and the current implementation. Verify that all
> commands, ports, project paths, and API endpoints are correct.

**Purpose:** Final documentation deliverable, required for the app to "run
end-to-end from a clean clone using only the README" per the Definition of
Done.

**Key decisions / notes:** The README's assumptions section was expanded to
include the stub-data scenario table (flight number → expected unified
status), since that table is the most useful thing for a reviewer or
interviewer to have on hand during a live demo.

---
