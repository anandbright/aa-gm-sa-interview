
using FlightService.Controllers;
using FlightService.Domain;
using FlightService.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace FlightServiceTest
{

    public class Generators {

        private Random random = new Random();

        private static int[] integerCornerCases = { 0, 1, -1, int.MaxValue, int.MinValue };
        private int intCalls = 0;
        public int integer() {
            if (intCalls < integerCornerCases.Length)
                return integerCornerCases[intCalls++];
            else
                return random.Next(int.MinValue, int.MaxValue);
        }

        public int integer(int max)
        {
            return random.Next(max);
        }

        public DateOnly dateOnly(DateOnly start, DateOnly end) {
            long ticks = end.ToDateTime(TimeOnly.MinValue).Ticks - start.ToDateTime(TimeOnly.MinValue).Ticks;
            long randomTicks = (long)(random.NextDouble() * ticks);
            DateTime randomDate = start.ToDateTime(TimeOnly.MinValue).AddTicks(randomTicks);
            return new DateOnly(randomDate.Year, randomDate.Month, randomDate.Day);
        }

        public String alphabetic(int length) {
            char[] chars = new char[length];
            for (int i = 0; i < length; i++)
            {
                int c = random.Next(26);
                chars[i] = (char)(c + (int)'A');
            }
            return new String(chars);
        }

        public char status() {
            char[] statuses = { 'S', 'N', 'T', 'I', 'F', 'C' };
            int index = random.Next(statuses.Length);
            return statuses[index];
        }
    }

    public class FlightTest
    {
        [Fact]
        public void constructorSets()
        {
            Generators gen = new Generators();
            for (int i = 0; i < 1000; i++) {
                int id = gen.integer();
                DateOnly date = gen.dateOnly(new DateOnly(2025, 1, 1), new DateOnly(2025, 12, 31));
                int number = gen.integer(9998) + 1;
                String origin = gen.alphabetic(3);
                String destination = gen.alphabetic(3);
                char status = gen.status();

                Flight flight = new Flight(id, date, number, origin, destination, status, new DateTimeOffset(2025, 9, 22, 10, 5, 0, new TimeSpan(7, 0, 0)), new DateTimeOffset(2025, 9, 22, 12, 15, 0, new TimeSpan(5, 0, 0)));
                Assert.Equal(id, flight.getId());
                Assert.Equal(date, flight.getDate());
                Assert.Equal(number, flight.getNumber());
                Assert.Equal(origin, flight.getOrigin());
                Assert.Equal(destination, flight.getDestination());

                Assert.Throws<ArgumentException>( () =>
                {
                    flight = new Flight(id, date, 10000, origin, destination, status, new DateTimeOffset(2025, 9, 22, 10, 5, 0, new TimeSpan(7, 0, 0)), new DateTimeOffset(2025, 9, 22, 12, 15, 0, new TimeSpan(5, 0, 0)));
                    flight = new Flight(id, date, -1, origin, destination, status, new DateTimeOffset(2025, 9, 22, 10, 5, 0, new TimeSpan(7, 0, 0)), new DateTimeOffset(2025, 9, 22, 12, 15, 0, new TimeSpan(5, 0, 0)));
                    flight = new Flight(id, date, number, "AB!", destination, status, new DateTimeOffset(2025, 9, 22, 10, 5, 0, new TimeSpan(7, 0, 0)), new DateTimeOffset(2025, 9, 22, 12, 15, 0, new TimeSpan(5, 0, 0)));
                    flight = new Flight(id, date, number, origin, "DE&", status, new DateTimeOffset(2025, 9, 22, 10, 5, 0, new TimeSpan(7, 0, 0)), new DateTimeOffset(2025, 9, 22, 12, 15, 0, new TimeSpan(5, 0, 0)));
                    flight = new Flight(id, date, number, origin, "def", status, new DateTimeOffset(2025, 9, 22, 10, 5, 0, new TimeSpan(7, 0, 0)), new DateTimeOffset(2025, 9, 22, 12, 15, 0, new TimeSpan(5, 0, 0)));
                });
            }
        }

        [Fact]
        public void departureAndArrivalTimeWorks() {
            Flight flight = new Flight(1, new DateOnly(2025, 9, 11), 1234, "ABC", "DEF", 'S', new DateTimeOffset(2025, 9, 11, 10, 5, 0, new TimeSpan(7, 0, 0)), new DateTimeOffset(2025, 9, 11, 12, 15, 0, new TimeSpan(5, 0, 0)));

            Assert.Equal(flight.getDeparture(), new DateTimeOffset(2025, 9, 11, 10, 5, 0, new TimeSpan(7, 0, 0)));

            flight.setStatus(Status.Out);
            Assert.True(flight.getActualDeparture().Value.ToUnixTimeSeconds() > flight.getScheduledDeparture().ToUnixTimeSeconds());

            flight.setStatus(Status.In);
            Assert.True(flight.getActualArrival().Value.ToUnixTimeSeconds() > flight.getScheduledArrival().ToUnixTimeSeconds());
        }

        [Fact]
        public void serviceGetReturnsFlight()
        {
            IFlightService flightService = new FlightService.Services.FlightService(Mock.Of<ILogger<FlightService.Services.FlightService>>());

            Generators gen = new Generators();
            for (int i = 0; i < 1000; i++)
            {
                int id = gen.integer();

                Flight flight = flightService.getFlight(id);
                Assert.Equal(id, flight.getId());
            }
        }

        [Fact]
        public void controllerGetReturnsFlight()
        {
            var mockService = new Mock<IFlightService>();
            mockService.Setup(service => 
                service.getFlight(It.IsAny<int>())
                )
                .Returns((int id) => 
                new Flight(id, new DateOnly(2025, 9, 11), 1234, "ABC", "DEF", 'S', new DateTimeOffset(2025, 9, 11, 10, 5, 0, new TimeSpan(7, 0, 0)), new DateTimeOffset(2025, 9, 11, 12, 15, 0, new TimeSpan(5, 0, 0)))
                );
            FlightController controller = new FlightController(mockService.Object, Mock.Of<ILogger<FlightController>>());

            Generators gen = new Generators();
            for (int i = 0; i < 1000; i++)
            {
                int id = gen.integer();

                ActionResult<Flight> result = controller.Get(id);
                Assert.Equal(id, result.Value?.getId());
            }
        }
    }
}
