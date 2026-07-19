using FlightStatus.Domain.Enums;
using FlightStatus.Domain.Interfaces;
using FlightStatus.Domain.Models;

namespace FlightStatus.Application.Services;

public class FlightStatusService
{
    private readonly IEnumerable<IFlightStatusProvider> _providers;

    public FlightStatusService(IEnumerable<IFlightStatusProvider> providers)
    {
        _providers = providers;
    }

    public async Task<FlightStatusResult> GetFlightStatusAsync(string flightNumber, string date, CancellationToken cancellationToken)
    {
        var results = new List<FlightStatusResult>();

        foreach (var provider in _providers)
        {
            var result = await provider.GetStatusAsync(flightNumber, date, cancellationToken);
            if (result is not null)
            {
                results.Add(NormalizeResult(result));
            }
        }

        if (results.Count == 0)
        {
            return CreateUnknownResult(flightNumber, date);
        }

        var selected = results.Aggregate((best, current) => ShouldPrefer(current, best) ? current : best);
        selected.Message = BuildMessage(selected);

        return selected;
    }

    private static FlightStatusResult NormalizeResult(FlightStatusResult result)
    {
        var normalized = new FlightStatusResult
        {
            FlightNumber = result.FlightNumber,
            Date = result.Date,
            Status = result.Status,
            Message = result.Message,
            ScheduledDepartureUtc = result.ScheduledDepartureUtc,
            ActualDepartureUtc = result.ActualDepartureUtc,
            ScheduledArrivalUtc = result.ScheduledArrivalUtc,
            ActualArrivalUtc = result.ActualArrivalUtc,
            Terminal = result.Terminal,
            Gate = result.Gate,
            DelayReason = result.DelayReason,
            LastUpdatedUtc = result.LastUpdatedUtc,
            SourceProvider = result.SourceProvider
        };

        if (normalized.Status != FlightStatus.Domain.Enums.FlightStatus.Unknown)
        {
            return normalized;
        }

        normalized.Status = InferStatusFromTiming(normalized);
        return normalized;
    }

    private static FlightStatus.Domain.Enums.FlightStatus InferStatusFromTiming(FlightStatusResult result)
    {
        if (TryGetTimingDelta(result.ScheduledDepartureUtc, result.ActualDepartureUtc, out var departureDelta))
        {
            return departureDelta <= 15 ? FlightStatus.Domain.Enums.FlightStatus.OnTime : FlightStatus.Domain.Enums.FlightStatus.Delayed;
        }

        if (TryGetTimingDelta(result.ScheduledArrivalUtc, result.ActualArrivalUtc, out var arrivalDelta))
        {
            return arrivalDelta <= 15 ? FlightStatus.Domain.Enums.FlightStatus.OnTime : FlightStatus.Domain.Enums.FlightStatus.Delayed;
        }

        return FlightStatus.Domain.Enums.FlightStatus.Unknown;
    }

    private static bool TryGetTimingDelta(DateTimeOffset? scheduledUtc, DateTimeOffset? actualUtc, out int deltaMinutes)
    {
        deltaMinutes = 0;

        if (!scheduledUtc.HasValue || !actualUtc.HasValue)
        {
            return false;
        }

        deltaMinutes = (int)Math.Abs((actualUtc.Value - scheduledUtc.Value).TotalMinutes);
        return true;
    }

    private static bool ShouldPrefer(FlightStatusResult candidate, FlightStatusResult current)
    {
        var candidateTimestamp = candidate.LastUpdatedUtc ?? DateTimeOffset.MinValue;
        var currentTimestamp = current.LastUpdatedUtc ?? DateTimeOffset.MinValue;

        if (candidateTimestamp != currentTimestamp)
        {
            return candidateTimestamp > currentTimestamp;
        }

        if (current.Status == FlightStatus.Domain.Enums.FlightStatus.Unknown && candidate.Status != FlightStatus.Domain.Enums.FlightStatus.Unknown)
        {
            return true;
        }

        if (candidate.Status == FlightStatus.Domain.Enums.FlightStatus.Unknown && current.Status != FlightStatus.Domain.Enums.FlightStatus.Unknown)
        {
            return false;
        }

        return false;
    }

    private static FlightStatusResult CreateUnknownResult(string flightNumber, string date)
    {
        return new FlightStatusResult
        {
            FlightNumber = flightNumber,
            Date = date,
            Status = FlightStatus.Domain.Enums.FlightStatus.Unknown,
            Message = "No usable flight status was returned by either provider.",
            SourceProvider = null
        };
    }

    private static string BuildMessage(FlightStatusResult result)
    {
        if (!string.IsNullOrWhiteSpace(result.Message))
        {
            return result.Message;
        }

        return result.Status switch
        {
            FlightStatus.Domain.Enums.FlightStatus.OnTime => "The flight is on time.",
            FlightStatus.Domain.Enums.FlightStatus.Delayed => "The flight is delayed.",
            FlightStatus.Domain.Enums.FlightStatus.Cancelled => "The flight has been cancelled.",
            FlightStatus.Domain.Enums.FlightStatus.Diverted => "The flight has been diverted.",
            _ => "No usable flight status was returned by either provider."
        };
    }
}
