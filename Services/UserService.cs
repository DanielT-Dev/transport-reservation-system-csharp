using MyClientApp.Models;

namespace MyClientApp.Services;

public class UserService
{
    private readonly ApiClient _api;

    public UserService(ApiClient api)
    {
        _api = api;
    }

    public Task<List<User>> GetAllUsersAsync()
        => _api.GetListAsync<User>("api/users");

    public Task<User?> GetUserByIdAsync(int id)
        => _api.GetAsync<User>($"api/users/{id}");

    public Task CreateUserAsync(User user)
        => _api.PostAsync("api/users", user);

    public Task UpdateUserAsync(User user)
        => _api.PutAsync($"api/users/{user.Id}", user);

    public Task DeleteUserAsync(int id)
        => _api.DeleteAsync($"api/users/{id}");

    public Task<List<User>> GetUserPageAsync(int pageNumber)
        => _api.GetListAsync<User>($"api/users/page/{pageNumber}");

    public Task<int> GetTotalUsersAsync()
        => _api.GetAsync<int>("api/users/total");

    public Task<User?> LoginAsync(string email, string password)
    {
        var payload = new { email, password };
        return _api.PostAsyncAndReturn<User>("api/users/login", payload);
    }
}