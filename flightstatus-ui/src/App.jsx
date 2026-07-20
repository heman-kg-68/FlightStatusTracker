import { useState } from "react";
import "./styles.css";

const defaultFlightNumber = "A100";
const defaultDate = "2026-07-19";

const statusStyles = {
  OnTime: {
    label: "On Time",
    className: "status-pill on-time",
  },
  Delayed: {
    label: "Delayed",
    className: "status-pill delayed",
  },
  Cancelled: {
    label: "Cancelled",
    className: "status-pill cancelled",
  },
  Diverted: {
    label: "Diverted",
    className: "status-pill diverted",
  },
  Unknown: {
    label: "Unknown",
    className: "status-pill unknown",
  },
};

function App() {
  const [flightNumber, setFlightNumber] = useState(defaultFlightNumber);
  const [date, setDate] = useState(defaultDate);
  const [result, setResult] = useState(null);
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(false);
  const [hasSearched, setHasSearched] = useState(false);

  const handleSubmit = async (event) => {
    event.preventDefault();
    setLoading(true);
    setError("");
    setHasSearched(true);

    try {
      const response = await fetch(
        `api/flights/status?flightNumber=${encodeURIComponent(flightNumber)}&date=${encodeURIComponent(date)}`,
      );

      const payload = await response.json().catch(() => null);

      if (!response.ok) {
        throw new Error(
          payload?.error || "The request could not be completed.",
        );
      }

      setResult(payload);
    } catch (err) {
      setResult(null);
      setError(err.message || "Unable to load flight status right now.");
    } finally {
      setLoading(false);
    }
  };

  const statusKey = result?.status || "Unknown";
  const pill = statusStyles[statusKey] || statusStyles.Unknown;

  return (
    <div className="app-shell">
      <div className="card">
        <header>
          <p className="eyebrow">SkyRoute Support Console</p>
          <h1>Flight Status Tracker</h1>
        </header>

        <form onSubmit={handleSubmit} className="search-form">
          <label>
            <span>Flight number</span>
            <input
              value={flightNumber}
              onChange={(event) => setFlightNumber(event.target.value)}
              placeholder="e.g. SKY-PAIR"
              required
            />
          </label>

          <label>
            <span>Date</span>
            <input
              type="date"
              value={date}
              onChange={(event) => setDate(event.target.value)}
              required
            />
          </label>

          <button type="submit" disabled={loading}>
            {loading ? "Loading…" : "Check status"}
          </button>
        </form>

        {error ? <div className="alert">{error}</div> : null}

        {!hasSearched ? (
          <div className="empty-state">
            <h2>Search for a flight</h2>
            <p>
              Use the form above to retrieve the merged status from AeroTrack
              and QuickFlight.
            </p>
          </div>
        ) : null}

        {result ? (
          <section className="result-panel">
            <div className="result-header">
              <div>
                <p className="eyebrow">Current status</p>
                <h2>{result.flightNumber}</h2>
              </div>
              <span className={pill.className}>{pill.label}</span>
            </div>

            <p className="message">{result.message}</p>

            <div className="meta-grid">
              <div>
                <span className="meta-label">Date</span>
                <strong>{result.date}</strong>
              </div>
              <div>
                <span className="meta-label">Provider</span>
                <strong>{result.sourceProvider || "Unknown"}</strong>
              </div>
              <div>
                <span className="meta-label">Last updated</span>
                <strong>
                  {result.lastUpdatedUtc
                    ? new Date(result.lastUpdatedUtc).toLocaleString()
                    : "Unavailable"}
                </strong>
              </div>
            </div>

            <div className="detail-grid">
              {result.terminal ? (
                <div>
                  <span className="meta-label">Terminal</span>
                  <strong>{result.terminal}</strong>
                </div>
              ) : null}
              {result.gate ? (
                <div>
                  <span className="meta-label">Gate</span>
                  <strong>{result.gate}</strong>
                </div>
              ) : null}
              {result.delayReason ? (
                <div>
                  <span className="meta-label">Delay reason</span>
                  <strong>{result.delayReason}</strong>
                </div>
              ) : null}
            </div>
          </section>
        ) : null}
      </div>
    </div>
  );
}

export default App;
