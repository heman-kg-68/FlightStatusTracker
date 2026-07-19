using FlightStatus.Domain.Interfaces;
using FlightStatus.Domain.Models;

namespace FlightStatus.Infrastructure.Providers;

public class AeroTrackProvider : IFlightStatusProvider
{
    private static readonly IReadOnlyDictionary<string, ProviderFlightStatusData> StubData =
        new Dictionary<string, ProviderFlightStatusData>(StringComparer.OrdinalIgnoreCase)
        {
            ["SKY-PAIR"] = new ProviderFlightStatusData
            {
                FlightNumber = "SKY-PAIR",
                Date = "2026-07-19",
                Status = "ON_TIME",
                Message = "AeroTrack reported the flight is on time.",
                ScheduledDepartureUtc = new DateTimeOffset(2026, 7, 19, 10, 0, 0, TimeSpan.Zero),
                ActualDepartureUtc = new DateTimeOffset(2026, 7, 19, 9, 55, 0, TimeSpan.Zero),
                ScheduledArrivalUtc = new DateTimeOffset(2026, 7, 19, 14, 0, 0, TimeSpan.Zero),
                ActualArrivalUtc = new DateTimeOffset(2026, 7, 19, 14, 0, 0, TimeSpan.Zero),
                LastUpdatedUtc = new DateTimeOffset(2026, 7, 19, 9, 30, 0, TimeSpan.Zero)
            },
            ["SKY-AERO-ONLY"] = new ProviderFlightStatusData
            {
                FlightNumber = "SKY-AERO-ONLY",
                Date = "2026-07-19",
                Status = "DELAY",
                Message = "AeroTrack reported a delay.",
                ScheduledDepartureUtc = new DateTimeOffset(2026, 7, 19, 10, 0, 0, TimeSpan.Zero),
                ActualDepartureUtc = new DateTimeOffset(2026, 7, 19, 10, 5, 0, TimeSpan.Zero),
                ScheduledArrivalUtc = new DateTimeOffset(2026, 7, 19, 14, 0, 0, TimeSpan.Zero),
                ActualArrivalUtc = new DateTimeOffset(2026, 7, 19, 14, 15, 0, TimeSpan.Zero),
                Terminal = "A",
                Gate = "12",
                DelayReason = "Weather",
                LastUpdatedUtc = new DateTimeOffset(2026, 7, 19, 9, 40, 0, TimeSpan.Zero)
            },
            ["SKY-SAME"] = new ProviderFlightStatusData
            {
                FlightNumber = "SKY-SAME",
                Date = "2026-07-19",
                Status = "ON_TIME",
                Message = "AeroTrack reported the flight is on time.",
                ScheduledDepartureUtc = new DateTimeOffset(2026, 7, 19, 10, 0, 0, TimeSpan.Zero),
                ActualDepartureUtc = new DateTimeOffset(2026, 7, 19, 9, 55, 0, TimeSpan.Zero),
                ScheduledArrivalUtc = new DateTimeOffset(2026, 7, 19, 14, 0, 0, TimeSpan.Zero),
                ActualArrivalUtc = new DateTimeOffset(2026, 7, 19, 14, 0, 0, TimeSpan.Zero),
                LastUpdatedUtc = new DateTimeOffset(2026, 7, 19, 9, 50, 0, TimeSpan.Zero)
            }
        };

    public string Name => "AeroTrack";

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
