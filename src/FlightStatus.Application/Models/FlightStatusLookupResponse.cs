using FlightStatus.Domain.Models;

namespace FlightStatus.Application.Models;

public class FlightStatusLookupResponse
{
    public FlightStatusResult? Result { get; set; }
    public string? Error { get; set; }
}
