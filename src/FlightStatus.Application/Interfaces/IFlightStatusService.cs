using FlightStatus.Domain.Models;

namespace FlightStatus.Application.Interfaces;

public interface IFlightStatusService
{
    Task<FlightStatusResult> GetFlightStatusAsync(
        string flightNumber,
        DateOnly date,
        CancellationToken cancellationToken);
}
