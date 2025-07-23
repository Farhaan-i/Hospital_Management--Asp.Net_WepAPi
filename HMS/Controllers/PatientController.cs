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
        public ActionResult<List<Patient>> GetAllPatients()
        {
            var patients = _context.Patients.ToList();
            return Ok(patients);
        }

        [HttpGet("patients/{id}")]
        public ActionResult<Patient> GetPatientById(int id)
        {
            var patient = _context.Patients.Find(id);
            if (patient == null)
            {
                return NotFound($"No patient found with ID {id}");
            }
            return Ok(patient);
        }

        [HttpPost("patients")]
        public IActionResult AddPatient([FromBody] PatientDto dto)
        {
            if (dto == null)
            {
                return BadRequest("Invalid patient data.");
            }

            var patient = new Patient
            {
                PatientName = dto.PatientName,
                PatientEmail = dto.PatientEmail,
                PatientPhoneNumber = dto.PatientPhoneNumber,
                PatientDateOfBirth = dto.PatientDateOfBirth,
                Gender = dto.Gender
            };

            _context.Patients.Add(patient);
            _context.SaveChanges();

            return Ok(new { message = "Patient added successfully", patientId = patient.PatientId });
        }

        [HttpPut("patients/{id}")]
        public IActionResult UpdatePatient(int id, [FromBody] PatientDto dto)
        {
            if (dto == null || dto.PatientId != id)
            {
                return BadRequest("Invalid patient data or mismatched ID.");
            }

            var patient = _context.Patients.FirstOrDefault(p => p.PatientId == id);
            if (patient == null)
            {
                return NotFound("Patient not found.");
            }

            patient.PatientName = dto.PatientName;
            patient.PatientEmail = dto.PatientEmail;
            patient.PatientPhoneNumber = dto.PatientPhoneNumber;
            patient.PatientDateOfBirth = dto.PatientDateOfBirth;

            _context.SaveChanges();
            return Ok("Patient updated successfully.");
        }

        [HttpDelete("patients/{id}")]
        public IActionResult DeletePatient(int id)
        {
            var patient = _context.Patients.Find(id);
            if (patient == null)
            {
                return NotFound($"No patient found with ID {id}");
            }

            _context.Patients.Remove(patient);
            _context.SaveChanges();
            return NoContent();
        }

        [HttpGet("medical-history/{phoneNumber}")]
        public IActionResult GetMedicalHistoryByPhoneNumber(string phoneNumber)
        {
            try
            {
                var patient = _context.Patients.FirstOrDefault(p => p.PatientPhoneNumber == phoneNumber);
                if (patient == null)
                {
                    return NotFound("Patient not found.");
                }

                var history = _context.MedicalHistories
                    .Where(mh => mh.PatientId == patient.PatientId)
                    .Select(mh => new MedicalHistoryDto
                    {
                        HistoryId = mh.HistoryId,
                        PatientId = mh.PatientId,
                        Diagnosis = mh.Diagnosis,
                        Treatment = mh.Treatment,
                        DateRecorded = mh.DateRecorded
                    })
                    .ToList();

                return Ok(history);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = $"Error retrieving medical history: {ex.Message}" });
            }
        }
    }
}