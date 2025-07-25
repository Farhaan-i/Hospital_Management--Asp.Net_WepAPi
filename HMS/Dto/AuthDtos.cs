namespace HMS.Dto
{
    public class LoginDto
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
    public class RegisterDto
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Role { get; set; } // "Doctor" or "Staff"
        public int? DoctorId { get; set; }
        public int? StaffId { get; set; }
    }
    public class AuthResponseDto
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }

        public string Email { get; set; }
        public string Username { get; set; }
        public string Role { get; set; }
        public int? DoctorId { get; set; }
        public int? StaffId { get; set; }
    }
    public class RefreshTokenDto
    {
        public string RefreshToken { get; set; }
    }
}
