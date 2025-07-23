using System.ComponentModel.DataAnnotations;
using System.Numerics;

namespace HMS.Models
{
    public class Appointment
    {
        [Key]
        public int AppointmentId { get; set; }
        public int PatientId { get; set; }
        public int DoctorId { get; set; }
        public int SlotId { get; set; }
        public DateTime AppointmentDate { get; set; }

        public string? Status { get; set; } = "Booked"; // Scheduled, Completed, Canceled
                                                        // Navigation Properties
        
        public Patient? Patient { get; set; }
        public Doctor? Doctor { get; set; }
        public Slot? Slot { get; set; }
    }
}
