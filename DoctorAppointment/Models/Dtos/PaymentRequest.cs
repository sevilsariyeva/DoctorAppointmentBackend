namespace DoctorAppointment.Models.Dtos
{
    public class PaymentRequest
    {
        public string? AppointmentId { get; set; }
        public decimal? Payment { get; set; }
    }
}
