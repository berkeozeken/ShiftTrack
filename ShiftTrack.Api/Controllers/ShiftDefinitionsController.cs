using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShiftTrack.Api.Dtos;
using ShiftTrack.Api.Helpers;
using ShiftTrack.Api.Models;

namespace ShiftTrack.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ShiftDefinitionsController : ControllerBase
    {
        private readonly ShiftTrackDbContext _db;

        public ShiftDefinitionsController(ShiftTrackDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<ActionResult<List<ShiftDefinitionResponseDto>>> GetAll()
        {
            var items = await _db.ShiftDefinitions
                .AsNoTracking()
                .OrderBy(x => x.Id)
                .Select(x => new ShiftDefinitionResponseDto
                {
                    Id = x.Id,
                    StartTime = TimeParser.ToHm(x.StartTime),
                    EndTime = TimeParser.ToHm(x.EndTime),
                    BreakStartTime = TimeParser.ToHm(x.BreakStartTime),
                    BreakEndTime = TimeParser.ToHm(x.BreakEndTime),
                    IsOvertimeEligible = x.IsOvertimeEligible
                })
                .ToListAsync();

            return Ok(items);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<ShiftDefinitionResponseDto>> GetById(int id)
        {
            var entity = await _db.ShiftDefinitions.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
            if (entity == null) return NotFound();

            return Ok(new ShiftDefinitionResponseDto
            {
                Id = entity.Id,
                StartTime = TimeParser.ToHm(entity.StartTime),
                EndTime = TimeParser.ToHm(entity.EndTime),
                BreakStartTime = TimeParser.ToHm(entity.BreakStartTime),
                BreakEndTime = TimeParser.ToHm(entity.BreakEndTime),
                IsOvertimeEligible = entity.IsOvertimeEligible
            });
        }

        [HttpPost]
        public async Task<ActionResult<ShiftDefinitionResponseDto>> Create([FromBody] ShiftDefinitionCreateUpdateDto dto)
        {
            if (!TryValidateTimes(dto, out var start, out var end, out var bStart, out var bEnd, out var error))
                return BadRequest(new { message = error });

            if (!ValidateBreakWindow(start, end, bStart, bEnd, out error))
                return BadRequest(new { message = error });

            var entity = new ShiftDefinition
            {
                StartTime = start,
                EndTime = end,
                BreakStartTime = bStart,
                BreakEndTime = bEnd,
                IsOvertimeEligible = dto.IsOvertimeEligible
            };

            _db.ShiftDefinitions.Add(entity);
            await _db.SaveChangesAsync();

            var response = new ShiftDefinitionResponseDto
            {
                Id = entity.Id,
                StartTime = TimeParser.ToHm(entity.StartTime),
                EndTime = TimeParser.ToHm(entity.EndTime),
                BreakStartTime = TimeParser.ToHm(entity.BreakStartTime),
                BreakEndTime = TimeParser.ToHm(entity.BreakEndTime),
                IsOvertimeEligible = entity.IsOvertimeEligible
            };

            return CreatedAtAction(nameof(GetById), new { id = entity.Id }, response);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] ShiftDefinitionCreateUpdateDto dto)
        {
            var entity = await _db.ShiftDefinitions.FirstOrDefaultAsync(x => x.Id == id);
            if (entity == null) return NotFound();

            if (!TryValidateTimes(dto, out var start, out var end, out var bStart, out var bEnd, out var error))
                return BadRequest(new { message = error });

            if (!ValidateBreakWindow(start, end, bStart, bEnd, out error))
                return BadRequest(new { message = error });

            entity.StartTime = start;
            entity.EndTime = end;
            entity.BreakStartTime = bStart;
            entity.BreakEndTime = bEnd;
            entity.IsOvertimeEligible = dto.IsOvertimeEligible;

            await _db.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var entity = await _db.ShiftDefinitions.FirstOrDefaultAsync(x => x.Id == id);
            if (entity == null) return NotFound();

            _db.ShiftDefinitions.Remove(entity);
            await _db.SaveChangesAsync();
            return NoContent();
        }

        private static bool TryValidateTimes(
            ShiftDefinitionCreateUpdateDto dto,
            out TimeSpan start,
            out TimeSpan end,
            out TimeSpan bStart,
            out TimeSpan bEnd,
            out string error)
        {
            error = string.Empty;
            start = end = bStart = bEnd = default;

            if (!TimeParser.TryParseHm(dto.StartTime, out start))
            {
                error = "StartTime must be in HH:mm format.";
                return false;
            }

            if (!TimeParser.TryParseHm(dto.EndTime, out end))
            {
                error = "EndTime must be in HH:mm format.";
                return false;
            }

            if (!TimeParser.TryParseHm(dto.BreakStartTime, out bStart))
            {
                error = "BreakStartTime must be in HH:mm format.";
                return false;
            }

            if (!TimeParser.TryParseHm(dto.BreakEndTime, out bEnd))
            {
                error = "BreakEndTime must be in HH:mm format.";
                return false;
            }

            return true;
        }

        private static bool ValidateBreakWindow(
            TimeSpan shiftStart,
            TimeSpan shiftEnd,
            TimeSpan breakStart,
            TimeSpan breakEnd,
            out string error)
        {
            error = string.Empty;

            // Ayný gün vardiyasý: End >= Start
            var isOvernight = shiftEnd < shiftStart;

            // Mola süresi pozitif olmalý (mola da gece taþabilir ama burada basit tutuyoruz:
            // Task örnekleri ayný gün molasý. Gece vardiyasýnda da mola kabul edilsin diye
            // sadece breakStart != breakEnd kontrolü yapýyoruz.)
            if (breakStart == breakEnd)
            {
                error = "Break start and end cannot be the same.";
                return false;
            }

            // Ayný gün vardiyasý için daha net kontrol:
            if (!isOvernight)
            {
                if (shiftStart == shiftEnd)
                {
                    error = "Shift start and end cannot be the same.";
                    return false;
                }

                // Molanýn vardiya aralýðýnda olmasý beklenir (Task kapsamý)
                if (breakStart < shiftStart || breakStart > shiftEnd || breakEnd < shiftStart || breakEnd > shiftEnd)
                {
                    error = "Break times must be within the shift window.";
                    return false;
                }

                if (breakEnd <= breakStart)
                {
                    error = "BreakEndTime must be after BreakStartTime for same-day shifts.";
                    return false;
                }
            }
            else
            {
                // Gece vardiyasý: 16:00-00:00 gibi end < start olabilir.
                // Burada aþýrý kýsýt koymuyoruz; sadece shiftStart==shiftEnd engeli kalsýn.
                if (shiftStart == shiftEnd)
                {
                    error = "Shift start and end cannot be the same.";
                    return false;
                }
            }

            return true;
        }
    }
}
