using FlightService.Domain;

namespace FlightService.Services
{
    public class FlightService(ILogger<FlightService> logger) : IFlightService
    {
        Flight IFlightService.getFlight(int id)
        {
            logger.LogInformation("Creating flight with id " + id);  //TODO make this structured (json)
            return new Flight(id: id,
                flightDate: new DateOnly(2025, 10, 15),
                flightNumber: 1000 + id, // Make flight number unique based on ID
                origin: GetOriginByFlightId(id),
                destination: GetDestinationByFlightId(id),
                status: GetStatusByFlightId(id),
                departureTime: new DateTimeOffset(2025, 10, 15, 10, 30, 0, TimeSpan.FromHours(-5)),
                arrivalTime: new DateTimeOffset(2025, 10, 15, 14, 45, 0, TimeSpan.FromHours(-8))
            );
        }
    }
}
