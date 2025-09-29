namespace FlightService.Domain
{
    public class Flight
    {
        private int id;
        private DateOnly date;
        private int number;
        private String origin;
        private String destination;
        private Status status;
        private DateTimeOffset scheduledDeparture;
        private DateTimeOffset scheduledArrival;
        private DateTimeOffset? estimatedDeparture = null;
        private DateTimeOffset? estimatedArrival = null;
        private DateTimeOffset? actualDeparture = null;
        private DateTimeOffset? actualArrival = null;

        public Flight(int id, DateOnly date, int number, String origin, String destination, char status, DateTimeOffset scheduledDeparture, DateTimeOffset scheduledArrival) {
            this.id = id;
            this.date = date;
            if (number >= 1 && number <= 9999)
                this.number = number;
            else
                throw new ArgumentException("flight number " + number + " must be in [1, 9999]");
            if (origin.Length == 3 && origin.ToUpper().All(c => c >= 'A' && c <= 'Z'))
                this.origin = origin;
            else
                throw new ArgumentException("origin airport " + origin + " must be 3 character alphabetic");
            if (destination.Length == 3 && destination.ToUpper().All(c => c >= 'A' && c <= 'Z'))
                this.destination = destination;
            else
                throw new ArgumentException("destination airport " + destination + " must be 3 character alphabetic");
            try {
                this.status = (Status)status;
            }
            catch (Exception e) {
                throw new ArgumentException("status " + status + " must be a valid status code [S, T, F, N, I]");
            }
            this.scheduledDeparture = scheduledDeparture;
            this.scheduledArrival = scheduledArrival;
        }

        public int getId() { return id; }

        public DateOnly getDate() { return date; }

        public int getNumber() { return number; }

        public String getOrigin() { return origin; }

        public String getDestination() { return destination; }

        public Status getStatus() { return status; }

        public DateTimeOffset getScheduledDeparture() { return scheduledDeparture; }
        public DateTimeOffset getScheduledArrival() { return scheduledArrival; }
        public DateTimeOffset? getEstimatedDeparture() { return estimatedDeparture; }
        public DateTimeOffset? getEstimatedArrival() { return estimatedArrival; }
        public DateTimeOffset? getActualArrival() { return actualArrival; }
        public DateTimeOffset? getActualDeparture() { return actualDeparture; }

        public DateTimeOffset getDeparture() {
            if (actualDeparture != null)
                return actualDeparture.Value;
            else if (estimatedDeparture != null)
                return estimatedDeparture.Value;
            return scheduledDeparture;
        }

        public DateTimeOffset getArrival()
        {
            if (actualArrival != null)
                return actualArrival.Value;
            else if (estimatedArrival != null)
                return estimatedArrival.Value;
            return scheduledArrival;
        }

        public void setStatus(Status status) {
            this.status = status;
            if (status == Status.Out)
                actualDeparture = DateTimeOffset.Now;
            else if (status == Status.In)
                actualArrival = DateTimeOffset.Now;
        }
    }

    public enum Status { 
        Scheduled = 'S',
        Out = 'T',
        Off = 'F',
        On = 'N',
        In = 'I',
        Cancelled = 'C'
    }
}
