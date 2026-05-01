namespace MyClientApp.Models;

public class Race : IIdentifiable<int>
{
    public int Id { get; set; }
    public string Destination { get; set; } = "";
    public string Date { get; set; } = "";
    public string Time { get; set; } = "";
    public List<bool> AvailableSeats { get; set; } = new();

    public Race() { }

    public Race(int id, string destination, string date, string time, List<bool> availableSeats)
    {
        Id = id;
        Destination = destination;
        Date = date;
        Time = time;
        AvailableSeats = availableSeats;
    }

    public Race(string destination, string date, string time, List<bool> availableSeats)
    {
        Destination = destination;
        Date = date;
        Time = time;
        AvailableSeats = availableSeats;
    }

    public override string ToString()
    {
        return $"{Id} {Destination} {Date} {Time} {string.Join(",", AvailableSeats)}";
    }
}