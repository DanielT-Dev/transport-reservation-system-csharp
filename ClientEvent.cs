namespace Client;

public static class ClientEvents
{
    public static event Action? ReservationsChanged;
    public static event Action? RacesChanged;

    public static void NotifyReservationsChanged()
        => ReservationsChanged?.Invoke();

    public static void NotifyRacesChanged()
        => RacesChanged?.Invoke();
}