using System.ComponentModel.DataAnnotations;

namespace HMS.Models
{
    public class UserLoginLog
    {
        [Key]
        public int LogId { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        public string Username { get; set; }
        public DateTime LoginTime { get; set; }
        public string IPAddress { get; set; }
        public string UserAgent { get; set; }
    }
}
