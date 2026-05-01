using System.Net.Http.Json;
using System.Text.Json;

namespace MyClientApp.Services;

public class ApiClient
{
    private readonly HttpClient _http;

    public ApiClient(string baseUrl)
    {
        _http = new HttpClient
        {
            BaseAddress = new Uri(baseUrl)
        };
    }

    public async Task<List<T>> GetListAsync<T>(string url)
    {
        var result = await _http.GetFromJsonAsync<List<T>>(url);
        return result ?? new List<T>();
    }

    public async Task<T?> GetAsync<T>(string url)
    {
        return await _http.GetFromJsonAsync<T>(url);
    }

    public async Task PostAsync<T>(string url, T data)
    {
        var response = await _http.PostAsJsonAsync(url, data);
        response.EnsureSuccessStatusCode();
    }

    public async Task PutAsync<T>(string url, T data)
    {
        var response = await _http.PutAsJsonAsync(url, data);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteAsync(string url)
    {
        var response = await _http.DeleteAsync(url);
        response.EnsureSuccessStatusCode();
    }

    public async Task<T?> PostAsyncAndReturn<T>(string url, object data)
    {
        var response = await _http.PostAsJsonAsync(url, data);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<T>();
    }
}