using System.ComponentModel.DataAnnotations;

namespace HMS.Models
{
    public class Doctor
    {
        [Key]
        public int DoctorId { get; set; }
        public string DoctorName { get; set; }
        public string Specialization { get; set; }
        public string? DoctorEmail { get; set; }

        public string? DoctorContactNumber { get; set; }
        // Navigation Properties
        public List<Appointment>? Appointments { get; set; }
        public List<Slot>? Slots { get; set; }
    }
}
