namespace HMS.Dto
{
    public class MedicalHistoryDto
    {
        public int HistoryId { get; set; }
        public int PatientId { get; set; }
        public string? Diagnosis { get; set; }
        public string? Treatment { get; set; }
        public DateTime? DateRecorded { get; set; }
    }
}
