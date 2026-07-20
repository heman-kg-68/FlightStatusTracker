# reflection.md

## What I would improve with more time

- **Global exception handling.** A centralized exception-handling middleware
  (or `IExceptionHandler` in .NET 8) so unhandled errors return a consistent,
  clean response shape instead of relying on scattered try/catch blocks and
  the developer exception page. This would sit alongside — not replace —
  targeted try/catch blocks in places like provider calls, where catching
  and logging locally gives more useful context than letting the exception
  bubble all the way up before it's handled.
- **Structured logging.** Right now a failing provider and a genuinely missing
  flight number both silently resolve to "no data" — from the outside they're
  indistinguishable. Adding `ILogger` around provider calls would make real
  failures debuggable instead of hidden behind the same fallback path.
- **Split status normalization and merge logic into dedicated classes.**
  `FlightStatusService` currently handles orchestration, status normalization,
  and merge/tie-break logic together. Separating these concerns into their
  own files, each behind its own interface, would let the service focus
  purely on orchestration — querying providers and delegating the rest — and
  make normalization and merging independently testable without needing the
  full service wired up.
- **Configurable business constants.** The
  15-minute OnTime/Delayed threshold is currently a hardcoded literal inline
  in the code. Moving it to a named constant, or further into
  appsettings.json if it needs to change without a rebuild, would make it
  visible and changeable in one place — small thing, but it's a business
  rule, not an implementation detail.
- **Resilience patterns** (timeouts, retries, circuit breaker — e.g. via
  Polly) if real provider APIs ever replaced the stubs.

### Testing

- **Integration tests covering the full HTTP pipeline**, not just business
  logic — exercising the actual endpoint end-to-end, including the 400
  validation paths (missing flight number, missing date, malformed date),
  not just the service layer in isolation.
- **Frontend testing.**
  Adding tests for the UI Components would round out coverage —
  right now testing is entirely backend-focused, and the frontend states
  defined in the spec (results, empty, error) aren't verified automatically.

### API design

- **Standardize error response shapes** using `ProblemDetails` consistently —
  right now different validation failures (missing params vs. a malformed
  date) could plausibly return different response shapes depending on how
  each was implemented.
- **Input validation as a dedicated concern.** Flight number and date
  validation currently live inline in the endpoint. Extracting this into a
  small, focused validator (e.g. via FluentValidation or a simple validator
  class) would keep validation logic testable in isolation and easier to
  extend if more fields or rules are added later.
- **API versioning**, even if just documented as a future consideration —
  nothing urgent for a take-home challenge, but worth naming as a gap.
