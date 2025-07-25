using System.ComponentModel.DataAnnotations;

namespace HMS.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string Role { get; set; } // "Doctor" or "Staff"
        public int? DoctorId { get; set; } // Foreign key to Doctor table
        public int? StaffId { get; set; } // Foreign key to Staff table
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
