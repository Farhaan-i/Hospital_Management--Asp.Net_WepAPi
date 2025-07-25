using HMS.Models;
using HMS.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HMS.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SlotController : ControllerBase
    {
        private readonly AppDbContext _context;

        public SlotController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("GenerateSlots")]
        public async Task<IActionResult> GenerateSlots([FromBody] slotDto dto)
        {
            try
            {
                if (dto == null || string.IsNullOrEmpty(dto.StartDate) || string.IsNullOrEmpty(dto.EndDate) || dto.DoctorId == 0)
                    return BadRequest("Invalid slot data.");

                DateTime startDate = DateTime.Parse(dto.StartDate);
                DateTime endDate = DateTime.Parse(dto.EndDate);
                int slotDuration = Convert.ToInt32(dto.SlotDuration);
                TimeSpan startTime = TimeSpan.Parse(dto.DailyStartTime);
                TimeSpan endTime = TimeSpan.Parse(dto.DailyEndTime);

                await _context.Database.ExecuteSqlInterpolatedAsync(
                    $"EXEC usp_GenerateSlots @StartDate = {startDate}, @EndDate = {endDate}, @SlotDuration = {slotDuration}, @DailyStartTime = {startTime}, @DailyEndTime = {endTime}, @DoctorId = {dto.DoctorId}");

                return Ok("Slots generated successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


        [HttpDelete("delete-before-today/{doctorId}")]
        public async Task<IActionResult> DeleteSlotsBeforeToday(int doctorId)
        {
            await _context.Database.ExecuteSqlInterpolatedAsync(
                $"EXEC usp_DeleteSlotsBeforeToday @DoctorId = {doctorId}");

            return Ok("Old slots deleted successfully.");
        }


        [HttpGet("UnbookedSlots/{doctorId}")]
        public async Task<IActionResult> GetUnbookedSlotsByDoctor(int doctorId)
        {
            var slots = await _context.Slots
                .FromSqlInterpolated($"EXEC usp_GetUnbookedSlotsByDoctor @DoctorId = {doctorId}")
                .ToListAsync();

            return slots.Any() ? Ok(slots) : NotFound("No unbooked slots found.");
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> GetSlotById(int id)
        {
            var slot = _context.Slots
            .FromSqlInterpolated($"EXEC usp_GetSlotById @SlotId = {id}")
            .AsEnumerable()
            .FirstOrDefault(); // ✅ Works fine

            return slot == null ? NotFound($"No slot found with ID {id}.") : Ok(slot);

        }


        [HttpPut("update")]
        public async Task<IActionResult> UpdateSlot([FromBody] Slot slot)
        {
            await _context.Database.ExecuteSqlInterpolatedAsync(
                $"EXEC usp_UpdateSlot @SlotId = {slot.SlotId}, @SlotDate = {slot.SlotDate}, @StartTime = {slot.StartTime}, @EndTime = {slot.EndTime}, @IsBooked = {slot.IsBooked}, @DoctorId = {slot.DoctorId}");

            return Ok("Slot updated successfully.");
        }


        [HttpGet("available")]
        public async Task<IActionResult> GetUnbookedSlotsByDoctorAndDate([FromQuery] int doctorId, [FromQuery] DateTime date)
        {
            var slots = await _context.Slots
                .FromSqlInterpolated($"EXEC usp_GetUnbookedSlotsByDoctorAndDate @DoctorId = {doctorId}, @SlotDate = {date}")
                .ToListAsync();

            return slots.Any()
                ? Ok(slots)
                : NotFound($"No available slots found for Doctor ID {doctorId} on {date:yyyy-MM-dd}.");
        }

    }
}
