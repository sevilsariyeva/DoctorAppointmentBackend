namespace DoctorAppointment.Models.Dtos
{
    public class LoginUserResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string Token { get; set; }
    }
}
