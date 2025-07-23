using System.ComponentModel.DataAnnotations;

namespace HMS.Models
{
    public class MedicalHistory
    {
        [Key]
        public int HistoryId { get; set; }
        public int PatientId { get; set; }
        public string? Diagnosis { get; set; }
        public string? Treatment { get; set; }
        public DateTime? DateRecorded { get; set; }
        // Navigation Property
        public Patient? Patient { get; set; }
    }
}
