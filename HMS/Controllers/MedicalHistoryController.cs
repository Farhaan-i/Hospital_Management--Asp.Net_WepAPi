using HMS.Models;
using HMS.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HMS.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MedicalHistoryController : ControllerBase
    {
        private readonly AppDbContext _context;

        public MedicalHistoryController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("medical-histories")]
        public async Task<IActionResult> GetAllMedicalHistories()
        {
            var histories = await _context.MedicalHistories
                .FromSqlInterpolated($"EXEC usp_GetAllMedicalHistories")
                .ToListAsync();

            return Ok(histories);
        }

        [HttpGet("by-patient/{patientId}")]
        public async Task<IActionResult> GetMedicalHistoryByPatientId(int patientId)
        {
            var histories = await _context.MedicalHistories
                .FromSqlInterpolated($"EXEC usp_GetMedicalHistoryByPatientId @PatientId = {patientId}")
                .ToListAsync();

            return histories.Any()
                ? Ok(histories)
                : NotFound($"No medical history found for patient ID {patientId}");
        }

        [HttpPost("medical-histories")]
        public async Task<IActionResult> AddMedicalHistory([FromBody] MedicalHistoryDto dto)
        {
            var added = await _context.MedicalHistories
                .FromSqlInterpolated($@"
                    EXEC usp_AddMedicalHistory 
                        @PatientId = {dto.PatientId}, 
                        @Diagnosis = {dto.Diagnosis}, 
                        @Treatment = {dto.Treatment}, 
                        @DateRecorded = {dto.DateRecorded ?? DateTime.Now}")
                .ToListAsync();

            var history = added.FirstOrDefault();
            return history == null
                ? StatusCode(500, "Failed to add medical history.")
                : CreatedAtAction(nameof(GetMedicalHistoryByPatientId), new { patientId = history.PatientId }, history);
        }

        [HttpPut("medical-histories/{id}")]
        public async Task<IActionResult> UpdateMedicalHistory(int id, [FromBody] MedicalHistoryDto dto)
        {
            if (dto == null || id != dto.HistoryId)
                return BadRequest("Medical history ID mismatch or invalid data.");

            await _context.Database.ExecuteSqlInterpolatedAsync($@"
                EXEC usp_UpdateMedicalHistory 
                    @HistoryId = {dto.HistoryId}, 
                    @Diagnosis = {dto.Diagnosis}, 
                    @Treatment = {dto.Treatment}, 
                    @DateRecorded = {dto.DateRecorded}");

            return NoContent();
        }

        [HttpDelete("medical-histories/{id}")]
        public async Task<IActionResult> DeleteMedicalHistory(int id)
        {
            await _context.Database.ExecuteSqlInterpolatedAsync(
                $"EXEC usp_DeleteMedicalHistory @HistoryId = {id}");

            return NoContent();
        }
    }
}
