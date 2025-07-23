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
        public IActionResult GenerateSlots([FromBody] slotDto dto)
        {
            try
            {
                if (dto == null || string.IsNullOrEmpty(dto.StartDate) || string.IsNullOrEmpty(dto.EndDate) || dto.DoctorId == 0)
                {
                    return BadRequest("Invalid slot data.");
                }

                // Validate Doctor existence
                var doctorExists = _context.Doctors.Any(d => d.DoctorId == dto.DoctorId);
                if (!doctorExists)
                {
                    return BadRequest($"Doctor with ID {dto.DoctorId} does not exist.");
                }

                DateTime startDate = DateTime.Parse(dto.StartDate);
                DateTime endDate = DateTime.Parse(dto.EndDate);
                TimeSpan slotDuration = TimeSpan.FromMinutes(Convert.ToDouble(dto.SlotDuration));
                TimeSpan dailyStartTime = TimeSpan.Parse(dto.DailyStartTime);
                TimeSpan dailyEndTime = TimeSpan.Parse(dto.DailyEndTime);

                if (dailyStartTime >= dailyEndTime)
                {
                    return BadRequest("Daily start time must be before end time.");
                }

                List<Slot> slots = new();

                for (var date = startDate.Date; date <= endDate.Date; date = date.AddDays(1))
                {
                    for (var time = dailyStartTime; time < dailyEndTime; time = time.Add(slotDuration))
                    {
                        var slot = new Slot
                        {
                            SlotDate = date,
                            StartTime = time,
                            EndTime = time.Add(slotDuration),
                            DoctorId = dto.DoctorId,
                            IsBooked = false
                        };

                        slots.Add(slot);
                        _context.Slots.Add(slot);
                    }
                }

                _context.SaveChanges();
                return Ok(new { message = "Slots generated successfully!", slots });
            }
            catch (FormatException ex)
            {
                return BadRequest(new { message = "Invalid date or time format.", details = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error.", details = ex.Message });
            }
        }

        [HttpDelete("delete-before-today/{doctorId}")]
        public IActionResult DeleteSlotsBeforeToday(int doctorId)
        {
            try
            {
                var today = DateTime.Today;
                var slotsToDelete = _context.Slots
                    .Where(s => s.DoctorId == doctorId && s.SlotDate < today)
                    .ToList();

                if (slotsToDelete.Any())
                {
                    _context.Slots.RemoveRange(slotsToDelete);
                    _context.SaveChanges();
                }

                return Ok("Old slots deleted successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Error deleting slots.", details = ex.Message });
            }
        }

        [HttpGet("UnbookedSlots/{doctorId}")]
        public ActionResult<List<Slot>> GetUnbookedSlotsByDoctor(int doctorId)
        {
            try
            {
                var slots = _context.Slots
                    .Where(s => s.DoctorId == doctorId && !s.IsBooked)
                    .ToList();

                if (!slots.Any())
                {
                    return NotFound("No unbooked slots found.");
                }

                return Ok(slots);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("{id}")]
        public IActionResult GetSlotById(int id)
        {
            try
            {
                var slot = _context.Slots.FirstOrDefault(s => s.SlotId == id);
                if (slot == null)
                {
                    return NotFound($"No slot found with ID {id}.");
                }

                return Ok(slot);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Error retrieving slot.", details = ex.Message });
            }
        }

        [HttpPut("update")]
        public IActionResult UpdateSlot([FromBody] Slot slot)
        {
            try
            {
                _context.Slots.Update(slot);
                _context.SaveChanges();
                return Ok("Slot updated successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Error updating slot.", details = ex.Message });
            }
        }

        [HttpGet("available")]
        public ActionResult<List<Slot>> GetUnbookedSlotsByDoctorAndDate([FromQuery] int doctorId, [FromQuery] DateTime date)
        {
            var slots = _context.Slots
                .Where(s => s.DoctorId == doctorId && s.SlotDate.Date == date.Date && !s.IsBooked)
                .ToList();

            if (!slots.Any())
            {
                return NotFound($"No available slots found for Doctor ID {doctorId} on {date:yyyy-MM-dd}.");
            }

            return Ok(slots);
        }
    }
}
