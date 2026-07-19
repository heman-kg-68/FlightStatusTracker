using FlightStatus.Domain.Enums;
using FlightStatus.Domain.Interfaces;
using FlightStatus.Domain.Models;

namespace FlightStatus.Infrastructure.Providers;

public class AeroTrackProvider : IFlightStatusProvider
{
    public string Name => "AeroTrack";

    public Task<FlightStatusResult?> GetStatusAsync(string flightNumber, string date, CancellationToken cancellationToken)
    {
        var result = new FlightStatusResult
        {
            FlightNumber = flightNumber,
            Date = date,
            Status = NormalizeStatus("ON_TIME"),
            Message = "AeroTrack reported the flight is on time.",
            ScheduledDepartureUtc = new DateTimeOffset(2026, 7, 19, 10, 0, 0, TimeSpan.Zero),
            ActualDepartureUtc = new DateTimeOffset(2026, 7, 19, 9, 55, 0, TimeSpan.Zero),
            ScheduledArrivalUtc = new DateTimeOffset(2026, 7, 19, 14, 0, 0, TimeSpan.Zero),
            ActualArrivalUtc = new DateTimeOffset(2026, 7, 19, 14, 0, 0, TimeSpan.Zero),
            Terminal = "A",
            Gate = "12",
            DelayReason = null,
            LastUpdatedUtc = new DateTimeOffset(2026, 7, 19, 9, 30, 0, TimeSpan.Zero),
            SourceProvider = Name
        };

        return Task.FromResult<FlightStatusResult?>(result);
    }

    private static FlightStatus.Domain.Enums.FlightStatus NormalizeStatus(string? status)
    {
        return status?.Trim().ToUpperInvariant() switch
        {
            "ON_TIME" => FlightStatus.Domain.Enums.FlightStatus.OnTime,
            "DELAY" => FlightStatus.Domain.Enums.FlightStatus.Delayed,
            "CANCELLED" => FlightStatus.Domain.Enums.FlightStatus.Cancelled,
            "DIVERTED" => FlightStatus.Domain.Enums.FlightStatus.Diverted,
            _ => FlightStatus.Domain.Enums.FlightStatus.Unknown
        };
    }
}
