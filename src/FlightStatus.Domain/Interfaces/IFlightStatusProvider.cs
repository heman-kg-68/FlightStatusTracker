using FlightStatus.Domain.Models;

namespace FlightStatus.Domain.Interfaces;

public interface IFlightStatusProvider
{
    string Name { get; }

    Task<ProviderFlightStatusData?> GetStatusAsync(string flightNumber, DateOnly date, CancellationToken cancellationToken);
}
