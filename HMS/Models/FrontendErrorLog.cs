using System.ComponentModel.DataAnnotations;

namespace HMS.Models
{
    public class FrontendErrorLog
    {
        [Key]
        public int ErrorLogId { get; set; }
        public int? UserId { get; set; } // nullable

        public string Username { get; set; }
        public string ErrorMessage { get; set; }
        public string Url { get; set; }
        public string Component { get; set; }
        public DateTime TimeOccurred { get; set; }
        public string AdditionalInfo { get; set; }


    }
}
