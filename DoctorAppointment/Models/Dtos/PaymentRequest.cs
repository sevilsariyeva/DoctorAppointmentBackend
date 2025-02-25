namespace DoctorAppointment.Models.Dtos
{
    public class PaymentRequest
    {
        public string? AppointmentId { get; set; }
        public decimal? Amount { get; set; }
    }
}
