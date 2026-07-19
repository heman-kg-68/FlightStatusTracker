using FlightStatus.Domain.Interfaces;
using FlightStatus.Domain.Models;

namespace FlightStatus.Infrastructure.Providers;

public class QuickFlightProvider : IFlightStatusProvider
{
    private static readonly IReadOnlyDictionary<string, ProviderFlightStatusData> StubData =
        new Dictionary<string, ProviderFlightStatusData>(StringComparer.OrdinalIgnoreCase)
        {
            ["A100"] = new ProviderFlightStatusData
            {
                FlightNumber = "A100",
                Status = "SCHEDULED_ON_TIME",
                Message = "QuickFlight reported the flight is on time.",
                ScheduledDepartureUtc = new DateTimeOffset(2026, 7, 19, 10, 0, 0, TimeSpan.Zero),
                ScheduledArrivalUtc = new DateTimeOffset(2026, 7, 19, 14, 0, 0, TimeSpan.Zero),
                LastUpdatedUtc = new DateTimeOffset(2026, 7, 19, 9, 20, 0, TimeSpan.Zero)
            },
            ["A200"] = new ProviderFlightStatusData
            {
                FlightNumber = "A200",
                Status = "Late",
                Message = "QuickFlight reported the flight is delayed.",
                ScheduledDepartureUtc = new DateTimeOffset(2026, 7, 19, 10, 0, 0, TimeSpan.Zero),
                ScheduledArrivalUtc = new DateTimeOffset(2026, 7, 19, 14, 0, 0, TimeSpan.Zero),
                LastUpdatedUtc = new DateTimeOffset(2026, 7, 19, 9, 35, 0, TimeSpan.Zero)
            },
            ["A300"] = new ProviderFlightStatusData
            {
                FlightNumber = "A300",
                Status = "CANCELED",
                Message = "QuickFlight reported the flight is canceled.",
                ScheduledDepartureUtc = new DateTimeOffset(2026, 7, 19, 10, 0, 0, TimeSpan.Zero),
                ScheduledArrivalUtc = new DateTimeOffset(2026, 7, 19, 14, 0, 0, TimeSpan.Zero),
                LastUpdatedUtc = new DateTimeOffset(2026, 7, 19, 7, 45, 0, TimeSpan.Zero)
            },
            ["A400"] = new ProviderFlightStatusData
            {
                FlightNumber = "A400",
                Status = "Divert",
                Message = "QuickFlight reported the flight was diverted.",
                ScheduledDepartureUtc = new DateTimeOffset(2026, 7, 19, 10, 0, 0, TimeSpan.Zero),
                ScheduledArrivalUtc = new DateTimeOffset(2026, 7, 19, 14, 0, 0, TimeSpan.Zero),
                LastUpdatedUtc = new DateTimeOffset(2026, 7, 19, 12, 50, 0, TimeSpan.Zero)
            },
            ["A600"] = new ProviderFlightStatusData
            {
                FlightNumber = "A600",
                Status = "CANCELED",
                Message = "QuickFlight reported the flight is canceled.",
                ScheduledDepartureUtc = new DateTimeOffset(2026, 7, 19, 10, 0, 0, TimeSpan.Zero),
                ScheduledArrivalUtc = new DateTimeOffset(2026, 7, 19, 14, 0, 0, TimeSpan.Zero),
                LastUpdatedUtc = new DateTimeOffset(2026, 7, 19, 8, 15, 0, TimeSpan.Zero)
            },
            ["A700"] = new ProviderFlightStatusData
            {
                FlightNumber = "A700",
                Status = "Late",
                Message = "QuickFlight reported the flight is delayed.",
                ScheduledDepartureUtc = new DateTimeOffset(2026, 7, 19, 10, 0, 0, TimeSpan.Zero),
                ScheduledArrivalUtc = new DateTimeOffset(2026, 7, 19, 14, 0, 0, TimeSpan.Zero),
                LastUpdatedUtc = new DateTimeOffset(2026, 7, 19, 10, 15, 0, TimeSpan.Zero) // newer than AeroTrack's A700 entry
            },
            ["A800"] = new ProviderFlightStatusData
            {
                FlightNumber = "A800",
                Status = "NOT_A_REAL_STATUS", // unmapped on purpose -> normalises to Unknown
                Message = "QuickFlight returned an unrecognised status code.",
                ScheduledDepartureUtc = new DateTimeOffset(2026, 7, 19, 10, 0, 0, TimeSpan.Zero),
                ScheduledArrivalUtc = new DateTimeOffset(2026, 7, 19, 14, 0, 0, TimeSpan.Zero),
                LastUpdatedUtc = new DateTimeOffset(2026, 7, 19, 9, 10, 0, TimeSpan.Zero)
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
