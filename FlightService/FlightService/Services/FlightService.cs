using FlightService.Domain;

namespace FlightService.Services
{
    public class FlightService(ILogger<FlightService> logger) : IFlightService
    {
        Flight IFlightService.getFlight(int id)
        {
            logger.LogInformation("Creating flight with id " + id);  //TODO make this structured (json)
            return new Flight(id, new DateOnly(2025, 9, 11), 1234, "ABC", "DEF", 'S', new DateTimeOffset(2025, 9, 11, 10, 5, 0, new TimeSpan(7, 0, 0)), new DateTimeOffset(2025, 9, 11, 12, 15, 0, new TimeSpan(5, 0, 0)));
        }
    }
}
