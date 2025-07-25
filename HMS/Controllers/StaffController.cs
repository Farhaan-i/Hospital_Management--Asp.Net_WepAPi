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
        public async Task<IActionResult> RegisterStaff([FromBody] Staff staff)
        {
            if (staff == null || string.IsNullOrEmpty(staff.StaffEmail))
                return BadRequest("Invalid staff data.");

            var inserted = await _context.Staffs
                .FromSqlInterpolated($@"
                    EXEC usp_RegisterStaff 
                        @Name = {staff.StaffName}, 
                        @Role = {staff.StaffRole}, 
                        @Email = {staff.StaffEmail}, 
                        @Phone = {staff.StaffPhoneNumber}")
                .ToListAsync();

            var savedStaff = inserted.FirstOrDefault();

            return savedStaff == null
                ? StatusCode(500, "Staff registration failed.")
                : Ok(new { message = "Staff registered successfully!", Staff = savedStaff });
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllStaff()
        {
            var staffList = await _context.Staffs
                .FromSqlInterpolated($"EXEC usp_GetAllStaff")
                .ToListAsync();

            return Ok(staffList);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetStaffById(int id)
        {
            var staffList = await _context.Staffs
                .FromSqlInterpolated($"EXEC usp_GetStaffById @StaffId = {id}")
                .ToListAsync();

            var staff = staffList.FirstOrDefault();

            return staff == null
                ? NotFound(new { Message = $"No staff found with ID {id}" })
                : Ok(staff);
        }

        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateStaff(int id, [FromBody] Staff staff)
        {
            if (id != staff.StaffId)
                return BadRequest(new { Message = "Staff ID mismatch" });

            await _context.Database.ExecuteSqlInterpolatedAsync($@"
                EXEC usp_UpdateStaff 
                    @StaffId = {staff.StaffId}, 
                    @Name = {staff.StaffName}, 
                    @Role = {staff.StaffRole}, 
                    @Email = {staff.StaffEmail}, 
                    @Phone = {staff.StaffPhoneNumber}");

            return Ok(new { Message = "Staff updated successfully.", Staff = staff });
        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteStaff(int id)
        {
            await _context.Database.ExecuteSqlInterpolatedAsync(
                $"EXEC usp_DeleteStaff @StaffId = {id}");

            return Ok(new { Message = "Staff deleted successfully." });
        }
    }
}
