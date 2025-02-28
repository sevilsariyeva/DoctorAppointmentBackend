namespace DoctorAppointment.Models.Dtos
{
    public class JwtTokenResponse
    {
        public string Token { get; set; }
        public DateTime ExpiryTime { get; set; }
    }
}
