using System.ComponentModel.DataAnnotations;

namespace HMS.Dto
{
    public class AppointmentUpdateDto
    {
            [Required]
            public int PatientId { get; set; }

            [Required]
            public int DoctorId { get; set; }

            [Required]
            public int SlotId { get; set; }

            [Required]
            public DateTime AppointmentDate { get; set; }

            public string? Status { get; set; } // e.g., Booked, Cancelled, Rescheduled
      
    }
}
