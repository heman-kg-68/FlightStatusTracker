namespace FlightStatus.Application.Models;

public class FlightStatusLookupRequest
{
    public string FlightNumber { get; set; } = string.Empty;
    public string Date { get; set; } = string.Empty;
}
