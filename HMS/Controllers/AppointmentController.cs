using HMS.Models;
using HMS.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HMS.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AppointmentController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AppointmentController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("appointments")]
        public ActionResult GetAllAppointments()
        {
            return Ok(_context.Appointments.ToList());
        }

        [HttpGet("appointments/{id}")]
        public ActionResult GetAppointmentById(int id)
        {
            var appointment = _context.Appointments.Find(id);
            if (appointment == null)
                return NotFound($"No appointment found with ID {id}");

            return Ok(appointment);
        }

        [HttpPost("appointments")]
        public IActionResult AddAppointment([FromBody] AppointmentDto dto)
        {
            if (dto == null)
                return BadRequest("Invalid appointment data.");

            using var transaction = _context.Database.BeginTransaction();
            try
            {
                var appointment = new Appointment
                {
                    PatientId = dto.PatientId,
                    DoctorId = dto.DoctorId,
                    SlotId = dto.SlotId,
                    AppointmentDate = dto.AppointmentDate,
                    Status = dto.Status ?? "Booked"
                };

                _context.Appointments.Add(appointment);
                _context.SaveChanges();

                var slot = _context.Slots.FirstOrDefault(s => s.SlotId == dto.SlotId);
                if (slot != null)
                {
                    slot.IsBooked = true;
                    slot.PatientId = dto.PatientId;
                    slot.AppointmentId = appointment.AppointmentId;
                    _context.Slots.Update(slot);
                    _context.SaveChanges();
                }

                transaction.Commit();
                return Ok(new { message = "Appointment booked successfully!", appointmentId = appointment.AppointmentId });
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                return StatusCode(500, $"Error booking appointment: {ex.Message}");
            }
        }

        [HttpPut("appointments/{id}")]
        public IActionResult UpdateAppointment(int id, [FromBody] Appointment appointment)
        {
            if (id != appointment.AppointmentId)
                return BadRequest("Appointment ID mismatch");

            _context.Appointments.Update(appointment);
            _context.SaveChanges();
            return NoContent();
        }

        [HttpDelete("appointments/{id}")]
        public IActionResult DeleteAppointment(int id)
        {
            var appointment = _context.Appointments.Find(id);
            if (appointment == null)
                return NotFound($"No appointment found with ID {id}");

            _context.Appointments.Remove(appointment);
            _context.SaveChanges();
            return NoContent();
        }

        [HttpPut("appointments/{id}/cancel")]
        public IActionResult CancelAppointment(int id)
        {
            var appointment = _context.Appointments.Find(id);
            if (appointment == null)
                return NotFound($"No appointment found with ID {id}");

            var slot = _context.Slots.FirstOrDefault(s => s.SlotId == appointment.SlotId);
            if (slot == null)
                return BadRequest("Associated slot not found.");

            var slotStart = appointment.AppointmentDate.Date.Add(slot.StartTime);
            if (slotStart <= DateTime.Now)
                return BadRequest("Cannot cancel past appointments.");

            appointment.Status = "Cancelled";
            _context.Appointments.Update(appointment);

            slot.IsBooked = false;
            slot.PatientId = null;
            slot.AppointmentId = null;
            _context.Slots.Update(slot);

            _context.SaveChanges();
            return Ok("Appointment cancelled and slot is now available.");
        }

        [HttpGet("appointments/doctor/{doctorId}/date/{date}")]
        public ActionResult GetAppointmentsByDoctorAndDate(int doctorId, DateTime date)
        {
            var appointments = _context.Appointments
                .Where(a => a.DoctorId == doctorId && a.AppointmentDate.Date == date.Date)
                .ToList();

            if (!appointments.Any())
                return NotFound($"No appointments found for doctor ID {doctorId} on {date:yyyy-MM-dd}");

            return Ok(appointments);
        }

        [HttpGet("doctor-by-name/{name}")]
        public ActionResult GetDoctorsByName(string name)
        {
            var doctors = _context.Doctors
                .Where(d => d.DoctorName.ToLower().Contains(name.ToLower()))
                .Select(d => new
                {
                    d.DoctorId,
                    d.DoctorName,
                    d.Specialization,
                    d.DoctorEmail,
                    d.DoctorContactNumber
                })
                .ToList();

            if (!doctors.Any())
                return NotFound($"No doctors found with name containing '{name}'.");

            return Ok(doctors);
        }


        [HttpGet("specialization/{specialization}")]
        public ActionResult GetDoctorsBySpecialization(string specialization)
        {
            var doctors = _context.Doctors
                .Where(d => d.Specialization.ToLower() == specialization.ToLower())
                .ToList();

            if (!doctors.Any())
                return NotFound($"No doctors found with specialization '{specialization}'.");

            return Ok(doctors);
        }

        [HttpGet("todays-appointments/{doctorId}")]
        public IActionResult GetTodaysAppointments(int doctorId)
        {
            var today = DateTime.Today;
            var appointments = _context.Appointments
                .Where(a => a.DoctorId == doctorId && a.AppointmentDate.Date == today)
                .ToList();

            if (!appointments.Any())
                return NotFound("No appointments found for this doctor today.");

            return Ok(appointments);
        }

        [HttpGet("today")]
        public ActionResult GetTodaysAppointments()
        {
            var today = DateTime.Today;
            var appointments = _context.Appointments
                .Where(a => a.AppointmentDate.Date == today)
                .Select(a => new AppointmentDto
                {
                    AppointmentId = a.AppointmentId,
                    PatientId = a.PatientId,
                    DoctorId = a.DoctorId,
                    SlotId = a.SlotId,
                    AppointmentDate = a.AppointmentDate,
                    Status = a.Status
                })
                .ToList();

            if (!appointments.Any())
                return NotFound("No appointments found for today.");

            return Ok(appointments);
        }

        [HttpDelete("cancel/{appointmentId}")]
        public IActionResult CancelAppointmentById(int appointmentId)
        {
            using var transaction = _context.Database.BeginTransaction();
            try
            {
                var appointment = _context.Appointments.Find(appointmentId);
                if (appointment == null)
                    return NotFound("Appointment not found.");

                var slot = _context.Slots.FirstOrDefault(s => s.SlotId == appointment.SlotId);
                if (slot != null)
                {
                    slot.IsBooked = false;
                    slot.PatientId = null;
                    slot.AppointmentId = null;
                    _context.Slots.Update(slot);
                }

                _context.Appointments.Remove(appointment);
                _context.SaveChanges();
                transaction.Commit();

                return Ok("Appointment cancelled successfully, slot is now available.");
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                return StatusCode(500, $"Error cancelling appointment: {ex.Message}");
            }
        }

        [HttpGet("past/{patientId}")]
        public ActionResult GetPastAppointments(int patientId)
        {
            var appointments = _context.Appointments
                .Where(a => a.PatientId == patientId && a.AppointmentDate < DateTime.Now)
                .Include(a => a.Doctor)
                .Include(a => a.Slot)
                .ToList();

            if (!appointments.Any())
                return NotFound("No past appointments found.");

            return Ok(appointments);
        }

        [HttpGet("upcoming/{patientId}")]
        public ActionResult GetUpcomingAppointments(int patientId)
        {
            var appointments = _context.Appointments
                .Where(a => a.PatientId == patientId && a.AppointmentDate > DateTime.Now)
                .Select(a => new AppointmentDto
                {
                    AppointmentId = a.AppointmentId,
                    PatientId = a.PatientId,
                    DoctorId = a.DoctorId,
                    SlotId = a.SlotId,
                    AppointmentDate = a.AppointmentDate,
                    Status = a.Status
                })
                .ToList();

            if (!appointments.Any())
                return NotFound("No upcoming appointments found.");

            return Ok(appointments);
        }
    }
}
