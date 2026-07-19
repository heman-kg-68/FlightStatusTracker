using FlightStatus.Domain.Interfaces;
using FlightStatus.Domain.Models;

namespace FlightStatus.Infrastructure.Providers;

public class QuickFlightProvider : IFlightStatusProvider
{
    private static readonly IReadOnlyDictionary<string, ProviderFlightStatusData> StubData =
        new Dictionary<string, ProviderFlightStatusData>(StringComparer.OrdinalIgnoreCase)
        {
            ["SKY-PAIR"] = new ProviderFlightStatusData
            {
                FlightNumber = "SKY-PAIR",
                Date = "2026-07-19",
                Status = "Late",
                Message = "QuickFlight reported the flight is delayed.",
                ScheduledDepartureUtc = new DateTimeOffset(2026, 7, 19, 10, 0, 0, TimeSpan.Zero),
                ActualDepartureUtc = new DateTimeOffset(2026, 7, 19, 10, 35, 0, TimeSpan.Zero),
                ScheduledArrivalUtc = new DateTimeOffset(2026, 7, 19, 14, 0, 0, TimeSpan.Zero),
                ActualArrivalUtc = new DateTimeOffset(2026, 7, 19, 14, 25, 0, TimeSpan.Zero),
                LastUpdatedUtc = new DateTimeOffset(2026, 7, 19, 10, 10, 0, TimeSpan.Zero)
            },
            ["SKY-QUICK-ONLY"] = new ProviderFlightStatusData
            {
                FlightNumber = "SKY-QUICK-ONLY",
                Date = "2026-07-19",
                Status = "CANCELED",
                Message = "QuickFlight reported the flight is canceled.",
                ScheduledDepartureUtc = new DateTimeOffset(2026, 7, 19, 10, 0, 0, TimeSpan.Zero),
                ActualDepartureUtc = new DateTimeOffset(2026, 7, 19, 10, 35, 0, TimeSpan.Zero),
                ScheduledArrivalUtc = new DateTimeOffset(2026, 7, 19, 14, 0, 0, TimeSpan.Zero),
                ActualArrivalUtc = new DateTimeOffset(2026, 7, 19, 14, 25, 0, TimeSpan.Zero),
                LastUpdatedUtc = new DateTimeOffset(2026, 7, 19, 10, 20, 0, TimeSpan.Zero)
            },
            ["SKY-SAME"] = new ProviderFlightStatusData
            {
                FlightNumber = "SKY-SAME",
                Date = "2026-07-19",
                Status = "SCHEDULED_ON_TIME",
                Message = "QuickFlight reported the flight is on time.",
                ScheduledDepartureUtc = new DateTimeOffset(2026, 7, 19, 10, 0, 0, TimeSpan.Zero),
                ActualDepartureUtc = new DateTimeOffset(2026, 7, 19, 9, 55, 0, TimeSpan.Zero),
                ScheduledArrivalUtc = new DateTimeOffset(2026, 7, 19, 14, 0, 0, TimeSpan.Zero),
                ActualArrivalUtc = new DateTimeOffset(2026, 7, 19, 14, 0, 0, TimeSpan.Zero),
                LastUpdatedUtc = new DateTimeOffset(2026, 7, 19, 10, 30, 0, TimeSpan.Zero)
            }
        };

    public string Name => "QuickFlight";

    public Task<ProviderFlightStatusData?> GetStatusAsync(string flightNumber, string date, CancellationToken cancellationToken)
    {
        if (!StubData.TryGetValue(flightNumber, out var result))
        {
            return Task.FromResult<ProviderFlightStatusData?>(null);
        }

        return Task.FromResult<ProviderFlightStatusData?>(new ProviderFlightStatusData
        {
            FlightNumber = result.FlightNumber,
            Date = date,
            Status = result.Status,
            Message = result.Message,
            ScheduledDepartureUtc = result.ScheduledDepartureUtc,
            ActualDepartureUtc = result.ActualDepartureUtc,
            ScheduledArrivalUtc = result.ScheduledArrivalUtc,
            ActualArrivalUtc = result.ActualArrivalUtc,
            Terminal = result.Terminal,
            Gate = result.Gate,
            DelayReason = result.DelayReason,
            LastUpdatedUtc = result.LastUpdatedUtc
        });
    }
}
