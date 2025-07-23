namespace HMS.Dto
{
    public class PatientDto
    {
        public int PatientId { get; set; }
        public string PatientName { get; set; }
        public string? PatientEmail { get; set; }
        public string PatientPhoneNumber { get; set; }
        public DateTime? PatientDateOfBirth { get; set; }
        public string? Gender { get; set; }  // 

    }
}
