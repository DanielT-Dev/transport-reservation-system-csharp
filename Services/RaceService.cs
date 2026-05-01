using MyClientApp.Models;

namespace MyClientApp.Services;

public class RaceService
{
    private readonly ApiClient _api;

    public RaceService(ApiClient api)
    {
        _api = api;
    }

    public Task<List<Race>> FindAllAsync()
        => _api.GetListAsync<Race>("api/races");

    public Task<Race?> GetRaceByIdAsync(int id)
        => _api.GetAsync<Race>($"api/races/{id}");

    public Task SaveRaceAsync(Race race)
        => _api.PostAsync("api/races", race);

    public Task UpdateRaceAsync(Race race)
        => _api.PutAsync($"api/races/{race.Id}", race);

    public Task DeleteRaceAsync(int id)
        => _api.DeleteAsync($"api/races/{id}");

    public Task<List<Race>> GetRacesPageAsync(int pageNumber)
        => _api.GetListAsync<Race>($"api/races/page/{pageNumber}");

    public async Task<int> GetTotalRacesAsync()
    {
        return await _api.GetAsync<int>("api/races/total");
    }

    public async Task<Race?> FindByDetailsAsync(string dest, string date, string time)
    {
        var races = await FindAllAsync();

        return races.FirstOrDefault(r =>
            string.Equals(r.Destination, dest, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(r.Date, date, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(r.Time, time, StringComparison.OrdinalIgnoreCase));
    }
}