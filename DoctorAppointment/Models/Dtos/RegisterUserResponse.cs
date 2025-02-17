namespace DoctorAppointment.Models.Dtos
{
    public class RegisterUserResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string Token { get; set; }
    }
}
