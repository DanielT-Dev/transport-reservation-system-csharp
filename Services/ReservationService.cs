using MyClientApp.Models;

namespace MyClientApp.Services;

public class ReservationService
{
    private readonly ApiClient _api;

    public ReservationService(ApiClient api)
    {
        _api = api;
    }

    public Task<List<Reservation>> FindAllAsync()
        => _api.GetListAsync<Reservation>("api/reservations");

    public Task<Reservation?> GetReservationByIdAsync(int id)
        => _api.GetAsync<Reservation>($"api/reservations/{id}");

    public Task SaveReservationAsync(Reservation reservation)
        => _api.PostAsync("api/reservations", reservation);

    public Task UpdateReservationAsync(Reservation reservation)
        => _api.PutAsync($"api/reservations/{reservation.Id}", reservation);

    public Task DeleteReservationAsync(int id)
        => _api.DeleteAsync($"api/reservations/{id}");

    public Task<List<Reservation>> GetReservationsPageAsync(int page)
        => _api.GetListAsync<Reservation>($"api/reservations/page/{page}");

    public Task<int> GetTotalReservationsAsync()
        => _api.GetAsync<int>("api/reservations/total");

    public async Task<List<SeatDTO>> GetSeatsForRaceAsync(int raceId)
    {
        var reservedSeats = await _api.GetAsync<Dictionary<int, string>>($"api/reservations/{raceId}/seats")
                           ?? new Dictionary<int, string>();

        var result = new List<SeatDTO>();
        for (int i = 1; i <= 18; i++)
        {
            var name = reservedSeats.ContainsKey(i) ? reservedSeats[i] : "-";
            result.Add(new SeatDTO(i, name));
        }

        return result;
    }

    public async Task<List<int>> GetFreeSeatsAsync(int raceId)
    {
        var reservedSeats = await _api.GetAsync<Dictionary<int, string>>($"api/reservations/{raceId}/seats")
                           ?? new Dictionary<int, string>();

        var free = new List<int>();
        for (int i = 1; i <= 18; i++)
        {
            if (!reservedSeats.ContainsKey(i))
                free.Add(i);
        }

        return free;
    }

    public Task CreateReservationAsync(int raceId, string name, List<int> seats)
    {
        var reservation = new Reservation(0, 1, raceId, name, seats);
        return SaveReservationAsync(reservation);
    }
}