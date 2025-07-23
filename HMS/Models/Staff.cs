using System.ComponentModel.DataAnnotations;

namespace HMS.Models
{
    public class Staff
    {
        [Key]
        public int StaffId { get; set; }
        public string StaffName { get; set; }
        public string StaffRole { get; set; } // E.g., Receptionist, Nurse, Admin
        public string? StaffEmail { get; set; }
        public string StaffPhoneNumber { get; set; }

    }
}
