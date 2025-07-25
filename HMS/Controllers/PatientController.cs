using HMS.Models;
using HMS.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HMS.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PatientController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PatientController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("patients")]
        public async Task<IActionResult> GetAllPatients()
        {
            var patients = await _context.Patients
                .FromSqlInterpolated($"EXEC usp_GetAllPatients")
                .ToListAsync();

            return Ok(patients);
        }

        [HttpGet("patients/{id}")]
        public async Task<IActionResult> GetPatientById(int id)
        {
            var patientList = await _context.Patients
                .FromSqlInterpolated($"EXEC usp_GetPatientById @PatientId = {id}")
                .ToListAsync();

            var patient = patientList.FirstOrDefault();

            return patient == null
                ? NotFound($"No patient found with ID {id}")
                : Ok(patient);
        }

        [HttpPost("patients")]
        public async Task<IActionResult> AddPatient([FromBody] PatientDto dto)
        {
            var inserted = await _context.Patients
                .FromSqlInterpolated($@"
                    EXEC usp_AddPatient 
                        @Name = {dto.PatientName}, 
                        @Email = {dto.PatientEmail}, 
                        @Phone = {dto.PatientPhoneNumber}, 
                        @Dob = {dto.PatientDateOfBirth}, 
                        @Gender = {dto.Gender}")
                .ToListAsync();

            var patientId = inserted.FirstOrDefault()?.PatientId ?? 0;

            return Ok(new { message = "Patient added successfully", patientId });
        }

        [HttpPut("patients/{id}")]
        public async Task<IActionResult> UpdatePatient(int id, [FromBody] PatientDto dto)
        {
            if (dto == null || dto.PatientId != id)
                return BadRequest("Invalid patient data or mismatched ID.");

            await _context.Database.ExecuteSqlInterpolatedAsync($@"
                EXEC usp_UpdatePatient 
                    @PatientId = {dto.PatientId}, 
                    @Name = {dto.PatientName}, 
                    @Email = {dto.PatientEmail}, 
                    @Phone = {dto.PatientPhoneNumber}, 
                    @Dob = {dto.PatientDateOfBirth}, 
                    @Gender = {dto.Gender}");

            return Ok("Patient updated successfully.");
        }

        [HttpDelete("patients/{id}")]
        public async Task<IActionResult> DeletePatient(int id)
        {
            await _context.Database.ExecuteSqlInterpolatedAsync(
                $"EXEC usp_DeletePatient @PatientId = {id}");

            return NoContent();
        }

        [HttpGet("medical-history/{phoneNumber}")]
        public async Task<IActionResult> GetMedicalHistoryByPhoneNumber(string phoneNumber)
        {
            var history = await _context.MedicalHistories
                .FromSqlInterpolated($"EXEC usp_GetMedicalHistoryByPhoneNumber @PhoneNumber = {phoneNumber}")
                .ToListAsync();

            return history.Any()
                ? Ok(history)
                : NotFound("Patient or medical history not found.");
        }
    }
}
