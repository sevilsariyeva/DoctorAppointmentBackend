namespace DoctorAppointment.Models.Dtos
{
    public class CancelAppointmentRequest
    {
        public string? AppointmentId { get; set; }
        public string? UserId { get; set; }
    }
}
