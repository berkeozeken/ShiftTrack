using System.Net.Http.Json;
using ShiftTrack.Web.Models;

namespace ShiftTrack.Web.Services;

public class ShiftTrackApiClient
{
    private readonly HttpClient _http;

    public ShiftTrackApiClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<ShiftDefinitionDto>> GetAllAsync()
        => await _http.GetFromJsonAsync<List<ShiftDefinitionDto>>("api/ShiftDefinitions")
           ?? new List<ShiftDefinitionDto>();

    public async Task<ShiftDefinitionDto?> GetByIdAsync(int id)
        => await _http.GetFromJsonAsync<ShiftDefinitionDto>($"api/ShiftDefinitions/{id}");

    public async Task CreateAsync(ShiftDefinitionDto dto)
    {
        var res = await _http.PostAsJsonAsync("api/ShiftDefinitions", dto);
        res.EnsureSuccessStatusCode();
    }

    public async Task UpdateAsync(int id, ShiftDefinitionDto dto)
    {
        var res = await _http.PutAsJsonAsync($"api/ShiftDefinitions/{id}", dto);
        res.EnsureSuccessStatusCode();
    }

    public async Task DeleteAsync(int id)
    {
        var res = await _http.DeleteAsync($"api/ShiftDefinitions/{id}");
        res.EnsureSuccessStatusCode();
    }
}
