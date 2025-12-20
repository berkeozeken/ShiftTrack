using Microsoft.AspNetCore.Mvc;
using ShiftTrack.Web.Models;
using ShiftTrack.Web.Services;

namespace ShiftTrack.Web.Controllers;

public class ShiftDefinitionsController : Controller
{
    private readonly ShiftTrackApiClient _api;

    public ShiftDefinitionsController(ShiftTrackApiClient api)
    {
        _api = api;
    }

    public async Task<IActionResult> Index()
    {
        var items = await _api.GetAllAsync();
        return View(items);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new ShiftDefinitionDto());
    }

    [HttpPost]
    public async Task<IActionResult> Create(ShiftDefinitionDto model)
    {
        if (!ModelState.IsValid) return View(model);

        await _api.CreateAsync(model);
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var item = await _api.GetByIdAsync(id);
        if (item is null) return NotFound();

        return View(item);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(int id, ShiftDefinitionDto model)
    {
        if (!ModelState.IsValid) return View(model);

        await _api.UpdateAsync(id, model);
        return RedirectToAction(nameof(Index));
    }

    // AJAX delete endpoint
    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        await _api.DeleteAsync(id);
        return Ok(new { success = true });
    }
}
