using FlightStatus.Domain.Models;

namespace FlightStatus.Domain.Interfaces;

public interface IFlightStatusProvider
{
    string Name { get; }

    Task<FlightStatusResult?> GetStatusAsync(string flightNumber, string date, CancellationToken cancellationToken);
}
