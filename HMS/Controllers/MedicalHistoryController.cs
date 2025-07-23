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
        public ActionResult GetAllMedicalHistories()
        {
            var histories = _context.MedicalHistories.ToList();
            return Ok(histories);
        }

        [HttpGet("by-patient/{patientId}")]
        public IActionResult GetMedicalHistoryByPatientId(int patientId)
        {
            var histories = _context.MedicalHistories
                .Where(mh => mh.PatientId == patientId)
                .ToList();

            if (!histories.Any())
            {
                return NotFound($"No medical history found for patient ID {patientId}");
            }

            return Ok(histories);
        }


        [HttpPost("medical-histories")]
        public ActionResult AddMedicalHistory([FromBody] MedicalHistoryDto dto)
        {
            if (dto == null)
            {
                return BadRequest("Invalid medical history data.");
            }

            var history = new MedicalHistory
            {
                PatientId = dto.PatientId,
                Diagnosis = dto.Diagnosis,
                Treatment = dto.Treatment,
                DateRecorded = dto.DateRecorded ?? DateTime.Now
            };

            _context.MedicalHistories.Add(history);
            _context.SaveChanges();

            return CreatedAtAction(nameof(GetMedicalHistoryByPatientId), new { id = history.HistoryId }, history);
        }

        [HttpPut("medical-histories/{id}")]
        public ActionResult UpdateMedicalHistory(int id, [FromBody] MedicalHistoryDto dto)
        {
            if (dto == null || id != dto.HistoryId)
            {
                return BadRequest("Medical history ID mismatch or invalid data.");
            }

            var history = _context.MedicalHistories.Find(id);
            if (history == null)
            {
                return NotFound($"No medical history found with ID {id}");
            }

            history.Diagnosis = dto.Diagnosis;
            history.Treatment = dto.Treatment;
            history.DateRecorded = dto.DateRecorded;

            _context.MedicalHistories.Update(history);
            _context.SaveChanges();

            return NoContent();
        }

        [HttpDelete("medical-histories/{id}")]
        public ActionResult DeleteMedicalHistory(int id)
        {
            var history = _context.MedicalHistories.Find(id);
            if (history == null)
            {
                return NotFound($"No medical history found with ID {id}");
            }

            _context.MedicalHistories.Remove(history);
            _context.SaveChanges();

            return NoContent();
        }
    }
}