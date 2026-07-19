namespace FlightStatus.Domain.Models;

public class ProviderFlightStatusData
{
    public string FlightNumber { get; set; } = string.Empty;
    public string Date { get; set; } = string.Empty;
    public string? Status { get; set; }
    public string? Message { get; set; }
    public DateTimeOffset? ScheduledDepartureUtc { get; set; }
    public DateTimeOffset? ActualDepartureUtc { get; set; }
    public DateTimeOffset? ScheduledArrivalUtc { get; set; }
    public DateTimeOffset? ActualArrivalUtc { get; set; }
    public string? Terminal { get; set; }
    public string? Gate { get; set; }
    public string? DelayReason { get; set; }
    public DateTimeOffset? LastUpdatedUtc { get; set; }
}
