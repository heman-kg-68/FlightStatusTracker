using FlightStatus.Application.Interfaces;
using FlightStatus.Domain.Models;
using FlightStatusEnum = FlightStatus.Domain.Enums.FlightStatus;

namespace FlightStatus.Application.Services;

/// <summary>
/// Queries all registered flight status providers, normalises their responses
/// </summary>
public class FlightStatusService : IFlightStatusService
{
    private readonly IEnumerable<IFlightStatusProvider> _providers;

    public FlightStatusService(IEnumerable<IFlightStatusProvider> providers)
    {
        _providers = providers;
    }

    public async Task<FlightStatusResult> GetFlightStatusAsync(
        string flightNumber,
        DateOnly date,
        CancellationToken cancellationToken)
    {
        var results = await QueryAllProvidersAsync(flightNumber, date, cancellationToken);

        var merged = results.Count == 0
            ? CreateUnknownResult(flightNumber, date)
            : SelectBestResult(results);

        merged.Message = BuildMessage(merged);
        return merged;
    }

    private async Task<List<FlightStatusResult>> QueryAllProvidersAsync(
        string flightNumber,
        DateOnly date,
        CancellationToken cancellationToken)
    {
        var tasks = _providers.Select(provider =>
            QuerySingleProviderAsync(provider, flightNumber, date, cancellationToken));

        var results = await Task.WhenAll(tasks);

        return results.Where(r => r is not null).Select(r => r!).ToList();
    }

    private static async Task<FlightStatusResult?> QuerySingleProviderAsync(
        IFlightStatusProvider provider,
        string flightNumber,
        DateOnly date,
        CancellationToken cancellationToken)
    {
        try
        {
            var data = await provider.GetStatusAsync(flightNumber, date, cancellationToken);
            return data is null ? null : NormalizeResult(data, provider.Name);
        }
        catch
        {
            return null;
        }
    }

    private static FlightStatusResult NormalizeResult(ProviderFlightStatusData data, string providerName)
    {
        var normalized = new FlightStatusResult
        {
            FlightNumber = data.FlightNumber,
            Date = data.Date,
            Status = NormalizeStatus(data.Status),
            Message = data.Message ?? "",
            ScheduledDepartureUtc = data.ScheduledDepartureUtc,
            ActualDepartureUtc = data.ActualDepartureUtc,
            ScheduledArrivalUtc = data.ScheduledArrivalUtc,
            ActualArrivalUtc = data.ActualArrivalUtc,
            Terminal = data.Terminal,
            Gate = data.Gate,
            DelayReason = data.DelayReason,
            LastUpdatedUtc = data.LastUpdatedUtc,
            SourceProvider = providerName
        };

        if (normalized.Status == FlightStatusEnum.Unknown)
        {
            normalized.Status = InferStatusFromTiming(normalized);
        }

        return normalized;
    }

    private static FlightStatusEnum NormalizeStatus(string? status) => status?.Trim().ToUpperInvariant() switch
    {
        "ON_TIME" or "SCHEDULED_ON_TIME" => FlightStatusEnum.OnTime,
        "DELAY" or "LATE" => FlightStatusEnum.Delayed,
        "CANCELLED" or "CANCELED" => FlightStatusEnum.Cancelled,
        "DIVERTED" or "DIVERT" => FlightStatusEnum.Diverted,
        _ => FlightStatusEnum.Unknown
    };

    /// <summary>
    /// Fallback for providers that report Unknown but still supplied schedule/actual
    /// times — derive OnTime/Delayed from the timing delta rather than giving up.
    /// </summary>
    private static FlightStatusEnum InferStatusFromTiming(FlightStatusResult result)
    {
        if (TryGetTimingDeltaMinutes(result.ScheduledDepartureUtc, result.ActualDepartureUtc, out var departureDelta))
        {
            return departureDelta <= 15 ? FlightStatusEnum.OnTime : FlightStatusEnum.Delayed;
        }

        if (TryGetTimingDeltaMinutes(result.ScheduledArrivalUtc, result.ActualArrivalUtc, out var arrivalDelta))
        {
            return arrivalDelta <= 15 ? FlightStatusEnum.OnTime : FlightStatusEnum.Delayed;
        }

        return FlightStatusEnum.Unknown;
    }

    private static bool TryGetTimingDeltaMinutes(DateTimeOffset? scheduledUtc, DateTimeOffset? actualUtc, out int deltaMinutes)
    {
        deltaMinutes = 0;

        if (!scheduledUtc.HasValue || !actualUtc.HasValue)
        {
            return false;
        }

        deltaMinutes = (int)Math.Abs((actualUtc.Value - scheduledUtc.Value).TotalMinutes);
        return true;
    }

    /// <summary>
    /// Picks the result to return when one or more providers responded.
    /// Prefers the later lastUpdatedUtc; on a tie, prefers a known status over Unknown.
    /// </summary>
    private static FlightStatusResult SelectBestResult(List<FlightStatusResult> results)
    {
        if (results.Count == 1)
        {
            return results[0];
        }

        return results
            .OrderByDescending(r => r.LastUpdatedUtc ?? DateTimeOffset.MinValue)
            .First();
    }

    private static FlightStatusResult CreateUnknownResult(string flightNumber, DateOnly date) => new()
    {
        FlightNumber = flightNumber,
        Date = date.ToString("yyyy-MM-dd"),
        Status = FlightStatusEnum.Unknown,
        Message = "No usable flight status was returned by either provider.",
        SourceProvider = null
    };

    private static string BuildMessage(FlightStatusResult result)
    {
        if (!string.IsNullOrWhiteSpace(result.Message))
        {
            return result.Message;
        }

        return result.Status switch
        {
            FlightStatusEnum.OnTime => "The flight is on time.",
            FlightStatusEnum.Delayed => "The flight is delayed.",
            FlightStatusEnum.Cancelled => "The flight has been cancelled.",
            FlightStatusEnum.Diverted => "The flight has been diverted.",
            _ => "No usable flight status was returned by either provider."
        };
    }
}