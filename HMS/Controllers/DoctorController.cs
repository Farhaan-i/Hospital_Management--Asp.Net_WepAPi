using HMS.Dto;
using HMS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace HMS.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DoctorController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DoctorController(AppDbContext context)
        {
            _context = context;
        }

        // 1. Get All Doctors
        [HttpGet("doctors")]
        public async Task<ActionResult<List<Doctor>>> GetAllDoctors()
        {
            var doctors = await _context.Doctors
                .FromSqlRaw("EXEC GetAllDoctors")
                .ToListAsync();

            return Ok(doctors);
        }

        // 2. Get Doctor by ID
        [HttpGet("doctors/{id}")]
        public async Task<ActionResult<Doctor>> GetDoctorById(int id)
        {

            var doctor = _context.Doctors
                .FromSqlRaw("EXEC GetDoctorById @DoctorId", new SqlParameter("@DoctorId", id))
                .AsEnumerable() // Materialize the result
                .FirstOrDefault();


            if (doctor == null)
                return NotFound($"No doctor found with ID {id}");

            return Ok(doctor);
        }

        // 3. Add Doctor
        [HttpPost("doctors")]
        public async Task<ActionResult> AddDoctor([FromBody] DoctorDto doctorDto)
        {
            var parameters = new[]
            {
                new SqlParameter("@DoctorName", doctorDto.DoctorName),
                new SqlParameter("@Specialization", doctorDto.Specialization),
                new SqlParameter("@DoctorEmail", doctorDto.DoctorEmail ?? (object)DBNull.Value),
                new SqlParameter("@DoctorContactNumber", doctorDto.DoctorContactNumber ?? (object)DBNull.Value)
            };

            await _context.Database.ExecuteSqlRawAsync("EXEC AddDoctor @DoctorName, @Specialization, @DoctorEmail, @DoctorContactNumber", parameters);

            return Ok("Doctor added successfully.");
        }

        // 4. Update Doctor
        [HttpPut("doctors/{id}")]
        public async Task<ActionResult> UpdateDoctor(int id, [FromBody] DoctorDto doctorDto)
        {
            var parameters = new[]
            {
                new SqlParameter("@DoctorId", id),
                new SqlParameter("@DoctorName", doctorDto.DoctorName),
                new SqlParameter("@Specialization", doctorDto.Specialization),
                new SqlParameter("@DoctorEmail", doctorDto.DoctorEmail ?? (object)DBNull.Value),
                new SqlParameter("@DoctorContactNumber", doctorDto.DoctorContactNumber ?? (object)DBNull.Value)
            };

            await _context.Database.ExecuteSqlRawAsync("EXEC UpdateDoctor @DoctorId, @DoctorName, @Specialization, @DoctorEmail, @DoctorContactNumber", parameters);

            return NoContent();
        }

        // 5. Delete Doctor
        [HttpDelete("doctors/{id}")]
        public async Task<ActionResult> DeleteDoctor(int id)
        {
            try
            {
                await _context.Database.ExecuteSqlRawAsync("EXEC DeleteDoctor @DoctorId", new SqlParameter("@DoctorId", id));

                // Return a 200 OK with a confirmation message
                return Ok($"Doctor with ID {id} has been deleted successfully.");
            }
            catch (Exception ex)
            {
                // Optional: handle errors gracefully
                return StatusCode(500, $"Error deleting doctor: {ex.Message}");
            }
        }


        // 6. Cancel Appointments by Date
        [HttpPut("cancel/{doctorId}/{date}")]
        public async Task<IActionResult> CancelAppointments(int doctorId, DateTime date)
        {
            var parameters = new[]
            {
                new SqlParameter("@DoctorId", doctorId),
                new SqlParameter("@AppointmentDate", date)
            };

            await _context.Database.ExecuteSqlRawAsync("EXEC CancelAppointmentsByDate @DoctorId, @AppointmentDate", parameters);

            return Ok($"Appointments for doctor ID {doctorId} on {date:yyyy-MM-dd} have been marked as canceled.");
        }
    }
}