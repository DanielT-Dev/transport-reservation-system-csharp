namespace MyClientApp.Models;

public class SeatDTO
{
    public int SeatNumber { get; set; }
    public string ClientName { get; set; } = "";

    public SeatDTO(int seatNumber, string clientName)
    {
        SeatNumber = seatNumber;
        ClientName = clientName;
    }
}