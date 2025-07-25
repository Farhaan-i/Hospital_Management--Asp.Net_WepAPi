namespace HMS.Dto
{
    public class UpdateAppointmentDto
    {
        public int AppointmentId { get; set; }
        public int PatientId { get; set; }
        public int DoctorId { get; set; }
        public int SlotId { get; set; }
        public DateTime AppointmentDate { get; set; }
        public string Status { get; set; }
    }
}
