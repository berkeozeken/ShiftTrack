using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShiftTrack.Api.Data;
using ShiftTrack.Api.Models;

namespace ShiftTrack.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ShiftDefinitionsController : ControllerBase
{
    private readonly AppDbContext _db;

    public ShiftDefinitionsController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<List<ShiftDefinition>>> GetAll()
    {
        var items = await _db.ShiftDefinitions
            .AsNoTracking()
            .OrderBy(x => x.Id)
            .ToListAsync();

        return Ok(items);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ShiftDefinition>> GetById(int id)
    {
        var item = await _db.ShiftDefinitions
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);

        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost]
    public async Task<ActionResult<ShiftDefinition>> Create(ShiftDefinition model)
    {
        _db.ShiftDefinitions.Add(model);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = model.Id }, model);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, ShiftDefinition model)
    {
        var item = await _db.ShiftDefinitions.FirstOrDefaultAsync(x => x.Id == id);
        if (item is null) return NotFound();

        item.StartTime = model.StartTime;
        item.EndTime = model.EndTime;
        item.BreakStartTime = model.BreakStartTime;
        item.BreakEndTime = model.BreakEndTime;
        item.IsOvertimeEligible = model.IsOvertimeEligible;

        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var item = await _db.ShiftDefinitions.FirstOrDefaultAsync(x => x.Id == id);
        if (item is null) return NotFound();

        _db.ShiftDefinitions.Remove(item);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}
