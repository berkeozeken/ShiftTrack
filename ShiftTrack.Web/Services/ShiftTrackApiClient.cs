using System.Net.Http.Json;
using ShiftTrack.Web.ViewModels;

namespace ShiftTrack.Web.Services
{
    public class ShiftTrackApiClient
    {
        private readonly HttpClient _http;

        public ShiftTrackApiClient(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<ShiftDefinitionViewModel>> GetShiftDefinitionsAsync()
        {
            var result = await _http.GetFromJsonAsync<List<ShiftDefinitionViewModel>>("/api/ShiftDefinitions");
            return result ?? new List<ShiftDefinitionViewModel>();
        }

        public async Task CreateShiftDefinitionAsync(ShiftDefinitionFormViewModel form)
        {
            var dto = new
            {
                startTime = form.StartTime,
                endTime = form.EndTime,
                breakStartTime = form.BreakStartTime,
                breakEndTime = form.BreakEndTime,
                isOvertimeEligible = form.IsOvertimeEligible
            };

            var resp = await _http.PostAsJsonAsync("/api/ShiftDefinitions", dto);
            resp.EnsureSuccessStatusCode();
        }

        public async Task UpdateShiftDefinitionAsync(int id, ShiftDefinitionFormViewModel form)
        {
            var dto = new
            {
                startTime = form.StartTime,
                endTime = form.EndTime,
                breakStartTime = form.BreakStartTime,
                breakEndTime = form.BreakEndTime,
                isOvertimeEligible = form.IsOvertimeEligible
            };

            var resp = await _http.PutAsJsonAsync($"/api/ShiftDefinitions/{id}", dto);
            resp.EnsureSuccessStatusCode();
        }

        public async Task DeleteShiftDefinitionAsync(int id)
        {
            var resp = await _http.DeleteAsync($"/api/ShiftDefinitions/{id}");
            resp.EnsureSuccessStatusCode();
        }
    }
}
