using HMS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HMS.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StaffController : ControllerBase
    {
        private readonly AppDbContext _context;

        public StaffController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("register-staff")]
        public IActionResult RegisterStaff([FromBody] Staff staff)
        {
            try
            {
                if (staff == null || string.IsNullOrEmpty(staff.StaffEmail))
                {
                    return BadRequest("Invalid staff data.");
                }

                _context.Staffs.Add(staff);
                _context.SaveChanges();

                // Optional: Add user registration logic here if needed

                return Ok(new { message = "Staff registered successfully!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error registering staff: {ex.Message}");
            }
        }

        [HttpGet("all")]
        public IActionResult GetAllStaff()
        {
            try
            {
                var staffList = _context.Staffs.ToList();
                return Ok(staffList);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public IActionResult GetStaffById(int id)
        {
            try
            {
                var staff = _context.Staffs.FirstOrDefault(s => s.StaffId == id);
                if (staff == null)
                {
                    return NotFound(new { Message = $"No staff found with ID {id}" });
                }
                return Ok(staff);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        [HttpPut("update/{id}")]
        public IActionResult UpdateStaff(int id, [FromBody] Staff staff)
        {
            try
            {
                if (id != staff.StaffId)
                {
                    return BadRequest(new { Message = "Staff ID mismatch" });
                }

                var existingStaff = _context.Staffs.FirstOrDefault(s => s.StaffId == id);
                if (existingStaff == null)
                {
                    return NotFound(new { Message = $"No staff found with ID {id}" });
                }

                existingStaff.StaffName = staff.StaffName;
                existingStaff.StaffRole = staff.StaffRole;
                existingStaff.StaffEmail = staff.StaffEmail;
                existingStaff.StaffPhoneNumber = staff.StaffPhoneNumber;

                _context.SaveChanges();
                return Ok(new { Message = "Staff updated successfully.", Staff = existingStaff });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        [HttpDelete("delete/{id}")]
        public IActionResult DeleteStaff(int id)
        {
            try
            {
                var staff = _context.Staffs.FirstOrDefault(s => s.StaffId == id);
                if (staff == null)
                {
                    return NotFound(new { Message = $"No staff found with ID {id}" });
                }

                _context.Staffs.Remove(staff);
                _context.SaveChanges();
                return Ok(new { Message = "Staff deleted successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Error = ex.Message });
            }
        }
    }
}
