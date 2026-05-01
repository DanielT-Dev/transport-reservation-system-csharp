namespace MyClientApp.Models;

public class Reservation : IIdentifiable<int>
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int RaceId { get; set; }
    public string Name { get; set; } = "";
    public List<int> Seats { get; set; } = new();

    public Reservation() { }

    public Reservation(int id, int userId, int raceId, string name, List<int> seats)
    {
        Id = id;
        UserId = userId;
        RaceId = raceId;
        Name = name;
        Seats = seats;
    }

    public Reservation(int userId, int raceId, string name, List<int> seats)
    {
        UserId = userId;
        RaceId = raceId;
        Name = name;
        Seats = seats;
    }

    public override string ToString()
    {
        return $"{Id} {Name} {UserId} {RaceId} {string.Join(",", Seats)}";
    }
}