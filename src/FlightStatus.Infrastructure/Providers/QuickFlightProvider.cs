using FlightStatus.Domain.Enums;
using FlightStatus.Domain.Interfaces;
using FlightStatus.Domain.Models;

namespace FlightStatus.Infrastructure.Providers;

public class QuickFlightProvider : IFlightStatusProvider
{
    public string Name => "QuickFlight";

    public Task<FlightStatusResult?> GetStatusAsync(string flightNumber, string date, CancellationToken cancellationToken)
    {
        var result = new FlightStatusResult
        {
            FlightNumber = flightNumber,
            Date = date,
            Status = NormalizeStatus("Late"),
            Message = "QuickFlight reported the flight is delayed.",
            ScheduledDepartureUtc = new DateTimeOffset(2026, 7, 19, 10, 0, 0, TimeSpan.Zero),
            ActualDepartureUtc = new DateTimeOffset(2026, 7, 19, 10, 35, 0, TimeSpan.Zero),
            ScheduledArrivalUtc = new DateTimeOffset(2026, 7, 19, 14, 0, 0, TimeSpan.Zero),
            ActualArrivalUtc = new DateTimeOffset(2026, 7, 19, 14, 25, 0, TimeSpan.Zero),
            Terminal = null,
            Gate = null,
            DelayReason = null,
            LastUpdatedUtc = new DateTimeOffset(2026, 7, 19, 10, 10, 0, TimeSpan.Zero),
            SourceProvider = Name
        };

        return Task.FromResult<FlightStatusResult?>(result);
    }

    private static FlightStatus.Domain.Enums.FlightStatus NormalizeStatus(string? status)
    {
        return status?.Trim().ToUpperInvariant() switch
        {
            "SCHEDULED_ON_TIME" => FlightStatus.Domain.Enums.FlightStatus.OnTime,
            "LATE" => FlightStatus.Domain.Enums.FlightStatus.Delayed,
            "CANCELED" => FlightStatus.Domain.Enums.FlightStatus.Cancelled,
            "DIVERT" => FlightStatus.Domain.Enums.FlightStatus.Diverted,
            _ => FlightStatus.Domain.Enums.FlightStatus.Unknown
        };
    }
}
