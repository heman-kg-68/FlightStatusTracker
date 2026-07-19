using FlightStatus.Domain.Interfaces;
using FlightStatus.Domain.Models;

namespace FlightStatus.Infrastructure.Providers;

public class AeroTrackProvider : IFlightStatusProvider
{
private static readonly IReadOnlyDictionary<string, ProviderFlightStatusData> StubData =
        new Dictionary<string, ProviderFlightStatusData>(StringComparer.OrdinalIgnoreCase)
        {
            ["A100"] = new ProviderFlightStatusData
            {
                FlightNumber = "A100",
                Status = "ON_TIME",
                Message = "AeroTrack reported the flight is on time.",
                ScheduledDepartureUtc = new DateTimeOffset(2026, 7, 19, 10, 0, 0, TimeSpan.Zero),
                ActualDepartureUtc = new DateTimeOffset(2026, 7, 19, 9, 55, 0, TimeSpan.Zero),
                ScheduledArrivalUtc = new DateTimeOffset(2026, 7, 19, 14, 0, 0, TimeSpan.Zero),
                ActualArrivalUtc = new DateTimeOffset(2026, 7, 19, 14, 0, 0, TimeSpan.Zero),
                Terminal = "A",
                Gate = "12",
                LastUpdatedUtc = new DateTimeOffset(2026, 7, 19, 9, 30, 0, TimeSpan.Zero)
            },
            ["A200"] = new ProviderFlightStatusData
            {
                FlightNumber = "A200",
                Status = "DELAY",
                Message = "AeroTrack reported a delay.",
                ScheduledDepartureUtc = new DateTimeOffset(2026, 7, 19, 10, 0, 0, TimeSpan.Zero),
                ActualDepartureUtc = new DateTimeOffset(2026, 7, 19, 10, 40, 0, TimeSpan.Zero),
                ScheduledArrivalUtc = new DateTimeOffset(2026, 7, 19, 14, 0, 0, TimeSpan.Zero),
                ActualArrivalUtc = new DateTimeOffset(2026, 7, 19, 14, 45, 0, TimeSpan.Zero),
                Terminal = "A",
                Gate = "8",
                DelayReason = "Weather",
                LastUpdatedUtc = new DateTimeOffset(2026, 7, 19, 9, 45, 0, TimeSpan.Zero)
            },
            ["A300"] = new ProviderFlightStatusData
            {
                FlightNumber = "A300",
                Status = "CANCELLED",
                Message = "AeroTrack reported the flight is cancelled.",
                ScheduledDepartureUtc = new DateTimeOffset(2026, 7, 19, 10, 0, 0, TimeSpan.Zero),
                ScheduledArrivalUtc = new DateTimeOffset(2026, 7, 19, 14, 0, 0, TimeSpan.Zero),
                Terminal = "B",
                Gate = "3",
                DelayReason = "Mechanical issue",
                LastUpdatedUtc = new DateTimeOffset(2026, 7, 19, 8, 0, 0, TimeSpan.Zero)
            },
            ["A400"] = new ProviderFlightStatusData
            {
                FlightNumber = "A400",
                Status = "DIVERTED",
                Message = "AeroTrack reported the flight was diverted.",
                ScheduledDepartureUtc = new DateTimeOffset(2026, 7, 19, 10, 0, 0, TimeSpan.Zero),
                ActualDepartureUtc = new DateTimeOffset(2026, 7, 19, 10, 5, 0, TimeSpan.Zero),
                ScheduledArrivalUtc = new DateTimeOffset(2026, 7, 19, 14, 0, 0, TimeSpan.Zero),
                ActualArrivalUtc = new DateTimeOffset(2026, 7, 19, 15, 30, 0, TimeSpan.Zero),
                Terminal = "C",
                Gate = "21",
                DelayReason = "Diverted to alternate airport",
                LastUpdatedUtc = new DateTimeOffset(2026, 7, 19, 13, 0, 0, TimeSpan.Zero)
            },
            ["A500"] = new ProviderFlightStatusData
            {
                FlightNumber = "A500",
                Status = "ON_TIME",
                Message = "AeroTrack reported the flight is on time.",
                ScheduledDepartureUtc = new DateTimeOffset(2026, 7, 19, 10, 0, 0, TimeSpan.Zero),
                ActualDepartureUtc = new DateTimeOffset(2026, 7, 19, 9, 58, 0, TimeSpan.Zero),
                ScheduledArrivalUtc = new DateTimeOffset(2026, 7, 19, 14, 0, 0, TimeSpan.Zero),
                ActualArrivalUtc = new DateTimeOffset(2026, 7, 19, 14, 2, 0, TimeSpan.Zero),
                Terminal = "A",
                Gate = "5",
                LastUpdatedUtc = new DateTimeOffset(2026, 7, 19, 9, 30, 0, TimeSpan.Zero)
            },
            ["A700"] = new ProviderFlightStatusData
            {
                FlightNumber = "A700",
                Status = "ON_TIME",
                Message = "AeroTrack reported the flight is on time.",
                ScheduledDepartureUtc = new DateTimeOffset(2026, 7, 19, 10, 0, 0, TimeSpan.Zero),
                ActualDepartureUtc = new DateTimeOffset(2026, 7, 19, 9, 58, 0, TimeSpan.Zero),
                ScheduledArrivalUtc = new DateTimeOffset(2026, 7, 19, 14, 0, 0, TimeSpan.Zero),
                ActualArrivalUtc = new DateTimeOffset(2026, 7, 19, 14, 2, 0, TimeSpan.Zero),
                Terminal = "A",
                Gate = "9",
                LastUpdatedUtc = new DateTimeOffset(2026, 7, 19, 9, 0, 0, TimeSpan.Zero) // older
            },
            ["A800"] = new ProviderFlightStatusData
            {
                FlightNumber = "A800",
                Status = "SOMETHING_WEIRD", // unmapped on purpose -> normalises to Unknown
                Message = "AeroTrack returned an unrecognised status code.",
                ScheduledDepartureUtc = new DateTimeOffset(2026, 7, 19, 10, 0, 0, TimeSpan.Zero),
                ScheduledArrivalUtc = new DateTimeOffset(2026, 7, 19, 14, 0, 0, TimeSpan.Zero),
                LastUpdatedUtc = new DateTimeOffset(2026, 7, 19, 9, 0, 0, TimeSpan.Zero)
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
