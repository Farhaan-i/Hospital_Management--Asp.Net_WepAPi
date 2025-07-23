using System.ComponentModel.DataAnnotations;

namespace HMS.Models
{
    public class Slot
    {
        [Key]
        public int SlotId { get; set; }

        public DateTime SlotDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }

        public bool IsBooked { get; set; } = false; // Indicates whether the slot is booked

        // Foreign Key for Doctor
        public int DoctorId { get; set; }
        public Doctor? Doctor { get; set; }

        // Optional Foreign Key for Patient (if booked)
        public int? PatientId { get; set; }
        public Patient? Patient { get; set; }

        public int? AppointmentId { get; set; }

        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    }
}
